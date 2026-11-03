using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Extended portfolio, biography, and external profiles.
/// </summary>
[ApiController]
[Route("api/v1/students")]
[Authorize]
public class StudentProfilesController(IStudentProfileService profileService) : ControllerBase
{
    [HttpGet("{id:int}/profile")]
    [ProducesResponseType(typeof(StudentProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile([FromRoute] int id, CancellationToken ct)
    {
        var result = await profileService.GetExtendedProfileAsync(id, ct);
        return Ok(result);
    }

    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(StudentProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpsertMyProfile([FromBody] UpsertProfileRequest request, CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        var result = await profileService.UpsertExtendedProfileAsync(studentId, request, ct);
        return Ok(result);
    }
}
