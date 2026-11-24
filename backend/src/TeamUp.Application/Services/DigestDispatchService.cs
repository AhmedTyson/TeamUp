// Date: 2026-11-24
// Purpose: Editorial digest compiler rendering Leaderboard + Campus News and Freelancer-style Project Match emails with strict opt-in filtering.

using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Gamification;
using TeamUp.Application.Projects;

namespace TeamUp.Application.Services;

public sealed class DigestDispatchService(
    IAppDbContext db,
    IGamificationService gamification,
    IProjectRecommendationService recommendations,
    ITemplateRenderer templateRenderer,
    IEmailSender emailSender,
    IOptions<EmailOptions> emailOptions) : IDigestDispatchService
{
    private readonly EmailOptions _email = emailOptions.Value;

    public async Task<DigestDispatchResultDto> DispatchLeaderboardDigestAsync(
        LeaderboardDigestRequest request,
        CancellationToken ct = default)
    {
        var baseUrl = NormalizeBaseUrl(_email.FrontendBaseUrl);
        var periodLabel = string.IsNullOrWhiteSpace(request.PeriodLabel)
            ? $"WEEKLY CAMPUS DIGEST · {DateTime.UtcNow:MMM dd, yyyy}".ToUpperInvariant()
            : request.PeriodLabel.Trim();
        var issueNumber = $"ISSUE {DateTime.UtcNow:yyyyMMdd}";

        var scoreByStudent = await db.ScoreTransactions
            .AsNoTracking()
            .GroupBy(t => t.StudentId)
            .Select(g => new { StudentId = g.Key, TotalPoints = g.Sum(x => x.Points) })
            .ToDictionaryAsync(x => x.StudentId, x => x.TotalPoints, ct);

        var allStudents = await db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.StudentId,
                s.FirstName,
                s.LastName,
                s.Email,
                FacultyCode = s.Faculty.Code,
                UniversityName = s.Faculty.University.Name,
                s.EmailNotificationsEnabled,
                s.ReceiveLeaderboardDigests
            })
            .ToListAsync(ct);

        var rankedStandings = allStudents
            .Select(s =>
            {
                var xp = scoreByStudent.GetValueOrDefault(s.StudentId, 0);
                var metrics = gamification.CalculateLevelMetrics(s.StudentId, xp);
                return new
                {
                    s.StudentId,
                    FullName = $"{s.FirstName} {s.LastName}".Trim(),
                    s.Email,
                    s.FacultyCode,
                    s.UniversityName,
                    TotalXp = xp,
                    metrics.Level,
                    s.EmailNotificationsEnabled,
                    s.ReceiveLeaderboardDigests
                };
            })
            .OrderByDescending(x => x.TotalXp)
            .ThenBy(x => x.StudentId)
            .Select((row, index) => new
            {
                Rank = index + 1,
                row.StudentId,
                row.FullName,
                row.Email,
                row.FacultyCode,
                row.UniversityName,
                row.TotalXp,
                row.Level,
                row.EmailNotificationsEnabled,
                row.ReceiveLeaderboardDigests
            })
            .ToList();

        var leaderboardRowsHtml = BuildLeaderboardRowsHtml(rankedStandings.Take(10));
        var newsItems = request.NewsItems is { Count: > 0 }
            ? request.NewsItems
            : new[]
            {
                new CampusNewsItem(
                    "PLATFORM · MATCHING ENGINE",
                    "Portfolio-to-Squad Recommendations Are Live",
                    "Claim verified skills and interests in Profile Studio to receive ranked recruiting project matches.")
            };
        var newsItemsHtml = BuildNewsItemsHtml(newsItems);

        var isTestMode = !string.IsNullOrWhiteSpace(request.TestRecipientEmail);
        var targets = isTestMode
            ? rankedStandings
                .Where(s => string.Equals(s.Email, request.TestRecipientEmail!.Trim(), StringComparison.OrdinalIgnoreCase))
                .Take(1)
                .Select(s => new LeaderboardRecipientTarget(s.Email, s.FullName, s.Rank.ToString(), s.TotalXp.ToString("N0"), s.Level.ToString()))
                .DefaultIfEmpty(new LeaderboardRecipientTarget(request.TestRecipientEmail!.Trim(), "TeamUp Administrator", "1", "0", "1"))
                .ToList()
            : rankedStandings
                .Where(s => s.EmailNotificationsEnabled && s.ReceiveLeaderboardDigests)
                .OrderBy(s => s.StudentId)
                .Select(s => new LeaderboardRecipientTarget(s.Email, s.FullName, s.Rank.ToString(), s.TotalXp.ToString("N0"), s.Level.ToString()))
                .ToList();

        var fragments = new Dictionary<string, SafeHtmlFragment>
        {
            ["LeaderboardRowsHtml"] = leaderboardRowsHtml,
            ["NewsItemsHtml"] = newsItemsHtml
        };

        string? previewHtml = null;
        var sentCount = 0;

        foreach (var target in targets)
        {
            var html = await templateRenderer.RenderEmailAsync(
                "LeaderboardDigest.html",
                new Dictionary<string, string>
                {
                    ["IssueNumber"] = issueNumber,
                    ["PeriodLabel"] = periodLabel,
                    ["RecipientName"] = target.FullName,
                    ["MyRank"] = target.Rank,
                    ["MyXp"] = target.Xp,
                    ["MyLevel"] = target.Level,
                    ["LeaderboardUrl"] = $"{baseUrl}/leaderboard",
                    ["PreferencesUrl"] = $"{baseUrl}/profile/me"
                },
                fragments,
                ct);

            previewHtml ??= html;

            if (request.DryRun)
                continue;

            var plainText = $"TeamUp Campus Leaderboard & Dispatch ({periodLabel})\nRank: #{target.Rank} | {target.Xp} XP | Level {target.Level}\nView full standings: {baseUrl}/leaderboard";
            await emailSender.SendTemplatedEmailAsync(
                new EmailDispatchRequest(
                    target.Email,
                    $"TeamUp {periodLabel} — Top Standings & Campus News",
                    html,
                    plainText),
                ct);
            sentCount++;
        }

        if (previewHtml is null)
        {
            previewHtml = await templateRenderer.RenderEmailAsync(
                "LeaderboardDigest.html",
                new Dictionary<string, string>
                {
                    ["IssueNumber"] = issueNumber,
                    ["PeriodLabel"] = periodLabel,
                    ["RecipientName"] = "Preview Student",
                    ["MyRank"] = "1",
                    ["MyXp"] = "0",
                    ["MyLevel"] = "1",
                    ["LeaderboardUrl"] = $"{baseUrl}/leaderboard",
                    ["PreferencesUrl"] = $"{baseUrl}/profile/me"
                },
                fragments,
                ct);
        }

        return new DigestDispatchResultDto(
            DigestType: "LeaderboardDigest",
            DryRun: request.DryRun,
            IsTestMode: isTestMode,
            EligibleRecipientsCount: targets.Count,
            SentCount: sentCount,
            SkippedCount: 0,
            PreviewHtml: previewHtml);
    }

    public async Task<DigestDispatchResultDto> DispatchProjectMatchDigestAsync(
        ProjectMatchDigestRequest request,
        CancellationToken ct = default)
    {
        var baseUrl = NormalizeBaseUrl(_email.FrontendBaseUrl);
        var maxMatches = Math.Clamp(request.MaxMatchesPerStudent, 1, 10);
        var minScore = Math.Clamp(request.MinMatchPercentage, 0, 100);
        var generatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var isTestMode = !string.IsNullOrWhiteSpace(request.TestRecipientEmail);

        var recipientsQuery = db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.StudentId,
                FullName = s.FirstName + " " + s.LastName,
                s.Email,
                s.EmailNotificationsEnabled,
                s.ReceiveProjectMatchEmails
            });

        var recipients = isTestMode
            ? await recipientsQuery
                .Where(s => s.Email == request.TestRecipientEmail!.Trim().ToLowerInvariant())
                .Take(1)
                .ToListAsync(ct)
            : await recipientsQuery
                .Where(s => s.EmailNotificationsEnabled && s.ReceiveProjectMatchEmails)
                .OrderBy(s => s.StudentId)
                .ToListAsync(ct);

        var studentIds = recipients.Select(r => r.StudentId).ToArray();
        var skillsLookup = studentIds.Length == 0
            ? new Dictionary<int, List<string>>()
            : (await db.StudentSkills
                .AsNoTracking()
                .Where(ss => studentIds.Contains(ss.StudentId))
                .Select(ss => new { ss.StudentId, SkillName = ss.Skill.Name })
                .ToListAsync(ct))
                .GroupBy(x => x.StudentId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.SkillName).OrderBy(n => n).ToList());

        string? previewHtml = null;
        var sentCount = 0;
        var skippedCount = 0;

        foreach (var recipient in recipients)
        {
            var effectiveMinScore = isTestMode ? 0 : minScore;
            var matches = await recommendations.GetRecommendedProjectsForStudentAsync(
                recipient.StudentId,
                maxMatches,
                effectiveMinScore,
                ct);

            if (matches.Count == 0)
            {
                skippedCount++;
                continue;
            }

            var skillsSummary = skillsLookup.TryGetValue(recipient.StudentId, out var owned) && owned.Count > 0
                ? string.Join(" · ", owned)
                : "General campus profile (add verified skills in Profile Studio to sharpen scores)";

            var cardsHtml = BuildMatchedProjectCardsHtml(matches, baseUrl);
            var html = await templateRenderer.RenderEmailAsync(
                "ProjectMatchDigest.html",
                new Dictionary<string, string>
                {
                    ["RecipientName"] = recipient.FullName.Trim(),
                    ["MatchCount"] = matches.Count.ToString(),
                    ["StudentSkillsSummary"] = skillsSummary,
                    ["GeneratedDate"] = generatedDate,
                    ["ProjectsUrl"] = $"{baseUrl}/projects",
                    ["PreferencesUrl"] = $"{baseUrl}/profile/me"
                },
                new Dictionary<string, SafeHtmlFragment>
                {
                    ["MatchedProjectCardsHtml"] = cardsHtml
                },
                ct);

            previewHtml ??= html;

            if (request.DryRun)
                continue;

            var plainText = $"{matches.Count} recruiting squads match your portfolio on TeamUp ({generatedDate}). Top match: {matches[0].Title} ({matches[0].MatchPercentage}%).";
            await emailSender.SendTemplatedEmailAsync(
                new EmailDispatchRequest(
                    recipient.Email,
                    $"TeamUp Squad Match: {matches.Count} recruiting project(s) fit your portfolio",
                    html,
                    plainText),
                ct);
            sentCount++;
        }

        if (previewHtml is null && isTestMode)
        {
            var fallbackMatches = await db.Projects
                .AsNoTracking()
                .Where(p => p.Status == Domain.Enums.ProjectStatus.Recruiting)
                .OrderByDescending(p => p.CreatedAt)
                .Take(maxMatches)
                .Select(p => new ProjectMatchRecommendationDto(
                    p.ProjectId,
                    p.Title,
                    p.Description,
                    85,
                    p.Members.Count(),
                    p.MaxTeamSize,
                    Math.Max(p.MaxTeamSize - p.Members.Count(), 0),
                    p.Creator.FirstName + " " + p.Creator.LastName,
                    p.Creator.Faculty.University.Name,
                    p.Creator.Faculty.Code,
                    new[] { "React", "TypeScript" },
                    Array.Empty<string>(),
                    Array.Empty<string>()))
                .ToListAsync(ct);

            if (fallbackMatches.Count > 0)
            {
                var cardsHtml = BuildMatchedProjectCardsHtml(fallbackMatches, baseUrl);
                previewHtml = await templateRenderer.RenderEmailAsync(
                    "ProjectMatchDigest.html",
                    new Dictionary<string, string>
                    {
                        ["RecipientName"] = "TeamUp Administrator",
                        ["MatchCount"] = fallbackMatches.Count.ToString(),
                        ["StudentSkillsSummary"] = "Preview stack · Portfolio Match Simulation",
                        ["GeneratedDate"] = generatedDate,
                        ["ProjectsUrl"] = $"{baseUrl}/projects",
                        ["PreferencesUrl"] = $"{baseUrl}/profile/me"
                    },
                    new Dictionary<string, SafeHtmlFragment>
                    {
                        ["MatchedProjectCardsHtml"] = cardsHtml
                    },
                    ct);

                if (!request.DryRun)
                {
                    await emailSender.SendTemplatedEmailAsync(
                        new EmailDispatchRequest(
                            request.TestRecipientEmail!.Trim(),
                            $"TeamUp Squad Match Preview ({fallbackMatches.Count} project(s))",
                            previewHtml,
                            $"TeamUp Squad Match preview with {fallbackMatches.Count} recruiting squad(s)."),
                        ct);
                    sentCount++;
                    skippedCount = Math.Max(0, skippedCount - 1);
                }
            }
        }

        return new DigestDispatchResultDto(
            DigestType: "ProjectMatchDigest",
            DryRun: request.DryRun,
            IsTestMode: isTestMode,
            EligibleRecipientsCount: recipients.Count,
            SentCount: sentCount,
            SkippedCount: skippedCount,
            PreviewHtml: previewHtml);
    }

    private static SafeHtmlFragment BuildLeaderboardRowsHtml(IEnumerable<dynamic> topRows)
    {
        var rows = new StringBuilder();
        var index = 0;

        foreach (var row in topRows)
        {
            var bg = index == 0 ? "#EDF8F1" : index % 2 == 1 ? "#F8F9F7" : "#FFFFFF";
            var rankColor = index == 0 ? "#15733F" : "#14151C";
            rows.Append(
                $"<tr style=\"background-color:{bg};\">"
                + $"<td style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:12px;font-weight:700;color:{rankColor};\">#{row.Rank:D2}</td>"
                + $"<td style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-size:13px;font-weight:600;color:#14151C;\">{WebUtility.HtmlEncode((string)row.FullName)}</td>"
                + $"<td style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-size:11px;color:#6B7280;\">{WebUtility.HtmlEncode((string)row.FacultyCode)}</td>"
                + $"<td align=\"right\" style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:12px;color:#4B5260;\">{row.Level}</td>"
                + $"<td align=\"right\" style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:12px;font-weight:700;color:#14151C;\">{((int)row.TotalXp):N0}</td>"
                + "</tr>");
            index++;
        }

        if (index == 0)
        {
            rows.Append("<tr><td colspan=\"5\" style=\"padding:12px;font-size:13px;color:#6B7280;\">No ranked students recorded yet.</td></tr>");
        }

        return new SafeHtmlFragment(rows.ToString());
    }

    private static SafeHtmlFragment BuildNewsItemsHtml(IReadOnlyList<CampusNewsItem> newsItems)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < newsItems.Count; i++)
        {
            var item = newsItems[i];
            var marginBottom = i == newsItems.Count - 1 ? "0" : "12px";
            sb.Append(
                $"<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:0 0 {marginBottom};border:1px solid #E2E6E0;background-color:#FFFFFF;\">"
                + "<tr><td style=\"padding:14px 16px;\">"
                + $"<p style=\"margin:0 0 4px;font-family:'SFMono-Regular',Consolas,monospace;font-size:10px;letter-spacing:0.1em;color:#15733F;font-weight:700;text-transform:uppercase;\">{WebUtility.HtmlEncode(item.Category)}</p>"
                + $"<p style=\"margin:0 0 4px;font-size:15px;font-weight:700;color:#14151C;\">{WebUtility.HtmlEncode(item.Headline)}</p>"
                + $"<p style=\"margin:0;font-size:13px;line-height:1.5;color:#4B5260;\">{WebUtility.HtmlEncode(item.Summary)}</p>"
                + "</td></tr></table>");
        }

        return new SafeHtmlFragment(sb.ToString());
    }

    private static SafeHtmlFragment BuildMatchedProjectCardsHtml(
        IReadOnlyList<ProjectMatchRecommendationDto> matches,
        string baseUrl)
    {
        var sb = new StringBuilder();

        foreach (var match in matches)
        {
            var projectUrl = WebUtility.HtmlEncode($"{baseUrl}/projects/{match.ProjectId}");
            var summary = match.Description.Length > 150
                ? match.Description[..147] + "..."
                : match.Description;
            var seatsText = $"{match.OpenSeats} / {match.MaxTeamSize} spots open";

            var chips = new StringBuilder();
            foreach (var skill in match.MatchedSkills)
            {
                chips.Append(
                    $"<span style=\"display:inline-block;margin:0 6px 6px 0;padding:3px 8px;border:1px solid #BBE5CB;background-color:#EDF8F1;color:#15733F;font-family:'SFMono-Regular',Consolas,monospace;font-size:11px;font-weight:600;border-radius:2px;\">+ {WebUtility.HtmlEncode(skill)}</span>");
            }
            foreach (var missing in match.MissingSkills)
            {
                chips.Append(
                    $"<span style=\"display:inline-block;margin:0 6px 6px 0;padding:3px 8px;border:1px solid #DDE2E7;background-color:#F4F6F8;color:#475569;font-family:'SFMono-Regular',Consolas,monospace;font-size:11px;font-weight:600;border-radius:2px;\">- {WebUtility.HtmlEncode(missing)}</span>");
            }

            sb.Append(
                "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:0 0 18px;border:1px solid #E2E6E0;background-color:#FFFFFF;\">"
                + "<tr><td style=\"padding:18px 20px;\">"
                + "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:0 0 10px;\"><tr>"
                + $"<td align=\"left\" style=\"vertical-align:middle;\"><span style=\"display:inline-block;padding:3px 9px;background-color:#14151C;color:#D3FB52;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:11px;font-weight:700;letter-spacing:0.08em;border-radius:2px;\">{match.MatchPercentage}% MATCH</span></td>"
                + $"<td align=\"right\" style=\"vertical-align:middle;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:11px;color:#15733F;font-weight:700;\">{WebUtility.HtmlEncode(seatsText)}</td>"
                + "</tr></table>"
                + "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:0 0 12px;background-color:#E5E7EB;height:4px;\"><tr>"
                + $"<td width=\"{Math.Clamp(match.MatchPercentage, 5, 100)}%\" style=\"background-color:#15733F;height:4px;font-size:0;line-height:4px;\">&nbsp;</td>"
                + "<td style=\"height:4px;font-size:0;line-height:4px;\">&nbsp;</td>"
                + "</tr></table>"
                + $"<h2 style=\"margin:0 0 4px;font-size:18px;line-height:1.3;font-weight:700;color:#14151C;\">{WebUtility.HtmlEncode(match.Title)}</h2>"
                + $"<p style=\"margin:0 0 10px;font-size:12px;color:#6B7280;\">Lead: <strong style=\"color:#14151C;\">{WebUtility.HtmlEncode(match.CreatorName)}</strong> &middot; {WebUtility.HtmlEncode(match.UniversityName)} &middot; {WebUtility.HtmlEncode(match.FacultyCode)}</p>"
                + $"<p style=\"margin:0 0 12px;font-size:14px;line-height:1.5;color:#4B5260;\">{WebUtility.HtmlEncode(summary)}</p>"
                + $"<div style=\"margin:0 0 14px;\">{chips}</div>"
                + "<table role=\"presentation\" cellspacing=\"0\" cellpadding=\"0\"><tr>"
                + $"<td style=\"background-color:#14151C;border-radius:2px;\"><a href=\"{projectUrl}\" style=\"display:inline-block;padding:10px 18px;font-family:'SFMono-Regular',Consolas,monospace;font-size:11px;font-weight:700;letter-spacing:0.1em;text-transform:uppercase;color:#D3FB52;text-decoration:none;\">INSPECT &amp; APPLY</a></td>"
                + "</tr></table>"
                + "</td></tr></table>");
        }

        return new SafeHtmlFragment(sb.ToString());
    }

    private static string NormalizeBaseUrl(string? url) =>
        string.IsNullOrWhiteSpace(url)
            ? "https://teamup-ieee.vercel.app"
            : url.Trim().TrimEnd('/');

    private sealed record LeaderboardRecipientTarget(
        string Email,
        string FullName,
        string Rank,
        string Xp,
        string Level);
}
