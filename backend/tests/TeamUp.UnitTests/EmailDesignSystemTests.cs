// Date: 2026-11-24
// Purpose: Unit tests for the Obsidian/Lime Editorial email template system, SafeHtmlFragment single-pass rendering, and PDF attachment dispatch.

using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Infrastructure.Services;
using TeamUp.Infrastructure.Templates;

namespace TeamUp.UnitTests;

public class EmailDesignSystemTests
{
    [Fact]
    public void GetAvailableEmailTemplates_ReturnsAllSevenEditorialTemplates()
    {
        var renderer = new TemplateRenderer();
        var templates = renderer.GetAvailableEmailTemplates();

        var expected = new[]
        {
            "AdminReportEmail.html",
            "EmailVerification.html",
            "EventNotification.html",
            "GeneralNotification.html",
            "LeaderboardDigest.html",
            "PasswordReset.html",
            "ProjectMatchDigest.html"
        };

        Assert.Equal(expected, templates);
    }

    [Fact]
    public async Task RenderEmailAsync_SinglePassPreventsCrossExpansionAndEscapesScalars()
    {
        var renderer = new TemplateRenderer();

        var html = await renderer.RenderEmailAsync(
            "LeaderboardDigest.html",
            new Dictionary<string, string>
            {
                ["IssueNumber"] = "ISSUE 042",
                ["PeriodLabel"] = "WEEKLY SNAPSHOT",
                ["RecipientName"] = "<script>alert(1)</script> {{{LeaderboardRowsHtml}}}",
                ["MyRank"] = "4",
                ["MyXp"] = "1840",
                ["MyLevel"] = "7",
                ["LeaderboardUrl"] = "https://teamup-ieee.vercel.app/leaderboard",
                ["PreferencesUrl"] = "https://teamup-ieee.vercel.app/profile/me"
            },
            new Dictionary<string, SafeHtmlFragment>
            {
                ["LeaderboardRowsHtml"] = new("<tr><td>#1</td><td>{{RecipientName}}</td><td>FCAI</td><td>8</td><td>2840</td></tr>"),
                ["NewsItemsHtml"] = new("<div style=\"border:1px solid #E2E6E0;padding:12px;\">Spring Hackathon opens Monday.</div>")
            });

        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html);
        Assert.DoesNotContain("<script>alert(1)</script>", html);
        Assert.Contains("{{{LeaderboardRowsHtml}}}", html);
        Assert.Contains("<td>{{RecipientName}}</td>", html);
    }

    [Fact]
    public async Task AllSevenTemplates_RenderCleanlyWithZeroUnresolvedTokensAndZeroAiSlop()
    {
        var renderer = new TemplateRenderer();

        var renderedPages = new List<string>
        {
            await renderer.RenderEmailAsync("PasswordReset.html", new Dictionary<string, string>
            {
                ["Email"] = "salma@cu.edu.eg",
                ["ResetUrl"] = "https://teamup-ieee.vercel.app/auth/reset-password?token=abc",
                ["ResetToken"] = "tok_reset_987",
                ["ExpiresMinutes"] = "15"
            }),
            await renderer.RenderEmailAsync("EmailVerification.html", new Dictionary<string, string>
            {
                ["FirstName"] = "Salma",
                ["Email"] = "salma@cu.edu.eg",
                ["UniversityName"] = "Cairo University",
                ["FacultyName"] = "Faculty of Computers and Artificial Intelligence",
                ["VerifyUrl"] = "https://teamup-ieee.vercel.app/auth/verify?code=491028",
                ["VerificationCode"] = "491-028",
                ["ExpiresMinutes"] = "15"
            }),
            await renderer.RenderEmailAsync("EventNotification.html", new Dictionary<string, string>
            {
                ["RecipientName"] = "Omar Mostafa",
                ["StatusLabel"] = "CONFIRMED SEAT",
                ["EventMonth"] = "DEC",
                ["EventDay"] = "14",
                ["EventTitle"] = "IEEE Cairo Embedded & Robotics Summit",
                ["OrganizationName"] = "IEEE Cairo University Student Branch",
                ["EventDateRange"] = "2026-12-14 10:00 - 15:00 UTC",
                ["VenueLocation"] = "FCAI Main Auditorium, Giza",
                ["SummaryText"] = "Bring your student ID for rotating QR check-in at Hall A.",
                ["EventUrl"] = "https://teamup-ieee.vercel.app/events/12",
                ["PreferencesUrl"] = "https://teamup-ieee.vercel.app/profile/me"
            }),
            await renderer.RenderEmailAsync("GeneralNotification.html", new Dictionary<string, string>
            {
                ["RecipientName"] = "Hana El-Sayed",
                ["CategoryLabel"] = "APPLICATION REVIEWED",
                ["NotificationTitle"] = "You joined Autonomous Rover Telemetry",
                ["SummaryMessage"] = "Your squad application was accepted by the team lead.",
                ["ContextLabel"] = "PROJECT",
                ["ContextValue"] = "Autonomous Rover Telemetry",
                ["TimestampUtc"] = "2026-11-24 03:30 UTC",
                ["ActionLabel"] = "OPEN PROJECT WORKSPACE",
                ["ActionUrl"] = "https://teamup-ieee.vercel.app/projects/7",
                ["PreferencesUrl"] = "https://teamup-ieee.vercel.app/profile/me"
            }),
            await renderer.RenderEmailAsync(
                "LeaderboardDigest.html",
                new Dictionary<string, string>
                {
                    ["IssueNumber"] = "ISSUE 014",
                    ["PeriodLabel"] = "WEEKLY CAMPUS DIGEST",
                    ["RecipientName"] = "Salma Khaled",
                    ["MyRank"] = "1",
                    ["MyXp"] = "2840",
                    ["MyLevel"] = "8",
                    ["LeaderboardUrl"] = "https://teamup-ieee.vercel.app/leaderboard",
                    ["PreferencesUrl"] = "https://teamup-ieee.vercel.app/profile/me"
                },
                new Dictionary<string, SafeHtmlFragment>
                {
                    ["LeaderboardRowsHtml"] = new("<tr><td>#1</td><td>Salma Khaled</td><td>FCAI</td><td>8</td><td>2840</td></tr>"),
                    ["NewsItemsHtml"] = new("<p>Demo Day submissions close Thursday at 23:59 UTC.</p>")
                }),
            await renderer.RenderEmailAsync(
                "ProjectMatchDigest.html",
                new Dictionary<string, string>
                {
                    ["RecipientName"] = "Karim Mansour",
                    ["MatchCount"] = "2",
                    ["StudentSkillsSummary"] = "React, TypeScript, UI/UX Design",
                    ["GeneratedDate"] = "2026-11-24",
                    ["ProjectsUrl"] = "https://teamup-ieee.vercel.app/projects",
                    ["PreferencesUrl"] = "https://teamup-ieee.vercel.app/profile/me"
                },
                new Dictionary<string, SafeHtmlFragment>
                {
                    ["MatchedProjectCardsHtml"] = new("<table role=\"presentation\" width=\"100%\"><tr><td>92% MATCH &mdash; Campus Transit Planner</td></tr></table>")
                }),
            await renderer.RenderEmailAsync("AdminReportEmail.html", new Dictionary<string, string>
            {
                ["AdminName"] = "Platform Admin",
                ["ReportPeriod"] = "MONTHLY GOVERNANCE LEDGER",
                ["GeneratedAtUtc"] = "2026-11-24 03:30 UTC",
                ["TotalStudents"] = "412",
                ["ActiveProjects"] = "28",
                ["ActiveEvents"] = "9",
                ["AuditEventsCount"] = "15",
                ["AttachmentFileName"] = "TeamUp_Governance_2026-11-24.pdf",
                ["AdminConsoleUrl"] = "https://teamup-ieee.vercel.app/admin/communications"
            })
        };

        var bannedTokens = new[] { "{{", "}}", "linear-gradient", "radial-gradient", "box-shadow", "🚀", "✨", "🎉", "🔥" };
        foreach (var html in renderedPages)
        {
            Assert.Contains("<img", html, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("alt=\"TeamUp\"", html, StringComparison.Ordinal);
            Assert.Contains("/images/brand/teamup-logo-email.png", html, StringComparison.Ordinal);
            Assert.Contains("#14151C", html);
            Assert.Contains("#D3FB52", html);
            Assert.Contains("#F3F5F2", html);
            foreach (var banned in bannedTokens)
            {
                Assert.DoesNotContain(banned, html, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public async Task BrandAssets_And_SmtpEmailSender_EmbedValidSvgPngAndCidLinkedLogo()
    {
        Assert.Contains("<svg", BrandAssets.LogoVectorSvg, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("#d3fb52", BrandAssets.LogoVectorSvg, StringComparison.OrdinalIgnoreCase);
        Assert.True(BrandAssets.EmailLogoPngBytes.Length > 1000);
        Assert.Equal([0x89, 0x50, 0x4E, 0x47], BrandAssets.EmailLogoPngBytes.Take(4).ToArray());

        var renderer = new TemplateRenderer();
        var html = await renderer.RenderEmailAsync("PasswordReset.html", new Dictionary<string, string>
        {
            ["Email"] = "salma@cu.edu.eg",
            ["ResetUrl"] = "https://teamup-ieee.vercel.app/auth/reset-password?token=abc",
            ["ResetToken"] = "tok_cid_check",
            ["ExpiresMinutes"] = "15"
        });

        using var mail = SmtpEmailSender.BuildMailMessage(
            "no-reply@teamup.edu",
            "TeamUp Platform",
            "salma@cu.edu.eg",
            "Reset your TeamUp password",
            html,
            "Plain fallback",
            attachments: null,
            frontendBaseUrl: "https://teamup-ieee.vercel.app");

        var htmlView = Assert.Single(mail.AlternateViews, v => v.ContentType?.MediaType == MediaTypeNames.Text.Html);
        using var reader = new StreamReader(htmlView.ContentStream);
        var mimeHtml = await reader.ReadToEndAsync();
        Assert.Contains("cid:teamup-logo", mimeHtml, StringComparison.Ordinal);

        var linkedLogo = Assert.Single(htmlView.LinkedResources);
        Assert.Equal(BrandAssets.EmailLogoContentId, linkedLogo.ContentId);
        Assert.Equal(MediaTypeNames.Image.Png, linkedLogo.ContentType?.MediaType);
    }

    [Fact]
    public async Task InMemoryEmailSender_RecordsTemplatedDispatchWithPdfAttachmentAndClearsCleanly()
    {
        var sender = new InMemoryEmailSender(NullLogger<InMemoryEmailSender>.Instance);
        var pdfBytes = Encoding.ASCII.GetBytes("%PDF-1.7\n1 0 obj\n<<>>\nendobj\ntrailer\n%%EOF");

        await sender.SendPasswordResetEmailAsync("admin@teamup.local", "secret_reset_tok");

        await sender.SendTemplatedEmailAsync(new EmailDispatchRequest(
            ToEmail: "admin@teamup.local",
            Subject: "TeamUp Governance Report — 2026-11-24",
            HtmlBody: "<html><body>Governance Summary</body></html>",
            PlainTextBody: "Governance Summary",
            Attachments:
            [
                new EmailAttachment("TeamUp_Report.pdf", "application/pdf", pdfBytes)
            ]));

        var recorded = sender.GetSentMessages("admin@teamup.local");
        var msg = Assert.Single(recorded);
        Assert.Equal("TeamUp Governance Report — 2026-11-24", msg.Subject);
        Assert.Equal("Governance Summary", msg.PlainTextBody);
        Assert.Contains("Governance Summary", msg.HtmlBody);

        var attachment = Assert.Single(msg.Attachments);
        Assert.Equal("TeamUp_Report.pdf", attachment.FileName);
        Assert.Equal("application/pdf", attachment.ContentType);
        Assert.Equal(pdfBytes.Length, attachment.SizeBytes);

        // Password reset tokens remain segregated from general templated dispatch inspection.
        Assert.Equal("secret_reset_tok", sender.GetLastResetToken("admin@teamup.local"));

        sender.Clear();
        Assert.Empty(sender.GetSentMessages());
        Assert.Null(sender.GetLastResetToken("admin@teamup.local"));
    }
}
