# Code review: TeamUp Round 5 + .NET 11 Diff (Backend & Frontend)

**Pre-flight classification:** Strict **Refactor Review** (observable behavior must remain unchanged). Every recommendation that alters observable behavior, error propagation, validation acceptance, or public API signatures is separated and explicitly labelled **`Behavior change — confirm with author`**, distinct from **`Pure refactor`** items.

**Date:** 2026-11-26  
**Skill Applied:** `clean-code-guard` ([SKILL.md](file:///C:/Users/ahmed/.gemini/config/skills/clean-code-guard/SKILL.md) & [review-checklist.md](file:///C:/Users/ahmed/.gemini/config/skills/clean-code-guard/references/review-checklist.md))  
**Scope:** Changed & newly added source files across `backend/src` (37 files) and `frontend/src`.  
**Mode:** Read-only findings report (no application source files edited).

---

## Summary
**Verdict: Needs work before merge on Critical items.** The Round 5 + `.NET 11` work delivers major structural upgrades—a modularized 74-line `Program.cs`, OpenAPI 3.2, `OutputCache` invalidation, post-commit domain event dispatch, hardened URL/upload validators, self-resetting `CoverImage`, and gated React queries—with zero commented-out code or `Step N` comment pollution. However, the review uncovered **16 Critical findings** (9 Backend, 7 Frontend) alongside SOLID, DRY, KISS, and parameter-explosion refactoring opportunities across large services and page components.

---

## Critical findings

### Backend (`backend/src`)

1. **Silent `OutputCache` eviction miss on plural entity types in `AdminController.RestoreEntity`** — `backend/src/TeamUp.Api/Controllers/AdminController.cs:72-83` *(cross-ref `AdminService.cs:228,245,265,290,322`)*
   - **Classification:** `Behavior change — confirm with author` (fixes plural-route cache invalidation) + `Pure refactor` (centralizing tag mapping)
   - **Evidence:**
     ```csharp
     var tagToEvict = entityType.Trim().ToLowerInvariant() switch
     {
         "university" or "faculty" => "catalog-universities",
         "skill" => "catalog-skills",
         "organization" => "catalog-org-categories",
         "badge" => "catalog-badges",
         _ => null
     };
     ```
     `AdminService.RestoreEntityAsync` accepts plural names (`"universities"`, `"faculties"`, `"skills"`, `"organizations"`, `"badges"`) and evicts `IMemoryCache`, but `AdminController` only matches singular names, leaving `tagToEvict` as `null` and keeping stale HTTP `OutputCache` responses alive for 5 minutes.
   - **Principle violated:** AI Failure Mode #13 (Plausible-but-wrong code), DRY, OCP.
   - **Suggested fix:** Match both singular and plural forms—or have `RestoreEntityAsync` return the normalized entity type and resolve the tag via one shared `CatalogOutputCacheTags` dictionary.

2. **Hardcoded mock match data (`85%`, `["React", "TypeScript"]`) in production digest code** — `backend/src/TeamUp.Application/Services/DigestDispatchService.cs:284-305` *(specifically `:295, :302`)*
   - **Classification:** `Behavior change — confirm with author`
   - **Evidence:**
     ```csharp
     .Select(p => new ProjectMatchRecommendationDto(
         p.ProjectId, p.Title, p.Description,
         85,
         p.Members.Count(), p.MaxTeamSize, Math.Max(p.MaxTeamSize - p.Members.Count(), 0),
         ...
         new[] { "React", "TypeScript" },
         Array.Empty<string>(), Array.Empty<string>()))
     ```
   - **Principle violated:** AI Failure Mode #12 (*"Declares success" — mock fallbacks in production code*).
   - **Suggested fix:** When `TestToMe` finds zero real matches, either score projects using `ComputeMatchScore` against their actual required skills, render an empty-state digest notice, or fail explicitly instead of sending fabricated `85%` / `React` / `TypeScript` recommendations.

3. **Bare unlogged `catch` in `/health` and swallowed cancellation in `DatabaseReadinessHealthCheck`** — `backend/src/TeamUp.Api/Extensions/HealthCheckExtensions.cs:52-68, :89-92`
   - **Classification:** `Pure refactor` (removing `/health` duplication + adding logging) + `Behavior change — confirm with author` (propagating `OperationCanceledException` instead of converting client disconnects into `503`)
   - **Evidence:**
     ```csharp
     // HealthCheckExtensions.cs:64-67
     catch
     {
         return Results.Json(new { status = "Unhealthy", database = "Disconnected" }, statusCode: StatusCodes.Status503ServiceUnavailable);
     }
     // HealthCheckExtensions.cs:89-92
     catch (Exception ex)
     {
         return HealthCheckResult.Unhealthy("Disconnected", ex);
     }
     ```
   - **Principle violated:** AI Failure Mode #1 (Catch-all error swallowing) + DRY (`/health` duplicates `/health/ready` and `DatabaseReadinessHealthCheck`).
   - **Suggested fix:** Reuse the `ready` `HealthCheckOptions` for `/health` (`MapHealthChecks("/health", readyOptions)`), catch `Exception ex when (ex is not OperationCanceledException)`, and log failures with `ILogger<DatabaseReadinessHealthCheck>`.

4. **Broad `catch (Exception ex)` swallowing startup errors + `SeedImages` path & "embedded fallback" mismatch** — `backend/src/TeamUp.Api/Extensions/DatabaseStartupExtensions.cs:66-84` & `backend/src/TeamUp.Application/Services/ImageService.cs:78-85`
   - **Classification:** `Behavior change — confirm with author`
   - **Evidence:**
     ```csharp
     await imageService.EnsureDefaultCoversAsync(Path.Combine(app.Environment.ContentRootPath, "SeedImages"));
     ...
     catch (Exception ex)
     {
         app.Logger.LogWarning(ex, "Default cover seeding skipped; embedded fallback will serve default covers.");
     }
     // ImageService.cs:84
     var path = Path.Combine(AppContext.BaseDirectory, "SeedImages", cover.FileName);
     ```
   - **Principle violated:** AI Failure Mode #1, AI Failure Mode #13, Section B (Disinformation comments: `SeedImages/*.png` are loose disk files copied to output via `TeamUp.Api.csproj`, not `EmbeddedResource`s, and startup reads `ContentRootPath` while GET reads `AppContext.BaseDirectory`).
   - **Suggested fix:** Use one canonical source (`AppContext.BaseDirectory` or true embedded resources), fix the misleading comments, and narrow `catch` so unexpected DI/cancellation errors are not silently suppressed.

5. **Leaked `_deferDomainEvents` flag when `BeginTransactionAsync` throws + unforwarded `CancellationToken`** — `backend/src/TeamUp.Application/Common/Interfaces/DatabaseTransactionExtensions.cs:34-51`
   - **Classification:** `Behavior change — confirm with author`
   - **Evidence:**
     ```csharp
     db.ClearDeferredDomainEvents();
     db.BeginDeferringDomainEvents();
     await using var transaction = await db.Database.BeginTransactionAsync(isolationLevel, ct);
     try { ... }
     catch
     {
         db.ClearDeferredDomainEvents();
         await transaction.RollbackAsync(CancellationToken.None);
         throw;
     }
     ```
   - **Principle violated:** AI Failure Mode #13 (Plausible-but-wrong state cleanup). If `BeginTransactionAsync` throws before entering `try`, `_deferDomainEvents` remains `true` on the scoped `AppDbContext`, suppressing subsequent domain event dispatches; `strategy.ExecuteAsync` also omits `ct`.
   - **Suggested fix:** Call `BeginDeferringDomainEvents()` inside the `try` (or wrap both with cleanup) and forward `ct` to `strategy.ExecuteAsync`.

6. **Bare `catch` silently turns malformed SQL Server config/auth/TLS errors into local SQLite fallback** — `backend/src/TeamUp.Infrastructure/DependencyInjection.cs:113-129`
   - **Classification:** `Behavior change — confirm with author`
   - **Evidence:**
     ```csharp
     private static bool IsSqlServerReachable(string connectionString)
     {
         try { ... conn.Open(); return true; }
         catch { return false; }
     }
     ```
   - **Principle violated:** AI Failure Mode #1 (Catch-all error swallowing). Syntax/auth/TLS errors silently fall back to `TeamUp.dev.db` without diagnostics.
   - **Suggested fix:** Catch `SqlException` specifically (letting `ArgumentException` propagate), log a warning on fallback, and deduplicate the repeated SQLite `AddDbContext` block (`:90-93` and `:104-107`).

7. **Cancelled `CancellationToken` in `finally` leaves SQLite `PRAGMA foreign_keys = OFF` active (+ positional `SELECT *` copy)** — `backend/src/TeamUp.Infrastructure/Persistence/SqliteDevSchemaPatcher.cs:233-253, :194`
   - **Classification:** `Behavior change — confirm with author` (cancellation cleanup) + `Pure refactor` (explicit column list at `:194`)
   - **Evidence:**
     ```csharp
     // :247-252
     finally
     {
         await using var pragmaOn = conn.CreateCommand();
         pragmaOn.CommandText = "PRAGMA legacy_alter_table = OFF; PRAGMA foreign_keys = ON;";
         await pragmaOn.ExecuteNonQueryAsync(ct);
     }
     // :194
     INSERT INTO "StoredImage" SELECT * FROM "_StoredImage_legacy";
     ```
   - **Principle violated:** AI Failure Mode #13. If `ct` cancels during table rebuild, `pragmaOn.ExecuteNonQueryAsync(ct)` throws before restoring `PRAGMA foreign_keys = ON`.
   - **Suggested fix:** Pass `CancellationToken.None` in `finally`, and list all 6 `StoredImage` columns explicitly in `INSERT INTO ... SELECT ...`.

8. **Second-order placeholder injection in `TemplateRenderer.RenderReportAsync`** — `backend/src/TeamUp.Infrastructure/Services/TemplateRenderer.cs:69-80`
   - **Classification:** `Behavior change — confirm with author`
   - **Evidence:**
     ```csharp
     foreach (var (key, value) in encodedValues)
     {
         template = template.Replace($"{{{{{key}}}}}", value, StringComparison.Ordinal);
     }
     ```
   - **Principle violated:** AI Failure Mode #13 & DRY. Sequential replacement lets an earlier scalar field containing `{{AttendanceRows}}` get expanded by a later raw HTML fragment, whereas `RenderEmailAsync` already uses single-pass `TokenPatternRegex()`.
   - **Suggested fix:** Unify `RenderReportAsync` onto the single-pass `TokenPatternRegex()` + `(scalarValues, rawFragments)` pipeline from `RenderEmailAsync`.

9. **`OrganizationService.LoadDetailCoreAsync` omits `DefaultCoverCatalog` fallback applied on sibling endpoints** — `backend/src/TeamUp.Application/Services/OrganizationService.cs:144` *(vs `:96-98`, `ProjectService.cs:72`, `EventService.cs:789`)*
   - **Classification:** `Behavior change — confirm with author`
   - **Evidence:** List endpoint resolves `i.CoverImageUrl ?? DefaultCoverCatalog.UrlFor($"org:{i.OrganizationId}")` (`:97`), whereas detail returns raw `organization.CoverImageUrl` (`:144`).
   - **Principle violated:** AI Failure Mode #13 (Semantic inconsistency).
   - **Suggested fix:** Confirm whether `OrganizationDetailDto.CoverImageUrl` is intentionally `null` for edit forms (if so, document with an intent comment); otherwise apply the same fallback as `ProjectDetailDto` and `EventDto`.

---

### Frontend (`frontend/src`)

10. **`BadgesGallery` rejects newly allowed asset/stored-image URLs and bypasses `resolveMediaUrl`** — `frontend/src/features/gamification/BadgesGallery.tsx:123-130`
    - **Classification:** `Behavior change — confirm with author`
    - **Evidence:**
      ```tsx
      {badge.iconUrl && badge.iconUrl.startsWith("http") ? (
        <img
          src={badge.iconUrl}
          alt={badge.name}
          loading="lazy"
          decoding="async"
          className="size-7 rounded-lg object-contain"
        />
      ) : ...
      ```
    - **Principle violated:** AI Failure Mode #13 (partial migration after `AdminGamificationGovernance` allowed `/badges/...` and `/api/v1/images/{guid}`), #5 (duplication), and missing `referrerPolicy="no-referrer"`.
    - **Suggested fix:** Use `resolveMediaUrl(badge.iconUrl)` from `@/lib/urlValidation` and add `referrerPolicy="no-referrer"`.

11. **`ExplorePage` silently swallows API errors into misleading empty states** — `frontend/src/features/explore/ExplorePage.tsx:112-181`
    - **Classification:** `Behavior change — confirm with author`
    - **Evidence:**
      ```tsx
      {recruiting.isLoading ? (
        ...
      ) : (recruiting.data?.items.length ?? 0) === 0 ? (
        <EmptyState title="No open recruitment right now" ... />
      ) : (
        ...
      ```
      *(Repeated across all 3 rails: `recruiting`, `events`, `projects`.)*
    - **Principle violated:** AI Failure Mode #1 (Error suppression) & #10 (Inconsistency with sibling directories).
    - **Suggested fix:** Check `query.isError` before the empty-state check and render `<ErrorBanner error={query.error} onRetry={() => void query.refetch()} />`.

12. **`NotificationBell` collides on shared cache key with mismatched query params, bypasses `notificationsApi`, and triggers unhandled `.mutateAsync()` promises** — `frontend/src/components/layout/NotificationBell.tsx:54-92, 185-187, 219-221`
    - **Classification:** Delegating to `notificationsApi.markAsRead`/`markAllAsRead` & `NotificationDto` (**`Pure refactor`**); adding `take=15` and switching unhandled `void *.mutateAsync()` to `.mutate()` (**`Behavior change — confirm with author`**)
    - **Evidence:** Shares `queryKeys.notifications.inbox(false)` without `take: 15`, re-declares `NavbarNotificationItem` (`:29-37`), uses raw `apiClient` alongside `notificationsApi`, and fires unawaited `void markAllReadMutation.mutateAsync()` / `void markReadMutation.mutateAsync(...)`.
    - **Principle violated:** AI Failure Mode #13, #5, #1, & SOLID — DIP.
    - **Suggested fix:** Reuse `notificationsApi` for all 4 operations, drop `NavbarNotificationItem` and raw `apiClient`/`API_ENDPOINTS`, and replace `void *.mutateAsync(...)` with `*.mutate(...)`.

13. **`resolveMediaUrl` `catch` fallback double-prefixes relative `/api` bases and omits scheme check** — `frontend/src/lib/urlValidation.ts:80-89`
    - **Classification:** `Behavior change — confirm with author`
    - **Evidence:**
      ```ts
      try {
        const origin = new URL(trimmedBase).origin;
        return `${origin}${trimmed}`;
      } catch {
        return trimmed.startsWith("/api/") ? `${trimmedBase.replace(/\/$/, "")}${trimmed}` : trimmed;
      }
      ```
    - **Principle violated:** AI Failure Mode #1 & #13. A relative `VITE_API_BASE_URL="/api"` throws and returns `"/api/api/v1/images/..."`, while `"javascript:..."` yields `new URL(...).origin === "null"` without throwing.
    - **Suggested fix:** Verify `parsedBase.protocol === "http:" || "https:"` before returning `${parsedBase.origin}${trimmed}`, and avoid double-prefixing relative `/api` bases in `catch`.

14. **`OrganizationRequestModal` imports raw `"zod"`, bypassing CSP `jitless` config (`zodCspConfig`)** — `frontend/src/features/organizations/components/OrganizationRequestModal.tsx:9, 36`
    - **Classification:** `Behavior change — confirm with author`
    - **Evidence:** `import { z } from "zod";` and `scope: z.nativeEnum(ORGANIZATION_SCOPES),`.
    - **Principle violated:** AI Failure Mode #10 (all 7 sibling Zod modules import `@/lib/zodCspConfig` to disable `new Function()` under `script-src 'self'` CSP) & AI Failure Mode #6 (deprecated Zod v4 `z.nativeEnum`).
    - **Suggested fix:** Move `proposalFormSchema` into `features/organizations/schemas/organizationSchemas.ts` using `@/lib/zodCspConfig` and `z.enum(ORGANIZATION_SCOPE_VALUES)`.

15. **`ProjectDetailPage` checks wrong query for Overview errors, suppresses creator 403s, and risks duplicate rows on Applications tab** — `frontend/src/features/projects/ProjectDetailPage.tsx:45-47, 354-355, 504, 561-584`
    - **Classification:** Guarding `:561` with `isCreator && ...` (**`Pure refactor`**); showing `myApplicationQuery.isError` at `:504` and removing dead `isApiForbidden` / `applicationForbidden` suppression (**`Behavior change — confirm with author`**)
    - **Evidence:** At `:504` (inside `!isMember`, where `applicationsQuery` is disabled), the UI checks `{applicationsQuery.isError && !applicationForbidden ...}`. At `:561-563`, `(applicationsQuery.data ?? []).filter(isCreator || ...)` runs unconditionally before `{!isCreator && myApplication && ...}` (`:576`).
    - **Principle violated:** AI Failure Mode #11, #1, & #13.
    - **Suggested fix:** Gate the creator list with `isCreator && ...`, wire `myApplicationQuery.isError` on Overview, and delete unused `isApiForbidden` / `applicationForbidden`.

16. **`UniversityLogo` accepts `/api/v1/images/{guid}` without resolving via `resolveMediaUrl`** — `frontend/src/components/ui/UniversityLogo.tsx:7, 69`
    - **Classification:** `Behavior change — confirm with author`
    - **Evidence:** `const validPrimaryUrl = logoUrl && isSafeAssetOrHttpUrl(logoUrl) ? logoUrl.trim() : null;`
    - **Principle violated:** AI Failure Mode #13 (partial migration vs `Avatar`, `CoverImage`, `OrganizationMark`). Relative `/api/v1/images/{guid}` fails to prepend `VITE_API_BASE_URL` on split-origin deployments.
    - **Suggested fix:** Use `resolveMediaUrl(logoUrl)` instead of raw `isSafeAssetOrHttpUrl`.

---

## Important findings

### Backend (`backend/src`)

1. **Oversized, high-complexity functions (>50 lines / cyclo >10 / nesting ≥5)** — `Pure refactor` (Section A F1/F2/F3, KISS, AI #8):
   - `SecurityPipelineExtensions.cs:20-250` (`AddTeamUpSecurityServices`, 231 lines) & `:252-311` (`UseTeamUpSecurityPipeline`, 60 lines).
   - `DigestDispatchService.cs:180-350` (171 lines) & `:29-178` (150 lines).
   - `ProjectRecommendationService.cs:134-279` (146 lines), `:13-132` (120 lines), & `:281-378` (98 lines).
   - `OrganizationService.cs:1078-1214` (137 lines), `:550-634` (85 lines), `:781-863` (83 lines), `:38-101` (64 lines), `:308-366` (59 lines), `:941-999` (59 lines).
   - `AuthService.cs:118-199` (82 lines), `:43-116` (74 lines), `:201-257` (57 lines); 8 constructor dependencies.
   - `DatabaseStartupExtensions.cs:16-85` (70 lines, nesting depth 5).
   - `ProfileService.cs:109-175` (67 lines), `:177-240` (64 lines), `:52-107` (56 lines).
   - `SecurityStartupValidator.cs:56-117` (`Validate`, 62 lines, cyclo ~17).
   - `EventService.cs:616-676` (61 lines mixing auth/DB/HTML/templating), `:695-749` (55 lines), `:48-97` (50 lines).
   - `OpenApiAndCachingExtensions.cs:16-72` (57 lines).
   - `ProjectService.cs:212-261` (50 lines).
   - `SafeUrlValidator.cs:85-130` (`IsRestrictedIpAddress`, cyclo ~26). Split IPv4 vs IPv6 checks.
2. **Parameter explosion (>4 params without query/request DTOs)** — `Pure refactor` (Section A F5, AI #9):
   - `EventsController.cs:21-30, 59-68` (9 params); `EventService.cs:26-57` (8–9 params) & `:189,253,277,463,527,580` (repeated `actorStudentId, actorRole`).
   - `OrganizationsController.cs:25-33, 78-86` (8 params); `OrganizationService.cs:38-45, 308, 461, 740, 761` (6–7 params).
   - `AdminController.cs:90-96, 194-199` (5–6 params); `ProjectsController.cs:19-25` (6 params); `ProjectService.cs:16,128,154,212` (5 params); `JwtTokenService.cs:19-25` (`GenerateAccessToken`, 6 params).
3. **Boolean flag arguments & misleading flag naming** — `Pure refactor` (Section A F6 & N2):
   - `OrganizationService.cs:111,123-124` (`includeMemberEmails` also hides private events) & `:514` (`GetHierarchyAsync(..., bool includeEmails = true, ...)`).
   - `EventService.cs:56` (`bool publicOnly`) & `:771` (`bool exposeMeetingUrl`).
4. **Banned generic identifiers (`result`, `data`, `item`, `value`)** — `Pure refactor` (Section A N6, AI #7):
   - `var result = await ...; return OkResponse(result);` across ~80+ controller actions; `data` (`ImageService.cs:46`), `result` (`AuthService.cs:146`, `ProjectRecommendationService.cs:142`, `AppDbContext.cs:61`, `DatabaseTransactionExtensions.cs:34`), `item` (`ProjectService.cs:133,142`, `ProfileService.cs:425`, `DigestDispatchService.cs:397`), `value` (`OrganizationService.cs:60`, `SafeUrlValidator.cs:28,55`, `SecurityStartupValidator.cs:128`, `DatabaseTransactionExtensions.cs:41`).
5. **Misleading `MaxProfilePhotoBytes` name & unused CQS return** — `ImageFileValidator.cs:9` (`Pure refactor`: rename to `MaxImageBytes`); `ImageService.cs:19,89` (`Behavior change — confirm with author`: `EnsureDefaultCoversAsync` returns an unused `int`).
6. **`EventsController.cs` style inconsistency** — `EventsController.cs:20,39,47,58,76,81-82,...` (`Pure refactor`, Section B Fmt5): uses verbose `[ProducesResponseType]` / `CreatedAtAction` instead of `[ProducesOk<T>]` / `[ProducesCreated<T>]` / `CreatedResponse` used by all sibling controllers.
7. **Packed multi-statement lines** — `ProjectService.cs:130,134-135,141,144,...` (`Pure refactor`, Section B Fmt1/Fmt4).
8. **Inconsistent SQLite provider detection** — `DatabaseStartupExtensions.cs:71` (`ProviderName == ...` vs `IsSqlite()` at `:26`) & `AppDbContext.cs:139` (`Pure refactor`, Section B Fmt5).
9. **SOLID SRP God classes** — `Pure refactor`: `OrganizationService.cs` (1,256 lines, 6 areas), `EventService.cs` (796 lines, 4 areas), `DigestDispatchService.cs` (embeds ~95 lines of HTML markup at `:364-459`), `AdminController.cs` (7 dependencies + controller-level auditing at `:157-163, :181-187`), `ProjectService.cs`, `ProfileService.cs`.
10. **SOLID DIP violations** — `Pure refactor`: `ImageService.cs:15-25` places `IImageService` + DTOs in `TeamUp.Application.Services` next to concrete `ImageService`; `EventService.cs:751-761` & `ProjectService.cs:219-223` embed raw SQL Server `WITH (UPDLOCK, ROWLOCK)` queries inside Application services.
11. **DRY knowledge duplication** — `Pure refactor` (Section D DRY, AI #5):
    - 19-line stream-read + magic-byte validation (`LocalProfilePhotoStorage.cs:19-37` vs `ImageService.cs:29-48`).
    - 24 raw OutputCache tag strings (`"catalog-universities"`, `"catalog-skills"`, `"catalog-org-categories"`, `"catalog-badges"`) across 6 controllers.
    - ~65-line recommendation loop & 4-tuple signature repeated 5× in `ProjectRecommendationService.cs`.
    - `AffiliationFormatting` fallback strings duplicated across 6 queries (`ProjectRecommendationService`, `DigestDispatchService`, `ProfileService`).
    - Level formula duplicated in `ProfileService.cs:215-222` vs `IGamificationService.CalculateLevelMetrics`.
    - `"https://teamup-ieee.vercel.app"` repeated 6× across `DigestDispatchService`, `SecurityOptions`, `GamificationDomainEventHandlers`, `AdminReportPdfService`, `BrandAssets`.
    - 10-role admin list duplicated in `OrganizationService.cs:396-405` vs `OrganizationRoles.AdministrativeRoles`.
    - `StudentSummaryDto` mapping (3×) & session revocation (3×) in `AuthService.cs`; 18-field `MembershipApplicationDto` projection (3×) in `OrganizationService.cs`; `NormalizeOptional` in `OrganizationService` & `EventService`; rate-limit partitioning (5×) & HTTP 401/403/429 payloads (3×) in `SecurityPipelineExtensions.cs`; digest template maps in `DigestDispatchService.cs`.
12. **YAGNI optional DI param** — `DigestDispatchService.cs:22` (`Pure refactor`): `gamificationOptions = null` has zero non-DI callers.
13. **Unused imports** — `FacultiesController.cs:5` (`using TeamUp.Api.Extensions;`) & `SecurityPipelineExtensions.cs:11` (`using TeamUp.Api.Middleware;`) (`Pure refactor`, AI #11).
14. **Dead public method** — `DefaultCoverCatalog.cs:26` (`IsDefaultCover`, zero callers) (`Behavior change — confirm with author`, AI #11).
15. **Impossible-case defensive `Math.Min`** — `ImageService.cs:32` after line 29 already threw (`Pure refactor`, AI #2).
16. **Non-transactional multi-`SaveChangesAsync` soft delete** — `ProfileService.cs:293-303` (`Behavior change — confirm with author`).
17. **Incomplete SQL Server TLS connection-string parser** — `SecurityStartupValidator.cs:119-144` misses `"Trust Server Certificate"` synonym and quoted `;` (`Behavior change — confirm with author`). Use `DbConnectionStringBuilder`.
18. **Hardcoded `"admin@teamup.local"` email fallback & bare `catch` in `AppDbContextFactory`** — `AdminController.cs:226-229` & `AppDbContextFactory.cs:35-38` (`Behavior change — confirm with author`).

### Frontend (`frontend/src`)

19. **`OrganizationMark` stale `imageError` boolean & dead `?? ""` guard** — `frontend/src/features/organizations/components/OrganizationMark.tsx:20-22, 34-36` (Narrowing to `resolvedLogoUrl`: **`Pure refactor`**; switching `imageError` to `failedUrl`: **`Behavior change — confirm with author`**).
20. **`ExplorePage` 3× duplicated 25-line `<section>` rail, `data!` assertions, & `explore: true` query-key pollution** — `frontend/src/features/explore/ExplorePage.tsx:87-90, 106-182` (**`Pure refactor`**; confirming whether projects rail should filter `status: Recruiting`: **`Behavior change — confirm with author`**).
21. **`OrganizationRequestModal` duplicates URL schemas, `SCOPE_OPTIONS`, & default `categorySuggestions`** — `frontend/src/features/organizations/components/OrganizationRequestModal.tsx:25-57, 96-97, 180` (`Pure refactor`).
22. **Duplicate `@/components/ui` import (`OrganizationsDirectoryPage.tsx:20-36`) & 6× duplicated `aspect-[2/1]` / bleed classes (`OrganizationsDirectoryPage`, `EventsDirectoryPage`, `ProjectsDiscoveryPage`, `ExplorePage`)** — (`Pure refactor`).
23. **`CoverImage` missing `decoding="async"` and `referrerPolicy="no-referrer"`** — `frontend/src/components/ui/CoverImage.tsx:23-29` (`Behavior change — confirm with author`).
24. **Positional optional params before `AbortSignal`, boolean flag args (`unreadOnly`, `enabled`), & unused `useMyEventRegistrations` pagination params** — `notifications.api.ts:21-25`, `events.api.ts:140-144`, `useEventHooks.ts:38-45` (`Pure refactor`).
25. **3× duplicated unread-count query + fallback (`NotificationsInboxPage:68-71,109`, `StudentDashboardPage:72-76,87`, `NotificationBell:46-52,131-134`) & redundant `typeof ... === "number"`** — (`Pure refactor`).
26. **Dead/unused `API_ENDPOINTS.images`** — `frontend/src/api/endpoints.ts:151-154` (`Pure refactor`, AI #11 / YAGNI).
27. **`ApplicationRow` ISP violation (`onStatus={() => undefined}`) & `isLead` disinformation naming** — `frontend/src/features/projects/ProjectDetailPage.tsx:567, 577-583, 692-704` (`Pure refactor`).
28. **`AdminGamificationGovernance` duplicated badge validation/payload (`:61-99`) & stale `"Icon URL (https://...)"` label (`:461`)** — Extracting helper (**`Pure refactor`**); aligning empty-icon check & label (**`Behavior change — confirm with author`**).
29. **`ScoreLedgerTable` OCP `switch` dispatch** — `frontend/src/features/gamification/ScoreLedgerTable.tsx:28-43` (`Pure refactor` to convert to lookup map).
30. **Pre-existing god components expanded in diff** — `ProjectDetailPage.tsx` (638 lines), `AdminGamificationGovernance.tsx` (500 lines), `EventDetailPage.tsx` (412 lines), `OrganizationsDirectoryPage.tsx` (391 lines), `StudentDashboardPage.tsx` (354 lines), `EventsDirectoryPage.tsx` (294 lines), `Navbar.tsx` (1,194 lines) (`Pure refactor`).
31. **`NotificationsInboxPage` `All Notifications ({notifications.length})` displays capped active-tab length** — `frontend/src/features/notifications/NotificationsInboxPage.tsx:156, 165` (`Behavior change — confirm with author`).
32. **Inconsistent `queryKeys` user-scoping (`myApplication` vs `myRegistration`) & redundant prefix invalidations** — `queryKeys.ts:58,80-82`, `ProjectDetailPage.tsx:138-148`, `useEventHooks.ts:85-97`, `useOrganizationHooks.ts:101-118` (`Pure refactor`).

---

## Nits

### Backend (`backend/src`) — all `Pure refactor`
1. **Redundant fully-qualified namespaces:** `ProjectService.cs:39`, `DigestDispatchService.cs:22,238`, `OrganizationService.cs:97`, `EventService.cs:24,344,789`, `SecurityPipelineExtensions.cs:79,158`, `AppDbContext.cs:123,125`, `SqliteDevSchemaPatcher.cs:92,103,114,175,200,233`.
2. **Import order & `sealed` consistency:** Move `using System.Text;` before `Microsoft.*` in `OrganizationsController.cs:7`; seal remaining concrete controllers consistently if desired.
3. **`ImagesController` OpenAPI metadata & magic constant:** Add `[ProducesNotFound]` on `GetImage` (`:35-46`) and hoist `3 * 1024 * 1024` constant (`:22-23`).
4. **Formatting & micro-cleanups:** Blank line after `_deferDomainEvents` (`AppDbContext.cs:15-16`); reuse `now` in `EventService.cs:732`; pass `StringComparison.OrdinalIgnoreCase` in `SecurityPipelineExtensions.cs:273`; pass `student` in `AuthService.cs:72`; hoist `JwtSecurityTokenHandler` in `JwtTokenService.cs:64`; parameterize/validate table name in `SqliteDevSchemaPatcher.cs:106`.

### Frontend (`frontend/src`) — all `Pure refactor`
1. **Unsorted import (`EventsDirectoryPage.tsx:25`):** Move `CoverImage` after `Button`.
2. **Stale JSDoc headers (`Avatar.tsx:3`, `OrganizationMark.tsx:3`, `urlValidation.ts:65-68`):** Update to reflect `resolveMediaUrl` / `/api/v1/images/{id}`.
3. **Redundant defensive checks:** `unwrapApiResponse(res) ?? null` (`projects.api.ts:180`), `?? registrations.length` (`StudentDashboardPage.tsx:160`), `.or(z.literal(""))` after length-0 refine (`urlValidation.ts:106,115`).
4. **Ambient `React.ReactNode` (`ExplorePage.tsx:19`):** Import `type { ReactNode } from "react"`.
5. **Magic numbers (`OrganizationsDirectoryPage.tsx:78`, `useOrganizationHooks.ts:255`):** Extract `MAX_CATEGORY_CHIPS = 10` and `CATEGORIES_STALE_TIME_MS = 5 * 60_000`.
6. **Duplicated 20-line image wrapper (`UniversityLogo.tsx:72-93, 98-118`):** Unify via single render helper/props derivation.
7. **Redundant `useEffect` dependency (`EventDetailPage.tsx:101`):** Drop duplicate `myRegQuery.data` alongside derived `backendMyReg`.
8. **Generic loop identifiers & restating JSX comments:** Domain variable names and fewer `{/* Header */}` comments across `OrganizationsDirectoryPage`, `ProjectDetailPage`, `NotificationsInboxPage`, `StudentDashboardPage`.
9. **Mixed LF/CRLF line endings:** Flagged by Git on `NotificationBell.tsx` and `organizationSchemas.ts`.
10. **Duplicate `from "@/api"` import (`ProjectsDiscoveryPage.tsx:5-6`):** Merge adjacent imports.

---

## What's good
1. **74-line modular `Program.cs` & ordered security pipeline (`TeamUp.Api/Program.cs:1-74`):** Extracted cleanly into focused extension files with crisp security middleware ordering (`ForwardedHeaders` → `ExceptionHandler` → `Cors` → `Authentication` → `Authorization` → `OutputCache` → `RateLimiter`).
2. **High-signal "why" comments without comment pollution:** Zero `Step N` scaffolding and zero commented-out code across both stacks; inline comments document invariants, threat mitigations (`OpenApiAndCachingExtensions.cs:59,83-84`, `DatabaseTransactionExtensions.cs:53-54`, `EventDetailPage.tsx:76-78`, `ProjectDetailPage.tsx:102-113`), and bundle-splitting constraints (`features/auth/index.ts:37-40`).
3. **Zero-allocation & cryptographic backend hardening:** `Span`-based magic-byte checks (`ImageFileValidator.cs`), `[GeneratedRegex]` single-pass email rendering (`TemplateRenderer.cs`) and UUID asset validation (`SafeUrlValidator.cs`), deterministic FNV-1a cover mapping (`DefaultCoverCatalog.cs`), constant-time login padding (`AuthService.cs`), and post-commit domain events (`AppDbContext.cs`).
4. **Self-resetting React image error state (`CoverImage.tsx:17-19` & `uiKit.test.tsx:207-222`):** Uses `failedUrl === src` without an extra `useEffect` render cycle.
5. **Targeted API over-fetching reduction:** Lazy org lookup in `EventDetailPage`, `/applications/mine` and `/registrations/mine` endpoints avoiding noisy BOLA 403s, `unread-count` polling in `NotificationBell`, and clean OCP map extension in `StatusPill.tsx:56,71`.

---

## Self-check coverage
- [x] Walked Section A (naming & functions) — Backend & Frontend
- [x] Walked Section B (comments & formatting) — Backend & Frontend
- [x] Walked Section C (SOLID) — Backend & Frontend
- [x] Walked Section D (DRY, KISS, YAGNI) — Backend & Frontend
- [x] Walked Section E (AI failure modes) — Backend & Frontend

---

## Remediation Verification (2026-11-26)

All **16 Critical findings** (9 Backend, 7 Frontend) plus targeted Important and Nit cleanups were applied using TDD (9 new failing tests added first and verified red before production fixes), and `docs/api/openapi.json` was regenerated to reflect `[ProducesNotFound]` on `ImagesController.GetImage`.

### Critical Findings Resolved
- **Backend #1–#9:**
  1. Centralized `CatalogOutputCacheTags` and matched both singular & plural entities in `AdminController.RestoreEntity` (`CatalogOutputCacheTags.ForRestoredEntityType`), plus replaced `"admin@teamup.local"` fallback with `UnauthorizedException`.
  2. Replaced hardcoded `85%` / `["React", "TypeScript"]` fallback data in `DigestDispatchService` with real `ProjectRequiredSkills` queries and truthful `0%` match preview (without sending fabricated emails or counting them in `SentCount`).
  3. Unified `/health` and `/health/ready` onto shared `readyOptions` in `HealthCheckExtensions`, propagated `OperationCanceledException`, and logged DB readiness failures.
  4. Unified `SeedImages` lookup on `AppContext.BaseDirectory`, deduplicated 2 MB magic-byte upload reading via `ImageFileValidator.ReadAndValidateImageAsync` (`MaxImageBytes`), removed dead `IsDefaultCover`, fixed disk-fallback comments, narrowed startup cover catch to non-cancellation exceptions, and added `[ProducesNotFound]` + `MaxMultipartRequestBytes` in `ImagesController`.
  5. Prevented `_deferDomainEvents` flag leaks in `DatabaseTransactionExtensions.ExecuteInTransactionAsync` when `BeginTransactionAsync` throws and forwarded `CancellationToken` to `IExecutionStrategy.ExecuteAsync`.
  6. Narrowed `IsSqlServerReachable` catch to `SqlException` (with warning log on SQLite fallback) and `AppDbContextFactory` catch to `IOException | JsonException`, deduplicating SQLite `AddDbContext` registration.
  7. Passed `CancellationToken.None` to restore `PRAGMA foreign_keys = ON` in `SqliteDevSchemaPatcher`, expanded positional `SELECT *` to explicit 6 columns for `StoredImage`, and allowlist-validated table names.
  8. Converted `TemplateRenderer.RenderReportAsync` to single-pass `TokenPatternRegex()` replacement to eliminate second-order placeholder injection.
  9. Applied `DefaultCoverCatalog.UrlFor($"org:{id}")` fallback in `OrganizationService.LoadDetailCoreAsync` and unified `EnsureNotLastAdminAsync` onto `OrganizationRoles.AdministrativeRoleNames`.
- **Frontend #10–#16:**
  10. Updated `BadgesGallery` to resolve relative `/badges/...` and `/api/v1/images/{guid}` icons via `resolveMediaUrl` with `referrerPolicy="no-referrer"`.
  11. Extracted generic `DiscoveryRail<T>` in `ExplorePage` rendering `ErrorBanner` (with retry) on query failures instead of empty states, dropping `!` assertions and `explore: true` key pollution.
  12. Created shared [useUnreadNotificationCount.ts](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/src/features/notifications/useUnreadNotificationCount.ts) and refactored `NotificationBell` to use `notificationsApi` + `.mutate()` with `onError` toast handling (removing duplicate `NavbarNotificationItem`).
  13. Hardened `resolveMediaUrl` scheme validation (`http:`/`https:`), trailing-slash normalization, and relative `/api` non-double-prefixing.
  14. Moved `proposalFormSchema` and shared `ORGANIZATION_SCOPE_OPTIONS` into `organizationSchemas.ts` (`@/lib/zodCspConfig` + `z.enum`), removing raw `"zod"` imports from `OrganizationRequestModal`.
  15. Fixed `ProjectDetailPage` to check `myApplicationQuery.isError` inside Overview `!isMember`, gated the Applications creator list with `isCreator && ...`, removed dead `isApiForbidden`/`applicationForbidden`, and cleaned up `ApplicationRow` (`canReview`, optional `onStatus`).
  16. Updated `UniversityLogo` to resolve URLs via `resolveMediaUrl`, deduplicated image wrapper markup, updated `OrganizationMark` (`failedUrl` reset on prop change), hardened `CoverImage` (`aspect-[2/1]`, `decoding="async"`, `referrerPolicy="no-referrer"`), extracted `buildBadgePayload` in `AdminGamificationGovernance`, converted `ScoreLedgerTable` to a lookup map, and removed unused `API_ENDPOINTS.images`.

### Final Post-Edit Verification Results
- **Backend Solution (`TeamUp.sln`, `-c Release`, `.NET 11`):** Build succeeded (`0 Warning(s), 0 Error(s)`)
  - `TeamUp.UnitTests`: **147/147 passed** (includes 3 new TDD tests in `StrideSecurityUnitTests` and `PersistenceHardeningTests`)
  - `TeamUp.IntegrationTests`: **111/111 passed** (includes 2 new TDD tests in `CachingAndConcurrencyTests` and `DefaultCoverTests`, plus `OpenApiAndPostmanTests`)
- **Frontend (`frontend`):**
  - `npm run typecheck` (`tsc --noEmit`): **0 errors**
  - `npm run test` (Vitest): **12/12 files passed, 99/99 tests passed** (includes 4 new TDD tests across `securityAndLib.test.ts`, `explorePage.test.tsx`, `uiKit.test.tsx`)
  - `npm run build` (`vite build`): **Succeeded** (`3.02s`)
- **Total Combined Automated Tests Passing:** **357/357**
