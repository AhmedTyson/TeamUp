# TeamUp Backend: Task List

Plan, defects D1-D10, REST table and team split: see `plan.md` and `docs/api/API-PROPOSAL.md`.
Size: XS/S/M/L. Owner: M1-M4. Every task = one PR. Verify = `dotnet build` and `dotnet test` green unless stated.

### Modular Implementation Plans
- [Module 01: Auth, Identity & Profiles (Tyson)](./modules/01-auth-identity-profile.md)
- [Module 02: Skills, Projects, Teams & Tasks (Ossama)](./modules/02-skills-projects-tasks.md)
- [Module 03: Activities, Organizations, Events & Attendance (Ibrahim)](./modules/03-organizations-and-events.md)
- [Module 04: Gamification, Scoring Ledger, Badges & Notifications (Youssef)](./modules/04-gamification-and-alerts.md)

## Phase 0: Foundation (M1 leads, others review)

- [x] **T1 (S, M1): Init .NET solution**
  - AC: `TeamUp.sln` with Api, Application, Domain, Infrastructure, UnitTests, IntegrationTests on net10.0; project refs follow Api -> Application -> Domain, Infrastructure -> Application; `.gitignore`, `Directory.Build.props` (nullable on, warnings as errors), `.editorconfig`.
  - Verify: `dotnet build` ok; `dotnet run --project src/TeamUp.Api` serves `/health`.
  - Deps: none. Files: `TeamUp.sln`, `src/*/*.csproj`, `Directory.Build.props`.

- [x] **T2 (S, M1): CI + branch rules**
  - AC: GitHub Actions runs restore, build, test on PR; `main` protected, 1 review required; PR template; `CONTRIBUTING.md` with branch naming and who owns what.
  - Verify: opened test PR shows green check; branch protection active via GitHub API.
  - Deps: T1. Files: `.github/workflows/ci.yml`, `.github/pull_request_template.md`, `CONTRIBUTING.md`.

- [x] **T3 (M, M1): Domain base + soft delete infrastructure**
  - AC: `BaseEntity` (`DeletedAt`), `ISoftDelete`; `AppDbContext` with global query filter for all `ISoftDelete` (`DeletedAt == null`); `SaveChanges` interceptor converts hard deletes into soft deletes; `IgnoreQueryFilters` helper for admin.
  - Verify: unit tests: delete sets flags, deleted row not returned, admin query returns it.
  - Deps: T1. Files: `Domain/Common/*`, `Infrastructure/Persistence/AppDbContext.cs`, `SoftDeleteInterceptor.cs`.

- [x] **T4 (S, M1): Config, secrets, error handling**
  - AC: connection string and JWT key from user-secrets/env, `appsettings` holds placeholders only; global RFC 7807 problem details middleware; API versioning `/api/v1`; paging helper `PagedResult<T>`.
  - Verify: bad request returns `application/problem+json`.
  - Deps: T1. Files: `Api/Program.cs`, `Api/Middleware/*`, `Application/Common/PagedResult.cs`.

### Checkpoint A (after T1-T4)
- [x] Solution builds, CI green, soft-delete tests pass, team agrees on conventions.

## Phase 1: Schema, fixed (all four members, parallel after T5)

- [x] **T5 (S, M2): Fix mapping docs and diagram (defects D1-D10)**
  - AC: `Mapping/description.md` updated: all FKs `ON DELETE NO ACTION ON UPDATE NO ACTION`; soft-delete columns; filtered unique indexes; surrogate `ApplicationID` for both application tables; `EventSession` surrogate PK + `(EventID, SessionNo)` unique; `SessionAttendance` PK `(SessionID, StudentID)`; naming note; PNG/SVG regenerated.
  - Verify: grep shows no `CASCADE` or `SET NULL` left; PNG viewed.
  - Deps: none. Files: `Mapping/description.md`, `Mapping/source/gen_schema.py`, `Mapping/*.png|svg`.

- [x] **T6 (M, M1): Identity module entities + configs**
  - AC: `University`, `Faculty`, `Student`, `StudentInterest`, `StudentProfile` entities; `IEntityTypeConfiguration` each; `Email` filtered unique; profile 1:1 unique; FKs `OnDelete(Restrict)`; enums stored as string with CHECK.
  - Verify: unit test builds model; migration script inspected for `NO ACTION`.
  - Deps: T3, T5.

- [x] **T7 (M, M2): Skills and Projects entities + configs**
  - AC: `Skill`, `StudentSkill`, `Project`, `ProjectRequiredSkill`, `ProjectMember`, `ProjectApplication` (surrogate PK + pending-unique filtered index), `ProjectTask`; all `Restrict`; CHECK `MaxTeamSize >= 1`.
  - Deps: T3, T5, T6 (Student FK).

- [x] **T8 (M, M3): Organizations and Events entities + configs**
  - AC: `Organization`, `OrganizationMember`, `MembershipApplication`, `Event`, `EventRegistration`, `EventSession`, `SessionAttendance`; CHECK `EndDate >= StartDate`, `Capacity >= 1`; unique `(EventID, SessionNo)`.
  - Deps: T3, T5, T6.

- [x] **T9 (S, M4): Gamification entities + configs**
  - AC: `ScoreTransaction` (append-only, no update/delete allowed in interceptor), `Badge`, `StudentBadge`, `Notification`; indexes on `(StudentID, CreatedAt)`.
  - Deps: T3, T5, T6.

- [x] **T10 (M, M1): Initial migration + SQL Server verification**
  - AC: single `InitialCreate` migration covering all 23 tables; applied to local SQL Server; generated script has zero `CASCADE`/`SET NULL`; every FK shows `NO ACTION` for update and delete; filtered indexes present.
  - Verify: `dotnet ef database update`; query `sys.foreign_keys` for `delete_referential_action = 0` and `update_referential_action = 0` on all rows.
  - Deps: T6-T9. Files: `Infrastructure/Migrations/*`.

- [x] **T11 (S, M4): Seed data**
  - AC: universities, faculties, ~30 skills, badges, one admin; idempotent; dev-only.
  - Deps: T10.

### Checkpoint B (after T5-T11)
- [x] DB created from migration, schema matches fixed mapping diagram, FK action query clean, review with team.

## Phase 2: Auth (M1 - Tyson)

- [x] **T12 (M, M1): Register and login with JWT**
  - AC: `POST /auth/register`, `POST /auth/login`; password hashed (Identity `PasswordHasher` or BCrypt); JWT carries `sub`, `role`; `[Authorize]` and role policies wired.
  - Verify: integration tests: register 201, duplicate email 409, bad password 401, protected route 401 without token.
  - Deps: T10.

- [x] **T13 (S, M1): Refresh token and logout**
  - AC: refresh token stored hashed/crypto-random, rotates on use, logout revokes; short access token lifetime.
  - Deps: T12.

## Phase 2.5: N-Tier & SOLID Architecture Alignment (Architecture Blueprint)
*Full design document: `docs/architecture/N-TIER-SOLID-ARCHITECTURE-PLAN.md`*

- [ ] **T-ARCH1 (S, M1): Domain Events & Aggregate Invariants**
  - AC: `IDomainEvent` interface, `BaseEntity.AddDomainEvent()`, `DomainEventDispatcher` publishing events on `SaveChangesAsync()`.
  - Deps: T3.

- [ ] **T-ARCH2 (S, M1): Controller Single Responsibility Decomposition**
  - AC: Decompose multi-purpose controllers into focused, single-responsibility controllers (`StudentsController`, `StudentProfilesController`, `StudentInterestsController`).
  - Deps: T15.

- [ ] **T-ARCH3 (M, M1-M4): Feature Slices & CQRS Structure**
  - AC: Scaffold vertical slices (`Commands/`, `Queries/`, `Handlers/`) across module directories in `TeamUp.Application`.
  - Deps: T-ARCH1.

## Phase 3: Vertical API slices (parallel by owner, each: service + controller + validators + tests)

M1 (Tyson)
- [ ] **T14 (S): Universities and Faculties CRUD** - paging, soft delete, `PlatformAdmin` writes.
- [x] **T15 (M): Students, profile, interests** - `GET /students/me`, `PATCH /students/me`, `GET/PUT /students/me/profile`, `GET /students/{id}/profile`, interests add/remove/restore; owner or admin only; `DELETE /students/{id}` soft deletes and anonymises nothing else.

M2
- [ ] **T16 (S): Skills CRUD + `StudentSkill`** - `PUT/DELETE /students/{id}/skills/{skillId}`.
- [ ] **T17 (M): Projects CRUD + required skills** - creator-only update/delete; `TeamSize` computed, not stored.
- [ ] **T18 (M): Project applications and members** - apply, accept/reject (creator), accept creates `ProjectMember` in one transaction, respect `MaxTeamSize`, re-apply allowed after rejection.
- [ ] **T19 (S): Project tasks** - CRUD, assign/unassign only to current members, soft-removing a member unassigns their tasks.

M3
- [ ] **T20 (S): Organizations CRUD + members** - `OrgAdmin` manages members.
- [ ] **T21 (M): Membership applications** - same pattern as T18.
- [ ] **T22 (M): Events + sessions CRUD** - organizer-only; soft delete of event soft-deletes its sessions (service level, one transaction).
- [ ] **T23 (M): Registrations** - capacity check, waitlist status, `RegisteredCount` computed.
- [ ] **T24 (S): Session attendance check-in** - requires registration, one check-in per student per session, verification method enum.

M4
- [ ] **T25 (M): Scoring** - `ScoreTransaction` append-only, `GET /students/{id}/score` returns total and `Level = FLOOR(SQRT(total/100)) + 1`, hooks called by T19 (task done) and T24 (attendance).
- [ ] **T26 (S): Badges CRUD + award** - background check of criteria after score change; award idempotent.
- [ ] **T27 (S): Notifications** - list, mark read, mark all read, soft delete; raised by T18, T21, T26.

Each of T14-T27 AC (common): endpoints from `plan.md` table exist, correct status codes (`201`+`Location`, `204`, `404`, `409`), `DELETE` is soft (row remains with `DeletedAt` set), ownership/role enforced, integration tests cover happy path + one auth failure + one soft-delete assertion.

### Checkpoint C (after T12-T27)
- [ ] Full flow works: register, create project, apply, accept, complete task, score and badge, notification received.
- [ ] No hard deletes anywhere (grep `ExecuteDelete`, `Remove(` reviewed).

## Phase 4: API documentation (M4 leads, owners review own module)

- [x] **T28 (S, M4): API proposal doc** - `docs/api/API-PROPOSAL.md`: conventions, auth, pagination, error format, full endpoint table with request/response examples, status codes per endpoint, role matrix.
  - Deps: none (drafted and reviewed across all 4 module owners).

- [x] **T29 (XS, M4): Soft delete policy doc** - `docs/api/SOFT-DELETE.md`: `DELETE` = soft delete, flags, restore endpoint, query filter, unique-after-delete rule, ledger exception, FK `NO ACTION` rationale, how deletes cascade in services.
  - Deps: T3.

- [ ] **T30 (S, M4): OpenAPI + Scalar/Swagger UI** - spec generated from controllers with XML comments, JWT scheme, export `docs/api/openapi.json` in CI.
  - Deps: T14-T27 progressively.

- [ ] **T31 (S, M4): Postman collection** - generated from OpenAPI, env with token variable.
  - Deps: T30.

## Phase 5: Hardening and handoff

- [ ] **T32 (S, M1): Security pass** - rate limit on auth, CORS allowlist, no secrets in repo, authorization tests per role. Use `security-and-hardening` skill.
- [ ] **T33 (S, M3): Index and query review** - check query plans for list endpoints, add missing indexes, AsNoTracking on reads.
- [ ] **T34 (S, M2): README + runbook** - setup, migrations, test commands, team workflow.

### Checkpoint D (final)
- [ ] CI green, all tests pass, OpenAPI exported, docs reviewed by all 4 members, demo done.

## Dependency summary

```
T1 -> T2, T3, T4
T3 + T5 -> T6 -> T7, T8, T9 -> T10 -> T11, T12 -> T13
T12 -> T14..T27 (parallel by owner; T18/T21 need T27 for notifications, T19/T24 need T25 for score)
T3 -> T29 ; T14..T27 -> T30 -> T31
```

Suggested start order for day 1: M1 T1, M2 T5 (diagram work, no code deps; reassign T5 from M1 to M2 to balance load), M3 reads plan and drafts T28 outline, M4 drafts T29.
