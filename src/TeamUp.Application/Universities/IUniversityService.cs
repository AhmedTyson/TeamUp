using TeamUp.Application.Common;

namespace TeamUp.Application.Universities;

public interface IUniversityService
{
    Task<PagedResult<UniversityDto>> GetUniversitiesAsync(string? search, PageRequest page, CancellationToken ct = default);
    Task<IReadOnlyList<UniversityDetailDto>> GetAcademicCatalogAsync(CancellationToken ct = default);
    Task<UniversityDetailDto> GetUniversityByIdAsync(int id, CancellationToken ct = default);
    Task<UniversityDetailDto> CreateUniversityAsync(CreateUniversityRequest req, CancellationToken ct = default);
    Task<UniversityDetailDto> UpdateUniversityAsync(int id, UpdateUniversityRequest req, CancellationToken ct = default);
    Task DeleteUniversityAsync(int id, CancellationToken ct = default);
}
