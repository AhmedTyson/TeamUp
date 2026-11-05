using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class StudentSkill : BaseEntity
{
    public int StudentId { get; set; }
    public int SkillId { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; }
    public decimal YearsOfExperience { get; set; }
    public Student Student { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
