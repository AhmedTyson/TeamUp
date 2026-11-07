# TeamUp — API Reference

> **RESTful API Architectural Reference & Narrative Module Guide** · Standards, envelope conventions, authentication matrices, and domain operation overviews across all 99 endpoints.

> **Sources:**
> - [docs/api/API-PROPOSAL.md](../docs/api/API-PROPOSAL.md)
> - [docs/api/API-ENDPOINTS.md](../docs/api/API-ENDPOINTS.md)
> - [src/TeamUp.Api/Controllers](../src/TeamUp.Api/Controllers)
> - [src/TeamUp.Application/Common/Models](../src/TeamUp.Application/Common/Models)

---

## Table of Contents

1. [Architectural Standards & Response Envelopes](#architectural-standards--response-envelopes)
2. [Authentication Schemes & Authorization Roles](#authentication-schemes--authorization-roles)
3. [Rate Limiting Policies](#rate-limiting-policies)
4. [Domain Modules Overview](#domain-modules-overview)
   - [System & Health](#0-system--health)
   - [Module 1: Authentication & Identity](#1-authentication--identity)
   - [Module 2: Universities & Faculties](#2-universities--faculties)
   - [Module 3: Student Discovery & Profiles](#3-student-discovery--profiles)
   - [Module 4: Skills Master Catalog & Claims](#4-skills-master-catalog--claims)
   - [Module 5: Projects & Team Formation](#5-projects--team-formation)
   - [Module 6: Project Applications](#6-project-applications)
   - [Module 7: Project Sprint Tasks Board](#7-project-sprint-tasks-board)
   - [Module 8: Student Organizations & Memberships](#8-student-organizations--memberships)
   - [Module 9: Membership Recruitment Applications](#9-membership-recruitment-applications)
   - [Module 10: Campus Events & Schedules](#10-campus-events--schedules)
   - [Module 11: Event Sessions Timetable](#11-event-sessions-timetable)
   - [Module 12: Event Registrations & Attendance](#12-event-registrations--attendance)
   - [Module 13: Gamification Scoring Ledger](#13-gamification-scoring-ledger)
   - [Module 14: Badges & Achievements](#14-badges--achievements)
   - [Module 15: Notifications Inbox](#15-notifications-inbox)
   - [Module 16: Platform Administration & Governance](#16-platform-administration--governance)
5. [Machine-Readable Contracts](#machine-readable-contracts)

---

## Architectural Standards & Response Envelopes

- **Base Route:** `/api/v1`
- **Naming Conventions:** Kebab-case plural nouns (e.g., `/api/v1/score-transactions`, `/api/v1/project-tasks`).
- **Standard Success Envelope (`BaseResponse<T>`):**
  ```json
  {
    "success": true,
    "message": "Resource retrieved successfully.",
    "statusCode": 200,
    "data": { ... },
    "errors": null,
    "timestamp": "2026-11-06T00:30:00Z"
  }
  ```
- **Standard Error Envelope (`BaseResponse`):**
  ```json
  {
    "success": false,
    "message": "Validation failed.",
    "statusCode": 400,
    "data": null,
    "errors": [
      { "field": "Password", "message": "Password must be at least 8 characters long." }
    ],
    "timestamp": "2026-11-06T00:30:00Z"
  }
  ```

---

## Authentication Schemes & Authorization Roles

TeamUp enforces token-based authentication via the `Authorization: Bearer <JWT>` header:

| Tier | Icon | Meaning / Required Claims |
|---|---|---|
| **Public** | `🌐 Public` | Anonymous access allowed; no credentials required. |
| **Auth** | `🔒 Auth` | Requires a valid, unexpired JWT bearer token from any authenticated student or admin. |
| **Owner** | `👤 Owner` | Caller's `sub` claim must match the resource creator/owner. |
| **Member** | `👥 Member` | Caller must be a confirmed member of the project team or student organization. |
| **OrgAdmin** | `🛡️ OrgAdmin` | Caller must hold an active board or administrator seat in the host student organization. |
| **PlatformAdmin** | `👑 PlatformAdmin` | Super-admin access across all university tenants and resources. |

---

## Rate Limiting Policies

Authentication endpoints (`/api/v1/auth/*`) are governed by an IP-partitioned rate limiter configured for **60 requests per minute**. Excess requests receive `429 Too Many Requests`.

---

## Domain Modules Overview

### 0. System & Health
- `GET /health` — Verifies container health and database connectivity. Returns `200 OK`.

### 1. Authentication & Identity
- `POST /api/v1/auth/register` — Student self-registration with academic faculty affiliation.
- `POST /api/v1/auth/login` — Verifies credentials, returns short-lived JWT access token and refresh token.
- `POST /api/v1/auth/refresh` — Cryptographic refresh token rotation; old token is immediately revoked.
- `POST /api/v1/auth/logout` — Revokes active refresh token and terminates the session.
- `POST /api/v1/auth/forgot-password` — Generates a 15-minute cryptographically secure reset token.
- `POST /api/v1/auth/reset-password` — Resets account password using verified token.

### 2. Universities & Faculties
- `GET /api/v1/universities/catalog` — High-performance cached endpoint for registration dropdowns.
- `GET /api/v1/universities` — Paginated university directory with keyword search.
- `POST / PUT / DELETE /api/v1/universities` — Administrative university catalog management.
- `GET / POST / PUT / DELETE /api/v1/universities/{id}/faculties` — Hierarchical faculty management.

### 3. Student Discovery & Profiles
- `GET /api/v1/students` — Directory search filtering by faculty, academic year, and keywords.
- `GET /api/v1/students/{id}` — Public student portfolio card (bio, claimed skills, badges).
- `GET / PATCH /api/v1/students/me` — Authenticated profile management and bio updates.
- `DELETE /api/v1/students/me` — Self-service account soft-deletion (`DeletedAt = UtcNow`).
- `GET / PUT /api/v1/students/me/interests` — Student interest tags lifecycle.

### 4. Skills Master Catalog & Claims
- `GET /api/v1/skills` — Hierarchical skills master catalog with categories.
- `POST / PUT / DELETE /api/v1/skills` — Admin skills catalog administration.
- `GET /api/v1/students/{id}/skills` — List claimed skills with proficiency ratings (1–5).
- `PUT / DELETE /api/v1/students/me/skills/{skillId}` — Claim, update proficiency, or remove a skill.

### 5. Projects & Team Formation
- `GET /api/v1/projects` — Filterable project catalog (status, faculty, required skills).
- `POST /api/v1/projects` — Create project with team size targets and vacancy definitions.
- `GET / PUT / DELETE /api/v1/projects/{id}` — Project management and status lifecycle transitions.
- `POST / DELETE /api/v1/projects/{id}/required-skills` — Specify required skills for vacancies.
- `GET / POST / DELETE /api/v1/projects/{id}/members` — Project team roster management.

### 6. Project Applications
- `POST /api/v1/projects/{id}/applications` — Submit candidate application for an open vacancy.
- `GET /api/v1/projects/{id}/applications` — Project owner reviews incoming candidate applications.
- `PATCH /api/v1/projects/{id}/applications/{appId}/accept` — Accept application (adds to project roster).
- `PATCH /api/v1/projects/{id}/applications/{appId}/reject` — Reject application with feedback.

### 7. Project Sprint Tasks Board
- `GET /api/v1/projects/{id}/tasks` — Sprint task board grouped by status (`Todo`, `InProgress`, `Completed`).
- `POST / PUT / DELETE /api/v1/projects/{id}/tasks` — Task creation and detail updates.
- `PATCH /api/v1/projects/{id}/tasks/{taskId}/status` — Move task between sprint columns.
- `PATCH /api/v1/projects/{id}/tasks/{taskId}/assign` — Assign member to deliverable.

### 8. Student Organizations & Memberships
- `GET /api/v1/organizations` — Public campus student activities and clubs directory.
- `POST / PUT / DELETE /api/v1/organizations` — Register student club and manage leadership.
- `GET / POST / DELETE /api/v1/organizations/{id}/members` — Executive board and member roster management.

### 9. Membership Recruitment Applications
- `POST /api/v1/organizations/{id}/applications` — Apply for student organization committee recruitment.
- `GET / PATCH /api/v1/organizations/{id}/applications` — OrgAdmin reviews and accepts/rejects recruits.

### 10. Campus Events & Schedules
- `GET /api/v1/events` — Campus event calendar (workshops, hackathons, seminars).
- `POST / PUT / DELETE /api/v1/events` — Event creation with seat capacity limits.
- `GET /api/v1/events/{id}` — Detailed event schedule, venue, and registration status.

### 11. Event Sessions Timetable
- `GET /api/v1/events/{id}/sessions` — Multi-session breakdown for multi-day workshops.
- `POST / PUT / DELETE /api/v1/events/{id}/sessions` — Schedule individual session slots, dates, and rooms.

### 12. Event Registrations & Attendance
- `POST /api/v1/events/{id}/register` — Student registers for event with waitlist transitions.
- `DELETE /api/v1/events/{id}/register` — Cancel registration (promotes next waitlisted student).
- `POST /api/v1/events/{id}/sessions/{sessionId}/check-in` — QR Code / PIN code attendance check-in.
- `GET /api/v1/events/{id}/sessions/{sessionId}/attendance` — Session attendance roster for instructors.

### 13. Gamification Scoring Ledger
- `GET /api/v1/students/{id}/score-summary` — Current points balance, current level, and XP to next level.
- `GET /api/v1/students/{id}/score-transactions` — Append-only historical transaction audit log.
- `POST /api/v1/gamification/award` — Admin / system award points trigger.

### 14. Badges & Achievements
- `GET /api/v1/badges` — Master badges catalog with achievement criteria descriptions.
- `POST / PUT / DELETE /api/v1/badges` — Admin badge creation and criteria configuration.
- `GET /api/v1/students/{id}/badges` — List verifiable badges unlocked by a student.

### 15. Notifications Inbox
- `GET /api/v1/notifications` — In-app notification inbox with unread status flags.
- `PATCH /api/v1/notifications/{id}/read` — Mark individual notification as read.
- `PATCH /api/v1/notifications/read-all` — Mark entire inbox as read.

### 16. Platform Administration & Governance
- `GET /api/v1/admin/dashboard` — Platform telemetry (total active students, projects, events).
- `PATCH /api/v1/admin/students/{id}/lock` — Suspend student account.
- `POST /api/v1/admin/restore/{entityType}/{id}` — Universal soft-delete restoration (`.IgnoreQueryFilters()`).

---

## Machine-Readable Contracts

- **OpenAPI v3 JSON:** [`docs/api/openapi.json`](../docs/api/openapi.json)
- **Postman Collection:** [`docs/api/postman/TeamUp_Collection.json`](../docs/api/postman/TeamUp_Collection.json)
- **Postman Environment:** [`docs/api/postman/TeamUp_Environment.json`](../docs/api/postman/TeamUp_Environment.json)
