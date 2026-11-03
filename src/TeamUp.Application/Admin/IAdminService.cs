using TeamUp.Application.Auth;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Admin;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardStatsAsync(CancellationToken ct = default);
    Task<StudentSummaryDto> UpdateStudentRoleAsync(int studentId, UserRole newRole, CancellationToken ct = default);
    Task<IReadOnlyList<SoftDeletedItemDto>> GetSoftDeletedRecordsAsync(string? entityType, CancellationToken ct = default);
    Task RestoreEntityAsync(string entityType, int id, CancellationToken ct = default);
}
