// Date: 2026-11-25
// Purpose: Entity capturing student proposals for creating new student organizations pending PlatformAdmin approval.

using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class OrganizationCreationRequest : BaseEntity
{
    public int RequestId { get; set; }
    public int RequesterStudentId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Category { get; set; } = null!;
    public OrganizationScope Scope { get; set; } = OrganizationScope.UniversityBranch;
    public int? UniversityId { get; set; }
    public string? City { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? CurrentSeasonName { get; set; }
    public string? Justification { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByStudentId { get; set; }
    public string? ReviewNotes { get; set; }
    public int? CreatedOrganizationId { get; set; }

    public Student RequesterStudent { get; set; } = null!;
    public University? University { get; set; }
    public Student? ReviewedByStudent { get; set; }
    public Organization? CreatedOrganization { get; set; }
}
