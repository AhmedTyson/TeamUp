using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

public class Organization : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? LogoUrl { get; set; }
}
