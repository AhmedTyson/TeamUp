# TeamUpDB — Relational Schema Mapping Documentation

## 1. Mapping Methodology & Architecture

The relational schema for **TeamUpDB** was derived systematically from the Chen ERD following standard database normalization principles (3NF / BCNF). 

### Visual Conventions ("Row-of-Cells" Style)
- **Table Representation:** Each table is drawn as a single horizontal row of cells with the bold entity/table name positioned directly above it.
- **Primary Key (PK):** Marked with a **solid underline** beneath the attribute text.
- **Foreign Key (FK):** Marked with a **dashed underline** beneath the attribute text.
- **Composite Primary Key:** Marked with **both solid & dashed underlines** (when a key component is also an FK), plus a continuous **underline below the table box** spanning all primary key cells.
- **Candidate Keys (UQ):** Marked with a red **`UQ`** badge in the upper-right corner of the cell (e.g., `Student.Email`, `StudentProfile.StudentID`).
- **Orthogonal Relationships:** All foreign key constraints are visualized as orthogonal, bent-corner arrows originating from the FK cell and terminating at the primary key cell of the referenced table. Arrows route through dedicated inter-tier corridors with zero crossings through table boxes.
- **Derived / Non-persisted Attributes:** Attributes like `Student.TotalPoints`, `Student.Level`, `Project.TeamSize`, and `Event.RegisteredCount` are computed via SQL aggregation or EF Core domain logic and are **not stored** as physical database columns to eliminate data anomalies.

---

## 2. Complete Inventory of Tables (23 Tables across 5 Tiers)

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ TIER 1: Academic & Core Identity                                                       │
│ 1. University              2. Faculty                 3. Student                       │
│ 4. StudentInterest         5. StudentProfile                                           │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 2: Gamification, Skills & System Alerts                                           │
│ 6. Notification            7. ScoreTransaction        8. Badge                         │
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

---

## 3. Step-by-Step Relational Schema Specification

### Tier 1: Academic Institutions & Core Identity

#### 1. `University`
*Root institution entity.*
- `UniversityID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(150), NOT NULL)
- `City` (NVARCHAR(100), NOT NULL)
- `Country` (NVARCHAR(100), NOT NULL)
- `Website` (NVARCHAR(200), NULL)

#### 2. `Faculty`
*Represents academic colleges within a university (1:N relationship from `University`).*
- `FacultyID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(150), NOT NULL)
- `Code` (NVARCHAR(20), NOT NULL)
- `UniversityID` (INT, NOT NULL, **FK** → `University.UniversityID` ON DELETE RESTRICT)

#### 3. `Student`
*Central student user account. Holds login credentials and academic enrollment.*
- `StudentID` (INT, IDENTITY, **PK**)
- `Email` (NVARCHAR(150), NOT NULL, **UQ**) — Candidate key used for JWT auth
- `PasswordHash` (NVARCHAR(256), NOT NULL) — Argon2id / BCrypt hash
- `FirstName` (NVARCHAR(50), NOT NULL) — Flattened from composite `Name`
- `LastName` (NVARCHAR(50), NOT NULL) — Flattened from composite `Name`
- `AcademicYear` (INT, NOT NULL, CHECK `AcademicYear BETWEEN 1 AND 7`)
- `Role` (NVARCHAR(30), NOT NULL, DEFAULT `'Student'`)
- `FacultyID` (INT, NOT NULL, **FK** → `Faculty.FacultyID` ON DELETE RESTRICT)

#### 4. `StudentInterest`
*Multivalued attribute table for student areas of interest (e.g., AI, Web, Robotics).*
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE CASCADE)
- `Interest` (NVARCHAR(50), NOT NULL, **PK**)
- **Composite PK:** `(StudentID, Interest)`

#### 5. `StudentProfile`
*Strict 1:1 extension table storing optional student biography and social media links.*
- `ProfileID` (INT, IDENTITY, **PK**)
- `StudentID` (INT, NOT NULL, **FK, UQ** → `Student.StudentID` ON DELETE CASCADE)
- `Bio` (NVARCHAR(1000), NULL)
- `PhotoUrl` (NVARCHAR(300), NULL)
- `GitHubUrl` (NVARCHAR(200), NULL)
- `LinkedInUrl` (NVARCHAR(200), NULL)

---

### Tier 2: Skills, Gamification & Notifications

#### 6. `Notification`
*In-app alert inbox for students.*
- `NotificationID` (INT, IDENTITY, **PK**)
- `StudentID` (INT, NOT NULL, **FK** → `Student.StudentID` ON DELETE CASCADE)
- `Title` (NVARCHAR(120), NOT NULL)
- `Message` (NVARCHAR(500), NOT NULL)
- `Type` (NVARCHAR(40), NOT NULL) — e.g., `TaskAssignment`, `ApplicationStatus`
- `IsRead` (BIT, NOT NULL, DEFAULT 0)
- `CreatedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)

#### 7. `ScoreTransaction`
*Audit ledger recording all gamified point additions and deductions.*
- `TransactionID` (INT, IDENTITY, **PK**)
- `StudentID` (INT, NOT NULL, **FK** → `Student.StudentID` ON DELETE CASCADE)
- `Points` (INT, NOT NULL) — e.g., `+50`, `+100`
- `Reason` (NVARCHAR(200), NOT NULL)
- `SourceType` (NVARCHAR(50), NOT NULL) — `EventAttendance`, `TaskCompleted`
- `CreatedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)

#### 8. `Badge`
*Catalog of unlockable gamification achievements.*
- `BadgeID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(80), NOT NULL)
- `Description` (NVARCHAR(250), NOT NULL)
- `IconUrl` (NVARCHAR(300), NOT NULL)
- `CriteriaType` (NVARCHAR(50), NOT NULL) — `PointsThreshold`, `EventCount`
- `CriteriaValue` (INT, NOT NULL)

#### 9. `StudentBadge`
*Junction table representing M:N relationship `EarnsBadge`.*
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE CASCADE)
- `BadgeID` (INT, NOT NULL, **PK, FK** → `Badge.BadgeID` ON DELETE CASCADE)
- `AwardedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- **Composite PK:** `(StudentID, BadgeID)`

#### 10. `Skill`
*Master lookup catalog of skills.*
- `SkillID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(60), NOT NULL, **UQ**)
- `Category` (NVARCHAR(50), NOT NULL) — `Frontend`, `Backend`, `DevOps`
- `Description` (NVARCHAR(250), NULL)

#### 11. `StudentSkill`
*Junction table representing M:N relationship `HasSkill` with proficiency attributes.*
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE CASCADE)
- `SkillID` (INT, NOT NULL, **PK, FK** → `Skill.SkillID` ON DELETE CASCADE)
- `ProficiencyLevel` (NVARCHAR(30), NOT NULL) — `Beginner`, `Intermediate`, `Advanced`
- `YearsOfExperience` (DECIMAL(3,1), NOT NULL, DEFAULT 0.0)
- **Composite PK:** `(StudentID, SkillID)`

---

### Tier 3: Projects, Teams & Task Management

#### 12. `Project`
*Student-created collaborative initiatives.*
- `ProjectID` (INT, IDENTITY, **PK**)
- `Title` (NVARCHAR(150), NOT NULL)
- `Description` (NVARCHAR(MAX), NOT NULL)
- `Status` (NVARCHAR(30), NOT NULL, DEFAULT `'Recruiting'`)
- `CreatedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `MaxTeamSize` (INT, NOT NULL, CHECK `MaxTeamSize >= 1`)
- `CreatorID` (INT, NOT NULL, **FK** → `Student.StudentID` ON DELETE NO ACTION)

#### 13. `ProjectRequiredSkill`
*Junction table representing M:N relationship `Requires`.*
- `ProjectID` (INT, NOT NULL, **PK, FK** → `Project.ProjectID` ON DELETE CASCADE)
- `SkillID` (INT, NOT NULL, **PK, FK** → `Skill.SkillID` ON DELETE CASCADE)
- `RequiredLevel` (NVARCHAR(30), NOT NULL) — `Beginner`, `Intermediate`, `Advanced`
- `IsMandatory` (BIT, NOT NULL, DEFAULT 1)
- **Composite PK:** `(ProjectID, SkillID)`

#### 14. `ProjectMember`
*Junction table representing M:N relationship `MemberOf` (confirmed team members).*
- `ProjectID` (INT, NOT NULL, **PK, FK** → `Project.ProjectID` ON DELETE CASCADE)
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE CASCADE)
- `Role` (NVARCHAR(60), NOT NULL) — e.g., `Team Lead`, `Backend Dev`, `UI/UX`
- `JoinedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- **Composite PK:** `(ProjectID, StudentID)`

#### 15. `ProjectApplication`
*Junction table representing M:N relationship `AppliesTo` (candidate applications).*
- `ProjectID` (INT, NOT NULL, **PK, FK** → `Project.ProjectID` ON DELETE CASCADE)
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE CASCADE)
- `Message` (NVARCHAR(500), NULL)
- `Status` (NVARCHAR(30), NOT NULL, DEFAULT `'Pending'`) — `Pending`, `Accepted`, `Rejected`
- `AppliedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- **Composite PK:** `(ProjectID, StudentID)`

#### 16. `ProjectTask`
*Task breakdown inside a project with optional assignment to a student.*
- `TaskID` (INT, IDENTITY, **PK**)
- `Title` (NVARCHAR(150), NOT NULL)
- `Description` (NVARCHAR(1000), NULL)
- `Status` (NVARCHAR(30), NOT NULL, DEFAULT `'Todo'`) — `Todo`, `InProgress`, `Done`
- `DueDate` (DATETIME2, NULL)
- `Priority` (NVARCHAR(20), NOT NULL, DEFAULT `'Medium'`) — `Low`, `Medium`, `High`
- `ProjectID` (INT, NOT NULL, **FK** → `Project.ProjectID` ON DELETE CASCADE)
- `AssignedStudentID` (INT, NULL, **FK** → `Student.StudentID` ON DELETE SET NULL)

---

### Tier 4: Student Activities & Organizations

#### 17. `Organization`
*Student organizations, chapters, and extracurricular societies.*
- `OrganizationID` (INT, IDENTITY, **PK**)
- `Name` (NVARCHAR(120), NOT NULL, **UQ**)
- `Description` (NVARCHAR(MAX), NOT NULL)
- `Category` (NVARCHAR(50), NOT NULL) — `Technical`, `Cultural`, `Charity`
- `LogoUrl` (NVARCHAR(300), NULL)

#### 18. `OrganizationMember`
*Junction table representing M:N relationship `OrgMember`.*
- `OrganizationID` (INT, NOT NULL, **PK, FK** → `Organization.OrganizationID` ON DELETE CASCADE)
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE CASCADE)
- `Role` (NVARCHAR(60), NOT NULL) — e.g., `President`, `PR Head`, `Member`
- `JoinedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- **Composite PK:** `(OrganizationID, StudentID)`

#### 19. `MembershipApplication`
*Junction table representing M:N recruitment applications `JoinRequest`.*
- `OrganizationID` (INT, NOT NULL, **PK, FK** → `Organization.OrganizationID` ON DELETE CASCADE)
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE CASCADE)
- `Message` (NVARCHAR(600), NULL)
- `Status` (NVARCHAR(30), NOT NULL, DEFAULT `'Pending'`)
- `AppliedAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `ReviewedAt` (DATETIME2, NULL)
- **Composite PK:** `(OrganizationID, StudentID)`

---

### Tier 5: Events, Workshops, Sessions & Attendance

#### 20. `Event`
*Events hosted by student organizations.*
- `EventID` (INT, IDENTITY, **PK**)
- `Title` (NVARCHAR(150), NOT NULL)
- `Description` (NVARCHAR(MAX), NOT NULL)
- `Type` (NVARCHAR(40), NOT NULL) — `Workshop`, `Hackathon`, `Competition`
- `Location` (NVARCHAR(200), NOT NULL)
- `StartDate` (DATETIME2, NOT NULL)
- `EndDate` (DATETIME2, NOT NULL, CHECK `EndDate >= StartDate`)
- `Capacity` (INT, NOT NULL, CHECK `Capacity >= 1`)
- `OrganizationID` (INT, NOT NULL, **FK** → `Organization.OrganizationID` ON DELETE CASCADE)

#### 21. `EventRegistration`
*Junction table representing M:N relationship `Registers`.*
- `EventID` (INT, NOT NULL, **PK, FK** → `Event.EventID` ON DELETE CASCADE)
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE CASCADE)
- `RegisteredAt` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `Status` (NVARCHAR(30), NOT NULL, DEFAULT `'Confirmed'`)
- **Composite PK:** `(EventID, StudentID)`

#### 22. `EventSession`
*Weak entity table identified by parent `Event` (1:N identifying relationship).*
- `EventID` (INT, NOT NULL, **PK, FK** → `Event.EventID` ON DELETE CASCADE)
- `SessionID` (INT, NOT NULL, **PK**) — Discriminator / partial key within the event
- `Title` (NVARCHAR(120), NOT NULL)
- `StartTime` (DATETIME2, NOT NULL)
- `EndTime` (DATETIME2, NOT NULL, CHECK `EndTime >= StartTime`)
- `Room` (NVARCHAR(60), NULL)
- **Composite PK:** `(EventID, SessionID)`

#### 23. `SessionAttendance`
*Junction table representing M:N relationship `Attends`.*
- `EventID` (INT, NOT NULL, **PK, FK**) ──┐ References composite PK of `EventSession`
- `SessionID` (INT, NOT NULL, **PK, FK**) ┘
- `StudentID` (INT, NOT NULL, **PK, FK** → `Student.StudentID` ON DELETE CASCADE)
- `CheckInTime` (DATETIME2, NOT NULL, DEFAULT `SYSUTCDATETIME()`)
- `VerificationMethod` (NVARCHAR(40), NOT NULL) — `QRCode`, `RFID`, `ManualByOrganizer`
- **Composite PK:** `(EventID, SessionID, StudentID)`
- **Foreign Key Constraints:**
  - `FOREIGN KEY (EventID, SessionID) REFERENCES EventSession(EventID, SessionID) ON DELETE CASCADE`
  - `FOREIGN KEY (StudentID) REFERENCES Student(StudentID) ON DELETE CASCADE`

---

## 4. Normalization Verification (3NF / BCNF)

| Normal Form | Rule | Verification in TeamUpDB |
|---|---|---|
| **1NF** | Atomic attributes, no repeating groups. | Multivalued attributes (`StudentInterest`) moved to a separate table. Composite attribute (`Student.Name`) flattened into `FirstName` and `LastName`. All cells contain single values. |
| **2NF** | In 1NF and no partial dependencies on composite keys. | In all junction tables (`StudentSkill`, `ProjectMember`, `ProjectApplication`, `ProjectRequiredSkill`, `OrganizationMember`, `MembershipApplication`, `EventRegistration`, `SessionAttendance`, `StudentBadge`), every non-key column (`ProficiencyLevel`, `Role`, `Message`, `Status`, `CheckInTime`) depends strictly on the entire composite key. |
| **3NF** | In 2NF and no transitive functional dependencies ($X \to Y \to Z$). | `Student` references `FacultyID`, which references `UniversityID`. There is no redundant `UniversityID` column on `Student`, eliminating transitive anomalies. |
| **BCNF** | Every determinant is a candidate key. | All candidate keys (`Email` on `Student`, `Name` on `Organization`, `Name` on `Skill`, `StudentID` on `StudentProfile`) are enforced via unique constraints. |

---

## 5. Technology Integration Notes (ASP.NET Core / EF Core / SQL Server)

- **SQL Server Dialect:** Uses `IDENTITY(1,1)` for auto-incrementing integer keys, `DATETIME2` for microsecond precision timestamps, `NVARCHAR` for full Unicode / Arabic support, and `BIT` for boolean flags.
- **EF Core Composite Keys:** Configured in `DbContext.OnModelCreating` using Fluent API:
  ```csharp
  modelBuilder.Entity<ProjectMember>()
      .HasKey(pm => new { pm.ProjectID, pm.StudentID });
  ```
- **Derived / Computed Properties:** Decorated with `[NotMapped]` in C# model classes or queried via SQL Views / EF Core Value Converters.
- **Cascade Paths:** Multiple cascade paths in SQL Server are prevented by setting `ON DELETE NO ACTION` / `RESTRICT` on `Project.CreatorID` and `ProjectTask.AssignedStudentID`.

---

## 6. Deliverable Artifacts

- **Image render:** `TeamUp-Relational-Mapping.png` (high-resolution PNG screenshot, 3900 × 2700)
- **Scalable vector:** `source/TeamUp-Relational-Mapping.svg`
- **Generator script:** `source/gen_schema.py`
