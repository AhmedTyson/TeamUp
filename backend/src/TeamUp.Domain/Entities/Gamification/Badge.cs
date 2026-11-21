using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class Badge : BaseEntity
{
    public int BadgeId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string IconUrl { get; set; } = null!;
    public BadgeCriteriaType CriteriaType { get; set; }
    public int CriteriaValue { get; set; }
}
