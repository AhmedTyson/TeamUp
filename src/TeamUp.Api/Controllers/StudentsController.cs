using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Authenticated student account management.
/// </summary>
[Route("api/v1/students")]
[Authorize]
public class StudentsController(IStudentAccountService accountService) : BaseApiController
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(BaseResponse<StudentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await accountService.GetMyAccountAsync(CurrentStudentId, ct);
        return OkResponse(result);
    }

    [HttpPatch("me")]
    [ProducesResponseType(typeof(BaseResponse<StudentDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateStudentRequest request, CancellationToken ct)
    {
        var result = await accountService.UpdateMyAccountAsync(CurrentStudentId, request, ct);
        return OkResponse(result, "Student account updated successfully.");
    }
}
