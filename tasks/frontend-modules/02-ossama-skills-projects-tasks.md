# Frontend Module 02: Skills Matrix, Projects, Team Formation & Sprint Kanban Board

> **Owner:** M2 — Ossama  
> **API Operations Owned:** 30 (`Skills` 5 · `StudentSkills` 3 · `Projects` 17 · `Tasks` 5)  
> **Master Spec:** [`tasks/frontend-plan.md`](../frontend-plan.md)  
> **Owned Folders:** `frontend/src/api/{skills.api,projects.api,tasks.api}.ts`, `frontend/src/features/skills/`, `frontend/src/features/projects/`

---

## 1. Scope & Responsibilities

M2 owns the core student collaboration loop: discovering projects by skill and status, publishing project ideas with mandatory/preferred skill requirements, vetting applicants into teams, executing sprint tasks on a 4-column Kanban board (triggering `+50 XP` domain rewards on `Done`), and managing the master skill catalog + student proficiency claims.

---

## 2. Owned Routes

| Route | Guard | View Description |
|---|---|---|
| `/skills` | Auth | Master Skill Catalog grouped/filtered by `category` & `search`, personal **"My Claimed Skills"** drawer/tab (`ProficiencyLevel` + `yearsOfExperience`), plus `PlatformAdmin` Create/Edit/Delete skill modals |
| `/projects` | Auth | Project Discovery Hub (Handshake dark spotlight cards with team capacity meters `teamSize / maxTeamSize`, status pills, keyword/status/skillId filters, pagination, and **Create Project** CTA) |
| `/projects/new` | Auth | Dedicated Create Project wizard (Title, Description, MaxTeamSize + immediate step to attach Required Skills) |
| `/projects/:id` | Auth | Tabbed Project Workspace: **Overview & Required Skills**, **Team Roster & Roles**, **Applications**, and **Sprint Kanban Board** (`Todo` → `InProgress` → `InReview` → `Done`) |

---

## 3. Owned API Endpoints (30 Operations)

| Group | Method | Endpoint | Auth / Notes |
|---|---|---|---|
| **Skills** | `GET` | `/api/v1/skills` | Auth · `?category=&search=` (backend cached) |
| **Skills** | `GET` | `/api/v1/skills/{id}` | Auth |
| **Skills** | `POST` | `/api/v1/skills` | `PlatformAdmin` → `201` |
| **Skills** | `PUT` | `/api/v1/skills/{id}` | `PlatformAdmin` → `200` |
| **Skills** | `DELETE` | `/api/v1/skills/{id}` | `PlatformAdmin` → `204` |
| **StudentSkills** | `GET` | `/api/v1/students/{id}/skills` | Auth · used in `/skills` and exported widget for M4 profiles |
| **StudentSkills** | `PUT` | `/api/v1/students/me/skills/{skillId}` | Auth (Self) · `{ proficiencyLevel, yearsOfExperience }` |
| **StudentSkills** | `DELETE` | `/api/v1/students/me/skills/{skillId}` | Auth (Self) → `204` |
| **Projects** | `GET` | `/api/v1/projects` | Auth · `?keyword=&status=&skillId=&page=&pageSize=` |
| **Projects** | `POST` | `/api/v1/projects` | Auth → `201 ProjectDetailDto` (creator becomes Team Lead) |
| **Projects** | `GET` | `/api/v1/projects/{id}` | Auth → `ProjectDetailDto` (includes `members` & `requiredSkills`) |
| **Projects** | `PATCH` | `/api/v1/projects/{id}` | Creator/Lead → `200` (status `Completed` awards delivery XP) |
| **Projects** | `DELETE` | `/api/v1/projects/{id}` | Creator/Lead/Admin → `204` |
| **Required Skills** | `GET` | `/api/v1/projects/{id}/required-skills` | Auth |
| **Required Skills** | `PUT` | `/api/v1/projects/{id}/required-skills/{skillId}` | Creator/Lead · `{ requiredLevel, isMandatory }` → `204` |
| **Required Skills** | `DELETE` | `/api/v1/projects/{id}/required-skills/{skillId}` | Creator/Lead → `204` |
| **Project Members** | `GET` | `/api/v1/projects/{id}/members` | Auth |
| **Project Members** | `PATCH` | `/api/v1/projects/{id}/members/{studentId}` | Creator/Lead · `{ role }` → `204` |
| **Project Members** | `DELETE` | `/api/v1/projects/{id}/members/{studentId}` | Creator/Lead or Self leave → `204` (unassigns member's tasks) |
| **Applications** | `GET` | `/api/v1/projects/{id}/applications` | Creator/Lead/Applicant |
| **Applications** | `POST` | `/api/v1/projects/{id}/applications` | Auth · `{ message }` → `201` (`409` on duplicate/full team) |
| **Applications** | `PATCH` | `/api/v1/projects/{id}/applications/{appId}` | Creator/Lead · `{ status: "Accepted" \| "Rejected" }` (`409` if team hits `maxTeamSize`!) |
| **Applications** | `DELETE` | `/api/v1/projects/{id}/applications/{appId}` | Applicant withdraw → `204` |
| **Project Tasks** | `GET` | `/api/v1/projects/{id}/tasks` | Auth (Team Members) |
| **Project Tasks** | `POST` | `/api/v1/projects/{id}/tasks` | Team Member/Lead · `{ title, description, dueDate, priority }` → `201` |
| **Tasks** | `GET` | `/api/v1/tasks/{id}` | Auth |
| **Tasks** | `PATCH` | `/api/v1/tasks/{id}` | Team Member/Lead · `{ title, description, status, priority, dueDate }` (`Done` triggers `+50 XP`) |
| **Tasks** | `DELETE` | `/api/v1/tasks/{id}` | Team Member/Lead → `204` |
| **Tasks** | `PUT` | `/api/v1/tasks/{id}/assignee` | Team Lead/Member · `{ studentId }` (confirmed team members only!) → `204` |
| **Tasks** | `DELETE` | `/api/v1/tasks/{id}/assignee` | Team Lead/Member → `204` |

---

## 4. Phased Task Checklist

### Phase 1 — Typed APIs, Skills Catalog & Project Discovery (Days 3–6)
- [x] **M2-01:** Create `skills.api.ts`, `projects.api.ts`, and `tasks.api.ts` with typed DTOs and wrappers matching `docs/api/openapi.json`.
- [x] **M2-02:** Build `/skills` page with category filter, debounced keyword search, skill cards, and `PlatformAdmin` Create/Edit/Delete skill controls.
- [x] **M2-03:** Build **"My Claimed Skills"** manager (`PUT/DELETE /students/me/skills/{skillId}`) with proficiency selector and years-of-experience input.
- [x] **M2-04:** Build `/projects` discovery with URL-synced `keyword`, `status`, `skillId`, and `page` filters, `<SpotlightCard>` project tiles, team-capacity progress, and status pills.
- [x] **M2-05:** Build `/projects/:id` overview with lead-only edit/status/delete actions, required-skill editor and match indicators against the logged-in student's claimed skills.

### Phase 2 — Team Recruitment, Applications & Sprint Kanban Board (Days 7–11)
- [x] **M2-06:** Build `/projects/new` creation form with Zod validation (`maxTeamSize >= 1`) and optional required-skill tagging.
- [x] **M2-07:** Build lead-only Required Skills editor supporting add/update/remove, proficiency level, and mandatory toggle.
- [x] **M2-08:** Build Team Roster tab with profile links, inline role updates, and remove/leave confirmation warning about task assignments.
- [x] **M2-09:** Build Applications tab:
  - Non-member view: **Apply to Join** modal (`POST .../applications`) + **Withdraw** button for pending application.
  - Creator/Lead view: Applicant queue with **Accept / Reject** buttons, handling `409 Conflict` cleanly when concurrent accepts hit `maxTeamSize` (`RowVersion` protection).
- [x] **M2-10:** Build Sprint Kanban Board tab (`Todo`, `InProgress`, `InReview`, `Done`):
  - Task cards with `TaskPriority` badges (`Low`, `Medium`, `High`, `Urgent`), due date overdue highlight, assignee avatar, and quick status transition menu (accessible keyboard-first + optional drag-and-drop).
  - Create / Edit / Delete task modal and Assignee picker restricted strictly to current `ProjectDetailDto.members`.
  - On status transition to `Done` (or project `Completed`), fire `invalidateGamification()` and show celebratory lime XP toast (`+50 XP awarded!`).

### Phase 3 — Cross-Module Widget & Testing (Days 12–14)
- [x] **M2-11:** Export `<StudentSkillsPanel studentId={id} editable={isMe} />` for profile integration.
- [x] **M2-12:** Write Vitest + Testing Library suites for Project Discovery filtering, Application Accept `409` handling, Kanban task creation/status move, and Skill Claim upsert/removal.

---

## 5. Exports Required by Other Teammates

- `<StudentSkillsPanel studentId={number} editable?: boolean />` → consumed by **M4** on Student Public Profile and My Profile pages.
- `projectRoutes` route fragment → imported in `frontend/src/app/router.tsx`.
