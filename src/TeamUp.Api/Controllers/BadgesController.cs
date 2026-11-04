using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Auth;
using TeamUp.Api.Models.DTOs;
using TeamUp.Api.Services;

namespace TeamUp.Api.Controllers;

[ApiController]
[Authorize]
public class BadgesController : ControllerBase
{
    private readonly IBadgeService _badgeService;

    public BadgesController(IBadgeService badgeService)
    {
        _badgeService = badgeService;
    }

    private bool IsOwnerOrAdmin(string targetStudentId)
    {
        if (User.IsPlatformAdmin()) return true;
        return User.GetStudentId() == targetStudentId;
    }

    // Endpoint #86: List active badge catalog
    [HttpGet("api/v1/badges")]
    [ProducesResponseType(typeof(List<BadgeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListBadges()
    {
        var result = await _badgeService.GetBadgeCatalogAsync();
        return Ok(result);
    }

    // Endpoint #87: Create a badge
    [HttpPost("api/v1/badges")]
    [ProducesResponseType(typeof(BadgeResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBadge([FromBody] BadgeCreateDto dto)
    {
        if (!User.IsPlatformAdmin()) return Forbid();

        try
        {
            var result = await _badgeService.CreateBadgeAsync(dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // Endpoint #88: Update badge
    [HttpPut("api/v1/badges/{id}")]
    [ProducesResponseType(typeof(BadgeResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateBadge(string id, [FromBody] BadgeUpdateDto dto)
    {
        if (!User.IsPlatformAdmin()) return Forbid();

        var result = await _badgeService.UpdateBadgeAsync(id, dto);
        if (result == null) return NotFound(new { message = $"Badge {id} not found" });

        return Ok(result);
    }

    // Endpoint #89: Soft-delete badge
    [HttpDelete("api/v1/badges/{id}")]
    public async Task<IActionResult> DeleteBadge(string id)
    {
        if (!User.IsPlatformAdmin()) return Forbid();

        var success = await _badgeService.SoftDeleteBadgeAsync(id);
        if (!success) return NotFound(new { message = $"Badge {id} not found" });

        return Ok(new { message = "Badge successfully soft-deleted" });
    }

    // Endpoint #90: List student badges
    [HttpGet("api/v1/students/{id}/badges")]
    [ProducesResponseType(typeof(List<StudentBadgeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentBadges(string id)
    {
        if (!IsOwnerOrAdmin(id)) return Forbid();

        var result = await _badgeService.GetStudentBadgesAsync(id);
        return Ok(result);
    }

    // Endpoint #91: Manually award badge
    [HttpPost("api/v1/students/{id}/badges/{badgeId}")]
    [ProducesResponseType(typeof(StudentBadgeResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AwardBadge(string id, string badgeId)
    {
        if (!User.IsPlatformAdmin()) return Forbid();

        try
        {
            var result = await _badgeService.ManualAwardBadgeAsync(id, badgeId);
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
}

