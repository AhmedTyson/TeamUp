// Date: 2026-11-21
// Purpose: Platform administration endpoints for system metrics, guarded role updates, universal soft-delete recovery, and immutable audit logs (STRIDE THR-R-01, THR-E-03).

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Common;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Dedicated cross-cutting administration dashboard, role governance, audit logs, and soft-delete recovery.
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
    [ProducesOk<StudentSummaryDto>, ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> UpdateStudentRole(
        [FromRoute] int id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken ct)
    {
        var result = await adminService.UpdateStudentRoleAsync(id, request.Role, CurrentStudentId, ct);
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

    [HttpGet("audit-logs")]
    [ProducesOk<PagedResult<AuditLogDto>>]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] int? actorStudentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await adminService.GetAuditLogsAsync(action, entityType, actorStudentId, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }
}
