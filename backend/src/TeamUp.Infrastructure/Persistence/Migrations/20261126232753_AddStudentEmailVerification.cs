using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261126232753_AddStudentEmailVerification : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "EmailVerificationToken",
            table: "Student",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "EmailVerificationTokenExpiresAt",
            table: "Student",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "EmailVerifiedAtUtc",
            table: "Student",
            type: "datetime2",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EmailVerificationToken",
            table: "Student");

        migrationBuilder.DropColumn(
            name: "EmailVerificationTokenExpiresAt",
            table: "Student");

        migrationBuilder.DropColumn(
            name: "EmailVerifiedAtUtc",
            table: "Student");
    }
}
