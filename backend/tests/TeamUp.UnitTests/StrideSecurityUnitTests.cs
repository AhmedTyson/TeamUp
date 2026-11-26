// Date: 2026-11-21
// Purpose: Unit tests verifying STRIDE security controls (startup secret/TLS guards, rotating HMAC attendance codes, SSRF/XSS URL validator, AuditLog immutability, directory page cap, JWT claims, and templates).

using System.IdentityModel.Tokens.Jwt;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Common.Validation;
using TeamUp.Application.Gamification;
using TeamUp.Application.Gamification.Validators;
using TeamUp.Application.Organizations;
using TeamUp.Application.Organizations.Validators;
using TeamUp.Application.Students;
using TeamUp.Application.Students.Validators;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Services;

namespace TeamUp.UnitTests;

public class StrideSecurityUnitTests
{
    [Fact]
    public void SecurityStartupValidator_Production_MissingOrFallbackJwtKey_Throws()
    {
        var missingKey = new SecurityStartupSettings(
            IsDevelopment: false,
            IsProduction: true,
            JwtKey: "",
            JwtIssuer: "TeamUp",
            JwtAudience: "TeamUp.Clients",
            EmailProvider: "Smtp",
            DatabaseProvider: "SqlServer",
            ConnectionString: "Server=db;Database=TeamUp;Encrypt=True;TrustServerCertificate=False;");

        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(missingKey));

        var fallbackKey = missingKey with { JwtKey = "DevelopmentFallbackSecretKeyForTeamUpAppMustBeAtLeast32BytesLong!" };
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(fallbackKey));

        var shortKey = missingKey with { JwtKey = "TooShortKey123!" };
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(shortKey));

        var devMarkerKey = missingKey with { JwtKey = "DevelopmentOnlyLocalSigningKey-12345678901234567890" };
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(devMarkerKey));

        var prodMarkerKey = missingKey with { JwtKey = "MustBeOverriddenInProd-12345678901234567890" };
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(prodMarkerKey));
    }

    [Fact]
    public void SecurityStartupValidator_Production_InMemoryEmailOrTrustServerCert_Throws()
    {
        var strongKey = new string('K', 64);

        var inMemoryProd = new SecurityStartupSettings(
            IsDevelopment: false,
            IsProduction: true,
            JwtKey: strongKey,
            JwtIssuer: "TeamUp",
            JwtAudience: "TeamUp.Clients",
            EmailProvider: "InMemory",
            DatabaseProvider: "SqlServer",
            ConnectionString: "Server=db;Database=TeamUp;Encrypt=True;TrustServerCertificate=False;");

        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(inMemoryProd));

        var insecureSql = inMemoryProd with
        {
            EmailProvider = "Smtp",
            ConnectionString = "Server=db;Database=TeamUp;User Id=sa;Password=Strong!;TrustServerCertificate=True;"
        };
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(insecureSql));

        var encryptFalseSql = insecureSql with
        {
            ConnectionString = "Server=db;Database=TeamUp;User Id=sa;Password=Strong!;Encrypt=False;"
        };
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(encryptFalseSql));

        var secureProd = insecureSql with
        {
            ConnectionString = "Server=db;Database=TeamUp;User Id=sa;Password=Strong!;Encrypt=True;TrustServerCertificate=False;",
            EmailSmtpHost = "smtp.teamup.edu"
        };
        var ex = Record.Exception(() => SecurityStartupValidator.Validate(secureProd));
        Assert.Null(ex);

        var missingSmtpHost = secureProd with { EmailSmtpHost = "" };
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(missingSmtpHost));

        var sqliteProd = secureProd with { DatabaseProvider = "Sqlite" };
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(sqliteProd));
    }

    [Theory]
    [InlineData("https://example.com/logo.png", true)]
    [InlineData("http://cdn.teamup.edu/avatar.jpg", true)]
    [InlineData("https://img.local/photo.png", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("data:text/html,<script>alert(1)</script>", false)]
    [InlineData("file:///etc/passwd", false)]
    [InlineData("ftp://example.com/file.zip", false)]
    [InlineData("http://169.254.169.254/latest/meta-data/", false)]
    [InlineData("http://127.0.0.1:8080/internal", false)]
    [InlineData("http://localhost/admin", false)]
    [InlineData("http://10.0.0.5/secret", false)]
    [InlineData("http://172.16.0.10/secret", false)]
    [InlineData("http://192.168.1.1/router", false)]
    [InlineData("http://[::1]/", false)]
    [InlineData("http://[::ffff:169.254.169.254]/", false)]
    [InlineData("https://user:pass@example.com/secret", false)]
    [InlineData("http://localhost./admin", false)]
    [InlineData("http://metadata.google.internal./", false)]
    public void SafeUrlValidator_ExternalHttpUrl_BlocksDangerousSchemesAndSsrf(string url, bool expected)
    {
        Assert.Equal(expected, SafeUrlValidator.IsSafeExternalHttpUrl(url));
    }

    [Theory]
    [InlineData("/badges/first-steps.png", true)]
    [InlineData("/api/v1/images/c0e70000-0000-4000-8000-000000000001", true)]
    [InlineData("/images/people/student-1-abc.png", true)]
    [InlineData("/logos/teamup-logo.png", true)]
    [InlineData("https://cdn.teamup.edu/badge.png", true)]
    [InlineData("//evil.example.com/badge.png", false)]
    [InlineData("/\\evil.example.com/badge.png", false)]
    [InlineData("/badges/../secret.png", false)]
    [InlineData("/api/v1/admin/dashboard", false)]
    [InlineData("/api/v1/images/not-a-guid", false)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("http://169.254.169.254/badge.png", false)]
    public void SafeUrlValidator_AssetOrExternalUrl_ValidatesBadgeIcons(string url, bool expected)
    {
        Assert.Equal(expected, SafeUrlValidator.IsSafeAssetOrExternalUrl(url));
    }

    [Fact]
    public void Validators_RejectSsrfAndXssUrls()
    {
        var orgValidator = new CreateOrganizationRequestValidator();
        Assert.False(orgValidator.Validate(new CreateOrganizationRequest("Org", "Desc", "Tech", "http://169.254.169.254/meta")).IsValid);
        Assert.False(orgValidator.Validate(new CreateOrganizationRequest("Org", "Desc", "Tech", "javascript:alert(1)")).IsValid);
        Assert.False(orgValidator.Validate(new CreateOrganizationRequest("Org", "Desc", "Tech", "/api/v1/admin")).IsValid);
        Assert.True(orgValidator.Validate(new CreateOrganizationRequest("Org", "Desc", "Tech", "/api/v1/images/c0e70000-0000-4000-8000-000000000001")).IsValid);

        var updateOrgValidator = new UpdateOrganizationRequestValidator();
        Assert.True(updateOrgValidator.Validate(new UpdateOrganizationRequest("Org", "Desc", "Tech", "/api/v1/images/c0e70000-0000-4000-8000-000000000001")).IsValid);
        Assert.False(updateOrgValidator.Validate(new UpdateOrganizationRequest("Org", "Desc", "Tech", "/api/v1/admin")).IsValid);

        var proposalValidator = new CreateOrganizationProposalRequestValidator();
        Assert.True(proposalValidator.Validate(new CreateOrganizationProposalRequest("Org", "Desc", "Tech", "/api/v1/images/c0e70000-0000-4000-8000-000000000001")).IsValid);

        var badgeValidator = new CreateBadgeRequestValidator();
        Assert.False(badgeValidator.Validate(new CreateBadgeRequest("Badge", "Desc", "javascript:alert(1)", BadgeCriteriaType.PointsThreshold, 10)).IsValid);
        Assert.True(badgeValidator.Validate(new CreateBadgeRequest("Badge", "Desc", "/badges/first.png", BadgeCriteriaType.PointsThreshold, 10)).IsValid);

        var updateBadgeValidator = new UpdateBadgeRequestValidator();
        Assert.False(updateBadgeValidator.Validate(new UpdateBadgeRequest(null, null, "http://127.0.0.1/icon.png", null, null)).IsValid);

        var profileValidator = new UpsertProfileRequestValidator();
        Assert.False(profileValidator.Validate(new UpsertProfileRequest("Bio", "http://192.168.1.5/pic.png", null, null)).IsValid);
    }

    [Fact]
    public void ImageFileValidator_RequiresMinimum12BytesAndValidSignature()
    {
        byte[] truncatedJpeg = [0xFF, 0xD8, 0xFF, 0xE0];
        Assert.False(ImageFileValidator.TryValidateImagePayload(truncatedJpeg, out _, out _));

        byte[] paddedJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];
        Assert.True(ImageFileValidator.TryValidateImagePayload(paddedJpeg, out var ext, out var contentType));
        Assert.Equal(".jpg", ext);
        Assert.Equal("image/jpeg", contentType);
    }

    [Fact]
    public void AttendanceCodeService_ValidatesWithin60SecondWindowAndRejectsExpiredOrTampered()
    {
        var opts = Options.Create(new JwtOptions { Key = "UnitTestAttendanceSigningKeyMustBeAtLeast32BytesLong!" });
        var svc = new AttendanceCodeService(opts);
        var t0 = new DateTime(2026, 11, 21, 12, 0, 10, DateTimeKind.Utc);

        var (code, expiresAt) = svc.GenerateCode(sessionId: 42, utcNow: t0);
        Assert.Equal(new DateTime(2026, 11, 21, 12, 1, 0, DateTimeKind.Utc), expiresAt);
        Assert.True(svc.ValidateCode(42, code, t0.AddSeconds(30)));
        Assert.False(svc.ValidateCode(42, code, t0.AddSeconds(60)));
        Assert.False(svc.ValidateCode(99, code, t0));
        Assert.False(svc.ValidateCode(42, code + "tampered", t0));
    }

    [Fact]
    public void JwtOptions_DefaultsAndClaims_EnforceShortLifetimeAndSecurityStamp()
    {
        var defaults = new JwtOptions();
        Assert.Equal(15, defaults.AccessTokenMinutes);
        Assert.Equal(15, defaults.PasswordResetTokenMinutes);

        var stamp = Guid.NewGuid();
        var jwtSvc = new JwtTokenService(Options.Create(new JwtOptions
        {
            Key = "UnitTestJwtSigningKeyForClaimsVerification32BytesLong!",
            Issuer = "TeamUp",
            Audience = "TeamUp.Clients",
            AccessTokenMinutes = 15
        }));

        var (token, _) = jwtSvc.GenerateAccessToken(7, "s@teamup.edu", "Student", "Sara Ali", stamp, [14, 22]);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(stamp.ToString("D"), parsed.Claims.First(c => c.Type == TeamUpClaimTypes.SecurityStamp).Value);
        var orgClaims = parsed.Claims.Where(c => c.Type == TeamUpClaimTypes.OrgAdmin).Select(c => c.Value).ToList();
        Assert.Contains("14", orgClaims);
        Assert.Contains("22", orgClaims);
    }

    [Fact]
    public void StudentDirectoryQuery_ClampsPageSizeToMaximum20()
    {
        var query = new StudentDirectoryQuery(Page: 1, PageSize: 100);
        var page = query.ToPageRequest();
        Assert.Equal(20, page.SafePageSize);
    }

    [Fact]
    public void AuditLog_IsAppendOnly_BlocksUpdateAndDelete()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .AddInterceptors(new SoftDeleteInterceptor())
            .Options;

        using var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        var log = new AuditLog
        {
            Action = "RoleChanged",
            EntityType = "Student",
            EntityId = "1",
            OldValues = "{\"role\":\"Student\"}",
            NewValues = "{\"role\":\"OrgAdmin\"}"
        };
        db.AuditLogs.Add(log);
        db.SaveChanges();

        log.Action = "Tampered";
        Assert.Throws<InvalidOperationException>(() => db.SaveChanges());

        db.Entry(log).State = EntityState.Unchanged;
        db.AuditLogs.Remove(log);
        Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
    }

    [Fact]
    public async Task TemplateRenderer_RendersEmailAndReportTemplatesSafely()
    {
        var renderer = new TemplateRenderer();

        var emailHtml = await renderer.RenderEmailAsync("PasswordReset.html", new Dictionary<string, string>
        {
            ["Email"] = "<script>alert(1)</script>@teamup.edu",
            ["ResetUrl"] = "https://teamup.edu/reset?token=abc",
            ["ResetToken"] = "tok_123",
            ["ExpiresMinutes"] = "15"
        });

        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", emailHtml);
        Assert.DoesNotContain("<script>alert(1)</script>", emailHtml);
        Assert.Contains("15 minutes", emailHtml);

        var reportHtml = await renderer.RenderReportAsync("SessionAttendanceReport.html", new Dictionary<string, string>
        {
            ["OrganizationName"] = "IEEE",
            ["EventTitle"] = "AI Summit",
            ["SessionTitle"] = "#1 — Opening",
            ["Room"] = "Hall A",
            ["SessionWindow"] = "2026-11-22 10:00 – 11:00 UTC",
            ["CheckedInCount"] = "1",
            ["ConfirmedCount"] = "2",
            ["GeneratedAtUtc"] = "2026-11-22 12:00:00 UTC",
            ["AttendanceRows"] = "<tr><td>1</td><td>Ada</td><td>QRCode</td><td>now</td><td>1</td></tr>"
        });

        Assert.Contains("AI Summit", reportHtml);
        Assert.Contains("IEEE", reportHtml);
        Assert.Contains("Ada", reportHtml);
        Assert.Contains("<svg", reportHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TeamUp", reportHtml);
    }

    [Fact]
    public async Task TemplateRenderer_RenderReportAsync_PreventsSecondOrderPlaceholderInjection()
    {
        var renderer = new TemplateRenderer();
        var attendanceRow = "<tr><td>1</td><td>Ada</td><td>QRCode</td><td>now</td><td>1</td></tr>";

        var reportHtml = await renderer.RenderReportAsync("SessionAttendanceReport.html", new Dictionary<string, string>
        {
            ["OrganizationName"] = "IEEE",
            ["EventTitle"] = "{{AttendanceRows}}",
            ["SessionTitle"] = "#1",
            ["Room"] = "Hall A",
            ["SessionWindow"] = "2026-11-22 10:00 UTC",
            ["CheckedInCount"] = "1",
            ["ConfirmedCount"] = "1",
            ["GeneratedAtUtc"] = "2026-11-22 12:00:00 UTC",
            ["AttendanceRows"] = attendanceRow
        });

        Assert.Contains("{{AttendanceRows}}", reportHtml);
        var occurrences = reportHtml.Split(attendanceRow, StringSplitOptions.None).Length - 1;
        Assert.Equal(1, occurrences);
    }

    [Fact]
    public void SecurityStartupValidator_RejectsSpacedTrustServerCertificateSynonymInProduction()
    {
        var spacedTrust = new SecurityStartupSettings(
            IsDevelopment: false,
            IsProduction: true,
            JwtKey: new string('K', 64),
            JwtIssuer: "TeamUp",
            JwtAudience: "TeamUp.Clients",
            EmailProvider: "Smtp",
            EmailSmtpHost: "smtp.teamup.edu",
            DatabaseProvider: "SqlServer",
            ConnectionString: "Server=prod.db;Database=TeamUp;Encrypt=True;Trust Server Certificate=True;");

        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(spacedTrust));

        var encryptNo = spacedTrust with
        {
            ConnectionString = "Server=prod.db;Database=TeamUp;Encrypt=no;TrustServerCertificate=False;"
        };
        Assert.Throws<InvalidOperationException>(() => SecurityStartupValidator.Validate(encryptNo));
    }
}
