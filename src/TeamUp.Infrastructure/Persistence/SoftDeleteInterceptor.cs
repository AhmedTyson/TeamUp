using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TeamUp.Domain.Common;

namespace TeamUp.Infrastructure.Persistence;

/// <summary>
/// Turns physical deletes into soft deletes and blocks changes to append-only ledgers.
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Apply(DbContext? context)
    {
        if (context is null) return;
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is IAppendOnly && entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException(
                    $"{entry.Metadata.ClrType.Name} is append-only: it cannot be updated or deleted. Insert a compensating row instead.");

            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete soft)
            {
                entry.State = EntityState.Modified;
                soft.DeletedAt = now;
                MarkOnly(entry, nameof(ISoftDelete.DeletedAt));
            }
        }
    }

    // Deleted entries are reset to Modified; make sure only the soft-delete columns are written.
    private static void MarkOnly(EntityEntry entry, params string[] props)
    {
        foreach (var p in entry.Properties)
            p.IsModified = props.Contains(p.Metadata.Name);
    }
}
