// Date: 2026-11-21
// Purpose: Event session management endpoints enforcing organization-scoped BOLA authorization (STRIDE THR-T-01).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;

namespace TeamUp.Api.Controllers;

[Route("api/v1/sessions")]
[Authorize]
public sealed class EventSessionsController(IEventService eventService) : BaseApiController
{
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<EventSessionDto>), StatusCodes.Status200OK)]
    [ProducesForbidden]
    public async Task<IActionResult> UpdateSession([FromRoute] int id, [FromBody] UpdateEventSessionRequest request, CancellationToken ct)
    {
        var result = await eventService.UpdateSessionAsync(id, CurrentStudentId, CurrentUserRole, request, ct);
        return OkResponse(result, "Event session updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesForbidden]
    public async Task<IActionResult> DeleteSession([FromRoute] int id, CancellationToken ct)
    {
        await eventService.DeleteSessionAsync(id, CurrentStudentId, CurrentUserRole, ct);
        return NoContentResponse();
    }
}
