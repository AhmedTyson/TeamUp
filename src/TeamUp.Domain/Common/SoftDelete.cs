namespace TeamUp.Domain.Common;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

/// <summary>Rows of these entities can be inserted but never updated or deleted (ledgers).</summary>
public interface IAppendOnly;

public abstract class BaseEntity : ISoftDelete
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
