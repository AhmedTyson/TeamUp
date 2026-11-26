# TeamUp Round 5 Verification & Polish Report

**Date:** 2026-11-26  
**Target Repository:** `TeamUp` (`HEAD` at `e9a70b3` + local SQL Server Express configuration)  
**Verified Archive:** `C:\Users\ahmed\Downloads\TeamUp_Fixes_Round5-04a69bff.zip` (13 commits, Rounds 1–5)

---

## 1. Verification Verdict Summary

Every bug and gap claimed across all 13 commits (18+ fixes) in `TeamUp_Fixes_Round5-04a69bff.zip` was verified directly against the current `TeamUp` codebase (`e9a70b3`) and confirmed to be **real and accurately remediated** by the patch.

- **Zero physical cascades:** All EF Core relationships and migrations (`20261125064828_AddStoredImages`, `20261125121701_AddCoversAndSystemImages`) strictly enforce `DeleteBehavior.Restrict` / `ReferentialAction.Restrict`.
- **Single local merge conflict:** `backend/src/TeamUp.Infrastructure/DependencyInjection.cs` differed locally due to our `SqlConnection`-based `IsSqlServerReachable` fix for `.\SQLEXPRESS`. Resolved cleanly by applying the patch and preserving `IsSqlServerReachable` alongside the new `GamificationOptions` and `NotificationOptions` bindings.

---

## 2. Commit-by-Commit Verification Matrix

| # | Commit Subject | Verified Bug/Gap in `e9a70b3` | Fix & Test Verdict |
|---|---|---|---|
| 01 | `fix(attendance): enforce session check-in window and claw back XP on revoke` | `RecordAttendanceCoreAsync` lacked session time-window validation; `RevokeAttendanceAsync` deleted attendance without reversing `ScoreTransaction` XP. | **Real & Solved.** Adds configurable `EarlyCheckInMinutes` (30) / `LateCheckInMinutes` (15) and append-only `AttendanceRevocation` compensating row + regression tests. |
| 02 | `fix(recommendations): give optional-only projects the full skills bucket in match scoring` | `ComputeMatchScore` hardcoded `20.0` weight for optional skills even when `mandatory.Count == 0`, capping optional-only projects at 40/100. | **Real & Solved.** Assigns `80.0` weight when `mandatory.Count == 0`, symmetric with mandatory-only projects + unit test. |
| 03 | `fix(orgs): stop leaking member emails to any authenticated user` | `GetOrganizationById`, `GetMembers`, and `GetHierarchy` returned `x.Student.Email` to any authenticated user regardless of `ShowEmailInDirectory`. | **Real & Solved.** Scopes email visibility to org/platform admins, self, or `ShowEmailInDirectory == true` + integration test. |
| 04 | `perf(digests): batch recommendation fan-out to a constant number of queries` | `DigestDispatchService` invoked `GetRecommendedProjectsForStudentAsync` inside a per-recipient loop (~4 queries per recipient). | **Real & Solved.** Adds `GetRecommendedProjectsForStudentsAsync` batching queries to constant ~5 queries + unit parity test. |
| 05 | `perf(ui): lazy-load list images (logos, avatars, badge icons) below the fold` | `Avatar`, `UniversityLogo`, `BadgesGallery`, and `OrganizationMark` loaded images eagerly in directories. | **Real & Solved.** Adds `loading="lazy"` and `decoding="async"`. |
| 06 | `perf(bundle): stop re-exporting auth pages from the features/auth barrel` | `features/auth/index.ts` re-exported 4 auth pages statically, defeating `React.lazy` in `authRoutes.tsx`. | **Real & Solved.** Removes page re-exports from barrel and updates `authPagesAndShell.test.tsx`. |
| 07 | `perf(events): stop fetching 100 organizations on every event detail view` | `EventDetailPage.tsx` unconditionally called `useOrganizations({ page: 1, pageSize: 100 })` on every view. | **Real & Solved.** Gates fetch with `canManage && editEventOpen`. |
| 08 | `fix(projects): let applicants see and withdraw their own application` | `ProjectDetailPage` fired creator-only `GET /projects/{id}/applications` for all users, causing 403 and hiding pending status & Withdraw button. | **Real & Solved.** Adds `GET /api/v1/projects/{id}/applications/mine` + `ProjectDetailPage` gating + integration test. |
| 09 | `perf(notifications): cap inbox fetch at 100 latest and mark-all-read in one UPDATE` | `NotificationService.GetNotificationsAsync` was unbounded; `MarkAllAsReadAsync` loaded all rows into memory before saving. | **Real & Solved.** Caps query with `.Take(...)` and uses `ExecuteUpdateAsync`. |
| 10 | `feat(api): paginate remaining unbounded GET lists; notifications default to latest 15` | `my-registrations`, `notifications`, `soft-deleted`, `my-applications`, `requests/my`, and `participants` were unbounded or aggregated in memory. | **Real & Solved.** Paginates `my-registrations`, adds `/registrations/mine` and `/notifications/unread-count`, aggregates org participants in SQL, bounds histories + 5 integration tests. |
| 11 | `fix(business-rules): close registration at event end; never orphan an org's last admin` | `RegisterAsync` allowed registering for past events; `RemoveMemberAsync`/`UpdateMemberRoleAsync` could remove/demote an org's last admin. | **Real & Solved.** Adds `EndDate <= DateTime.UtcNow` check and `EnsureNotLastAdminAsync` guard + 4 integration tests. |
| 12 | `Round 4 flexibility: configurable limits, free-text org categories, DB-stored images API, Explore feed` | Hardcoded XP/inbox/admin caps, rigid frontend category `<Select>`, disk-only uploads, and siloed discovery directories. | **Real & Solved.** Adds `GamificationOptions`, `NotificationOptions`, `AdminOptions.SoftDeletedPreviewLimit`, `GET /organizations/public/categories`, `StoredImage` + `/api/v1/images`, `/explore` page + tests. |
| 13 | `Round 5 generated covers: 10 on-brand generated artworks seeded into StoredImages` | Discovery cards lacked covers and default media fallback. | **Real & Solved.** Adds `cover-01..10.png`, `DefaultCoverCatalog`, `EnsureDefaultCoversAsync`, `CoverImage.tsx` + 3 integration tests. |

---

## 3. Five Polish Defects Caught During Verification & Remediation Plan

Line-by-line inspection of the Round 5 patch surfaced 5 subtle defects that we remediate cleanly in Stage 1 and Stage 3:

1. **Cross-Origin Image Serving & Direct-Tab CSP (`Program.cs`, `ImagesController.cs`):**
   - `Program.cs` only applied `img-src 'self'` CSP to `/images` and `/logos` static prefixes, leaving `/api/v1/images/{id}` with `default-src 'none'`. Additionally, `ImagesController.GetImage` lacked `Cross-Origin-Resource-Policy: cross-origin` (unlike `UseStaticFiles`), which blocks cross-origin `<img>` embedding when frontend and API run on separate origins.
   - **Fix (Stage 1):** Add `/api/v1/images` to `isStaticAsset` in `Program.cs` and set `Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin"` in `ImagesController.GetImage` (with TDD integration test).
2. **Stale Error State in `CoverImage.tsx`:**
   - `const [failed, setFailed] = useState(false)` stayed `true` forever if the `url` prop changed on a reused card component.
   - **Fix (Stage 1):** Track `failedUrl` (`const failed = failedUrl === src;`) so changing `src` resets error state cleanly (with Vitest coverage).
3. **Unsafe Path Concatenation for `/api/` in `frontend/src/lib/urlValidation.ts`:**
   - `resolveMediaUrl` stripped trailing slashes from `trimmedBase` and concatenated `trimmed` (`/api/v1/images/...`). If `VITE_API_BASE_URL` includes a path prefix (e.g., `https://api.example.com/api`), it produced `https://api.example.com/api/api/v1/images/...`.
   - **Fix (Stage 1):** Extract `new URL(trimmedBase).origin` safely inside `try/catch` (matching `/images/people/student-*` handling) with Vitest coverage.
4. **`isLead` vs `isCreator` Render Mismatch in `ProjectDetailPage.tsx`:**
   - The applications query is gated on `isCreator`, but the applications section UI checks `isLead`. A non-creator member holding a `"Lead"` role has `isLead === true` and `isCreator === false`, causing an empty section where neither `applicationsQuery` nor `myApplicationQuery` renders.
   - **Fix (Stage 1):** Use `isCreator` consistently for the applications queue visibility vs `myApplication`.
5. **Unbounded `nvarchar(max)` on `Event.CoverImageUrl` & `Project.CoverImageUrl`:**
   - Round 5 left `CoverImageUrl` without `HasMaxLength(500)` in `EventConfigs.cs` / `ProjectConfigs.cs` (unlike `Organization.CoverImageUrl`).
   - **Fix (Deferred cleanly to Apply Phase 3):** Recorded for EF Core 11 persistence hardening so we do not mutate Round 5's existing migration checksum prior to upgrading to EF Core 11.
