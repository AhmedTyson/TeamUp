# TeamUp — Modules 5, 6 and 7 API Test Report

> **Execution Timestamp:** 2026-11-04 07:27 UTC+02:00
> **Target Environment:** ASP.NET Core 10 Web API using `WebApplicationFactory` (in-process test server)
> **Database Engine:** SQLite in-memory (test database; no SQL Server connection used)
> **Test Project:** `tests/TeamUp.IntegrationTests/TeamUp.IntegrationTests.csproj`
> **Coverage:** Projects & Teams, Project Applications, and Project Tasks

---

## Execution Summary

**Command:**
```powershell
dotnet test tests\TeamUp.IntegrationTests\TeamUp.IntegrationTests.csproj --no-restore --filter FullyQualifiedName~ProjectsTeamsTasksTests --logger "console;verbosity=normal"
```

**Result:** **2 passed, 0 failed** — both module-specific integration tests passed.

The test host creates a fresh in-memory SQLite database and registers test accounts. The checks below report the HTTP status codes asserted by the integration tests. Response bodies are not captured as a live HTTP transcript by these tests; payloads shown in requests are the test inputs.

---

### Test Case: 01. Project, Application, Membership, and Task Lifecycle

**Test:** `ProjectApplicationAndTaskLifecycle_WorksEndToEnd`

#### 01.1 Register creator and applicant

The test registers two users through `POST /api/v1/auth/register` and uses their returned access tokens for the remaining requests.

**Response:** `201 Created` for each registration — **🟢 PASSED** (Expected: `201`)

#### 01.2 Create project

**Request:**
```http
POST /api/v1/projects
Authorization: Bearer <creator-access-token>
Content-Type: application/json

{
  "title": "TeamUp Test Project",
  "description": "Integration test project",
  "maxTeamSize": 2
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)

The test deserializes the project response, verifies the creator is the only initial member, and verifies the creator's role is `Team Lead`.

#### 01.3 Submit project application

**Request:**
```http
POST /api/v1/projects/{projectId}/applications
Authorization: Bearer <applicant-access-token>
Content-Type: application/json

{
  "message": "I would like to contribute."
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)

The test deserializes the created application and uses its application and student IDs in subsequent requests.

#### 01.4 Creator lists applications

**Request:**
```http
GET /api/v1/projects/{projectId}/applications
Authorization: Bearer <creator-access-token>
```

**Response:** `200 OK` — **🟢 PASSED**

The test also reads the response as JSON successfully.

#### 01.5 Creator accepts application

**Request:**
```http
PATCH /api/v1/projects/{projectId}/applications/{applicationId}
Authorization: Bearer <creator-access-token>
Content-Type: application/json

{
  "status": "Accepted"
}
```

**Response:** `200 OK` — **🟢 PASSED** (Expected: `200`)

#### 01.6 Confirm project membership

**Request:**
```http
GET /api/v1/projects/{projectId}/members
Authorization: Bearer <creator-access-token>
```

**Response:** `200 OK` — **🟢 PASSED**

The response is deserialized as a member list, and the test verifies that it contains the creator and the accepted applicant (two members).

#### 01.7 Accepted member creates a task

**Request:**
```http
POST /api/v1/projects/{projectId}/tasks
Authorization: Bearer <applicant-access-token>
Content-Type: application/json

{
  "title": "Implement API",
  "description": "Build the endpoint",
  "dueDate": "<UTC timestamp two days from test execution>"
}
```

**Response:** `201 Created` — **🟢 PASSED** (Expected: `201`)

The test deserializes the created task response.

#### 01.8 Creator assigns the task

**Request:**
```http
PUT /api/v1/tasks/{taskId}/assignee
Authorization: Bearer <creator-access-token>
Content-Type: application/json

{
  "studentId": <accepted-applicant-id>
}
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)

The creator then fetches the task and the test verifies that `assignedStudentId` matches the applicant.

#### 01.9 Remove member and clear assignment

**Request:**
```http
DELETE /api/v1/projects/{projectId}/members/{studentId}
Authorization: Bearer <creator-access-token>
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)

The creator fetches the task again; the test verifies `assignedStudentId` is now `null`.

#### 01.10 Removed member loses task access

**Request:**
```http
GET /api/v1/tasks/{taskId}
Authorization: Bearer <former-member-access-token>
```

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)

#### 01.11 Delete task and verify soft-delete visibility

**Request:**
```http
DELETE /api/v1/tasks/{taskId}
Authorization: Bearer <creator-access-token>
```

**Response:** `204 No Content` — **🟢 PASSED** (Expected: `204`)

Follow-up request:
```http
GET /api/v1/tasks/{taskId}
Authorization: Bearer <creator-access-token>
```

**Response:** `404 Not Found` — **🟢 PASSED** (Expected: `404`)

---

### Test Case: 02. Project Endpoints Require Authentication

**Test:** `ProjectEndpoints_RequireAuthentication`

**Request:**
```http
GET /api/v1/projects
```

No authorization header is supplied.

**Response:** `401 Unauthorized` — **🟢 PASSED** (Expected: `401`)

---

## Coverage Notes

The automated tests above exercise the primary project → application acceptance → task assignment → member removal → task soft-delete flow and an unauthenticated project request.

The following behaviors are **not covered by this test run** and need additional automated tests or manual API verification:

- Project search filters and pagination; project update and project soft-delete.
- Required-skill add, update, and removal.
- Project member role changes and self-removal.
- Application rejection, withdrawal, duplicate pending applications, team-capacity limits, and non-creator authorization.
- Task updates, unassignment, invalid assignees, project task-board listing, and non-creator task deletion.
- A live deployment against SQL Server.

For route definitions and request examples, see [MODULES-5-6-7-IMPLEMENTATION.md](./MODULES-5-6-7-IMPLEMENTATION.md) and [MODULES-5-6-7-TESTING.md](./MODULES-5-6-7-TESTING.md).
