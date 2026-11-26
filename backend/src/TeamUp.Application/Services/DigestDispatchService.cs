// Date: 2026-11-24
// Purpose: Editorial digest compiler rendering Leaderboard + Campus News and Freelancer-style Project Match emails with strict opt-in filtering.

using System.Globalization;
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
    IOptions<EmailOptions> emailOptions,
    IOptions<GamificationOptions> gamificationOptions,
    TimeProvider? timeProvider = null) : IDigestDispatchService
{
    private readonly EmailOptions _email = emailOptions.Value;
    private readonly int _leaderboardSize = gamificationOptions.Value.DigestLeaderboardSize > 0
        ? gamificationOptions.Value.DigestLeaderboardSize
        : 10;
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    #region Leaderboard Digest

    public async Task<DigestDispatchResultDto> DispatchLeaderboardDigestAsync(
        LeaderboardDigestRequest request,
        CancellationToken ct = default)
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var baseUrl = NormalizeBaseUrl(_email.FrontendBaseUrl);
        var periodLabel = string.IsNullOrWhiteSpace(request.PeriodLabel)
            ? $"WEEKLY CAMPUS DIGEST · {now:MMM dd, yyyy}".ToUpperInvariant()
            : request.PeriodLabel.Trim();
        var issueNumber = $"ISSUE {now:yyyyMMdd}";

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
                FacultyCode = s.Faculty != null ? s.Faculty.Code : "IND",
                UniversityName = s.Faculty != null ? s.Faculty.University.Name : (s.City != null && s.City != "" ? s.City + " Community" : "Egypt Open Community"),
                s.EmailNotificationsEnabled,
                s.ReceiveLeaderboardDigests
            })
            .ToListAsync(ct);

        var rankedStandings = allStudents
            .Select(s =>
            {
                var xp = scoreByStudent.GetValueOrDefault(s.StudentId, 0);
                var metrics = gamification.CalculateLevelMetrics(s.StudentId, xp);
                return (s.StudentId, FullName: $"{s.FirstName} {s.LastName}".Trim(), s.Email, s.FacultyCode, s.UniversityName, TotalXp: xp, metrics.Level, s.EmailNotificationsEnabled, s.ReceiveLeaderboardDigests);
            })
            .OrderByDescending(x => x.TotalXp)
            .ThenBy(x => x.StudentId)
            .Index()
            .Select(entry => new LeaderboardDigestRow(
                entry.Index + 1,
                entry.Item.StudentId,
                entry.Item.FullName,
                entry.Item.Email,
                entry.Item.FacultyCode,
                entry.Item.UniversityName,
                entry.Item.TotalXp,
                entry.Item.Level,
                entry.Item.EmailNotificationsEnabled,
                entry.Item.ReceiveLeaderboardDigests))
            .ToList();

        var leaderboardRowsHtml = DigestHtmlBuilder.BuildLeaderboardRowsHtml(
            rankedStandings.Take(_leaderboardSize)
                .Select(r => new LeaderboardDigestEntry(r.Rank, r.FullName, r.FacultyCode, r.TotalXp, r.Level)));
        var newsItems = request.NewsItems is { Count: > 0 }
            ? request.NewsItems
            : new[]
            {
                new CampusNewsItem(
                    "PLATFORM · MATCHING ENGINE",
                    "Portfolio-to-Squad Recommendations Are Live",
                    "Claim verified skills and interests in Profile Studio to receive ranked recruiting project matches.")
            };
        var newsItemsHtml = DigestHtmlBuilder.BuildNewsItemsHtml(newsItems);

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

    #endregion

    #region Project Match Digest

    public async Task<DigestDispatchResultDto> DispatchProjectMatchDigestAsync(
        ProjectMatchDigestRequest request,
        CancellationToken ct = default)
    {
        var baseUrl = NormalizeBaseUrl(_email.FrontendBaseUrl);
        var maxMatches = Math.Clamp(request.MaxMatchesPerStudent, 1, 10);
        var minScore = Math.Clamp(request.MinMatchPercentage, 0, 100);
        var generatedDate = _time.GetUtcNow().UtcDateTime.ToString("yyyy-MM-dd");
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

        var testRecipientEmailNormalized = isTestMode
            ? request.TestRecipientEmail!.Trim().ToLowerInvariant()
            : null;

        var recipients = isTestMode
            ? await recipientsQuery
                .Where(s => s.Email == testRecipientEmailNormalized)
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

        // One batched recommendation pass for the whole recipient list (a constant number of queries)
        // instead of re-reading the same recruiting pool once per recipient.
        var matchesByStudent = await recommendations.GetRecommendedProjectsForStudentsAsync(
            studentIds,
            maxMatches,
            isTestMode ? 0 : minScore,
            ct);

        foreach (var recipient in recipients)
        {
            var matches = matchesByStudent.TryGetValue(recipient.StudentId, out var found)
                ? found
                : (IReadOnlyList<ProjectMatchRecommendationDto>)[];

            if (matches.Count == 0)
            {
                skippedCount++;
                continue;
            }

            var skillsSummary = skillsLookup.TryGetValue(recipient.StudentId, out var owned) && owned.Count > 0
                ? string.Join(" · ", owned)
                : "General campus profile (add verified skills in Profile Studio to sharpen scores)";

            var cardsHtml = DigestHtmlBuilder.BuildMatchedProjectCardsHtml(matches, baseUrl);
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
            var fallbackRows = await db.Projects
                .AsNoTracking()
                .Where(p => p.Status == Domain.Enums.ProjectStatus.Recruiting)
                .OrderByDescending(p => p.CreatedAt)
                .Take(maxMatches)
                .Select(p => new
                {
                    p.ProjectId,
                    p.Title,
                    p.Description,
                    MemberCount = p.Members.Count(),
                    p.MaxTeamSize,
                    CreatorName = p.Creator.FirstName + " " + p.Creator.LastName,
                    UniversityName = p.Creator.Faculty != null
                        ? p.Creator.Faculty.University.Name
                        : (p.Creator.City != null && p.Creator.City != "" ? p.Creator.City + " Community" : "Egypt Open Community"),
                    FacultyCode = p.Creator.Faculty != null ? p.Creator.Faculty.Code : "IND"
                })
                .ToListAsync(ct);

            if (fallbackRows.Count > 0)
            {
                var fallbackProjectIds = fallbackRows.Select(p => p.ProjectId).ToArray();
                var requiredSkillsByProject = (await db.ProjectRequiredSkills
                        .AsNoTracking()
                        .Where(prs => fallbackProjectIds.Contains(prs.ProjectId))
                        .Select(prs => new { prs.ProjectId, SkillName = prs.Skill.Name })
                        .ToListAsync(ct))
                    .GroupBy(x => x.ProjectId)
                    .ToDictionary(
                        g => g.Key,
                        g => (IReadOnlyList<string>)g.Select(x => x.SkillName).OrderBy(n => n).ToList());

                var fallbackMatches = fallbackRows
                    .Select(p => new ProjectMatchRecommendationDto(
                        p.ProjectId,
                        p.Title,
                        p.Description,
                        0,
                        p.MemberCount,
                        p.MaxTeamSize,
                        Math.Max(p.MaxTeamSize - p.MemberCount, 0),
                        p.CreatorName,
                        p.UniversityName,
                        p.FacultyCode,
                        [],
                        requiredSkillsByProject.GetValueOrDefault(p.ProjectId, []),
                        []))
                    .ToList();

                var cardsHtml = DigestHtmlBuilder.BuildMatchedProjectCardsHtml(fallbackMatches, baseUrl);
                previewHtml = await templateRenderer.RenderEmailAsync(
                    "ProjectMatchDigest.html",
                    new Dictionary<string, string>
                    {
                        ["RecipientName"] = "TeamUp Administrator",
                        ["MatchCount"] = fallbackMatches.Count.ToString(),
                        ["StudentSkillsSummary"] = "No matched student portfolio · Preview of recruiting squads",
                        ["GeneratedDate"] = generatedDate,
                        ["ProjectsUrl"] = $"{baseUrl}/projects",
                        ["PreferencesUrl"] = $"{baseUrl}/profile/me"
                    },
                    new Dictionary<string, SafeHtmlFragment>
                    {
                        ["MatchedProjectCardsHtml"] = cardsHtml
                    },
                    ct);
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

    #endregion

    #region Internal Models & Helpers

    private sealed record LeaderboardDigestRow(
        int Rank,
        int StudentId,
        string FullName,
        string Email,
        string FacultyCode,
        string UniversityName,
        int TotalXp,
        int Level,
        bool EmailNotificationsEnabled,
        bool ReceiveLeaderboardDigests);

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

    #endregion
}
