using TeamUp.Domain.Enums;

namespace TeamUp.Application.Projects;

public sealed record CreateProjectRequest(string Title, string Description, int MaxTeamSize);
public sealed record UpdateProjectRequest(string? Title, string? Description, ProjectStatus? Status, int? MaxTeamSize);
public sealed record UpsertRequiredSkillRequest(ProficiencyLevel RequiredLevel, bool IsMandatory = true);
public sealed record UpdateProjectMemberRequest(string Role);
public sealed record CreateProjectApplicationRequest(string? Message);
public sealed record ReviewProjectApplicationRequest(ApplicationStatus Status);
public sealed record CreateProjectTaskRequest(string Title, string? Description, DateTime? DueDate, TaskPriority Priority = TaskPriority.Medium);
public sealed record UpdateProjectTaskRequest(string? Title, string? Description, ProjectTaskStatus? Status, TaskPriority? Priority, DateTime? DueDate);
public sealed record AssignTaskRequest(int StudentId);

public sealed record ProjectSkillDto(int SkillId, string Name, string Category, ProficiencyLevel RequiredLevel, bool IsMandatory);
public sealed record ProjectMemberDto(int StudentId, string FirstName, string LastName, string Role, DateTime JoinedAt);
public sealed record ProjectListItemDto(int ProjectId, string Title, string Description, ProjectStatus Status, int MaxTeamSize, int TeamSize, int CreatorId, string CreatorName, DateTime CreatedAt);
public sealed record ProjectDetailDto(int ProjectId, string Title, string Description, ProjectStatus Status, int MaxTeamSize, int TeamSize, int CreatorId, string CreatorName, DateTime CreatedAt, IReadOnlyList<ProjectMemberDto> Members, IReadOnlyList<ProjectSkillDto> RequiredSkills);
public sealed record ProjectApplicationDto(int ApplicationId, int ProjectId, int StudentId, string StudentName, string? Message, ApplicationStatus Status, DateTime AppliedAt);
public sealed record ProjectTaskDto(int TaskId, string Title, string? Description, ProjectTaskStatus Status, TaskPriority Priority, DateTime? DueDate, int ProjectId, int? AssignedStudentId, string? AssignedStudentName);
