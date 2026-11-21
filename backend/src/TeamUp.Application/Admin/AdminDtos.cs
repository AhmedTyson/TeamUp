using TeamUp.Application.Auth;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Admin;

public sealed record AdminDashboardDto(
    int ActiveStudentsCount,
    int ActiveUniversitiesCount,
    int ActiveFacultiesCount,
    int SoftDeletedStudentsCount,
    int SoftDeletedUniversitiesCount,
    int SoftDeletedFacultiesCount,
    IReadOnlyDictionary<string, int> RoleDistribution
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
