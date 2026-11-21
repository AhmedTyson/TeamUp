// Date: 2026-11-21
// Purpose: ASP.NET Core IRequestContext adapter capturing authenticated actor metadata and sanitized RemoteIpAddress for immutable audit logging (STRIDE THR-R-01, THR-S-02).

using TeamUp.Application.Common.Security;

namespace TeamUp.Api.Middleware;

public sealed class HttpRequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
{
    public int? ActorStudentId
    {
        get
        {
            var id = httpContextAccessor.HttpContext?.User.GetStudentId() ?? 0;
            return id > 0 ? id : null;
        }
    }

    public string? ActorEmail => httpContextAccessor.HttpContext?.User.GetEmail();

    public string? ActorRole => httpContextAccessor.HttpContext?.User.GetUserRole();

    public string? ClientIp => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
