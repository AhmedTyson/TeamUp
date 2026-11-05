using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common;
using TeamUp.Application.Universities;

namespace TeamUp.Api.Controllers;

[Route("api/v1/universities")]
public class UniversitiesController(IUniversityService universityService) : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesOk<PagedResult<UniversityDto>>]
    public async Task<IActionResult> GetUniversities(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await universityService.GetUniversitiesAsync(search, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }

    [HttpGet("catalog")]
    [AllowAnonymous]
    [ProducesOk<IReadOnlyList<UniversityDetailDto>>]
    public async Task<IActionResult> GetAcademicCatalog(CancellationToken ct = default)
    {
        var result = await universityService.GetAcademicCatalogAsync(ct);
        return OkResponse(result, "Academic catalog retrieved successfully for registration lookups.");
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesOk<UniversityDetailDto>, ProducesNotFound]
    public async Task<IActionResult> GetUniversityById([FromRoute] int id, CancellationToken ct)
    {
        var result = await universityService.GetUniversityByIdAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesCreated<UniversityDetailDto>]
    [ProducesForbidden, ProducesConflict]
    public async Task<IActionResult> CreateUniversity([FromBody] CreateUniversityRequest request, CancellationToken ct)
    {
        var result = await universityService.CreateUniversityAsync(request, ct);
        return CreatedResponse(nameof(GetUniversityById), new { id = result.UniversityId }, result, "University created successfully.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesOk<UniversityDetailDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateUniversity(
        [FromRoute] int id,
        [FromBody] UpdateUniversityRequest request,
        CancellationToken ct)
    {
        var result = await universityService.UpdateUniversityAsync(id, request, ct);
        return OkResponse(result, "University updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> DeleteUniversity([FromRoute] int id, CancellationToken ct)
    {
        await universityService.DeleteUniversityAsync(id, ct);
        return NoContentResponse();
    }
}
