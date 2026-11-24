// Date: 2026-11-25
// Purpose: Strongly-typed seed models deserialized from the embedded Egyptian campus ecosystem JSON dataset.

namespace TeamUp.Infrastructure.Seed.Models;

public sealed record EgyptCampusSeedData
{
    public string Version { get; init; } = string.Empty;
    public List<UniversitySeedItem> Universities { get; init; } = [];
    public List<SkillSeedItem> Skills { get; init; } = [];
    public List<BadgeSeedItem> Badges { get; init; } = [];
    public List<StudentSeedItem> Students { get; init; } = [];
    public List<OrganizationSeedItem> Organizations { get; init; } = [];
    public List<ProjectSeedItem> Projects { get; init; } = [];
    public List<EventSeedItem> Events { get; init; } = [];
    public List<MembershipApplicationSeedItem> MembershipApplications { get; init; } = [];
}

public sealed record UniversitySeedItem
{
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = "Egypt";
    public string? Website { get; init; }
    public string? LogoUrl { get; init; }
    public List<FacultySeedItem> Faculties { get; init; } = [];
}

public sealed record FacultySeedItem
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}

public sealed record SkillSeedItem
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed record BadgeSeedItem
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string IconUrl { get; init; } = string.Empty;
    public string CriteriaType { get; init; } = string.Empty;
    public int CriteriaValue { get; init; }
}

public sealed record StudentSeedItem
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public int AcademicYear { get; init; } = 1;
    public string AffiliationType { get; init; } = "UniversityStudent";
    public string? City { get; init; }
    public string? FacultyCode { get; init; }
    public int Points { get; init; }
    public string? PhotoUrl { get; init; }
    public string? Bio { get; init; }
    public string? GitHubUrl { get; init; }
    public string? LinkedInUrl { get; init; }
    public List<string> Interests { get; init; } = [];
    public List<StudentSkillSeedItem> Skills { get; init; } = [];
}

public sealed record StudentSkillSeedItem
{
    public string Name { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = "Intermediate";
    public decimal YearsOfExperience { get; init; } = 1;
}

public sealed record OrganizationSeedItem
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string Scope { get; init; } = "UniversityBranch";
    public string? UniversityName { get; init; }
    public string? City { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? WebsiteUrl { get; init; }
    public bool IsRecruiting { get; init; }
    public string? CurrentSeasonName { get; init; }
    public List<OrganizationUnitSeedItem> Units { get; init; } = [];
    public List<OrganizationMemberSeedItem> Members { get; init; } = [];
}

public sealed record OrganizationUnitSeedItem
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string UnitType { get; init; } = "TechnicalChapter";
    public bool IsRecruiting { get; init; }
    public int? OpenSeats { get; init; }
}

public sealed record OrganizationMemberSeedItem
{
    public string StudentEmail { get; init; } = string.Empty;
    public string Role { get; init; } = "Member";
    public string? UnitCode { get; init; }
    public string HierarchyTier { get; init; } = "OrganizingCrew";
}

public sealed record ProjectSeedItem
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = "Recruiting";
    public int MaxTeamSize { get; init; } = 5;
    public string CreatorEmail { get; init; } = string.Empty;
    public List<ProjectSkillSeedItem> RequiredSkills { get; init; } = [];
    public List<ProjectMemberSeedItem> Members { get; init; } = [];
    public List<ProjectTaskSeedItem> Tasks { get; init; } = [];
    public List<ProjectApplicationSeedItem> Applications { get; init; } = [];
}

public sealed record ProjectSkillSeedItem
{
    public string Name { get; init; } = string.Empty;
    public string RequiredLevel { get; init; } = "Intermediate";
    public bool IsMandatory { get; init; } = true;
}

public sealed record ProjectMemberSeedItem
{
    public string StudentEmail { get; init; } = string.Empty;
    public string Role { get; init; } = "Contributor";
}

public sealed record ProjectTaskSeedItem
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = "Todo";
    public string Priority { get; init; } = "Medium";
    public int DueOffsetDays { get; init; } = 7;
    public string? AssignedStudentEmail { get; init; }
}

public sealed record ProjectApplicationSeedItem
{
    public string StudentEmail { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string Status { get; init; } = "Pending";
}

public sealed record EventSeedItem
{
    public string OrganizationName { get; init; } = string.Empty;
    public string? UnitCode { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Type { get; init; } = "Workshop";
    public string DeliveryMode { get; init; } = "InPerson";
    public bool IsPublic { get; init; } = true;
    public string? MeetingUrl { get; init; }
    public string Location { get; init; } = string.Empty;
    public int StartOffsetDays { get; init; } = 5;
    public int StartHourUtc { get; init; } = 10;
    public int DurationHours { get; init; } = 4;
    public int Capacity { get; init; } = 100;
    public List<EventSessionSeedItem> Sessions { get; init; } = [];
    public List<EventRegistrationSeedItem> Registrations { get; init; } = [];
}

public sealed record EventSessionSeedItem
{
    public int SessionNo { get; init; }
    public string Title { get; init; } = string.Empty;
    public int OffsetHours { get; init; }
    public int DurationHours { get; init; } = 3;
    public string? Room { get; init; }
    public List<string> AttendedEmails { get; init; } = [];
}

public sealed record EventRegistrationSeedItem
{
    public string StudentEmail { get; init; } = string.Empty;
    public string Status { get; init; } = "Confirmed";
}

public sealed record MembershipApplicationSeedItem
{
    public string OrganizationName { get; init; } = string.Empty;
    public string StudentEmail { get; init; } = string.Empty;
    public string? UnitCode { get; init; }
    public string? SeasonName { get; init; }
    public string? Message { get; init; }
    public string Stage { get; init; } = "Applied";
    public decimal? EvaluationScore { get; init; }
    public string? InterviewNotes { get; init; }
}
