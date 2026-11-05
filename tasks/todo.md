# TeamUp Backend: Master Task Checklist (20 Phases)

**Architecture:** Clean Architecture (.NET 10, EF Core 10, SQL Server, JWT Bearer, FluentValidation).  
**Ownership:** M1 (Tyson - Auth/Identity/Admin), M2 (Ossama - Skills/Projects/Tasks), M3 (Ibrahim - Orgs/Events), M4 (Youssef - Gamification/Notifications).  
**Rules:** Single-column `DeletedAt` soft-delete, zero physical cascades, `BaseApiController` with uniform `BaseResponse<T>` JSON envelope.

---

## Phase 01: Solution Scaffold, CI & Baseline Infrastructure
*Foundation established by M1.*

- [x] **P01.1 (S, M1): Init .NET 10 Clean Architecture Solution**
  - AC: Scaffold `TeamUp.sln` with `Api`, `Application`, `Domain`, `Infrastructure`, `UnitTests`, `IntegrationTests`. Strict compiler flags (`Nullable=enable`, `TreatWarningsAsErrors=true`).
  - Verify: `dotnet build` produces 0 errors and 0 warnings.
  - Deps: None. Files: `TeamUp.sln`, `Directory.Build.props`.
- [x] **P01.2 (S, M1): Automated CI Workflow & Quality Gates**
  - AC: GitHub Actions workflow building and testing on every pull request.
  - Verify: Opened PRs show green CI badge.
  - Deps: P01.1. Files: `.github/workflows/ci.yml`.
- [x] **P01.3 (M, M1): Soft-Delete Foundation & EF Interceptor**
  - AC: `ISoftDelete` interface with `DeletedAt DATETIME2 NULL`. `SoftDeleteInterceptor` converts hard deletes into soft-deletes and protects `IAppendOnly` ledgers.
  - Verify: Unit test verifies physical delete transforms into `DeletedAt = UtcNow`.
  - Deps: P01.1. Files: `src/TeamUp.Domain/Common/*`, `src/TeamUp.Infrastructure/Persistence/SoftDeleteInterceptor.cs`.
- [x] **P01.4 (S, M1): Response Envelope & Global Exception Handling**
  - AC: Standardized `BaseResponse<T>` envelope, `BaseApiController`, and `GlobalExceptionHandler` mapping domain exceptions into uniform JSON.
  - Verify: Unhandled exception returns `BaseResponse` with 500 status and friendly error message.
  - Deps: P01.1. Files: `src/TeamUp.Api/Controllers/Common/*`, `src/TeamUp.Api/Middleware/*`.

---

## Phase 02: Relational Schema & 22 Domain Entities
*Cascade-free relational database model verified on SQL Server.*

- [x] **P02.1 (S, M2): Relational Mapping & Diagram Cascade Elimination**
  - AC: Fix D1-D10 defects in `Mapping/description.md`. Replace all `CASCADE` with `NO ACTION`.
  - Verify: Mapping documentation shows zero cascade paths.
  - Deps: P01.1. Files: `Mapping/*`.
- [x] **P02.2 (M, M1-M4): 22 Domain Entities & EF Core Configurations**
  - AC: Entity classes and `IEntityTypeConfiguration` implementations across all 4 modules. String enums with SQL `CHECK` constraints.
  - Verify: EF Core model builds successfully in unit tests.
  - Deps: P01.3, P02.1. Files: `src/TeamUp.Domain/Entities/*`, `src/TeamUp.Infrastructure/Persistence/Configurations/*`.
- [x] **P02.3 (M, M1): Initial EF Core Migration & Cascade Verification**
  - AC: Single `InitialCreate` migration applied to SQL Server Docker instance (`teamup-sql:14333`).
  - Verify: SQL query on `sys.foreign_keys` confirms 0 delete/update referential actions.
  - Deps: P02.2. Files: `src/TeamUp.Infrastructure/Persistence/Migrations/*`.
- [x] **P02.4 (S, M4): Idempotent Database Seeder**
  - AC: `DbSeeder.cs` populates default universities, faculties, and seed admin user if absent.
  - Verify: Database seeder runs idempotently on app launch in development.
  - Deps: P02.3. Files: `src/TeamUp.Infrastructure/Seed/DbSeeder.cs`.

---

## Phase 03: Core Identity & JWT Authentication
*Secure authentication engine and claims infrastructure.*

- [x] **P03.1 (M, M1): Student Registration & Argon2/PBKDF2 Password Hashing**
  - AC: `POST /api/v1/auth/register`, identity password hashing, faculty verification, conflict checks on existing email.
  - Verify: Integration test registers new student and returns 201 Created with JWT tokens.
  - Deps: P02.3. Files: `src/TeamUp.Application/Auth/*`, `src/TeamUp.Infrastructure/Services/AuthService.cs`.
- [x] **P03.2 (M, M1): Student Login & Claims-Enriched Access Tokens**
  - AC: `POST /api/v1/auth/login`, validates password hash, issues JWT with `sub`, `email`, `role`, and student metadata.
  - Verify: Invalid password returns 401 Unauthorized; valid returns 200 with tokens.
  - Deps: P03.1. Files: `src/TeamUp.Application/Common/Security/*`.
- [x] **P03.3 (S, M1): Refresh Token Rotation & Session Revocation**
  - AC: `POST /api/v1/auth/refresh` rotates 64-byte crypto token. `POST /api/v1/auth/logout` clears active refresh token.
  - Verify: Replaying an old refresh token returns 401 Unauthorized.
  - Deps: P03.2. Files: `src/TeamUp.Infrastructure/Services/AuthService.cs`.

---

## Phase 04: Academic Catalog Module: Universities & Faculties
*Public search and administrative catalog management.*

- [x] **P04.1 (S, M1): University & Faculty DTOs with FluentValidation**
  - AC: Input validation for university names, faculty codes, and optional website URLs.
  - Verify: Validator unit tests assert required fields and length limits.
  - Deps: P01.1. Files: `src/TeamUp.Application/Universities/*`.
- [x] **P04.2 (M, M1): Universities Service & Controller**
  - AC: `GET /api/v1/universities/catalog` (anonymous for registration UX), `GET /universities` (paged search), Admin CRUD endpoints.
  - Verify: Integration tests verify public catalog access and admin-only modifications.
  - Deps: P04.1. Files: `src/TeamUp.Api/Controllers/UniversitiesController.cs`.
- [x] **P04.3 (M, M1): Faculties Service & Nested Endpoints**
  - AC: `GET /api/v1/universities/{id}/faculties`, `POST .../faculties`, `PUT /faculties/{id}`, `DELETE /faculties/{id}`.
  - Verify: Nested faculty operations assert university foreign key validity.
  - Deps: P04.2. Files: `src/TeamUp.Api/Controllers/FacultiesController.cs`.

---

## Phase 05: Student Discovery & Account Management
*Student directory search, public portfolio profiles, and self-service account lifecycle.*

- [x] **P05.1 (S, M1): Directory Search Query & Public Card DTOs**
  - AC: `StudentDirectoryQuery` with keyword, faculty, and academic year filters; `StudentCardDto`.
  - Verify: Filtering queries return only active, non-deleted students.
  - Deps: P03.1. Files: `src/TeamUp.Application/Students/*`.
- [x] **P05.2 (M, M1): Student Account & Discovery Endpoints**
  - AC: `GET /api/v1/students` (directory), `GET /students/{id}` (public profile card), `GET /students/me`, `PATCH /students/me`.
  - Verify: Integration tests verify authenticated discovery and self-profile updates.
  - Deps: P05.1. Files: `src/TeamUp.Api/Controllers/StudentsController.cs`.
- [x] **P05.3 (S, M1): Extended Portfolio & Interests Sub-Controllers**
  - AC: Dedicated `StudentProfilesController` (`me/profile`, `{id}/profile`) and `StudentInterestsController` (`interests` tags).
  - Verify: Upserting portfolio information updates profile without touching student account row.
  - Deps: P05.2. Files: `src/TeamUp.Api/Controllers/StudentProfilesController.cs`, `StudentInterestsController.cs`.
- [x] **P05.4 (S, M1): Account Soft-Deletion Lifecycle**
  - AC: `DELETE /api/v1/students/me` sets `DeletedAt = UtcNow`, clears refresh tokens, unassigns tasks.
  - Verify: Soft-deleted student cannot login and disappears from directory.
  - Deps: P05.2. Files: `src/TeamUp.Infrastructure/Services/ProfileService.cs`.

---

## Phase 06: Password Recovery & Platform Governance
*Cryptographic password reset tokens and platform administration.*

- [x] **P06.1 (S, M1): Password Recovery Flow**
  - AC: `POST /api/v1/auth/forgot-password`, `POST /api/v1/auth/reset-password` with 1-hour expiration.
  - Verify: Reset token flow updates password and allows login with new credential.
  - Deps: P03.1. Files: `src/TeamUp.Infrastructure/Services/AuthService.cs`.
- [x] **P06.2 (S, M1): Dedicated Platform Admin Controller**
  - AC: `GET /api/v1/admin/dashboard` (system stats), `PATCH /admin/students/{id}/role` (role promotion), `GET /admin/soft-deleted`.
  - Verify: Forbidden (403) for regular students; OK (200) for PlatformAdmin.
  - Deps: P05.4. Files: `src/TeamUp.Api/Controllers/AdminController.cs`.
- [x] **P06.3 (S, M1): Universal Soft-Delete Restoration Engine**
  - AC: `POST /api/v1/admin/restore/{entityType}/{id}` universally restores soft-deleted records.
  - Verify: Restored record becomes visible to standard queries.
  - Deps: P06.2. Files: `src/TeamUp.Infrastructure/Services/AdminService.cs`.

---

## Phase 07: Projects, Teams & Sprint Task Management
*Collaborative project creation, team recruitment, applications, and task boards.*

- [x] **P07.1 (M, M2): Projects CRUD & Required Skill Matching**
  - AC: `GET/POST /api/v1/projects`, `PATCH/DELETE /projects/{id}`, creator is automatically "Team Lead".
  - Verify: Integration test creates project and retrieves detail with team members.
  - Deps: P05.1. Files: `src/TeamUp.Api/Controllers/ProjectsController.cs`.
- [x] **P07.2 (M, M2): Project Applications Lifecycle**
  - AC: `POST /projects/{id}/applications`, `PATCH .../applications/{appId}` (Accept/Reject), `DELETE withdraw`. Accepting creates `ProjectMember`.
  - Verify: Accepting applicant increases team member count and enforces `MaxTeamSize`.
  - Deps: P07.1. Files: `src/TeamUp.Infrastructure/Services/ProjectService.cs`.
- [x] **P07.3 (S, M2): Sprint Task Board & Member Assignment**
  - AC: `GET/POST /projects/{id}/tasks`, `GET/PATCH/DELETE /api/v1/tasks/{id}`, `PUT /tasks/{id}/assignee`.
  - Verify: Tasks can only be assigned to confirmed team members; member removal unassigns tasks.
  - Deps: P07.2. Files: `src/TeamUp.Api/Controllers/TasksController.cs`.
- [x] **P07.4 (S, M1): PR #1 Audit, Refactoring & Merge**
  - AC: Standardize PR #1 to `BaseApiController`, wrap responses in `BaseResponse<T>`, add FluentValidators, clean root docs, merge into `main`.
  - Verify: 71/71 tests passing, CI green.
  - Deps: P07.1-P07.3. Files: `ProjectsController.cs`, `TasksController.cs`, `ProjectValidators.cs`.

---

### Checkpoint 1 (Phases 01–07 Completed)
- [x] Solution builds with zero warnings.
- [x] 71 unit and integration tests passing.
- [x] Database migrations verified cascade-free on SQL Server.
- [x] Modules 1, 2, 3, 5, 6, 7 operational.

---

## Phase 08: Security Remediation & Cryptographic Hardening
*Immediate critical vulnerability patch and defensive token hardening.*

- [ ] **P08.1 (S, M1): Redact Password Reset Token from API Response**
  - Description: Remove `ResetToken` from `ForgotPasswordResponse` to eliminate critical Account Takeover and user enumeration vulnerability.
  - AC:
    - [ ] `ForgotPasswordResponse` contains only `string Message`.
    - [ ] `POST /api/v1/auth/forgot-password` returns opaque generic message regardless of email existence.
    - [ ] Token is saved to DB but NEVER serialized to HTTP JSON response.
  - Verification: Integration test verifies `ResetToken` is not present in response payload.
  - Deps: P06.1. Files: `src/TeamUp.Application/Auth/AuthDtos.cs`, `src/TeamUp.Infrastructure/Services/AuthService.cs`.
  - Scope: S (2 files).
- [ ] **P08.2 (S, M1): Refactor Integration Tests for Reset Token Flow**
  - Description: Update test harness to extract reset token directly from database context instead of relying on leaked HTTP response.
  - AC:
    - [ ] Test queries test DbContext inside factory scope to obtain `student.PasswordResetToken`.
    - [ ] End-to-end password reset flow verifies successfully without API token leakage.
  - Verification: `dotnet test --filter FullyQualifiedName~PasswordReset` passes green.
  - Deps: P08.1. Files: `tests/TeamUp.IntegrationTests/AuthAndProfileTests.cs`.
  - Scope: S (1 file).
- [ ] **P08.3 (M, M1): Hash Refresh and Reset Tokens at Rest (SHA-256)**
  - Description: Prevent token hijacking on database dumps by storing cryptographic hashes of refresh and reset tokens.
  - AC:
    - [ ] Generate 64-byte crypto random token, return raw token to client.
    - [ ] Store `SHA256(token)` in `Student.RefreshToken` and `Student.PasswordResetToken`.
    - [ ] Verify incoming tokens by computing hash before querying database.
  - Verification: Database inspection shows hashed strings; refresh and reset flows pass.
  - Deps: P08.2. Files: `src/TeamUp.Application/Common/Security/IJwtTokenService.cs`, `src/TeamUp.Infrastructure/Services/JwtTokenService.cs`, `AuthService.cs`.
  - Scope: M (3 files).
- [ ] **P08.4 (S, M1): Partitioned Rate Limiter by Client IP**
  - Description: Protect public auth endpoints against distributed brute-force attacks with IP-partitioned rate limiting.
  - AC:
    - [ ] Configure `PartitionedRateLimiter` partitioned on client IP.
    - [ ] Exceeding 30 requests/minute per IP returns 429 Too Many Requests.
  - Verification: Test sends rapid requests from single client and receives 429.
  - Deps: P08.1. Files: `src/TeamUp.Api/Program.cs`.
  - Scope: S (1 file).

---

## Phase 09: Domain Model Factorization & Bounded Contexts
*Eliminate monolithic `Entities.cs` file and organize domain by bounded contexts.*

- [ ] **P09.1 (S, M1): Deconstruct Identity Bounded Context**
  - Description: Split `University`, `Faculty`, `Student`, `StudentProfile`, and `StudentInterest` into dedicated files.
  - AC:
    - [ ] Separate files in `src/TeamUp.Domain/Entities/Identity/`.
    - [ ] Namespaces and imports aligned cleanly.
  - Verification: Solution builds with zero warnings.
  - Deps: P02.2. Files: `src/TeamUp.Domain/Entities/Identity/*`.
  - Scope: S (5 files).
- [ ] **P09.2 (S, M2): Deconstruct Projects & Skills Bounded Context**
  - Description: Split `Project`, `ProjectMember`, `ProjectApplication`, `ProjectRequiredSkill`, `ProjectTask`, `Skill`, and `StudentSkill` into dedicated files.
  - AC:
    - [ ] Separate files in `src/TeamUp.Domain/Entities/Projects/`.
  - Verification: Solution builds with zero warnings.
  - Deps: P09.1. Files: `src/TeamUp.Domain/Entities/Projects/*`.
  - Scope: S (7 files).
- [ ] **P09.3 (S, M3): Deconstruct Organizations & Events Bounded Context**
  - Description: Split `Organization`, `OrganizationMember`, `MembershipApplication`, `Event`, `EventSession`, `EventRegistration`, and `SessionAttendance` into dedicated files.
  - AC:
    - [ ] Separate files in `src/TeamUp.Domain/Entities/Organizations/`.
  - Verification: Solution builds with zero warnings.
  - Deps: P09.2. Files: `src/TeamUp.Domain/Entities/Organizations/*`.
  - Scope: S (7 files).
- [ ] **P09.4 (XS, M4): Deconstruct Gamification & Notifications Bounded Context**
  - Description: Split `ScoreTransaction`, `Badge`, `StudentBadge`, and `Notification` into dedicated files and delete monolithic `Entities.cs`.
  - AC:
    - [ ] Separate files in `src/TeamUp.Domain/Entities/Gamification/`.
    - [ ] Remove obsolete monolithic `src/TeamUp.Domain/Entities/Entities.cs`.
  - Verification: `dotnet test TeamUp.sln` passes 100%.
  - Deps: P09.3. Files: `src/TeamUp.Domain/Entities/Gamification/*`.
  - Scope: S (5 files).

---

## Phase 10: Clean Architecture Layer Realignment
*Fix architecture inversion: define `IAppDbContext` in Application and migrate business services.*

- [ ] **P10.1 (M, M1): Define `IAppDbContext` Interface in Application Layer**
  - Description: Create `IAppDbContext` in `TeamUp.Application` exposing `DbSet<T>` and `SaveChangesAsync`.
  - AC:
    - [ ] Interface defined in `TeamUp.Application.Common.Interfaces.IAppDbContext`.
    - [ ] `AppDbContext` in `TeamUp.Infrastructure` implements `IAppDbContext`.
  - Verification: `TeamUp.Application` compiles without referencing `TeamUp.Infrastructure`.
  - Deps: P09.4. Files: `src/TeamUp.Application/Common/Interfaces/IAppDbContext.cs`, `src/TeamUp.Infrastructure/Persistence/AppDbContext.cs`.
  - Scope: M (2 files).
- [ ] **P10.2 (M, M1): Migrate Identity & Academic Services to Application**
  - Description: Move `AuthService`, `ProfileService`, `UniversityService`, and `FacultyService` from `Infrastructure/Services` to `Application/Services`.
  - AC:
    - [ ] Business logic services live in `TeamUp.Application.Services`.
    - [ ] Services inject `IAppDbContext` rather than concrete `AppDbContext`.
  - Verification: All identity, university, and profile integration tests pass.
  - Deps: P10.1. Files: `src/TeamUp.Application/Services/*`, `src/TeamUp.Infrastructure/DependencyInjection.cs`.
  - Scope: M (5 files).
- [ ] **P10.3 (M, M2): Migrate Project Services to Application**
  - Description: Move `ProjectService` from `Infrastructure/Services` to `Application/Services`.
  - AC:
    - [ ] `ProjectService` depends on `IAppDbContext`.
    - [ ] Infrastructure retains only persistence, interceptors, and external adapters.
  - Verification: Project and task integration tests pass.
  - Deps: P10.2. Files: `src/TeamUp.Application/Services/ProjectService.cs`.
  - Scope: M (2 files).

---

## Phase 11: Interface Segregation (ISP) for Project Services
*Decompose God interface `IProjectService` (22 methods) into focused, single-responsibility services.*

- [ ] **P11.1 (S, M2): Define Segregated Project Domain Interfaces**
  - Description: Replace monolithic `IProjectService` with 4 focused interfaces.
  - AC:
    - [ ] `IProjectService`: Core CRUD, status, and directory search.
    - [ ] `IProjectSkillService`: Required skills management.
    - [ ] `IProjectMemberService`: Membership roles, member removal, application review.
    - [ ] `IProjectTaskService`: Sprint task board CRUD, assignments, and status updates.
  - Verification: Interfaces compile with clear, decoupled method contracts.
  - Deps: P10.3. Files: `src/TeamUp.Application/Projects/Interfaces/*`.
  - Scope: S (4 files).
- [ ] **P11.2 (M, M2): Implement Segregated Project Services**
  - Description: Implement the 4 segregated services to replace monolithic `ProjectService`.
  - AC:
    - [ ] `ProjectService`, `ProjectSkillService`, `ProjectMemberService`, `ProjectTaskService`.
    - [ ] Each class contains < 100 lines and adheres to SRP.
  - Verification: Unit tests verify independent service operations.
  - Deps: P11.1. Files: `src/TeamUp.Application/Projects/Services/*`.
  - Scope: M (4 files).
- [ ] **P11.3 (S, M2): Refactor Controllers to Inject Segregated Interfaces**
  - Description: Update `ProjectsController` and `TasksController` to inject only the specific services they require.
  - AC:
    - [ ] `TasksController` injects only `IProjectTaskService`.
    - [ ] `ProjectsController` injects `IProjectService`, `IProjectMemberService`, and `IProjectSkillService`.
  - Verification: All 71 tests in `TeamUp.IntegrationTests` pass.
  - Deps: P11.2. Files: `src/TeamUp.Api/Controllers/ProjectsController.cs`, `TasksController.cs`.
  - Scope: S (2 files).

---

### Checkpoint 2 (Phases 08–11 Completed)
- [ ] Security vulnerability resolved: Reset token redacted from API response.
- [ ] Tokens stored hashed with SHA-256.
- [ ] Domain entities factorized into bounded context folders.
- [ ] Clean Architecture layer alignment complete (`IAppDbContext`).
- [ ] God interface `IProjectService` factorized into 4 focused services.

---

## Phase 12: Module 04 — Skills Master Catalog & Student Claims (`T16`)
*Master skills catalog, categorization, and student proficiency endorsements.*

- [ ] **P12.1 (S, M2): Skills Catalog DTOs & FluentValidators**
  - Description: Define request/response DTOs and validation rules for master skills and student skill claims.
  - AC:
    - [ ] `CreateSkillRequest(Name, Category, Description)`, `UpdateSkillRequest`.
    - [ ] `UpsertStudentSkillRequest(ProficiencyLevel, YearsOfExperience)`.
    - [ ] Validates non-empty names, recognized categories, and valid enum values.
  - Verification: Validator unit tests assert required fields and length rules.
  - Deps: P10.1. Files: `src/TeamUp.Application/Skills/*`.
  - Scope: S (3 files).
- [ ] **P12.2 (M, M2): Skills Service & Student Skill Claims Implementation**
  - Description: Implement `ISkillService` for master catalog CRUD and student claimed skills.
  - AC:
    - [ ] Filtered search by category and keyword.
    - [ ] Soft-delete on skills preserves historical records.
    - [ ] `PUT /students/{id}/skills/{skillId}` upserts student proficiency; `DELETE` removes claim.
  - Verification: Unit tests verify skill claims and uniqueness constraints.
  - Deps: P12.1. Files: `src/TeamUp.Application/Services/SkillService.cs`.
  - Scope: M (2 files).
- [ ] **P12.3 (S, M2): Skills Controller & Student Skills Endpoints**
  - AC:
    - [ ] `GET /api/v1/skills`, `POST /api/v1/skills` (`PlatformAdmin`), `GET/PUT/DELETE /skills/{id}`.
    - [ ] `GET /api/v1/students/{id}/skills`, `PUT /students/{id}/skills/{skillId}`, `DELETE`.
    - [ ] Endpoints return standardized `BaseResponse<T>`.
  - Verification: Integration tests cover skills CRUD and student proficiency claims.
  - Deps: P12.2. Files: `src/TeamUp.Api/Controllers/SkillsController.cs`.
  - Scope: S (2 files).

---

## Phase 13: Module 08 & 09 — Student Organizations & Membership Applications (`T20, T21`)
*Student club management, OrgAdmin governance, and membership recruitment workflows.*

- [ ] **P13.1 (S, M3): Organization DTOs, Enums & FluentValidators**
  - AC:
    - [ ] `CreateOrganizationRequest`, `UpdateOrganizationRequest`, `UpdateOrgMemberRoleRequest`.
    - [ ] `CreateMembershipApplicationRequest`, `ReviewMembershipApplicationRequest`.
    - [ ] Validators enforce character limits and role enum validity.
  - Verification: Validator tests pass in `TeamUp.UnitTests`.
  - Deps: P10.1. Files: `src/TeamUp.Application/Organizations/*`.
  - Scope: S (3 files).
- [ ] **P13.2 (M, M3): Organization Service & Application Lifecycle**
  - Description: Implement `IOrganizationService` for club creation, member roles, and recruitment.
  - AC:
    - [ ] Creator is assigned `OrgAdmin`.
    - [ ] Application submission enforces single pending application per student.
    - [ ] Approving application creates `OrganizationMember` in single transaction.
  - Verification: Integration test verifies application submit -> accept -> member added.
  - Deps: P13.1. Files: `src/TeamUp.Application/Services/OrganizationService.cs`.
  - Scope: M (2 files).
- [ ] **P13.3 (M, M3): Organizations & Membership Controllers**
  - AC:
    - [ ] `GET/POST /api/v1/organizations`, `GET/PATCH/DELETE /organizations/{id}`.
    - [ ] `GET /organizations/{id}/members`, `PATCH/DELETE .../members/{studentId}`.
    - [ ] `GET/POST /organizations/{id}/applications`, `PATCH review`, `DELETE withdraw`.
  - Verification: Integration tests verify OrgAdmin authorization and member removal.
  - Deps: P13.2. Files: `src/TeamUp.Api/Controllers/OrganizationsController.cs`.
  - Scope: M (2 files).

---

## Phase 14: Module 10 — Campus Events & Multi-Session Schedules (`T22`)
*Campus events, scheduling, session agendas, and service-level soft-delete cascading.*

- [ ] **P14.1 (S, M3): Event & Session DTOs and Validators**
  - AC:
    - [ ] `CreateEventRequest(Title, Description, Type, Location, StartDate, EndDate, Capacity)`.
    - [ ] `CreateEventSessionRequest(SessionNo, Title, StartTime, EndTime, Room)`.
    - [ ] Validator verifies `EndDate >= StartDate` and `Capacity >= 1`.
  - Verification: Validator tests assert date chronological validation.
  - Deps: P13.1. Files: `src/TeamUp.Application/Events/*`.
  - Scope: S (3 files).
- [ ] **P14.2 (M, M3): Event Service & Multi-Session Management**
  - Description: Implement `IEventService` for event publishing and nested session scheduling.
  - AC:
    - [ ] Only `OrgAdmin` of owning organization can create/update events.
    - [ ] Soft-deleting an event soft-deletes all nested `EventSessions` in one transaction.
  - Verification: Test verifies deleting event marks all associated sessions with `DeletedAt`.
  - Deps: P14.1. Files: `src/TeamUp.Application/Services/EventService.cs`.
  - Scope: M (2 files).
- [ ] **P14.3 (M, M3): Events & Sessions Controller**
  - AC:
    - [ ] `GET/POST /api/v1/events`, `GET/PATCH/DELETE /events/{id}`.
    - [ ] `GET/POST /events/{id}/sessions`, `GET/PUT/DELETE /api/v1/sessions/{id}`.
    - [ ] Filter events by organization, type, and date range.
  - Verification: Integration tests cover event creation, session scheduling, and cascade delete.
  - Deps: P14.2. Files: `src/TeamUp.Api/Controllers/EventsController.cs`, `SessionsController.cs`.
  - Scope: M (2 files).

---

## Phase 15: Module 11 & 12 — Registrations & Attendance Check-in (`T23, T24`)
*Capacity-constrained event registration, waitlisting, and attendance check-in.*

- [ ] **P15.1 (S, M3): Registration & Attendance DTOs and Enums**
  - AC:
    - [ ] `RegisterForEventRequest`, `CheckInAttendanceRequest(VerificationMethod)`.
    - [ ] Verification methods: `QRCode`, `ManualPin`, `NFC`.
  - Verification: Validator tests pass.
  - Deps: P14.1. Files: `src/TeamUp.Application/Events/RegistrationDtos.cs`.
  - Scope: S (2 files).
- [ ] **P15.2 (M, M3): Event Registration Service with Capacity & Waitlist Engine**
  - Description: Implement registration logic with atomic capacity checks.
  - AC:
    - [ ] If registered count < capacity, status is `Confirmed`.
    - [ ] If registered count >= capacity, status is `Waitlisted`.
    - [ ] Soft-canceling registration automatically promotes first waitlisted student.
  - Verification: Integration test fills capacity, verifies waitlist status and promotion.
  - Deps: P15.1. Files: `src/TeamUp.Application/Services/EventRegistrationService.cs`.
  - Scope: M (2 files).
- [ ] **P15.3 (M, M3): Session Attendance Check-in Service & Verification**
  - Description: Implement attendance check-in service ensuring exactly one check-in per student per session.
  - AC:
    - [ ] Student must have a `Confirmed` event registration to check in.
    - [ ] Prevents duplicate check-in for the same session (409 Conflict).
    - [ ] Dispatches `StudentAttendedSessionEvent` domain event for gamification score.
  - Verification: Integration test verifies check-in success and duplicate rejection.
  - Deps: P15.2. Files: `src/TeamUp.Application/Services/AttendanceService.cs`.
  - Scope: M (2 files).
- [ ] **P15.4 (S, M3): Registrations & Attendance Controllers**
  - AC:
    - [ ] `GET/POST /api/v1/events/{id}/registrations`, `DELETE .../registrations/me`.
    - [ ] `GET /api/v1/sessions/{id}/attendance`, `POST .../attendance` (Check-in).
  - Verification: Full registration and attendance flow integration test passes.
  - Deps: P15.3. Files: `src/TeamUp.Api/Controllers/EventRegistrationsController.cs`, `AttendanceController.cs`.
  - Scope: S (2 files).

---

## Phase 16: Module 13 — Gamification Scoring Ledger (`T25`)
*Append-only transaction ledger, non-linear level calculations, and automatic activity hooks.*

- [ ] **P16.1 (S, M4): Gamification Scoring DTOs & Contracts**
  - AC:
    - [ ] `ScoreSummaryDto(TotalPoints, Level, TransactionsCount)`.
    - [ ] `ScoreTransactionDto(TransactionId, Points, Reason, CreatedAt)`.
  - Verification: Contracts compile cleanly in `TeamUp.Application`.
  - Deps: P10.1. Files: `src/TeamUp.Application/Gamification/*`.
  - Scope: S (2 files).
- [ ] **P16.2 (M, M4): Append-Only Scoring Ledger Service**
  - Description: Implement `IScoringService` interacting with append-only `ScoreTransaction` table.
  - AC:
    - [ ] Appending points creates new immutable transaction row.
    - [ ] Level calculation formula: `Level = (int)Math.Floor(Math.Sqrt(totalPoints / 100.0)) + 1`.
    - [ ] Zero updates or deletes permitted (`SoftDeleteInterceptor` throws `InvalidOperationException`).
  - Verification: Unit test verifies level calculations across point milestones (0, 100, 400, 900 pts).
  - Deps: P16.1. Files: `src/TeamUp.Application/Services/ScoringService.cs`.
  - Scope: M (2 files).
- [ ] **P16.3 (S, M4): Wire Automatic Award Hooks from Tasks & Attendance**
  - Description: Award score points automatically when tasks are completed or sessions attended.
  - AC:
    - [ ] Task marked `Done` awards +50 points to assignee.
    - [ ] Session attendance check-in awards +25 points to student.
    - [ ] Handled via domain events: `TaskCompletedEvent`, `SessionAttendedEvent`.
  - Verification: Completing task triggers domain event and increases student point total.
  - Deps: P16.2. Files: `src/TeamUp.Application/Gamification/EventHandlers/*`.
  - Scope: S (3 files).
- [ ] **P16.4 (S, M4): Score Controller**
  - AC:
    - [ ] `GET /api/v1/students/{id}/score` (points & level).
    - [ ] `GET /api/v1/students/{id}/score-transactions` (paged history).
  - Verification: Integration tests assert score endpoints return accurate calculations.
  - Deps: P16.3. Files: `src/TeamUp.Api/Controllers/ScoreController.cs`.
  - Scope: S (1 file).

---

## Phase 17: Module 14 & 15 — Badges Engine & Notifications Inbox (`T26, T27`)
*Automated achievement badges, criterion evaluation, and notifications inbox.*

- [ ] **P17.1 (S, M4): Badge & Notification DTOs and Validators**
  - AC:
    - [ ] `CreateBadgeRequest(Name, Description, IconUrl, RequiredPoints)`.
    - [ ] `BadgeDto`, `NotificationDto(Id, Title, Message, IsRead, CreatedAt)`.
  - Verification: Validator tests pass in `TeamUp.UnitTests`.
  - Deps: P16.1. Files: `src/TeamUp.Application/Gamification/BadgeDtos.cs`, `NotificationDtos.cs`.
  - Scope: S (2 files).
- [ ] **P17.2 (M, M4): Idempotent Badge Award Engine**
  - Description: Implement `IBadgeService` evaluating badge eligibility following point changes.
  - AC:
    - [ ] Automatic check runs after score updates.
    - [ ] Awarding is idempotent: students cannot receive duplicate badges.
    - [ ] Dispatches `BadgeAwardedNotification`.
  - Verification: Integration test verifies reaching point threshold awards badge once.
  - Deps: P17.1. Files: `src/TeamUp.Application/Services/BadgeService.cs`.
  - Scope: M (2 files).
- [ ] **P17.3 (M, M4): Notifications Inbox Service & Lifecycle**
  - Description: Implement `INotificationService` managing user notification streams.
  - AC:
    - [ ] Create notifications for project applications, acceptances, badges, and announcements.
    - [ ] Mark notification as read (`PATCH /notifications/{id}`).
    - [ ] Mark all notifications as read (`PATCH /students/{id}/notifications`).
    - [ ] Soft-delete notification (`DELETE /notifications/{id}`).
  - Verification: Integration test verifies notification creation, read status, and soft-delete.
  - Deps: P17.2. Files: `src/TeamUp.Application/Services/NotificationService.cs`.
  - Scope: M (2 files).
- [ ] **P17.4 (S, M4): Badges & Notifications Controllers**
  - AC:
    - [ ] `GET/POST /api/v1/badges`, `GET /students/{id}/badges`.
    - [ ] `GET /api/v1/students/me/notifications`, `PATCH .../{id}`, `PATCH mark-all`, `DELETE`.
  - Verification: Integration tests cover badge awards and notification inbox interactions.
  - Deps: P17.3. Files: `src/TeamUp.Api/Controllers/BadgesController.cs`, `NotificationsController.cs`.
  - Scope: S (2 files).

---

### Checkpoint 3 (Phases 12–17 Completed)
- [ ] All 15 functional business modules fully implemented.
- [ ] All 99 endpoints from `docs/api/API-PROPOSAL.md` operational.
- [ ] Gamification ledger, badge awards, and notifications wired via domain events.
- [ ] Zero physical cascade deletes across all operations.

---

## Phase 18: Performance, In-Memory Caching & Concurrency Hardening
*High-throughput read caching, index coverage, and optimistic concurrency tokens.*

- [ ] **P18.1 (S, M1): In-Memory Caching for Static Reference Catalogs**
  - Description: Add cached read-through layer for universities, faculties, and skills catalogs.
  - AC:
    - [ ] Cache `/api/v1/universities/catalog` and `/api/v1/skills` using `IMemoryCache` (1-hour TTL).
    - [ ] Admin mutations automatically invalidate relevant cache keys.
  - Verification: Test verifies second call retrieves from memory cache without SQL query.
  - Deps: P12.3. Files: `src/TeamUp.Application/Common/Caching/*`, `src/TeamUp.Api/Program.cs`.
  - Scope: S (3 files).
- [ ] **P18.2 (M, M1): Optimistic Concurrency Protection (`RowVersion`)**
  - Description: Protect capacity-limited entities (`Project.MaxTeamSize`, `Event.Capacity`) from race conditions.
  - AC:
    - [ ] Add `[Timestamp] public byte[] RowVersion { get; set; } = [];` to `Project` and `Event`.
    - [ ] Generate EF Core migration and verify concurrency token mapping.
    - [ ] Concurrent conflicting updates throw `DbUpdateConcurrencyException` and return 409 Conflict.
  - Verification: Concurrency test asserts simultaneous acceptance is rejected with 409.
  - Deps: P14.3. Files: `src/TeamUp.Domain/Entities/*`, `src/TeamUp.Infrastructure/Persistence/Migrations/*`.
  - Scope: M (3 files).
- [ ] **P18.3 (S, M3): SQL Index Review & Query Plan Optimization**
  - Description: Ensure covered indexes exist on foreign keys, soft-delete filters, and search columns.
  - AC:
    - [ ] Composite indexes on `(EventId, Status, DeletedAt)`, `(ProjectId, Status, DeletedAt)`.
    - [ ] Ensure `AsNoTracking()` applied on all read queries.
  - Verification: Code inspection confirms `AsNoTracking()` on all read operations.
  - Deps: P18.2. Files: `src/TeamUp.Infrastructure/Persistence/Configurations/*`.
  - Scope: S (4 files).

---

## Phase 19: Comprehensive OpenAPI v3 Contracts & Postman Collection (`T28-T31`)
*Standardized API documentation, schemas, and interactive test collections.*

- [ ] **P19.1 (S, M4): XML Documentation & Schema Annotations**
  - Description: Enable XML documentation across Api and Application for rich OpenAPI descriptions.
  - AC:
    - [ ] Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `TeamUp.Api.csproj`.
    - [ ] All 99 endpoints documented with summaries, remarks, and status codes.
  - Verification: Build produces XML documentation without compiler warnings.
  - Deps: P17.4. Files: `src/TeamUp.Api/TeamUp.Api.csproj`, `Controllers/*`.
  - Scope: S (5 files).
- [ ] **P19.2 (S, M4): OpenAPI v3 Configuration & Interactive UI**
  - Description: Configure OpenAPI v3 with JWT Bearer security scheme and Swagger/Scalar UI.
  - AC:
    - [ ] `Program.cs` registers OpenApi with JWT Bearer security requirements.
    - [ ] Interactive UI accessible at `/scalar/v1` or `/swagger` in development.
  - Verification: Launching API allows authorizing with Bearer token in UI.
  - Deps: P19.1. Files: `src/TeamUp.Api/Program.cs`.
  - Scope: S (1 file).
- [ ] **P19.3 (S, M4): Export Verified `docs/api/openapi.json`**
  - Description: Export static OpenAPI JSON specification for client SDK generators and documentation portals.
  - AC:
    - [ ] Export full OpenAPI v3 document to `docs/api/openapi.json`.
    - [ ] Validated against OpenAPI 3.0 schema validator.
  - Verification: File exists and passes JSON schema validation.
  - Deps: P19.2. Files: `docs/api/openapi.json`.
  - Scope: S (1 file).
- [ ] **P19.4 (S, M4): Export Postman Collection & Environment**
  - Description: Generate Postman v2.1 collection covering all 15 modules with pre-request auth scripts.
  - AC:
    - [ ] Collection exported to `docs/api/postman/TeamUp_Collection.json`.
    - [ ] Environment file exported with `baseUrl`, `accessToken`, `studentId`.
  - Verification: Postman collection imports cleanly into Postman client.
  - Deps: P19.3. Files: `docs/api/postman/*`.
  - Scope: S (2 files).

---

## Phase 20: Comprehensive Live Verification & Release Package (`T32-T34`)
*Automated PowerShell live test suite, security re-audit, and clean distribution zip.*

- [ ] **P20.1 (M, M1): Extend PowerShell Live API Test Runner**
  - Description: Update `scripts/test_live_api.ps1` to execute end-to-end tests across all 15 modules against SQL Server.
  - AC:
    - [ ] Test covers: Register -> Join Project -> Complete Task -> Join Org -> Attend Event -> Earn Score -> Award Badge.
    - [ ] Assert status codes and `BaseResponse<T>` envelope consistency on every request.
  - Verification: `pwsh scripts/test_live_api.ps1` runs with 100% success against local Docker container.
  - Deps: P18.1-P19.4. Files: `scripts/test_live_api.ps1`.
  - Scope: M (1 file).
- [ ] **P20.2 (S, M1): Generate Master Live Test Evidence Report**
  - Description: Format live execution HTTP request/response evidence into comprehensive documentation.
  - AC:
    - [ ] Write report to `docs/api/MASTER-LIVE-TEST-REPORT.md`.
    - [ ] Document request payloads, response bodies, and execution timings.
  - Verification: Report viewed and confirmed complete.
  - Deps: P20.1. Files: `docs/api/MASTER-LIVE-TEST-REPORT.md`.
  - Scope: S (1 file).
- [ ] **P20.3 (S, M1): Security Audit & Zero-Cascade Verification**
  - Description: Re-run `security-audit` skill workflow and verify zero physical cascade actions in SQL Server.
  - AC:
    - [ ] Zero high/critical vulnerabilities found.
    - [ ] SQL Server query confirms `delete_referential_action = 0` on all 22 entity foreign keys.
    - [ ] All unit and integration tests green (`dotnet test TeamUp.sln`).
  - Verification: Automated check passes with 0 failures.
  - Deps: P20.2. Files: None (Audit check).
  - Scope: S (1 report).
- [ ] **P20.4 (S, M1): Package Clean Distribution Archive**
  - Description: Synchronize clean repository state to `C:\Users\ahmed\Downloads\TeamUp.zip`.
  - AC:
    - [ ] Exclude `bin/`, `obj/`, `.vs/`, and temp files.
    - [ ] Package archive updated at `C:\Users\ahmed\Downloads\TeamUp.zip`.
    - [ ] Git working tree clean and committed to `main`.
  - Verification: Zip file exists with updated timestamp and valid archive contents.
  - Deps: P20.3. Files: `C:\Users\ahmed\Downloads\TeamUp.zip`.
  - Scope: S (1 archive).

---

### Checkpoint 4 (Final Project Completion)
- [ ] 20 phases completed.
- [ ] All 99 endpoints tested live against SQL Server.
- [ ] Clean Architecture layers fully decoupled.
- [ ] Zero critical/high security vulnerabilities.
- [ ] Distribution package refreshed at `C:\Users\ahmed\Downloads\TeamUp.zip`.
