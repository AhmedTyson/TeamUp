# TeamUp — RESTful API Endpoint Tracking & Assignment Document

## 1. Architectural Standards & Conventions

- **Base Route:** `/api/v1`
- **Resource Nouns:** Plural lowercase kebab-case (e.g., `/api/v1/projects`, `/api/v1/score-transactions`).
- **HTTP Methods:**
  - `GET`: Safe & idempotent retrieval.
  - `POST`: Create resource (`201 Created` with `Location` header) or trigger business action.
  - `PUT`: Full replacement of resource or idempotent state assignment.
  - `PATCH`: Partial update (e.g., status transition).
  - `DELETE`: **Soft delete** (`204 No Content`). Sets `DeletedAt = DateTime.UtcNow`.
- **Soft Delete Rule:** Single column `DeletedAt DATETIME2 NULL` (`NULL` = active, timestamp = soft-deleted). Filtered unique indexes enforce `WHERE [DeletedAt] IS NULL`.
- **Error Format:** RFC 7807 Problem Details (`application/problem+json`).
- **Authentication:** `Authorization: Bearer <JWT>` containing `sub` (StudentID) and `role` (`Student`, `OrgAdmin`, `PlatformAdmin`).
- **Pagination:** Query parameters `?page=1&pageSize=20`. Response wrapped in `PagedResult<T>`.

---

## 2. Team Member Responsibility Roster (4 Members)

| Code | Team Member Focus | Functional Modules Owned |
|---|---|---|
| **M1** | Member 1 (Tech Lead / Auth & Identity) | Authentication, Universities, Faculties, Students, Profiles, Interests, Health |
| **M2** | Member 2 (Projects & Teams) | Skills, StudentSkills, Projects, RequiredSkills, Members, Applications, Tasks |
| **M3** | Member 3 (Student Activities & Events) | Organizations, OrgMembers, RecruitmentApps, Events, Sessions, Registrations, Attendance |
| **M4** | Member 4 (Gamification & Alerts) | Scoring (Ledger), Badges, StudentBadges, Notifications, OpenAPI |

---

## 3. Authorization & Status Legend

| Column | Icon / Label | Meaning |
|---|---|---|
| **Authorize** | `🌐 Public` | Open to all callers; no authentication header required. |
| | `🔒 Auth` | Requires valid student or admin JWT token. |
| | `👤 Owner` | Restricted to the resource owner (self or project/org creator). |
| | `👥 Member` | Restricted to confirmed members of the project team. |
| | `🛡️ OrgAdmin` | Restricted to board/admin members of the host student organization. |
| | `👑 PlatformAdmin` | Super-admin access across the entire university platform. |
| **Status** | `✅ Done` | Implemented, tested, and verified in codebase. |
| | `🟡 In Progress` | Currently under active development. |
| | `⏳ Pending` | Scheduled in implementation plan backlog (Tasks T12–T27). |
| | `🔴 Missing` | Identified requirement awaiting scheduling. |

---

## 4. Master Endpoint Tracking Table (95 Endpoints)

### Phase 0: System & Health

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | GET | `/health` | Application & database connectivity health check. | 🌐 Public | M1 | ✅ Done |

---

### Module 1: Authentication & Identity (Owned by M1)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 2 | POST | `/api/v1/auth/register` | Student user registration with faculty affiliation. | 🌐 Public | M1 | ⏳ Pending (T12) |
| 3 | POST | `/api/v1/auth/login` | Login with email/password; returns JWT access & refresh token. | 🌐 Public | M1 | ⏳ Pending (T12) |
| 4 | POST | `/api/v1/auth/refresh` | Rotate expired access token using valid refresh token. | 🌐 Public | M1 | ⏳ Pending (T13) |
| 5 | POST | `/api/v1/auth/logout` | Invalidate active session and revoke refresh token. | 🔒 Auth | M1 | ⏳ Pending (T13) |
| 6 | POST | `/api/v1/auth/forgot-password` | Request password reset token via university email. | 🌐 Public | M1 | ⏳ Pending |
| 7 | POST | `/api/v1/auth/reset-password` | Reset password using verified reset token. | 🌐 Public | M1 | ⏳ Pending |

---

### Module 2: Universities & Faculties (Owned by M1)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 8 | GET | `/api/v1/universities` | List all accredited universities (paginated). | 🔒 Auth | M1 | ⏳ Pending (T14) |
| 9 | POST | `/api/v1/universities` | Create new accredited university. | 👑 PlatformAdmin | M1 | ⏳ Pending (T14) |
| 10 | GET | `/api/v1/universities/{id}` | Get university details and list of constituent faculties. | 🔒 Auth | M1 | ⏳ Pending (T14) |
| 11 | PUT | `/api/v1/universities/{id}` | Update university name, city, website. | 👑 PlatformAdmin | M1 | ⏳ Pending (T14) |
| 12 | DELETE | `/api/v1/universities/{id}` | Soft-delete university (`DeletedAt = UtcNow`). | 👑 PlatformAdmin | M1 | ⏳ Pending (T14) |
| 13 | GET | `/api/v1/universities/{id}/faculties` | List faculties within a university. | 🔒 Auth | M1 | ⏳ Pending (T14) |
| 14 | POST | `/api/v1/universities/{id}/faculties` | Add new faculty/college to university. | 👑 PlatformAdmin | M1 | ⏳ Pending (T14) |
| 15 | PUT | `/api/v1/faculties/{id}` | Update faculty name or academic code. | 👑 PlatformAdmin | M1 | ⏳ Pending (T14) |
| 16 | DELETE | `/api/v1/faculties/{id}` | Soft-delete faculty (`DeletedAt = UtcNow`). | 👑 PlatformAdmin | M1 | ⏳ Pending (T14) |

---

### Module 3: Students, Profiles & Interests (Owned by M1)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 17 | GET | `/api/v1/students` | Search / filter student directory by faculty or academic year. | 🔒 Auth | M1 | ⏳ Pending (T15) |
| 18 | GET | `/api/v1/students/{id}` | Public student portfolio card & badges. | 🔒 Auth | M1 | ⏳ Pending (T15) |
| 19 | GET | `/api/v1/students/me` | Fetch authenticated student's full profile and credentials. | 👤 Owner | M1 | ⏳ Pending (T15) |
| 20 | PATCH | `/api/v1/students/me` | Update student academic year or personal names. | 👤 Owner | M1 | ⏳ Pending (T15) |
| 21 | DELETE | `/api/v1/students/me` | Soft-delete student account. | 👤 Owner / 👑 Admin | M1 | ⏳ Pending (T15) |
| 22 | GET | `/api/v1/students/{id}/profile` | Get student extended portfolio (Bio, Photo, GitHub, LinkedIn). | 🔒 Auth | M1 | ⏳ Pending (T15) |
| 23 | PUT | `/api/v1/students/me/profile` | Upsert bio, portfolio photo, GitHub URL, LinkedIn URL. | 👤 Owner | M1 | ⏳ Pending (T15) |
| 24 | GET | `/api/v1/students/{id}/interests` | Get student list of academic/extracurricular interests. | 🔒 Auth | M1 | ⏳ Pending (T15) |
| 25 | POST | `/api/v1/students/me/interests` | Add an interest tag (e.g. `AI`, `Robotics`). | 👤 Owner | M1 | ⏳ Pending (T15) |
| 26 | DELETE | `/api/v1/students/me/interests/{tag}` | Remove an interest tag. | 👤 Owner | M1 | ⏳ Pending (T15) |

---

### Module 4: Skills Management (Owned by M2)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 27 | GET | `/api/v1/skills` | List standard skills catalog grouped by category. | 🔒 Auth | M2 | ⏳ Pending (T16) |
| 28 | POST | `/api/v1/skills` | Add new technical or soft skill to master catalog. | 👑 PlatformAdmin | M2 | ⏳ Pending (T16) |
| 29 | PUT | `/api/v1/skills/{id}` | Update skill name, category, or description. | 👑 PlatformAdmin | M2 | ⏳ Pending (T16) |
| 30 | DELETE | `/api/v1/skills/{id}` | Soft-delete skill (`DeletedAt = UtcNow`). | 👑 PlatformAdmin | M2 | ⏳ Pending (T16) |
| 31 | GET | `/api/v1/students/{id}/skills` | List skills claimed by student with proficiency ratings. | 🔒 Auth | M2 | ⏳ Pending (T16) |
| 32 | PUT | `/api/v1/students/me/skills/{skillId}` | Claim skill / update proficiency and years of experience. | 👤 Owner | M2 | ⏳ Pending (T16) |
| 33 | DELETE | `/api/v1/students/me/skills/{skillId}` | Remove claimed skill from student profile. | 👤 Owner | M2 | ⏳ Pending (T16) |

---

### Module 5: Projects & Teams (Owned by M2)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 34 | GET | `/api/v1/projects` | Search projects (filter by skill, status, keyword). | 🔒 Auth | M2 | ⏳ Pending (T17) |
| 35 | POST | `/api/v1/projects` | Create project (creator automatically assigned Team Lead). | 🔒 Auth | M2 | ⏳ Pending (T17) |
| 36 | GET | `/api/v1/projects/{id}` | Project details, members, and required skills. | 🔒 Auth | M2 | ⏳ Pending (T17) |
| 37 | PATCH | `/api/v1/projects/{id}` | Update project title, description, or recruiting status. | 👤 Creator | M2 | ⏳ Pending (T17) |
| 38 | DELETE | `/api/v1/projects/{id}` | Soft-delete project & cascade soft-delete to tasks. | 👤 Creator | M2 | ⏳ Pending (T17) |
| 39 | GET | `/api/v1/projects/{id}/required-skills` | Get required skills list for candidate teammate matching. | 🔒 Auth | M2 | ⏳ Pending (T17) |
| 40 | PUT | `/api/v1/projects/{id}/required-skills/{skillId}` | Upsert required skill, minimum level, and mandatory flag. | 👤 Creator | M2 | ⏳ Pending (T17) |
| 41 | DELETE | `/api/v1/projects/{id}/required-skills/{skillId}` | Remove required skill from project. | 👤 Creator | M2 | ⏳ Pending (T17) |
| 42 | GET | `/api/v1/projects/{id}/members` | List confirmed project team members and assigned roles. | 🔒 Auth | M2 | ⏳ Pending (T18) |
| 43 | PATCH | `/api/v1/projects/{id}/members/{studentId}` | Update member role (e.g., `Scrum Master`, `UI Designer`). | 👤 Creator | M2 | ⏳ Pending (T18) |
| 44 | DELETE | `/api/v1/projects/{id}/members/{studentId}` | Remove member from project (unassigns their active tasks). | 👤 Creator / Self | M2 | ⏳ Pending (T18) |

---

### Module 6: Project Applications (Owned by M2)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 45 | GET | `/api/v1/projects/{id}/applications` | List applications to join project team. | 👤 Creator | M2 | ⏳ Pending (T18) |
| 46 | POST | `/api/v1/projects/{id}/applications` | Submit application to join project team with pitch message. | 🔒 Auth | M2 | ⏳ Pending (T18) |
| 47 | PATCH | `/api/v1/projects/{id}/applications/{appId}` | Accept or Reject application (Accept adds to `ProjectMember`). | 👤 Creator | M2 | ⏳ Pending (T18) |
| 48 | DELETE | `/api/v1/projects/{id}/applications/{appId}` | Withdraw pending application. | 👤 Applicant | M2 | ⏳ Pending (T18) |

---

### Module 7: Project Tasks (Owned by M2)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 49 | GET | `/api/v1/projects/{id}/tasks` | List sprint tasks in project board. | 👥 Member | M2 | ⏳ Pending (T19) |
| 50 | POST | `/api/v1/projects/{id}/tasks` | Create deliverable task in project board. | 👥 Member | M2 | ⏳ Pending (T19) |
| 51 | GET | `/api/v1/tasks/{id}` | Get task details, assignee, status, due date. | 👥 Member | M2 | ⏳ Pending (T19) |
| 52 | PATCH | `/api/v1/tasks/{id}` | Update task status (`Todo`, `InProgress`, `Done`) or priority. | 👥 Member | M2 | ⏳ Pending (T19) |
| 53 | PUT | `/api/v1/tasks/{id}/assignee` | Assign task to a confirmed project team member. | 👤 Creator | M2 | ⏳ Pending (T19) |
| 54 | DELETE | `/api/v1/tasks/{id}/assignee` | Unassign student from task. | 👤 Creator | M2 | ⏳ Pending (T19) |
| 55 | DELETE | `/api/v1/tasks/{id}` | Soft-delete task. | 👤 Creator | M2 | ⏳ Pending (T19) |

---

### Module 8: Student Activities & Organizations (Owned by M3)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 56 | GET | `/api/v1/organizations` | Browse student clubs, chapters (IEEE, GDG) & societies. | 🔒 Auth | M3 | ⏳ Pending (T20) |
| 57 | POST | `/api/v1/organizations` | Register new student organization. | 👑 Admin / Verified | M3 | ⏳ Pending (T20) |
| 58 | GET | `/api/v1/organizations/{id}` | Organization details, board listing, and organized events. | 🔒 Auth | M3 | ⏳ Pending (T20) |
| 59 | PATCH | `/api/v1/organizations/{id}` | Update organization description, category, or logo. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T20) |
| 60 | DELETE | `/api/v1/organizations/{id}` | Soft-delete organization. | 👑 PlatformAdmin | M3 | ⏳ Pending (T20) |
| 61 | GET | `/api/v1/organizations/{id}/members` | List active organization board and general members. | 🔒 Auth | M3 | ⏳ Pending (T20) |
| 62 | PATCH | `/api/v1/organizations/{id}/members/{studentId}` | Update member role (e.g., `President`, `Head of PR`). | 🛡️ OrgAdmin | M3 | ⏳ Pending (T20) |
| 63 | DELETE | `/api/v1/organizations/{id}/members/{studentId}` | Remove member from organization. | 🛡️ OrgAdmin / Self | M3 | ⏳ Pending (T20) |

---

### Module 9: Membership Applications (Owned by M3)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 64 | GET | `/api/v1/organizations/{id}/applications` | List recruitment applications for organization. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T21) |
| 65 | POST | `/api/v1/organizations/{id}/applications` | Submit application to join student club during recruitment. | 🔒 Auth | M3 | ⏳ Pending (T21) |
| 66 | PATCH | `/api/v1/organizations/{id}/applications/{appId}` | Accept or Reject candidate membership application. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T21) |
| 67 | DELETE | `/api/v1/organizations/{id}/applications/{appId}` | Withdraw pending membership application. | 👤 Applicant | M3 | ⏳ Pending (T21) |

---

### Module 10: Events & Workshops (Owned by M3)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 68 | GET | `/api/v1/events` | Browse upcoming workshops, hackathons & competitions. | 🔒 Auth | M3 | ⏳ Pending (T22) |
| 69 | POST | `/api/v1/events` | Create new event hosted by an organization. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T22) |
| 70 | GET | `/api/v1/events/{id}` | Event agenda, location, dates, and registration count. | 🔒 Auth | M3 | ⏳ Pending (T22) |
| 71 | PATCH | `/api/v1/events/{id}` | Update event venue, dates, or registration capacity. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T22) |
| 72 | DELETE | `/api/v1/events/{id}` | Soft-delete event & cascade soft-delete to sessions. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T22) |

---

### Module 11: Event Sessions (Owned by M3)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 73 | GET | `/api/v1/events/{id}/sessions` | Get session timetable/tracks inside a multi-part event. | 🔒 Auth | M3 | ⏳ Pending (T22) |
| 74 | POST | `/api/v1/events/{id}/sessions` | Add session block to event timetable. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T22) |
| 75 | PUT | `/api/v1/sessions/{id}` | Update session timing, title, or room assignment. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T22) |
| 76 | DELETE | `/api/v1/sessions/{id}` | Soft-delete session (`DeletedAt = UtcNow`). | 🛡️ OrgAdmin | M3 | ⏳ Pending (T22) |

---

### Module 12: Event Registrations & Attendance (Owned by M3)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 77 | GET | `/api/v1/events/{id}/registrations` | View registered attendee list & confirmed capacity. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T23) |
| 78 | POST | `/api/v1/events/{id}/registrations` | Register authenticated student for event (capacity checked). | 🔒 Auth | M3 | ⏳ Pending (T23) |
| 79 | DELETE | `/api/v1/events/{id}/registrations` | Cancel event registration. | 👤 Self / 🛡️ OrgAdmin | M3 | ⏳ Pending (T23) |
| 80 | GET | `/api/v1/sessions/{id}/attendance` | View verified check-in list for session. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T24) |
| 81 | POST | `/api/v1/sessions/{id}/attendance` | Check-in attendee (QRCode, RFID, or Manual verification). | 🛡️ OrgAdmin | M3 | ⏳ Pending (T24) |
| 82 | DELETE | `/api/v1/sessions/{id}/attendance/{studentId}` | Revoke attendance check-in record. | 🛡️ OrgAdmin | M3 | ⏳ Pending (T24) |

---

### Module 13: Gamification & Scoring Ledger (Owned by M4)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 83 | GET | `/api/v1/students/{id}/score` | Get total points, current level (`FLOOR(SQRT(pts/100))+1`). | 🔒 Auth | M4 | ⏳ Pending (T25) |
| 84 | GET | `/api/v1/students/{id}/score-transactions` | View immutable audit ledger of points awarded/deducted. | 👤 Self / 👑 Admin | M4 | ⏳ Pending (T25) |
| 85 | POST | `/api/v1/score-transactions` | Manual bonus points award or administrative adjustment. | 👑 PlatformAdmin | M4 | ⏳ Pending (T25) |

---

### Module 14: Badges & Achievements (Owned by M4)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 86 | GET | `/api/v1/badges` | View master badge catalog and unlock criteria. | 🔒 Auth | M4 | ⏳ Pending (T26) |
| 87 | POST | `/api/v1/badges` | Create new badge definition (icon, threshold). | 👑 PlatformAdmin | M4 | ⏳ Pending (T26) |
| 88 | PUT | `/api/v1/badges/{id}` | Update badge criteria type or icon asset. | 👑 PlatformAdmin | M4 | ⏳ Pending (T26) |
| 89 | DELETE | `/api/v1/badges/{id}` | Soft-delete badge definition. | 👑 PlatformAdmin | M4 | ⏳ Pending (T26) |
| 90 | GET | `/api/v1/students/{id}/badges` | View badges unlocked by student with award timestamps. | 🔒 Auth | M4 | ⏳ Pending (T26) |
| 91 | POST | `/api/v1/students/{id}/badges/{badgeId}` | Manually award achievement badge to student. | 👑 PlatformAdmin | M4 | ⏳ Pending (T26) |

---

### Module 15: Notifications Inbox (Owned by M4)

| # | Method | Route | Description | Authorize | Backend | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 92 | GET | `/api/v1/notifications` | Current student in-app notifications inbox. | 👤 Owner | M4 | ⏳ Pending (T27) |
| 93 | PATCH | `/api/v1/notifications/{id}` | Mark single notification as read (`IsRead = true`). | 👤 Owner | M4 | ⏳ Pending (T27) |
| 94 | PATCH | `/api/v1/notifications/mark-all-read` | Mark all unread notifications in inbox as read. | 👤 Owner | M4 | ⏳ Pending (T27) |
| 95 | DELETE | `/api/v1/notifications/{id}` | Soft-delete notification from inbox. | 👤 Owner | M4 | ⏳ Pending (T27) |
