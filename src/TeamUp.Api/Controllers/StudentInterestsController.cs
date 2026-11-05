using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Extracurricular and technical interest tags.
/// </summary>
[Route("api/v1/students")]
[Authorize]
public class StudentInterestsController(IStudentInterestService interestService) : BaseApiController
{
    [HttpGet("{id:int}/interests")]
    [ProducesOk<IReadOnlyList<string>>]
    public async Task<IActionResult> GetInterests([FromRoute] int id, CancellationToken ct)
    {
        var result = await interestService.GetInterestsAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost("me/interests")]
    [ProducesOk]
    public async Task<IActionResult> AddInterest([FromBody] AddInterestRequest request, CancellationToken ct)
    {
        await interestService.AddInterestAsync(CurrentStudentId, request.Interest, ct);
        return OkMessage("Interest added successfully.");
    }

    [HttpDelete("me/interests/{interest}")]
    [ProducesNoContent]
    public async Task<IActionResult> RemoveInterest([FromRoute] string interest, CancellationToken ct)
    {
        await interestService.RemoveInterestAsync(CurrentStudentId, interest, ct);
        return NoContentResponse();
    }
}
