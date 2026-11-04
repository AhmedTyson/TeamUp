using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Auth;
using TeamUp.Api.Models.DTOs;
using TeamUp.Api.Services;

namespace TeamUp.Api.Controllers;

[ApiController]
[Authorize]
public class GamificationController : ControllerBase
{
    private readonly IGamificationService _gamificationService;

    public GamificationController(IGamificationService gamificationService)
    {
        _gamificationService = gamificationService;
    }

    private bool IsOwnerOrAdmin(string targetStudentId)
    {
        if (User.IsPlatformAdmin()) return true;
        return User.GetStudentId() == targetStudentId;
    }

    // Endpoint #83: Retrieve total points, level, and progress
    [HttpGet("api/v1/students/{id}/score")]
    [ProducesResponseType(typeof(StudentScoreResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentScore(string id)
    {
        if (!IsOwnerOrAdmin(id)) return Forbid();

        try
        {
            var result = await _gamificationService.GetStudentScoreAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Endpoint #84: Retrieve score transaction history
    [HttpGet("api/v1/students/{id}/score-transactions")]
    [ProducesResponseType(typeof(ScoreTransactionListResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScoreTransactions(
        string id,
        [FromQuery] string? sourceType,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        if (!IsOwnerOrAdmin(id)) return Forbid();

        var result = await _gamificationService.GetScoreTransactionsAsync(id, sourceType, page, limit);
        return Ok(result);
    }

    // Endpoint #85: Create manual bonus or adjustment
    [HttpPost("api/v1/score-transactions")]
    [ProducesResponseType(typeof(ScoreTransactionResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateScoreTransaction([FromBody] ScoreTransactionCreateDto dto)
    {
        if (!User.IsPlatformAdmin()) return Forbid();

        if (dto.Points == 0)
        {
            return BadRequest(new { message = "Points must not be zero" });
        }

        try
        {
            var result = await _gamificationService.AddScoreTransactionAsync(
                dto.StudentId,
                dto.Points,
                dto.Reason,
                "ManualAdjustment",
                User.GetStudentId()
            );
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

