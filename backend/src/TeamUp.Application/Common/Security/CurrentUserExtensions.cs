// Date: 2026-11-21
// Purpose: ClaimsPrincipal extraction helpers for identity, role, SecurityStamp, and scoped organization admin claims.

using System.Security.Claims;

namespace TeamUp.Application.Common.Security;

public static class CurrentUserExtensions
{
    public static int GetStudentId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : 0;
    }

    public static string? GetUserRole(this ClaimsPrincipal principal) =>
        principal.FindFirst(ClaimTypes.Role)?.Value ?? principal.FindFirst("role")?.Value;

    public static string? GetEmail(this ClaimsPrincipal principal) =>
        principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst("email")?.Value;

    public static Guid? GetSecurityStamp(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(TeamUpClaimTypes.SecurityStamp)?.Value;
        return Guid.TryParse(value, out var stamp) ? stamp : null;
    }

    public static IReadOnlyList<int> GetAdminOrganizationIds(this ClaimsPrincipal principal) =>
        principal.FindAll(TeamUpClaimTypes.OrgAdmin)
            .Select(c => int.TryParse(c.Value, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
}
