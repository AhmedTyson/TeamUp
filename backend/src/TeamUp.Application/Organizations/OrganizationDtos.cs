using TeamUp.Domain.Enums;

namespace TeamUp.Application.Organizations;

public sealed record CreateOrganizationRequest(
    string Name,
    string Description,
    string Category,
    string? LogoUrl,
    OrganizationScope Scope = OrganizationScope.UniversityBranch,
    int? UniversityId = null,
    string? City = null,
    string? CoverImageUrl = null,
    string? WebsiteUrl = null,
    bool IsRecruiting = false,
    string? CurrentSeasonName = null);

public sealed record UpdateOrganizationRequest(
    string Name,
    string Description,
    string Category,
    string? LogoUrl,
    OrganizationScope Scope = OrganizationScope.UniversityBranch,
    int? UniversityId = null,
    string? City = null,
    string? CoverImageUrl = null,
    string? WebsiteUrl = null,
    bool IsRecruiting = false,
    string? CurrentSeasonName = null);

public sealed record UpdateOrganizationMemberRequest(
    string Role,
    int? OrganizationUnitId = null,
    OrganizationHierarchyTier? HierarchyTier = null);

public sealed record OrganizationMemberDto(
    int StudentId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    DateTime JoinedAt,
    int? OrganizationUnitId = null,
    string? UnitName = null,
    string? UnitCode = null,
    OrganizationHierarchyTier HierarchyTier = OrganizationHierarchyTier.OrganizingCrew);

public sealed record OrganizationUnitDto(
    int OrganizationUnitId,
    int OrganizationId,
    string Name,
    string Code,
    string? Description,
    OrganizationUnitType UnitType,
    bool IsRecruiting,
    int? OpenSeats,
    int MemberCount);

public sealed record CreateOrganizationUnitRequest(
    string Name,
    string Code,
    string? Description,
    OrganizationUnitType UnitType = OrganizationUnitType.TechnicalChapter,
    bool IsRecruiting = true,
    int? OpenSeats = null);

public sealed record UpdateOrganizationUnitRequest(
    string Name,
    string Code,
    string? Description,
    OrganizationUnitType UnitType = OrganizationUnitType.TechnicalChapter,
    bool IsRecruiting = true,
    int? OpenSeats = null);

public sealed record OrganizationSummaryDto(
    int OrganizationId,
    string Name,
    string Description,
    string Category,
    string? LogoUrl,
    int MemberCount,
    OrganizationScope Scope = OrganizationScope.UniversityBranch,
    int? UniversityId = null,
    string? UniversityName = null,
    string? City = null,
    string? CoverImageUrl = null,
    string? WebsiteUrl = null,
    bool IsRecruiting = false,
    string? CurrentSeasonName = null,
    int UnitCount = 0);

public sealed record OrganizationEventSummaryDto(
    int EventId,
    string Title,
    EventType Type,
    string Location,
    DateTime StartDate,
    DateTime EndDate,
    EventDeliveryMode DeliveryMode = EventDeliveryMode.InPerson,
    bool IsPublic = true);

public sealed record OrganizationDetailDto(
    int OrganizationId,
    string Name,
    string Description,
    string Category,
    string? LogoUrl,
    IReadOnlyList<OrganizationMemberDto> Members,
    IReadOnlyList<OrganizationEventSummaryDto> Events,
    OrganizationScope Scope = OrganizationScope.UniversityBranch,
    int? UniversityId = null,
    string? UniversityName = null,
    string? City = null,
    string? CoverImageUrl = null,
    string? WebsiteUrl = null,
    bool IsRecruiting = false,
    string? CurrentSeasonName = null,
    IReadOnlyList<OrganizationUnitDto>? Units = null);

public sealed record CreateMembershipApplicationRequest(
    string? Message,
    int? OrganizationUnitId = null,
    string? SeasonName = null);

public sealed record ReviewMembershipApplicationRequest(ApplicationStatus Status);

public sealed record UpdateApplicationStageRequest(
    RecruitmentStage Stage,
    decimal? EvaluationScore = null,
    string? InterviewNotes = null,
    int? OrganizationUnitId = null);

public sealed record MembershipApplicationDto(
    int ApplicationId,
    int OrganizationId,
    int StudentId,
    string StudentFirstName,
    string StudentLastName,
    string StudentEmail,
    string? Message,
    ApplicationStatus Status,
    DateTime AppliedAt,
    DateTime? ReviewedAt,
    int? OrganizationUnitId = null,
    string? UnitName = null,
    string? UnitCode = null,
    string? SeasonName = null,
    RecruitmentStage Stage = RecruitmentStage.Applied,
    decimal? EvaluationScore = null,
    string? InterviewNotes = null,
    string? OrganizationName = null);

public sealed record OrganizationHierarchyDto(
    int OrganizationId,
    string OrganizationName,
    OrganizationScope Scope,
    IReadOnlyList<OrganizationMemberDto> HighBoard,
    IReadOnlyList<OrganizationMemberDto> CommitteeLeadership,
    IReadOnlyList<OrganizationMemberDto> OrganizingCrew,
    IReadOnlyList<OrganizationUnitDto> Units,
    int ParticipantsCount);

public sealed record OrganizationParticipantDto(
    int StudentId,
    string FirstName,
    string LastName,
    string AffiliationLabel,
    int EventsRegisteredCount,
    int SessionsAttendedCount,
    string? LastEventTitle,
    DateTime? LastActiveAt);

public sealed record CreateOrganizationProposalRequest(
    string Name,
    string Description,
    string Category,
    string? LogoUrl,
    OrganizationScope Scope = OrganizationScope.UniversityBranch,
    int? UniversityId = null,
    string? City = null,
    string? CoverImageUrl = null,
    string? WebsiteUrl = null,
    string? CurrentSeasonName = null,
    string? Justification = null);

public sealed record ReviewOrganizationProposalRequest(
    bool Approve,
    string? ReviewNotes = null);

public sealed record OrganizationCreationRequestDto(
    int RequestId,
    int RequesterStudentId,
    string RequesterName,
    string RequesterEmail,
    string? RequesterUniversityName,
    string Name,
    string Description,
    string Category,
    string? LogoUrl,
    OrganizationScope Scope,
    int? UniversityId,
    string? UniversityName,
    string? City,
    string? CoverImageUrl,
    string? WebsiteUrl,
    string? CurrentSeasonName,
    string? Justification,
    ApplicationStatus Status,
    DateTime RequestedAt,
    DateTime? ReviewedAt,
    int? ReviewedByStudentId,
    string? ReviewedByName,
    string? ReviewNotes,
    int? CreatedOrganizationId);
