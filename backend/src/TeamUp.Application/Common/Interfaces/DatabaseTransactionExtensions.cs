// Date: 2026-11-26
// Purpose: Execution-strategy-safe transaction helpers ensuring SqlServerRetryingExecutionStrategy compatibility and post-commit domain event dispatch.

using System.Data;
using Microsoft.EntityFrameworkCore;

namespace TeamUp.Application.Common.Interfaces;

public static class DatabaseTransactionExtensions
{
    extension(IAppDbContext db)
    {
        public async Task ExecuteInTransactionAsync(
            IsolationLevel isolationLevel,
            Func<CancellationToken, Task> action,
            CancellationToken ct = default)
        {
            await db.ExecuteInTransactionAsync<object?>(
                isolationLevel,
                async token =>
                {
                    await action(token);
                    return null;
                },
                ct);
        }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            IsolationLevel isolationLevel,
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken ct = default)
        {
            var strategy = db.Database.CreateExecutionStrategy();
            var executionOutcome = await strategy.ExecuteAsync<object?, TResult>(
                state: null,
                operation: async (_, token) =>
                {
                    db.ClearDeferredDomainEvents();
                    await using var transaction = await db.Database.BeginTransactionAsync(isolationLevel, token);
                    db.BeginDeferringDomainEvents();
                    try
                    {
                        var outcome = await action(token);
                        await transaction.CommitAsync(token);
                        return outcome;
                    }
                    catch
                    {
                        db.ClearDeferredDomainEvents();
                        await transaction.RollbackAsync(CancellationToken.None);
                        throw;
                    }
                },
                verifySucceeded: null,
                cancellationToken: ct);

            // Dispatch buffered domain events strictly after commit so external side effects (e.g. SMTP)
            // never hold row/serializable DB locks or abort committed writes.
            await db.DispatchDeferredDomainEventsAsync(ct);
            return executionOutcome;
        }
    }
}
