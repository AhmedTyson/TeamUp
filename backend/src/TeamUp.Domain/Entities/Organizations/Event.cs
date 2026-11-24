using TeamUp.Domain.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Domain.Entities;

public class Event : BaseEntity
{
    public int EventId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EventType Type { get; set; }
    public string Location { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Capacity { get; set; }
    public int OrganizationId { get; set; }
    public EventDeliveryMode DeliveryMode { get; set; } = EventDeliveryMode.InPerson;
    public bool IsPublic { get; set; } = true;
    public string? MeetingUrl { get; set; }
    public int? OrganizationUnitId { get; set; }
    public Organization Organization { get; set; } = null!;
    public OrganizationUnit? Unit { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<EventSession> Sessions { get; set; } = [];
}
