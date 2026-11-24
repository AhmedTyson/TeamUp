using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class OrganizationMember : BaseEntity
{
    public int OrganizationId { get; set; }
    public int StudentId { get; set; }
    public string Role { get; set; } = null!;
    public int? OrganizationUnitId { get; set; }
    public OrganizationHierarchyTier HierarchyTier { get; set; } = OrganizationHierarchyTier.OrganizingCrew;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Organization Organization { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public OrganizationUnit? Unit { get; set; }
}
