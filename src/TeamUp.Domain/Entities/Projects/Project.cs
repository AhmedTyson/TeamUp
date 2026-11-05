using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class Project : BaseEntity
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ProjectStatus Status { get; set; } = ProjectStatus.Recruiting;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int MaxTeamSize { get; set; }
    public int CreatorId { get; set; }
    public Student Creator { get; set; } = null!;
    public byte[] RowVersion { get; set; } = [];
    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<ProjectTask> Tasks { get; set; } = [];
}
