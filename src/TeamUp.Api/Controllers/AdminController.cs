using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Models;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Dedicated cross-cutting administration dashboard, role governance, and soft-delete recovery.
/// </summary>
[Route("api/v1/admin")]
[Authorize(Roles = "PlatformAdmin")]
public class AdminController(IAdminService adminService) : BaseApiController
{
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(BaseResponse<AdminDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await adminService.GetDashboardStatsAsync(ct);
        return OkResponse(result);
    }

    [HttpPatch("students/{id:int}/role")]
    [ProducesResponseType(typeof(BaseResponse<StudentSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudentRole(
        [FromRoute] int id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken ct)
    {
        var result = await adminService.UpdateStudentRoleAsync(id, request.Role, ct);
        return OkResponse(result, "Student role updated successfully.");
    }

    [HttpGet("soft-deleted")]
    [ProducesResponseType(typeof(BaseResponse<IReadOnlyList<SoftDeletedItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSoftDeleted([FromQuery] string? entityType, CancellationToken ct)
    {
        var result = await adminService.GetSoftDeletedRecordsAsync(entityType, ct);
        return OkResponse(result);
    }

    [HttpPost("restore/{entityType}/{id:int}")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RestoreEntity(
        [FromRoute] string entityType,
        [FromRoute] int id,
        CancellationToken ct)
    {
        await adminService.RestoreEntityAsync(entityType, id, ct);
        return OkMessage($"{entityType} with ID {id} has been restored successfully.");
    }
}
