# Frontend Feature Slices

Organize domain UI modules under `frontend/src/features/<domain>/` so each vertical slice stays modular and aligned with the backend bounded contexts:

- `admin/` — PlatformAdmin dashboard, academic catalog management, role governance, soft-delete restore, audit logs, and email communications/reports
- `auth/` — Login, registration (with `/api/v1/universities/catalog`), token refresh, password reset, `ProtectedRoute`, `GuestOnlyRoute`, and `RoleGuard`
- `catalog/` — Public student showcase hooks and university catalog fallback helpers
- `events/` — Multi-session events, registrations, and rotating QR/PIN attendance verification
- `gamification/` — Score ledger, level progression, campus leaderboard, and badges gallery
- `notifications/` — In-app notifications inbox and mark-read actions
- `organizations/` — Student clubs directory, committee members, and membership applications
- `profile/` — Student directory, public portfolio cards, bio/links, portrait photo upload, interests, and privacy/email preferences
- `projects/` — Project discovery, creation, required skills, team roster, applications, and Kanban tasks
- `skills/` — Master skill catalog & student skill claims

Route fragments are composed modularly under `frontend/src/app/routes/` (`authRoutes.tsx`, `communityRoutes.tsx`, `projectRoutes.tsx`, `orgEventRoutes.tsx`). All HTTP calls should go through `frontend/src/api/` rather than calling `fetch`/`axios` directly inside components.
