namespace TeamUp.Application.Universities;

public sealed record CreateUniversityRequest(
    string Name,
    string City,
    string Country,
    string? Website,
    string? LogoUrl = null
);

public sealed record UpdateUniversityRequest(
    string Name,
    string City,
    string Country,
    string? Website,
    string? LogoUrl = null
);

public sealed record UniversityDto(
    int UniversityId,
    string Name,
    string City,
    string Country,
    string? Website,
    string? LogoUrl,
    int FacultyCount
);

public sealed record FacultySummaryDto(
    int FacultyId,
    string Name,
    string Code
);

public sealed record UniversityDetailDto(
    int UniversityId,
    string Name,
    string City,
    string Country,
    string? Website,
    string? LogoUrl,
    IReadOnlyList<FacultySummaryDto> Faculties
);
