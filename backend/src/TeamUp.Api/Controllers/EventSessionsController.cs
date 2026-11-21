using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;

namespace TeamUp.Api.Controllers;

[Route("api/v1/sessions")]
[Authorize(Roles = "OrgAdmin,PlatformAdmin")]
public sealed class EventSessionsController(IEventService eventService) : BaseApiController
{
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BaseResponse<EventSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSession([FromRoute] int id, [FromBody] UpdateEventSessionRequest request, CancellationToken ct)
    {
        var result = await eventService.UpdateSessionAsync(id, request, ct);
        return OkResponse(result, "Event session updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteSession([FromRoute] int id, CancellationToken ct)
    {
        await eventService.DeleteSessionAsync(id, ct);
        return NoContentResponse();
    }
}
