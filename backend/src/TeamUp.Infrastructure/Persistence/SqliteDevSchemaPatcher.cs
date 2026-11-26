// Date: 2026-11-26
// Purpose: Idempotent SQLite local-dev schema patcher keeping TeamUp.dev.db aligned with EF Core migrations (including Student nullable FacultyId, StoredImage nullable uploader, and ScoreTransaction AttendanceRevocation check constraint).

using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TeamUp.Infrastructure.Persistence;

public static class SqliteDevSchemaPatcher
{
    #region Schema Patch Definitions & Entry Point

    private static readonly (string Table, string Column, string AlterSql)[] ColumnPatches =
    [
        ("University", "LogoUrl", "ALTER TABLE \"University\" ADD COLUMN \"LogoUrl\" TEXT NULL;"),
        ("Student", "FailedLoginCount", "ALTER TABLE \"Student\" ADD COLUMN \"FailedLoginCount\" INTEGER NOT NULL DEFAULT 0;"),
        ("Student", "LockoutEndUtc", "ALTER TABLE \"Student\" ADD COLUMN \"LockoutEndUtc\" TEXT NULL;"),
        ("Student", "PreviousRefreshToken", "ALTER TABLE \"Student\" ADD COLUMN \"PreviousRefreshToken\" TEXT NULL;"),
        ("Student", "PreviousRefreshTokenRotatedAt", "ALTER TABLE \"Student\" ADD COLUMN \"PreviousRefreshTokenRotatedAt\" TEXT NULL;"),
        ("ScoreTransaction", "SourceReferenceId", "ALTER TABLE \"ScoreTransaction\" ADD COLUMN \"SourceReferenceId\" INTEGER NULL;"),
        ("Student", "EmailNotificationsEnabled", "ALTER TABLE \"Student\" ADD COLUMN \"EmailNotificationsEnabled\" INTEGER NOT NULL DEFAULT 1;"),
        ("Student", "ReceiveLeaderboardDigests", "ALTER TABLE \"Student\" ADD COLUMN \"ReceiveLeaderboardDigests\" INTEGER NOT NULL DEFAULT 1;"),
        ("Student", "ReceiveProjectMatchEmails", "ALTER TABLE \"Student\" ADD COLUMN \"ReceiveProjectMatchEmails\" INTEGER NOT NULL DEFAULT 1;"),
        ("Student", "AffiliationType", "ALTER TABLE \"Student\" ADD COLUMN \"AffiliationType\" TEXT NOT NULL DEFAULT 'UniversityStudent';"),
        ("Student", "City", "ALTER TABLE \"Student\" ADD COLUMN \"City\" TEXT NULL;"),
        ("Organization", "Scope", "ALTER TABLE \"Organization\" ADD COLUMN \"Scope\" TEXT NOT NULL DEFAULT 'UniversityBranch';"),
        ("Organization", "UniversityId", "ALTER TABLE \"Organization\" ADD COLUMN \"UniversityId\" INTEGER NULL;"),
        ("Organization", "City", "ALTER TABLE \"Organization\" ADD COLUMN \"City\" TEXT NULL;"),
        ("Organization", "CoverImageUrl", "ALTER TABLE \"Organization\" ADD COLUMN \"CoverImageUrl\" TEXT NULL;"),
        ("Organization", "WebsiteUrl", "ALTER TABLE \"Organization\" ADD COLUMN \"WebsiteUrl\" TEXT NULL;"),
        ("Organization", "IsRecruiting", "ALTER TABLE \"Organization\" ADD COLUMN \"IsRecruiting\" INTEGER NOT NULL DEFAULT 0;"),
        ("Organization", "CurrentSeasonName", "ALTER TABLE \"Organization\" ADD COLUMN \"CurrentSeasonName\" TEXT NULL;"),
        ("OrganizationMember", "OrganizationUnitId", "ALTER TABLE \"OrganizationMember\" ADD COLUMN \"OrganizationUnitId\" INTEGER NULL;"),
        ("OrganizationMember", "HierarchyTier", "ALTER TABLE \"OrganizationMember\" ADD COLUMN \"HierarchyTier\" TEXT NOT NULL DEFAULT 'OrganizingCrew';"),
        ("MembershipApplication", "OrganizationUnitId", "ALTER TABLE \"MembershipApplication\" ADD COLUMN \"OrganizationUnitId\" INTEGER NULL;"),
        ("MembershipApplication", "SeasonName", "ALTER TABLE \"MembershipApplication\" ADD COLUMN \"SeasonName\" TEXT NULL;"),
        ("MembershipApplication", "Stage", "ALTER TABLE \"MembershipApplication\" ADD COLUMN \"Stage\" TEXT NOT NULL DEFAULT 'Applied';"),
        ("MembershipApplication", "ReviewerStudentId", "ALTER TABLE \"MembershipApplication\" ADD COLUMN \"ReviewerStudentId\" INTEGER NULL;"),
        ("MembershipApplication", "EvaluationScore", "ALTER TABLE \"MembershipApplication\" ADD COLUMN \"EvaluationScore\" REAL NULL;"),
        ("MembershipApplication", "InterviewNotes", "ALTER TABLE \"MembershipApplication\" ADD COLUMN \"InterviewNotes\" TEXT NULL;"),
        ("Event", "CoverImageUrl", "ALTER TABLE \"Event\" ADD COLUMN \"CoverImageUrl\" TEXT NULL;"),
        ("Project", "CoverImageUrl", "ALTER TABLE \"Project\" ADD COLUMN \"CoverImageUrl\" TEXT NULL;"),
        ("Event", "DeliveryMode", "ALTER TABLE \"Event\" ADD COLUMN \"DeliveryMode\" TEXT NOT NULL DEFAULT 'InPerson';"),
        ("Event", "IsPublic", "ALTER TABLE \"Event\" ADD COLUMN \"IsPublic\" INTEGER NOT NULL DEFAULT 1;"),
        ("Event", "MeetingUrl", "ALTER TABLE \"Event\" ADD COLUMN \"MeetingUrl\" TEXT NULL;"),
        ("Event", "OrganizationUnitId", "ALTER TABLE \"Event\" ADD COLUMN \"OrganizationUnitId\" INTEGER NULL;"),
    ];

    private static readonly string[] DdlCreateIfMissingStatements =
    [
        "CREATE UNIQUE INDEX IF NOT EXISTS IX_ScoreTransaction_StudentId_SourceType_SourceReferenceId ON ScoreTransaction (StudentId, SourceType, SourceReferenceId) WHERE SourceReferenceId IS NOT NULL;",
        "CREATE TABLE IF NOT EXISTS OrganizationUnit (OrganizationUnitId INTEGER PRIMARY KEY AUTOINCREMENT, OrganizationId INTEGER NOT NULL, Name TEXT NOT NULL, Code TEXT NOT NULL, Description TEXT NULL, UnitType TEXT NOT NULL DEFAULT 'TechnicalChapter', IsRecruiting INTEGER NOT NULL DEFAULT 0, OpenSeats INTEGER NULL, DeletedAt TEXT NULL, FOREIGN KEY (OrganizationId) REFERENCES Organization (OrganizationId));",
        "CREATE UNIQUE INDEX IF NOT EXISTS IX_OrganizationUnit_OrganizationId_Code ON OrganizationUnit (OrganizationId, Code) WHERE DeletedAt IS NULL;",
        "CREATE TABLE IF NOT EXISTS StoredImage (StoredImageId TEXT NOT NULL CONSTRAINT PK_StoredImage PRIMARY KEY, UploadedByStudentId INTEGER NULL, Content BLOB NOT NULL, ContentType TEXT NOT NULL, SizeBytes INTEGER NOT NULL, UploadedAt TEXT NOT NULL, FOREIGN KEY (UploadedByStudentId) REFERENCES Student (StudentId) ON DELETE RESTRICT);",
        "CREATE INDEX IF NOT EXISTS IX_StoredImage_UploadedByStudentId ON StoredImage (UploadedByStudentId);",
        "CREATE TABLE IF NOT EXISTS OrganizationCreationRequest (RequestId INTEGER PRIMARY KEY AUTOINCREMENT, RequesterStudentId INTEGER NOT NULL, Name TEXT NOT NULL, Description TEXT NOT NULL, Category TEXT NOT NULL, Scope TEXT NOT NULL DEFAULT 'UniversityBranch', UniversityId INTEGER NULL, City TEXT NULL, LogoUrl TEXT NULL, CoverImageUrl TEXT NULL, WebsiteUrl TEXT NULL, CurrentSeasonName TEXT NULL, Justification TEXT NULL, Status TEXT NOT NULL DEFAULT 'Pending', RequestedAt TEXT NOT NULL DEFAULT (datetime('now')), ReviewedAt TEXT NULL, ReviewedByStudentId INTEGER NULL, ReviewNotes TEXT NULL, CreatedOrganizationId INTEGER NULL, DeletedAt TEXT NULL, FOREIGN KEY (RequesterStudentId) REFERENCES Student (StudentId), FOREIGN KEY (UniversityId) REFERENCES University (UniversityId), FOREIGN KEY (ReviewedByStudentId) REFERENCES Student (StudentId), FOREIGN KEY (CreatedOrganizationId) REFERENCES Organization (OrganizationId));",
        "CREATE UNIQUE INDEX IF NOT EXISTS IX_OrganizationCreationRequest_RequesterStudentId_Name ON OrganizationCreationRequest (RequesterStudentId, Name) WHERE DeletedAt IS NULL AND Status = 'Pending';",
        "CREATE INDEX IF NOT EXISTS IX_OrganizationCreationRequest_Status_RequestedAt ON OrganizationCreationRequest (Status, RequestedAt) WHERE DeletedAt IS NULL;",
        "CREATE INDEX IF NOT EXISTS IX_OrganizationCreationRequest_RequesterStudentId ON OrganizationCreationRequest (RequesterStudentId);"
    ];

    public static async Task PatchAsync(AppDbContext db, CancellationToken ct = default)
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        foreach (var (table, column, alterSql) in ColumnPatches)
        {
            if (await TableExistsAsync(conn, table, ct) && !await ColumnExistsAsync(conn, table, column, ct))
            {
                try
                {
                    await db.Database.ExecuteSqlRawAsync(alterSql, ct);
                }
                catch (SqliteException ex) when (ex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
                {
                    // Safe race guard if column already exists.
                }
            }
        }

        foreach (var sql in DdlCreateIfMissingStatements)
        {
            await db.Database.ExecuteSqlRawAsync(sql, ct);
        }

        await RebuildStudentIfFacultyNotNullAsync(conn, ct);
        await RebuildStoredImageIfUploaderNotNullAsync(conn, ct);
        await RebuildScoreTransactionIfMissingAttendanceRevocationAsync(conn, ct);
    }

    #endregion

    #region Schema Probes

    private static readonly HashSet<string> AllowedPatchTables =
        ColumnPatches.Select(p => p.Table).ToHashSet(StringComparer.Ordinal);

    private static async Task<bool> TableExistsAsync(DbConnection conn, string table, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
        var p = cmd.CreateParameter();
        p.ParameterName = "$name";
        p.Value = table;
        cmd.Parameters.Add(p);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct) ?? 0) > 0;
    }

    private static async Task<bool> ColumnExistsAsync(DbConnection conn, string table, string column, CancellationToken ct)
    {
        if (!AllowedPatchTables.Contains(table))
            throw new ArgumentException($"Unsupported SQLite schema patch table '{table}'.", nameof(table));

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = $col;";
        var p = cmd.CreateParameter();
        p.ParameterName = "$col";
        p.Value = column;
        cmd.Parameters.Add(p);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct) ?? 0) > 0;
    }

    #endregion

    #region Table Rebuild Migrations

    private static async Task RebuildStudentIfFacultyNotNullAsync(DbConnection conn, CancellationToken ct)
    {
        await using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT \"notnull\" FROM pragma_table_info('Student') WHERE name = 'FacultyId';";
        var facultyNotNull = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct) ?? 0) == 1;
        if (!facultyNotNull)
            return;

        await ExecuteTableRebuildAsync(conn, """
            ALTER TABLE "Student" RENAME TO "_Student_legacy";
            CREATE TABLE "Student" (
                "StudentId" INTEGER NOT NULL CONSTRAINT "PK_Student" PRIMARY KEY AUTOINCREMENT,
                "Email" TEXT NOT NULL,
                "PasswordHash" TEXT NOT NULL,
                "FirstName" TEXT NOT NULL,
                "LastName" TEXT NOT NULL,
                "AcademicYear" INTEGER NOT NULL,
                "Role" TEXT NOT NULL DEFAULT 'Student',
                "SecurityStamp" TEXT NOT NULL,
                "ShowEmailInDirectory" INTEGER NOT NULL DEFAULT 0,
                "FacultyId" INTEGER NULL,
                "RefreshToken" TEXT NULL,
                "RefreshTokenExpiresAt" TEXT NULL,
                "PasswordResetToken" TEXT NULL,
                "PasswordResetTokenExpiresAt" TEXT NULL,
                "DeletedAt" TEXT NULL,
                "FailedLoginCount" INTEGER NOT NULL DEFAULT 0,
                "LockoutEndUtc" TEXT NULL,
                "PreviousRefreshToken" TEXT NULL,
                "PreviousRefreshTokenRotatedAt" TEXT NULL,
                "EmailNotificationsEnabled" INTEGER NOT NULL DEFAULT 1,
                "ReceiveLeaderboardDigests" INTEGER NOT NULL DEFAULT 1,
                "ReceiveProjectMatchEmails" INTEGER NOT NULL DEFAULT 1,
                "AffiliationType" TEXT NOT NULL DEFAULT 'UniversityStudent',
                "City" TEXT NULL,
                CONSTRAINT "CK_Student_AcademicYear" CHECK ([AcademicYear] BETWEEN 1 AND 7),
                CONSTRAINT "CK_Student_Role" CHECK ([Role] IN ('Student', 'OrgAdmin', 'PlatformAdmin')),
                CONSTRAINT "FK_Student_Faculty_FacultyId" FOREIGN KEY ("FacultyId") REFERENCES "Faculty" ("FacultyId") ON DELETE RESTRICT
            );
            INSERT INTO "Student" (
                "StudentId", "Email", "PasswordHash", "FirstName", "LastName", "AcademicYear", "Role",
                "SecurityStamp", "ShowEmailInDirectory", "FacultyId", "RefreshToken", "RefreshTokenExpiresAt",
                "PasswordResetToken", "PasswordResetTokenExpiresAt", "DeletedAt", "FailedLoginCount",
                "LockoutEndUtc", "PreviousRefreshToken", "PreviousRefreshTokenRotatedAt",
                "EmailNotificationsEnabled", "ReceiveLeaderboardDigests", "ReceiveProjectMatchEmails",
                "AffiliationType", "City"
            )
            SELECT
                "StudentId", "Email", "PasswordHash", "FirstName", "LastName", "AcademicYear", "Role",
                "SecurityStamp", "ShowEmailInDirectory", "FacultyId", "RefreshToken", "RefreshTokenExpiresAt",
                "PasswordResetToken", "PasswordResetTokenExpiresAt", "DeletedAt", "FailedLoginCount",
                "LockoutEndUtc", "PreviousRefreshToken", "PreviousRefreshTokenRotatedAt",
                "EmailNotificationsEnabled", "ReceiveLeaderboardDigests", "ReceiveProjectMatchEmails",
                "AffiliationType", "City"
            FROM "_Student_legacy";
            DROP TABLE "_Student_legacy";
            CREATE UNIQUE INDEX "IX_Student_Email" ON "Student" ("Email") WHERE [DeletedAt] IS NULL;
            CREATE INDEX "IX_Student_FacultyId" ON "Student" ("FacultyId");
            """, ct);
    }

    private static async Task RebuildStoredImageIfUploaderNotNullAsync(DbConnection conn, CancellationToken ct)
    {
        await using var imageCheckCmd = conn.CreateCommand();
        imageCheckCmd.CommandText = "SELECT \"notnull\" FROM pragma_table_info('StoredImage') WHERE name = 'UploadedByStudentId';";
        var uploaderNotNull = Convert.ToInt32(await imageCheckCmd.ExecuteScalarAsync(ct) ?? 0) == 1;
        if (!uploaderNotNull)
            return;

        await ExecuteTableRebuildAsync(conn, """
            ALTER TABLE "StoredImage" RENAME TO "_StoredImage_legacy";
            CREATE TABLE "StoredImage" (
                "StoredImageId" TEXT NOT NULL CONSTRAINT "PK_StoredImage" PRIMARY KEY,
                "UploadedByStudentId" INTEGER NULL,
                "Content" BLOB NOT NULL,
                "ContentType" TEXT NOT NULL,
                "SizeBytes" INTEGER NOT NULL,
                "UploadedAt" TEXT NOT NULL,
                CONSTRAINT "FK_StoredImage_Student_UploadedByStudentId" FOREIGN KEY ("UploadedByStudentId") REFERENCES "Student" ("StudentId") ON DELETE RESTRICT
            );
            INSERT INTO "StoredImage" ("StoredImageId", "UploadedByStudentId", "Content", "ContentType", "SizeBytes", "UploadedAt")
            SELECT "StoredImageId", "UploadedByStudentId", "Content", "ContentType", "SizeBytes", "UploadedAt"
            FROM "_StoredImage_legacy";
            DROP TABLE "_StoredImage_legacy";
            CREATE INDEX "IX_StoredImage_UploadedByStudentId" ON "StoredImage" ("UploadedByStudentId");
            """, ct);
    }

    private static async Task RebuildScoreTransactionIfMissingAttendanceRevocationAsync(DbConnection conn, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'ScoreTransaction';";
        var createSql = (await cmd.ExecuteScalarAsync(ct)) as string;
        if (string.IsNullOrWhiteSpace(createSql) ||
            createSql.Contains("AttendanceRevocation", StringComparison.Ordinal))
        {
            return;
        }

        await ExecuteTableRebuildAsync(conn, """
            ALTER TABLE "ScoreTransaction" RENAME TO "_ScoreTransaction_legacy";
            CREATE TABLE "ScoreTransaction" (
                "TransactionId" INTEGER NOT NULL CONSTRAINT "PK_ScoreTransaction" PRIMARY KEY AUTOINCREMENT,
                "StudentId" INTEGER NOT NULL,
                "Points" INTEGER NOT NULL,
                "Reason" TEXT NOT NULL,
                "SourceType" TEXT NOT NULL,
                "SourceReferenceId" INTEGER NULL,
                "CreatedAt" TEXT NOT NULL,
                CONSTRAINT "CK_ScoreTransaction_SourceType" CHECK ([SourceType] IN ('EventAttendance', 'TaskCompleted', 'ProjectDelivered', 'Manual', 'AttendanceRevocation')),
                CONSTRAINT "FK_ScoreTransaction_Student_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Student" ("StudentId") ON DELETE RESTRICT
            );
            INSERT INTO "ScoreTransaction" ("TransactionId", "StudentId", "Points", "Reason", "SourceType", "SourceReferenceId", "CreatedAt")
            SELECT "TransactionId", "StudentId", "Points", "Reason", "SourceType", "SourceReferenceId", "CreatedAt"
            FROM "_ScoreTransaction_legacy";
            DROP TABLE "_ScoreTransaction_legacy";
            CREATE INDEX "IX_ScoreTransaction_StudentId_CreatedAt" ON "ScoreTransaction" ("StudentId", "CreatedAt");
            CREATE UNIQUE INDEX "IX_ScoreTransaction_StudentId_SourceType_SourceReferenceId" ON "ScoreTransaction" ("StudentId", "SourceType", "SourceReferenceId") WHERE SourceReferenceId IS NOT NULL;
            """, ct);
    }

    private static async Task ExecuteTableRebuildAsync(DbConnection conn, string sql, CancellationToken ct)
    {
        await using var pragmaOff = conn.CreateCommand();
        pragmaOff.CommandText = "PRAGMA foreign_keys = OFF; PRAGMA legacy_alter_table = ON;";
        await pragmaOff.ExecuteNonQueryAsync(ct);
        try
        {
            await using var tx = await conn.BeginTransactionAsync(ct);
            await using var rebuildCmd = conn.CreateCommand();
            rebuildCmd.Transaction = tx;
            rebuildCmd.CommandText = sql;
            await rebuildCmd.ExecuteNonQueryAsync(ct);
            await tx.CommitAsync(ct);
        }
        finally
        {
            await using var pragmaOn = conn.CreateCommand();
            pragmaOn.CommandText = "PRAGMA legacy_alter_table = OFF; PRAGMA foreign_keys = ON;";
            await pragmaOn.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }

    #endregion
}
