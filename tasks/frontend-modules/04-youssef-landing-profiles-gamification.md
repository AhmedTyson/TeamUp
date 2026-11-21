# Frontend Module 04: Flagship Landing Page, Student Profiles, Gamification & Notifications

> **Owner:** M4 — Youssef  
> **API Operations Owned:** 26 (`Students` 6 · `StudentProfiles` 2 · `StudentInterests` 3 · `Gamification` 4 · `Badges` 6 · `Notifications` 4 · `Health` 1)  
> **Master Spec:** [`tasks/frontend-plan.md`](../frontend-plan.md)  
> **Landing Reference:** [`docs/prototypes/landing-static.html`](../../docs/prototypes/landing-static.html)  
> **Owned Folders:** `frontend/src/api/{students.api,gamification.api,badges.api,notifications.api}.ts`, `frontend/src/features/landing/`, `frontend/src/features/profile/`, `frontend/src/features/gamification/`

---

## 1. Scope & Responsibilities

M4 owns the **product's first impression** (porting the full Handshake-inspired `landing-static.html` prototype into a production React experience), the **Student Community & Portfolio layer** (privacy-hardened directory, public portfolios, self-profile & interest tags), and the **Gamification & Real-Time Engagement Hub** (Campus Leaderboard, append-only XP Score Ledger, Badges Engine, and Global Notifications Inbox).

---

## 2. Owned Routes

| Route | Guard | View Description |
|---|---|---|
| `/` | Public | **Flagship Handshake-Inspired Landing Page** faithfully ported from `docs/prototypes/landing-static.html` (Morphing Liquid-Pill Header, Mesh Hero + Live Typewriter Search Bar, 3×2 Spotlight Grid, Infinite Campus Guilds Marquee, 4 Alternating Gradient Showcase Rows, Footer + One-Shot Burst `TeamUp` Wordmark) |
| `/students` | Auth | **Student Directory** (`directory-policy` rate-limited, strict max page size `20`, filters for `Search`, `FacultyId`, `AcademicYear`; respects `showEmailInDirectory` privacy mask) |
| `/students/:id` | Auth | **Public Student Portfolio** (Avatar, University/Faculty/Academic Year, Bio, safe GitHub/LinkedIn links, Interest Chips, M2's `<StudentSkillsPanel />`, Score/Level summary card & Earned Badges shelf) |
| `/profile/me` | Auth (Self) | **My Account & Portfolio Studio** (Edit First/Last Name, Academic Year, **`ShowEmailInDirectory` Privacy Toggle**; Bio/Photo/GitHub/LinkedIn upsert; Interest Tag chips add/remove; M2's editable `<StudentSkillsPanel />`; Danger Zone soft-delete account) |
| `/leaderboard` | Auth | **Campus Leaderboard** (Top 3 electric-lime/emerald podium cards + paginated ranking table with rank, student portfolio link, university/faculty, level badge, total XP) |
| `/gamification` | Auth | **My XP, Ledger & Badges Hub** (Level progress bar displaying `FLOOR(SQRT(pts/100))+1`, `currentLevelStart → nextLevelThreshold`, `pointsToNext`; Read-Only Append-Only **Score Transactions Ledger** table; **Badges Showcase** showing Unlocked vs Locked criteria; `PlatformAdmin` tabs for Badge Catalog CRUD, Manual Badge Award, and Manual Score Adjustment) |
| `/notifications` | Auth | **Notifications Inbox** (All vs Unread filter toggle, typed icon badges for `ApplicationStatus`, `TaskAssigned`, `BadgeEarned`, `EventReminder`, Mark Read, Mark All Read, Delete) |

---

## 3. Owned API Endpoints (26 Operations)

| Group | Method | Endpoint | Auth / Notes |
|---|---|---|---|
| **Health** | `GET` | `/health` | Public · powers `useApiHealth()` live badge |
| **Students** | `GET` | `/api/v1/students` | Auth (`directory-policy`) · `?Search=&FacultyId=&AcademicYear=&Page=&PageSize=` (**cap `PageSize <= 20`!**) |
| **Students** | `GET` | `/api/v1/students/{id}` | Auth → `StudentPublicProfileDto` |
| **Students** | `GET` | `/api/v1/students/me` | Auth (Self) → `StudentDetailDto` (includes `showEmailInDirectory`) |
| **Students** | `PATCH` | `/api/v1/students/me` | Auth (Self) · `{ firstName, lastName, academicYear, showEmailInDirectory }` |
| **Students** | `DELETE` | `/api/v1/students/me` | Auth (Self) → `204` (soft-deletes account, triggers logout!) |
| **Students** | `DELETE` | `/api/v1/students/{id}` | `PlatformAdmin` → `204` |
| **Profiles** | `GET` | `/api/v1/students/{id}/profile` | Auth → `StudentProfileDto` |
| **Profiles** | `PUT` | `/api/v1/students/me/profile` | Auth (Self) · `{ bio, photoUrl, gitHubUrl, linkedInUrl }` (safe `https?://` validation!) |
| **Interests** | `GET` | `/api/v1/students/{id}/interests` | Auth → `string[]` |
| **Interests** | `POST` | `/api/v1/students/me/interests` | Auth (Self) · `{ interest }` |
| **Interests** | `DELETE` | `/api/v1/students/me/interests/{interest}` | Auth (Self) → `204` (URI-encode tag) |
| **Gamification** | `GET` | `/api/v1/gamification/leaderboard` | Auth · `?page=&pageSize=` → `PagedResult<LeaderboardEntryDto>` |
| **Gamification** | `GET` | `/api/v1/students/{id}/score` | Auth → `StudentScoreDto` (`totalPoints, level, currentLevelStart, nextLevelThreshold, pointsToNext, progress`) |
| **Gamification** | `GET` | `/api/v1/students/{id}/score-transactions` | **Self or `PlatformAdmin` only** (`403` for other students — hide ledger tab on others' profiles!) |
| **Gamification** | `POST` | `/api/v1/score-transactions` | `PlatformAdmin` · `{ studentId, points, reason, sourceType }` → `201` |
| **Badges** | `GET` | `/api/v1/badges` | Auth → `BadgeDto[]` |
| **Badges** | `POST` | `/api/v1/badges` | `PlatformAdmin` → `201` |
| **Badges** | `PUT` | `/api/v1/badges/{id}` | `PlatformAdmin` → `200` |
| **Badges** | `DELETE` | `/api/v1/badges/{id}` | `PlatformAdmin` → `204` |
| **Badges** | `GET` | `/api/v1/students/{id}/badges` | Auth → `StudentBadgeDto[]` |
| **Badges** | `POST` | `/api/v1/students/{id}/badges/{badgeId}` | `PlatformAdmin` → `201` (manual badge award) |
| **Notifications** | `GET` | `/api/v1/notifications` | Auth · `?unreadOnly=true\|false` |
| **Notifications** | `PATCH` | `/api/v1/notifications/{id}` | Auth (Self) → `204` (mark read) |
| **Notifications** | `PATCH` | `/api/v1/notifications/mark-all-read` | Auth (Self) → `204` |
| **Notifications** | `DELETE` | `/api/v1/notifications/{id}` | Auth (Self) → `204` |

---

## 4. Phased Task Checklist

### Phase 1 — Flagship Landing Page Port & Discovery Surfaces (Days 3–6)
- [ ] **M4-01:** Create `students.api.ts`, `gamification.api.ts`, `badges.api.ts`, and `notifications.api.ts` + shared `invalidateGamification(queryClient, studentId)` helper.
- [ ] **M4-02:** Port [`docs/prototypes/landing-static.html`](../../docs/prototypes/landing-static.html) into clean modular React components under `frontend/src/features/landing/` (rendered outside the narrow `AppShell` container so the hero & marquee stay true full-bleed):
  - `LandingHeader.tsx`: Morphing header (`scrollY > 90px` → floating white pill + `T` lime badge), liquid gliding pill indicator (`ResizeObserver`/ref-measured), 5 module dropdowns routing to live SPA paths, plus right-zone **Sign In / Join Free** (or user menu when logged in).
  - `HeroSection.tsx`: Multi-layer mesh gradient (`.teamup-hero-bg`), `Antonio` giant headline, white rounded-pill search bar with typewriter animation that transitions cleanly into manual input and submits to `/projects?keyword=...` / `/organizations?search=...` / `/events?search=...` / `/students?search=...` depending on active filter chip.
  - `ServicesGridSection.tsx` & `CampusMarquee.tsx`: 3×2 `<SpotlightCard>` grid + seamless RTL infinite marquee of campus guilds with pause-on-hover and `prefers-reduced-motion` static fallback.
  - `FeatureShowcaseSection.tsx`: All 4 alternating gradient showcase rows (`.showcase-gradient-1..4`) with crisp elevated white cards and real `<Link>` CTAs.
  - `LandingFooter.tsx`: Tagline, 3-column clickable sitemap links, and one-shot letter-burst giant `TeamUp` wordmark using `useInViewOnce`.
- [ ] **M4-03:** Build `/students` Student Directory with Faculty dropdown (from `universitiesApi.getCatalog()`), Academic Year filter (`1–6`), debounced keyword search, page-size cap strictly `<= 20` (`THR-I-01`), graceful `"Email private"` display when email is omitted, and `429` rate-limit feedback.
- [ ] **M4-04:** Build `/leaderboard` Campus Leaderboard with Top-3 podium highlight cards (lime/emerald accents) and paginated leaderboard table.

### Phase 2 — Portfolios, Gamification Studio & Notifications Inbox (Days 7–11)
- [ ] **M4-05:** Build `/students/:id` Public Student Portfolio composing Student Info, Bio, safe external GitHub/LinkedIn links (`rel="noopener noreferrer"`), Interests chips, M2's `<StudentSkillsPanel studentId={id} />`, `<StudentGamificationSummary studentId={id} />`, and Earned Badges grid (plus `PlatformAdmin` soft-delete button with confirmation).
- [ ] **M4-06:** Build `/profile/me` Self-Profile Studio:
  - Account details form (`PATCH /students/me`) with prominent **`Show my email in student directory`** opt-in toggle (`false` by default).
  - Portfolio form (`PUT /students/me/profile`) with live avatar preview and Zod `https?://` validation for `photoUrl`, `gitHubUrl`, `linkedInUrl`.
  - Interests manager (`POST /students/me/interests`, `DELETE /students/me/interests/{interest}`).
  - Danger Zone: Soft-delete account (`DELETE /students/me`) requiring typed email confirmation, then calling `logout()`.
- [ ] **M4-07:** Build `/gamification` Hub:
  - Personal Level & XP Card showing `level`, `totalPoints`, progress bar (`progress * 100%`), `pointsToNext`, and `FLOOR(SQRT(pts/100))+1` tooltip.
  - Read-only Append-Only **Score Transactions Ledger** (`GET /students/{myId}/score-transactions`) with colored `ScoreSourceType` pills (`EventAttendance`, `TaskCompleted`, `ProjectDelivered`, `Manual`).
  - **Badges Gallery** merging `GET /badges` + `GET /students/{myId}/badges` to show Unlocked badges (awarded date) alongside Locked milestones (`criteriaType` + `criteriaValue`).
  - `PlatformAdmin` governance tabs: Create/Edit/Delete Badge (`iconUrl` validated), Manual Award Badge (`POST /students/{id}/badges/{badgeId}`), and Manual Score Adjustment (`POST /score-transactions`).
- [ ] **M4-08:** Build Global `<NotificationBell />` dropdown widget (unread count pill, 60s background polling via TanStack Query `refetchInterval`, quick mark-read + "View all") AND `/notifications` full inbox page (`unreadOnly` filter, Mark Read, Mark All Read, Delete).

### Phase 3 — Integration & Testing (Days 12–14)
- [ ] **M4-09:** Export `<NotificationBell />`, `<XpLevelChip />`, `<StudentGamificationSummary />`, and `invalidateGamification()` for M1/M2/M3 integration.
- [ ] **M4-10:** Write Vitest + Testing Library tests for Landing Hero search navigation, Student Directory 20-item page cap & email privacy display, Self-Profile `showEmailInDirectory` toggle, Gamification progress calculation, and Notification Mark-All-Read flow.

---

## 5. Exports Required by Other Teammates

- `<NotificationBell />` & `<XpLevelChip />` → mounted in **M1**'s `AppShell` top navbar.
- `invalidateGamification(queryClient, studentId)` → called by **M2** (on Task `Done`) and **M3** (on Session Check-In).
- `communityRoutes` route fragment (including `/`) → imported in `frontend/src/app/router.tsx`.
