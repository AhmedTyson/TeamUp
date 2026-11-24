// Date: 2026-11-24
// Purpose: Digest compilation and opt-in filtered dispatch contracts for Leaderboard + News and Portfolio-Matched Projects emails.

namespace TeamUp.Application.Gamification;

public sealed record CampusNewsItem(
    string Category,
    string Headline,
    string Summary);

public sealed record LeaderboardDigestRequest(
    string? PeriodLabel = null,
    IReadOnlyList<CampusNewsItem>? NewsItems = null,
    bool DryRun = false,
    string? TestRecipientEmail = null);

public sealed record ProjectMatchDigestRequest(
    int MaxMatchesPerStudent = 4,
    int MinMatchPercentage = 20,
    bool DryRun = false,
    string? TestRecipientEmail = null);

public sealed record DigestDispatchResultDto(
    string DigestType,
    bool DryRun,
    bool IsTestMode,
    int EligibleRecipientsCount,
    int SentCount,
    int SkippedCount,
    string? PreviewHtml);

public interface IDigestDispatchService
{
    Task<DigestDispatchResultDto> DispatchLeaderboardDigestAsync(
        LeaderboardDigestRequest request,
        CancellationToken ct = default);

    Task<DigestDispatchResultDto> DispatchProjectMatchDigestAsync(
        ProjectMatchDigestRequest request,
        CancellationToken ct = default);
}
