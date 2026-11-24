using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class Organization : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public OrganizationScope Scope { get; set; } = OrganizationScope.UniversityBranch;
    public int? UniversityId { get; set; }
    public University? University { get; set; }
    public string? City { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsRecruiting { get; set; }
    public string? CurrentSeasonName { get; set; }
    public ICollection<OrganizationUnit> Units { get; set; } = [];
}
