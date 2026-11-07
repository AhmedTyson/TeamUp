# TeamUp — Project Wiki

> **Student Collaboration & Gamification Platform** · Clean Architecture .NET 10 Web API + EF Core 10 + Microsoft SQL Server 2022, 23 relational tables, 99 REST endpoints, 100% automated test coverage.
>
> This folder is the project's documentation home. All architectural blueprints, database models, operational guides, and endpoint references are indexed here.

---

## Quick Facts

| Dimension | Specification |
|---|---|
| **Backend Framework** | .NET 10 / ASP.NET Core Web API / C# 13 (`Nullable=enable`, `TreatWarningsAsErrors=true`) |
| **Architecture** | Clean Architecture / DDD Bounded Contexts / CQRS / N-Tier Modular Monolith |
| **Database & ORM** | Microsoft SQL Server 2022 · Entity Framework Core 10 · 23 Tables · 29 Foreign Keys |
| **Integrity Rules** | **Zero Cascades** (`ON DELETE NO ACTION`) · Single-Column Soft Delete (`DeletedAt`) · Filtered Unique Indexes |
| **Security & Auth** | JWT Bearer Auth · PBKDF2 Password Hashing · SHA-256 Crypto Refresh Tokens · Partitioned Rate Limiter |
| **API Surface** | 99 RESTful Endpoints (`/api/v1/*` + `/health`) · Uniform `BaseResponse<T>` Envelope |
| **Test Suite** | **100 / 100 Passing Tests** (57 Unit Tests + 43 Integration Tests via `WebApplicationFactory`) |
| **Engineering Team** | 4 Functional Contexts: M1 (Auth/Identity/Admin), M2 (Projects/Tasks), M3 (Orgs/Events), M4 (Gamification/Alerts) |

---

## Start Here

- [System Overview](./System%20Overview.md) — Platform vision, core capability map, and system context diagram.
- [Getting Started Guide](./Getting%20Started%20Guide.md) — Local prerequisites, SQL Server setup, migrations, database seeding, and first run.
- [Development Guidelines](./Development%20Guidelines.md) — Coding conventions, Clean Architecture rules, branch workflows, and definition of done.

---

## Architecture & Design

- [Architecture Overview](./Architecture.md) — N-Tier Clean Architecture, sequence diagrams, Result pattern, and Domain Events pub-sub.
- [Technology Stack & Architecture](./Technology%20Stack%20%26%20Architecture.md) — Framework choices, library dependencies, concurrency topology, and version notes.
- [Backend Services](./Backend%20Services.md) — Comprehensive catalogue of application services, domain logic, and cross-module interactions.
- [Database Design & Relational Mapping](./Database%20Design%20%26%20Relational%20Mapping.md) — Chen ERD (13 entities), Relational Mapping (23 tables across 5 tiers), 3NF audit, and D1–D10 schema design corrections.

---

## Operations & Reference

- [Infrastructure](./Infrastructure.md) — Docker Compose (SQL Server), GitHub Actions CI pipeline, rate limiting, and health monitoring.
- [Deployment Guide](./DEPLOYMENT.md) — Production containerization, environment configuration, database migration runbook, and backup/DR policies.
- [Environment Configuration](./ENVIRONMENT.md) — Exhaustive reference of configuration keys, connection strings, JWT parameters, and seed credentials.
- [Release Sign-off](./RELEASE_SIGN_OFF.md) — 20-Phase release verification checklist, zero-cascade DB audit sign-off, and live test execution logs.
- [API Reference](./API%20Reference.md) — Architectural standards, problem details, authentication schemes, and narrative module guide.
- [API Endpoints Reference](./API%20Endpoints%20Reference.md) — Curated master catalog of all 99 endpoints with verbs, auth requirements, payloads, and HTTP status codes.
- [Routes Appendix](./ROUTES-APPENDIX.md) — Full 99-endpoint audit tracking matrix mapped to controllers, actions, and security tiers.

---

## Audits & Advanced Research

- [Clean Code & Architecture Audit](./Clean%20Code%20%26%20Architecture%20Audit.md) — Grade A audit summary (96% compliance), SOLID enforcement matrix, query optimization, and security hardening.
- [Adaptable AI Engine Research](./Adaptable%20AI%20Engine%20Research.md) — Autonomous Adaptable AI architecture research (6 pillars, 30 phases: intent routing, multimodal avatar intake, flyer OCR, and DAG task decomposition).

---

## Knowledge Map

```text
TeamUp Documentation Home (wiki/Home.md)
│
├── 🚀 Start Here
│   ├── System Overview.md ──────────── Capability Map & Architecture Context
│   ├── Getting Started Guide.md ────── Local Setup, Seeding & First Run
│   └── Development Guidelines.md ───── Coding Rules, Git Workflows & DoD
│
├── 🏛️ Architecture & Database
│   ├── Architecture.md ─────────────── Clean Architecture, CQRS & Domain Events
│   ├── Technology Stack.md ─────────── .NET 10, EF Core 10, SQL Server & Libraries
│   ├── Backend Services.md ─────────── 12 Application Services & Business Logic
│   └── Database Design.md ──────────── Chen ERD, Relational Mapping & Zero Cascades
│
├── ⚙️ Operations & Deployment
│   ├── Infrastructure.md ───────────── Docker Compose, CI Quality Gates & Rate Limiter
│   ├── DEPLOYMENT.md ───────────────── Production Runbook & Disaster Recovery
│   ├── ENVIRONMENT.md ──────────────── Configuration Keys & Connection Strings
│   └── RELEASE_SIGN_OFF.md ─────────── 20-Phase Verification & Live Test Sign-off
│
├── 📡 API Reference
│   ├── API Reference.md ────────────── Architectural Envelope & Auth Matrix
│   ├── API Endpoints Reference.md ──── Curated 99-Endpoint Catalog with Payloads
│   └── ROUTES-APPENDIX.md ──────────── Raw Route-to-Controller Audit Matrix
│
└── 🔬 Audits & AI Research
    ├── Clean Code Audit.md ─────────── Grade A Code Quality & SOLID Enforcement
    └── Adaptable AI Engine.md ──────── 6-Pillar Autonomous Entity Synthesis
```

---

## Source Artifacts & Asset Mapping

| Documentation Guide | Corresponding Repository Source |
|---|---|
| Database ERD | [`ERD/description.md`](../ERD/description.md) · [`ERD/TeamUp-Chen-ERD.png`](../ERD/TeamUp-Chen-ERD.png) |
| Relational Schema | [`Mapping/description.md`](../Mapping/description.md) · [`Mapping/TeamUp-Relational-Mapping.png`](../Mapping/TeamUp-Relational-Mapping.png) |
| Architecture & SOLID | [`docs/architecture/N-TIER-SOLID-ARCHITECTURE-PLAN.md`](../docs/architecture/N-TIER-SOLID-ARCHITECTURE-PLAN.md) |
| Clean Code Audit | [`docs/architecture/CLEAN-CODE-AUDIT.md`](../docs/architecture/CLEAN-CODE-AUDIT.md) |
| API Specifications | [`docs/api/API-PROPOSAL.md`](../docs/api/API-PROPOSAL.md) · [`docs/api/openapi.json`](../docs/api/openapi.json) |
| Postman Collection | [`docs/api/postman/TeamUp_Collection.json`](../docs/api/postman/TeamUp_Collection.json) |
| Live Verification Logs | [`docs/api/MASTER-LIVE-TEST-REPORT.md`](../docs/api/MASTER-LIVE-TEST-REPORT.md) |
| 20-Phase Task Checklists | [`tasks/plan.md`](../tasks/plan.md) · [`tasks/todo.md`](../tasks/todo.md) |
| AI Research Index | [`docs/research/adaptable-ai-engine/SUMMARY.md`](../docs/research/adaptable-ai-engine/SUMMARY.md) |

---

## Repository Coordinates

- **Repository:** <https://github.com/AhmedTyson/TeamUp>
- **Primary Branch:** `main`
- **Solution:** `TeamUp.sln` (.NET 10)
