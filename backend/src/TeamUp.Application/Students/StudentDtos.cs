// Date: 2026-11-21
// Purpose: Student account, profile, directory query (max 20 page size), and privacy-aware card DTOs (STRIDE THR-I-01).

using TeamUp.Application.Common;
using TeamUp.Domain.Enums;

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
    int? AcademicYear,
    bool? ShowEmailInDirectory = null,
    bool? EmailNotificationsEnabled = null,
    bool? ReceiveLeaderboardDigests = null,
    bool? ReceiveProjectMatchEmails = null,
    AffiliationType? AffiliationType = null,
    int? FacultyId = null,
    string? City = null
);

public sealed record StudentDetailDto(
    int StudentId,
    string FirstName,
    string LastName,
    string Email,
    int AcademicYear,
    string Role,
    int? FacultyId,
    string FacultyName,
    string UniversityName,
    StudentProfileDto? Profile,
    IReadOnlyList<string> Interests,
    bool ShowEmailInDirectory = false,
    bool EmailNotificationsEnabled = true,
    bool ReceiveLeaderboardDigests = true,
    bool ReceiveProjectMatchEmails = true,
    AffiliationType AffiliationType = AffiliationType.UniversityStudent,
    string? City = null
);

public sealed record StudentCardDto(
    int StudentId,
    string FirstName,
    string LastName,
    string? Email,
    int AcademicYear,
    string Role,
    int? FacultyId,
    string FacultyName,
    string UniversityName,
    IReadOnlyList<string> Interests,
    string? PhotoUrl = null,
    AffiliationType AffiliationType = AffiliationType.UniversityStudent,
    string? City = null
);

public sealed record PublicStudentShowcaseDto(
    int StudentId,
    string FirstName,
    string LastName,
    int AcademicYear,
    string FacultyName,
    string FacultyCode,
    string UniversityName,
    string? UniversityLogoUrl,
    string? Bio,
    string PhotoUrl,
    int TotalPoints,
    int Level,
    int BadgesCount,
    IReadOnlyList<string> TopSkills
);

public sealed record StudentPublicProfileDto(
    int StudentId,
    string FirstName,
    string LastName,
    int AcademicYear,
    string FacultyName,
    string UniversityName,
    StudentProfileDto? Profile,
    IReadOnlyList<string> Interests,
    string? Email = null,
    AffiliationType AffiliationType = AffiliationType.UniversityStudent,
    string? City = null
);

public sealed record StudentDirectoryQuery(
    string? Search = null,
    int? FacultyId = null,
    int? AcademicYear = null,
    int Page = 1,
    int PageSize = 20
)
{
    /// <summary>Aggressive directory page-size cap preventing bulk student roster harvesting (STRIDE THR-I-01).</summary>
    public const int MaxDirectoryPageSize = 20;

    public PageRequest ToPageRequest() => new(Page, Math.Clamp(PageSize, 1, MaxDirectoryPageSize));
}

public sealed record AddInterestRequest(
    string Interest
);
