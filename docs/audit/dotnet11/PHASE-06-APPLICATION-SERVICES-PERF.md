# Phase 06 Audit — Application Services Performance, Concurrency & `.NET 11` Runtime Readiness

**Date:** 2026-11-26  
**Scope:** `backend/src/TeamUp.Application/` (15 Services, 3 Security Services, 7 Domain Event Handlers, DTOs, Validators).

---

## 1. High-Priority Findings & Concrete Fixes

| ID | Severity | Location | Finding | Recommendation (Apply Phase 6) |
|---|---|---|---|---|
| APP-01 | **P0** | `EventService.cs:415-455, 467-493`; `GamificationDomainEventHandlers.cs:150-190`; `AppDbContext.cs:59-64` | `RegisterAsync` and `CancelRegistrationAsync` call `SaveChangesAsync` before `transaction.CommitAsync`, dispatching `EventRegisteredDomainEvent` / `WaitlistPromotedDomainEvent` **inside** a `Serializable` + `UPDLOCK/ROWLOCK` transaction, so SMTP email I/O holds database locks! | Suppress auto-dispatch or dispatch email-sending notifications *after* `transaction.CommitAsync` completes (or decouple email sends post-commit) so SMTP latency never holds SQL row locks or aborts committed seats. |
| APP-02 | **P0** | `FacultyService.cs:42-54, 90-116` | `GetFacultyByIdAsync` and `UpdateFacultyAsync` omit `.Include(f => f.Students)` / scalar count projection and always return `StudentsCount = 0`. | Project `f.Students.Count` in SQL (`GetFacultyByIdAsync`) and query `CountAsync` (`UpdateFacultyAsync`). |
| APP-03 | **P0** | `ProjectService.cs:197-223` | `ReviewApplicationAsync` loads `project` and `application` *before* opening the `Serializable` transaction and lacks `UPDLOCK, ROWLOCK` on SQL Server, creating a seat-capacity race window. | Open `CreateExecutionStrategy().ExecuteAsync` + `BeginTransactionAsync(IsolationLevel.Serializable)` first, lock `Project` (`UPDLOCK, ROWLOCK` on SQL Server), and re-read application & member count inside the transaction. |
| APP-04 | **P1** | `GamificationDomainEventHandlers.cs:80-92` | `ProjectCompletedDomainEventHandler` executes ~8 sequential DB queries/saves per team member in a loop. | Preload existing `ScoreTransactions` for `MemberStudentIds` once and insert missing awards in a single `SaveChangesAsync` batch. |
| APP-05 | **P1** | `DigestDispatchService.cs:39-58`; `ProjectRecommendationService.cs:187-192`; `OrganizationAuthorizationService.cs:50-60` | `DispatchLeaderboardDigestAsync` loads **every student's entire score sum** into memory before `.Take(_leaderboardSize)`; `GetRecommendedProjectsForStudentsAsync` queries `ProjectApplications` across *all* students in the pool instead of filtering `studentIds.Contains(a.StudentId)`; `GetAdminOrganizationIdsAsync` filters memberships in memory. | Apply `.Take(_leaderboardSize)` in SQL for leaderboard digest; add `&& studentIds.Contains(a.StudentId)` in `ProjectRecommendationService`; filter admin roles/`HighBoard` in SQL in `OrganizationAuthorizationService`. |
| APP-06 | **P1** | `ProfileService.cs:216` | `GetPublicShowcaseAsync` hardcodes `/ 100.0` for student level calculation instead of using `IGamificationService.CalculateLevelMetrics` (`GamificationOptions.LevelStepPoints`). | Inject `IGamificationService` (or `IOptions<GamificationOptions>`) into `ProfileService` so level math stays consistent everywhere. |
| APP-07 | **P1** | `OrganizationService.cs:381-394` | `EnsureNotLastAdminAsync` checks only `OrganizationRoles.IsAdminRole(x.Role)`, ignoring `HierarchyTier == OrganizationHierarchyTier.HighBoard` (which `OrganizationAuthorizationService` grants admin rights to), and materializes roles with `ToListAsync()`. | Align predicate with `OrganizationAuthorizationService` and use `AnyAsync` in SQL. |
| APP-08 | **P2** | `ProjectService.cs:100-102, 157-159`; `ProfileService.cs:287-291`; `OrganizationService.cs:241-254, 485-490` | Multiple bulk updates/soft-deletes load child entities into memory and loop before `SaveChangesAsync`. | Convert `RemoveMemberAsync`, `DeleteStudentAccountAsync`, and `DeleteUnitAsync` (task/unit unassignment) to `.ExecuteUpdateAsync(...)` and check `SoftDelete` timestamps explicitly via `TimeProvider`. |
| APP-09 | **P2** | `BadgeService.cs:21-88`; `SkillService.cs:144`; `EventService.cs:79-84, 698-701`; `ProjectService.cs:164, 172` | Missing `.AsNoTracking()` on read queries (`RecordAttendanceCoreAsync`, `GetMyApplicationAsync`, `GetApplicationsAsync`, `BadgeService`), ignored `.Include()` before `.Select()`, and missing `.AsSplitQuery()` on `QueryEventsCoreAsync` (`Include(x => x.Sessions)` with `Skip/Take`). | Add `.AsNoTracking()`, remove redundant `.Include()` before `.Select()`, and add `.AsSplitQuery()` on `QueryEventsCoreAsync`. |
| APP-10 | **P2** | `DigestDispatchService.cs:365` | `BuildLeaderboardRowsHtml(IEnumerable<dynamic> topRows)` uses `dynamic` DLR dispatch (suboptimal for `.NET 11` RuntimeAsync & trimming). | Replace `dynamic` with a strongly-typed `private sealed record LeaderboardDigestRow(...)`. |

---

## 2. `.NET 11` Runtime Async Verification
- Audited all 79 Application files: **0 `ThreadStatic`**, **0 `AsyncLocal`**, **0 `.Result` / `.Wait()`** blocking calls, and **100% `CancellationToken` forwarding** across async service signatures. Fully compatible with `.NET 11` Runtime-Native Async!
