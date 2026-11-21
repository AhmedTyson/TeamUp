namespace TeamUp.Application.Universities;

public interface IFacultyService
{
    Task<IReadOnlyList<FacultyDto>> GetFacultiesByUniversityIdAsync(int universityId, CancellationToken ct = default);
    Task<FacultyDto> GetFacultyByIdAsync(int id, CancellationToken ct = default);
    Task<FacultyDto> CreateFacultyAsync(int universityId, CreateFacultyRequest req, CancellationToken ct = default);
    Task<FacultyDto> UpdateFacultyAsync(int id, UpdateFacultyRequest req, CancellationToken ct = default);
    Task DeleteFacultyAsync(int id, CancellationToken ct = default);
}
