using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Authenticated student account management.
/// </summary>
[ApiController]
[Route("api/v1/students")]
[Authorize]
public class StudentsController(IStudentAccountService accountService) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        var result = await accountService.GetMyAccountAsync(studentId, ct);
        return Ok(result);
    }

    [HttpPatch("me")]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateStudentRequest request, CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        var result = await accountService.UpdateMyAccountAsync(studentId, request, ct);
        return Ok(result);
    }
}
