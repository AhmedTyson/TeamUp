namespace TeamUp.Application.Projects;

public interface IProjectMemberService
{
    Task<IReadOnlyList<ProjectMemberDto>> GetMembersAsync(int projectId, CancellationToken ct);
    Task UpdateMemberRoleAsync(int studentId, int projectId, int memberStudentId, UpdateProjectMemberRequest request, CancellationToken ct);
    Task RemoveMemberAsync(int studentId, int projectId, int memberStudentId, CancellationToken ct);
    Task<IReadOnlyList<ProjectApplicationDto>> GetApplicationsAsync(int studentId, int projectId, CancellationToken ct);
    Task<ProjectApplicationDto> ApplyAsync(int studentId, int projectId, CreateProjectApplicationRequest request, CancellationToken ct);
    Task<ProjectApplicationDto> ReviewApplicationAsync(int studentId, int projectId, int applicationId, ReviewProjectApplicationRequest request, CancellationToken ct);
    Task WithdrawApplicationAsync(int studentId, int projectId, int applicationId, CancellationToken ct);
}
