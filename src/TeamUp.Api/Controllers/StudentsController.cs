using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

[ApiController]
[Route("api/v1/students")]
[Authorize]
public class StudentsController(IProfileService profileService) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        var result = await profileService.GetMyProfileAsync(studentId, ct);
        return Ok(result);
    }

    [HttpPatch("me")]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateStudentRequest request, CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        var result = await profileService.UpdateMyProfileAsync(studentId, request, ct);
        return Ok(result);
    }

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

    [HttpGet("{id:int}/interests")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInterests([FromRoute] int id, CancellationToken ct)
    {
        var result = await profileService.GetInterestsAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("me/interests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddInterest([FromBody] AddInterestRequest request, CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        await profileService.AddInterestAsync(studentId, request.Interest, ct);
        return Ok(new { message = "Interest added successfully." });
    }

    [HttpDelete("me/interests/{interest}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveInterest([FromRoute] string interest, CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        await profileService.RemoveInterestAsync(studentId, interest, ct);
        return NoContent();
    }
}
