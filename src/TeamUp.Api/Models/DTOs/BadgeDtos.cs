using System.ComponentModel.DataAnnotations;

namespace TeamUp.Api.Models.DTOs;

public record BadgeCreateDto(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, MinLength(5)] string Description,
    [Required, Url, StringLength(500)] string IconUrl,
    [Required, RegularExpression("^(TotalPoints|EventsAttended|ProjectsCompleted|TasksCompleted|SkillsVerified|Manual)$")] string CriteriaType,
    [Range(0, int.MaxValue)] int CriteriaValue = 0
);

public record BadgeUpdateDto(
    [StringLength(120, MinimumLength = 2)] string? Name,
    [MinLength(5)] string? Description,
    [Url, StringLength(500)] string? IconUrl,
    [RegularExpression("^(TotalPoints|EventsAttended|ProjectsCompleted|TasksCompleted|SkillsVerified|Manual)$")] string? CriteriaType,
    [Range(0, int.MaxValue)] int? CriteriaValue
);

public record BadgeResponseDto(
    string BadgeId,
    string Name,
    string Description,
    string IconUrl,
    string CriteriaType,
    int CriteriaValue,
    bool IsDeleted,
    DateTime CreatedAt
);

public record StudentBadgeResponseDto(
    string StudentId,
    string BadgeId,
    string BadgeName,
    string Description,
    string IconUrl,
    string CriteriaType,
    int CriteriaValue,
    DateTime AwardedAt
);

