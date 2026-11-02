# TeamUp — Soft Delete Architectural Specification

## 1. Core Principle
Physical deletion (`DELETE FROM <table>`) is **strictly prohibited** in TeamUp application code. All entity removals are executed via **soft delete**:

```
State Change:
IsDeleted: 0 -> 1
DeletedAt: NULL -> UTC Timestamp (DATETIME2)
```

The HTTP `DELETE` verb returns `204 No Content` upon successful soft-deletion.

---

## 2. Infrastructure Implementation

### A. Domain Contracts
- `ISoftDelete`: implemented by `BaseEntity`. Every business entity inherits this contract, providing:
  - `bool IsDeleted { get; set; }`
  - `DateTime? DeletedAt { get; set; }`
- `IAppendOnly`: marker interface for immutable audit ledgers (e.g., `ScoreTransaction`). These records have **no** soft delete; they can never be modified or removed.

### B. Transparent Interception (`SoftDeleteInterceptor`)
Registered in EF Core pipeline (`AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>())`):
1. Intercepts any entity with `EntityState.Deleted` that implements `ISoftDelete`.
2. Mutates state to `EntityState.Modified`.
3. Sets `IsDeleted = true` and `DeletedAt = DateTime.UtcNow`.
4. Marks **only** `IsDeleted` and `DeletedAt` as modified properties to prevent accidental side-effect mutations on business fields.
5. If an `IAppendOnly` entity is flagged as `Modified` or `Deleted`, the interceptor throws an `InvalidOperationException` to abort the transaction.

### C. Automatic Query Filtering
In `AppDbContext.OnModelCreating`, every entity implementing `ISoftDelete` receives a global query filter:
```csharp
modelBuilder.Entity(entity.ClrType).HasQueryFilter(e => !e.IsDeleted);
```
All LINQ queries (`_db.Students.Where(...)`, `_db.Projects.Include(...)`) automatically omit soft-deleted records without manual filtering.

### D. Administrative Queries & Restoration
Authorized administrators can query deleted records and restore them using:
```csharp
// Query soft-deleted entities
var deletedStudents = await _db.Students.IgnoreQueryFilters().Where(s => s.IsDeleted).ToListAsync();

// Restore a soft-deleted record
entity.IsDeleted = false;
entity.DeletedAt = null;
await _db.SaveChangesAsync();
```

---

## 3. Database Constraints Interaction

### Why All Foreign Keys Use `ON DELETE NO ACTION`
Physical database cascades (`ON DELETE CASCADE`, `ON DELETE SET NULL`) conflict fundamentally with soft deletion:
- If a hard delete was ever accidentally executed, physical cascades would bypass the interceptor and permanently delete child records.
- SQL Server does not support trigger-based cascade on soft delete without complex triggers.
- **Rule:** Cascade soft-deletions (e.g., deleting a Project soft-deletes its uncompleted Tasks) are performed explicitly at the application/service level within a single database transaction.

### Unique Indexes & Filtered Unique Constraints
Standard `UNIQUE` constraints break under soft deletion because a deleted record prevents a user from re-registering the same email or re-creating a deleted skill name.

**Solution:** All unique constraints are defined as SQL Server **Filtered Unique Indexes**:
```sql
CREATE UNIQUE INDEX [IX_Student_Email] ON [Student] ([Email]) WHERE [IsDeleted] = 0;
CREATE UNIQUE INDEX [IX_Skill_Name] ON [Skill] ([Name]) WHERE [IsDeleted] = 0;
CREATE UNIQUE INDEX [IX_Organization_Name] ON [Organization] ([Name]) WHERE [IsDeleted] = 0;
CREATE UNIQUE INDEX [IX_StudentProfile_StudentID] ON [StudentProfile] ([StudentID]) WHERE [IsDeleted] = 0;
```
Once a record is soft-deleted (`IsDeleted = 1`), the identifier is immediately liberated for reuse.

### Re-Applying to Teams & Organizations
`ProjectApplication` and `MembershipApplication` use surrogate `ApplicationID` primary keys and a specialized filtered unique index:
```sql
CREATE UNIQUE INDEX [IX_ProjectApplication_ProjectID_StudentID] 
ON [ProjectApplication] ([ProjectID], [StudentID]) 
WHERE [IsDeleted] = 0 AND [Status] = 'Pending';
```
This ensures a student can only have **one pending application** at any time, but can cleanly re-apply if their previous application was `Rejected` or `Withdrawn`.
