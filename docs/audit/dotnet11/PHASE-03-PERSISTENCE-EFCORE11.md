# Phase 03 Audit — EF Core 11, Persistence & SQL Server/SQLite (`TeamUp.Infrastructure`)

**Date:** 2026-11-26  
**Scope:** `AppDbContext.cs`, `Configurations/*.cs`, `SoftDeleteInterceptor.cs`, `AppDbContextFactory.cs`, `DependencyInjection.cs`, `Migrations/*`, `Seed/DbSeeder.cs`, `Reports/AdminReportPdfService.cs`, `Services/*`.

---

## 1. Critical Findings (P0 / P1)

| ID | Severity | Location | Finding | Recommendation (Apply Phases 3 & 6) |
|---|---|---|---|---|
| EF-01 | **P0** | `DependencyInjection.cs:82` (`EnableRetryOnFailure()`) vs `EventService.cs:230,415,467`, `OrganizationService.cs:163,237,779,1070`, `ProjectService.cs:98,155,202` | `SqlServerRetryingExecutionStrategy` **throws `InvalidOperationException`** whenever user-initiated `BeginTransactionAsync` is called outside `CreateExecutionStrategy().ExecuteAsync(...)`. Breaks event registration, org workflows, and project mutations on SQL Server! | Wrap all manual `BeginTransactionAsync` blocks in `CreateExecutionStrategy().ExecuteAsync(...)` (via a shared helper `DatabaseExecutionExtensions` or `IAppDbContext` helper). |
| EF-02 | **P0** | `EventConfigs.cs:97` (`Event.IsPublic`) & `IdentityConfigs.cs:50-52` (`Student.EmailNotificationsEnabled`, `ReceiveLeaderboardDigests`, `ReceiveProjectMatchEmails`) | `.HasDefaultValue(true)` without `.HasSentinel(...)` on `bool` CLR properties causes EF Core to omit `false` on `INSERT`, letting SQL Server's `DEFAULT (1)` silently overwrite explicit `false` opt-outs! (`OrganizationMember.HierarchyTier` already spotted and used `.HasSentinel`.) | Remove `.HasDefaultValue(true)` (or configure `.HasSentinel(true)` / drop DB default constraint cleanly in a migration) so explicit `false` values persist faithfully. |
| EF-03 | **P1** | `EventConfigs.cs:88-112`, `ProjectConfigs.cs:32-46` | `Event.CoverImageUrl` and `Project.CoverImageUrl` lack `.HasMaxLength(500)` (`nvarchar(max)` in `20261125121701_AddCoversAndSystemImages`), unlike `Organization.CoverImageUrl` (`EventConfigs.cs:19`). | Add `.HasMaxLength(500)` to both `EventConfig` and `ProjectConfig` and generate EF Core 11 migration `HardenCoverImageUrlMaxLength` (zero cascade). |
| EF-04 | **P1** | `Program.cs:350-533` (`sqlitePatchStatements`) | Inline 180-line SQLite dev patcher swallows all exceptions via bare `catch {}` and **omits** updating `ScoreTransaction`'s `CK_ScoreTransaction_SourceType` check constraint to include `'AttendanceRevocation'`, causing `SQLite Error 19` on attendance revocation in legacy `TeamUp.dev.db`! | Extract into `SqliteDevSchemaPatcher` in `TeamUp.Infrastructure/Persistence/`, inspect `pragma_table_info` / `sqlite_master` explicitly (only ignoring benign duplicate-column errors), and rebuild `ScoreTransaction` when `'AttendanceRevocation'` is missing. Add unit test! |
| EF-05 | **P1** | `DependencyInjection.cs:82`, `AppDbContextFactory.cs:45` | EF Core 11 + `Microsoft.Data.SqlClient 7.0` defaults `Encrypt=True` and benefits from explicit SQL Server 2022 compatibility level `160`. | Configure `sql.UseCompatibilityLevel(160)` in `UseSqlServer(...)` and verify local/prod TLS settings (`TrustServerCertificate=True` only in Dev/Factory). |

---

## 2. Secondary Findings (P2 / P3)

- **[P1] `SoftDeleteInterceptor` vs `ExecuteUpdateAsync`/`ExecuteDeleteAsync`:** `ExecuteDeleteAsync` bypasses `SaveChangesInterceptor`. Never use `ExecuteDeleteAsync` on `ISoftDelete` or `IAppendOnly` entities; when converting `DeleteOrganizationAsync` / `DeleteProjectAsync` to `ExecuteUpdateAsync(s => s.SetProperty(x => x.DeletedAt, now))`, ensure timestamp is explicitly set via `TimeProvider`.
- **[P2] `TimeProvider` Injection:** `SoftDeleteInterceptor.cs:29`, `JwtTokenService.cs:28`, `InMemoryEmailSender.cs`, `AdminReportPdfService.cs`, and `DbSeeder.cs` call `DateTime.UtcNow` directly. Register `TimeProvider.System` in DI and inject into Infrastructure services.
- **[P2] `JwtTokenService.cs:63,68-70` & `TemplateRenderer.cs:18-20`:** Migrate `JwtSecurityTokenHandler` -> `JsonWebTokenHandler` (or keep verified token behavior), `RandomNumberGenerator.Create()` -> `RandomNumberGenerator.GetBytes(64)`, and `new Regex(..., RegexOptions.Compiled)` -> `[GeneratedRegex]`.
- **[P2] `QuestPDF` Font Embedding (`AdminReportPdfService.cs:175`):** `.FontFamily("Lato")` relies on host OS fonts (risky on Linux containers). Document for Linux Docker deployments.
- **[P2] `StoredImageConfig.cs`:** `UploadedAt` lacks `.HasDefaultValueSql("SYSUTCDATETIME()")` on SQL Server (unlike other entities).
- **[P2] Zero-Cascade Migration Audit:** All 13 existing migrations verified **0 `Cascade` / 0 `SetNull`** (`ReferentialAction.Restrict` throughout).
