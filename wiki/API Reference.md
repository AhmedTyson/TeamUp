# TeamUp — API Reference

> **RESTful API Architectural Reference & Narrative Module Guide** · Standards, envelope conventions, authentication schemes, rate limits, and domain operation overviews across all **116 endpoints** (115 controller actions across 19 controllers + `/health`).

> **Sources:**
> - [docs/api/openapi.json](../docs/api/openapi.json)
> - [wiki/ROUTES-APPENDIX.md](./ROUTES-APPENDIX.md)
> - [backend/src/TeamUp.Api/Controllers](../backend/src/TeamUp.Api/Controllers)
> - [backend/src/TeamUp.Application/Common/Models](../backend/src/TeamUp.Application/Common/Models)

---

## Table of Contents

1. [Architectural Standards & Response Envelopes](#architectural-standards--response-envelopes)
2. [Authentication Schemes & Authorization Roles](#authentication-schemes--authorization-roles)
3. [Rate Limiting Policies](#rate-limiting-policies)
4. [Domain Modules Overview](#domain-modules-overview)
   - [0. System & Health](#0-system--health)
   - [1. Authentication & Identity](#1-authentication--identity)
   - [2. Universities & Faculties](#2-universities--faculties)
   - [3. Student Discovery, Profiles & Photos](#3-student-discovery-profiles--photos)
   - [4. Skills Master Catalog & Claims](#4-skills-master-catalog--claims)
   - [5. Projects & Team Formation](#5-projects--team-formation)
   - [6. Project Applications](#6-project-applications)
   - [7. Project Sprint Tasks Board](#7-project-sprint-tasks-board)
   - [8. Student Organizations & Memberships](#8-student-organizations--memberships)
   - [9. Membership Recruitment Applications](#9-membership-recruitment-applications)
   - [10. Campus Events & Schedules](#10-campus-events--schedules)
   - [11. Event Sessions Timetable](#11-event-sessions-timetable)
   - [12. Event Registrations & Attendance](#12-event-registrations--attendance)
   - [13. Gamification Scoring Ledger & Leaderboard](#13-gamification-scoring-ledger--leaderboard)
   - [14. Badges & Achievements](#14-badges--achievements)
   - [15. Notifications Inbox](#15-notifications-inbox)
   - [16. Platform Administration, Governance & Reports](#16-platform-administration-governance--reports)
5. [Machine-Readable Contracts](#machine-readable-contracts)

---

## Architectural Standards & Response Envelopes

- **Base Route:** `/api/v1` (plus `/health`)
- **Naming Conventions:** Kebab-case plural nouns (e.g., `/api/v1/score-transactions`, `/api/v1/required-skills`).
- **Standard Success Envelope (`ApiResponse<T>` / `BaseResponse<T>`):**
  ```json
  {
    "success": true,
    "message": "Success",
    "statusCode": 200,
    "data": { },
    "errors": null,
    "timestamp": "2026-11-24T12:00:00Z"
  }
  ```
- **Standard Error Envelope (`ApiResponse` / `BaseResponse` / `ErrorResponse`):**
  ```json
  {
    "success": false,
    "message": "Validation failed: Please correct the highlighted fields and try again.",
    "statusCode": 400,
    "errors": [
      { "field": "password", "message": "Password must be at least 8 characters long." }
    ],
    "timestamp": "2026-11-24T12:00:00Z"
  }
  ```

---

## Authentication Schemes & Authorization Roles

TeamUp enforces token-based authentication via the `Authorization: Bearer <JWT>` header, validated on every request against `ISecurityStampValidator`:

| Tier | Icon | Meaning / Required Claims |
|---|---|---|
| **Public** | `🌐 Public` | Anonymous access allowed (`[AllowAnonymous]`). |
| **Auth** | `🔒 Auth` | Requires a valid, unexpired JWT bearer token with matching `security_stamp`. |
| **Owner / Creator** | `👤 Owner` / `👤 Creator` | Caller's `sub` claim must match the resource owner or project creator. |
| **Member** | `👥 Member` | Caller must be a confirmed member (or creator) of the project team. |
| **OrgAdmin** | `🛡️ OrgAdmin` | Caller must hold an active board/admin seat in the target organization (or `PlatformAdmin`). |
| **PlatformAdmin** | `👑 PlatformAdmin` | Global platform administrator (`Role = PlatformAdmin`). |

---

## Rate Limiting Policies

All limits use a `60`-second fixed window (`RateLimitingOptions`) partitioned by `StudentId` or sanitized `RemoteIpAddress`:
- **Global Authenticated:** `120 req/min` per `StudentId` (`/health` exempt).
- **Global Anonymous:** `30 req/min` per `RemoteIpAddress`.
- **`auth-policy` (`/api/v1/auth/*`):** `30 req/min` per `RemoteIpAddress`.
- **`directory-policy` (`GET /api/v1/students`):** `30 req/min` per `StudentId` (max `20` rows/page).

Excess requests return `429 Too Many Requests` with a `Retry-After` header.

---

## Domain Modules Overview

### 0. System & Health
- `GET /health` (`🌐 Public`) — Verifies API readiness and database connectivity (`{"status": "Healthy", "database": "Connected"}`, or `503` if disconnected).

### 1. Authentication & Identity
*Controller: `AuthController` · Rate Limit: `auth-policy` (`30 req/min` per IP)*
- `POST /api/v1/auth/register` (`🌐 Public`) — Student self-registration with faculty affiliation and email notification opt-in flags.
- `POST /api/v1/auth/login` (`🌐 Public`) — Verifies PBKDF2 password hash (with escalating temporary lockout), returns JWT + 64-byte refresh token.
- `POST /api/v1/auth/refresh` (`🌐 Public`) — Rotates SHA-256 hashed refresh token (`10s` reuse grace window; revokes session on stale replay).
- `POST /api/v1/auth/logout` (`🔒 Auth`) — Clears refresh tokens and rotates `SecurityStamp`.
- `POST /api/v1/auth/forgot-password` (`🌐 Public`) — Generates a 15-minute SHA-256 hashed reset token and sends it via `IEmailSender` (redacted from HTTP response).
- `POST /api/v1/auth/reset-password` (`🌐 Public`) — Resets password, clears lockout, and rotates `SecurityStamp`.

### 2. Universities & Faculties
*Controllers: `UniversitiesController`, `FacultiesController`*
- `GET /api/v1/universities/catalog` (`🌐 Public`) — 1-hour `IMemoryCache` lookup of universities and nested faculties for registration dropdowns.
- `GET /api/v1/universities` & `GET /api/v1/universities/{id}` (`🌐 Public`) — Paginated university search and detail lookup.
- `POST / PUT / DELETE /api/v1/universities` (`👑 PlatformAdmin`) — Administrative university catalog CRUD with cache invalidation.
- `GET /api/v1/universities/{universityId}/faculties` & `GET /api/v1/faculties/{id}` (`🌐 Public`) — Public faculty lookups.
- `POST /api/v1/universities/{universityId}/faculties` & `PUT / DELETE /api/v1/faculties/{id}` (`👑 PlatformAdmin`) — Administrative faculty CRUD with cache invalidation.

### 3. Student Discovery, Profiles & Photos
*Controllers: `StudentsController`, `StudentProfilesController`, `StudentInterestsController`*
- `GET /api/v1/students/showcase` (`🌐 Public`) — Public showcase cards of top campus builders.
- `GET /api/v1/students` (`🔒 Auth`, `directory-policy`) — Privacy-filtered directory search (`search`, `facultyId`, `academicYear`, max `pageSize=20`; hides email unless `ShowEmailInDirectory=true` or caller is `PlatformAdmin`).
- `GET /api/v1/students/{id}` (`🔒 Auth`) — Public student profile summary.
- `GET / PATCH / DELETE /api/v1/students/me` (`👤 Owner`) — Authenticated account retrieval, updates (including privacy & email preferences), and self soft-deletion (`DeletedAt`).
- `DELETE /api/v1/students/{id}` (`👑 PlatformAdmin`) — Admin student soft-deletion.
- `GET /api/v1/students/{id}/profile` (`🔒 Auth`) & `PUT /api/v1/students/me/profile` (`👤 Owner`) — Extended bio, photo URL, GitHub/LinkedIn URLs (`SafeUrlValidator`).
- `POST /api/v1/students/me/profile/photo` (`👤 Owner`) — `multipart/form-data` portrait upload (JPEG/PNG/WebP magic-byte validated, max 3 MB).
- `GET /api/v1/students/{id}/interests` (`🔒 Auth`), `POST /api/v1/students/me/interests`, `DELETE /api/v1/students/me/interests/{interest}` (`👤 Owner`) — Student interest tags lifecycle.

### 4. Skills Master Catalog & Claims
*Controllers: `SkillsController`, `StudentSkillsController`*
- `GET /api/v1/skills` & `GET /api/v1/skills/{id}` (`🔒 Auth`) — Master skills catalog (unfiltered list cached for 1 hour).
- `POST /api/v1/skills`, `PUT / DELETE /api/v1/skills/{id}` (`👑 PlatformAdmin`) — Master skill catalog administration.
- `GET /api/v1/students/{id}/skills` (`🔒 Auth`) — List student's claimed skills (`ProficiencyLevel` + `YearsOfExperience`).
- `PUT / DELETE /api/v1/students/me/skills/{skillId}` (`👤 Owner`) — Claim, update, or remove a skill.

### 5. Projects & Team Formation
*Controller: `ProjectsController`*
- `GET /api/v1/projects` (`🔒 Auth`) — Filterable project catalog (`keyword`, `status`, `skillId`, paginated).
- `POST /api/v1/projects` (`🔒 Auth`) — Create project (`Creator` becomes initial `ProjectMember`).
- `GET /api/v1/projects/{id}` (`🔒 Auth`), `PATCH / DELETE /api/v1/projects/{id}` (`👤 Creator`) — Project detail, updates (`Completed` awards `+100 XP` per member), and transactional soft-delete.
- `GET /api/v1/projects/{id}/required-skills` (`🔒 Auth`), `PUT / DELETE /api/v1/projects/{id}/required-skills/{skillId}` (`👤 Creator`) — Required skills matrix management.
- `GET /api/v1/projects/{id}/members` (`🔒 Auth`), `PATCH /api/v1/projects/{id}/members/{studentId}` (`👤 Creator`), `DELETE /api/v1/projects/{id}/members/{studentId}` (`👤 Creator / Self`) — Team roster management.

### 6. Project Applications
*Controller: `ProjectsController`*
- `POST /api/v1/projects/{id}/applications` (`🔒 Auth`) — Submit candidate application (`Pending` filtered unique index prevents duplicate pending apps).
- `GET /api/v1/projects/{id}/applications` (`👤 Creator`) — Project creator reviews incoming applications.
- `PATCH /api/v1/projects/{id}/applications/{appId}` (`👤 Creator`) — Review application (`Accepted` or `Rejected`, triggers `ApplicationReviewedDomainEvent`).
- `DELETE /api/v1/projects/{id}/applications/{appId}` (`👤 Applicant`) — Withdraw a pending application.

### 7. Project Sprint Tasks Board
*Controllers: `ProjectsController`, `TasksController`*
- `GET / POST /api/v1/projects/{id}/tasks` (`👥 Member`) — List project tasks or create a new task (`Todo`, `InProgress`, `InReview`, `Done`).
- `GET / PATCH /api/v1/tasks/{id}` (`👥 Member`) — View task or update task fields/status (`Done` awards `+50 XP` once per task).
- `PUT / DELETE /api/v1/tasks/{id}/assignee` (`👤 Creator`) — Assign (`TaskAssignedDomainEvent`) or unassign a team member.
- `DELETE /api/v1/tasks/{id}` (`👤 Creator`) — Soft-delete a task.

### 8. Student Organizations & Memberships
*Controller: `OrganizationsController`*
- `GET /api/v1/organizations` & `GET /api/v1/organizations/{id}` (`🔒 Auth`) — Campus organizations directory and details.
- `POST /api/v1/organizations` (`🛡️ OrgAdmin / 👑 PlatformAdmin`) — Create student organization.
- `PATCH /api/v1/organizations/{id}` (`🛡️ OrgAdmin`) — Update organization details.
- `DELETE /api/v1/organizations/{id}` (`👑 PlatformAdmin`) — Soft-delete organization.
- `GET /api/v1/organizations/{id}/members` (`🔒 Auth`), `PATCH /api/v1/organizations/{id}/members/{studentId}` (`🛡️ OrgAdmin`), `DELETE /api/v1/organizations/{id}/members/{studentId}` (`🛡️ OrgAdmin / Self`) — Organization roster management.

### 9. Membership Recruitment Applications
*Controller: `OrganizationsController`*
- `POST /api/v1/organizations/{id}/applications` (`🔒 Auth`) — Apply to join a student organization.
- `GET /api/v1/organizations/{id}/applications` & `PATCH /api/v1/organizations/{id}/applications/{appId}` (`🛡️ OrgAdmin`) — Review and accept/reject applications (`ApplicationReviewedDomainEvent`).
- `DELETE /api/v1/organizations/{id}/applications/{appId}` (`👤 Applicant`) — Withdraw pending application.

### 10. Campus Events & Schedules
*Controller: `EventsController`*
- `GET /api/v1/events` & `GET /api/v1/events/{id}` (`🔒 Auth`) — Filterable campus event calendar (`search`, `organizationId`, `type`, `from`, `to`).
- `POST /api/v1/events`, `PATCH / DELETE /api/v1/events/{id}` (`🛡️ OrgAdmin`) — Event creation, updates, and soft-deletion.

### 11. Event Sessions Timetable
*Controllers: `EventsController`, `EventSessionsController`*
- `GET /api/v1/events/{id}/sessions` (`🔒 Auth`) — List sessions for an event.
- `POST /api/v1/events/{id}/sessions` (`🛡️ OrgAdmin`) — Add session to event timetable.
- `PUT / DELETE /api/v1/sessions/{id}` (`🛡️ OrgAdmin`) — Update or soft-delete a session.

### 12. Event Registrations & Attendance
*Controllers: `EventRegistrationsController`, `SessionAttendanceController`*
- `GET /api/v1/events/{id}/registrations` (`🛡️ OrgAdmin`) — View confirmed and waitlisted registrants.
- `POST /api/v1/events/{id}/registrations` (`🔒 Auth`) — Register for event (`Confirmed` or `Waitlisted` with `RowVersion` concurrency).
- `DELETE /api/v1/events/{id}/registrations` (`👤 Self / 🛡️ OrgAdmin`) — Cancel registration (promotes next waitlisted student via `WaitlistPromotedDomainEvent`).
- `GET /api/v1/sessions/{id}/attendance` (`🛡️ OrgAdmin`) — View session attendance log (`RecordedByStudentId` included).
- `POST /api/v1/sessions/{id}/attendance/check-in-code` (`🛡️ OrgAdmin`) — Generate 60-second rotating HMAC-SHA256 check-in code.
- `POST /api/v1/sessions/{id}/attendance` (`🛡️ OrgAdmin`) — Organizer check-in (`QRCode`, `RFID`, `ManualByOrganizer`, awards `+25 XP`).
- `POST /api/v1/sessions/{id}/attendance/self-check-in` (`🔒 Auth`) — Student self check-in using active 60s rotating code (`+25 XP`, capped at `MaxSelfCheckInsPerWindow`).
- `DELETE /api/v1/sessions/{id}/attendance/{studentId}` (`🛡️ OrgAdmin`) — Revoke an attendance record.
- `GET /api/v1/sessions/{id}/attendance/report` (`🛡️ OrgAdmin`) — Printable HTML attendance report (`SessionAttendanceReport.html`).

### 13. Gamification Scoring Ledger & Leaderboard
*Controller: `GamificationController`*
- `GET /api/v1/students/{id}/score` (`🔒 Auth`) — Student points total, level, threshold bounds, and progress percentage.
- `GET /api/v1/students/{id}/score-transactions` (`👤 Self / 👑 PlatformAdmin`) — Paginated append-only score ledger history.
- `POST /api/v1/score-transactions` (`👑 PlatformAdmin`) — Manual score adjustment (logged to `AuditLog`).
- `GET /api/v1/gamification/leaderboard` (`🔒 Auth`) — Paginated ranked campus leaderboard.

### 14. Badges & Achievements
*Controller: `BadgesController`*
- `GET /api/v1/badges` (`🔒 Auth`) — 1-hour cached master badges catalog.
- `POST /api/v1/badges`, `PUT / DELETE /api/v1/badges/{id}` (`👑 PlatformAdmin`) — Badge catalog CRUD.
- `GET /api/v1/students/{id}/badges` (`🔒 Auth`) — Badges unlocked by a student.
- `POST /api/v1/students/{id}/badges/{badgeId}` (`👑 PlatformAdmin`) — Manually award a badge.

### 15. Notifications Inbox
*Controller: `NotificationsController`*
- `GET /api/v1/notifications` (`👤 Owner`) — In-app notification inbox (`?unreadOnly=true`).
- `PATCH /api/v1/notifications/{id}` (`👤 Owner`) — Mark single notification as read.
- `PATCH /api/v1/notifications/mark-all-read` (`👤 Owner`) — Mark all notifications as read.
- `DELETE /api/v1/notifications/{id}` (`👤 Owner`) — Soft-delete notification.

### 16. Platform Administration, Governance & Reports
*Controller: `AdminController`*
- `GET /api/v1/admin/dashboard` (`👑 PlatformAdmin`) — Platform counts and role distribution.
- `PATCH /api/v1/admin/students/{id}/role` (`👑 PlatformAdmin`) — Role update (guarded by `AllowRuntimePlatformAdminPromotion`; rotates `SecurityStamp` and writes `AuditLog`).
- `GET /api/v1/admin/soft-deleted` & `POST /api/v1/admin/restore/{entityType}/{id}` (`👑 PlatformAdmin`) — Soft-deleted record inspection and universal restore.
- `GET /api/v1/admin/audit-logs` (`👑 PlatformAdmin`) — Paginated immutable `AuditLog` query (`action`, `entityType`, `actorStudentId`).
- `GET /api/v1/admin/reports/platform-summary.pdf` & `POST /api/v1/admin/reports/send-summary` (`👑 PlatformAdmin`) — Generate QuestPDF governance summary or email it to an admin.
- `GET /api/v1/admin/templates` & `GET /api/v1/admin/templates/preview/{templateName}` (`👑 PlatformAdmin`) — List and preview HTML email templates.
- `POST /api/v1/admin/communications/leaderboard-digest` & `POST /api/v1/admin/communications/project-matches` (`👑 PlatformAdmin`) — Dispatch or dry-run opt-in digest emails.

---

## Machine-Readable Contracts

- **OpenAPI v3 JSON (116 operations):** [`docs/api/openapi.json`](../docs/api/openapi.json)
- **Postman Collection:** [`docs/api/postman/TeamUp_Collection.json`](../docs/api/postman/TeamUp_Collection.json)
- **Postman Environment (`http://localhost:5186`):** [`docs/api/postman/TeamUp_Environment.json`](../docs/api/postman/TeamUp_Environment.json)
