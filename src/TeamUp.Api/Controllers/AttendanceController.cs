using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamUp.Api.Auth;
using TeamUp.Api.Data;
using TeamUp.Api.Models.DTOs;
using TeamUp.Api.Services;

namespace TeamUp.Api.Controllers;

[ApiController]
[Route("api/v1/sessions/{id}/attendance")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly TeamUpDbContext _db;

    public AttendanceController(IAttendanceService attendanceService, TeamUpDbContext db)
    {
        _attendanceService = attendanceService;
        _db = db;
    }

    private async Task<string?> GetSessionOrgIdAsync(string sessionId)
    {
        var session = await _db.EventSessions
            .Include(s => s.Event)
            .FirstOrDefaultAsync(s => s.SessionID == sessionId && !s.IsDeleted && !s.Event.IsDeleted);

        return session?.Event.OrganizationID;
    }

    // Endpoint #80: List attendance for an event session
    [HttpGet]
    [ProducesResponseType(typeof(SessionAttendanceListResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAttendance(
        string id,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var orgId = await GetSessionOrgIdAsync(id);
        if (orgId == null) return NotFound(new { message = $"Active session {id} not found" });

        if (!await User.IsOrgAdminAsync(_db, orgId))
        {
            return Forbid();
        }

        var result = await _attendanceService.ListSessionAttendanceAsync(id, page, limit);
        return Ok(result);
    }

    // Endpoint #81: Verify student attendance
    [HttpPost]
    [ProducesResponseType(typeof(SessionAttendanceResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> VerifyAttendance(string id, [FromBody] AttendanceVerifyRequestDto dto)
    {
        var orgId = await GetSessionOrgIdAsync(id);
        if (orgId == null) return NotFound(new { message = $"Active session {id} not found" });

        if (!await User.IsOrgAdminAsync(_db, orgId))
        {
            return Forbid();
        }

        try
        {
            var result = await _attendanceService.VerifyAttendanceAsync(id, dto.StudentId, dto.VerificationMethod);
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

    // Endpoint #82: Revoke an attendance record
    [HttpDelete("{studentId}")]
    [ProducesResponseType(typeof(SessionAttendanceResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeAttendance(string id, string studentId)
    {
        var orgId = await GetSessionOrgIdAsync(id);
        if (orgId == null) return NotFound(new { message = $"Active session {id} not found" });

        if (!await User.IsOrgAdminAsync(_db, orgId))
        {
            return Forbid();
        }

        try
        {
            var result = await _attendanceService.RevokeAttendanceAsync(id, studentId);
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

