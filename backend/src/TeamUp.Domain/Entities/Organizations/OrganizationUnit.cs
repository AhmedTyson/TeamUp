// Date: 2026-11-24
// Purpose: Sub-unit (technical chapter, operational committee, affinity group) inside an organization or student branch.

using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class OrganizationUnit : BaseEntity
{
    public int OrganizationUnitId { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public OrganizationUnitType UnitType { get; set; } = OrganizationUnitType.TechnicalChapter;
    public bool IsRecruiting { get; set; }
    public int? OpenSeats { get; set; }
    public Organization Organization { get; set; } = null!;
}
