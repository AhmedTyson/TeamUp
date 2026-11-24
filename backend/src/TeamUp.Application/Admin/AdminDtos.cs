// Date: 2026-11-21
// Purpose: Platform administration DTOs including immutable audit log response contract (STRIDE THR-R-01).

using TeamUp.Application.Auth;
using TeamUp.Application.Gamification;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Admin;

public sealed record AdminDashboardDto(
    int ActiveStudentsCount,
    int ActiveUniversitiesCount,
    int ActiveFacultiesCount,
    int SoftDeletedStudentsCount,
    int SoftDeletedUniversitiesCount,
    int SoftDeletedFacultiesCount,
    IReadOnlyDictionary<string, int> RoleDistribution,
    int PendingOrganizationRequestsCount = 0
);

public sealed record UpdateUserRoleRequest(
    UserRole Role
);

public sealed record SoftDeletedItemDto(
    int Id,
    string EntityType,
    string DisplayName,
    DateTime DeletedAt
);

public sealed record AuditLogDto(
    int AuditLogId,
    int? ActorStudentId,
    string? ActorEmail,
    string Action,
    string EntityType,
    string EntityId,
    string? OldValues,
    string? NewValues,
    string? ClientIp,
    DateTime OccurredAt
);

public sealed record SendAdminReportRequest(
    string? RecipientEmail = null,
    string? ReportPeriod = null
);

public sealed record AdminLeaderboardDigestCommand(
    string Mode = "TestToMe",
    string? PeriodLabel = null,
    IReadOnlyList<CampusNewsItem>? NewsItems = null,
    bool DryRun = false
);

public sealed record AdminProjectMatchDigestCommand(
    string Mode = "TestToMe",
    int MaxMatchesPerStudent = 4,
    int MinMatchPercentage = 20,
    bool DryRun = false
);
