# Phase 10 Audit — Documentation, OpenAPI, SQL Scripts & Repo Hygiene

**Date:** 2026-11-26  
**Scope:** `README.md`, `CONTRIBUTING.md`, `wiki/*.md`, `docs/api/*`, `docs/database/teamup-sqlserver-idempotent.sql`, `docs/architecture/*`, `docs/DEPLOYMENT_FREE.md`.

---

## 1. Key Findings & Remediation Plan

| ID | Severity | Location | Finding | Remediation (Apply Phase 10) |
|---|---|---|---|---|
| DOC-01 | **P1** | `docs/database/teamup-sqlserver-idempotent.sql:1374-1415` | Stops at migration `20261124175844_AddFlexibleActivitiesAndHierarchy` (10 of 13); misses `OrganizationCreationRequest`, `StoredImage`, and `CoverImageUrl` columns! Manual SQL Server provisioning fails at runtime. | Regenerate via `dotnet tool run dotnet-ef migrations script -i -o docs/database/teamup-sqlserver-idempotent.sql` under EF Core 11 and assert 0 cascades. |
| DOC-02 | **P1** | `docs/api/openapi.json` (23,928 lines) | Stale at 89 paths and `.NET 10` header; missing 11 Round 4–5 & Org Request paths (`/api/v1/images`, `/organizations/public/categories`, `/projects/{id}/applications/mine`, `/events/{id}/registrations/mine`, `/notifications/unread-count`, `/organizations/requests*`, `/admin/organization-requests*`, plus `/health/live` & `/health/ready`). | Export fresh OpenAPI 3.2 JSON directly from running `.NET 11` API (`GET /openapi/v1.json`) to `docs/api/openapi.json`. |
| DOC-03 | **P1** | `docs/api/postman/TeamUp_Collection.json` | Missing Round 1–5 endpoints and cites `.NET 10`. | Update `TeamUp_Collection.json` (or sync via `OpenApiAndPostmanTests`) and update `.NET 11` metadata. |
| DOC-04 | **P2** | `README.md`, `wiki/ENVIRONMENT.md`, `docs/api/API-ENDPOINTS.md`, `wiki/ROUTES-APPENDIX.md`, `wiki/Home.md`, `docs/DEPLOYMENT_FREE.md` | Stale `.NET 10` / `C# 14` / `EF Core 10` labels, outdated route count (`116` vs `143+`), outdated table/FK/migration counts (`24 tables / 31 FKs / 9 migrations` vs `27 DbSets / 41 FKs / 13+ migrations`), and missing Round 4 config sections (`Gamification`, `Notifications`, `Admin:SoftDeletedPreviewLimit`, `Attendance:EarlyCheckInMinutes`/`LateCheckInMinutes`, `RateLimiting:UploadPermitLimit`). | Update `README.md`, `wiki/ENVIRONMENT.md`, `docs/api/API-ENDPOINTS.md`, `docs/DEPLOYMENT_FREE.md`, `wiki/ROUTES-APPENDIX.md`, and `wiki/Home.md` accurately. |
