namespace TeamUp.Application.Students;

public interface IProfileService : IStudentAccountService, IStudentProfileService, IStudentInterestService
{
    Task<StudentDetailDto> GetMyProfileAsync(int studentId, CancellationToken ct = default);
    Task<StudentDetailDto> UpdateMyProfileAsync(int studentId, UpdateStudentRequest req, CancellationToken ct = default);
}
