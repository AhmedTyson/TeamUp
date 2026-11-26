using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ScoreTransaction_SourceType",
                table: "ScoreTransaction");

            migrationBuilder.CreateTable(
                name: "StoredImage",
                columns: table => new
                {
                    StoredImageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByStudentId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredImage", x => x.StoredImageId);
                    table.ForeignKey(
                        name: "FK_StoredImage_Student_UploadedByStudentId",
                        column: x => x.UploadedByStudentId,
                        principalTable: "Student",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ScoreTransaction_SourceType",
                table: "ScoreTransaction",
                sql: "[SourceType] IN ('EventAttendance', 'TaskCompleted', 'ProjectDelivered', 'Manual', 'AttendanceRevocation')");

            migrationBuilder.CreateIndex(
                name: "IX_StoredImage_UploadedByStudentId",
                table: "StoredImage",
                column: "UploadedByStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoredImage");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ScoreTransaction_SourceType",
                table: "ScoreTransaction");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ScoreTransaction_SourceType",
                table: "ScoreTransaction",
                sql: "[SourceType] IN ('EventAttendance', 'TaskCompleted', 'ProjectDelivered', 'Manual')");
        }
    }
}
