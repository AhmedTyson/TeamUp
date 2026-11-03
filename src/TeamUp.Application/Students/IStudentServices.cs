namespace TeamUp.Application.Students;

/// <summary>Single Responsibility: Student account-level identity and settings.</summary>
public interface IStudentAccountService
{
    Task<StudentDetailDto> GetMyAccountAsync(int studentId, CancellationToken ct = default);
    Task<StudentDetailDto> UpdateMyAccountAsync(int studentId, UpdateStudentRequest req, CancellationToken ct = default);
}

/// <summary>Single Responsibility: Extended portfolio, biography, and social links.</summary>
public interface IStudentProfileService
{
    Task<StudentProfileDto> GetExtendedProfileAsync(int studentId, CancellationToken ct = default);
    Task<StudentProfileDto> UpsertExtendedProfileAsync(int studentId, UpsertProfileRequest req, CancellationToken ct = default);
}

/// <summary>Single Responsibility: Student extracurricular and technical interest tags.</summary>
public interface IStudentInterestService
{
    Task<IReadOnlyList<string>> GetInterestsAsync(int studentId, CancellationToken ct = default);
    Task AddInterestAsync(int studentId, string interest, CancellationToken ct = default);
    Task RemoveInterestAsync(int studentId, string interest, CancellationToken ct = default);
}
