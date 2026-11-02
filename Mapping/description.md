# TeamUpDB — Relational Schema Mapping Documentation

## 1. Mapping Methodology & Architecture

The relational schema for **TeamUpDB** was derived systematically from the Chen ERD following standard database normalization principles (3NF / BCNF), hardened against real-world database anomalies, and optimized for **ASP.NET Core 10**, **Entity Framework Core 10**, and **Microsoft SQL Server**.

### Visual Conventions ("Row-of-Cells" Style)
- **Table Representation:** Each table is drawn as a single horizontal row of cells with the bold entity/table name positioned directly above it.
- **Primary Key (PK):** Marked with a **solid underline** beneath the attribute text.
- **Foreign Key (FK):** Marked with a **dashed underline** beneath the attribute text.
- **Composite Primary Key:** Marked with **both solid & dashed underlines** (when a key component is also an FK), plus a continuous **underline below the table box** spanning all primary key cells.
- **Filtered Unique Indexes (`UQ`):** Marked with a red **`UQ`** badge in the upper-right corner of the cell. Enforced in SQL Server via filtered unique indexes (`WHERE [DeletedAt] IS NULL`), which allows re-use of unique values (such as an email address or skill name) after an account or record is soft-deleted.
- **Orthogonal Relationships:** All foreign key constraints are visualized as orthogonal, bent-corner arrows originating from the FK cell and terminating at the primary key cell of the referenced table. Arrows route through dedicated inter-tier corridors with zero crossings through table boxes.

---

## 2. Critical Schema Design Corrections Applied

The original raw mapping contained classical modeling traps that have been corrected in this production schema:

| Defect | Original Problem | Corrected Specification | Rationale |
|---|---|---|---|
| **D1: Cascade Delete vs Soft Delete** | Used `ON DELETE CASCADE` and `SET NULL` across FKs. | **All 29 Foreign Keys use `ON DELETE NO ACTION` (Restrict).** | With soft delete, rows are never physically deleted. Physical cascades would either never fire or catastrophically wipe historical records if a hard delete was ever issued. Cascade soft-deletion is handled explicitly at the application / service level. |
| **D2: Missing Update Constraints** | `ON UPDATE` was omitted. | **All 29 Foreign Keys use `ON UPDATE NO ACTION`.** | Primary keys are immutable (surrogate `IDENTITY` or fixed IDs). `NO ACTION` prevents accidental key mutations and aligns with SQL Server best practices. |
| **D3: Nullable FK Cleansing** | `ProjectTask.AssignedStudentID` used `ON DELETE SET NULL`. | `ON DELETE NO ACTION`. | Soft-deleting a student does not invoke SQL Server `SET NULL`. The application service explicitly unassigns active tasks when soft-deleting a student. |
| **D4: Single Soft-Delete Column** | Missing from raw schema. | `DeletedAt DATETIME2 NULL` added to 22 tables (`NULL` = active, timestamp = deleted). | Single-column architecture: eliminates desynchronization between boolean flags and timestamps, saves storage, and captures exact audit timestamp natively. |
| **D5: Unique Index Soft-Delete Collisions** | Standard `UNIQUE` constraints would block re-registering an email or re-creating a deleted skill. | Filtered Unique Indexes: `CREATE UNIQUE INDEX ... WHERE [DeletedAt] IS NULL`. | Allows clean reuse of business identifiers while preserving soft-deleted audit history. |
| **D6: Multi-Application Lockout** | `ProjectApplication` & `MembershipApplication` used composite PK `(ProjectID, StudentID)`, blocking re-applying after rejection. | Surrogate PK `ApplicationID INT IDENTITY`, plus a filtered unique index: `WHERE [DeletedAt] IS NULL AND [Status] = 'Pending'`. | Matches real student workflow: students can re-apply if an earlier application was rejected or withdrawn, but cannot spam multiple simultaneous pending applications. |
| **D7: Manual Weak Key Fragility** | `EventSession` used composite PK `(EventID, SessionID)` with manual non-identity numbering. | Surrogate PK `SessionID INT IDENTITY`, plus filtered unique index on `(EventID, SessionNo) WHERE [DeletedAt] IS NULL`. | Simplifies foreign keys from `SessionAttendance` down to a single `SessionID` column and delegates key generation safely to SQL Server. |
| **D8: Naming Harmonization** | ERD used `Task` (reserved C# keyword) and `EventAttendance`. | Renamed to `ProjectTask` and `SessionAttendance`. | Eliminates namespace collisions with `System.Threading.Tasks.Task` and clarifies domain ownership. |
| **D9: Unchecked Enums-as-Strings** | Enums were unconstrained strings. | SQL Server `CHECK` constraints on all enum columns (`CK_Student_Role`, `CK_Project_Status`, etc.). | Guarantees database-level domain integrity even if accessed outside EF Core. |
| **D10: Ledger Immutability** | `ScoreTransaction` treated as a mutable entity. | Marked `IAppendOnly`. `AppDbContext` interceptor blocks any update or delete. | Financial/gamification audit ledgers must never be altered or soft-deleted; corrections require compensating transaction rows. |

---

## 3. Complete Inventory of Tables (23 Tables across 5 Tiers)

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ TIER 1: Academic & Core Identity                                                       │
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
│ TIER 4: Student Activities & Organizations                                             │
│ 17. Organization          18. OrganizationMember     19. MembershipApplication        │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 5: Events, Workshops & Attendance                                                 │
│ 20. Event                 21. EventRegistration      22. EventSession                  │
│ 23. SessionAttendance                                                                  │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

> **Note on Soft-Delete Column:** Every table except `ScoreTransaction` contains a single audit column: `DeletedAt DATETIME2 NULL` (`NULL` = active, timestamp = soft-deleted). To preserve diagram readability, this column is documented here rather than repeated inside every cell box.

---

## 4. Step-by-Step Relational Schema Specification

### Tier 1: Academic Institutions & Core Identity

#### 1. `University`
*Root accredited higher-education institution.*
- `UniversityID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(150), NOT NULL) — Filtered UNIQUE index `WHERE [DeletedAt] IS NULL`
- `City` (NVARCHAR(100), NOT NULL)
- `Country` (NVARCHAR(100), NOT NULL)
- `Website` (NVARCHAR(200), NULL)
- `DeletedAt` (DATETIME2, NULL)

#### 2. `Faculty`
*Academic colleges within a university (1:N relationship from `University`).*
- `FacultyID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(150), NOT NULL)
- `Code` (NVARCHAR(20), NOT NULL)
- `UniversityID` (INT, NOT NULL, **FK** → `University.UniversityID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `DeletedAt` (DATETIME2, NULL)
- **Constraint:** Filtered UNIQUE index on `(UniversityID, Code) WHERE [DeletedAt] IS NULL`

#### 3. `Student`
*Central student user account. Holds login credentials and academic enrollment.*
- `StudentID` (INT, IDENTITY, **PK**)
- `Email` (NVARCHAR(150), NOT NULL) — Filtered UNIQUE index `WHERE [DeletedAt] IS NULL` (Candidate key for JWT auth)
- `PasswordHash` (NVARCHAR(256), NOT NULL) — Argon2id / ASP.NET Identity PasswordHasher
- `FirstName` (NVARCHAR(50), NOT NULL) — Flattened from composite `Name`
- `LastName` (NVARCHAR(50), NOT NULL) — Flattened from composite `Name`
- `AcademicYear` (INT, NOT NULL) — CHECK `[AcademicYear] BETWEEN 1 AND 7`
- `Role` (VARCHAR(30), NOT NULL, DEFAULT `'Student'`) — CHECK `[Role] IN ('Student', 'OrgAdmin', 'PlatformAdmin')`
- `FacultyID` (INT, NOT NULL, **FK** → `Faculty.FacultyID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `DeletedAt` (DATETIME2, NULL)

#### 4. `StudentInterest`
*Multivalued attribute table for student areas of interest (e.g., AI, Web, Robotics).*
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `Interest` (NVARCHAR(50), NOT NULL, **PK**)
- `DeletedAt` (DATETIME2, NULL)
- **Composite PK:** `(StudentID, Interest)`

#### 5. `StudentProfile`
*Strict 1:1 extension table storing optional student biography, portfolio, and social links.*
- `ProfileID` (INT, IDENTITY, **PK**)
- `StudentID` (INT, NOT NULL, **FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `Bio` (NVARCHAR(1000), NULL)
- `PhotoUrl` (NVARCHAR(300), NULL)
- `GitHubUrl` (NVARCHAR(200), NULL)
- `LinkedInUrl` (NVARCHAR(200), NULL)
- `DeletedAt` (DATETIME2, NULL)
- **Constraint:** Filtered UNIQUE index on `StudentID WHERE [DeletedAt] IS NULL` (enforces 1:1)

---

### Tier 2: Skills, Gamification & Notifications

#### 6. `Notification`
*In-app alert inbox for students.*
- `NotificationID` (INT, IDENTITY, **PK**)
- `StudentID` (INT, NOT NULL, **FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `Title` (NVARCHAR(120), NOT NULL)
- `Message` (NVARCHAR(500), NOT NULL)
- `Type` (VARCHAR(30), NOT NULL) — CHECK `[Type] IN ('ApplicationStatus', 'TaskAssigned', 'BadgeEarned', 'EventReminder')`
- `IsRead` (BIT, NOT NULL, DEFAULT 0)
- `CreatedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `DeletedAt` (DATETIME2, NULL)
- **Index:** `(StudentID, IsRead, CreatedAt)`

#### 7. `ScoreTransaction` (Append-Only Audit Ledger)
*Immutable audit ledger recording all gamified point additions and deductions.*
- `TransactionID` (INT, IDENTITY, **PK**)
- `StudentID` (INT, NOT NULL, **FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `Points` (INT, NOT NULL) — Positive or negative integer
- `Reason` (NVARCHAR(200), NOT NULL)
- `SourceType` (VARCHAR(30), NOT NULL) — CHECK `[SourceType] IN ('EventAttendance', 'TaskCompleted', 'ProjectDelivered', 'Manual')`
- `CreatedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- **Note:** No `DeletedAt` column. Entity implements `IAppendOnly`. The `SoftDeleteInterceptor` blocks any SQL `UPDATE` or `DELETE`. EF Core global query filter hides transactions whose parent `Student` is soft-deleted.

#### 8. `Badge`
*Catalog of unlockable gamification achievements.*
- `BadgeID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(80), NOT NULL) — Filtered UNIQUE index `WHERE [DeletedAt] IS NULL`
- `Description` (NVARCHAR(250), NOT NULL)
- `IconUrl` (NVARCHAR(300), NOT NULL)
- `CriteriaType` (VARCHAR(30), NOT NULL) — CHECK `[CriteriaType] IN ('PointsThreshold', 'EventCount', 'TaskCount')`
- `CriteriaValue` (INT, NOT NULL)
- `DeletedAt` (DATETIME2, NULL)

#### 9. `StudentBadge`
*Junction table representing M:N relationship `EarnsBadge`.*
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `BadgeID` (INT, NOT NULL, **PK, FK** → `Badge.BadgeID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `AwardedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `DeletedAt` (DATETIME2, NULL)
- **Composite PK:** `(StudentID, BadgeID)`

#### 10. `Skill`
*Master lookup catalog of skills.*
- `SkillID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(60), NOT NULL) — Filtered UNIQUE index `WHERE [DeletedAt] IS NULL`
- `Category` (NVARCHAR(50), NOT NULL)
- `Description` (NVARCHAR(250), NULL)
- `DeletedAt` (DATETIME2, NULL)

#### 11. `StudentSkill`
*Junction table representing M:N relationship `HasSkill` with proficiency attributes.*
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `SkillID` (INT, NOT NULL, **PK, FK** → `Skill.SkillID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `ProficiencyLevel` (VARCHAR(30), NOT NULL) — CHECK `[ProficiencyLevel] IN ('Beginner', 'Intermediate', 'Advanced')`
- `YearsOfExperience` (DECIMAL(3,1), NOT NULL, DEFAULT 0.0) — CHECK `[YearsOfExperience] >= 0`
- `DeletedAt` (DATETIME2, NULL)
- **Composite PK:** `(StudentID, SkillID)`

---

### Tier 3: Projects, Teams & Task Management

#### 12. `Project`
*Student-created collaborative initiatives.*
- `ProjectID` (INT, IDENTITY, **PK**)
- `Title` (NVARCHAR(150), NOT NULL)
- `Description` (NVARCHAR(MAX), NOT NULL)
- `Status` (VARCHAR(30), NOT NULL, DEFAULT `'Recruiting'`) — CHECK `[Status] IN ('Recruiting', 'InProgress', 'Completed', 'Archived')`
- `CreatedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `MaxTeamSize` (INT, NOT NULL) — CHECK `[MaxTeamSize] >= 1`
- `CreatorID` (INT, NOT NULL, **FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `DeletedAt` (DATETIME2, NULL)

#### 13. `ProjectRequiredSkill`
*Junction table representing M:N relationship `Requires`.*
- `ProjectID` (INT, NOT NULL, **PK, FK** → `Project.ProjectID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `SkillID` (INT, NOT NULL, **PK, FK** → `Skill.SkillID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `RequiredLevel` (VARCHAR(30), NOT NULL) — CHECK `[RequiredLevel] IN ('Beginner', 'Intermediate', 'Advanced')`
- `IsMandatory` (BIT, NOT NULL)
- `DeletedAt` (DATETIME2, NULL)
- **Composite PK:** `(ProjectID, SkillID)`

#### 14. `ProjectMember`
*Junction table representing M:N relationship `MemberOf` (confirmed team members).*
- `ProjectID` (INT, NOT NULL, **PK, FK** → `Project.ProjectID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `Role` (NVARCHAR(60), NOT NULL) — e.g., `Team Lead`, `Backend Dev`, `UI/UX`
- `JoinedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `DeletedAt` (DATETIME2, NULL)
- **Composite PK:** `(ProjectID, StudentID)`

#### 15. `ProjectApplication`
*Candidate applications for project teams. Uses surrogate PK to allow re-applying after rejection.*
- `ApplicationID` (INT, IDENTITY, **PK**)
- `ProjectID` (INT, NOT NULL, **FK** → `Project.ProjectID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `StudentID` (INT, NOT NULL, **FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `Message` (NVARCHAR(500), NULL)
- `Status` (VARCHAR(30), NOT NULL, DEFAULT `'Pending'`) — CHECK `[Status] IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')`
- `AppliedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `DeletedAt` (DATETIME2, NULL)
- **Constraint:** Filtered UNIQUE index on `(ProjectID, StudentID) WHERE [DeletedAt] IS NULL AND [Status] = 'Pending'`

#### 16. `ProjectTask`
*Task breakdown inside a project with optional assignment to a student team member.*
- `TaskID` (INT, IDENTITY, **PK**)
- `Title` (NVARCHAR(150), NOT NULL)
- `Description` (NVARCHAR(1000), NULL)
- `Status` (VARCHAR(30), NOT NULL, DEFAULT `'Todo'`) — CHECK `[Status] IN ('Todo', 'InProgress', 'InReview', 'Done')`
- `DueDate` (DATETIME2, NULL)
- `Priority` (VARCHAR(20), NOT NULL) — CHECK `[Priority] IN ('Low', 'Medium', 'High', 'Urgent')`
- `ProjectID` (INT, NOT NULL, **FK** → `Project.ProjectID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `AssignedStudentID` (INT, NULL, **FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `DeletedAt` (DATETIME2, NULL)

---

### Tier 4: Student Activities & Organizations

#### 17. `Organization`
*Student organizations, chapters, and extracurricular societies.*
- `OrganizationID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(120), NOT NULL) — Filtered UNIQUE index `WHERE [DeletedAt] IS NULL`
- `Description` (NVARCHAR(MAX), NOT NULL)
- `Category` (NVARCHAR(50), NOT NULL)
- `LogoUrl` (NVARCHAR(300), NULL)
- `DeletedAt` (DATETIME2, NULL)

#### 18. `OrganizationMember`
*Junction table representing M:N relationship `OrgMember`.*
- `OrganizationID` (INT, NOT NULL, **PK, FK** → `Organization.OrganizationID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `Role` (NVARCHAR(60), NOT NULL) — e.g., `President`, `PR Head`, `Member`
- `JoinedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `DeletedAt` (DATETIME2, NULL)
- **Composite PK:** `(OrganizationID, StudentID)`

#### 19. `MembershipApplication`
*Junction table representing recruitment applications `JoinRequest`. Uses surrogate PK.*
- `ApplicationID` (INT, IDENTITY, **PK**)
- `OrganizationID` (INT, NOT NULL, **FK** → `Organization.OrganizationID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `StudentID` (INT, NOT NULL, **FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `Message` (NVARCHAR(600), NULL)
- `Status` (VARCHAR(30), NOT NULL, DEFAULT `'Pending'`) — CHECK `[Status] IN ('Pending', 'Accepted', 'Rejected', 'Withdrawn')`
- `AppliedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `ReviewedAt` (DATETIME2, NULL)
- `DeletedAt` (DATETIME2, NULL)
- **Constraint:** Filtered UNIQUE index on `(OrganizationID, StudentID) WHERE [DeletedAt] IS NULL AND [Status] = 'Pending'`

---

### Tier 5: Events, Workshops, Sessions & Attendance

#### 20. `Event`
*Events hosted by student organizations.*
- `EventID` (INT, IDENTITY, **PK**)
- `Title` (NVARCHAR(150), NOT NULL)
- `Description` (NVARCHAR(MAX), NOT NULL)
- `Type` (VARCHAR(30), NOT NULL) — CHECK `[Type] IN ('Workshop', 'Hackathon', 'Session', 'Competition')`
- `Location` (NVARCHAR(200), NOT NULL)
- `StartDate` (DATETIME2, NOT NULL)
- `EndDate` (DATETIME2, NOT NULL) — CHECK `[EndDate] >= [StartDate]`
- `Capacity` (INT, NOT NULL) — CHECK `[Capacity] >= 1`
- `OrganizationID` (INT, NOT NULL, **FK** → `Organization.OrganizationID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `DeletedAt` (DATETIME2, NULL)

#### 21. `EventRegistration`
*Junction table representing M:N relationship `Registers`.*
- `EventID` (INT, NOT NULL, **PK, FK** → `Event.EventID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `RegisteredAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `Status` (VARCHAR(30), NOT NULL, DEFAULT `'Confirmed'`) — CHECK `[Status] IN ('Confirmed', 'Waitlisted', 'Cancelled')`
- `DeletedAt` (DATETIME2, NULL)
- **Composite PK:** `(EventID, StudentID)`

#### 22. `EventSession`
*Sessions/tracks inside an event. Uses surrogate `SessionID` PK.*
- `SessionID` (INT, IDENTITY, **PK**)
- `EventID` (INT, NOT NULL, **FK** → `Event.EventID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `SessionNo` (INT, NOT NULL)
- `Title` (NVARCHAR(120), NOT NULL)
- `StartTime` (DATETIME2, NOT NULL)
- `EndTime` (DATETIME2, NOT NULL) — CHECK `[EndTime] >= [StartTime]`
- `Room` (NVARCHAR(60), NULL)
- `DeletedAt` (DATETIME2, NULL)
- **Constraint:** Filtered UNIQUE index on `(EventID, SessionNo) WHERE [DeletedAt] IS NULL`

#### 23. `SessionAttendance`
*Junction table representing M:N relationship `Attends`.*
- `SessionID` (INT, NOT NULL, **PK, FK** → `EventSession.SessionID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE NO ACTION ON UPDATE NO ACTION)
- `CheckInTime` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `VerificationMethod` (VARCHAR(30), NOT NULL) — CHECK `[VerificationMethod] IN ('QRCode', 'RFID', 'ManualByOrganizer')`
- `DeletedAt` (DATETIME2, NULL)
- **Composite PK:** `(SessionID, StudentID)`

---

## 5. Normalization Verification (3NF / BCNF)

| Normal Form | Rule | Verification in TeamUpDB |
|---|---|---|
| **1NF** | Atomic attributes, no repeating groups. | Multivalued attribute (`StudentInterest`) is its own table. Composite attribute (`Student.Name`) is flattened into `FirstName` and `LastName`. All cells contain scalar atomic values. |
| **2NF** | In 1NF and no partial dependencies on composite keys. | In all junction tables (`StudentSkill`, `ProjectMember`, `ProjectRequiredSkill`, `OrganizationMember`, `EventRegistration`, `SessionAttendance`, `StudentBadge`), every non-key column (`ProficiencyLevel`, `Role`, `RequiredLevel`, `CheckInTime`) depends strictly on the entire composite key. |
| **3NF** | In 2NF and no transitive functional dependencies ($X \to Y \to Z$). | `Student` references `FacultyID`, which references `UniversityID`. There is no redundant `UniversityID` column on `Student`, eliminating transitive anomalies. Derived metrics (`TotalPoints`, `Level`, `TeamSize`, `RegisteredCount`) are calculated on-the-fly and never stored. |
| **BCNF** | Every determinant is a candidate key. | All candidate keys (`Email` on `Student`, `Name` on `Organization`, `Name` on `Skill`, `StudentID` on `StudentProfile`) are enforced via filtered unique indexes. |

---

## 6. Deliverable Artifacts

- **Image render:** `TeamUp-Relational-Mapping.png` (high-resolution PNG screenshot, 3900 × 2700)
- **Scalable vector:** `source/TeamUp-Relational-Mapping.svg` and `TeamUp-Relational-Mapping.svg`
- **Generator script:** `source/gen_schema.py`
