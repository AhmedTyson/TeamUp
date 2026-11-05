using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

public class StudentBadge : BaseEntity
{
    public int StudentId { get; set; }
    public int BadgeId { get; set; }
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    public Student Student { get; set; } = null!;
    public Badge Badge { get; set; } = null!;
}
