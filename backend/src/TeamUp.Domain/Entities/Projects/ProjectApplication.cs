using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class ProjectApplication : BaseEntity
{
    public int ApplicationId { get; set; }
    public int ProjectId { get; set; }
    public int StudentId { get; set; }
    public string? Message { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public Project Project { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
