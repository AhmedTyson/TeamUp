using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;

namespace TeamUp.Api.Controllers;

[Route("api/v1/sessions/{id:int}/attendance")]
[Authorize(Roles = "OrgAdmin,PlatformAdmin")]
public sealed class SessionAttendanceController(IEventService eventService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IReadOnlyList<SessionAttendanceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendance([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.GetAttendanceAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<SessionAttendanceDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CheckIn([FromRoute] int id, [FromBody] CreateAttendanceRequest request, CancellationToken ct)
    {
        var result = await eventService.CheckInAsync(id, request, ct);
        return CreatedResponse(result, "Student checked in successfully.");
    }

    [HttpDelete("{studentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Revoke([FromRoute] int id, [FromRoute] int studentId, CancellationToken ct)
    {
        await eventService.RevokeAttendanceAsync(id, studentId, ct);
        return NoContentResponse();
    }
}
