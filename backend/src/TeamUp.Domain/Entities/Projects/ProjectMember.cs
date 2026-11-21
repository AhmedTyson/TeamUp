using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public int StudentId { get; set; }
    public string Role { get; set; } = null!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Project Project { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
