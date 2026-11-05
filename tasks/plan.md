# TeamUp Backend: 20-Phase Implementation & Architecture Plan

**Stack:** .NET 10, ASP.NET Core Web API, EF Core 10, Microsoft SQL Server, JWT Bearer Auth, FluentValidation.  
**Architecture:** Clean Architecture, Domain-Driven Design (DDD) Bounded Contexts, CQRS/Service Segregation.  
**Team Allocation:** Tyson (M1 - Auth/Identity/Admin), Ossama (M2 - Skills/Projects/Tasks), Ibrahim (M3 - Orgs/Events), Youssef (M4 - Gamification/Notifications).  
**Source Specifications:** `ERD/description.md`, `Mapping/description.md`, `docs/api/API-PROPOSAL.md`.

---

## 20-Phase Master Plan Overview

| Phase | Phase Name | Focus Area | Status |
|---|---|---|---|
| **Phase 01** | Solution Scaffold & Baseline Infrastructure | .NET 10 setup, CI, BaseEntity, SoftDeleteInterceptor, BaseResponse | ✅ Completed |
| **Phase 02** | Relational Schema & 22 Domain Entities | Cascade-free schema, Check constraints, Migrations, DbSeeder | ✅ Completed |
| **Phase 03** | Core Identity & JWT Authentication | Register, Login, Refresh tokens rotation, Claims, Passwords | ✅ Completed |
| **Phase 04** | Academic Catalog: Universities & Faculties | Public catalog lookups, Admin CRUD, Nested faculty endpoints | ✅ Completed |
| **Phase 05** | Student Discovery & Account Management | Search directory, Portfolio biography, Account soft-delete | ✅ Completed |
| **Phase 06** | Password Recovery & Platform Governance | Reset tokens, Admin dashboard, Role governance, Universal restore | ✅ Completed |
| **Phase 07** | Projects, Teams & Sprint Task Management | Project creation, Member roles, Applications, Task lifecycle (PR #1) | ✅ Completed |
| **Phase 08** | Security Remediation & Crypto Hardening | Redact reset token from API, SHA-256 token hashing, Partitioned rate limiting | ✅ Completed |
| **Phase 09** | Domain Model Factorization | Decompose monolithic `Entities.cs` into modular bounded subdirectories | ✅ Completed |
| **Phase 10** | Clean Architecture Layer Realignment | Introduce `IAppDbContext` in Application, migrate services from Infrastructure | ✅ Completed |
| **Phase 11** | Interface Segregation (ISP) for Projects | Split God `IProjectService` into 4 focused SRP services | ✅ Completed |
| **Phase 12** | Module 04: Skills Catalog & Student Claims | Master skills catalog, categories, proficiency ratings (`T16`) | ✅ Completed |
| **Phase 13** | Module 08 & 09: Organizations & Applications | Student activities, OrgAdmin management, Membership applications (`T20, T21`) | ⏳ Queued (Ibrahim) |
| **Phase 14** | Module 10: Events & Multi-Session Schedules | Event lifecycle, capacity, sessions, cascade soft-delete (`T22`) | ⏳ Ready |
| **Phase 15** | Module 11 & 12: Registrations & Attendance | Registration waitlist, QR/pin attendance check-in, duplicate prevention (`T23, T24`) | ⏳ Ready |
| **Phase 16** | Module 13: Gamification Scoring Ledger | Append-only `ScoreTransaction`, level formula, activity award hooks (`T25`) | ⏳ Ready |
| **Phase 17** | Module 14 & 15: Badges & Notifications | Badge rule evaluator, notifications inbox, mark as read, event dispatch (`T26, T27`) | ⏳ Ready |
| **Phase 18** | Performance, Caching & Concurrency | In-memory cache for catalogs, `RowVersion` optimistic concurrency | ⏳ Ready |
| **Phase 19** | OpenAPI v3 Contracts & Postman Collection | OpenAPI generation, XML docs, Scalar/Swagger UI, Postman collection (`T28-T31`) | ⏳ Ready |
| **Phase 20** | Live Verification & Release Package | PowerShell live runner, zero cascade verification, ZIP archive refresh (`T32-T34`) | ⏳ Ready |

---

## Detailed Phase Roadmaps & Bounded Contexts

### Phase 01: Solution Scaffold & Baseline Infrastructure (Completed)
- **T1:** Initialize .NET 10 solution, enforce `Nullable=enable`, `TreatWarningsAsErrors=true`.
- **T2:** Automated GitHub Actions CI workflow, PR verification.
- **T3:** Single-column `DeletedAt` soft-delete base and EF Core `SaveChangesInterceptor`.
- **T4:** Global exception middleware with clean `BaseResponse<T>` JSON envelope.

### Phase 02: Relational Schema & 22 Domain Entities (Completed)
- **T5:** Relational mapping diagram fixes (eliminate all physical cascades).
- **T6-T9:** 22 domain entity configurations with `OnDelete(Restrict)`.
- **T10:** Initial EF Core migration with zero cascade actions verified against SQL Server.
- **T11:** Idempotent database seeder with administrative credentials.

### Phase 03: Core Identity & JWT Authentication (Completed)
- **T12:** Argon2/PBKDF2 password hashing, registration, login, role claims.
- **T13:** Refresh token rotation, revocation on logout, short-lived access tokens.

### Phase 04: Academic Catalog: Universities & Faculties (Completed)
- **T14:** Search universities, faculties, anonymous catalog lookups for registration UX.

### Phase 05: Student Discovery & Account Management (Completed)
- **T15:** Student directory search, public portfolio cards, self/admin account deletion.

### Phase 06: Password Recovery & Platform Governance (Completed)
- **T-AUTH4:** Crypto tokens for password reset flow, expiry validation.
- **T-AUTH5:** Dedicated `AdminController`, dashboard statistics, universal restore.

### Phase 07: Projects, Teams & Sprint Task Management (Completed)
- **T17-T19:** PR #1 merged. Projects CRUD, team formation, project tasks, and envelope alignment.

---

### Phase 08: Security Remediation & Cryptographic Hardening (Immediate Next)
- **P08.1:** Remove `ResetToken` from `ForgotPasswordResponse` to eliminate account takeover vulnerability.
- **P08.2:** Update `AuthAndProfileTests` to retrieve reset tokens directly via test DbContext scope.
- **P08.3:** Hash refresh tokens and password reset tokens (`SHA-256`) at rest in database.
- **P08.4:** Implement IP-partitioned rate limiter (`PartitionedRateLimiter`) on auth endpoints.

### Phase 09: Domain Model Factorization & Bounded Contexts
- **P09.1:** Deconstruct monolithic `Entities.cs` into `Domain/Entities/Identity/`.
- **P09.2:** Deconstruct into `Domain/Entities/Projects/`.
- **P09.3:** Deconstruct into `Domain/Entities/Organizations/`.
- **P09.4:** Deconstruct into `Domain/Entities/Gamification/`.

### Phase 10: Clean Architecture Layer Realignment
- **P10.1:** Define `IAppDbContext` interface in `TeamUp.Application.Common.Interfaces`.
- **P10.2:** Implement `IAppDbContext` in `TeamUp.Infrastructure.Persistence.AppDbContext`.
- **P10.3:** Migrate service implementations from `Infrastructure/Services` to `Application/Services`.
- **P10.4:** Reorganize dependency injection extensions across Application and Infrastructure.

### Phase 11: Interface Segregation (ISP) for Projects & Tasks
- **P11.1:** Segregate `IProjectService` into `IProjectService` (Core CRUD & Discovery).
- **P11.2:** Create `IProjectMemberService` & `IProjectApplicationService`.
- **P11.3:** Create `IProjectTaskService` for sprint board operations.
- **P11.4:** Refactor `ProjectsController` and `TasksController` to consume segregated services.

### Phase 12: Module 04 — Skills Master Catalog & Student Claims (`T16`)
- **P12.1:** Skills catalog DTOs, FluentValidators, and master CRUD service.
- **P12.2:** `SkillsController` (`GET /api/v1/skills`, `POST`, `PUT`, `DELETE`).
- **P12.3:** Student claimed skills (`PUT/DELETE /api/v1/students/{id}/skills/{skillId}`) with proficiency levels.
- **P12.4:** Comprehensive unit and integration test suite for Module 04.

### Phase 13: Module 08 & 09 — Organizations & Applications (`T20, T21`)
- **P13.1:** Organization DTOs, validators, and `IOrganizationService`.
- **P13.2:** `OrganizationsController` (`GET/POST /api/v1/organizations`, `PATCH`, `DELETE`).
- **P13.3:** Membership applications lifecycle (`POST apply`, `PATCH review`, `DELETE withdraw`).
- **P13.4:** Integration tests for club creation, role promotion, and membership approval.

### Phase 14: Module 10 — Campus Events & Multi-Session Schedules (`T22`)
- **P14.1:** Event and Session DTOs, validators, and date/capacity validation.
- **P14.2:** `EventsController` (`GET/POST /api/v1/events`, `PATCH`, `DELETE`).
- **P14.3:** Nested session schedule management (`POST /events/{id}/sessions`, `PUT`, `DELETE`).
- **P14.4:** Service-level transactional cascade soft-deletion of sessions when event is deleted.

### Phase 15: Module 11 & 12 — Registrations & Attendance (`T23, T24`)
- **P15.1:** Event registration with capacity check and waitlist status calculation.
- **P15.2:** `EventRegistrationsController` (`POST /events/{id}/registrations`, `DELETE`).
- **P15.3:** Session check-in endpoint with verification methods (`QRCode`, `ManualPin`, `NFC`).
- **P15.4:** Uniqueness enforcement: one check-in per student per session.

### Phase 16: Module 13 — Gamification Scoring Ledger (`T25`)
- **P16.1:** Append-only ledger service for `ScoreTransaction`.
- **P16.2:** Level calculation engine: `Level = FLOOR(SQRT(TotalPoints / 100)) + 1`.
- **P16.3:** Wire automatic score awarding hooks into task completion and session attendance.
- **P16.4:** Student score profile endpoints (`GET /students/{id}/score`, `GET /score-transactions`).

### Phase 17: Module 14 & 15 — Badges Engine & Notifications Inbox (`T26, T27`)
- **P17.1:** Badge criteria evaluation engine (idempotent badge awards).
- **P17.2:** `BadgesController` (`GET/POST /api/v1/badges`, `GET /students/{id}/badges`).
- **P17.3:** Notifications inbox service (`IHasDomainEvents` dispatcher integration).
- **P17.4:** `NotificationsController` (`GET inbox`, `PATCH mark-read`, `PATCH mark-all-read`, `DELETE`).

### Phase 18: Performance, Caching & Concurrency Hardening
- **P18.1:** In-memory caching (`IMemoryCache`) for static catalogs (Universities, Faculties, Skills).
- **P18.2:** Optimistic concurrency tokens (`[Timestamp] byte[] RowVersion`) for capacity entities.
- **P18.3:** Database index review: covered composite indexes on foreign keys and active soft-delete filters.

### Phase 19: OpenAPI v3 Contracts & Postman Collection (`T28-T31`)
- **P19.1:** Enable XML documentation generation across Api and Application.
- **P19.2:** Configure OpenAPI v3 with Bearer JWT security scheme and Swagger/Scalar UI.
- **P19.3:** Export verified `docs/api/openapi.json`.
- **P19.4:** Generate and export postman environment and collection in `docs/api/postman/`.

### Phase 20: Comprehensive Live Verification & Release Package (`T32-T34`)
- **P20.1:** Extend PowerShell automated test runner `scripts/test_live_api.ps1` to cover all 99 endpoints.
- **P20.2:** Run live verification against SQL Server Docker instance and refresh `AUTH-PROFILE-TEST-REPORT.md`.
- **P20.3:** Execute security audit checklist (`security-audit` skill) with zero high/critical issues.
- **P20.4:** Package clean staging release archive to `C:\Users\ahmed\Downloads\TeamUp.zip`.
