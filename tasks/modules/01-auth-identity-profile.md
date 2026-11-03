# Module Plan 01: Authentication, Identity, Universities & Student Management

**Owner:** Tyson (M1 - Tech Lead)  
**Entities Owned:** `Student`, `Faculty`, `University`, `StudentProfile`, `StudentInterest`  
**Total Endpoints:** 30 (All 30 Completed, Tested & Verified)  
**Status:** 100% Completed (All 30 Endpoints Verified & Green)  

---

## 1. Master Scope & Ownership

Tyson is responsible for the platform's core identity, authentication lifecycle, academic institutions, and student governance:
1. **Authentication & Session Security:** Registration, Login, Token Rotation, Logout, Password Recovery.
2. **Academic Affiliation:** Accredited Universities and Faculties catalog, academic code validation, and nested lookups.
3. **Student Identity & Profile:** Account settings (`/me`), Extended Portfolio (`/profile`), Interest Tags (`/interests`).
4. **Student Directory & Discovery:** Public profile cards, multi-criteria directory search (faculty, academic year, skills).
5. **Platform Governance:** `PlatformAdmin` role-based access control, account soft-deletion, and rate limiting.

---

## 2. All 26 Endpoints Owned by Tyson

| # | Method | Route | Description | Authorize | Phase | Status |
|---|---|---|---|---|---|---|
| 1 | `GET` | `/health` | Service & DB connectivity health check. | 🌐 Public | Phase A | ✅ Done |
| 2 | `POST` | `/api/v1/auth/register` | Student registration with faculty linkage. | 🌐 Public | Phase A | ✅ Done |
| 3 | `POST` | `/api/v1/auth/login` | Login credentials verification & JWT issuance. | 🌐 Public | Phase A | ✅ Done |
| 4 | `POST` | `/api/v1/auth/refresh` | Cryptographic refresh token rotation. | 🌐 Public | Phase A | ✅ Done |
| 5 | `POST` | `/api/v1/auth/logout` | Revoke active refresh token. | 🔒 Auth | Phase A | ✅ Done |
| 6 | `POST` | `/api/v1/auth/forgot-password` | Request password reset token. | 🌐 Public | Phase D | ✅ Done |
| 7 | `POST` | `/api/v1/auth/reset-password` | Set new password with reset token. | 🌐 Public | Phase D | ✅ Done |
| 8 | `GET` | `/api/v1/universities` | Paginated list of universities. | 🔒 Auth | Phase B | ✅ Done |
| 9 | `POST` | `/api/v1/universities` | Create accredited university. | 👑 PlatformAdmin | Phase B | ✅ Done |
| 10 | `GET` | `/api/v1/universities/{id}` | Get university details + constituent faculties. | 🔒 Auth | Phase B | ✅ Done |
| 11 | `PUT` | `/api/v1/universities/{id}` | Update university information. | 👑 PlatformAdmin | Phase B | ✅ Done |
| 12 | `DELETE` | `/api/v1/universities/{id}` | Soft-delete university & child faculties. | 👑 PlatformAdmin | Phase B | ✅ Done |
| 13 | `GET` | `/api/v1/universities/{id}/faculties` | List faculties in university. | 🔒 Auth | Phase B | ✅ Done |
| 14 | `POST` | `/api/v1/universities/{id}/faculties` | Add faculty to university. | 👑 PlatformAdmin | Phase B | ✅ Done |
| 15 | `PUT` | `/api/v1/faculties/{id}` | Update faculty details & academic code. | 👑 PlatformAdmin | Phase B | ✅ Done |
| 16 | `DELETE` | `/api/v1/faculties/{id}` | Soft-delete faculty (`DeletedAt = UtcNow`). | 👑 PlatformAdmin | Phase B | ✅ Done |
| 17 | `GET` | `/api/v1/students` | Search / filter student directory. | 🔒 Auth | Phase C | ✅ Done |
| 18 | `GET` | `/api/v1/students/{id}` | Public student profile card & badges. | 🔒 Auth | Phase C | ✅ Done |
| 19 | `GET` | `/api/v1/students/me` | Fetch authenticated student profile. | 👤 Owner | Phase A | ✅ Done |
| 20 | `PATCH` | `/api/v1/students/me` | Update personal names or academic year. | 👤 Owner | Phase A | ✅ Done |
| 21 | `DELETE` | `/api/v1/students/me` | Soft-delete student account. | 👤 Owner / 👑 Admin | Phase C | ✅ Done |
| 22 | `GET` | `/api/v1/students/{id}/profile` | Fetch public extended portfolio. | 🔒 Auth | Phase A | ✅ Done |
| 23 | `PUT` | `/api/v1/students/me/profile` | Upsert bio, portfolio photo, GitHub, LinkedIn. | 👤 Owner | Phase A | ✅ Done |
| 24 | `GET` | `/api/v1/students/{id}/interests` | List student interest tags. | 🔒 Auth | Phase A | ✅ Done |
| 25 | `POST` | `/api/v1/students/me/interests` | Add interest tag to profile. | 👤 Owner | Phase A | ✅ Done |
| 26 | `DELETE` | `/api/v1/students/me/interests/{tag}` | Soft-delete interest tag from profile. | 👤 Owner | Phase A | ✅ Done |
| 27 | `GET` | `/api/v1/admin/dashboard` | Platform metrics & role distribution. | 👑 PlatformAdmin | Phase E | ✅ Done |
| 28 | `PATCH` | `/api/v1/admin/students/{id}/role` | Promote/demote student role (`Student`, `OrgAdmin`, `PlatformAdmin`). | 👑 PlatformAdmin | Phase E | ✅ Done |
| 29 | `GET` | `/api/v1/admin/soft-deleted` | Browse soft-deleted audit records across entities. | 👑 PlatformAdmin | Phase E | ✅ Done |
| 30 | `POST` | `/api/v1/admin/restore/{entityType}/{id}` | Universal restoration of soft-deleted records (`DeletedAt = null`). | 👑 PlatformAdmin | Phase E | ✅ Done |

---

## 3. Tyson Implementation Phases

### Phase A: Foundation, Core Auth & Profile (Completed)
- **T-TYSON-01 to T-TYSON-11:**
  - Solution scaffolding, CI pipeline, branch rules.
  - Single-column soft delete (`DeletedAt`), `SoftDeleteInterceptor`.
  - Database migration & SQL Server verification (23 tables, 29 FKs all `NO ACTION`, 10 filtered unique indexes).
  - JWT Bearer auth, password hasher, crypto refresh token rotation.
  - SRP Controllers (`AuthController`, `StudentsController`, `StudentProfilesController`, `StudentInterestsController`).
  - Standard `BaseApiController` & `BaseResponse<T>` JSON envelope.
  - FluentValidation action filter & RFC 7807 `ErrorResponse`.
  - 33 automated tests passing (21 Unit + 12 Integration).

---

### Phase B: Universities & Faculties Module (Target: T14)

#### Task B.1: University & Faculty DTOs and Validators (Size: S)
- **Description:** Define request and response records for universities and faculties with FluentValidation rules.
- **Acceptance Criteria:**
  - `CreateUniversityRequest(string Name, string City, string Country, string? Website)`: Name, City, Country required. Website must be absolute URL if provided.
  - `UpdateUniversityRequest(string Name, string City, string Country, string? Website)`: Same rules.
  - `CreateFacultyRequest(string Name, string Code)`: Name required (max 150), Code required (max 20, alphanumeric).
  - `UpdateFacultyRequest(string Name, string Code)`: Same rules.
  - Response DTOs: `UniversityDto`, `UniversityDetailDto`, `FacultyDto`.
- **Files Touched:**
  - `src/TeamUp.Application/Universities/UniversityDtos.cs`
  - `src/TeamUp.Application/Universities/Validators/UniversityValidators.cs`

#### Task B.2: University Service & Controller (Size: M)
- **Description:** Implement `IUniversityService` and `UniversitiesController` (`api/v1/universities`).
- **Acceptance Criteria:**
  - `GET /api/v1/universities`: Paginated search with `PageRequest`, `.AsNoTracking()`, returns `PagedResult<UniversityDto>`.
  - `GET /api/v1/universities/{id}`: Returns university details + constituent active faculties.
  - `POST /api/v1/universities`: Restricted to `PlatformAdmin`. Validates unique university name `WHERE [DeletedAt] IS NULL`. Returns `201 Created`.
  - `PUT /api/v1/universities/{id}`: Restricted to `PlatformAdmin`. Updates university. Returns `200 OK`.
  - `DELETE /api/v1/universities/{id}`: Restricted to `PlatformAdmin`. Soft-deletes university + child faculties in a single transaction. Returns `204 NoContent`.
- **Files Touched:**
  - `src/TeamUp.Application/Universities/IUniversityService.cs`
  - `src/TeamUp.Infrastructure/Services/UniversityService.cs`
  - `src/TeamUp.Api/Controllers/UniversitiesController.cs`

#### Task B.3: Faculty Service & Controller (Size: M)
- **Description:** Implement `IFacultyService` and `FacultiesController` with nested and root endpoints.
- **Acceptance Criteria:**
  - `GET /api/v1/universities/{id}/faculties`: Lists faculties belonging to a specific university.
  - `POST /api/v1/universities/{id}/faculties`: Restricted to `PlatformAdmin`. Validates unique Code within university. Returns `201 Created`.
  - `PUT /api/v1/faculties/{id}`: Restricted to `PlatformAdmin`. Updates faculty name and code. Returns `200 OK`.
  - `DELETE /api/v1/faculties/{id}`: Restricted to `PlatformAdmin`. Soft-deletes faculty (`DeletedAt = UtcNow`). Returns `204 NoContent`.
- **Files Touched:**
  - `src/TeamUp.Application/Universities/IFacultyService.cs`
  - `src/TeamUp.Infrastructure/Services/FacultyService.cs`
  - `src/TeamUp.Api/Controllers/FacultiesController.cs`

#### Task B.4: Universities & Faculties Integration Tests (Size: S)
- **Description:** Automated integration tests in `TeamUp.IntegrationTests/UniversityAndFacultyTests.cs`.
- **Verification:**
  - Paged list, detail lookup, duplicate name rejection (409), unauthorized caller rejection (403 for Student, 401 unauth), nested faculty creation, soft delete.

---

### Phase C: Student Directory, Public Discovery & Account Deletion

#### Task C.1: Student Directory Query & DTOs (Size: S)
- **Description:** Specification for student directory search with pagination, filtering by academic year, faculty, and search keywords.
- **Acceptance Criteria:**
  - `StudentDirectoryQuery(string? Search, int? FacultyId, int? AcademicYear, int Page, int PageSize)`
  - `StudentCardDto(int StudentId, string FullName, string Email, int AcademicYear, string FacultyName, string UniversityName, IReadOnlyList<string> Interests)`
- **Files Touched:**
  - `src/TeamUp.Application/Students/StudentDirectoryDtos.cs`

#### Task C.2: Directory & Discovery Endpoints in `StudentsController` (Size: M)
- **Description:** Implement `GET /api/v1/students` and `GET /api/v1/students/{id}` with `.AsNoTracking()`.
- **Acceptance Criteria:**
  - `GET /api/v1/students`: Filtered directory search returning `PagedResult<StudentCardDto>`.
  - `GET /api/v1/students/{id}`: Public profile card including active interests and badges earned.
- **Files Touched:**
  - `src/TeamUp.Infrastructure/Services/ProfileService.cs`
  - `src/TeamUp.Api/Controllers/StudentsController.cs`

#### Task C.3: Student Account Soft Deletion (Size: S)
- **Description:** Implement `DELETE /api/v1/students/me` and `DELETE /api/v1/students/{id}` (`PlatformAdmin`).
- **Acceptance Criteria:**
  - Marks `student.DeletedAt = DateTime.UtcNow`.
  - Clears `RefreshToken` and `RefreshTokenExpiresAt`.
  - Unassigns student from active tasks (`ProjectTask.AssignedStudentId = null`).
  - Returns `204 NoContent`.
- **Files Touched:**
  - `src/TeamUp.Infrastructure/Services/ProfileService.cs`
  - `src/TeamUp.Api/Controllers/StudentsController.cs`

#### Task C.4: Directory & Deletion Integration Tests (Size: S)
- **Verification:** Integration tests verifying directory filtering, public card query, self account deletion, and confirming deleted student cannot authenticate.

---

### Phase D: Password Recovery & Reset Flow

#### Task D.1: Password Reset Token Storage & Security (Size: S)
- **Description:** Add `PasswordResetToken` and `PasswordResetTokenExpiresAt` to `Student`.
- **Acceptance Criteria:**
  - Crypto-random 64-byte token with 1-hour expiration.
  - Migration `AddPasswordResetToken` applied to database.
- **Files Touched:**
  - `src/TeamUp.Domain/Entities/Entities.cs`
  - `src/TeamUp.Infrastructure/Persistence/Configurations/IdentityConfigs.cs`

#### Task D.2: Password Reset DTOs, Validators & Endpoints (Size: M)
- **Description:** Implement `POST /api/v1/auth/forgot-password` and `POST /api/v1/auth/reset-password` in `AuthController`.
- **Acceptance Criteria:**
  - `POST /forgot-password`: Generates reset token. Returns `200 OK` with safe generic message (no user enumeration).
  - `POST /reset-password`: Validates token and expiry. Enforces password complexity. Resets password, clears reset token, invalidates refresh token. Returns `200 OK`.
- **Files Touched:**
  - `src/TeamUp.Application/Auth/AuthDtos.cs`
  - `src/TeamUp.Application/Auth/Validators/AuthValidators.cs`
  - `src/TeamUp.Infrastructure/Services/AuthService.cs`
  - `src/TeamUp.Api/Controllers/AuthController.cs`

#### Task D.3: Password Reset Integration Tests (Size: S)
- **Verification:** Tests cover requesting token, valid reset + new password login, invalid token rejection (401), expired token rejection (401).

---

### Phase E: Governance, Rate Limiting & Polish

#### Task E.1: Platform Admin Authorization Policies (Size: XS)
- **Description:** Register explicit authorization policies (`PlatformAdminOnly`, `SelfOrAdmin`) in `Program.cs`.
- **Files Touched:** `src/TeamUp.Api/Program.cs`

#### Task E.2: Rate Limiting on Auth Endpoints (Size: S)
- **Description:** Configure `Microsoft.AspNetCore.RateLimiting` (10 requests/minute per client IP) on `/api/v1/auth/*`.
- **Files Touched:** `src/TeamUp.Api/Program.cs`

#### Task E.3: Master Test Run & Evidence Documentation (Size: S)
- **Description:** Update live runner script to execute all 26 endpoints owned by Tyson. Regenerate `AUTH-PROFILE-TEST-REPORT.md` and HTML preview dashboard.
