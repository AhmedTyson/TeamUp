using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class EventRegistration : BaseEntity
{
    public int EventId { get; set; }
    public int StudentId { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;
    public Event Event { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
