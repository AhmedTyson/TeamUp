using TeamUp.Domain.Common;

namespace TeamUp.Domain.Entities;

public class EventSession : BaseEntity
{
    public int SessionId { get; set; }
    public int EventId { get; set; }
    public int SessionNo { get; set; }
    public string Title { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Room { get; set; }
    public Event Event { get; set; } = null!;
}
