// Date: 2026-11-24
// Purpose: Regression tests for seeder hardening (TU-SEC-003/008) and deployment/config hygiene (TU-SEC-004/005).

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Security;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Seed;

namespace TeamUp.UnitTests;

public class Phase1SecurityTests
{
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "netlify.toml")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("Admin@TeamUp2026!")]
    [InlineData("StudentPass123!")]
    [InlineData("REPLACE_WITH_STRONG_ADMIN_PASSWORD_12_PLUS")]
    public void SeedAdminPassword_RejectsWeakOrPubliclyDocumentedValues(string? password) =>
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.ValidateSeedAdminPassword(password));

    [Fact]
    public void SeedAdminPassword_AcceptsUniqueStrongValue() =>
        SecurityStartupValidator.ValidateSeedAdminPassword("k7#Vq2!mZp9-Lx4Rt");

    [Fact]
    public void ProductionExampleConfig_DoesNotEnableSeeder()
    {
        var path = Path.Combine(RepoRoot(), "backend/src/TeamUp.Api/appsettings.Production.example.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        Assert.False(doc.RootElement.GetProperty("Seed").GetProperty("Enabled").GetBoolean());
    }

    // Note: DeployWorkflow_KeepsTlsValidationAndRequiresEnvironmentApproval is deferred to Phase 29
    // after verifying MonsterASP WebDeploy :8172 TLS trust and GitHub production environment configuration.

    [Fact]
    public void SpaHostConfigs_ShipContentSecurityPolicy()
    {
        var root = RepoRoot();
        Assert.Contains("Content-Security-Policy", File.ReadAllText(Path.Combine(root, "netlify.toml")));
        Assert.Contains("Content-Security-Policy", File.ReadAllText(Path.Combine(root, "frontend/vercel.json")));
    }

    [Fact]
    public void DocsDoNotPublishSeedPasswords()
    {
        var root = RepoRoot();
        foreach (var rel in new[] { "README.md", "wiki/Getting Started Guide.md", "wiki/DEPLOYMENT.md" })
        {
            var text = File.ReadAllText(Path.Combine(root, rel));
            Assert.DoesNotContain("Admin@TeamUp2026!", text);
            Assert.DoesNotContain("StudentPass123!", text);
        }
    }

    private static AppDbContext NewDb(SqliteConnection conn)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task Seeder_OutsideDevelopment_NeverOverwritesExistingPasswords_AndSkipsDemoOrgAdmin()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        using var db = NewDb(conn);
        var hasher = new PasswordHasher<Student>();

        await DbSeeder.SeedAsync(db, hasher.HashPassword(null!, "first-Bootstrap-Pass-1"), isDevelopment: false);

        Assert.False(await db.Students.AnyAsync(s => s.Email == "nour@cu.edu.eg"));
        var admin = await db.Students.SingleAsync(s => s.Email == "admin@teamup.local");
        var originalAdminHash = admin.PasswordHash;

        var showcase = await db.Students.Where(s => s.Role == UserRole.Student).ToListAsync();
        Assert.NotEmpty(showcase);
        foreach (var s in showcase)
            Assert.Equal(PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(s, s.PasswordHash, "first-Bootstrap-Pass-1"));

        // A second run with a different password (e.g. config drift) must not take over the account.
        await DbSeeder.SeedAsync(db, hasher.HashPassword(null!, "attacker-Known-Pass-2"), isDevelopment: false);
        db.ChangeTracker.Clear();
        var adminAfter = await db.Students.SingleAsync(s => s.Email == "admin@teamup.local");
        Assert.Equal(originalAdminHash, adminAfter.PasswordHash);
    }

    [Fact]
    public async Task Seeder_InDevelopment_StillSeedsDemoAccounts()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        using var db = NewDb(conn);
        await DbSeeder.SeedAsync(db, new PasswordHasher<Student>().HashPassword(null!, "dev-Pass-12345"), isDevelopment: true);
        Assert.True(await db.Students.AnyAsync(s => s.Email == "nour@cu.edu.eg"));
    }
}

public class PasswordLengthValidatorTests
{
    [Fact]
    public void Register_Login_Reset_RejectOversizedPasswords()
    {
        var huge = "Aa1" + new string('x', 200);
        Assert.False(new TeamUp.Application.Auth.Validators.RegisterRequestValidator()
            .Validate(new TeamUp.Application.Auth.RegisterRequest("A", "B", "a@b.co", huge, 2, 1)).IsValid);
        Assert.False(new TeamUp.Application.Auth.Validators.LoginRequestValidator()
            .Validate(new TeamUp.Application.Auth.LoginRequest("a@b.co", huge)).IsValid);
        Assert.False(new TeamUp.Application.Auth.Validators.ResetPasswordRequestValidator()
            .Validate(new TeamUp.Application.Auth.ResetPasswordRequest("a@b.co", "tok", huge)).IsValid);
    }
}
