# TeamUp Backend: Implementation Plan

Source of truth: `ERD/description.md`, `Mapping/description.md`.
Stack: .NET 10 (SDK 10.0.403 installed), ASP.NET Core Web API, EF Core + SQL Server, JWT auth.
Team: 4 members (M1 to M4) plus shared foundation done first by M1.

## Mapping defects to fix first (found while reviewing `Mapping/description.md`)

| # | Defect | Fix |
|---|---|---|
| D1 | Every FK uses `ON DELETE CASCADE`/`SET NULL`. Conflicts with the soft-delete rule (rows are never physically removed, so cascades never fire or, worse, wipe history if someone hard-deletes). | All FKs: `ON DELETE NO ACTION`. Soft-delete cascade done in the application (service layer) where needed. |
| D2 | `ON UPDATE` never specified anywhere. | All FKs: `ON UPDATE NO ACTION`. SQL Server only supports `NO ACTION` for update on identity keys, and PKs are immutable (IDENTITY). Document it. |
| D3 | `ProjectTask.AssignedStudentID` uses `ON DELETE SET NULL`. Soft delete never nulls it. | `NO ACTION`. Unassign in the service when a member is soft-removed. |
| D4 | No soft-delete column. | Add single column `DeletedAt DATETIME2 NULL` to every table except `ScoreTransaction` (NULL = active, timestamp = deleted). Eliminates bit/timestamp desync and captures audit time natively. |
| D5 | UNIQUE on `Student.Email`, `Skill.Name`, `Organization.Name`, `StudentProfile.StudentID` blocks reuse after soft delete. | Filtered unique indexes `WHERE DeletedAt IS NULL`. |
| D6 | Junction PKs `(ProjectID, StudentID)` on `ProjectApplication` and `MembershipApplication` contradict the ERD text ("student may re-apply after rejection"). | Add surrogate `ApplicationID` PK + filtered unique index on (ProjectID, StudentID) `WHERE Status = 'Pending' AND DeletedAt IS NULL`. Same for MembershipApplication. |
| D7 | `EventSession` composite PK `(EventID, SessionID)` with a non-identity `SessionID` means manual numbering. | Surrogate `SessionID` IDENTITY PK, unique `(EventID, SessionNo)`. `SessionAttendance` then references `SessionID` only, so the PK becomes `(SessionID, StudentID)`. |
| D8 | Names differ between ERD (`Task`, `EventAttendance`) and mapping (`ProjectTask`, `SessionAttendance`). | Keep mapping names, add a rename note in docs. |
| D9 | No CHECK on enums-as-strings (`Status`, `Role`, `Priority`...). | CHECK constraints or EF `HasConversion<string>()` with enums plus CHECK. |
| D10 | Mapping diagram needs redraw for D6, D7 and the soft-delete columns. | Regenerate with `Mapping/source/gen_schema.py`. |

## Soft delete policy (to be documented)

- `DELETE /resource/{id}` sets `DeletedAt = UTC now`, returns `204`.
- A global EF query filter `HasQueryFilter(e => e.DeletedAt == null)` hides deleted rows.
- `SaveChanges` interceptor converts `EntityState.Deleted` into the soft-delete update.
- Admin-only `POST /resource/{id}/restore` and `?includeDeleted=true` for admins.
- Re-creating a unique value (email, skill name) is allowed once the old row is soft-deleted.
- Append-only ledgers (`ScoreTransaction`) are never deleted, corrected with a compensating row.

## REST API proposal (v1, base `/api/v1`)

Conventions: plural nouns, JSON, `201` + `Location` on create, `204` on delete/no body, `PATCH` for partial update, `PUT` for full replace, pagination `?page=&pageSize=`, sorting `?sort=`, errors as RFC 7807 `application/problem+json`. JWT Bearer, roles `Student`, `OrgAdmin`, `PlatformAdmin`.

| Module | Endpoints |
|---|---|
| Auth | `POST /auth/register`, `POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout` |
| Universities | `GET/POST /universities`, `GET/PUT/PATCH/DELETE /universities/{id}`, `GET/POST /universities/{id}/faculties`, `GET/PUT/DELETE /faculties/{id}` |
| Students | `GET /students`, `GET/PATCH/DELETE /students/{id}`, `GET /students/me`, `GET/PUT /students/{id}/profile`, `GET/POST /students/{id}/interests`, `DELETE /students/{id}/interests/{interest}` |
| Skills | `GET/POST /skills`, `GET/PUT/DELETE /skills/{id}`, `GET /students/{id}/skills`, `PUT /students/{id}/skills/{skillId}`, `DELETE /students/{id}/skills/{skillId}` |
| Projects | `GET/POST /projects`, `GET/PUT/PATCH/DELETE /projects/{id}`, `GET/PUT/DELETE /projects/{id}/required-skills[/{skillId}]`, `GET /projects/{id}/members`, `PATCH/DELETE /projects/{id}/members/{studentId}` |
| Applications | `GET/POST /projects/{id}/applications`, `PATCH /projects/{id}/applications/{appId}` (accept/reject), `DELETE .../{appId}` (withdraw) |
| Tasks | `GET/POST /projects/{id}/tasks`, `GET/PUT/PATCH/DELETE /tasks/{id}`, `PUT/DELETE /tasks/{id}/assignee` |
| Organizations | `GET/POST /organizations`, `GET/PUT/PATCH/DELETE /organizations/{id}`, `GET /organizations/{id}/members`, `PATCH/DELETE /organizations/{id}/members/{studentId}` |
| Membership apps | `GET/POST /organizations/{id}/applications`, `PATCH /organizations/{id}/applications/{appId}`, `DELETE .../{appId}` |
| Events | `GET/POST /events`, `GET/PUT/PATCH/DELETE /events/{id}`, `GET/POST /events/{id}/sessions`, `GET/PUT/DELETE /sessions/{id}` |
| Registrations | `GET/POST /events/{id}/registrations`, `PATCH/DELETE /events/{id}/registrations/{studentId}` |
| Attendance | `GET /sessions/{id}/attendance`, `POST /sessions/{id}/attendance` (check-in), `DELETE /sessions/{id}/attendance/{studentId}` |
| Scoring | `GET /students/{id}/score` (total + level), `GET /students/{id}/score-transactions`, `POST /score-transactions` (admin/system) |
| Badges | `GET/POST /badges`, `GET/PUT/DELETE /badges/{id}`, `GET /students/{id}/badges`, `POST /students/{id}/badges` (admin) |
| Notifications | `GET /students/{id}/notifications`, `PATCH /notifications/{id}` (mark read), `PATCH /students/{id}/notifications` (mark all), `DELETE /notifications/{id}` |

## Team split (4 members)

| Member | Owns | Tables |
|---|---|---|
| M1 (tech lead) | Solution scaffold, EF base, soft delete, JWT auth, Universities/Faculties, Students/Profile, CI | University, Faculty, Student, StudentInterest, StudentProfile |
| M2 | Skills, Projects and Teams | Skill, StudentSkill, Project, ProjectRequiredSkill, ProjectMember, ProjectApplication, ProjectTask |
| M3 | Student Activities and Events | Organization, OrganizationMember, MembershipApplication, Event, EventRegistration, EventSession, SessionAttendance |
| M4 | Gamification, Notifications, API docs, tests infra | ScoreTransaction, Badge, StudentBadge, Notification, OpenAPI/Scalar, Postman |

Rules: one feature branch per member per task (`feat/<member>-<slug>`), PR into `main`, 1 reviewer from another member, no direct pushes. Shared files (`AppDbContext`, `Program.cs`) change only through per-module `IEntityTypeConfiguration` and `IServiceCollection` extension methods to avoid merge conflicts.

## Solution layout

```
src/
  TeamUp.Api/            controllers, middleware, Program.cs
  TeamUp.Application/    services, DTOs, validators
  TeamUp.Domain/         entities, enums, ISoftDelete
  TeamUp.Infrastructure/ DbContext, configurations, migrations, interceptors
tests/
  TeamUp.UnitTests/
  TeamUp.IntegrationTests/
docs/api/                API proposal, soft-delete policy, OpenAPI export
```

## Risks

- R1: SQL Server multiple cascade paths. Removed by D1 (all NO ACTION).
- R2: Filtered unique indexes ignored by some providers. Use SQL Server only, add integration test on real SQL Server (Testcontainers or LocalDB).
- R3: Merge conflicts on `DbContext`. Mitigated by per-module configuration classes and a single migration owner (M1).
- R4: Secrets. JWT key and connection string via user-secrets or env vars, never committed.
