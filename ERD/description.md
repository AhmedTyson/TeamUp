# TeamUpDB — Chen ERD Documentation

## 1. System Overview & Database Purpose

**TeamUp** is an integrated, student-centered collaboration and skill-building platform designed for university students from their first year through graduation. 

The **TeamUpDB** database provides the relational data foundation for:
- Connecting students across academic faculties and universities.
- Skill tracking, verification, and discovery.
- Cross-functional team formation, application workflows, and task execution.
- Student activity / organization recruitment and governance.
- Campus event management, multi-session tracking, and attendance verification.
- Gamification via event-driven scoring transactions, levels, and verifiable badges.
- Centralized user notifications across all modules.

---

## 2. Chen Notation Conventions Used

| Visual Element | Representation | Meaning in TeamUpDB |
|---|---|---|
| **Rectangle** | Single solid border | Strong Entity (e.g., `Student`, `Project`, `Event`) |
| **Double Rectangle** | Double solid border | Weak Entity (`EventSession`, dependent on `Event`) |
| **Diamond** | Single solid border | Standard Relationship (e.g., `HasSkill`, `MemberOf`) |
| **Double Diamond** | Double solid border | Identifying Relationship (`HasSession` for weak entity `EventSession`) |
| **Oval (solid)** | Single solid border | Simple Attribute |
| **Oval (solid + underlined text)** | Solid underline | Primary Key Attribute (e.g., `StudentID`, `ProjectID`) |
| **Oval (dashed underline)** | Dashed underline | Partial Key / Discriminator of a weak entity (`SessionID`) |
| **Double Oval** | Two concentric borders | Multivalued Attribute (`Interest` on `Student`) |
| **Dashed Oval** | Dashed border | Derived / Computed Attribute (`TotalPoints`, `Level`, `TeamSize`, `RegisteredCount`) |
| **Tree of Ovals** | Oval linked to sub-ovals | Composite Attribute (`Name` → `FirstName`, `LastName`) |
| **Single Line** | Single connector | Partial Participation |
| **Double Line** | Double parallel lines | Total Participation (Existence Dependency) |
| **Cardinality Labels** | `1`, `N`, `M` | Placed outside the entity border along the connecting line |

---

## 3. Entity Catalog (13 Entities)

### 1. `University` (Strong Entity)
- **Purpose:** Represents an accredited higher-education institution.
- **Attributes:**
  - `UniversityID` (PK, Key)
  - `Name` (Simple)
  - `City` (Simple)
  - `Country` (Simple)
  - `Website` (Simple)

### 2. `Faculty` (Strong Entity)
- **Purpose:** Represents a college, faculty, or school within a university (e.g., Faculty of Engineering, Faculty of Computers and AI).
- **Attributes:**
  - `FacultyID` (PK, Key)
  - `Name` (Simple)
  - `Code` (Simple, e.g., `ENG`, `FCAI`)

### 3. `Student` (Strong Entity — Central Hub)
- **Purpose:** Represents a registered student account on the platform.
- **Attributes:**
  - `StudentID` (PK, Key)
  - `Email` (Simple, Candidate Key)
  - `PasswordHash` (Simple, security credential for JWT auth)
  - `Name` (Composite: `FirstName`, `LastName`)
  - `AcademicYear` (Simple, e.g., 1 to 5)
  - `Role` (Simple, system access role: `Student`, `OrgAdmin`, `PlatformAdmin`)
  - `Interest` (Multivalued, e.g., `AI`, `Web Development`, `Robotics`)
  - `TotalPoints` (Derived, sum of positive points from `ScoreTransaction`)
  - `Level` (Derived, computed mathematically from `TotalPoints`)

### 4. `StudentProfile` (Strong Entity)
- **Purpose:** Extended public profile containing biographical and portfolio information.
- **Attributes:**
  - `ProfileID` (PK, Key)
  - `Bio` (Simple)
  - `PhotoUrl` (Simple)
  - `GitHubUrl` (Simple)
  - `LinkedInUrl` (Simple)

### 5. `Skill` (Strong Entity)
- **Purpose:** Master repository of technical, design, and soft skills tracked on the platform.
- **Attributes:**
  - `SkillID` (PK, Key)
  - `Name` (Simple, e.g., `React`, `ASP.NET Core`, `Docker`)
  - `Category` (Simple, e.g., `Frontend`, `Backend`, `DevOps`, `UI/UX`)
  - `Description` (Simple)

### 6. `Project` (Strong Entity)
- **Purpose:** Student-initiated or course-based collaborative project looking for teammates.
- **Attributes:**
  - `ProjectID` (PK, Key)
  - `Title` (Simple)
  - `Description` (Simple)
  - `Status` (Simple: `Recruiting`, `InProgress`, `Completed`, `Archived`)
  - `CreatedAt` (Simple)
  - `MaxTeamSize` (Simple)
  - `TeamSize` (Derived, count of confirmed records in `MemberOf`)

### 7. `Task` (Strong Entity)
- **Purpose:** Deliverable, sprint backlog item, or action item inside a project.
- **Attributes:**
  - `TaskID` (PK, Key)
  - `Title` (Simple)
  - `Description` (Simple)
  - `Status` (Simple: `Todo`, `InProgress`, `InReview`, `Done`)
  - `DueDate` (Simple)
  - `Priority` (Simple: `Low`, `Medium`, `High`, `Urgent`)

### 8. `Organization` (Strong Entity)
- **Purpose:** Student activity, club, chapter (e.g., IEEE, GDG, Enactus), or academic society.
- **Attributes:**
  - `OrganizationID` (PK, Key)
  - `Name` (Simple)
  - `Description` (Simple)
  - `Category` (Simple, e.g., `Technical`, `Charity`, `Leadership`, `Career`)
  - `LogoUrl` (Simple)

### 9. `Event` (Strong Entity)
- **Purpose:** Workshop, seminar, hackathon, competition, or conference hosted by an organization.
- **Attributes:**
  - `EventID` (PK, Key)
  - `Title` (Simple)
  - `Description` (Simple)
  - `Type` (Simple: `Workshop`, `Hackathon`, `Session`, `Competition`)
  - `Location` (Simple: physical venue or online URL)
  - `StartDate` (Simple)
  - `EndDate` (Simple)
  - `Capacity` (Simple)
  - `RegisteredCount` (Derived, count of registrations in `Registers`)

### 10. `EventSession` (Weak Entity)
- **Purpose:** Individual timetable block, track, or day inside a multi-part event (e.g., Day 1 Workshop, Final Pitching).
- **Identifying Entity:** `Event`
- **Attributes:**
  - `SessionID` (Partial Key / Discriminator, dashed underline)
  - `Title` (Simple)
  - `StartTime` (Simple)
  - `EndTime` (Simple)
  - `Room` (Simple)

### 11. `ScoreTransaction` (Strong Entity)
- **Purpose:** Immutable audit ledger entry awarding points for verified participation.
- **Attributes:**
  - `TransactionID` (PK, Key)
  - `Points` (Simple, positive or negative integer)
  - `Reason` (Simple, human-readable description)
  - `SourceType` (Simple: `EventAttendance`, `TaskCompleted`, `ProjectDelivered`)
  - `CreatedAt` (Simple)

### 12. `Badge` (Strong Entity)
- **Purpose:** Gamification badge / achievement token unlocked by meeting verifiable thresholds.
- **Attributes:**
  - `BadgeID` (PK, Key)
  - `Name` (Simple, e.g., `First Project Leader`, `Workshop Marathoner`)
  - `Description` (Simple)
  - `IconUrl` (Simple)
  - `CriteriaType` (Simple: `PointsThreshold`, `EventCount`, `TaskCount`)
  - `CriteriaValue` (Simple)

### 13. `Notification` (Strong Entity)
- **Purpose:** In-app alert dispatched to a student upon application decisions, assignments, or deadlines.
- **Attributes:**
  - `NotificationID` (PK, Key)
  - `Title` (Simple)
  - `Message` (Simple)
  - `Type` (Simple: `ApplicationStatus`, `TaskAssigned`, `BadgeEarned`, `EventReminder`)
  - `IsRead` (Simple, boolean flag)
  - `CreatedAt` (Simple)

---

## 4. Relationship Catalog (19 Relationships)

| Relationship | Between | Cardinality | Participation | Own Attributes | Semantic Meaning |
|---|---|---|---|---|---|
| **Contains** | University : Faculty | `1 : N` | University partial; Faculty total (double line) | None | A university contains multiple faculties; each faculty belongs to exactly one university. |
| **StudiesAt** | Faculty : Student | `1 : N` | Faculty partial; Student total (double line) | None | A student must be enrolled in one faculty; a faculty has many enrolled students. |
| **HasProfile** | Student : StudentProfile | `1 : 1` | Student partial; Profile total (double line) | None | Strict 1:1 relationship extending student biographical and social details. |
| **HasSkill** | Student : Skill | `M : N` | Student partial; Skill partial | `ProficiencyLevel`, `YearsOfExperience` | Student showcases skills with claimed proficiency (`Beginner`, `Intermediate`, `Advanced`). |
| **Creates** | Student : Project | `1 : N` | Student partial; Project total (double line) | None | A student creates/owns projects; every project must have exactly one creator/owner. |
| **MemberOf** | Student : Project | `M : N` | Student partial; Project partial | `Role`, `JoinedAt` | Confirmed team membership (e.g., `Frontend Dev`, `Scrum Master`, `UI Designer`). |
| **AppliesTo** | Student : Project | `M : N` | Student partial; Project partial | `Message`, `Status`, `AppliedAt` | Team recruitment application (`Pending`, `Accepted`, `Rejected`). |
| **Requires** | Project : Skill | `M : N` | Project partial; Skill partial | `RequiredLevel`, `IsMandatory` | Skills needed by a project to auto-match candidate teammates. |
| **HasTask** | Project : Task | `1 : N` | Project partial; Task total (double line) | None | Every task belongs strictly to one project. |
| **AssignedTo** | Student : Task | `1 : N` | Student partial; Task partial | None | Optional assignment of a project task to a specific team member. |
| **OrgMember** | Student : Organization | `M : N` | Student partial; Org partial | `Role`, `JoinedAt` | Official membership in a student activity (e.g., `Head of PR`, `Member`). |
| **JoinRequest** | Student : Organization | `M : N` | Student partial; Org partial | `Message`, `Status`, `AppliedAt`, `ReviewedAt` | Application to join an organization during recruitment seasons. |
| **Organizes** | Organization : Event | `1 : N` | Org partial; Event total (double line) | None | A student activity organizes multiple events; every event is hosted by an organization. |
| **HasSession** | Event : EventSession | `1 : N` | Event total (double); Session total (double) | None | Identifying relationship linking weak entity `EventSession` to parent `Event`. |
| **Registers** | Student : Event | `M : N` | Student partial; Event partial | `RegisteredAt`, `Status` | Student sign-up for an event (`Confirmed`, `Waitlisted`, `Cancelled`). |
| **Attends** | Student : EventSession | `M : N` | Student partial; Session partial | `CheckInTime`, `VerificationMethod` | Verified physical or QR attendance at a specific session. |
| **Earns** | Student : ScoreTransaction | `1 : N` | Student partial; Transaction total (double line) | None | Audit log of points earned by a student; every transaction belongs to one student. |
| **EarnsBadge** | Student : Badge | `M : N` | Student partial; Badge partial | `AwardedAt` | Gamification badges awarded to a student upon meeting milestone criteria. |
| **Receives** | Student : Notification | `1 : N` | Student partial; Notification total (double line) | None | Notifications dispatched to a specific student. |

---

## 5. Architectural & Design Decisions

1. **Weak Entity Selection (`EventSession`):**
   - An event session cannot exist without its parent `Event`.
   - Its primary key is composite: `(EventID, SessionID)`.
   - Drawn with a double rectangle, double diamond (`HasSession`), and dashed-underlined partial key (`SessionID`).

2. **Decoupling Event Registration vs. Attendance:**
   - `Registers` links `Student` to `Event` (intent to attend, capacity checks).
   - `Attends` links `Student` to `EventSession` (actual physical or digital check-in with a verification timestamp and method).
   - This cleanly handles multi-session events where a student registers once but attends 2 out of 3 sessions.

3. **Team Membership vs. Project Applications:**
   - Keeping `AppliesTo` separate from `MemberOf` prevents polluting the confirmed team list with rejected/pending candidates and allows students to re-apply if rejected.

4. **University-Student Relationship (Transitive Avoidance):**
   - In relational normalization (3NF), a student is linked to `Faculty`, and `Faculty` is linked to `University`.
   - Storing a direct `UniversityID` on `Student` would create transitive redundancy (`StudentID → FacultyID → UniversityID`), which is avoided.

5. **Gamification Derived Attributes:**
   - `Student.TotalPoints` and `Student.Level` are modeled as derived attributes (dashed ovals). In SQL Server and EF Core, `TotalPoints` is aggregated via `SUM(ScoreTransaction.Points)` or updated via application domain events. `Level` is calculated deterministically (`FLOOR(SQRT(TotalPoints / 100)) + 1`).

---

## 6. Deliverable Artifacts

- **Image render:** `TeamUp-Chen-ERD.png` (high-resolution PNG screenshot, 6600 × 5700)
- **Scalable vector:** `source/TeamUp-Chen-ERD.svg`
- **Editable diagram:** `source/TeamUp-Chen-ERD.drawio` (compatible with Draw.io / diagrams.net)
- **Generator script:** `source/gen_erd.py`
