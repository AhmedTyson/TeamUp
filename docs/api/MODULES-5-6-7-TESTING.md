# Modules 5, 6 and 7 — Test Guide

This guide covers Projects & Teams (Module 5), Project Applications (Module 6), and Project Tasks (Module 7).
For the implemented routes and request bodies, see [MODULES-5-6-7-IMPLEMENTATION.md](./MODULES-5-6-7-IMPLEMENTATION.md).

## Automated tests

Use the .NET 10 SDK required by the projects. From the repository root, run:

```powershell
dotnet restore TeamUp.sln
dotnet test TeamUp.sln --no-restore
```

The integration tests use an in-memory SQLite database and a test HTTP server; they do not need a running SQL Server.
`ProjectsTeamsTasksTests.ProjectApplicationAndTaskLifecycle_WorksEndToEnd` checks the main flow:

1. Register a project creator and an applicant.
2. Create a project and verify the creator is its `Team Lead`.
3. Apply to the project and accept the application.
4. Create a task as the accepted member and assign it.
5. Remove the member and verify their assignment is cleared and their task access is denied.
6. Soft-delete the task and verify it is no longer returned.

`ProjectsTeamsTasksTests.ProjectEndpoints_RequireAuthentication` verifies anonymous access is rejected.

## Manual API checklist

For manual testing, start the API with a valid database connection string and register two distinct student accounts. Send each account's access token in the `Authorization` header as `Bearer YOUR_ACCESS_TOKEN`. Send and receive JSON with `Content-Type: application/json`. Enum values are strings.

| Module | Request / action | Expected result |
|---|---|---|
| 5 | `POST /api/v1/projects` with `{"title":"Test project","description":"Testing modules 5-7","maxTeamSize":3}` | `201 Created`; creator appears as `Team Lead`. |
| 5 | `GET /api/v1/projects?keyword=Test&page=1&pageSize=20` | `200 OK`; matching project appears in the paged result. |
| 5 | `GET /api/v1/projects/{projectId}` | `200 OK`; project details include members and required skills. |
| 5 | `PATCH /api/v1/projects/{projectId}` with `{"title":"Updated test project"}` | `200 OK`; updated project is returned. |
| 5 | `GET /api/v1/projects/{projectId}/members` | `200 OK`; confirmed members and roles are returned. |
| 6 | As the second student, `POST /api/v1/projects/{projectId}/applications` with `{"message":"I would like to join."}` | `201 Created`; application starts as `Pending`. |
| 6 | As the creator, `GET /api/v1/projects/{projectId}/applications` | `200 OK`; the new application appears. |
| 6 | Before reviewing the application, repeat the applicant's `POST /api/v1/projects/{projectId}/applications` | Request is rejected; only one pending application per student/project is allowed. |
| 6 | As the creator, `PATCH /api/v1/projects/{projectId}/applications/{applicationId}` with `{"status":"Accepted"}` | `200 OK`; application is accepted and applicant becomes a project member. |
| 7 | As an active project member, `POST /api/v1/projects/{projectId}/tasks` with `{"title":"Test task","description":"Check the task flow","dueDate":"2026-11-20T18:00:00Z","priority":"High"}` | `201 Created`; task starts with status `Todo`. |
| 7 | `GET /api/v1/projects/{projectId}/tasks` and `GET /api/v1/tasks/{taskId}` | `200 OK`; task is visible on the board and by ID. |
| 7 | `PATCH /api/v1/tasks/{taskId}` with `{"status":"InProgress","priority":"Urgent"}` | `200 OK`; task fields are updated. |
| 7 | As the creator, `PUT /api/v1/tasks/{taskId}/assignee` with `{"studentId":<confirmedMemberId>}` | `204 No Content`; task is assigned to that member. |
| 7 | As the creator, `DELETE /api/v1/tasks/{taskId}/assignee` | `204 No Content`; task is unassigned. |
| 7 | As the creator, `DELETE /api/v1/tasks/{taskId}` then `GET /api/v1/tasks/{taskId}` | Delete returns `204 No Content`; subsequent read returns `404 Not Found`. |

Also verify authorization boundaries: unauthenticated project requests are rejected; only the creator can review applications, update or delete the project, and assign or delete tasks; task-board access requires active project membership. Use a project with spare team capacity when testing acceptance.
