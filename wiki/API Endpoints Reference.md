# TeamUp — API Endpoints Reference

> **Curated Endpoint Catalog & Request/Response Contracts** · Verified payload contracts, headers, HTTP status codes, and DTO samples grounded in `TeamUp.Api` controllers and `TeamUp.Application` DTOs.

> **Sources:**
> - [docs/api/openapi.json](../docs/api/openapi.json)
> - [wiki/ROUTES-APPENDIX.md](./ROUTES-APPENDIX.md)
> - [backend/src/TeamUp.Api/Controllers](../backend/src/TeamUp.Api/Controllers)
> - [backend/src/TeamUp.Application](../backend/src/TeamUp.Application)

---

## Table of Contents

1. [Authentication & Session Tokens](#1-authentication--session-tokens)
2. [Academic Catalog: Universities & Faculties](#2-academic-catalog-universities--faculties)
3. [Student Directory, Profiles & Photo Upload](#3-student-directory-profiles--photo-upload)
4. [Skills Master Catalog & Student Claims](#4-skills-master-catalog--student-claims)
5. [Projects & Team Formation](#5-projects--team-formation)
6. [Project Applications](#6-project-applications)
7. [Project Sprint Tasks Board](#7-project-sprint-tasks-board)
8. [Student Organizations & Leadership](#8-student-organizations--leadership)
9. [Membership Recruitment](#9-membership-recruitment)
10. [Campus Events & Sessions](#10-campus-events--sessions)
11. [Event Registration & Rotating QR/PIN Check-In](#11-event-registration--rotating-qrpin-check-in)
12. [Gamification Ledger, Score Summary & Leaderboard](#12-gamification-ledger-score-summary--leaderboard)
13. [Badges & Achievement Criteria](#13-badges--achievement-criteria)
14. [Notifications Inbox](#14-notifications-inbox)
15. [Platform Administration, Reports & Recovery](#15-platform-administration-reports--recovery)

---

### 1. Authentication & Session Tokens

#### Register New Student
- **Route:** `POST /api/v1/auth/register`
- **Access:** `🌐 Public` (`auth-policy`: `30 req/min` per IP)
- **Request Body (`RegisterRequest`):**
  ```json
  {
    "firstName": "Omar",
    "lastName": "Hassan",
    "email": "student@cu.edu.eg",
    "password": "Password123!",
    "academicYear": 3,
    "facultyId": 1,
    "emailNotificationsEnabled": true,
    "receiveLeaderboardDigests": true,
    "receiveProjectMatchEmails": true
  }
  ```
- **Response (`201 Created` — `BaseResponse<AuthResponse>`):**
  ```json
  {
    "success": true,
    "message": "Student registered successfully.",
    "statusCode": 201,
    "data": {
      "accessToken": "eyJhbGciOiJIUzI1Ni...",
      "refreshToken": "64_byte_crypto_string...",
      "expiresAt": "2026-11-24T12:15:00Z",
      "student": {
        "studentId": 105,
        "firstName": "Omar",
        "lastName": "Hassan",
        "email": "student@cu.edu.eg",
        "academicYear": 3,
        "role": "Student",
        "facultyId": 1,
        "facultyName": "Faculty of Computers and Artificial Intelligence",
        "universityName": "Cairo University",
        "photoUrl": null,
        "showEmailInDirectory": false,
        "emailNotificationsEnabled": true,
        "receiveLeaderboardDigests": true,
        "receiveProjectMatchEmails": true
      }
    },
    "errors": null,
    "timestamp": "2026-11-24T12:00:00Z"
  }
  ```

#### Authenticate & Login
- **Route:** `POST /api/v1/auth/login`
- **Access:** `🌐 Public` (`auth-policy`)
- **Request Body (`LoginRequest`):**
  ```json
  {
    "email": "student@cu.edu.eg",
    "password": "Password123!"
  }
  ```
- **Response (`200 OK`):** Returns `BaseResponse<AuthResponse>` containing `accessToken`, `refreshToken`, `expiresAt`, and `student` summary.

#### Refresh Access Token
- **Route:** `POST /api/v1/auth/refresh`
- **Access:** `🌐 Public` (`auth-policy`)
- **Request Body (`RefreshTokenRequest`):**
  ```json
  {
    "refreshToken": "64_byte_crypto_string..."
  }
  ```
- **Response (`200 OK`):** Returns newly rotated `accessToken` and fresh `refreshToken` (10s reuse grace window; stale replay revokes the session).

---

### 2. Academic Catalog: Universities & Faculties

#### Cached University Catalog (Registration UX)
- **Route:** `GET /api/v1/universities/catalog`
- **Access:** `🌐 Public`
- **Response (`200 OK`):** 1-hour `IMemoryCache`-backed list of `UniversityDetailDto` objects with nested `FacultySummaryDto` entries.

---

### 3. Student Directory, Profiles & Photo Upload

#### Public Student Showcase
- **Route:** `GET /api/v1/students/showcase?limit=6`
- **Access:** `🌐 Public`
- **Response (`200 OK`):** Returns `IReadOnlyList<PublicStudentShowcaseDto>` with top points, level, badge count, and top skills.

#### Search Students Directory
- **Route:** `GET /api/v1/students?search=react&facultyId=1&academicYear=3&page=1&pageSize=20`
- **Access:** `🔒 Auth` (`directory-policy`: `30 req/min`, max `pageSize=20`)
- **Response (`200 OK`):** Paginated `PagedResult<StudentCardDto>` (email omitted unless `ShowEmailInDirectory == true` or caller is `PlatformAdmin`).

#### Get Public Profile Summary
- **Route:** `GET /api/v1/students/{id}`
- **Access:** `🔒 Auth`
- **Response (`200 OK`):** Returns `StudentPublicProfileDto` (bio, social links, photo URL, interests, and email if opted in).

#### Upload Profile Portrait
- **Route:** `POST /api/v1/students/me/profile/photo`
- **Access:** `👤 Owner` (`multipart/form-data`, `file` field, max 3 MB JPEG/PNG/WebP)
- **Response (`200 OK`):** Returns updated `StudentProfileDto` with persisted `photoUrl`.

---

### 4. Skills Master Catalog & Student Claims

#### Get Skills Catalog
- **Route:** `GET /api/v1/skills?category=Backend&search=asp`
- **Access:** `🔒 Auth`
- **Response (`200 OK`):** List of `SkillDto` items (unfiltered catalog cached in `IMemoryCache` for 1 hour).

#### Claim / Update Skill Proficiency
- **Route:** `PUT /api/v1/students/me/skills/{skillId}`
- **Access:** `👤 Owner`
- **Request Body (`ClaimSkillRequest`):**
  ```json
  {
    "proficiencyLevel": "Advanced",
    "yearsOfExperience": 2.5
  }
  ```
- **Response (`200 OK`):** Returns `StudentSkillDto` (`Beginner`, `Intermediate`, or `Advanced`).

---

### 5. Projects & Team Formation

#### Create Project
- **Route:** `POST /api/v1/projects`
- **Access:** `🔒 Auth`
- **Request Body (`CreateProjectRequest`):**
  ```json
  {
    "title": "Smart Campus Navigator",
    "description": "Indoor positioning and turn-by-turn navigation for university buildings.",
    "maxTeamSize": 5
  }
  ```
- **Response (`201 Created`):** Returns `ProjectDetailDto` with creator added to `ProjectMember`.

#### Add / Update Required Skill on Project
- **Route:** `PUT /api/v1/projects/{id}/required-skills/{skillId}`
- **Access:** `👤 Creator`
- **Request Body (`UpsertRequiredSkillRequest`):**
  ```json
  {
    "requiredLevel": "Intermediate",
    "isMandatory": true
  }
  ```
- **Response (`204 No Content`)**

---

### 6. Project Applications

#### Apply to Join Project
- **Route:** `POST /api/v1/projects/{id}/applications`
- **Access:** `🔒 Auth`
- **Request Body (`CreateProjectApplicationRequest`):**
  ```json
  {
    "message": "I have 2 years of Flutter experience and want to contribute to the mobile UI."
  }
  ```
- **Response (`201 Created`):** `ProjectApplicationDto` created with `status: "Pending"`.

#### Review Application (Accept / Reject)
- **Route:** `PATCH /api/v1/projects/{id}/applications/{appId}`
- **Access:** `👤 Creator`
- **Request Body (`ReviewProjectApplicationRequest`):**
  ```json
  {
    "status": "Accepted"
  }
  ```
- **Response (`200 OK`):** Updates application status, adds applicant to `ProjectMember` when `Accepted`, and emits `ApplicationReviewedDomainEvent`.

---

### 7. Project Sprint Tasks Board

#### Create Sprint Task
- **Route:** `POST /api/v1/projects/{id}/tasks`
- **Access:** `👥 Member`
- **Request Body (`CreateProjectTaskRequest`):**
  ```json
  {
    "title": "Build Bluetooth beacon scanner service",
    "description": "Integrate cross-platform BLE scanning library.",
    "dueDate": "2026-12-01T18:00:00Z",
    "priority": "High"
  }
  ```
- **Response (`201 Created`):** Returns `ProjectTaskDto` with `status: "Todo"`.

#### Assign Task to Team Member
- **Route:** `PUT /api/v1/tasks/{id}/assignee`
- **Access:** `👤 Creator`
- **Request Body (`AssignTaskRequest`):**
  ```json
  {
    "studentId": 105
  }
  ```
- **Response (`204 No Content`):** Assigns task and triggers `TaskAssignedDomainEvent` notification.

#### Update Task / Transition Status
- **Route:** `PATCH /api/v1/tasks/{id}`
- **Access:** `👥 Member`
- **Request Body (`UpdateProjectTaskRequest`):**
  ```json
  {
    "status": "Done"
  }
  ```
- **Response (`200 OK`):** Updates task (`Todo`, `InProgress`, `InReview`, `Done`); transitioning an assigned task to `Done` triggers `TaskCompletedDomainEvent` (`+50 XP` once per task).

---

### 8. Student Organizations & Leadership

#### List Student Organizations
- **Route:** `GET /api/v1/organizations?search=IEEE&category=Technical&page=1&pageSize=20`
- **Access:** `🔒 Auth`
- **Response (`200 OK`):** Paginated `PagedResult<OrganizationSummaryDto>`.

---

### 9. Membership Recruitment

#### Apply for Club Membership
- **Route:** `POST /api/v1/organizations/{id}/applications`
- **Access:** `🔒 Auth`
- **Request Body (`CreateMembershipApplicationRequest`):**
  ```json
  {
    "message": "Eager to mentor junior students in web development."
  }
  ```
- **Response (`201 Created`):** Returns `MembershipApplicationDto`.

---

### 10. Campus Events & Sessions

#### Create Event
- **Route:** `POST /api/v1/events`
- **Access:** `🛡️ OrgAdmin`
- **Request Body (`CreateEventRequest`):**
  ```json
  {
    "title": "Cloud Native .NET 10 Hackathon",
    "description": "Full day hands-on building cloud-native microservices.",
    "type": "Hackathon",
    "location": "Lab 304, Faculty of Computers",
    "startDate": "2026-11-28T09:00:00Z",
    "endDate": "2026-11-28T17:00:00Z",
    "capacity": 50,
    "organizationId": 1
  }
  ```
- **Response (`201 Created`):** Returns `EventDto`.

---

### 11. Event Registration & Rotating QR/PIN Check-In

#### Register for Event
- **Route:** `POST /api/v1/events/{id}/registrations`
- **Access:** `🔒 Auth`
- **Response (`201 Created`):** Returns `EventRegistrationDto` (`Confirmed` or `Waitlisted`).

#### Generate Rotating 60s Check-In Code
- **Route:** `POST /api/v1/sessions/{id}/attendance/check-in-code`
- **Access:** `🛡️ OrgAdmin`
- **Response (`200 OK`):** Returns `SessionCheckInCodeDto` (`sessionId`, `code` in `{step}.{16-hex-sig}` format, `expiresAtUtc`).

#### Student Self Check-In
- **Route:** `POST /api/v1/sessions/{id}/attendance/self-check-in`
- **Access:** `🔒 Auth`
- **Request Body (`SelfCheckInRequest`):**
  ```json
  {
    "verificationCode": "29927200.a1b2c3d4e5f60718"
  }
  ```
- **Response (`201 Created`):** Returns `SessionAttendanceDto` and emits `SessionAttendedDomainEvent` (`+25 XP`).

#### Printable Session Attendance Report
- **Route:** `GET /api/v1/sessions/{id}/attendance/report`
- **Access:** `🛡️ OrgAdmin`
- **Response (`200 OK`, `text/html`):** Rendered `SessionAttendanceReport.html`.

---

### 12. Gamification Ledger, Score Summary & Leaderboard

#### Student Score Summary
- **Route:** `GET /api/v1/students/{id}/score`
- **Access:** `🔒 Auth`
- **Response (`200 OK`):**
  ```json
  {
    "success": true,
    "message": "Success",
    "statusCode": 200,
    "data": {
      "studentId": 105,
      "totalPoints": 450,
      "level": 3,
      "currentLevelStart": 400,
      "nextLevelThreshold": 900,
      "pointsToNext": 450,
      "progress": 10
    },
    "errors": null,
    "timestamp": "2026-11-24T12:00:00Z"
  }
  ```

#### Campus Leaderboard
- **Route:** `GET /api/v1/gamification/leaderboard?page=1&pageSize=20`
- **Access:** `🔒 Auth`
- **Response (`200 OK`):** Paginated `PagedResult<LeaderboardEntryDto>`.

#### Manual Score Adjustment
- **Route:** `POST /api/v1/score-transactions`
- **Access:** `👑 PlatformAdmin`
- **Request Body (`CreateScoreTransactionRequest`):**
  ```json
  {
    "studentId": 105,
    "points": 100,
    "reason": "Hackathon finalist award",
    "sourceType": "Manual"
  }
  ```
- **Response (`201 Created`):** Returns `ScoreTransactionDto` and records an `AuditLog` entry.

---

### 13. Badges & Achievement Criteria

#### List Student Badges
- **Route:** `GET /api/v1/students/{id}/badges`
- **Access:** `🔒 Auth`
- **Response (`200 OK`):** List of `StudentBadgeDto` entries (`studentId`, `badgeId`, `badgeName`, `description`, `iconUrl`, `criteriaType`, `criteriaValue`, `awardedAt`).

---

### 14. Notifications Inbox

#### Get Notifications
- **Route:** `GET /api/v1/notifications?unreadOnly=true`
- **Access:** `👤 Owner`
- **Response (`200 OK`):** List of `NotificationDto` items (`notificationId`, `studentId`, `title`, `message`, `type`, `isRead`, `createdAt`).

#### Mark Notification / All Read
- **Route:** `PATCH /api/v1/notifications/{id}` or `PATCH /api/v1/notifications/mark-all-read`
- **Access:** `👤 Owner`
- **Response (`204 No Content`)**

---

### 15. Platform Administration, Reports & Recovery

#### Immutable Audit Logs
- **Route:** `GET /api/v1/admin/audit-logs?action=RoleUpdated&entityType=Student&page=1&pageSize=20`
- **Access:** `👑 PlatformAdmin`
- **Response (`200 OK`):** Paginated `PagedResult<AuditLogDto>`.

#### Governance Summary PDF & Digests
- **Route:** `GET /api/v1/admin/reports/platform-summary.pdf` (`application/pdf`)
- **Route:** `POST /api/v1/admin/communications/leaderboard-digest` / `POST /api/v1/admin/communications/project-matches`
- **Access:** `👑 PlatformAdmin`

#### Universal Soft-Delete Restore
- **Route:** `POST /api/v1/admin/restore/{entityType}/{id}`
- **Access:** `👑 PlatformAdmin`
- **Response (`200 OK`):** Restores soft-deleted entity by clearing `DeletedAt = NULL` (and invalidating relevant caches).
