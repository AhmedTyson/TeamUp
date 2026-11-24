using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlexibleActivitiesAndHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FacultyId",
                table: "Student",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AffiliationType",
                table: "Student",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                defaultValue: "UniversityStudent");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Student",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HierarchyTier",
                table: "OrganizationMember",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                defaultValue: "OrganizingCrew");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationUnitId",
                table: "OrganizationMember",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Organization",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Organization",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentSeasonName",
                table: "Organization",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecruiting",
                table: "Organization",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "Organization",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                defaultValue: "UniversityBranch");

            migrationBuilder.AddColumn<int>(
                name: "UniversityId",
                table: "Organization",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Organization",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EvaluationScore",
                table: "MembershipApplication",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterviewNotes",
                table: "MembershipApplication",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationUnitId",
                table: "MembershipApplication",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewerStudentId",
                table: "MembershipApplication",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeasonName",
                table: "MembershipApplication",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "MembershipApplication",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                defaultValue: "Applied");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMode",
                table: "Event",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                defaultValue: "InPerson");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Event",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingUrl",
                table: "Event",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationUnitId",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationUnit",
                columns: table => new
                {
                    OrganizationUnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    UnitType = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "TechnicalChapter"),
                    IsRecruiting = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OpenSeats = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnit", x => x.OrganizationUnitId);
                    table.CheckConstraint("CK_OrganizationUnit_UnitType", "[UnitType] IN ('TechnicalChapter', 'OperationalCommittee', 'AffinityGroup', 'WorkingGroup')");
                    table.ForeignKey(
                        name: "FK_OrganizationUnit_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Student_AffiliationType",
                table: "Student",
                sql: "[AffiliationType] IN ('UniversityStudent', 'IndependentLearner', 'FreshGraduate', 'HighSchoolTech')");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMember_OrganizationUnitId",
                table: "OrganizationMember",
                column: "OrganizationUnitId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrganizationMember_HierarchyTier",
                table: "OrganizationMember",
                sql: "[HierarchyTier] IN ('HighBoard', 'CommitteeLeadership', 'OrganizingCrew', 'Participant')");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Scope_IsRecruiting",
                table: "Organization",
                columns: new[] { "Scope", "IsRecruiting" },
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_UniversityId",
                table: "Organization",
                column: "UniversityId",
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Organization_Scope",
                table: "Organization",
                sql: "[Scope] IN ('UniversityBranch', 'FacultyChapter', 'CrossUniversity', 'Independent', 'NationalNetwork')");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipApplication_OrganizationId_Stage",
                table: "MembershipApplication",
                columns: new[] { "OrganizationId", "Stage" },
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipApplication_OrganizationUnitId",
                table: "MembershipApplication",
                column: "OrganizationUnitId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MembershipApplication_Stage",
                table: "MembershipApplication",
                sql: "[Stage] IN ('Applied', 'HrScreening', 'CommitteeInterview', 'Accepted', 'Rejected', 'Withdrawn')");

            migrationBuilder.CreateIndex(
                name: "IX_Event_OrganizationUnitId",
                table: "Event",
                column: "OrganizationUnitId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Event_DeliveryMode",
                table: "Event",
                sql: "[DeliveryMode] IN ('InPerson', 'Online', 'Hybrid')");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_OrganizationId_Code",
                table: "OrganizationUnit",
                columns: new[] { "OrganizationId", "Code" },
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Event_OrganizationUnit_OrganizationUnitId",
                table: "Event",
                column: "OrganizationUnitId",
                principalTable: "OrganizationUnit",
                principalColumn: "OrganizationUnitId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipApplication_OrganizationUnit_OrganizationUnitId",
                table: "MembershipApplication",
                column: "OrganizationUnitId",
                principalTable: "OrganizationUnit",
                principalColumn: "OrganizationUnitId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organization_University_UniversityId",
                table: "Organization",
                column: "UniversityId",
                principalTable: "University",
                principalColumn: "UniversityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMember_OrganizationUnit_OrganizationUnitId",
                table: "OrganizationMember",
                column: "OrganizationUnitId",
                principalTable: "OrganizationUnit",
                principalColumn: "OrganizationUnitId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_OrganizationUnit_OrganizationUnitId",
                table: "Event");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipApplication_OrganizationUnit_OrganizationUnitId",
                table: "MembershipApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_Organization_University_UniversityId",
                table: "Organization");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMember_OrganizationUnit_OrganizationUnitId",
                table: "OrganizationMember");

            migrationBuilder.DropTable(
                name: "OrganizationUnit");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Student_AffiliationType",
                table: "Student");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationMember_OrganizationUnitId",
                table: "OrganizationMember");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrganizationMember_HierarchyTier",
                table: "OrganizationMember");

            migrationBuilder.DropIndex(
                name: "IX_Organization_Scope_IsRecruiting",
                table: "Organization");

            migrationBuilder.DropIndex(
                name: "IX_Organization_UniversityId",
                table: "Organization");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Organization_Scope",
                table: "Organization");

            migrationBuilder.DropIndex(
                name: "IX_MembershipApplication_OrganizationId_Stage",
                table: "MembershipApplication");

            migrationBuilder.DropIndex(
                name: "IX_MembershipApplication_OrganizationUnitId",
                table: "MembershipApplication");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MembershipApplication_Stage",
                table: "MembershipApplication");

            migrationBuilder.DropIndex(
                name: "IX_Event_OrganizationUnitId",
                table: "Event");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Event_DeliveryMode",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "AffiliationType",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "HierarchyTier",
                table: "OrganizationMember");

            migrationBuilder.DropColumn(
                name: "OrganizationUnitId",
                table: "OrganizationMember");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "CurrentSeasonName",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "IsRecruiting",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "UniversityId",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "EvaluationScore",
                table: "MembershipApplication");

            migrationBuilder.DropColumn(
                name: "InterviewNotes",
                table: "MembershipApplication");

            migrationBuilder.DropColumn(
                name: "OrganizationUnitId",
                table: "MembershipApplication");

            migrationBuilder.DropColumn(
                name: "ReviewerStudentId",
                table: "MembershipApplication");

            migrationBuilder.DropColumn(
                name: "SeasonName",
                table: "MembershipApplication");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "MembershipApplication");

            migrationBuilder.DropColumn(
                name: "DeliveryMode",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "MeetingUrl",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "OrganizationUnitId",
                table: "Event");

            migrationBuilder.AlterColumn<int>(
                name: "FacultyId",
                table: "Student",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
