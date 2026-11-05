namespace TeamUp.Application.Projects;

public interface IProjectSkillService
{
    Task<IReadOnlyList<ProjectSkillDto>> GetRequiredSkillsAsync(int projectId, CancellationToken ct);
    Task UpsertRequiredSkillAsync(int studentId, int projectId, int skillId, UpsertRequiredSkillRequest request, CancellationToken ct);
    Task RemoveRequiredSkillAsync(int studentId, int projectId, int skillId, CancellationToken ct);
}
