# Frontend Feature Slices

Organize domain UI modules under `frontend/src/features/<domain>/` so each vertical slice stays modular and aligned with the backend bounded contexts:

- `auth/` — Login, registration (with `/api/v1/universities/catalog`), token refresh, password reset
- `profile/` — Student directory, public portfolio cards, bio/links, interests
- `skills/` — Master skill catalog & student skill claims
- `projects/` — Project discovery, required skills, team roster, applications, Kanban tasks
- `organizations/` — Student clubs directory, committee members, membership applications
- `events/` — Multi-session events, registrations, QR/PIN attendance verification
- `gamification/` — Score ledger, level progression, badges, notifications inbox

All HTTP calls should go through `frontend/src/api/` rather than calling `fetch`/`axios` directly inside components.
