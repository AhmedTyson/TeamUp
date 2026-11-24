// Date: 2026-11-24
// Purpose: Integration tests verifying PlatformAdmin-only PDF governance reports, template previews, and digest dispatch endpoints.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Admin;
using TeamUp.Application.Gamification;
using TeamUp.Infrastructure.Services;

namespace TeamUp.IntegrationTests;

public class AdminReportsAndCommsTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private HttpClient AdminClient()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateAdminToken());
        return client;
    }

    [Fact]
    public async Task AdminReportsAndComms_ForbidStudentAndOrgAdmin()
    {
        var student = factory.CreateClient();
        student.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateStudentToken(8101));

        var orgAdmin = factory.CreateClient();
        orgAdmin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateToken(8102, "org_reports@teamup.test", "OrgAdmin"));

        foreach (var client in new[] { student, orgAdmin })
        {
            Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/admin/reports/platform-summary.pdf")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/v1/admin/reports/send-summary", new SendAdminReportRequest(), ApiHelper.Json)).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/admin/templates")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/admin/templates/preview/PasswordReset.html")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/v1/admin/communications/leaderboard-digest", new AdminLeaderboardDigestCommand(), ApiHelper.Json)).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/v1/admin/communications/project-matches", new AdminProjectMatchDigestCommand(), ApiHelper.Json)).StatusCode);
        }
    }

    [Fact]
    public async Task PlatformAdmin_CanDownloadValidGovernancePdf_AndEmailItWithAttachment()
    {
        var admin = AdminClient();

        var pdfRes = await admin.GetAsync("/api/v1/admin/reports/platform-summary.pdf?reportPeriod=QA%20GOVERNANCE%20LEDGER");
        Assert.Equal(HttpStatusCode.OK, pdfRes.StatusCode);
        Assert.Equal("application/pdf", pdfRes.Content.Headers.ContentType?.MediaType);

        var bytes = await pdfRes.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 2000, $"Expected multi-section PDF, got {bytes.Length} bytes");
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));

        var recipient = $"exec_{Guid.NewGuid():N}@teamup.test";
        var emailRes = await admin.PostAsJsonAsync(
            "/api/v1/admin/reports/send-summary",
            new SendAdminReportRequest(recipient, "Q4 GOVERNANCE LEDGER"),
            ApiHelper.Json);
        Assert.Equal(HttpStatusCode.OK, emailRes.StatusCode);

        var result = await ApiHelper.DataAsync<AdminReportEmailResultDto>(emailRes);
        Assert.Equal(recipient, result.RecipientEmail);
        Assert.False(string.IsNullOrWhiteSpace(result.FileName));
        Assert.True(result.PdfSizeBytes > 2000);

        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<InMemoryEmailSender>();
        var sent = Assert.Single(sender.GetSentMessages(recipient));
        var attachment = Assert.Single(sent.Attachments);
        Assert.Equal("application/pdf", attachment.ContentType);
        Assert.Equal(result.PdfSizeBytes, attachment.SizeBytes);
    }

    [Fact]
    public async Task PlatformAdmin_CanListAndPreviewAllSevenEmailTemplates()
    {
        var admin = AdminClient();

        var listRes = await admin.GetAsync("/api/v1/admin/templates");
        Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);
        var templates = await ApiHelper.DataAsync<List<string>>(listRes);
        Assert.Equal(7, templates.Count);

        foreach (var template in templates)
        {
            var previewRes = await admin.GetAsync($"/api/v1/admin/templates/preview/{Uri.EscapeDataString(template)}");
            Assert.Equal(HttpStatusCode.OK, previewRes.StatusCode);
            Assert.Equal("text/html", previewRes.Content.Headers.ContentType?.MediaType);

            var html = await previewRes.Content.ReadAsStringAsync();
            Assert.Contains("#14151C", html);
            Assert.Contains("#D3FB52", html);
            Assert.Contains("data:image/png;base64,", html);
            Assert.Contains("alt=\"TeamUp\"", html);
            Assert.DoesNotContain("{{", html);
            Assert.DoesNotContain("}}", html);
        }

        var missingRes = await admin.GetAsync("/api/v1/admin/templates/preview/NonExistentTemplate.html");
        Assert.Equal(HttpStatusCode.NotFound, missingRes.StatusCode);
    }

    [Fact]
    public async Task PlatformAdmin_CanTriggerLeaderboardAndProjectMatchDigestsInTestMode()
    {
        var admin = AdminClient();
        var api = new ApiHelper(factory);
        var (leadClient, _, _, _) = await api.RegisterAsync("digest_seed_lead");
        await api.CreateProjectAsync(leadClient, capacity: 4);

        var lbRes = await admin.PostAsJsonAsync(
            "/api/v1/admin/communications/leaderboard-digest",
            new AdminLeaderboardDigestCommand(
                Mode: "TestToMe",
                PeriodLabel: "WEEKLY TEST ISSUE",
                NewsItems: [new CampusNewsItem("CAMPUS", "Hackathon Finals", "Teams present Friday.")],
                DryRun: true),
            ApiHelper.Json);
        Assert.Equal(HttpStatusCode.OK, lbRes.StatusCode);
        var lbDto = await ApiHelper.DataAsync<DigestDispatchResultDto>(lbRes);
        Assert.True(lbDto.IsTestMode);
        Assert.True(lbDto.DryRun);
        Assert.Contains("Hackathon Finals", lbDto.PreviewHtml);

        var matchRes = await admin.PostAsJsonAsync(
            "/api/v1/admin/communications/project-matches",
            new AdminProjectMatchDigestCommand(
                Mode: "TestToMe",
                MaxMatchesPerStudent: 3,
                MinMatchPercentage: 0,
                DryRun: true),
            ApiHelper.Json);
        Assert.Equal(HttpStatusCode.OK, matchRes.StatusCode);
        var matchDto = await ApiHelper.DataAsync<DigestDispatchResultDto>(matchRes);
        Assert.True(matchDto.IsTestMode);
        Assert.True(matchDto.DryRun);
    }
}
