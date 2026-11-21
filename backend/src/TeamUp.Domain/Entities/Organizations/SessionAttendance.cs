using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class SessionAttendance : BaseEntity
{
    public int SessionId { get; set; }
    public int StudentId { get; set; }
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public VerificationMethod VerificationMethod { get; set; }
    public EventSession Session { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
