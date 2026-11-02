namespace TeamUp.Application.Students;

public interface IProfileService
{
    Task<StudentDetailDto> GetMyProfileAsync(int studentId, CancellationToken ct = default);
    Task<StudentDetailDto> UpdateMyProfileAsync(int studentId, UpdateStudentRequest req, CancellationToken ct = default);
    Task<StudentProfileDto> GetExtendedProfileAsync(int studentId, CancellationToken ct = default);
    Task<StudentProfileDto> UpsertExtendedProfileAsync(int studentId, UpsertProfileRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetInterestsAsync(int studentId, CancellationToken ct = default);
    Task AddInterestAsync(int studentId, string interest, CancellationToken ct = default);
    Task RemoveInterestAsync(int studentId, string interest, CancellationToken ct = default);
}
