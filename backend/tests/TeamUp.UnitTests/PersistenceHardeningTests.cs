// Date: 2026-11-26
// Purpose: Unit tests for EF Core 11 persistence hardening: HasSentinel bool persistence, SqliteDevSchemaPatcher legacy upgrades (ScoreTransaction CHECK constraint + nullable Student/StoredImage), and post-commit domain event dispatch.

using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Events;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Domain.Events;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.UnitTests;

public sealed class PersistenceHardeningTests
{
    private sealed class RecordingDispatcher : IDomainEventDispatcher
    {
        public List<IDomainEvent> Dispatched { get; } = [];
        public Func<IDomainEvent, Task>? OnDispatch { get; set; }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            foreach (var evt in domainEvents)
            {
                Dispatched.Add(evt);
                if (OnDispatch is not null)
                    await OnDispatch(evt);
            }
        }
    }

    [Fact]
    public async Task HasSentinel_AllowsPersistingExplicitFalse_ForEventIsPublicAndStudentEmailPrefs()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var uni = new University { Name = "Sentinel U", City = "Cairo", Country = "Egypt" };
        var fac = new Faculty { Name = "CS", Code = "CS", University = uni };
        db.Faculties.Add(fac);
        await db.SaveChangesAsync();

        var student = new Student
        {
            Email = "optout@cu.edu.eg",
            PasswordHash = "hash",
            FirstName = "Opt",
            LastName = "Out",
            AcademicYear = 2,
            FacultyId = fac.FacultyId,
            EmailNotificationsEnabled = false,
            ReceiveLeaderboardDigests = false,
            ReceiveProjectMatchEmails = false,
        };
        db.Students.Add(student);

        var org = new Organization { Name = "Sentinel Org", Description = "d", Category = "Tech" };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var ev = new Event
        {
            Title = "Private Workshop",
            Description = "d",
            Type = EventType.Workshop,
            Location = "Lab",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
            Capacity = 10,
            OrganizationId = org.OrganizationId,
            IsPublic = false,
        };
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var reloadedStudent = await db.Students.SingleAsync(s => s.StudentId == student.StudentId);
        Assert.False(reloadedStudent.EmailNotificationsEnabled);
        Assert.False(reloadedStudent.ReceiveLeaderboardDigests);
        Assert.False(reloadedStudent.ReceiveProjectMatchEmails);

        var reloadedEvent = await db.Events.SingleAsync(e => e.EventId == ev.EventId);
        Assert.False(reloadedEvent.IsPublic);
    }

    [Fact]
    public async Task SqliteDevSchemaPatcher_UpgradesLegacyScoreTransactionCheckAndStoredImageNullability()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = """
                CREATE TABLE "University" ("UniversityId" INTEGER PRIMARY KEY AUTOINCREMENT, "Name" TEXT NOT NULL, "City" TEXT NOT NULL, "Country" TEXT NOT NULL, "Website" TEXT NULL, "DeletedAt" TEXT NULL);
                CREATE TABLE "Faculty" ("FacultyId" INTEGER PRIMARY KEY AUTOINCREMENT, "Name" TEXT NOT NULL, "Code" TEXT NOT NULL, "UniversityId" INTEGER NOT NULL, "DeletedAt" TEXT NULL);
                CREATE TABLE "Student" ("StudentId" INTEGER PRIMARY KEY AUTOINCREMENT, "Email" TEXT NOT NULL, "PasswordHash" TEXT NOT NULL, "FirstName" TEXT NOT NULL, "LastName" TEXT NOT NULL, "AcademicYear" INTEGER NOT NULL, "Role" TEXT NOT NULL DEFAULT 'Student', "SecurityStamp" TEXT NOT NULL, "ShowEmailInDirectory" INTEGER NOT NULL DEFAULT 0, "FacultyId" INTEGER NOT NULL, "RefreshToken" TEXT NULL, "RefreshTokenExpiresAt" TEXT NULL, "PasswordResetToken" TEXT NULL, "PasswordResetTokenExpiresAt" TEXT NULL, "DeletedAt" TEXT NULL);
                CREATE TABLE "Organization" ("OrganizationId" INTEGER PRIMARY KEY AUTOINCREMENT, "Name" TEXT NOT NULL, "Description" TEXT NOT NULL, "Category" TEXT NOT NULL, "LogoUrl" TEXT NULL, "DeletedAt" TEXT NULL);
                CREATE TABLE "OrganizationMember" ("OrganizationId" INTEGER NOT NULL, "StudentId" INTEGER NOT NULL, "Role" TEXT NOT NULL, "JoinedAt" TEXT NOT NULL, "DeletedAt" TEXT NULL, PRIMARY KEY ("OrganizationId", "StudentId"));
                CREATE TABLE "MembershipApplication" ("ApplicationId" INTEGER PRIMARY KEY AUTOINCREMENT, "OrganizationId" INTEGER NOT NULL, "StudentId" INTEGER NOT NULL, "Status" TEXT NOT NULL, "AppliedAt" TEXT NOT NULL, "DeletedAt" TEXT NULL);
                CREATE TABLE "Event" ("EventId" INTEGER PRIMARY KEY AUTOINCREMENT, "Title" TEXT NOT NULL, "Description" TEXT NOT NULL, "Type" TEXT NOT NULL, "Location" TEXT NOT NULL, "StartDate" TEXT NOT NULL, "EndDate" TEXT NOT NULL, "Capacity" INTEGER NOT NULL, "OrganizationId" INTEGER NOT NULL, "RowVersion" BLOB NOT NULL, "DeletedAt" TEXT NULL);
                CREATE TABLE "Project" ("ProjectId" INTEGER PRIMARY KEY AUTOINCREMENT, "Title" TEXT NOT NULL, "Description" TEXT NOT NULL, "Status" TEXT NOT NULL, "MaxTeamSize" INTEGER NOT NULL, "CreatorId" INTEGER NOT NULL, "RowVersion" BLOB NOT NULL, "DeletedAt" TEXT NULL, "CreatedAt" TEXT NOT NULL);
                CREATE TABLE "ScoreTransaction" (
                    "TransactionId" INTEGER NOT NULL CONSTRAINT "PK_ScoreTransaction" PRIMARY KEY AUTOINCREMENT,
                    "StudentId" INTEGER NOT NULL,
                    "Points" INTEGER NOT NULL,
                    "Reason" TEXT NOT NULL,
                    "SourceType" TEXT NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    CONSTRAINT "CK_ScoreTransaction_SourceType" CHECK ([SourceType] IN ('EventAttendance', 'TaskCompleted', 'ProjectDelivered', 'Manual')),
                    CONSTRAINT "FK_ScoreTransaction_Student_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Student" ("StudentId") ON DELETE RESTRICT
                );
                CREATE TABLE "StoredImage" (
                    "StoredImageId" TEXT NOT NULL CONSTRAINT "PK_StoredImage" PRIMARY KEY,
                    "UploadedByStudentId" INTEGER NOT NULL,
                    "Content" BLOB NOT NULL,
                    "ContentType" TEXT NOT NULL,
                    "SizeBytes" INTEGER NOT NULL,
                    "UploadedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_StoredImage_Student_UploadedByStudentId" FOREIGN KEY ("UploadedByStudentId") REFERENCES "Student" ("StudentId") ON DELETE RESTRICT
                );
                INSERT INTO "University" ("Name", "City", "Country") VALUES ('Cairo U', 'Giza', 'Egypt');
                INSERT INTO "Faculty" ("Name", "Code", "UniversityId") VALUES ('FCAI', 'FCAI', 1);
                INSERT INTO "Student" ("Email", "PasswordHash", "FirstName", "LastName", "AcademicYear", "SecurityStamp", "FacultyId") VALUES ('legacy@cu.edu.eg', 'h', 'Leg', 'Acy', 2, '11111111-1111-1111-1111-111111111111', 1);
                INSERT INTO "ScoreTransaction" ("StudentId", "Points", "Reason", "SourceType", "CreatedAt") VALUES (1, 25, 'Session', 'EventAttendance', '2026-01-01T00:00:00Z');
                INSERT INTO "StoredImage" ("StoredImageId", "UploadedByStudentId", "Content", "ContentType", "SizeBytes", "UploadedAt") VALUES ('00000000-0000-0000-0000-000000000099', 1, X'89504E47', 'image/png', 4, '2026-01-01T00:00:00Z');
                """;
            await cmd.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        await using var db = new AppDbContext(options);

        await SqliteDevSchemaPatcher.PatchAsync(db);

        // Verify legacy data survived and new AttendanceRevocation + nullable FacultyId + nullable UploadedByStudentId work.
        db.Students.Add(new Student
        {
            Email = "indie@teamup.dev",
            PasswordHash = "h",
            FirstName = "Indie",
            LastName = "Learner",
            AcademicYear = 1,
            FacultyId = null,
            AffiliationType = AffiliationType.IndependentLearner,
        });
        db.ScoreTransactions.Add(new ScoreTransaction
        {
            StudentId = 1,
            Points = -25,
            Reason = "Revoked",
            SourceType = ScoreSourceType.AttendanceRevocation,
            SourceReferenceId = 7,
        });
        db.StoredImages.Add(new StoredImage
        {
            StoredImageId = Guid.NewGuid(),
            UploadedByStudentId = null,
            Content = [1, 2, 3, 4],
            ContentType = "image/png",
            SizeBytes = 4,
        });
        await db.SaveChangesAsync();

        Assert.Equal(2, await db.Students.CountAsync());
        Assert.Equal(2, await db.ScoreTransactions.CountAsync());
        Assert.Equal(2, await db.StoredImages.CountAsync());
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_DefersDomainEventsUntilAfterCommit()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();
        var dispatcher = new RecordingDispatcher();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        await using var db = new AppDbContext(options, dispatcher);
        await db.Database.EnsureCreatedAsync();

        bool? sawOpenTxDuringDispatch = null;
        dispatcher.OnDispatch = _ =>
        {
            sawOpenTxDuringDispatch = db.Database.CurrentTransaction is not null;
            return Task.CompletedTask;
        };

        await db.ExecuteInTransactionAsync(IsolationLevel.Serializable, async ct =>
        {
            var uni = new University { Name = "Tx Uni", City = "Cairo", Country = "Egypt" };
            uni.AddDomainEvent(new WaitlistPromotedDomainEvent(1, 2, "Workshop"));
            db.Universities.Add(uni);
            await db.SaveChangesAsync(ct);

            // While inside transaction before commit, event must not have been dispatched yet!
            Assert.Empty(dispatcher.Dispatched);
        });

        Assert.Single(dispatcher.Dispatched);
        Assert.False(sawOpenTxDuringDispatch);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenBeginTransactionFails_ResetsDeferredDomainEventsFlag()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();
        var dispatcher = new RecordingDispatcher();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        await using var db = new AppDbContext(options, dispatcher);
        await db.Database.EnsureCreatedAsync();

        // Force db.Database.BeginTransactionAsync to throw by holding an active native transaction.
        await using (var blockingTx = await conn.BeginTransactionAsync())
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                db.ExecuteInTransactionAsync(IsolationLevel.Serializable, _ => Task.CompletedTask));
            await blockingTx.RollbackAsync();
        }

        // Subsequent non-transactional SaveChangesAsync must still dispatch domain events immediately.
        var uni = new University { Name = "Recovered Uni", City = "Cairo", Country = "Egypt" };
        uni.AddDomainEvent(new WaitlistPromotedDomainEvent(10, 20, "Recovery Workshop"));
        db.Universities.Add(uni);
        await db.SaveChangesAsync();

        Assert.Single(dispatcher.Dispatched);
    }

    [Fact]
    public async Task PostCommitDomainEventHandlerFailure_DoesNotFailCommittedTransaction()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var dispatcher = new RecordingDispatcher { OnDispatch = _ => throw new InvalidOperationException("SMTP down") };
        await using var db = new AppDbContext(options, dispatcher);
        await db.Database.EnsureCreatedAsync();

        var org = new Organization { Name = "Resilient Org", Description = "d", Category = "Tech" };
        org.AddDomainEvent(new WaitlistPromotedDomainEvent(1, 1, "Evt"));
        db.Organizations.Add(org);

        await db.ExecuteInTransactionAsync(IsolationLevel.Serializable, async token =>
        {
            await db.SaveChangesAsync(token);
        });

        Assert.Single(dispatcher.Dispatched);
        db.ChangeTracker.Clear();
        Assert.True(await db.Organizations.AnyAsync(o => o.Name == "Resilient Org"));
    }
}