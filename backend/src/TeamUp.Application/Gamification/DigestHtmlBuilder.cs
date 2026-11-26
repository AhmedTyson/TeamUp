// Date: 2026-11-27
// Purpose: Pure HTML fragment compiler for editorial leaderboard and project-match digests.

using System.Globalization;
using System.Net;
using System.Text;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Projects;

namespace TeamUp.Application.Gamification;

public sealed record LeaderboardDigestEntry(
    int Rank,
    string FullName,
    string FacultyCode,
    int TotalXp,
    int Level);

public static class DigestHtmlBuilder
{
    public static SafeHtmlFragment BuildLeaderboardRowsHtml(IEnumerable<LeaderboardDigestEntry> topRows)
    {
        var rows = new StringBuilder();
        var count = 0;

        foreach (var (index, row) in topRows.Index())
        {
            count++;
            var bg = index == 0 ? "#EDF8F1" : index % 2 == 1 ? "#F8F9F7" : "#FFFFFF";
            var rankColor = index == 0 ? "#15733F" : "#14151C";
            var rankText = row.Rank.ToString("D2", CultureInfo.InvariantCulture);
            var levelText = row.Level.ToString(CultureInfo.InvariantCulture);
            var xpText = row.TotalXp.ToString("N0", CultureInfo.InvariantCulture);

            rows.Append(
                $"<tr style=\"background-color:{bg};\">"
                + $"<td style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:12px;font-weight:700;color:{rankColor};\">#{rankText}</td>"
                + $"<td style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-size:13px;font-weight:600;color:#14151C;\">{WebUtility.HtmlEncode(row.FullName)}</td>"
                + $"<td style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-size:11px;color:#6B7280;\">{WebUtility.HtmlEncode(row.FacultyCode)}</td>"
                + $"<td align=\"right\" style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:12px;color:#4B5260;\">{levelText}</td>"
                + $"<td align=\"right\" style=\"padding:9px 10px;border-top:1px solid #E2E6E0;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:12px;font-weight:700;color:#14151C;\">{xpText}</td>"
                + "</tr>");
        }

        if (count == 0)
        {
            rows.Append("<tr><td colspan=\"5\" style=\"padding:12px;font-size:13px;color:#6B7280;\">No ranked students recorded yet.</td></tr>");
        }

        return new SafeHtmlFragment(rows.ToString());
    }

    public static SafeHtmlFragment BuildNewsItemsHtml(IReadOnlyList<CampusNewsItem> newsItems)
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

    public static SafeHtmlFragment BuildMatchedProjectCardsHtml(
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
            var seatsText = $"{match.OpenSeats.ToString(CultureInfo.InvariantCulture)} / {match.MaxTeamSize.ToString(CultureInfo.InvariantCulture)} spots open";
            var pctText = match.MatchPercentage.ToString(CultureInfo.InvariantCulture);
            var barWidth = Math.Clamp(match.MatchPercentage, 5, 100).ToString(CultureInfo.InvariantCulture);

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
                + $"<td align=\"left\" style=\"vertical-align:middle;\"><span style=\"display:inline-block;padding:3px 9px;background-color:#14151C;color:#D3FB52;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:11px;font-weight:700;letter-spacing:0.08em;border-radius:2px;\">{pctText}% MATCH</span></td>"
                + $"<td align=\"right\" style=\"vertical-align:middle;font-family:'SFMono-Regular',Consolas,monospace;font-variant-numeric:tabular-nums;font-size:11px;color:#15733F;font-weight:700;\">{WebUtility.HtmlEncode(seatsText)}</td>"
                + "</tr></table>"
                + "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:0 0 12px;background-color:#E5E7EB;height:4px;\"><tr>"
                + $"<td width=\"{barWidth}%\" style=\"background-color:#15733F;height:4px;font-size:0;line-height:4px;\">&nbsp;</td>"
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
}
