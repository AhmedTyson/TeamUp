# TeamUp — Database Design & Relational Mapping

> **Relational Schema Specification & Normalization Verification** · Conceptual Chen ERD (13 entities, 19 relationships), Core Relational Mapping (23 tables across 5 tiers), Production Runtime Schema (24 tables, 31 FKs, 11 filtered unique indexes, 9 migrations), 3NF audit, and the D1–D10 schema correction catalog.

> **Sources:**
> - [ERD/description.md](../ERD/description.md)
> - [ERD/TeamUp-Chen-ERD.png](../ERD/TeamUp-Chen-ERD.png)
> - [Mapping/description.md](../Mapping/description.md)
> - [Mapping/TeamUp-Relational-Mapping.png](../Mapping/TeamUp-Relational-Mapping.png)
> - [backend/src/TeamUp.Infrastructure/Persistence/AppDbContext.cs](../backend/src/TeamUp.Infrastructure/Persistence/AppDbContext.cs)
> - [backend/src/TeamUp.Infrastructure/Persistence/Configurations](../backend/src/TeamUp.Infrastructure/Persistence/Configurations)

---

## Table of Contents

1. [Database Purpose & System Context](#database-purpose--system-context)
2. [Chen ERD Specification (13 Entities, 19 Relationships)](#chen-erd-specification-13-entities-19-relationships)
3. [Relational Schema Mapping (23 Core Tables + AuditLog = 24 Runtime Tables)](#relational-schema-mapping-23-core-tables--auditlog--24-runtime-tables)
4. [D1–D10 Critical Schema Corrections Catalog](#d1d10-critical-schema-corrections-catalog)
5. [3NF Normalization Audit & Invariant Guarantees](#3nf-normalization-audit--invariant-guarantees)
6. [Post-Mapping Production Schema Extensions (Migrations 2–9)](#post-mapping-production-schema-extensions-migrations-29)
7. [Diagram Artifacts & Generator Scripts](#diagram-artifacts--generator-scripts)

---

## Database Purpose & System Context

The **TeamUpDB** relational database provides ACID-compliant persistence for student identities, cross-faculty projects, student clubs, campus events, gamification transactions, and immutable governance audit logs.

### Key Database Metrics (Runtime Schema via `AppDbContextModelSnapshot.cs`):
- **Core Mapped Domain Tables:** **23 tables** across 5 architectural tiers (matching [`Mapping/TeamUp-Relational-Mapping.png`](../Mapping/TeamUp-Relational-Mapping.png)).
- **Total Runtime Tables:** **24 tables** (the 23 core domain tables + **`AuditLog`**, added during STRIDE security hardening).
- **Foreign Keys:** **31 constraints** (29 core + `AuditLog.ActorStudentId` + `SessionAttendance.RecordedByStudentId`) — **100% configured with `ON DELETE NO ACTION ON UPDATE NO ACTION` (`DeleteBehavior.Restrict`)**.
- **Soft-Delete Tables:** **22 tables** equipped with a single `DeletedAt DATETIME2 NULL`.
- **Append-Only Ledgers:** **2 immutable ledger tables** (`ScoreTransaction` and `AuditLog`) implementing `IAppendOnly`.
- **Filtered Unique Indexes:** **11 indexes** (10 soft-delete/pending-application indexes + 1 idempotency index on `ScoreTransaction(StudentId, SourceType, SourceReferenceId) WHERE [SourceReferenceId] IS NOT NULL`).
- **Check Constraints:** **20 SQL Server CHECK constraints** enforcing enum domains, date ordering, capacity, team size, and academic year (`1–7`).

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

*(Note: `Task` in the Chen ERD maps to `ProjectTask` in EF Core per correction D8; `AuditLog` is an out-of-band security/governance ledger added in migration 5 outside the 13 conceptual business entities.)*

### Chen Visual Notation Rules:
- **Rectangle (Solid):** Strong Entity (`Student`, `Project`, `Event`).
- **Double Rectangle:** Weak Entity (`EventSession`, dependent on `Event`).
- **Diamond (Solid):** Standard Relationship (`HasSkill`, `MemberOf`).
- **Double Diamond:** Identifying Relationship (`HasSession` for weak entity `EventSession`).
- **Oval (Underlined):** Primary Key Attribute (`StudentID`, `ProjectID`).
- **Oval (Dashed Underline):** Partial Key / Discriminator of weak entity (`SessionID` in ERD; refined to surrogate `SessionId` + unique `(EventId, SessionNo)` in relational mapping per D7).
- **Double Oval:** Multivalued Attribute (`Interest` on `Student`).
- **Dashed Oval:** Derived / Computed Attribute (`TotalPoints`, `Level`, `TeamSize`, `RegisteredCount`).

---

## Relational Schema Mapping (23 Core Tables + `AuditLog` = 24 Runtime Tables)

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ TIER 1: Academic Catalog & Core Identity (+ Governance Ledger)                         │
│ 1. University              2. Faculty                 3. Student                       │
│ 4. StudentInterest         5. StudentProfile          (+24. AuditLog - Append-Only)    │
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
│ 20. Event                 21. EventRegistration      22. EventSession                  │
│ 23. SessionAttendance                                                                  │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## D1–D10 Critical Schema Corrections Catalog

Prior to code implementation, the raw schema was rigorously audited and hardened against ten modeling defects:

| Defect | Original Problem | Hardened Schema Specification | Architectural Rationale |
|---|---|---|---|
| **D1: Cascade Deletes** | Physical `CASCADE` deletes across foreign keys. | **All Foreign Keys (31 runtime) use `ON DELETE NO ACTION` (Restrict).** | Prevents catastrophic cascade deletions; dependent soft-deletes are orchestrated transactionally in Application code. |
| **D2: Missing Update Constraints** | `ON UPDATE` omitted. | **All Foreign Keys enforce `ON UPDATE NO ACTION`.** | Primary keys are immutable; prevents accidental foreign key re-pointing. |
| **D3: Nullable FK Cleansing** | `ProjectTask.AssignedStudentID` used `ON DELETE SET NULL`. | `ON DELETE NO ACTION`. | Soft-deleting a student does not trigger SQL Server `SET NULL`. Application code unassigns active tasks. |
| **D4: Single-Column Soft Delete** | Missing or dual-column flags (`IsDeleted` + `DeletedAt`). | Single column `DeletedAt DATETIME2 NULL` on 22 tables. | Eliminates boolean/timestamp desynchronization; saves table storage. |
| **D5: Unique Index Soft-Delete Collisions** | Standard `UNIQUE` constraints blocked re-registering an email after soft deletion. | Filtered Unique Indexes: `CREATE UNIQUE INDEX ... WHERE [DeletedAt] IS NULL`. | Guarantees uniqueness for active rows while retaining historical audit records. |
| **D6: Multi-Application Lockout** | Composite PK `(ProjectID, StudentID)` blocked re-applying after rejection. | Surrogate PK `ApplicationId INT IDENTITY` with filtered index `WHERE [DeletedAt] IS NULL AND [Status] = 'Pending'`. | Allows students to re-apply if rejected, but blocks multiple concurrent pending requests. |
| **D7: Manual Weak Key Fragility** | `EventSession` used composite PK `(EventID, SessionID)` with manual non-identity numbers. | Surrogate PK `SessionId INT IDENTITY` + filtered index on `(EventId, SessionNo) WHERE [DeletedAt] IS NULL`. | Simplifies foreign key references from `SessionAttendance` down to a single integer column. |
| **D8: Naming Harmonization** | Reserved C# keywords (`Task`) used as entity names. | Renamed to `ProjectTask` and `SessionAttendance`. | Eliminates namespace collisions with `System.Threading.Tasks.Task`. |
| **D9: Unchecked Enums-as-Strings** | Enums stored as arbitrary text strings. | SQL Server `CHECK` constraints on all enum columns (`CK_Student_Role`, `CK_Project_Status`, etc.). | Guarantees database-level domain integrity even during direct SQL scripts. |
| **D10: Ledger Immutability** | `ScoreTransaction` treated as a mutable entity. | Marked `IAppendOnly` (along with `AuditLog`); interceptor strictly blocks `UPDATE` or `DELETE`. | Audit and gamification ledgers must be tamper-proof; corrections require compensating rows. |

---

## 3NF Normalization Audit & Invariant Guarantees

Every table in TeamUpDB satisfies Third Normal Form (3NF) and Boyce-Codd Normal Form (BCNF):
- **1NF:** All attributes are atomic. Multivalued attributes (e.g., student interests) are decomposed into distinct tables (`StudentInterest`).
- **2NF:** Zero partial key dependencies. All non-key attributes depend on the entire primary key.
- **3NF:** Zero transitive dependencies. Derived attributes (`TotalPoints`, `Level`, `RegisteredCount`, `TeamSize`) are computed dynamically via application queries rather than stored as redundant mutable columns.

---

## Post-Mapping Production Schema Extensions (Migrations 2–9)

While [`Mapping/TeamUp-Relational-Mapping.png`](../Mapping/TeamUp-Relational-Mapping.png) captures the initial 23-table design, migrations 2 through 9 added security, concurrency, and feature columns reflected in [`docs/database/teamup-sqlserver-idempotent.sql`](../docs/database/teamup-sqlserver-idempotent.sql):

| Migration | Additions & Hardening |
|---|---|
| `AddStudentRefreshToken` / `AddPasswordResetToken` | Added `RefreshToken`, `RefreshTokenExpiresAt`, `PasswordResetToken`, `PasswordResetTokenExpiresAt` on `Student`. |
| `AddActivityQueryIndexes` | Added filtered composite query indexes on `Organization`, `OrganizationMember`, `MembershipApplication`, `Event`, `EventRegistration`, `SessionAttendance`. |
| `AddStrideSecurityHardening` | Added `AuditLog` (24th table, `IAppendOnly`), `Student.SecurityStamp`, `Student.ShowEmailInDirectory`, `SessionAttendance.RecordedByStudentId` (31st FK), and `RowVersion` on `Project` & `Event`. |
| `AddUniversityLogoUrl` | Added `University.LogoUrl` (`NVARCHAR(500)`). |
| `AddAuthAbuseProtection` | Added `FailedLoginCount`, `LockoutEndUtc`, `PreviousRefreshToken`, `PreviousRefreshTokenRotatedAt` on `Student`. |
| `AddScoreSourceReference` | Added `ScoreTransaction.SourceReferenceId` + 11th filtered unique index `(StudentId, SourceType, SourceReferenceId) WHERE [SourceReferenceId] IS NOT NULL`. |
| `AddStudentEmailPreferences` | Added `EmailNotificationsEnabled`, `ReceiveLeaderboardDigests`, `ReceiveProjectMatchEmails` on `Student`. |

---

## Diagram Artifacts & Generator Scripts

The visual diagrams are generated programmatically:

| Artifact | Format | Generator Script |
|---|---|---|
| **Chen ERD Diagram** | PNG / SVG / Draw.io | [`ERD/source/gen_erd.py`](../ERD/source/gen_erd.py) · [`ERD/source/render.ps1`](../ERD/source/render.ps1) |
| **Relational Mapping Diagram** | PNG / SVG | [`Mapping/source/gen_schema.py`](../Mapping/source/gen_schema.py) |
