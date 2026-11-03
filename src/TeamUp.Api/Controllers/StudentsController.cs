using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Authenticated student account management and student directory discovery.
/// </summary>
[Route("api/v1/students")]
[Authorize]
public class StudentsController(IStudentAccountService accountService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<PagedResult<StudentCardDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentsDirectory([FromQuery] StudentDirectoryQuery query, CancellationToken ct)
    {
        var result = await accountService.GetStudentsDirectoryAsync(query, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<StudentPublicProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentById([FromRoute] int id, CancellationToken ct)
    {
        var result = await accountService.GetStudentPublicProfileByIdAsync(id, ct);
        return OkResponse(result);
    }

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

    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMe(CancellationToken ct)
    {
        await accountService.DeleteStudentAccountAsync(CurrentStudentId, ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStudent([FromRoute] int id, CancellationToken ct)
    {
        await accountService.DeleteStudentAccountAsync(id, ct);
        return NoContentResponse();
    }
}
