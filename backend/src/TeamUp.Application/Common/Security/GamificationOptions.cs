// Date: 2026-11-25
// Purpose: Configuration options for gamification rules (XP awards per action and the level curve step), so organizers
// can tune incentives without a code change. Defaults preserve the shipped economy.

using System.ComponentModel.DataAnnotations;

namespace TeamUp.Application.Common.Security;

public sealed class GamificationOptions
{
    public const string SectionName = "Gamification";

    /// <summary>XP awarded when a student attends a session (SessionAttendedDomainEvent).</summary>
    [Range(0, 10000)]
    public int PointsPerSession { get; set; } = 25;

    /// <summary>XP awarded once per completed task (TaskCompletedDomainEvent).</summary>
    [Range(0, 10000)]
    public int PointsPerCompletedTask { get; set; } = 50;

    /// <summary>XP awarded to each member when a project is delivered (ProjectCompletedDomainEvent).</summary>
    [Range(0, 10000)]
    public int PointsPerProjectDelivered { get; set; } = 100;

    /// <summary>
    /// Points span of one level step: level = floor(sqrt(points / LevelStepPoints)) + 1, so level N starts at
    /// (N-1)^2 * LevelStepPoints. Must stay positive; invalid values fall back to the default in the service.
    /// </summary>
    [Range(1, 100000)]
    public int LevelStepPoints { get; set; } = 100;

    /// <summary>How many top rows the weekly digest leaderboard email shows.</summary>
    [Range(1, 100)]
    public int DigestLeaderboardSize { get; set; } = 10;
}
