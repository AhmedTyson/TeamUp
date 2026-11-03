namespace TeamUp.Domain.Common;

public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }

    /// <summary>Convenience helper: true when DeletedAt has a timestamp.</summary>
    bool IsDeleted => DeletedAt.HasValue;
}

/// <summary>Rows of these entities can be inserted but never updated or deleted (ledgers).</summary>
public interface IAppendOnly;

public abstract class BaseEntity : ISoftDelete, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public DateTime? DeletedAt { get; set; }

    /// <summary>Convenience helper: true when DeletedAt has a timestamp (unmapped).</summary>
    public bool IsDeleted => DeletedAt.HasValue;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
