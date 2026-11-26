# Phase 07 Audit — Frontend Contracts, Performance & UX (`frontend/`)

**Date:** 2026-11-26  
**Scope:** `frontend/src/` (181 `.ts/.tsx` files), `frontend/package.json`, `vite.config.ts`, `tsconfig.json`, `e2e/*`.

---

## 1. Key Findings & Remediation Plan

| ID | Severity | Location | Finding | Remediation (Apply Phase 7) |
|---|---|---|---|---|
| FE-01 | **P1** | `lib/urlValidation.ts:79-92`, `features/organizations/schemas/organizationSchemas.ts:55-58`, `OrganizationRequestModal.tsx:45-50`, `AdminGamificationGovernance.tsx:27,63,89` | `optionalSafeUrlSchema` rejects `/api/v1/images/{guid}` DB-stored image URLs because it validates via `isSafeHttpUrl`. Organization logo and badge icon forms block DB-backed URLs. | Add `optionalSafeAssetOrUrlSchema` (and exact `/api/v1/images/{guid}` / `/images/` / `/logos/` check in `isSafeAssetOrHttpUrl`) and apply to `logoUrl` and `badgeIcon`, while keeping `optionalSafeUrlSchema` strictly `http(s)://` for external website/meeting/social URLs. |
| FE-02 | **P1** | `features/projects/ProjectDetailPage.tsx:109-113, 216-224` | `apply` mutation `onSuccess` invalidates `queryKeys.projects.applications(projectId)` instead of `queryKeys.projects.myApplication(projectId, user?.studentId)` (or `invalidateProjectData()`), leaving applicants on the stale `"Apply to join"` state until manual refresh. | Call `invalidateProjectData()` inside `apply.onSuccess`. |
| FE-03 | **P1** | `components/layout/NotificationBell.tsx:45-60, 121-122` | `NotificationBell` fetches `notifications` (now capped at latest 15) and counts `!n.isRead` on the array, under-counting unread badges when >15 unread alerts exist. | Query `notificationsApi.getUnreadCount()` using `queryKeys.notifications.unreadCount` (matching `StudentDashboardPage` and `NotificationsInboxPage`). |
| FE-04 | **P1** | `api/projects.api.ts:43-46` vs `backend/ProjectDtos.cs:18` & `api/endpoints.ts:9-150` | `ProjectDetailDto` on backend omits `CoverImageUrl`, so `project.coverImageUrl` is always `undefined` on `ProjectDetailPage`; `endpoints.ts` lacks `images` upload/read endpoints and still says `"matching all 67 paths"`. | Add `CoverImageUrl` to backend `ProjectDetailDto` + `ProjectService.GetProjectAsync`, and add `images` endpoints to `endpoints.ts`. |
| FE-05 | **P2** | Untracked files (`MotionPreferenceControl.tsx`, `motionPreference.ts`, `hero-student-cyan.jpg`, `hero-student-orange.jpg`) | 4 pre-existing untracked WIP files in git have zero imports in `src/` or `index.css`. | Preserve untouched (per instructions); document in final report so the user decides whether to wire or clean them up. |
| FE-06 | **P2** | `e2e/m3-activities-hierarchy-events.spec.ts:92, 128-129` | Test name `"and Arabic RTL mode"` contradicts English-only assertion (`toHaveCount(0)` on language toggle). | Rename test description to accurately reflect English-only enforcement check. |
