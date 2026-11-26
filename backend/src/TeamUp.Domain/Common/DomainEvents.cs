// Date: 2026-11-26
// Purpose: Domain event abstractions and deterministic event base capturing creation timestamp.

namespace TeamUp.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn => DateTime.UtcNow;
}

/// <summary>
/// Captures a deterministic UTC timestamp when a domain event record is instantiated.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent domainEvent);
    void RemoveDomainEvent(IDomainEvent domainEvent);
    void ClearDomainEvents();
}
