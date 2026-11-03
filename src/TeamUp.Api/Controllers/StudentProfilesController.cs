using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Extended portfolio, biography, and external profiles.
/// </summary>
[Route("api/v1/students")]
[Authorize]
public class StudentProfilesController(IStudentProfileService profileService) : BaseApiController
{
    [HttpGet("{id:int}/profile")]
    [ProducesResponseType(typeof(BaseResponse<StudentProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile([FromRoute] int id, CancellationToken ct)
    {
        var result = await profileService.GetExtendedProfileAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(BaseResponse<StudentProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpsertMyProfile([FromBody] UpsertProfileRequest request, CancellationToken ct)
    {
        var result = await profileService.UpsertExtendedProfileAsync(CurrentStudentId, request, ct);
        return OkResponse(result, "Profile updated successfully.");
    }
}
