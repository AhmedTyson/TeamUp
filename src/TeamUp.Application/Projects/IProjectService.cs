using TeamUp.Application.Common;

namespace TeamUp.Application.Projects;

public interface IProjectService
{
    Task<PagedResult<ProjectListItemDto>> GetProjectsAsync(string? keyword, string? status, int? skillId, PageRequest page, CancellationToken ct);
    Task<ProjectDetailDto> CreateProjectAsync(int studentId, CreateProjectRequest request, CancellationToken ct);
    Task<ProjectDetailDto> GetProjectAsync(int projectId, CancellationToken ct);
    Task<ProjectDetailDto> UpdateProjectAsync(int studentId, int projectId, UpdateProjectRequest request, CancellationToken ct);
    Task DeleteProjectAsync(int studentId, int projectId, CancellationToken ct);
    Task<IReadOnlyList<ProjectSkillDto>> GetRequiredSkillsAsync(int projectId, CancellationToken ct);
    Task UpsertRequiredSkillAsync(int studentId, int projectId, int skillId, UpsertRequiredSkillRequest request, CancellationToken ct);
    Task RemoveRequiredSkillAsync(int studentId, int projectId, int skillId, CancellationToken ct);
    Task<IReadOnlyList<ProjectMemberDto>> GetMembersAsync(int projectId, CancellationToken ct);
    Task UpdateMemberRoleAsync(int studentId, int projectId, int memberStudentId, UpdateProjectMemberRequest request, CancellationToken ct);
    Task RemoveMemberAsync(int studentId, int projectId, int memberStudentId, CancellationToken ct);
    Task<IReadOnlyList<ProjectApplicationDto>> GetApplicationsAsync(int studentId, int projectId, CancellationToken ct);
    Task<ProjectApplicationDto> ApplyAsync(int studentId, int projectId, CreateProjectApplicationRequest request, CancellationToken ct);
    Task<ProjectApplicationDto> ReviewApplicationAsync(int studentId, int projectId, int applicationId, ReviewProjectApplicationRequest request, CancellationToken ct);
    Task WithdrawApplicationAsync(int studentId, int projectId, int applicationId, CancellationToken ct);
    Task<IReadOnlyList<ProjectTaskDto>> GetProjectTasksAsync(int studentId, int projectId, CancellationToken ct);
    Task<ProjectTaskDto> CreateTaskAsync(int studentId, int projectId, CreateProjectTaskRequest request, CancellationToken ct);
    Task<ProjectTaskDto> GetTaskAsync(int studentId, int taskId, CancellationToken ct);
    Task<ProjectTaskDto> UpdateTaskAsync(int studentId, int taskId, UpdateProjectTaskRequest request, CancellationToken ct);
    Task AssignTaskAsync(int studentId, int taskId, AssignTaskRequest request, CancellationToken ct);
    Task UnassignTaskAsync(int studentId, int taskId, CancellationToken ct);
    Task DeleteTaskAsync(int studentId, int taskId, CancellationToken ct);
}
