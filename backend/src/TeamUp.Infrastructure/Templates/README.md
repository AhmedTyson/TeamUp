# Backend Templates Boundary

`TeamUp.Api` is a **pure JSON REST API** (`/api/v1/*`) designed for the decoupled React frontend (`frontend/`).

## Rules

- **No MVC Views or `.cshtml` pages** are permitted in `TeamUp.Api`.
- Server-side templates (Razor `.cshtml` or HTML templates) are **strictly limited** to non-UI backend outputs housed under `TeamUp.Infrastructure/Templates/`:
  - `Emails/` — transactional email templates (e.g., password reset, event confirmation, application notifications).
  - `Reports/` — structured printable or PDF report templates (e.g., attendance sheets, activity summaries).
