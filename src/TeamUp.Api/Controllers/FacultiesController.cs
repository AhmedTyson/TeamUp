using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Universities;

namespace TeamUp.Api.Controllers;

[Route("api/v1")]
public class FacultiesController(IFacultyService facultyService) : BaseApiController
{
    [HttpGet("universities/{universityId:int}/faculties")]
    [AllowAnonymous]
    [ProducesOk<IReadOnlyList<FacultyDto>>, ProducesNotFound]
    public async Task<IActionResult> GetFacultiesByUniversity([FromRoute] int universityId, CancellationToken ct)
    {
        var result = await facultyService.GetFacultiesByUniversityIdAsync(universityId, ct);
        return OkResponse(result);
    }

    [HttpPost("universities/{universityId:int}/faculties")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesCreated<FacultyDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> CreateFaculty(
        [FromRoute] int universityId,
        [FromBody] CreateFacultyRequest request,
        CancellationToken ct)
    {
        var result = await facultyService.CreateFacultyAsync(universityId, request, ct);
        return CreatedResponse(nameof(GetFacultyById), new { id = result.FacultyId }, result, "Faculty created successfully.");
    }

    [HttpGet("faculties/{id:int}")]
    [AllowAnonymous]
    [ProducesOk<FacultyDto>, ProducesNotFound]
    public async Task<IActionResult> GetFacultyById([FromRoute] int id, CancellationToken ct)
    {
        var result = await facultyService.GetFacultyByIdAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPut("faculties/{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesOk<FacultyDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateFaculty(
        [FromRoute] int id,
        [FromBody] UpdateFacultyRequest request,
        CancellationToken ct)
    {
        var result = await facultyService.UpdateFacultyAsync(id, request, ct);
        return OkResponse(result, "Faculty updated successfully.");
    }

    [HttpDelete("faculties/{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> DeleteFaculty([FromRoute] int id, CancellationToken ct)
    {
        await facultyService.DeleteFacultyAsync(id, ct);
        return NoContentResponse();
    }
}
