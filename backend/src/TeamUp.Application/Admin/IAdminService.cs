// Date: 2026-11-21
// Purpose: Platform admin service contract covering dashboard metrics, guarded role governance, universal restore, and audit log querying.

using TeamUp.Application.Auth;
using TeamUp.Application.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Admin;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardStatsAsync(CancellationToken ct = default);
    Task<StudentSummaryDto> UpdateStudentRoleAsync(int studentId, UserRole newRole, int actorStudentId = 0, CancellationToken ct = default);
    Task<IReadOnlyList<SoftDeletedItemDto>> GetSoftDeletedRecordsAsync(string? entityType, CancellationToken ct = default);
    Task RestoreEntityAsync(string entityType, int id, CancellationToken ct = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        string? action,
        string? entityType,
        int? actorStudentId,
        PageRequest page,
        CancellationToken ct = default);
}
