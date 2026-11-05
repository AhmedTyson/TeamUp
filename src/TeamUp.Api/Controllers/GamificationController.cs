using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common;
using TeamUp.Application.Gamification;

namespace TeamUp.Api.Controllers;

[Route("api/v1")]
[Authorize]
public class GamificationController(IGamificationService gamificationService) : BaseApiController
{
    [HttpGet("students/{id:int}/score")]
    [ProducesOk<StudentScoreDto>, ProducesNotFound]
    public async Task<IActionResult> GetStudentScore([FromRoute] int id, CancellationToken ct)
    {
        var result = await gamificationService.GetStudentScoreAsync(id, ct);
        return OkResponse(result);
    }

    [HttpGet("students/{id:int}/score-transactions")]
    [ProducesOk<PagedResult<ScoreTransactionDto>>, ProducesNotFound]
    public async Task<IActionResult> GetScoreTransactions(
        [FromRoute] int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await gamificationService.GetScoreTransactionsAsync(id, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }

    [HttpPost("score-transactions")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesCreated<ScoreTransactionDto>]
    [ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> AddScoreTransaction([FromBody] CreateScoreTransactionRequest request, CancellationToken ct)
    {
        var result = await gamificationService.AddScoreTransactionAsync(request.StudentId, request.Points, request.Reason, request.SourceType, ct);
        return CreatedResponse(result, "Score transaction created successfully.");
    }
}
