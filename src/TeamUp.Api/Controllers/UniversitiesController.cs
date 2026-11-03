using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Universities;

namespace TeamUp.Api.Controllers;

[Route("api/v1/universities")]
[Authorize]
public class UniversitiesController(IUniversityService universityService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<PagedResult<UniversityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUniversities(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await universityService.GetUniversitiesAsync(search, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<UniversityDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUniversityById([FromRoute] int id, CancellationToken ct)
    {
        var result = await universityService.GetUniversityByIdAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesResponseType(typeof(BaseResponse<UniversityDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUniversity([FromBody] CreateUniversityRequest request, CancellationToken ct)
    {
        var result = await universityService.CreateUniversityAsync(request, ct);
        return CreatedResponse(nameof(GetUniversityById), new { id = result.UniversityId }, result, "University created successfully.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesResponseType(typeof(BaseResponse<UniversityDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUniversity([FromRoute] int id, CancellationToken ct)
    {
        await universityService.DeleteUniversityAsync(id, ct);
        return NoContentResponse();
    }
}
