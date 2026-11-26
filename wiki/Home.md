# TeamUp — Project Wiki

> **Student Collaboration & Gamification Platform** · Clean Architecture .NET 11 Web API + React 19 SPA + EF Core 11 + Microsoft SQL Server 2022 · 100% Zero Cascades, OpenAPI 3.2 (100 paths / 143 operations), 348/348 automated tests passing.
>
> This folder is the project's documentation home. All architectural blueprints, database models, operational guides, and endpoint references are indexed here.

---

## Quick Facts

| Dimension | Specification |
|---|---|
| **Backend Framework** | .NET 11 / ASP.NET Core Web API / C# 15 (`Nullable=enable`, CPM via `Directory.Packages.props`) |
| **Frontend Web App** | React 19 + Vite 8 + TypeScript + Tailwind CSS v4 SPA (`frontend/`) with React Router v7 & TanStack Query v5 |
| **Architecture** | Clean Architecture / DDD Bounded Contexts / Modular Monolith (`Domain`, `Application`, `Infrastructure`, `Api`) |
| **Database & ORM** | Microsoft SQL Server 2022 (with Development SQLite fallback) · EF Core 11 · Zero Cascades · `StoredImage` storage |
| **Integrity Rules** | **Zero Cascades** (`ON DELETE NO ACTION` / `Restrict`) · `ISoftDelete` (`DeletedAt`) · 2 `IAppendOnly` Ledgers (`ScoreTransaction`, `AuditLog`) · Filtered Unique Indexes · CHECK Constraints |
| **Security & Auth** | JWT Bearer Auth (`SecurityStamp` validated) · PBKDF2 Password Hashing · SHA-256 Crypto Refresh Tokens (10s reuse grace) · Tiered Rate Limiting (`120/30/30/30/10` per 60s incl. `upload-policy`) · STRIDE Hardened |
| **API Surface** | OpenAPI 3.2 (100 paths / 143 operations + `/health/live` & `/health/ready`) · Uniform `ApiResponse<T>` / `BaseResponse<T>` Envelope |
| **Test Suite** | **348 / 348 Passing Tests** (144 Unit + 109 Integration via `WebApplicationFactory` + 95 Frontend Vitest) + Playwright E2E Smoke Suite |
| **Engineering Team** | 4 Functional Contexts: M1 (Auth/Identity/Admin), M2 (Skills/Projects/Tasks), M3 (Orgs/Events/Attendance), M4 (Gamification/Badges/Alerts) |

---

## Start Here

- [System Overview](./System%20Overview.md) — Platform vision, core capability map, and system context diagram.
- [Getting Started Guide](./Getting%20Started%20Guide.md) — Local prerequisites, SQL Server & SQLite setup, migrations, database seeding, and first run.
- [Development Guidelines](./Development%20Guidelines.md) — Coding conventions, Clean Architecture rules, branch workflows, and definition of done.

---

## Architecture & Design

- [Architecture Overview](./Architecture.md) — N-Tier Clean Architecture, sequence diagrams, exception-driven error handling, and Domain Events pub-sub.
- [Technology Stack & Architecture](./Technology%20Stack%20%26%20Architecture.md) — Backend & frontend stack choices, library dependencies, caching topology, and concurrency notes.
- [Backend Services](./Backend%20Services.md) — Comprehensive catalogue of application, security, infrastructure, and PDF/email services plus domain event handlers.
- [Database Design & Relational Mapping](./Database%20Design%20%26%20Relational%20Mapping.md) — Chen ERD (13 entities), Core Relational Mapping (23 tables across 5 tiers + `AuditLog` = 24 runtime tables), 3NF audit, and D1–D10 schema corrections.

---

## Operations & Reference

- [Infrastructure](./Infrastructure.md) — Docker SQL Server containerization, GitHub Actions CI/CD pipelines, tiered rate limiting, security headers, and health monitoring.
- [Deployment Guide](./DEPLOYMENT.md) — Production deployment options (MonsterASP.NET + Static SPA CDN and Self-Hosted Nginx/Kestrel), migrations, and backup/DR policies.
- [Environment Configuration](./ENVIRONMENT.md) — Exhaustive reference of backend and frontend configuration keys, connection strings, JWT/SMTP parameters, and rate limits.
- [Release Sign-off](./RELEASE_SIGN_OFF.md) — Historical `v1.0.0-GA` 20-Phase sign-off plus current post-GA verification snapshot (263/263 tests, STRIDE, React 19 SPA).
- [API Reference](./API%20Reference.md) — Architectural standards, response envelopes, authentication schemes, and narrative module guide across all 116 endpoints.
- [API Endpoints Reference](./API%20Endpoints%20Reference.md) — Curated endpoint catalog with verified verbs, auth requirements, DTO payloads, and HTTP status codes.
- [Routes Appendix](./ROUTES-APPENDIX.md) — Complete 116-route audit tracking matrix mapped to all 19 controllers, actions, and security tiers.

---

## Audits & Advanced Research

- [Clean Code & Architecture Audit](./Clean%20Code%20%26%20Architecture%20Audit.md) — Grade A audit summary, SOLID enforcement matrix, STRIDE security remediation, and query optimization.
- [Adaptable AI Engine Research](./Adaptable%20AI%20Engine%20Research.md) — Future-roadmap research blueprint (6 pillars, 30 phases: intent routing, multimodal avatar intake, flyer OCR, and DAG task decomposition).

---

## Knowledge Map

```text
TeamUp Documentation Home (wiki/Home.md)
│
├── 🚀 Start Here
│   ├── System Overview.md ──────────────────────── Capability Map & Architecture Context
│   ├── Getting Started Guide.md ────────────────── Local Setup, Seeding & First Run
│   └── Development Guidelines.md ───────────────── Coding Rules, Git Workflows & DoD
│
├── 🏛️ Architecture & Database
│   ├── Architecture.md ─────────────────────────── Clean Architecture, Exception Flow & Domain Events
│   ├── Technology Stack & Architecture.md ──────── .NET 11, React 19, EF Core 11 & Caching
│   ├── Backend Services.md ─────────────────────── Application, Security & Infra Services + 6 Domain Events
│   └── Database Design & Relational Mapping.md ─── Chen ERD, 23 Core / 24 Runtime Tables & Zero Cascades
│
├── ⚙️ Operations & Deployment
│   ├── Infrastructure.md ───────────────────────── Docker SQL, CI/CD Pipelines & Rate Limiter
│   ├── DEPLOYMENT.md ───────────────────────────── MonsterASP.NET, Static SPA & Self-Hosted Runbook
│   ├── ENVIRONMENT.md ──────────────────────────── Configuration Keys, Connection Strings & Frontend Env
│   └── RELEASE_SIGN_OFF.md ─────────────────────── 20-Phase GA Sign-Off & Current Verification Snapshot
│
├── 📡 API Reference
│   ├── API Reference.md ────────────────────────── Envelope Standards, Auth Matrix & Module Overview
│   ├── API Endpoints Reference.md ──────────────── Curated Endpoint Catalog with Verified DTO Payloads
│   └── ROUTES-APPENDIX.md ──────────────────────── Complete 116-Route Controller & Auth Matrix
│
└── 🔬 Audits & AI Research
    ├── Clean Code & Architecture Audit.md ──────── Grade A Code Quality, SOLID & STRIDE Remediation
    └── Adaptable AI Engine Research.md ─────────── Future 6-Pillar Autonomous Entity Synthesis Blueprint
```

---

## Source Artifacts & Asset Mapping

| Documentation Guide | Corresponding Repository Source |
|---|---|
| Frontend Web App (React 19 SPA) | [`frontend/src/App.tsx`](../frontend/src/App.tsx) · [`frontend/package.json`](../frontend/package.json) |
| Contribution & Team Rules | [`CONTRIBUTING.md`](../CONTRIBUTING.md) |
| Brand Assets & Logo | [`assets/teamup-logo.svg`](../assets/teamup-logo.svg) · [`assets/logo.svg`](../assets/logo.svg) |
| Database ERD | [`ERD/description.md`](../ERD/description.md) · [`ERD/TeamUp-Chen-ERD.png`](../ERD/TeamUp-Chen-ERD.png) |
| Relational Schema | [`Mapping/description.md`](../Mapping/description.md) · [`Mapping/TeamUp-Relational-Mapping.png`](../Mapping/TeamUp-Relational-Mapping.png) |
| Idempotent SQL Schema | [`docs/database/teamup-sqlserver-idempotent.sql`](../docs/database/teamup-sqlserver-idempotent.sql) |
| Architecture & SOLID | [`docs/architecture/N-TIER-SOLID-ARCHITECTURE-PLAN.md`](../docs/architecture/N-TIER-SOLID-ARCHITECTURE-PLAN.md) |
| Clean Code Audit | [`docs/architecture/CLEAN-CODE-AUDIT.md`](../docs/architecture/CLEAN-CODE-AUDIT.md) |
| STRIDE Threat Model | [`docs/security/STRIDE-THREAT-MODEL.md`](../docs/security/STRIDE-THREAT-MODEL.md) |
| Free Demo Deployment Guide | [`docs/DEPLOYMENT_FREE.md`](../docs/DEPLOYMENT_FREE.md) |
| API Specifications | [`docs/api/API-PROPOSAL.md`](../docs/api/API-PROPOSAL.md) · [`docs/api/openapi.json`](../docs/api/openapi.json) |
| Postman Collection | [`docs/api/postman/TeamUp_Collection.json`](../docs/api/postman/TeamUp_Collection.json) · [`docs/api/postman/TeamUp_Environment.json`](../docs/api/postman/TeamUp_Environment.json) |
| Live Verification Logs | [`docs/api/MASTER-LIVE-TEST-REPORT.md`](../docs/api/MASTER-LIVE-TEST-REPORT.md) |
| 20-Phase Task Checklists | [`tasks/plan.md`](../tasks/plan.md) · [`tasks/todo.md`](../tasks/todo.md) |
| AI Research Index | [`docs/research/adaptable-ai-engine/SUMMARY.md`](../docs/research/adaptable-ai-engine/SUMMARY.md) |

---

## Repository Coordinates

- **Repository:** <https://github.com/AhmedTyson/TeamUp>
- **Primary Branch:** `main`
- **Solution:** [`backend/TeamUp.sln`](../backend/TeamUp.sln) (.NET 11)
- **Frontend Workspace:** [`frontend/package.json`](../frontend/package.json) (React 19 + Vite 8)
