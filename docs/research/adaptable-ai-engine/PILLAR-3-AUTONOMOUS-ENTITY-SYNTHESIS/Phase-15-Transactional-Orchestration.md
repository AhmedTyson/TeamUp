# Phase 15: Cross-Entity Transactional Rollback & Compensation

**Pillar:** III — Autonomous Entity Synthesis & Execution  
**Module:** `TeamUp.AiGateway.Orchestration`  
**Target:** ACID Transaction Boundaries & Cloud Asset Compensating Workflows  

---

## 1. Problem & Architectural Challenge
When an autonomous AI pipeline executes a complex multi-entity action:
1. It uploads a processed flyer WebP to Cloud Blob Storage.
2. It inserts an `Event` record.
3. It inserts 3 `EventSession` records.
4. It attaches 2 `Skill` tags.

If step 3 fails because of a room scheduling collision, what happens?
- Without transactional guarantees, orphaned `Event` and partial `EventSession` rows pollute the database.
- The uploaded blob remains stranded in cloud storage, permanently accumulating storage costs.

**Requirement:** An **Atomic Orchestration Pipeline** that wraps database mutations in ACID transactions and registers **Compensating Actions** for external cloud resources upon failure.

---

## 2. Research & Saga / Unit-of-Work Architecture

### Dual-State Coordination (Database + Cloud Storage)
*Citation: Garcia-Molina & Salem (1987), "Sagas"; Fowler (2002), "Patterns of Enterprise Application Architecture".*

```
[Start Autonomous Action]
       │
       ├──► 1. Upload Cloud Asset (Staging Container)
       │       └── Compensating Action Registered: DeleteBlob(StagingUrl)
       │
       ├──► 2. Begin EF Core ExecutionStrategy Transaction
       │       ├── Insert Event
       │       ├── Insert Sessions
       │       └── Insert Skills
       │
       ├──► 3. If Exception / Validation Error:
       │       ├── Rollback SQL Transaction (Database untouched)
       │       └── Execute Compensating Actions (Delete Staged Blob)
       │
       └──► 4. If Success:
               ├── Commit SQL Transaction
               ├── Move Staged Blob to Permanent Storage
               └── Discard Compensating Action
```

---

## 3. Concrete Implementation (C# / Transactional Coordinator)

```csharp
namespace TeamUp.Application.Ai.Orchestration;

public class AutonomousActionCoordinator(
    AppDbContext db,
    IBlobStorageService blobService,
    ILogger<AutonomousActionCoordinator> logger) : IAutonomousActionCoordinator
{
    public async Task<TResult> ExecuteWithCompensationAsync<TResult>(
        Func<CancellationToken, Task<(TResult Result, List<string> CreatedBlobUrls)>> action,
        CancellationToken ct)
    {
        var createdBlobs = new List<string>();

        // Execution strategy ensures resilience against transient SQL connection drops
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            try
            {
                var (result, blobs) = await action(ct);
                createdBlobs.AddRange(blobs);

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Autonomous execution aborted. Rolling back database and cleaning up {Count} cloud assets.", createdBlobs.Count);

                await transaction.RollbackAsync(ct);

                // Execute Compensating Actions: purge uploaded cloud assets
                foreach (var blobUrl in createdBlobs)
                {
                    try
                    {
                        await blobService.DeleteBlobIfExistsAsync(blobUrl, CancellationToken.None);
                    }
                    catch (Exception blobEx)
                    {
                        logger.LogError(blobEx, "Failed to purge compensation asset {Url}", blobUrl);
                    }
                }

                throw; // Rethrow mapped domain exception
            }
        });
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Network Timeout during Commit:** If the client disconnects while transaction commits: EF Core execution strategy ensures database reaches consistent state; client reconnects and queries session draft via Redis key.
- **Poison Messages:** Any autonomous synthesis attempting more than 3 failed compensation cycles is routed to an admin Dead-Letter Queue (DLQ) with raw payload telemetry.
