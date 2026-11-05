using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Skills;

namespace TeamUp.Api.Controllers;

[Route("api/v1/skills")]
[Authorize]
public class SkillsController(ISkillService skillService) : BaseApiController
{
    [HttpGet]
    [ProducesOk<IReadOnlyList<SkillDto>>]
    public async Task<IActionResult> GetSkills(
        [FromQuery] string? category,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var result = await skillService.GetSkillsAsync(category, search, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:int}")]
    [ProducesOk<SkillDto>, ProducesNotFound]
    public async Task<IActionResult> GetSkillById([FromRoute] int id, CancellationToken ct)
    {
        var result = await skillService.GetSkillByIdAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesCreated<SkillDto>]
    [ProducesForbidden, ProducesConflict]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillRequest request, CancellationToken ct)
    {
        var result = await skillService.CreateSkillAsync(request, ct);
        return CreatedResponse(nameof(GetSkillById), new { id = result.SkillId }, result, "Skill added to master catalog successfully.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesOk<SkillDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateSkill([FromRoute] int id, [FromBody] UpdateSkillRequest request, CancellationToken ct)
    {
        var result = await skillService.UpdateSkillAsync(id, request, ct);
        return OkResponse(result, "Skill updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> DeleteSkill([FromRoute] int id, CancellationToken ct)
    {
        await skillService.DeleteSkillAsync(id, ct);
        return NoContentResponse();
    }
}
