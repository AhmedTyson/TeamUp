namespace TeamUp.Application.Projects;

public interface IProjectTaskService
{
    Task<IReadOnlyList<ProjectTaskDto>> GetProjectTasksAsync(int studentId, int projectId, CancellationToken ct);
    Task<ProjectTaskDto> CreateTaskAsync(int studentId, int projectId, CreateProjectTaskRequest request, CancellationToken ct);
    Task<ProjectTaskDto> GetTaskAsync(int studentId, int taskId, CancellationToken ct);
    Task<ProjectTaskDto> UpdateTaskAsync(int studentId, int taskId, UpdateProjectTaskRequest request, CancellationToken ct);
    Task AssignTaskAsync(int studentId, int taskId, AssignTaskRequest request, CancellationToken ct);
    Task UnassignTaskAsync(int studentId, int taskId, CancellationToken ct);
    Task DeleteTaskAsync(int studentId, int taskId, CancellationToken ct);
}
