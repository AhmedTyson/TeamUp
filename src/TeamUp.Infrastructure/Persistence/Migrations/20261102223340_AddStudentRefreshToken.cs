using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Student",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiresAt",
                table: "Student",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiresAt",
                table: "Student");
        }
    }
}
