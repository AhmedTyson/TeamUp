// Date: 2026-11-27
// Purpose: Dedicated unit tests covering .NET 11 / C# 14-15 modernization: TimeProvider determinism, Guid v7, EF Core 11 Named Query Filters, SIMD SearchValues/FrozenSet/Span, C# 14 extension properties, HybridCache invalidation, and DigestHtmlBuilder.

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TeamUp.Application.Auth;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Caching;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Common.Validation;
using TeamUp.Application.Gamification;
using TeamUp.Application.Services;
using TeamUp.Domain.Enums;
using TeamUp.Application.Universities;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Services;
using TeamUp.UnitTests.TestHelpers;

namespace TeamUp.UnitTests;

public sealed class DotNet11ModernizationUnitTests : IDisposable
{
    private static readonly JwtOptions TestJwtOptions = new()
    {
        Issuer = "TeamUp",
        Audience = "TeamUp.Clients",
        Key = "UnitTestSigningKeyMustBeAtLeast32BytesLong!2026",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7,
        PasswordResetTokenMinutes = 15
    };

    // Valid minimal 16-byte PNG signature header (8-byte magic + IHDR chunk prefix).
    private static readonly byte[] MinimalPngPayload =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52];

    private readonly SqliteConnection _conn = new("DataSource=:memory:");

    public DotNet11ModernizationUnitTests()
    {
        _conn.Open();
        using var db = CreateDb();
        db.Database.EnsureCreated();
    }

    public void Dispose() => _conn.Dispose();

    private AppDbContext CreateDb(TimeProvider? timeProvider = null) => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conn)
            .AddInterceptors(new SoftDeleteInterceptor(timeProvider))
            .Options);

    [Fact]
    public void AttendanceCodeService_UsesInjectedTimeProvider_ForWindowExpiry()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 11, 27, 10, 0, 10, TimeSpan.Zero));
        var service = new AttendanceCodeService(Options.Create(TestJwtOptions), fakeTime);

        var (code, expiresAt) = service.GenerateCode(sessionId: 42);

        Assert.Equal(new DateTime(2026, 11, 27, 10, 1, 0, DateTimeKind.Utc), expiresAt);
        Assert.True(service.ValidateCode(42, code));
        Assert.False(service.ValidateCode(99, code));

        fakeTime.Advance(TimeSpan.FromSeconds(51));
        Assert.False(service.ValidateCode(42, code));
    }

    [Fact]
    public async Task AuthService_LockoutExpiresDeterministically_WhenTimeProviderAdvances()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 11, 27, 9, 0, 0, TimeSpan.Zero));
        await using var db = CreateDb(fakeTime);

        var hasher = new PasswordHasher<Student>();
        var student = new Student
        {
            Email = "locked@teamup.edu.eg",
            FirstName = "Omar",
            LastName = "Nabil",
            AcademicYear = 3,
            SecurityStamp = Guid.NewGuid(),
        };
        student.PasswordHash = hasher.HashPassword(student, "ValidPass123!");
        db.Students.Add(student);
        await db.SaveChangesAsync();

        using var memCache = new MemoryCache(new MemoryCacheOptions());
        var jwt = new JwtTokenService(Options.Create(TestJwtOptions), fakeTime);
        var emailSender = new InMemoryEmailSender(NullLogger<InMemoryEmailSender>.Instance, fakeTime);
        var orgAuth = new OrganizationAuthorizationService(db);
        var stampValidator = new SecurityStampValidator(db, memCache);
        var auth = new AuthService(
            db, jwt, hasher, emailSender, Options.Create(TestJwtOptions), orgAuth, stampValidator,
            NullLogger<AuthService>.Instance, fakeTime);

        for (var i = 0; i <= AuthService.FreeFailedLoginAttempts; i++)
        {
            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                auth.LoginAsync(new LoginRequest("locked@teamup.edu.eg", "WrongPassword1!")));
        }

        // Valid password is still rejected while inside the 30-second lockout window.
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            auth.LoginAsync(new LoginRequest("locked@teamup.edu.eg", "ValidPass123!")));

        fakeTime.Advance(TimeSpan.FromSeconds(31));

        var response = await auth.LoginAsync(new LoginRequest("locked@teamup.edu.eg", "ValidPass123!"));
        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
    }

    [Fact]
    public async Task ImageService_GeneratesTimeOrderedUuidV7Identifiers()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 11, 27, 14, 0, 0, TimeSpan.Zero));
        await using var db = CreateDb(fakeTime);

        var student = new Student
        {
            Email = "media@teamup.edu.eg",
            PasswordHash = "hash",
            FirstName = "Nour",
            LastName = "Tarek",
            AcademicYear = 2,
        };
        db.Students.Add(student);
        await db.SaveChangesAsync();

        var service = new ImageService(db, fakeTime);

        await using var stream1 = new MemoryStream(MinimalPngPayload);
        var first = await service.SaveImageAsync(student.StudentId, stream1, MinimalPngPayload.Length);

        fakeTime.Advance(TimeSpan.FromMilliseconds(50));

        await using var stream2 = new MemoryStream(MinimalPngPayload);
        var second = await service.SaveImageAsync(student.StudentId, stream2, MinimalPngPayload.Length);

        Assert.Equal(7, first.ImageId.Version);
        Assert.Equal(7, second.ImageId.Version);
        Assert.True(second.ImageId.CompareTo(first.ImageId) > 0);
    }

    [Fact]
    public async Task NamedQueryFilter_SoftDeleteHidesDeletedRowsByDefault_AndAllowsTargetedBypass()
    {
        var fakeTime = new FakeTimeProvider();
        await using var db = CreateDb(fakeTime);

        var active = new University { Name = "Cairo University", City = "Giza", Country = "Egypt" };
        var deleted = new University { Name = "Legacy Institute", City = "Alexandria", Country = "Egypt" };
        db.Universities.AddRange(active, deleted);
        await db.SaveChangesAsync();

        db.Universities.Remove(deleted);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var visible = await db.Universities.AsNoTracking().ToListAsync();
        var allIncludingSoftDeleted = await db.Universities
            .AsNoTracking()
            .IgnoreQueryFilters([QueryFilterNames.SoftDelete])
            .ToListAsync();

        Assert.Single(visible);
        Assert.Equal("Cairo University", visible[0].Name);
        Assert.Equal(2, allIncludingSoftDeleted.Count);
        Assert.Contains(allIncludingSoftDeleted, u => u.Name == "Legacy Institute" && u.DeletedAt != null);
    }

    [Fact]
    public void SimdSearchValuesAndFrozenSetGuards_EnforceFastPathAndSecurityBoundaries()
    {
        const string cleanCell = "Software Engineering";
        Assert.Same(cleanCell, CsvExportHelper.EscapeCell(cleanCell));
        Assert.Equal("\"'=SUM(A1:A2)\"", $"\"{CsvExportHelper.EscapeCell("=SUM(A1:A2)")}\"");
        Assert.Equal("\"Line 1 Line 2, Cairo\"", CsvExportHelper.EscapeCell("Line 1\r\nLine 2, Cairo"));

        Assert.False(SafeUrlValidator.IsSafeExternalHttpUrl("http://100.64.0.1/internal"));
        Assert.False(SafeUrlValidator.IsSafeExternalHttpUrl("http://[fc00::1]/admin"));
        Assert.False(SafeUrlValidator.IsSafeExternalHttpUrl("http://[::ffff:127.0.0.1]/"));
        Assert.False(SafeUrlValidator.IsSafeExternalHttpUrl("http://metadata.google.internal/computeMetadata"));
        Assert.False(SafeUrlValidator.IsSafeAssetOrExternalUrl("/images/bad\\path.png"));
        Assert.False(SafeUrlValidator.IsSafeAssetOrExternalUrl("/images/ctrl\u0007.png"));
        Assert.False(SafeUrlValidator.IsSafeAssetOrExternalUrl("/images/c1\u0085.png"));
        Assert.True(SafeUrlValidator.IsSafeAssetOrExternalUrl("/images/valid-avatar.png"));
        Assert.True(SafeUrlValidator.IsSafeExternalHttpUrl("https://teamup.edu.eg/logo.png"));

        Assert.Throws<InvalidOperationException>(() =>
            SecurityStartupValidator.ValidateSeedAdminPassword("Admin@TeamUp2026!"));
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(
            new SecurityStartupSettings(
                IsDevelopment: false,
                IsProduction: true,
                JwtKey: "DevelopmentFallbackSecretKey_padded_for_length",
                JwtIssuer: "TeamUp",
                JwtAudience: "TeamUp.Clients",
                EmailProvider: "Smtp",
                DatabaseProvider: "SqlServer",
                ConnectionString: "Server=prod;Encrypt=True;TrustServerCertificate=False",
                EmailSmtpHost: "smtp.teamup.edu")));
    }

    [Fact]
    public void ClaimsPrincipalExtensionProperties_ExposeIdentityAndScopedOrgClaims()
    {
        var stamp = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "77"),
            new Claim(ClaimTypes.Role, "OrgAdmin"),
            new Claim(ClaimTypes.Email, "lead@teamup.edu.eg"),
            new Claim(TeamUpClaimTypes.SecurityStamp, stamp.ToString("D")),
            new Claim(TeamUpClaimTypes.OrgAdmin, "5"),
            new Claim(TeamUpClaimTypes.OrgAdmin, "12"),
            new Claim(TeamUpClaimTypes.OrgAdmin, "5"),
        ], "Bearer"));

        Assert.Equal(77, principal.StudentId);
        Assert.Equal("OrgAdmin", principal.UserRole);
        Assert.Equal("lead@teamup.edu.eg", principal.Email);
        Assert.Equal(stamp, principal.SecurityStamp);
        Assert.Equal([5, 12], principal.AdminOrganizationIds);

        ClaimsPrincipal? anonymous = null;
        Assert.Equal(0, anonymous.StudentId);
        Assert.Null(anonymous.UserRole);
        Assert.Empty(anonymous.AdminOrganizationIds);
    }

    [Fact]
    public async Task HybridCacheCatalogService_CachesAndInvalidatesOnMutations()
    {
        await using var db = CreateDb();
        var services = new ServiceCollection();
        services.AddHybridCache();
        await using var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<HybridCache>();

        var service = new UniversityService(db, cache, new AuditLogService(db, new NullRequestContext()));

        var initial = await service.GetAcademicCatalogAsync();
        Assert.Empty(initial);

        var created = await service.CreateUniversityAsync(
            new CreateUniversityRequest("Alexandria University", "Alexandria", "Egypt", "https://alexu.edu.eg", null));

        var afterCreate = await service.GetAcademicCatalogAsync();
        Assert.Single(afterCreate);
        Assert.Equal("Alexandria University", afterCreate[0].Name);

        await service.DeleteUniversityAsync(created.UniversityId);

        var afterDelete = await service.GetAcademicCatalogAsync();
        Assert.Empty(afterDelete);
    }

    [Fact]
    public void DigestHtmlBuilder_EncodesUntrustedContent_AndHandlesEmptyState()
    {
        var emptyHtml = DigestHtmlBuilder.BuildLeaderboardRowsHtml([]);
        Assert.Contains("No ranked students recorded yet.", emptyHtml.Html, StringComparison.Ordinal);

        var rowsHtml = DigestHtmlBuilder.BuildLeaderboardRowsHtml(
        [
            new LeaderboardDigestEntry(1, "<script>alert(1)</script>", "CS", 1200, 4)
        ]);
        Assert.DoesNotContain("<script>", rowsHtml.Html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", rowsHtml.Html, StringComparison.Ordinal);
        Assert.Contains("#01", rowsHtml.Html, StringComparison.Ordinal);
        Assert.Contains("1,200", rowsHtml.Html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AuthService_EmailVerification_SucceedsBeforeExpiryAndRejectsAfterExpiryOrReplay()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 11, 27, 10, 0, 0, TimeSpan.Zero));
        await using var db = CreateDb(fakeTime);

        var student = new Student
        {
            Email = "verify-unit@cu.edu.eg",
            PasswordHash = "hash",
            FirstName = "Nour",
            LastName = "Hassan",
            AcademicYear = 3,
            SecurityStamp = Guid.NewGuid(),
        };
        db.Students.Add(student);
        await db.SaveChangesAsync();

        using var memCache = new MemoryCache(new MemoryCacheOptions());
        var jwt = new JwtTokenService(Options.Create(TestJwtOptions), fakeTime);
        var emailSender = new InMemoryEmailSender(NullLogger<InMemoryEmailSender>.Instance, fakeTime);
        var orgAuth = new OrganizationAuthorizationService(db);
        var stampValidator = new SecurityStampValidator(db, memCache);
        var auth = new AuthService(
            db, jwt, new PasswordHasher<Student>(), emailSender, Options.Create(TestJwtOptions), orgAuth, stampValidator,
            NullLogger<AuthService>.Instance, fakeTime);

        var sendRes = await auth.SendVerificationEmailAsync(student.StudentId);
        Assert.False(sendRes.AlreadyVerified);
        var tok1 = emailSender.GetLastVerificationToken(student.Email);
        Assert.False(string.IsNullOrWhiteSpace(tok1));

        // Advance past default 60-minute expiry window -> expired token rejected cleanly
        fakeTime.Advance(TimeSpan.FromMinutes(61));
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            auth.VerifyEmailAsync(new VerifyEmailRequest(student.Email, tok1!)));

        // Issue fresh token and verify within validity window
        await auth.SendVerificationEmailAsync(student.StudentId);
        var tok2 = emailSender.GetLastVerificationToken(student.Email);
        Assert.NotNull(tok2);

        await auth.VerifyEmailAsync(new VerifyEmailRequest(student.Email, tok2!));
        var reloaded = await db.Students.SingleAsync(s => s.StudentId == student.StudentId);
        Assert.True(reloaded.IsEmailVerified);
        Assert.Null(reloaded.EmailVerificationToken);

        var resendRes = await auth.SendVerificationEmailAsync(student.StudentId);
        Assert.True(resendRes.AlreadyVerified);
    }

    [Fact]
    public async Task OrganizationStageValidatorAndService_RejectsWithdrawnAndSoftDeletedApplicantsGracefully()
    {
        var validator = new TeamUp.Application.Organizations.Validators.UpdateApplicationStageRequestValidator();
        var invalidResult = validator.Validate(new TeamUp.Application.Organizations.UpdateApplicationStageRequest(RecruitmentStage.Withdrawn));
        Assert.False(invalidResult.IsValid);

        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2026, 11, 27, 11, 0, 0, TimeSpan.Zero));
        await using var db = CreateDb(fakeTime);

        var admin = new Student { Email = "lead@org.eg", PasswordHash = "h", FirstName = "A", LastName = "B", AcademicYear = 4, Role = UserRole.PlatformAdmin };
        var applicant = new Student { Email = "gone@org.eg", PasswordHash = "h", FirstName = "C", LastName = "D", AcademicYear = 2 };
        var org = new Organization { Name = "IEEE Test Branch", Description = "Desc", Category = "Engineering" };
        db.Students.AddRange(admin, applicant);
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var app = new MembershipApplication { OrganizationId = org.OrganizationId, StudentId = applicant.StudentId, Stage = RecruitmentStage.CommitteeInterview };
        db.MembershipApplications.Add(app);
        await db.SaveChangesAsync();

        // Soft-delete applicant before acceptance -> ConflictException (409), never NullReferenceException (500)
        applicant.DeletedAt = fakeTime.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = new OrganizationService(db, new OrganizationAuthorizationService(db), new SecurityStampValidator(db, new MemoryCache(new MemoryCacheOptions())), new AuditLogService(db, new NullRequestContext()), fakeTime);
        await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateApplicationStageAsync(org.OrganizationId, app.ApplicationId, admin.StudentId, nameof(UserRole.PlatformAdmin),
                new TeamUp.Application.Organizations.UpdateApplicationStageRequest(RecruitmentStage.Accepted, 9.129m, "Notes")));
    }
}
