using TeamUp.Application.Common;

namespace TeamUp.Application.Students;

public sealed record StudentProfileDto(
    string? Bio,
    string? PhotoUrl,
    string? GitHubUrl,
    string? LinkedInUrl
);

public sealed record UpsertProfileRequest(
    string? Bio,
    string? PhotoUrl,
    string? GitHubUrl,
    string? LinkedInUrl
);

public sealed record UpdateStudentRequest(
    string? FirstName,
    string? LastName,
    int? AcademicYear
);

public sealed record StudentDetailDto(
    int StudentId,
    string FirstName,
    string LastName,
    string Email,
    int AcademicYear,
    string Role,
    int FacultyId,
    string FacultyName,
    string UniversityName,
    StudentProfileDto? Profile,
    IReadOnlyList<string> Interests
);

public sealed record StudentCardDto(
    int StudentId,
    string FirstName,
    string LastName,
    string Email,
    int AcademicYear,
    string Role,
    int FacultyId,
    string FacultyName,
    string UniversityName,
    IReadOnlyList<string> Interests
);

public sealed record StudentPublicProfileDto(
    int StudentId,
    string FirstName,
    string LastName,
    int AcademicYear,
    string FacultyName,
    string UniversityName,
    StudentProfileDto? Profile,
    IReadOnlyList<string> Interests
);

public sealed record StudentDirectoryQuery(
    string? Search = null,
    int? FacultyId = null,
    int? AcademicYear = null,
    int Page = 1,
    int PageSize = 20
)
{
    public PageRequest ToPageRequest() => new(Page, PageSize);
}

public sealed record AddInterestRequest(
    string Interest
);
