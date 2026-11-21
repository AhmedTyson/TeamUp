// Date: 2026-11-21
// Purpose: Immutable append-only audit log entity recording administrative and security-sensitive mutations (STRIDE THR-R-01).

using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

public class AuditLog : IAppendOnly
{
    public int AuditLogId { get; set; }
    public int? ActorStudentId { get; set; }
    public string? ActorEmail { get; set; }
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ClientIp { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public Student? ActorStudent { get; set; }
}
