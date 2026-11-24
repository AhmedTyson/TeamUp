using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationCreationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationCreationRequest",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterStudentId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Scope = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "UniversityBranch"),
                    UniversityId = table.Column<int>(type: "int", nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CurrentSeasonName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Justification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "Pending"),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByStudentId = table.Column<int>(type: "int", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedOrganizationId = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationCreationRequest", x => x.RequestId);
                    table.CheckConstraint("CK_OrganizationCreationRequest_Scope", "[Scope] IN ('UniversityBranch', 'FacultyChapter', 'CrossUniversity', 'Independent', 'NationalNetwork')");
                    table.CheckConstraint("CK_OrganizationCreationRequest_Status", "[Status] IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')");
                    table.ForeignKey(
                        name: "FK_OrganizationCreationRequest_Organization_CreatedOrganizationId",
                        column: x => x.CreatedOrganizationId,
                        principalTable: "Organization",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationCreationRequest_Student_RequesterStudentId",
                        column: x => x.RequesterStudentId,
                        principalTable: "Student",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationCreationRequest_Student_ReviewedByStudentId",
                        column: x => x.ReviewedByStudentId,
                        principalTable: "Student",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationCreationRequest_University_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "University",
                        principalColumn: "UniversityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCreationRequest_CreatedOrganizationId",
                table: "OrganizationCreationRequest",
                column: "CreatedOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCreationRequest_RequesterStudentId",
                table: "OrganizationCreationRequest",
                column: "RequesterStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCreationRequest_RequesterStudentId_Name",
                table: "OrganizationCreationRequest",
                columns: new[] { "RequesterStudentId", "Name" },
                unique: true,
                filter: "[DeletedAt] IS NULL AND [Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCreationRequest_ReviewedByStudentId",
                table: "OrganizationCreationRequest",
                column: "ReviewedByStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCreationRequest_Status_RequestedAt",
                table: "OrganizationCreationRequest",
                columns: new[] { "Status", "RequestedAt" },
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCreationRequest_UniversityId",
                table: "OrganizationCreationRequest",
                column: "UniversityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationCreationRequest");
        }
    }
}
