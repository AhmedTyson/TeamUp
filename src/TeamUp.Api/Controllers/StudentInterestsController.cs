using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Extracurricular and technical interest tags.
/// </summary>
[ApiController]
[Route("api/v1/students")]
[Authorize]
public class StudentInterestsController(IStudentInterestService interestService) : ControllerBase
{
    [HttpGet("{id:int}/interests")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInterests([FromRoute] int id, CancellationToken ct)
    {
        var result = await interestService.GetInterestsAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("me/interests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddInterest([FromBody] AddInterestRequest request, CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        await interestService.AddInterestAsync(studentId, request.Interest, ct);
        return Ok(new { message = "Interest added successfully." });
    }

    [HttpDelete("me/interests/{interest}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveInterest([FromRoute] string interest, CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        await interestService.RemoveInterestAsync(studentId, interest, ct);
        return NoContent();
    }
}
