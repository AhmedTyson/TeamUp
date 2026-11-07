# TeamUp — Database Design & Relational Mapping

> **Relational Schema Specification & Normalization Verification** · Chen ERD (13 entities, 19 relationships), Relational Mapping (23 tables across 5 tiers), 3NF audit, and the D1–D10 schema correction catalog.

> **Sources:**
> - [ERD/description.md](../ERD/description.md)
> - [ERD/TeamUp-Chen-ERD.png](../ERD/TeamUp-Chen-ERD.png)
> - [Mapping/description.md](../Mapping/description.md)
> - [Mapping/TeamUp-Relational-Mapping.png](../Mapping/TeamUp-Relational-Mapping.png)
> - [src/TeamUp.Infrastructure/Persistence/Configurations](../src/TeamUp.Infrastructure/Persistence/Configurations)

---

## Table of Contents

1. [Database Purpose & System Context](#database-purpose--system-context)
2. [Chen ERD Specification (13 Entities, 19 Relationships)](#chen-erd-specification-13-entities-19-relationships)
3. [Relational Schema Mapping (23 Tables across 5 Tiers)](#relational-schema-mapping-23-tables-across-5-tiers)
4. [D1–D10 Critical Schema Corrections Catalog](#d1d10-critical-schema-corrections-catalog)
5. [3NF Normalization Audit & Invariant Guarantees](#3nf-normalization-audit--invariant-guarantees)
6. [Diagram Artifacts & Generator Scripts](#diagram-artifacts--generator-scripts)

---

## Database Purpose & System Context

The **TeamUpDB** relational database provides ACID-compliant persistence for student identities, cross-faculty projects, student clubs, campus events, and gamification transactions.

### Key Database Metrics:
- **Tables:** 23 relational tables.
- **Foreign Keys:** 29 constraints — **100% configured with `ON DELETE NO ACTION ON UPDATE NO ACTION`**.
- **Soft-Delete Columns:** 22 tables equipped with a single `DeletedAt DATETIME2 NULL`.
- **Append-Only Ledgers:** 1 immutable ledger table (`ScoreTransaction`).
- **Filtered Unique Indexes:** 10 indexes enforced with `WHERE [DeletedAt] IS NULL`.

---

## Chen ERD Specification (13 Entities, 19 Relationships)

The conceptual data model is defined in [`ERD/description.md`](../ERD/description.md) using formal Peter Chen notation:

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│ 13 Entities (12 Strong Entities + 1 Weak Entity: EventSession)              │
│ 1. University           2. Faculty                3. Student                │
│ 4. StudentProfile       5. Skill                  6. Project                │
│ 7. ProjectTask          8. Organization           9. Event                  │
│ 10. EventSession (Weak) 11. ScoreTransaction     12. Badge                  │
│ 13. Notification                                                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Chen Visual Notation Rules:
- **Rectangle (Solid):** Strong Entity (`Student`, `Project`, `Event`).
- **Double Rectangle:** Weak Entity (`EventSession`, dependent on `Event`).
- **Diamond (Solid):** Standard Relationship (`HasSkill`, `MemberOf`).
- **Double Diamond:** Identifying Relationship (`HasSession` for weak entity `EventSession`).
- **Oval (Underlined):** Primary Key Attribute (`StudentID`, `ProjectID`).
- **Oval (Dashed Underline):** Partial Key / Discriminator of weak entity (`SessionNo`).
- **Double Oval:** Multivalued Attribute (`Interest` on `Student`).
- **Dashed Oval:** Derived / Computed Attribute (`TotalPoints`, `Level`, `TeamSize`).

---

## Relational Schema Mapping (23 Tables across 5 Tiers)

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ TIER 1: Academic Catalog & Core Identity                                               │
│ 1. University              2. Faculty                 3. Student                       │
│ 4. StudentInterest         5. StudentProfile                                           │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 2: Gamification, Skills & System Alerts                                           │
│ 6. Notification            7. ScoreTransaction (Ledger) 8. Badge                       │
│ 9. StudentBadge           10. Skill                  11. StudentSkill                  │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 3: Projects & Team Formation                                                      │
│ 12. Project               13. ProjectRequiredSkill   14. ProjectMember                 │
│ 15. ProjectApplication    16. ProjectTask                                              │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 4: Student Organizations & Recruitment                                            │
│ 17. Organization          18. OrganizationMember     19. MembershipApplication        │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 5: Events, Multi-Session Timetables & Attendance                                  │
│ 20. Event                 21. EventSession           22. EventRegistration             │
│ 23. SessionAttendance                                                                  │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## D1–D10 Critical Schema Corrections Catalog

Prior to code implementation, the raw schema was rigorously audited and hardened against ten modeling defects:

| Defect | Original Problem | Hardened Schema Specification | Architectural Rationale |
|---|---|---|---|
| **D1: Cascade Deletes** | Physical `CASCADE` deletes across foreign keys. | **All 29 Foreign Keys use `ON DELETE NO ACTION` (Restrict).** | Prevents catastrophic cascade deletions; dependent cascades are orchestrated transactionally in Application code. |
| **D2: Missing Update Constraints** | `ON UPDATE` omitted. | **All 29 Foreign Keys enforce `ON UPDATE NO ACTION`.** | Primary keys are immutable; prevents accidental foreign key re-pointing. |
| **D3: Nullable FK Cleansing** | `ProjectTask.AssignedStudentID` used `ON DELETE SET NULL`. | `ON DELETE NO ACTION`. | Soft-deleting a student does not trigger SQL Server `SET NULL`. Application code unassigns active tasks. |
| **D4: Single-Column Soft Delete** | Missing or dual-column flags (`IsDeleted` + `DeletedAt`). | Single column `DeletedAt DATETIME2 NULL`. | Eliminates boolean/timestamp desynchronization; saves table storage. |
| **D5: Unique Index Soft-Delete Collisions** | Standard `UNIQUE` constraints blocked re-registering an email after soft deletion. | Filtered Unique Indexes: `CREATE UNIQUE INDEX ... WHERE [DeletedAt] IS NULL`. | Guarantees uniqueness for active rows while retaining historical audit records. |
| **D6: Multi-Application Lockout** | Composite PK `(ProjectID, StudentID)` blocked re-applying after rejection. | Surrogate PK `ApplicationID INT IDENTITY` with filtered index `WHERE Status = 'Pending'`. | Allows students to re-apply if rejected, but blocks multiple concurrent pending requests. |
| **D7: Manual Weak Key Fragility** | `EventSession` used composite PK `(EventID, SessionID)` with manual non-identity numbers. | Surrogate PK `SessionID INT IDENTITY` + filtered index on `(EventID, SessionNo)`. | Simplifies foreign key references from `SessionAttendance` down to a single integer column. |
| **D8: Naming Harmonization** | Reserved C# keywords (`Task`) used as entity names. | Renamed to `ProjectTask` and `SessionAttendance`. | Eliminates namespace collisions with `System.Threading.Tasks.Task`. |
| **D9: Unchecked Enums-as-Strings** | Enums stored as arbitrary text strings. | SQL Server `CHECK` constraints on all enum columns (`CK_Student_Role`, etc.). | Guarantees database-level domain integrity even during direct SQL scripts. |
| **D10: Ledger Immutability** | `ScoreTransaction` treated as a mutable entity. | Marked `IAppendOnly`; interceptor strictly blocks SQL `UPDATE` or `DELETE`. | Financial-style audit ledgers must be tamper-proof; corrections require compensating rows. |

---

## 3NF Normalization Audit & Invariant Guarantees

Every table in TeamUpDB satisfies Third Normal Form (3NF) and Boyce-Codd Normal Form (BCNF):
- **1NF:** All attributes are atomic. Multivalued attributes (e.g., student interests) are decomposed into distinct tables (`StudentInterest`).
- **2NF:** Zero partial key dependencies. All non-key attributes depend on the entire primary key.
- **3NF:** Zero transitive dependencies. Derived attributes (`TotalPoints`, `Level`, `RegisteredCount`) are computed dynamically via application services or indexed views rather than stored as redundant mutable columns.

---

## Diagram Artifacts & Generator Scripts

The visual diagrams are generated programmatically to maintain complete synchronization with code:

| Artifact | Format | Generator Script |
|---|---|---|
| **Chen ERD Diagram** | PNG / SVG / Draw.io | [`ERD/source/gen_erd.py`](../ERD/source/gen_erd.py) · [`ERD/source/render.ps1`](../ERD/source/render.ps1) |
| **Relational Mapping Diagram** | PNG / SVG | [`Mapping/source/gen_schema.py`](../Mapping/source/gen_schema.py) |
