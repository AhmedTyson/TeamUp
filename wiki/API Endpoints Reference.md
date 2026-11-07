# TeamUp — API Endpoints Reference

> **Curated Endpoint Catalog & Request/Response Contracts** · Detailed payload contracts, headers, HTTP response statuses, and sample payloads for the primary endpoints across all 16 functional modules.

> **Sources:**
> - [docs/api/API-PROPOSAL.md](../docs/api/API-PROPOSAL.md)
> - [docs/api/openapi.json](../docs/api/openapi.json)
> - [src/TeamUp.Api/Controllers](../src/TeamUp.Api/Controllers)

---

## Table of Contents

1. [Authentication & Session Tokens](#1-authentication--session-tokens)
2. [Academic Catalog: Universities & Faculties](#2-academic-catalog-universities--faculties)
3. [Student Directory & Profiles](#3-student-directory--profiles)
4. [Skills Master Catalog & Student Claims](#4-skills-master-catalog--student-claims)
5. [Projects & Team Formation](#5-projects--team-formation)
6. [Project Applications](#6-project-applications)
7. [Project Sprint Tasks Board](#7-project-sprint-tasks-board)
8. [Student Organizations & Leadership](#8-student-organizations--leadership)
9. [Membership Recruitment](#9-membership-recruitment)
10. [Campus Events & Sessions](#10-campus-events--sessions)
11. [Event Registration & Attendance Check-In](#11-event-registration--attendance-check-in)
12. [Gamification Ledger & Score Summary](#12-gamification-ledger--score-summary)
13. [Badges & Achievement Criteria](#13-badges--achievement-criteria)
14. [Notifications Inbox](#14-notifications-inbox)
15. [Platform Administration & Recovery](#15-platform-administration--recovery)

---

### 1. Authentication & Session Tokens

#### Register New Student
- **Route:** `POST /api/v1/auth/register`
- **Access:** `🌐 Public`
- **Request Body:**
  ```json
  {
    "email": "student@cu.edu.eg",
    "password": "Password123!",
    "firstName": "Omar",
    "lastName": "Hassan",
    "facultyId": 1,
    "academicYear": 3
  }
  ```
- **Response (201 Created):**
  ```json
  {
    "success": true,
    "message": "Student registered successfully.",
    "statusCode": 201,
    "data": {
      "studentId": 105,
      "email": "student@cu.edu.eg",
      "accessToken": "eyJhbGciOiJIUzI1Ni...",
      "refreshToken": "64_byte_crypto_string...",
      "expiresAt": "2026-11-06T01:30:00Z"
    }
  }
  ```

#### Authenticate & Login
- **Route:** `POST /api/v1/auth/login`
- **Access:** `🌐 Public`
- **Request Body:**
  ```json
  {
    "email": "student@cu.edu.eg",
    "password": "Password123!"
  }
  ```
- **Response (200 OK):**
  ```json
  {
    "success": true,
    "message": "Login successful.",
    "statusCode": 200,
    "data": {
      "accessToken": "eyJhbGciOiJIUzI1Ni...",
      "refreshToken": "64_byte_crypto_string...",
      "expiresAt": "2026-11-06T01:30:00Z"
    }
  }
  ```

#### Refresh Access Token
- **Route:** `POST /api/v1/auth/refresh`
- **Access:** `🌐 Public`
- **Request Body:**
  ```json
  {
    "refreshToken": "64_byte_crypto_string..."
  }
  ```
- **Response (200 OK):** Returns newly rotated access token and fresh refresh token.

---

### 2. Academic Catalog: Universities & Faculties

#### Cached University Catalog (Registration UX)
- **Route:** `GET /api/v1/universities/catalog`
- **Access:** `🌐 Public`
- **Response (200 OK):** Cached list of universities and nested faculties with ID and Name.

---

### 3. Student Directory & Profiles

#### Search Students Directory
- **Route:** `GET /api/v1/students?facultyId=1&academicYear=3&keyword=react&page=1&pageSize=20`
- **Access:** `🔒 Auth`
- **Response (200 OK):** Paginated `PagedResult<StudentCardDto>` matching criteria.

#### Get Public Profile Card
- **Route:** `GET /api/v1/students/{id}`
- **Access:** `🔒 Auth`
- **Response (200 OK):** Public portfolio, bio, claimed skills with proficiency levels, and unlocked badges.

---

### 4. Skills Master Catalog & Student Claims

#### Get Skills Catalog
- **Route:** `GET /api/v1/skills`
- **Access:** `🌐 Public`
- **Response (200 OK):** Master list of skills grouped by technical categories (Frontend, Backend, AI/ML, DevOps, UI/UX).

#### Claim / Update Skill Proficiency
- **Route:** `PUT /api/v1/students/me/skills/{skillId}`
- **Access:** `🔒 Auth`
- **Request Body:**
  ```json
  {
    "proficiency": 4
  }
  ```
- **Response (200 OK):** Updated student skill endorsement.

---

### 5. Projects & Team Formation

#### Create Project
- **Route:** `POST /api/v1/projects`
- **Access:** `🔒 Auth`
- **Request Body:**
  ```json
  {
    "title": "Smart Campus Navigator",
    "description": "Indoor positioning and turn-by-turn navigation for university buildings.",
    "maxMembers": 5,
    "requiredSkillIds": [1, 4, 12]
  }
  ```
- **Response (201 Created):** Project entity created with caller assigned as `ProjectRole.Owner`.

---

### 6. Project Applications

#### Apply to Join Project
- **Route:** `POST /api/v1/projects/{id}/applications`
- **Access:** `🔒 Auth`
- **Request Body:**
  ```json
  {
    "coverMessage": "I have 2 years of Flutter experience and want to contribute to the mobile UI."
  }
  ```
- **Response (201 Created):** `ApplicationID` assigned; state set to `Pending`.

#### Accept Application
- **Route:** `PATCH /api/v1/projects/{id}/applications/{appId}/accept`
- **Access:** `👤 Owner`
- **Response (200 OK):** Adds applicant to `ProjectMembers` roster and notifies applicant.

---

### 7. Project Sprint Tasks Board

#### Create Sprint Task
- **Route:** `POST /api/v1/projects/{id}/tasks`
- **Access:** `👥 Member`
- **Request Body:**
  ```json
  {
    "title": "Build Bluetooth beacon scanner service",
    "description": "Integrate cross-platform BLE scanning library.",
    "assignedStudentId": 105,
    "priority": "High"
  }
  ```
- **Response (201 Created):** Task added with `Status: Todo`.

#### Update Task Status
- **Route:** `PATCH /api/v1/projects/{id}/tasks/{taskId}/status`
- **Access:** `👥 Member`
- **Request Body:**
  ```json
  {
    "status": "Completed"
  }
  ```
- **Response (200 OK):** Updates task status; triggers `TaskCompletedDomainEvent` awarding gamification points.

---

### 8. Student Organizations & Leadership

#### List Student Organizations
- **Route:** `GET /api/v1/organizations`
- **Access:** `🌐 Public`
- **Response (200 OK):** Campus student activities, clubs, chapter leaders, and mission statements.

---

### 9. Membership Recruitment

#### Apply for Club Membership
- **Route:** `POST /api/v1/organizations/{id}/applications`
- **Access:** `🔒 Auth`
- **Request Body:**
  ```json
  {
    "targetCommittee": "Technical",
    "statement": "Eager to mentor junior students in web development."
  }
  ```
- **Response (201 Created):** Recruitment application recorded.

---

### 10. Campus Events & Sessions

#### Create Event with Sessions
- **Route:** `POST /api/v1/events`
- **Access:** `🛡️ OrgAdmin`
- **Request Body:**
  ```json
  {
    "title": "Cloud Native .NET 10 Hackathon",
    "description": "Full day hands-on building cloud-native microservices.",
    "venue": "Lab 304, Faculty of Computers",
    "capacity": 50,
    "startDate": "2026-11-20T09:00:00Z",
    "endDate": "2026-11-20T17:00:00Z"
  }
  ```
- **Response (201 Created):** Event created.

---

### 11. Event Registration & Attendance Check-In

#### Check-in Attendance (QR / PIN Code)
- **Route:** `POST /api/v1/events/{id}/sessions/{sessionId}/check-in`
- **Access:** `🔒 Auth`
- **Request Body:**
  ```json
  {
    "checkInPin": "782914"
  }
  ```
- **Response (200 OK):**
  ```json
  {
    "success": true,
    "message": "Attendance verified successfully.",
    "statusCode": 200,
    "data": {
      "sessionId": 12,
      "studentId": 105,
      "checkedInAt": "2026-11-20T09:15:22Z",
      "pointsAwarded": 50
    }
  }
  ```

---

### 12. Gamification Ledger & Score Summary

#### Student Score Summary
- **Route:** `GET /api/v1/students/{id}/score-summary`
- **Access:** `🔒 Auth`
- **Response (200 OK):**
  ```json
  {
    "success": true,
    "message": "Score summary retrieved.",
    "statusCode": 200,
    "data": {
      "studentId": 105,
      "totalPoints": 450,
      "level": 3,
      "xpToNextLevel": 50
    }
  }
  ```

---

### 13. Badges & Achievement Criteria

#### List Student Badges
- **Route:** `GET /api/v1/students/{id}/badges`
- **Access:** `🔒 Auth`
- **Response (200 OK):** Unlocked badges with icon URLs, criteria, and unlocked timestamps.

---

### 14. Notifications Inbox

#### Get Notifications
- **Route:** `GET /api/v1/notifications?unreadOnly=true`
- **Access:** `🔒 Auth`
- **Response (200 OK):** Unread in-app alerts with deep-link navigation URLs.

---

### 15. Platform Administration & Recovery

#### Universal Soft-Delete Restore
- **Route:** `POST /api/v1/admin/restore/{entityType}/{id}`
- **Access:** `👑 PlatformAdmin`
- **Response (200 OK):** Restores soft-deleted entity by clearing `DeletedAt = NULL`.
