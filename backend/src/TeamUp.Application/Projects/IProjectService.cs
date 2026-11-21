using TeamUp.Application.Common;

namespace TeamUp.Application.Projects;

public interface IProjectService
{
    Task<PagedResult<ProjectListItemDto>> GetProjectsAsync(string? keyword, string? status, int? skillId, PageRequest page, CancellationToken ct);
    Task<ProjectDetailDto> CreateProjectAsync(int studentId, CreateProjectRequest request, CancellationToken ct);
    Task<ProjectDetailDto> GetProjectAsync(int projectId, CancellationToken ct);
    Task<ProjectDetailDto> UpdateProjectAsync(int studentId, int projectId, UpdateProjectRequest request, CancellationToken ct);
    Task DeleteProjectAsync(int studentId, int projectId, CancellationToken ct);
}
