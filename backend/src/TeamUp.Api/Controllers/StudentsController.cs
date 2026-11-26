// Date: 2026-11-21
// Purpose: Authenticated student account lifecycle and privacy-filtered, rate-limited directory discovery (STRIDE THR-I-01).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Authenticated student account management and student directory discovery.
/// </summary>
[Route("api/v1/students")]
[Authorize]
public sealed class StudentsController(IStudentAccountService accountService) : BaseApiController
{
    [HttpGet("showcase")]
    [AllowAnonymous]
    [ProducesOk<IReadOnlyList<PublicStudentShowcaseDto>>]
    public async Task<IActionResult> GetPublicShowcase([FromQuery] int limit = 6, CancellationToken ct = default)
    {
        var result = await accountService.GetPublicShowcaseAsync(limit, ct);
        return OkResponse(result);
    }

    [HttpGet]
    [EnableRateLimiting("directory-policy")]
    [ProducesOk<PagedResult<StudentCardDto>>]
    public async Task<IActionResult> GetStudentsDirectory([FromQuery] StudentDirectoryQuery query, CancellationToken ct)
    {
        var result = await accountService.GetStudentsDirectoryAsync(query, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesOk<StudentPublicProfileDto>, ProducesNotFound]
    public async Task<IActionResult> GetStudentById([FromRoute] int id, CancellationToken ct)
    {
        var result = await accountService.GetStudentPublicProfileByIdAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result);
    }

    [HttpGet("me")]
    [ProducesOk<StudentDetailDto>, ProducesNotFound]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await accountService.GetMyAccountAsync(CurrentStudentId, ct);
        return OkResponse(result);
    }

    [HttpPatch("me")]
    [ProducesOk<StudentDetailDto>]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateStudentRequest request, CancellationToken ct)
    {
        var result = await accountService.UpdateMyAccountAsync(CurrentStudentId, request, ct);
        return OkResponse(result, "Student account updated successfully.");
    }

    [HttpDelete("me")]
    [ProducesNoContent]
    public async Task<IActionResult> DeleteMe(CancellationToken ct)
    {
        await accountService.DeleteStudentAccountAsync(CurrentStudentId, ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> DeleteStudent([FromRoute] int id, CancellationToken ct)
    {
        await accountService.DeleteStudentAccountAsync(id, ct);
        return NoContentResponse();
    }
}
