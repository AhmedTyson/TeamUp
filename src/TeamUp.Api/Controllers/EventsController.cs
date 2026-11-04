using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Auth;
using TeamUp.Api.Data;
using TeamUp.Api.Models.DTOs;
using TeamUp.Api.Services;

namespace TeamUp.Api.Controllers;

[ApiController]
[Route("api/v1/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly TeamUpDbContext _db;

    public EventsController(IEventService eventService, TeamUpDbContext db)
    {
        _eventService = eventService;
        _db = db;
    }

    // Endpoint #68: Browse upcoming workshops, hackathons, and competitions
    [HttpGet]
    [ProducesResponseType(typeof(EventListResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListEvents(
        [FromQuery] string? type,
        [FromQuery] string? organizationId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var result = await _eventService.ListEventsAsync(type, organizationId, search, page, limit);
        return Ok(result);
    }

    // Endpoint #69: Create a new event hosted by an organization
    [HttpPost]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto dto)
    {
        // OrgAdmin or PlatformAdmin check
        if (!await User.IsOrgAdminAsync(_db, dto.OrganizationId))
        {
            return Forbid();
        }

        try
        {
            var result = await _eventService.CreateEventAsync(dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Endpoint #70: Event agenda, location, dates, and registration count
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvent(string id)
    {
        var result = await _eventService.GetEventAsync(id);
        if (result == null) return NotFound(new { message = $"Event {id} not found" });
        return Ok(result);
    }

    // Endpoint #71: Update event venue, dates, or registration capacity
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEvent(string id, [FromBody] EventUpdateDto dto)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null || ev.IsDeleted) return NotFound(new { message = $"Event {id} not found" });

        if (!await User.IsOrgAdminAsync(_db, ev.OrganizationID))
        {
            return Forbid();
        }

        try
        {
            var result = await _eventService.UpdateEventAsync(id, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }

    // Endpoint #72: Soft-delete event and cascade soft-delete to sessions
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null || ev.IsDeleted) return NotFound(new { message = $"Event {id} not found" });

        if (!await User.IsOrgAdminAsync(_db, ev.OrganizationID))
        {
            return Forbid();
        }

        var deleted = await _eventService.SoftDeleteEventAsync(id);
        if (!deleted) return NotFound(new { message = $"Event {id} not found" });

        return Ok(new { message = "Event and associated sessions successfully soft-deleted" });
    }
}

