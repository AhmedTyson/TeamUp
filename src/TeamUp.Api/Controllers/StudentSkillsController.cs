using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Skills;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Student claimed skills and proficiency ratings.
/// </summary>
[Route("api/v1/students")]
[Authorize]
public class StudentSkillsController(ISkillService skillService) : BaseApiController
{
    [HttpGet("{id:int}/skills")]
    [ProducesResponseType(typeof(BaseResponse<IReadOnlyList<StudentSkillDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentSkills([FromRoute] int id, CancellationToken ct)
    {
        var result = await skillService.GetStudentSkillsAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPut("me/skills/{skillId:int}")]
    [ProducesResponseType(typeof(BaseResponse<StudentSkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClaimSkill([FromRoute] int skillId, [FromBody] ClaimSkillRequest request, CancellationToken ct)
    {
        var result = await skillService.ClaimSkillAsync(CurrentStudentId, skillId, request, ct);
        return OkResponse(result, "Skill claimed successfully.");
    }

    [HttpDelete("me/skills/{skillId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveClaimedSkill([FromRoute] int skillId, CancellationToken ct)
    {
        await skillService.RemoveClaimedSkillAsync(CurrentStudentId, skillId, ct);
        return NoContentResponse();
    }
}
