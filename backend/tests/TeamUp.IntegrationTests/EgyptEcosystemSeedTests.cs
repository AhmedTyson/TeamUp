// Date: 2026-11-25
// Purpose: Verifies Egyptian campus JSON seed integrity, BIS Helwan presence, idempotent production execution, and CIA/AAA invariants.

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Seed;

namespace TeamUp.IntegrationTests;

public class EgyptEcosystemSeedTests
{
    private static AppDbContext CreateSqliteDb(SqliteConnection conn)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task SyncEcosystemCatalog_InProductionMode_IsIdempotent_AndEnforcesCiaAaaInvariants()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();
        await using var db = CreateSqliteDb(conn);

        // First production sync
        await DbSeeder.SyncEcosystemCatalogAsync(db, isDevelopment: false);

        var uniCount1 = await db.Universities.CountAsync();
        var facCount1 = await db.Faculties.CountAsync();
        var stuCount1 = await db.Students.CountAsync();
        var orgCount1 = await db.Organizations.CountAsync();
        var unitCount1 = await db.OrganizationUnits.CountAsync();
        var memberCount1 = await db.OrganizationMembers.CountAsync();
        var projCount1 = await db.Projects.CountAsync();
        var evtCount1 = await db.Events.CountAsync();
        var appCount1 = await db.MembershipApplications.CountAsync();

        // Forced second sync + non-forced fast-path skip must produce zero duplicates
        await DbSeeder.SyncEcosystemCatalogAsync(db, isDevelopment: false, forceSync: true);
        await DbSeeder.SyncEcosystemCatalogAsync(db, isDevelopment: false, forceSync: false);

        Assert.Equal(uniCount1, await db.Universities.CountAsync());
        Assert.Equal(facCount1, await db.Faculties.CountAsync());
        Assert.Equal(stuCount1, await db.Students.CountAsync());
        Assert.Equal(orgCount1, await db.Organizations.CountAsync());
        Assert.Equal(unitCount1, await db.OrganizationUnits.CountAsync());
        Assert.Equal(memberCount1, await db.OrganizationMembers.CountAsync());
        Assert.Equal(projCount1, await db.Projects.CountAsync());
        Assert.Equal(evtCount1, await db.Events.CountAsync());
        Assert.Equal(appCount1, await db.MembershipApplications.CountAsync());

        // Egyptian universities + BIS Helwan + HTTPS logos
        Assert.True(uniCount1 >= 45, $"Expected >= 45 Egyptian universities, got {uniCount1}");
        var unis = await db.Universities.Include(u => u.Faculties).ToListAsync();
        Assert.All(unis, u =>
        {
            Assert.False(string.IsNullOrWhiteSpace(u.LogoUrl));
            Assert.StartsWith("https://", u.LogoUrl!, StringComparison.OrdinalIgnoreCase);
        });

        var helwan = unis.Single(u => u.Name == "Helwan University");
        Assert.Contains(helwan.Faculties, f => f.Code == "BIS_HU" && f.Name.Contains("Business Information Systems", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(helwan.Faculties, f => f.Code == "FCAI_HU");
        Assert.Contains(helwan.Faculties, f => f.Code == "ENG_HU");

        // Student activities, verified logos, units, and 4-tier hierarchy
        Assert.True(orgCount1 >= 20, $"Expected >= 20 student activities, got {orgCount1}");
        var orgs = await db.Organizations.ToListAsync();
        Assert.All(orgs, o =>
        {
            Assert.False(string.IsNullOrWhiteSpace(o.LogoUrl));
            Assert.StartsWith("https://", o.LogoUrl!, StringComparison.OrdinalIgnoreCase);
        });
        var tiers = await db.OrganizationMembers.Select(m => m.HierarchyTier).Distinct().ToListAsync();
        Assert.Contains(OrganizationHierarchyTier.HighBoard, tiers);
        Assert.Contains(OrganizationHierarchyTier.CommitteeLeadership, tiers);
        Assert.Contains(OrganizationHierarchyTier.OrganizingCrew, tiers);
        Assert.Contains(OrganizationHierarchyTier.Participant, tiers);

        // CIA + AuthN + Confidentiality checks on production students
        Assert.True(stuCount1 >= 40, $"Expected >= 40 Egyptian community members, got {stuCount1}");
        Assert.False(await db.Students.AnyAsync(s => s.Email == "nour@cu.edu.eg"));
        var students = await db.Students.Include(s => s.Profile).ToListAsync();
        var hasher = new PasswordHasher<Student>();
        var distinctHashes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var stu in students)
        {
            Assert.Equal(UserRole.Student, stu.Role);
            Assert.False(stu.ShowEmailInDirectory);
            Assert.NotEqual(Guid.Empty, stu.SecurityStamp);
            Assert.True(distinctHashes.Add(stu.PasswordHash), $"Duplicate hash for {stu.Email}");
            Assert.Equal(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(stu, stu.PasswordHash, "Admin@TeamUp2026!"));
            Assert.Equal(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(stu, stu.PasswordHash, "StudentPass123!"));
            Assert.Equal(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(stu, stu.PasswordHash, "DevelopmentSeedPassword2026!"));
        }
        Assert.Contains(students, s => s.AffiliationType == AffiliationType.IndependentLearner);
        Assert.Contains(students, s => s.AffiliationType == AffiliationType.FreshGraduate);
        Assert.Contains(students, s => s.AffiliationType == AffiliationType.HighSchoolTech);
        Assert.Contains(students, s => s.IsEmailVerified);
        Assert.Contains(students, s => !s.IsEmailVerified);

        // Integrity on projects, events, and 4 Kanban recruitment stages
        Assert.True(projCount1 >= 15);
        var projects = await db.Projects.Include(p => p.Members).ToListAsync();
        Assert.All(projects, p =>
        {
            Assert.True(p.Members.Count >= 1);
            Assert.True(p.Members.Count <= p.MaxTeamSize);
        });

        Assert.True(evtCount1 >= 10);
        var sessions = await db.EventSessions.ToListAsync();
        Assert.All(sessions, s => Assert.True(s.EndTime > s.StartTime));

        var stages = await db.MembershipApplications.Select(a => a.Stage).Distinct().ToListAsync();
        Assert.Contains(RecruitmentStage.Applied, stages);
        Assert.Contains(RecruitmentStage.HrScreening, stages);
        Assert.Contains(RecruitmentStage.CommitteeInterview, stages);
        Assert.Contains(RecruitmentStage.Accepted, stages);

        // AAA Immutable AuditLog marker + absence of synthetic test copy
        var seedAuditLogs = await db.AuditLogs
            .Where(a => a.Action == DbSeeder.EcosystemSeedAction && a.EntityType == DbSeeder.EcosystemSeedEntityType)
            .ToListAsync();
        Assert.Single(seedAuditLogs);
        Assert.DoesNotContain("Password", seedAuditLogs[0].NewValues ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        foreach (var p in projects)
        {
            Assert.DoesNotContain("fake", p.Description, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("dummy", p.Description, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task SeedAsync_InDevelopmentMode_KeepsCredentialTestingAccountsUnverified()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();
        await using var db = CreateSqliteDb(conn);

        var hasher = new PasswordHasher<Student>();
        var devHash = hasher.HashPassword(new Student { Email = "seed@teamup.local" }, "DevPass2026!");
        await DbSeeder.SeedAsync(db, devHash, isDevelopment: true);

        var admin = await db.Students.SingleAsync(s => s.Email == "admin@teamup.local");
        var orgAdminDemo = await db.Students.SingleAsync(s => s.Email == "nour@cu.edu.eg");
        Assert.False(admin.IsEmailVerified);
        Assert.False(orgAdminDemo.IsEmailVerified);

        var verifiedCount = await db.Students.CountAsync(s => s.EmailVerifiedAtUtc != null);
        var unverifiedCount = await db.Students.CountAsync(s => s.EmailVerifiedAtUtc == null);
        Assert.True(verifiedCount > 20, $"Expected > 20 verified seeded students, got {verifiedCount}");
        Assert.True(unverifiedCount >= 2, $"Expected >= 2 unverified accounts including credential testers, got {unverifiedCount}");
    }
}
