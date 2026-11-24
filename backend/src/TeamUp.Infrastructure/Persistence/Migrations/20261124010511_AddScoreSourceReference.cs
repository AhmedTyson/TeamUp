using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreSourceReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceReferenceId",
                table: "ScoreTransaction",
                type: "int",
                nullable: true);

            // Backfill the key from the legacy "...#<id>: title" reason text. Only the earliest row per
            // (student, source, id) gets the key so the unique index can be created; historical duplicate
            // awards are left untouched (never deleted) for the owner to review.
            migrationBuilder.Sql(@"
WITH Parsed AS (
    SELECT TransactionId, StudentId, SourceType,
           TRY_CAST(SUBSTRING(Reason, CHARINDEX('#', Reason) + 1,
                    CHARINDEX(':', Reason, CHARINDEX('#', Reason)) - CHARINDEX('#', Reason) - 1) AS int) AS RefId
    FROM ScoreTransaction
    WHERE SourceType IN ('TaskCompleted', 'EventAttendance', 'ProjectDelivered')
      AND CHARINDEX('#', Reason) > 0
      AND CHARINDEX(':', Reason, CHARINDEX('#', Reason)) > CHARINDEX('#', Reason) + 1
), Ranked AS (
    SELECT TransactionId, RefId,
           ROW_NUMBER() OVER (PARTITION BY StudentId, SourceType, RefId ORDER BY TransactionId) AS rn
    FROM Parsed WHERE RefId IS NOT NULL
)
UPDATE t SET SourceReferenceId = r.RefId
FROM ScoreTransaction t
JOIN Ranked r ON r.TransactionId = t.TransactionId
WHERE r.rn = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTransaction_StudentId_SourceType_SourceReferenceId",
                table: "ScoreTransaction",
                columns: new[] { "StudentId", "SourceType", "SourceReferenceId" },
                unique: true,
                filter: "[SourceReferenceId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScoreTransaction_StudentId_SourceType_SourceReferenceId",
                table: "ScoreTransaction");

            migrationBuilder.DropColumn(
                name: "SourceReferenceId",
                table: "ScoreTransaction");
        }
    }
}
