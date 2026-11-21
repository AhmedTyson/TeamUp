using TeamUp.Domain.Enums;

namespace TeamUp.Application.Gamification;

public sealed record CreateBadgeRequest(
    string Name,
    string Description,
    string IconUrl,
    BadgeCriteriaType CriteriaType,
    int CriteriaValue
);

public sealed record UpdateBadgeRequest(
    string? Name,
    string? Description,
    string? IconUrl,
    BadgeCriteriaType? CriteriaType,
    int? CriteriaValue
);

public sealed record BadgeDto(
    int BadgeId,
    string Name,
    string Description,
    string IconUrl,
    BadgeCriteriaType CriteriaType,
    int CriteriaValue
);

public sealed record StudentBadgeDto(
    int StudentId,
    int BadgeId,
    string BadgeName,
    string Description,
    string IconUrl,
    BadgeCriteriaType CriteriaType,
    int CriteriaValue,
    DateTime AwardedAt
);
