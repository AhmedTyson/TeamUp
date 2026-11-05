using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class ProjectRequiredSkill : BaseEntity
{
    public int ProjectId { get; set; }
    public int SkillId { get; set; }
    public ProficiencyLevel RequiredLevel { get; set; }
    public bool IsMandatory { get; set; } = true;
    public Project Project { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
