# TeamUp Master `.NET 11` & Full-Repo Audit Specification (`00-MASTER-AUDIT-SPEC`)

**Date:** 2026-11-26  
**Baseline:** `TeamUp` (`e9a70b3` + verified Round 5 fixes + Stage 1 polish: 128 Unit Tests, 104 Integration Tests, 95 Vitest Tests, SQL Server `TeamUpDb` up to date).

---

## 1. Executive Summary Across All 10 Audit Phases

Auditing the full 392-file codebase (`211` backend `.cs` + `181` frontend `.ts/.tsx`), CI/CD pipelines, SQL scripts, and documentation surfaced **5 P0 critical runtime/correctness hazards**, **19 P1 high-impact gaps**, and **20+ P2/P3 modernization opportunities** as we upgrade from `.NET 10` (`net10.0` / C# 14) to **`.NET 11` (`net11.0` / C# 15 / ASP.NET Core 11 / EF Core 11)**.

### Prioritized P0 & P1 Findings Register

| Priority | ID | Area | Summary | Apply Phase |
|---|---|---|---|---|
| **P0** | EF-01 | Infrastructure / Services | `EnableRetryOnFailure()` breaks all 10 manual `BeginTransactionAsync` paths on SQL Server unless wrapped in `CreateExecutionStrategy().ExecuteAsync(...)`. | Phase 3 / Phase 6 |
| **P0** | EF-02 | Persistence Configs | `HasDefaultValue(true)` without `HasSentinel` on `Event.IsPublic` & 3 `Student` email-preference bools silently overwrites `false` opt-outs with `true` on `INSERT`. | Phase 3 |
| **P0** | APP-01 | `EventService` / `AppDbContext` | SMTP emails fire inline inside `Serializable` + `UPDLOCK/ROWLOCK` transactions in `RegisterAsync` / `CancelRegistrationAsync`. Move email dispatch post-commit. | Phase 6 |
| **P0** | APP-02 | `FacultyService` | `GetFacultyByIdAsync` and `UpdateFacultyAsync` omit `Students` count projection and always return `StudentsCount = 0`. | Phase 6 |
| **P0** | APP-03 | `ProjectService` | `ReviewApplicationAsync` loads `project`/`application` outside `Serializable` transaction and lacks SQL Server `UPDLOCK, ROWLOCK`. | Phase 6 |
| **P1** | BLD-01 | SDK & Build | Missing `global.json`, `Directory.Build.props`, `Directory.Packages.props`; upgrade SDK to `11.0.100`, `dotnet-ef` & Microsoft packages to `11.0.0`. | Phase 1 |
| **P1** | DOM-01 | `TeamUp.Domain` | `IDomainEvent.OccurredOn` evaluates `DateTime.UtcNow` lazily on every read instead of capturing event creation instant. | Phase 2 |
| **P1** | EF-03 | Persistence Configs | `Event.CoverImageUrl` & `Project.CoverImageUrl` lack `HasMaxLength(500)`. | Phase 3 |
| **P1** | EF-04 | SQLite Dev Patcher | Inline `Program.cs` SQLite DDL patcher lacks `ScoreTransaction` `CK_ScoreTransaction_SourceType` (`'AttendanceRevocation'`) rebuild and swallows exceptions silently. Extract to `SqliteDevSchemaPatcher` + unit tests. | Phase 3 |
| **P1** | EF-05 | SQL Server 2022 / EF11 | Configure `UseCompatibilityLevel(160)` and verify `Microsoft.Data.SqlClient 7.0` TLS behavior. | Phase 3 |
| **P1** | API-01/02 | `Program.cs` / OpenAPI | Modularize 625-line `Program.cs` into `Extensions/`, update `AddOpenApi` for OpenAPI 3.2 & `OpenApiSecuritySchemeReference`. | Phase 4 |
| **P1** | SEC-01 | `SafeUrlValidator` / Org | `OrganizationValidators` `LogoUrl` uses `IsSafeExternalHttpUrl` (rejecting `/api/v1/images/{guid}`); `IsSafeAssetOrExternalUrl` allows `/\evil.com` & arbitrary paths; `localhost.` trailing-dot bypass. | Phase 5 |
| **P1** | SEC-02 | `SecurityStartupValidator` | `appsettings.Development.json` JWT key not in `ForbiddenKeyMarkers`; Production allows blank `Email:SmtpHost` and `Sqlite`. | Phase 5 |
| **P1** | SEC-03 | `ImagesController` / Profiles | Upload endpoints lack dedicated `"upload-policy"` rate limit (`10/min`). | Phase 5 |
| **P1** | APP-04..07 | Application Services | Batch `ProjectCompletedDomainEventHandler`; bound `DigestDispatchService` leaderboard/recipients; fix `ProfileService` hardcoded `100.0` level math; align `EnsureNotLastAdminAsync` with `HighBoard`. | Phase 6 |
| **P1** | FE-01..04 | Frontend Contracts | Fix `optionalSafeAssetOrUrlSchema` for DB `logoUrl`/icons, `ProjectDetailPage` `apply` invalidation, `NotificationBell` exact `unreadCount`, and `ProjectDetailDto.CoverImageUrl`. | Phase 7 |
| **P1** | OPS-01/02 | CI/CD & MonsterASP | Bump workflows & scripts to `11.0.x` / `.NET 11`; add `SeedImages` validation + `runtimes/` pruning to `publish-monsterasp.ps1`. | Phase 9 |
| **P1** | DOC-01..03 | Docs / SQL / OpenAPI | Regenerate stale `teamup-sqlserver-idempotent.sql`, `openapi.json`, `TeamUp_Collection.json`, and update `README.md`, `wiki/ENVIRONMENT.md`, `API-ENDPOINTS.md`. | Phase 10 |

---

## 2. 10-Phase Execution Roadmap (Stage 3)

- **Apply Phase 1:** Install `.NET 11 SDK` (`11.0.100`), add `global.json`, `Directory.Build.props`, `Directory.Packages.props`, upgrade all 6 projects + `dotnet-ef` to `11.0.0`, resolve ASP.NET Core 11 / `Microsoft.OpenApi 3.x` breaks.
- **Apply Phase 2:** Fix `IDomainEvent.OccurredOn` capture, modernize `OrganizationRoles` (`FrozenSet` / C# 15 collection expressions), evaluate C# 15 `closed` `IDomainEvent` hierarchy, add Domain docblock headers.
- **Apply Phase 3:** Fix `HasDefaultValue(true)` sentinel bug (`Event.IsPublic`, `Student` prefs), add `HasMaxLength(500)` to `CoverImageUrl` + EF Core 11 migration, add `UseCompatibilityLevel(160)`, extract & test `SqliteDevSchemaPatcher` (incl. `ScoreTransaction` CHECK constraint), add `ExecuteInTransactionAsync` execution-strategy helper, inject `TimeProvider.System`.
- **Apply Phase 4:** Modularize `Program.cs` into `TeamUp.Api/Extensions/`, configure OpenAPI 3.2 + security requirement, `OutputCache` (with auth-aware catalog policy & tag eviction), Response Compression, `/health/live` & `/health/ready` + `/health`, and fix `/templates/preview/` CSP.
- **Apply Phase 5:** Fix `SafeUrlValidator` (strict `/api/v1/images/{guid}`, block `\`, trim trailing `.`), switch `OrganizationValidators` `LogoUrl` to `IsSafeAssetOrExternalUrl`, harden `SecurityStartupValidator` (dev JWT key, SMTP host, prod Sqlite), add `"upload-policy"` rate limit, timing-safe dummy PBKDF2 in `AuthService.LoginAsync`, and `ValidateDataAnnotations().ValidateOnStart()` on all 7 Options classes.
- **Apply Phase 6:** Wrap all 10 `BeginTransactionAsync` calls with execution strategy, move `EventService` email notifications post-commit, fix `FacultyService.StudentsCount`, harden `ProjectService.ReviewApplicationAsync` row lock, batch `ProjectCompletedDomainEventHandler` & `DigestDispatchService`, align `ProfileService` level step & `EnsureNotLastAdminAsync`, apply `.AsNoTracking()` / `.ExecuteUpdateAsync` / remove `dynamic`.
- **Apply Phase 7:** Update `urlValidation.ts` (`optionalSafeAssetOrUrlSchema`), `ProjectDetailPage` `apply` invalidation, `NotificationBell` `getUnreadCount`, `ProjectDetailDto.CoverImageUrl`, `endpoints.ts`, and `m3` E2E test label; preserve the 4 pre-existing untracked frontend files untouched.
- **Apply Phase 8:** Run full unit + integration + frontend test suites, boot `.NET 11` API against live `.\SQLEXPRESS` (`TeamUpDb`), verify `/health`, `/health/live`, `/health/ready`, `/openapi/v1.json`, `/api/v1/images/{id}`, 10 seeded `StoredImage` rows in SQL Server, and stop daemon.
- **Apply Phase 9:** Update `.github/workflows/ci.yml`, `deploy-monsterasp.yml`, `scripts/publish-monsterasp.ps1`, `scripts/test_live_api.ps1` for `.NET 11` + `SeedImages` + `runtimes/` pruning, and test `dotnet publish -c Release`.
- **Apply Phase 10:** Export live OpenAPI 3.2 to `docs/api/openapi.json`, regenerate `docs/database/teamup-sqlserver-idempotent.sql` (0 cascades), update `README.md`, `wiki/ENVIRONMENT.md`, `API-ENDPOINTS.md`, `TeamUp_Collection.json`, write `99-FINAL-EXECUTION-REPORT.md`, and run final verification gate.
