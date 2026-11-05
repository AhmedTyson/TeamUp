using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class Notification : BaseEntity
{
    public int NotificationId { get; set; }
    public int StudentId { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Student Student { get; set; } = null!;
}
