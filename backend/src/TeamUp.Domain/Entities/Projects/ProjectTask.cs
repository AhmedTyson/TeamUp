using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class ProjectTask : BaseEntity
{
    public int TaskId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.Todo;
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int ProjectId { get; set; }
    public int? AssignedStudentId { get; set; }
    public Project Project { get; set; } = null!;
    public Student? AssignedStudent { get; set; }
}
