# Phase 02 Audit — Domain Layer & C# 15 Modernization (`TeamUp.Domain`)

**Date:** 2026-11-26  
**Scope:** `backend/src/TeamUp.Domain/` (Common, Entities, Enums, Events)

---

## 1. Inventory Summary
- **Common (`3` files):** `Common/DomainEvents.cs`, `Common/OrganizationRoles.cs`, `Common/SoftDelete.cs`
- **Enums (`1` file):** `Enums/Enums.cs` (18 enums, 6 decorated with `[JsonConverter(typeof(JsonStringEnumConverter))]`)
- **Events (`1` file):** `Events/ActivityDomainEvents.cs` (7 `sealed record` domain events)
- **Entities (`23` files):** `Identity/` (5), `Organizations/` (7), `Projects/` (7), `Gamification/` (4)

---

## 2. Findings & Recommendations

| ID | Severity | Location | Finding | Recommendation (Apply Phase 2) |
|---|---|---|---|---|
| DOM-01 | **P1** | `Common/DomainEvents.cs:5` | `IDomainEvent.OccurredOn => DateTime.UtcNow;` evaluates on every property read rather than capturing event creation time, breaking deterministic auditing. | Capture `OccurredAt` / `OccurredOn` at construction (`DateTime OccurredOn { get; } = DateTime.UtcNow;` or constructor/init property on `DomainEventBase` / records) so raised timestamp is immutable. |
| DOM-02 | **P2** | `Common/OrganizationRoles.cs:27-45` | `AdministrativeRoles` and `CommitteeLeaderExactRoles` allocate mutable `HashSet<string>(StringComparer.OrdinalIgnoreCase)`. | Adopt `FrozenSet<string>` (`ToFrozenSet(StringComparer.OrdinalIgnoreCase)`) or C# 15 collection expression `with(StringComparer.OrdinalIgnoreCase)` for immutable O(1) lookups. |
| DOM-03 | **P2** | `Common/DomainEvents.cs:3`, `Events/ActivityDomainEvents.cs:9-47` | `IDomainEvent` is an open interface with all 7 implementations in `TeamUp.Domain` (explicitly ignored by EF Core in `AppDbContext.cs:71` and never STJ-serialized polymorphically). | Evaluate C# 15 `closed` hierarchy / `closed interface` (or keep `sealed record` implementations clean and documented) if supported cleanly in `.NET 11` SDK (`11.0.100`); verify zero EF Core / reflection impact. |
| DOM-04 | **P2** | 13 Entity constructors/defaults (`Notification.cs:14`, `ScoreTransaction.cs:14`, `StoredImage.cs:15`, etc.) | Default property initializers call `DateTime.UtcNow` directly, coupling entities to wall-clock time. | Keep safe defaults on entity constructors for backward compatibility, while ensuring Application services and `SoftDeleteInterceptor` assign timestamps from injected `TimeProvider`. |
| DOM-05 | **P3** | `Common/DomainEvents.cs`, `Common/SoftDelete.cs`, `Enums/Enums.cs`, 20 Entity files | Missing standardized `// Date:` / `// Purpose:` docblock headers per developer standards. | Add concise `// Date: 2026-11-26` + `// Purpose:` headers to touched Domain files. |
| DOM-06 | **P3** | `Entities/Identity/*.cs`, `Organizations/*.cs`, `Projects/*.cs` | `ICollection<T>` navigation properties are public setters (`= []`) and string properties use `= null!;`. | Preserve EF Core compatibility (avoid breaking `DbSeeder` & test initializers with forced `required` on every field in one step); document as deliberate EF entity design. |
