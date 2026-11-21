using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

public class Skill : BaseEntity
{
    public int SkillId { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
}
