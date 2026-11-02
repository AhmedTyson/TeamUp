namespace TeamUp.Domain.Common;

public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }

    /// <summary>Convenience helper: true when DeletedAt has a timestamp.</summary>
    bool IsDeleted => DeletedAt.HasValue;
}

/// <summary>Rows of these entities can be inserted but never updated or deleted (ledgers).</summary>
public interface IAppendOnly;

public abstract class BaseEntity : ISoftDelete
{
    public DateTime? DeletedAt { get; set; }

    /// <summary>Convenience helper: true when DeletedAt has a timestamp (unmapped).</summary>
    public bool IsDeleted => DeletedAt.HasValue;
}
