# Phase 08 Audit — Testing Quality, Coverage Gaps & SQL Server Fidelity

**Date:** 2026-11-26  
**Scope:** `backend/tests/TeamUp.UnitTests` (14 files, 128 tests), `backend/tests/TeamUp.IntegrationTests` (26 files, 104 tests), `frontend/src/test` (12 files, 95 tests), `frontend/e2e` (5 specs).

---

## 1. Key Findings & Coverage Gaps

| ID | Severity | Location | Finding | Remediation (Apply Phases 3–8) |
|---|---|---|---|---|
| TST-01 | **P1** | `TeamUpApiFactory.cs:92-135`, `CachingAndConcurrencyTests.cs:73-120` | Zero tests exercise real SQL Server (`MigrateAsync`, `WITH (UPDLOCK, ROWLOCK)`, `rowversion` concurrency exceptions `409`, `BeginTransactionAsync` with `EnableRetryOnFailure()`, or `UseCompatibilityLevel(160)`). | Verify live `.NET 11` API + migrations + seeded DB against local SQL Server `.\SQLEXPRESS` (`TeamUpDb`), and add a concurrency/retry-strategy test. |
| TST-02 | **P1** | `Program.cs:350-533` (`SqliteDevSchemaPatcher`) | Legacy SQLite dev DB upgrade paths (nullable `Student.FacultyId`, nullable `StoredImage.UploadedByStudentId`, and `ScoreTransaction` `CK_ScoreTransaction_SourceType` `'AttendanceRevocation'` rebuild) have 0% test coverage. | Extract `SqliteDevSchemaPatcher` into `TeamUp.Infrastructure/Persistence/` and add unit tests upgrading pre-Round-5 SQLite schemas losslessly. |
| TST-03 | **P1** | `HealthTests.cs:11-16`, `CachingAndConcurrencyTests.cs:41-71` | `/health/live`, `/health/ready`, `/health` 503 degraded branch, `OutputCache` tag eviction, `SafeUrlValidator` backslash/CGNAT/IPv6 ULA edge cases, `"upload-policy"` rate limit, and invalid Options `ValidateOnStart()` have zero coverage. | Write TDD unit + integration tests for every new Phase 3–5 behavior. |
| TST-04 | **P2** | `OpenApiAndPostmanTests.cs:11-158` | Only tests OpenAPI in Development without asserting `Bearer` scheme or Production 404; Postman collection check constructs a hardcoded JSON object rather than validating parity against live routes. | Expand `OpenApiAndPostmanTests` to assert `Bearer` security scheme and Production `/openapi/v1.json` 404. |
| TST-05 | **P2** | `AttendanceBurstTests.cs:43-44`, `PaginationAndInboxTests.cs:119`, `AuthAbuseProtectionTests.cs:48,85` | Tests rely on `Task.Delay` or direct DB timestamp mutation because services use `DateTime.UtcNow` instead of `TimeProvider`. | Inject `TimeProvider.System` in core services so future tests can use `FakeTimeProvider` cleanly. |
| TST-06 | **P3** | `TeamUpApiFactory.cs:78,94`, `XpIntegrityAndTaskAuthorizationTests.cs:133` | Synchronous `.GetAwaiter().GetResult()` / `.Result` and process-wide `Environment.SetEnvironmentVariable` in test helpers. | Clean up synchronous `.Result` usage in `XpIntegrityAndTaskAuthorizationTests.cs` if touched. |
