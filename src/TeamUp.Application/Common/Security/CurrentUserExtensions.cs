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
}
