using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using TeamUp.Api.Controllers.Common;
using TeamUp.Api.Extensions;
using TeamUp.Application.Gamification;

namespace TeamUp.Api.Controllers;

[Route("api/v1")]
[Authorize]
public sealed class BadgesController(
    IBadgeService badgeService,
    IOutputCacheStore outputCacheStore) : BaseApiController
{
    [HttpGet("badges")]
    [OutputCache(PolicyName = OpenApiAndCachingExtensions.CatalogOutputCachePolicyName, Tags = [CatalogOutputCacheTags.Badges])]
    [ProducesOk<IReadOnlyList<BadgeDto>>]
    public async Task<IActionResult> GetBadgeCatalog(CancellationToken ct)
    {
        var result = await badgeService.GetBadgeCatalogAsync(ct);
        return OkResponse(result);
    }

    [HttpGet("students/{id:int}/badges")]
    [ProducesOk<IReadOnlyList<StudentBadgeDto>>, ProducesNotFound]
    public async Task<IActionResult> GetStudentBadges([FromRoute] int id, CancellationToken ct)
    {
        var result = await badgeService.GetStudentBadgesAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPost("badges")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesCreated<BadgeDto>]
    [ProducesForbidden, ProducesConflict]
    public async Task<IActionResult> CreateBadge([FromBody] CreateBadgeRequest request, CancellationToken ct)
    {
        var result = await badgeService.CreateBadgeAsync(request, ct);
        await outputCacheStore.EvictByTagAsync(CatalogOutputCacheTags.Badges, ct);
        return CreatedResponse(result, "Badge created successfully in master catalog.");
    }

    [HttpPut("badges/{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesOk<BadgeDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateBadge([FromRoute] int id, [FromBody] UpdateBadgeRequest request, CancellationToken ct)
    {
        var result = await badgeService.UpdateBadgeAsync(id, request, ct);
        await outputCacheStore.EvictByTagAsync(CatalogOutputCacheTags.Badges, ct);
        return OkResponse(result, "Badge updated successfully.");
    }

    [HttpDelete("badges/{id:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesNoContent, ProducesForbidden, ProducesNotFound]
    public async Task<IActionResult> DeleteBadge([FromRoute] int id, CancellationToken ct)
    {
        await badgeService.DeleteBadgeAsync(id, ct);
        await outputCacheStore.EvictByTagAsync(CatalogOutputCacheTags.Badges, ct);
        return NoContentResponse();
    }

    [HttpPost("students/{id:int}/badges/{badgeId:int}")]
    [Authorize(Roles = "PlatformAdmin")]
    [ProducesCreated<StudentBadgeDto>]
    [ProducesForbidden, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> ManualAwardBadge([FromRoute] int id, [FromRoute] int badgeId, CancellationToken ct)
    {
        var result = await badgeService.ManualAwardBadgeAsync(id, badgeId, ct);
        return CreatedResponse(result, "Badge awarded successfully to student.");
    }
}
