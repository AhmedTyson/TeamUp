using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Auth;
using TeamUp.Api.Data;
using TeamUp.Api.Models.DTOs;
using TeamUp.Api.Services;

namespace TeamUp.Api.Controllers;

[ApiController]
[Route("api/v1/events/{id}/registrations")]
[Authorize]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly TeamUpDbContext _db;

    public RegistrationsController(IRegistrationService registrationService, TeamUpDbContext db)
    {
        _registrationService = registrationService;
        _db = db;
    }

    // Endpoint #77: List applications/registrations for an event
    [HttpGet]
    [ProducesResponseType(typeof(EventRegistrationListResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRegistrations(
        string id,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null || ev.IsDeleted) return NotFound(new { message = $"Event {id} not found" });

        if (!await User.IsOrgAdminAsync(_db, ev.OrganizationID))
        {
            return Forbid();
        }

        var result = await _registrationService.ListRegistrationsAsync(id, status, page, limit);
        return Ok(result);
    }

    // Endpoint #78: Submit registration for an event
    [HttpPost]
    [ProducesResponseType(typeof(EventRegistrationResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterForEvent(string id)
    {
        // Rule: extract student ID strictly from authenticated identity
        var studentId = User.GetStudentId();
        if (string.IsNullOrEmpty(studentId)) return Unauthorized();

        try
        {
            var result = await _registrationService.RegisterForEventAsync(id, studentId);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // Endpoint #79: Cancel registration
    [HttpDelete]
    [ProducesResponseType(typeof(EventRegistrationResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelRegistration(string id, [FromQuery] string? studentId)
    {
        var callerId = User.GetStudentId();
        if (string.IsNullOrEmpty(callerId)) return Unauthorized();

        var ev = await _db.Events.FindAsync(id);
        if (ev == null || ev.IsDeleted) return NotFound(new { message = $"Event {id} not found" });

        var targetStudentId = studentId ?? callerId;

        // If cancelling for someone else, caller must be OrgAdmin of the event's organization
        if (targetStudentId != callerId)
        {
            if (!await User.IsOrgAdminAsync(_db, ev.OrganizationID))
            {
                return Forbid();
            }
        }

        try
        {
            var result = await _registrationService.CancelRegistrationAsync(id, targetStudentId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

