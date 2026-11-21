// Date: 2026-11-21
// Purpose: Canonical scoped organization roles used for multi-tenant resource authorization (STRIDE THR-T-01 / THR-E-01).

namespace TeamUp.Domain.Common;

public static class OrganizationRoles
{
    public const string Admin = "Admin";
    public const string OrgAdmin = "OrgAdmin";
    public const string President = "President";
    public const string VicePresident = "Vice President";
    public const string Executive = "Executive";
    public const string Member = "Member";

    private static readonly HashSet<string> AdministrativeRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        Admin,
        OrgAdmin,
        President,
        VicePresident,
        Executive
    };

    public static bool IsAdminRole(string? role) =>
        !string.IsNullOrWhiteSpace(role) && AdministrativeRoles.Contains(role.Trim());
}
