// Date: 2026-11-21
// Purpose: Request context abstraction and immutable audit logging service (STRIDE THR-R-01).

using System.Text.Json;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Domain.Entities;

namespace TeamUp.Application.Common.Security;

public interface IRequestContext
{
    int? ActorStudentId { get; }
    string? ActorEmail { get; }
    string? ActorRole { get; }
    string? ClientIp { get; }
}

public sealed class NullRequestContext : IRequestContext
{
    public int? ActorStudentId => null;
    public string? ActorEmail => null;
    public string? ActorRole => null;
    public string? ClientIp => null;
}

public interface IAuditLogService
{
    Task RecordAsync(
        string action,
        string entityType,
        string entityId,
        object? oldValues = null,
        object? newValues = null,
        int? actorStudentIdOverride = null,
        CancellationToken ct = default);
}

public sealed class AuditLogService(IAppDbContext db, IRequestContext requestContext) : IAuditLogService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task RecordAsync(
        string action,
        string entityType,
        string entityId,
        object? oldValues = null,
        object? newValues = null,
        int? actorStudentIdOverride = null,
        CancellationToken ct = default)
    {
        var actorId = actorStudentIdOverride ?? requestContext.ActorStudentId;
        var entry = new AuditLog
        {
            ActorStudentId = actorId is > 0 ? actorId : null,
            ActorEmail = requestContext.ActorEmail,
            Action = action.Trim(),
            EntityType = entityType.Trim(),
            EntityId = entityId.Trim(),
            OldValues = oldValues is null ? null : JsonSerializer.Serialize(oldValues, JsonOptions),
            NewValues = newValues is null ? null : JsonSerializer.Serialize(newValues, JsonOptions),
            ClientIp = requestContext.ClientIp,
            OccurredAt = DateTime.UtcNow
        };

        db.AuditLogs.Add(entry);
        await db.SaveChangesAsync(ct);
    }
}
