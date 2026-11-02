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
    string Email,
    string FirstName,
    string LastName,
    int AcademicYear,
    string Role,
    int FacultyId,
    string FacultyName,
    string UniversityName,
    StudentProfileDto? Profile,
    IReadOnlyList<string> Interests
);

public sealed record AddInterestRequest(
    string Interest
);
