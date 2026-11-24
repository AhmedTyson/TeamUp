// Date: 2026-11-21
// Purpose: Session attendance endpoints with tenant-scoped authorization, rotating 60s QR check-in codes, RecordedByStudentId tracking, and HTML report output (STRIDE THR-T-02, THR-R-02).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;

namespace TeamUp.Api.Controllers;

[Route("api/v1/sessions/{id:int}/attendance")]
[Authorize]
public sealed class SessionAttendanceController(IEventService eventService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IReadOnlyList<SessionAttendanceDto>>), StatusCodes.Status200OK)]
    [ProducesForbidden]
    public async Task<IActionResult> GetAttendance([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.GetAttendanceAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result);
    }

    [HttpPost("check-in-code")]
    [ProducesResponseType(typeof(BaseResponse<SessionCheckInCodeDto>), StatusCodes.Status200OK)]
    [ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> GenerateCheckInCode([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.GenerateCheckInCodeAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result, "Rotating check-in code generated (valid for 60 seconds).");
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<SessionAttendanceDto>), StatusCodes.Status201Created)]
    [ProducesForbidden, ProducesConflict]
    public async Task<IActionResult> CheckIn([FromRoute] int id, [FromBody] CreateAttendanceRequest request, CancellationToken ct)
    {
        var result = await eventService.CheckInAsync(id, CurrentStudentId, CurrentUserRole, request, ct);
        return CreatedResponse(result, "Student checked in successfully.");
    }

    [HttpPost("self-check-in")]
    [ProducesResponseType(typeof(BaseResponse<SessionAttendanceDto>), StatusCodes.Status201Created)]
    [ProducesConflict, ProducesNotFound]
    public async Task<IActionResult> SelfCheckIn([FromRoute] int id, [FromBody] SelfCheckInRequest request, CancellationToken ct)
    {
        var result = await eventService.SelfCheckInAsync(id, CurrentStudentId, request.VerificationCode, ct);
        return CreatedResponse(result, "Self check-in verified successfully.");
    }

    [HttpDelete("{studentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesForbidden]
    public async Task<IActionResult> Revoke([FromRoute] int id, [FromRoute] int studentId, CancellationToken ct)
    {
        await eventService.RevokeAttendanceAsync(id, studentId, CurrentStudentId, CurrentUserRole, ct);
        return NoContentResponse();
    }

    [HttpGet("report")]
    [Produces("text/html")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> GetAttendanceReport([FromRoute] int id, CancellationToken ct)
    {
        var html = await eventService.RenderAttendanceReportHtmlAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpGet("export.csv")]
    [Produces("text/csv")]
    [ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> ExportAttendanceCsv([FromRoute] int id, CancellationToken ct)
    {
        var csv = await eventService.ExportAttendanceCsvAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv; charset=utf-8", $"attendance-session-{id}.csv");
    }
}
