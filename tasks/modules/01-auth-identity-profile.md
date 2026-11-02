# Module Plan 01: Authentication, Identity & Student Profiles

**Owner:** Tyson (M1 - Tech Lead)  
**Entities:** `Student`, `Faculty`, `University`, `StudentProfile`, `StudentInterest`  
**Dependencies:** `AppDbContext`, `SoftDeleteInterceptor`, `InitialCreate` migration  
**Status:** Verified & Done  

---

## 1. Module Overview & Scope

This module handles:
1. **Authentication & Token Lifecycle:**
   - Registration with password hashing (Argon2 / PBKDF2 via ASP.NET Identity).
   - Role-based JWT access token issuance (`sub`, `email`, `role`).
   - Refresh token issuance, rotation, and revocation (`logout`).
2. **Current Student Identity (`/api/v1/students/me`):**
   - Fetching authenticated student details, faculty, and role.
   - Updating mutable account fields (academic year, personal names).
   - Account soft deletion.
3. **Student Profile (`/api/v1/students/me/profile` & `.../{id}/profile`):**
   - 1:1 extended profile with portfolio links (Bio, PhotoUrl, GitHubUrl, LinkedInUrl).
   - Upserting profile data for the authenticated student.
4. **Student Interests (`/api/v1/students/me/interests`):**
   - Tagging student with areas of interest (AI, Web, DevOps, etc.).
   - Idempotent add and soft-delete removal.

---

## 2. API Endpoint Specification

| # | Method | Route | Description | Authorize | Status |
|---|---|---|---|---|---|
| 1 | `POST` | `/api/v1/auth/register` | Student user registration with faculty affiliation. | 🌐 Public | ✅ Done |
| 2 | `POST` | `/api/v1/auth/login` | Login with email/password; returns JWT access + refresh token. | 🌐 Public | ✅ Done |
| 3 | `POST` | `/api/v1/auth/refresh` | Rotate expired access token using valid refresh token. | 🌐 Public | ✅ Done |
| 4 | `POST` | `/api/v1/auth/logout` | Invalidate active session and revoke refresh token. | 🔒 Auth | ✅ Done |
| 5 | `GET` | `/api/v1/students/me` | Fetch authenticated student profile and account details. | 👤 Owner | ✅ Done |
| 6 | `PATCH` | `/api/v1/students/me` | Update personal names or academic year. | 👤 Owner | ✅ Done |
| 7 | `GET` | `/api/v1/students/{id}/profile`| Fetch student public extended profile. | 🔒 Auth | ✅ Done |
| 8 | `PUT` | `/api/v1/students/me/profile` | Upsert bio, portfolio photo, GitHub, LinkedIn. | 👤 Owner | ✅ Done |
| 9 | `GET` | `/api/v1/students/{id}/interests`| List student interest tags. | 🔒 Auth | ✅ Done |
| 10 | `POST` | `/api/v1/students/me/interests`| Add interest tag to profile. | 👤 Owner | ✅ Done |
| 11 | `DELETE`| `/api/v1/students/me/interests/{tag}`| Remove interest tag from profile. | 👤 Owner | ✅ Done |

---

## 3. Ordered Task Breakdown

### Task 1.1: JWT & Refresh Token Infrastructure
- **Description:** Configure JWT Bearer authentication in `TeamUp.Api`, add `JwtOptions`, implement `IJwtTokenService` to generate signed tokens with claims (`sub`, `email`, `role`, `name`), and add `RefreshToken` + `RefreshTokenExpiresAt` properties to `Student`.
- **Acceptance Criteria:**
  - Token signed with HMAC-SHA256 using key from configuration / user-secrets.
  - Generates cryptographically secure random refresh token (64-byte base64 string).
  - Validates issuer, audience, and lifetime.
- **Verification:** Unit tests generate and validate token claims.

### Task 1.2: Auth Service & Endpoints (`Register`, `Login`, `Refresh`, `Logout`)
- **Description:** Implement `IAuthService` and `AuthController` with endpoints for registration, login, token refresh, and logout.
- **Acceptance Criteria:**
  - `POST /api/v1/auth/register`: Returns `201 Created` with JWT + refresh token. Rejects duplicate emails with `409 Conflict`. Validates faculty exists.
  - `POST /api/v1/auth/login`: Returns `200 OK` on valid credentials, `401 Unauthorized` on wrong password or email not found.
  - `POST /api/v1/auth/refresh`: Rotates refresh token, returns new JWT + refresh token. Rejects expired or revoked token with `401 Unauthorized`.
  - `POST /api/v1/auth/logout`: Clears `RefreshToken` and `RefreshTokenExpiresAt`, returns `204 No Content`.
- **Verification:** Integration tests cover all 4 endpoints with success and failure paths.

### Task 1.3: Student Profile & Account Endpoints (`GET /students/me`, `PUT /students/me/profile`)
- **Description:** Implement `IProfileService` and `StudentsController` for fetching and updating current student account and 1:1 `StudentProfile`.
- **Acceptance Criteria:**
  - `GET /api/v1/students/me`: Returns student basic info, faculty name & university, role, academic year.
  - `PATCH /api/v1/students/me`: Partially updates names or academic year.
  - `GET /api/v1/students/{id}/profile`: Returns extended bio and social links. Returns `404 Not Found` if student doesn't exist.
  - `PUT /api/v1/students/me/profile`: Upserts `StudentProfile` record (creates if not existing, updates if present).
- **Verification:** Integration tests verify profile retrieval and update with JWT bearer token.

### Task 1.4: Student Interests Endpoints (`Interests`)
- **Description:** Add support for managing student interest tags via `StudentsController`.
- **Acceptance Criteria:**
  - `GET /api/v1/students/{id}/interests`: Returns list of strings.
  - `POST /api/v1/students/me/interests`: Adds tag, idempotent if tag already active.
  - `DELETE /api/v1/students/me/interests/{tag}`: Soft-deletes interest tag (`DeletedAt = UtcNow`).
- **Verification:** Tests assert interests added, listed, and soft-deleted.

---

## 4. Verification Checkpoint
- [ ] `dotnet build TeamUp.sln` compiles with 0 warnings.
- [ ] Integration test suite covers registration -> login -> profile update -> interests -> token refresh -> logout flow end-to-end.
- [ ] Unauthorized requests (`401`) and forbidden requests (`403`) verified on protected routes.
