# Frontend Module 01: Foundation, Design System, Auth & Platform Admin

> **Owner:** M1 — Ahmed Elsayed (Tyson · Tech Lead)  
> **API Operations Owned:** 22 (`Auth` 6 · `Universities` 6 · `Faculties` 5 · `Admin` 5)  
> **Master Spec:** [`tasks/frontend-plan.md`](../frontend-plan.md)  
> **Owned Folders:** `frontend/src/components/ui/`, `frontend/src/components/layout/`, `frontend/src/lib/`, `frontend/src/api/{client,endpoints,enums,types,auth.api,universities.api,admin.api}.ts`, `frontend/src/features/auth/`, `frontend/src/features/admin/`

---

## 1. Scope & Responsibilities

M1 establishes the **Phase 0 shared foundation** that unblocks M2, M3, and M4 by Day 2, then delivers all authentication, student onboarding, academic catalog management, and `PlatformAdmin` governance screens.

---

## 2. Owned Routes

| Route | Guard | View Description |
|---|---|---|
| `/auth/login` | Guest only (redirects to `/projects` if authenticated) | High-contrast Handshake split-layout Sign In form with inline validation and `session_expired` banner support |
| `/auth/register` | Guest only | Multi-field student onboarding with cached University → Faculty cascading dropdown (`GET /universities/catalog`) and academic year picker (`1–6`) |
| `/auth/forgot-password` | Guest only | Opaque password recovery request form (never reveals whether email exists; explains 15-min email token TTL) |
| `/auth/reset-password` | Guest only | Email + token (supports `?email=&token=` query prefill) + new password form with strength hints |
| `/admin` | `PlatformAdmin` | Overview dashboard KPIs (active & soft-deleted counts + role distribution bars) |
| `/admin/catalog` | `PlatformAdmin` | Universities & nested Faculties CRUD management (auto-invalidates catalog cache) |
| `/admin/roles` | `PlatformAdmin` | Student search & role promotion/demotion modal (`Student` / `OrgAdmin` with `THR-E-03` PlatformAdmin elevation guard messaging) |
| `/admin/restore` | `PlatformAdmin` | Universal Soft-Delete Recycle Bin across all 8 entity types (`student`, `university`, `faculty`, `skill`, `project`, `organization`, `event`, `badge`) with one-click Restore |
| `/admin/audit-logs` | `PlatformAdmin` | Filterable immutable `AuditLog` table (`action`, `entityType`, `actorStudentId`, old/new JSON diff drawer, client IP, timestamp) |

---

## 3. Owned API Endpoints (22 Operations)

| Group | Method | Endpoint | Auth / Notes |
|---|---|---|---|
| **Auth** | `POST` | `/api/v1/auth/register` | Public (`auth-policy`) → `201 AuthResponse` |
| **Auth** | `POST` | `/api/v1/auth/login` | Public (`auth-policy`) → `200 AuthResponse` |
| **Auth** | `POST` | `/api/v1/auth/refresh` | Public (`auth-policy`) → `200 AuthResponse` (single-flight mutex!) |
| **Auth** | `POST` | `/api/v1/auth/logout` | Auth → `204` (rotates `SecurityStamp`, clears local tokens) |
| **Auth** | `POST` | `/api/v1/auth/forgot-password` | Public → `200 ForgotPasswordResponse` (opaque message, no token) |
| **Auth** | `POST` | `/api/v1/auth/reset-password` | Public → `200 BaseResponse` |
| **Universities** | `GET` | `/api/v1/universities` | Public · `?search=&page=&pageSize=` |
| **Universities** | `GET` | `/api/v1/universities/catalog` | Public · cached anonymous lookup for registration dropdown |
| **Universities** | `GET` | `/api/v1/universities/{id}` | Public · `UniversityDetailDto` with faculties |
| **Universities** | `POST` | `/api/v1/universities` | `PlatformAdmin` → `201` |
| **Universities** | `PUT` | `/api/v1/universities/{id}` | `PlatformAdmin` → `200` |
| **Universities** | `DELETE` | `/api/v1/universities/{id}` | `PlatformAdmin` → `204` |
| **Faculties** | `GET` | `/api/v1/universities/{universityId}/faculties` | Public |
| **Faculties** | `GET` | `/api/v1/faculties/{id}` | Public |
| **Faculties** | `POST` | `/api/v1/universities/{universityId}/faculties` | `PlatformAdmin` → `201` |
| **Faculties** | `PUT` | `/api/v1/faculties/{id}` | `PlatformAdmin` → `200` |
| **Faculties** | `DELETE` | `/api/v1/faculties/{id}` | `PlatformAdmin` → `204` |
| **Admin** | `GET` | `/api/v1/admin/dashboard` | `PlatformAdmin` → `AdminDashboardDto` |
| **Admin** | `PATCH` | `/api/v1/admin/students/{id}/role` | `PlatformAdmin` → `StudentSummaryDto` (rotates target `SecurityStamp`) |
| **Admin** | `GET` | `/api/v1/admin/soft-deleted` | `PlatformAdmin` · `?entityType=` |
| **Admin** | `POST` | `/api/v1/admin/restore/{entityType}/{id}` | `PlatformAdmin` → `200` |
| **Admin** | `GET` | `/api/v1/admin/audit-logs` | `PlatformAdmin` · `?action=&entityType=&actorStudentId=&page=&pageSize=` |

---

## 4. Phased Task Checklist

### Phase 0 — Shared Foundation (Days 1–2 · Unblocks Entire Team)
- [x] **M1-01:** Install `@tanstack/react-query`, `react-hook-form`, `zod`, `@hookform/resolvers`, `sonner`, `qrcode.react` in `frontend/`.
- [x] **M1-02:** Sync `frontend/src/api/endpoints.ts` with all 67 OpenAPI paths (adding `auditLogs`, `leaderboard`, `checkInCode`, `selfCheckIn`, `attendanceReport`, `manualAwardBadge`) and create `frontend/src/api/enums.ts` (`as const` union types for all 12 domain enums + `OrganizationRoles`).
- [x] **M1-03:** Upgrade `frontend/src/api/client.ts` with single-flight `401` silent refresh queue + `Retry-After` extraction on `429` + `204 No Content` safe handling + HTML text fetcher helper.
- [x] **M1-04:** Update `frontend/src/index.css` with full Handshake design tokens (`@theme`), `.service-card` spotlight glow utilities, `.showcase-gradient-1..4`, and `@media (prefers-reduced-motion: reduce)` rules.
- [x] **M1-05:** Build shared UI primitive kit in `frontend/src/components/ui/`: `Button`, `Input`, `Textarea`, `Select`, `Card`, `SpotlightCard`, `Badge`, `StatusPill`, `Avatar`, `Modal`, `ConfirmDialog`, `Tabs`, `DataTable`, `Pagination`, `EmptyState`, `Skeleton`, `ErrorBanner`.
- [x] **M1-06:** Implement `AuthProvider` (`useAuth` hook with JWT payload decoder for `sub`, `role`, `org_admin[]` UI hints), `<ProtectedRoute>`, `<RoleGuard>`, `QueryClientProvider`, `<Toaster />`, `queryKeys.ts`, and modular route-fragment wiring in `router.tsx`.

### Phase 1 — Auth, Onboarding & AppShell (Days 3–6)
- [x] **M1-07:** Upgrade `frontend/src/api/auth.api.ts` (full `AuthResponse`, `forgotPassword`, `resetPassword`) and `universities.api.ts` (full CRUD + faculties).
- [x] **M1-08:** Build Login (`/auth/login`) and Register (`/auth/register`) pages with Zod schemas, University → Faculty dependent dropdown from `/universities/catalog`, and clear `400` field error mapping.
- [x] **M1-09:** Build Forgot Password (`/auth/forgot-password`) and Reset Password (`/auth/reset-password`) pages with query-string token prefill and rate-limit (`429`) cooldown feedback.
- [x] **M1-10:** Upgrade `AppShell` and `Navbar` for authenticated app routes (responsive mobile drawer, active nav indicator, slots for M4's `<NotificationBell />` and `<XpLevelChip />`, Admin link visible only to `PlatformAdmin`, Sign Out calling `POST /auth/logout`).

### Phase 2 — Platform Admin Governance Suite (Days 7–11)
- [x] **M1-11:** Create `frontend/src/api/admin.api.ts` covering all 5 admin endpoints.
- [x] **M1-12:** Build `/admin` Dashboard KPIs + role distribution breakdown cards + sub-navigation tabs.
- [x] **M1-13:** Build `/admin/catalog` Universities & nested Faculties CRUD management (create/edit/soft-delete with URL validation via `https?://` and confirmation dialogs).
- [x] **M1-14:** Build `/admin/roles` Student Role Governance view (handles `403` `THR-E-03` promotion guard cleanly and warns that role changes immediately invalidate target user sessions).
- [x] **M1-15:** Build `/admin/restore` 8-entity Soft-Delete Recycle Bin with entity-type filter pills and instant Restore + cache invalidation.
- [x] **M1-16:** Build `/admin/audit-logs` paginated, filterable immutable Audit Log table with expandable Old/New JSON diff inspection drawer.

### Phase 3 — Integration & Testing (Days 12–14)
- [ ] **M1-17:** Write Vitest tests for Axios refresh-token mutex (concurrent `401` calls trigger only 1 `/auth/refresh`), `AuthContext` guards, Login/Register forms, and Admin Restore flow.
- [ ] **M1-18:** Mount M4's `<NotificationBell />` & `<XpLevelChip />` into `AppShell` and lead final cross-module PR review.

---

## 5. Exports Required by Other Teammates

- `useAuth()` → `{ user, isAuthenticated, isLoading, isPlatformAdmin, isOrgAdminOf(orgId), login, register, logout }`
- `<ProtectedRoute />`, `<RoleGuard roles={["PlatformAdmin"]} />`
- `frontend/src/components/ui/*` & `frontend/src/lib/queryKeys.ts`
