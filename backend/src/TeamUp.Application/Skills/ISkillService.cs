namespace TeamUp.Application.Skills;

public interface ISkillService
{
    Task<IReadOnlyList<SkillDto>> GetSkillsAsync(string? category, string? search, CancellationToken ct = default);
    Task<SkillDto> GetSkillByIdAsync(int id, CancellationToken ct = default);
    Task<SkillDto> CreateSkillAsync(CreateSkillRequest request, CancellationToken ct = default);
    Task<SkillDto> UpdateSkillAsync(int id, UpdateSkillRequest request, CancellationToken ct = default);
    Task DeleteSkillAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<StudentSkillDto>> GetStudentSkillsAsync(int studentId, CancellationToken ct = default);
    Task<StudentSkillDto> ClaimSkillAsync(int studentId, int skillId, ClaimSkillRequest request, CancellationToken ct = default);
    Task RemoveClaimedSkillAsync(int studentId, int skillId, CancellationToken ct = default);
}
