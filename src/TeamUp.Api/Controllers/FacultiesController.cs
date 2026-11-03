using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Universities;

namespace TeamUp.Api.Controllers;

public class FacultiesController(IFacultyService facultyService) : BaseApiController
{
    [HttpGet("api/v1/universities/{universityId:int}/faculties")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<IReadOnlyList<FacultyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFacultiesByUniversity([FromRoute] int universityId, CancellationToken ct)
    {
        var result = await facultyService.GetFacultiesByUniversityIdAsync(universityId, ct);
        return OkResponse(result);
    }

    [HttpPost("api/v1/universities/{universityId:int}/faculties")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesResponseType(typeof(BaseResponse<FacultyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateFaculty(
        [FromRoute] int universityId,
        [FromBody] CreateFacultyRequest request,
        CancellationToken ct)
    {
        var result = await facultyService.CreateFacultyAsync(universityId, request, ct);
        return CreatedResponse(nameof(GetFacultyById), new { id = result.FacultyId }, result, "Faculty created successfully.");
    }

    [HttpGet("api/v1/faculties/{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<FacultyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFacultyById([FromRoute] int id, CancellationToken ct)
    {
        var result = await facultyService.GetFacultyByIdAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPut("api/v1/faculties/{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesResponseType(typeof(BaseResponse<FacultyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateFaculty(
        [FromRoute] int id,
        [FromBody] UpdateFacultyRequest request,
        CancellationToken ct)
    {
        var result = await facultyService.UpdateFacultyAsync(id, request, ct);
        return OkResponse(result, "Faculty updated successfully.");
    }

    [HttpDelete("api/v1/faculties/{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFaculty([FromRoute] int id, CancellationToken ct)
    {
        await facultyService.DeleteFacultyAsync(id, ct);
        return NoContentResponse();
    }
}
