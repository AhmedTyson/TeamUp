// Date: 2026-11-21
// Purpose: Event registration & waitlist cancellation endpoints enforcing organization-scoped authorization (STRIDE THR-T-01, THR-D-02).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;
using TeamUp.Domain.Enums;

namespace TeamUp.Api.Controllers;

[Route("api/v1/events/{id:int}/registrations")]
[Authorize]
public sealed class EventRegistrationsController(IEventService eventService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IReadOnlyList<EventRegistrationDto>>), StatusCodes.Status200OK)]
    [ProducesForbidden]
    public async Task<IActionResult> GetRegistrations([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.GetRegistrationsAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<EventRegistrationDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromRoute] int id, CancellationToken ct)
    {
        var result = await eventService.RegisterAsync(id, CurrentStudentId, ct);
        return CreatedResponse(result, result.Status == RegistrationStatus.Waitlisted
            ? "Event capacity is full. You have been added to the waitlist."
            : "Event registration confirmed successfully.");
    }

    [HttpDelete]
    [ProducesNoContent, ProducesForbidden]
    public async Task<IActionResult> Cancel(
        [FromRoute] int id, [FromQuery] int? studentId, CancellationToken ct)
    {
        var targetStudentId = studentId ?? CurrentStudentId;
        await eventService.CancelRegistrationAsync(id, targetStudentId, CurrentStudentId, CurrentUserRole, ct);
        return NoContentResponse();
    }
}
