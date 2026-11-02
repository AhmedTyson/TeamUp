# TeamUp — Single-Column Soft Delete Architectural Specification

## 1. Core Principle (Single Column Architecture)
Physical deletion (`DELETE FROM <table>`) is **strictly prohibited** in TeamUp application code. All entity removals are executed via a **single audit column**: `DeletedAt DATETIME2 NULL`.

```
Record State:
Active (Live):         DeletedAt IS NULL
Soft-Deleted:          DeletedAt IS NOT NULL (UTC Timestamp)

Transition:
DELETE /resource/{id}  ->  DeletedAt = DateTime.UtcNow  ->  Returns 204 No Content
POST /resource/{id}/restore -> DeletedAt = NULL         ->  Returns 200 OK
```

### Why a Single Column Instead of Two?
1. **Eliminates Desynchronization:** Using both `IsDeleted` and `DeletedAt` creates a dangerous failure mode where `IsDeleted = 1` while `DeletedAt IS NULL` (or vice-versa). A single nullable column is mathematically atomic.
2. **Reduced Storage Footprint:** Eliminates redundant bit flags across tens of millions of audit rows.
3. **Natural Audit Timestamp:** The exact moment of deletion is captured without needing secondary log joins.

---

## 2. Infrastructure Implementation

### A. Domain Contracts (`ISoftDelete` & `BaseEntity`)
Every business entity inherits from `BaseEntity`:
```csharp
public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }

    /// <summary>Unmapped convenience helper: true when DeletedAt has a timestamp.</summary>
    bool IsDeleted => DeletedAt.HasValue;
}

public abstract class BaseEntity : ISoftDelete
{
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}
```
`ScoreTransaction` (append-only ledger) does not implement `ISoftDelete`; it implements `IAppendOnly`.

### B. Transparent Interception (`SoftDeleteInterceptor`)
Registered in EF Core pipeline (`AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>())`):
1. Intercepts any entity with `EntityState.Deleted` that implements `ISoftDelete`.
2. Mutates state to `EntityState.Modified`.
3. Sets `soft.DeletedAt = DateTime.UtcNow`.
4. Marks **only** `DeletedAt` as modified in EF ChangeTracker, ensuring business columns are never accidentally altered on deletion.
5. If an `IAppendOnly` entity is flagged as `Modified` or `Deleted`, the interceptor throws `InvalidOperationException`.

### C. Automatic Query Filtering
In `AppDbContext.OnModelCreating`, every entity implementing `ISoftDelete` receives a global query filter:
```csharp
modelBuilder.Entity(entity.ClrType).HasQueryFilter(e => e.DeletedAt == null);
```
All standard queries (`_db.Students`, `_db.Projects`, etc.) automatically retrieve active records (`DeletedAt IS NULL`).

### D. Administrative Queries & Restoration
```csharp
// Query soft-deleted entities
var deleted = await _db.Students.IgnoreQueryFilters()
    .Where(s => s.DeletedAt != null)
    .ToListAsync();

// Restore a soft-deleted record
entity.DeletedAt = null;
await _db.SaveChangesAsync();
```

---

## 3. Database Constraints Interaction

### Why All Foreign Keys Use `ON DELETE NO ACTION`
Physical database cascades (`ON DELETE CASCADE`, `ON DELETE SET NULL`) conflict fundamentally with soft deletion:
- If a physical delete was issued, database cascades would bypass EF Core and permanently wipe child records.
- **Rule:** Cascade soft-deletions (e.g., deleting a Project soft-deletes its uncompleted Tasks) are performed explicitly at the application/service level within a single database transaction.

### Filtered Unique Indexes
Standard SQL `UNIQUE` constraints would permanently lock an identifier (like an email or skill name) even after the account is deleted.

**Solution:** All unique constraints are defined as SQL Server **Filtered Unique Indexes** targeting only active records:
```sql
CREATE UNIQUE INDEX [IX_Student_Email] ON [Student] ([Email]) WHERE [DeletedAt] IS NULL;
CREATE UNIQUE INDEX [IX_Skill_Name] ON [Skill] ([Name]) WHERE [DeletedAt] IS NULL;
CREATE UNIQUE INDEX [IX_Organization_Name] ON [Organization] ([Name]) WHERE [DeletedAt] IS NULL;
CREATE UNIQUE INDEX [IX_StudentProfile_StudentID] ON [StudentProfile] ([StudentID]) WHERE [DeletedAt] IS NULL;
```
Once a record is soft-deleted (`DeletedAt IS NOT NULL`), the identifier is immediately freed for reuse.

### Re-Applying to Teams & Organizations
`ProjectApplication` and `MembershipApplication` use surrogate `ApplicationID` primary keys and a specialized filtered unique index:
```sql
CREATE UNIQUE INDEX [IX_ProjectApplication_ProjectID_StudentID] 
ON [ProjectApplication] ([ProjectID], [StudentID]) 
WHERE [DeletedAt] IS NULL AND [Status] = 'Pending';
```
This guarantees that a student can only have **one open application** at a time, but can cleanly submit a new application if an earlier one was `Rejected` or `Withdrawn`.
