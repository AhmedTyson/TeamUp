# Frontend Module 03: Student Organizations, Multi-Session Events & Live QR Attendance

> **Owner:** M3 — Ibrahim  
> **API Operations Owned:** 30 (`Organizations` 12 · `Events` 7 · `EventSessions` 2 · `EventRegistrations` 3 · `SessionAttendance` 6)  
> **Master Spec:** [`tasks/frontend-plan.md`](../frontend-plan.md)  
> **Owned Folders:** `frontend/src/api/{organizations.api,events.api,sessions.api,attendance.api}.ts`, `frontend/src/features/organizations/`, `frontend/src/features/events/`

---

## 1. Scope & Responsibilities

M3 delivers the complete campus student activities & live event operations suite: club discovery, scoped multi-tenant OrgAdmin governance (strictly respecting STRIDE `THR-T-01` / `THR-E-01` per-organization admin roles), multi-session event scheduling, concurrency-safe registration & waitlist promotion, live 60-second rotating QR/PIN check-in projection, student self check-in, and sandboxed HTML attendance reports.

---

## 2. Owned Routes

| Route | Guard | View Description |
|---|---|---|
| `/organizations` | Auth | Student Clubs & Societies Directory (`search`, `category`, pagination, `<SpotlightCard>` tiles with logo, category chip, member count; Create Club CTA for `OrgAdmin` / `PlatformAdmin`) |
| `/organizations/:id` | Auth | Organization Profile: banner & safe logo, **Members Roster** (with scoped OrgAdmin role dropdown using `OrganizationRoles`), **Membership Applications** (Apply / Withdraw / Accept / Reject), and **Hosted Events** list |
| `/events` | Auth | Campus Events & Workshops Hub with `search`, `organizationId`, `type` (`Workshop`, `Hackathon`, `Session`, `Competition`), and `from`/`to` date-range filters + Create Event CTA (for OrgAdmins) |
| `/events/:id` | Auth | Event Detail: Hero metadata, real-time Seat Capacity bar (`registeredCount / capacity`), **Register / Cancel Registration** button (displaying `Confirmed` vs `Waitlisted` badge), **Sessions Timetable** (with OrgAdmin CRUD), and **Registrations Roster** |
| `/events/:id/sessions/:sessionId/attendance` | Scoped OrgAdmin / `PlatformAdmin` | Live Attendance Console: **60-Second Rotating QR + PIN Projector** (white high-contrast card with countdown bar & auto-refresh), Manual/RFID check-in form, Verified Attendance Table (displaying `recordedByStudentId` origin & `verificationMethod`), Revoke action, and **HTML Attendance Report** preview/print modal |
| `/check-in` | Auth (Student) | Mobile-first **Student Self Check-In** view (supports `/check-in?sessionId=104&code=XXXXXX` deep link from scanned QR code or manual 6-digit code entry → calls `POST /sessions/{id}/attendance/self-check-in`) |

---

## 3. Owned API Endpoints (30 Operations)

| Group | Method | Endpoint | Auth / Notes |
|---|---|---|---|
| **Organizations** | `GET` | `/api/v1/organizations` | Auth · `?search=&category=&page=&pageSize=` |
| **Organizations** | `POST` | `/api/v1/organizations` | `OrgAdmin\|PlatformAdmin` · `{ name, description, category, logoUrl }` (creator becomes scoped `Admin`) |
| **Organizations** | `GET` | `/api/v1/organizations/{id}` | Auth → `OrganizationDetailDto` (`members`, `events`) |
| **Organizations** | `PATCH` | `/api/v1/organizations/{id}` | Scoped OrgAdmin / `PlatformAdmin` |
| **Organizations** | `DELETE` | `/api/v1/organizations/{id}` | `PlatformAdmin` → `204` |
| **Org Members** | `GET` | `/api/v1/organizations/{id}/members` | Auth |
| **Org Members** | `PATCH` | `/api/v1/organizations/{id}/members/{studentId}` | Scoped OrgAdmin / `PlatformAdmin` · `{ role }` |
| **Org Members** | `DELETE` | `/api/v1/organizations/{id}/members/{studentId}` | Scoped OrgAdmin / `PlatformAdmin` / Self → `204` |
| **Org Apps** | `GET` | `/api/v1/organizations/{id}/applications` | Scoped OrgAdmin / Applicant |
| **Org Apps** | `POST` | `/api/v1/organizations/{id}/applications` | Auth · `{ message }` → `201` |
| **Org Apps** | `PATCH` | `/api/v1/organizations/{id}/applications/{appId}` | Scoped OrgAdmin · `{ status: "Accepted" \| "Rejected" }` |
| **Org Apps** | `DELETE` | `/api/v1/organizations/{id}/applications/{appId}` | Applicant withdraw → `204` |
| **Events** | `GET` | `/api/v1/events` | Auth · `?search=&organizationId=&type=&from=&to=&page=&pageSize=` |
| **Events** | `POST` | `/api/v1/events` | Scoped OrgAdmin of `organizationId` / `PlatformAdmin` → `201` |
| **Events** | `GET` | `/api/v1/events/{id}` | Auth → `EventDto` (includes `registeredCount`, `sessions`) |
| **Events** | `PATCH` | `/api/v1/events/{id}` | Scoped OrgAdmin / `PlatformAdmin` |
| **Events** | `DELETE` | `/api/v1/events/{id}` | Scoped OrgAdmin / `PlatformAdmin` → `204` (soft-deletes child sessions) |
| **Event Sessions** | `GET` | `/api/v1/events/{id}/sessions` | Auth |
| **Event Sessions** | `POST` | `/api/v1/events/{id}/sessions` | Scoped OrgAdmin · `{ sessionNo, title, startTime, endTime, room }` → `201` |
| **Event Sessions** | `PUT` | `/api/v1/sessions/{id}` | Scoped OrgAdmin → `200` |
| **Event Sessions** | `DELETE` | `/api/v1/sessions/{id}` | Scoped OrgAdmin → `204` |
| **Registrations** | `GET` | `/api/v1/events/{id}/registrations` | Auth (Scoped OrgAdmin / Self) |
| **Registrations** | `POST` | `/api/v1/events/{id}/registrations` | Auth → `201 EventRegistrationDto` (`Confirmed` or `Waitlisted`; handle `409`) |
| **Registrations** | `DELETE` | `/api/v1/events/{id}/registrations` | Auth · optional `?studentId=` (auto-promotes next waitlisted student!) → `204` |
| **Attendance** | `GET` | `/api/v1/sessions/{id}/attendance` | Scoped OrgAdmin / `PlatformAdmin` |
| **Attendance** | `POST` | `/api/v1/sessions/{id}/attendance` | Scoped OrgAdmin · `{ studentId, verificationMethod, verificationCode }` → `201` |
| **Attendance** | `POST` | `/api/v1/sessions/{id}/attendance/check-in-code` | Scoped OrgAdmin → `200 SessionCheckInCodeDto { sessionId, code, expiresAtUtc }` |
| **Attendance** | `POST` | `/api/v1/sessions/{id}/attendance/self-check-in` | Auth (Confirmed Student) · `{ verificationCode }` → `201` (awards XP!) |
| **Attendance** | `DELETE` | `/api/v1/sessions/{id}/attendance/{studentId}` | Scoped OrgAdmin → `204` |
| **Attendance** | `GET` | `/api/v1/sessions/{id}/attendance/report` | Scoped OrgAdmin → `200 text/html` (sandboxed HTML attendance report) |

---

## 4. Phased Task Checklist

### Phase 1 — Typed APIs, Organizations & Events Discovery (Days 3–6)
- [ ] **M3-01:** Create `organizations.api.ts`, `events.api.ts`, `sessions.api.ts`, and `attendance.api.ts` (including `getAttendanceReportHtml(sessionId)` returning `string` with `Accept: text/html`).
- [ ] **M3-02:** Build `/organizations` Directory with URL-synced `?search=&category=&page=`, category filter pills, safe `logoUrl` image fallback to monogram badge, and Create Organization modal (`https?://` URL validation matching `SafeUrlValidator`).
- [ ] **M3-03:** Build `/organizations/:id` Overview & Hosted Events tab, showing scoped OrgAdmin edit controls **only** when `isPlatformAdmin || isOrgAdminOf(orgId)` (verified against `members` + JWT `org_admin` claims).
- [ ] **M3-04:** Build `/events` Discovery Hub with URL-synced `?search=&organizationId=&type=&from=&to=&page=`, EventType filter chips (`Workshop`, `Hackathon`, `Session`, `Competition`), date-range picker, and seat capacity indicators.
- [ ] **M3-05:** Build `/events/:id` Event Detail page with visual seat capacity bar (`registeredCount / capacity`), multi-session schedule timeline cards, and organization header link.

### Phase 2 — Recruitment, Waitlist Engine & Live 60s QR Attendance (Days 7–11)
- [ ] **M3-06:** Build Organization Members & Membership Applications tabs:
  - Scoped OrgAdmin can update member roles (`Admin`, `OrgAdmin`, `President`, `Vice President`, `Executive`, `Member`), remove members, and accept/reject applications.
  - Regular students can submit an application (`POST`) and withdraw pending applications (`DELETE`).
- [ ] **M3-07:** Build Event CRUD & Multi-Session Timetable Manager (OrgAdmin): Create/Edit/Delete Event (validate `endDate >= startDate`, `capacity >= 1`) and Add/Edit/Delete Session (`endTime > startTime`, `sessionNo`, `room`), warning that deleting an event soft-deletes all child sessions.
- [ ] **M3-08:** Build Event Registration & Waitlist UX:
  - One-click **Register** (`POST .../registrations`) immediately reflecting `Confirmed` (emerald pill) or `Waitlisted` (amber pill).
  - **Cancel Registration** (`DELETE .../registrations`) with confirmation noting waitlist auto-promotion.
  - Graceful `409 Conflict` handler (concurrency race / duplicate registration) triggering automatic query refetch.
- [ ] **M3-09:** Build Live Attendance Console (`/events/:id/sessions/:sessionId/attendance`):
  - **Rotating 60-Second QR & PIN Projector:** Calls `POST .../check-in-code`, renders `<QRCodeSVG>` encoding `${window.location.origin}/check-in?sessionId=${sessionId}&code=${code}` on a crisp white card, displays large monospace PIN + live countdown progress bar, and auto-regenerates when `expiresAtUtc` reaches `0s` while projector mode is active.
  - **Manual / RFID Organizer Check-In Form:** Selects from confirmed registrants, picks `VerificationMethod` (`QRCode`, `RFID`, `ManualByOrganizer`), submits `POST .../attendance`.
  - **Attendee Audit Table:** Lists `firstName`, `lastName`, `checkInTime`, `verificationMethod`, `recordedByStudentId` (STRIDE `THR-R-02` origin), and Revoke button.
  - **HTML Attendance Report Modal:** Fetches `GET .../report`, renders inside `<iframe srcDoc={html} sandbox="allow-same-origin" />`, and offers a **Print / Export PDF** trigger.
- [ ] **M3-10:** Build Student Self Check-In view (`/check-in`): parses optional `?sessionId=&code=` URL params, accepts manual code input, submits `POST /sessions/{id}/attendance/self-check-in`, invalidates gamification cache (`+25/+100 XP` toast), and handles expired code (`400`) or duplicate check-in (`409`) clearly.

### Phase 3 — Integration & Testing (Days 12–14)
- [ ] **M3-11:** Write Vitest + Testing Library tests for scoped OrgAdmin visibility (Club A admin cannot see edit actions on Club B), Waitlist registration + `409` conflict handling, QR countdown auto-refresh timer (using `vi.useFakeTimers()`), and Self Check-In XP invalidation.

---

## 5. Exports Required by Other Teammates

- `orgEventRoutes` route fragment → imported in `frontend/src/app/router.tsx`.
- Calls shared `invalidateGamification(queryClient, studentId)` after successful self check-in so M4's XP chip & notifications update immediately.
