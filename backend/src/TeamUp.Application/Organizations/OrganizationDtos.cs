namespace TeamUp.Application.Organizations;

public sealed record CreateOrganizationRequest(
    string Name,
    string Description,
    string Category,
    string? LogoUrl);

public sealed record UpdateOrganizationRequest(
    string Name,
    string Description,
    string Category,
    string? LogoUrl);

public sealed record UpdateOrganizationMemberRequest(string Role);

public sealed record OrganizationMemberDto(
    int StudentId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    DateTime JoinedAt);

public sealed record OrganizationSummaryDto(
    int OrganizationId,
    string Name,
    string Description,
    string Category,
    string? LogoUrl,
    int MemberCount);

public sealed record OrganizationEventSummaryDto(
    int EventId,
    string Title,
    TeamUp.Domain.Enums.EventType Type,
    string Location,
    DateTime StartDate,
    DateTime EndDate);

public sealed record OrganizationDetailDto(
    int OrganizationId,
    string Name,
    string Description,
    string Category,
    string? LogoUrl,
    IReadOnlyList<OrganizationMemberDto> Members,
    IReadOnlyList<OrganizationEventSummaryDto> Events);

public sealed record CreateMembershipApplicationRequest(string? Message);

public sealed record ReviewMembershipApplicationRequest(TeamUp.Domain.Enums.ApplicationStatus Status);

public sealed record MembershipApplicationDto(
    int ApplicationId,
    int OrganizationId,
    int StudentId,
    string StudentFirstName,
    string StudentLastName,
    string StudentEmail,
    string? Message,
    TeamUp.Domain.Enums.ApplicationStatus Status,
    DateTime AppliedAt,
    DateTime? ReviewedAt);
