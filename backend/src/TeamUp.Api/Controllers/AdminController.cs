using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Dedicated cross-cutting administration dashboard, role governance, and soft-delete recovery.
/// </summary>
[Route("api/v1/admin")]
[Authorize(Roles = "PlatformAdmin")]
[ProducesForbidden]
public class AdminController(IAdminService adminService) : BaseApiController
{
    [HttpGet("dashboard")]
    [ProducesOk<AdminDashboardDto>]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await adminService.GetDashboardStatsAsync(ct);
        return OkResponse(result);
    }

    [HttpPatch("students/{id:int}/role")]
    [ProducesOk<StudentSummaryDto>, ProducesNotFound]
    public async Task<IActionResult> UpdateStudentRole(
        [FromRoute] int id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken ct)
    {
        var result = await adminService.UpdateStudentRoleAsync(id, request.Role, ct);
        return OkResponse(result, "Student role updated successfully.");
    }

    [HttpGet("soft-deleted")]
    [ProducesOk<IReadOnlyList<SoftDeletedItemDto>>]
    public async Task<IActionResult> GetSoftDeleted([FromQuery] string? entityType, CancellationToken ct)
    {
        var result = await adminService.GetSoftDeletedRecordsAsync(entityType, ct);
        return OkResponse(result);
    }

    [HttpPost("restore/{entityType}/{id:int}")]
    [ProducesOk, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> RestoreEntity(
        [FromRoute] string entityType,
        [FromRoute] int id,
        CancellationToken ct)
    {
        await adminService.RestoreEntityAsync(entityType, id, ct);
        return OkMessage($"{entityType} with ID {id} has been restored successfully.");
    }
}
