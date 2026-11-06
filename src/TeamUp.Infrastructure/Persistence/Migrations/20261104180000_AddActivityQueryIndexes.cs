using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TeamUp.Infrastructure.Persistence;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261104180000_AddActivityQueryIndexes")]
public partial class AddActivityQueryIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Organization_Category_Name",
            table: "Organization",
            columns: new[] { "Category", "Name" },
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationMember_OrganizationId_Role_JoinedAt",
            table: "OrganizationMember",
            columns: new[] { "OrganizationId", "Role", "JoinedAt" },
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_MembershipApplication_OrganizationId_AppliedAt",
            table: "MembershipApplication",
            columns: new[] { "OrganizationId", "AppliedAt" },
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Event_StartDate_Title",
            table: "Event",
            columns: new[] { "StartDate", "Title" },
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Event_OrganizationId_StartDate",
            table: "Event",
            columns: new[] { "OrganizationId", "StartDate" },
            filter: "[DeletedAt] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_EventRegistration_EventId_Status_RegisteredAt",
            table: "EventRegistration",
            columns: new[] { "EventId", "Status", "RegisteredAt" });

        migrationBuilder.CreateIndex(
            name: "IX_SessionAttendance_SessionId_CheckInTime",
            table: "SessionAttendance",
            columns: new[] { "SessionId", "CheckInTime" },
            filter: "[DeletedAt] IS NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_Organization_Category_Name", "Organization");
        migrationBuilder.DropIndex("IX_OrganizationMember_OrganizationId_Role_JoinedAt", "OrganizationMember");
        migrationBuilder.DropIndex("IX_MembershipApplication_OrganizationId_AppliedAt", "MembershipApplication");
        migrationBuilder.DropIndex("IX_Event_StartDate_Title", "Event");
        migrationBuilder.DropIndex("IX_Event_OrganizationId_StartDate", "Event");
        migrationBuilder.DropIndex("IX_EventRegistration_EventId_Status_RegisteredAt", "EventRegistration");
        migrationBuilder.DropIndex("IX_SessionAttendance_SessionId_CheckInTime", "SessionAttendance");
    }
}
