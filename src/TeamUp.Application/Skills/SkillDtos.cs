using TeamUp.Domain.Enums;

namespace TeamUp.Application.Skills;

public sealed record SkillDto(
    int SkillId,
    string Name,
    string Category,
    string? Description
);

public sealed record CreateSkillRequest(
    string Name,
    string Category,
    string? Description
);

public sealed record UpdateSkillRequest(
    string Name,
    string Category,
    string? Description
);

public sealed record StudentSkillDto(
    int SkillId,
    string Name,
    string Category,
    ProficiencyLevel ProficiencyLevel,
    decimal YearsOfExperience
);

public sealed record ClaimSkillRequest(
    ProficiencyLevel ProficiencyLevel,
    decimal YearsOfExperience
);
