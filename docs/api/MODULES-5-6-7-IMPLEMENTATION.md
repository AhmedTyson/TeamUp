# TeamUp — Modules 5, 6 and 7 Implementation Notes

This document records the implemented API surface for Projects & Teams, Project Applications, and Project Tasks.
All routes use the `/api/v1` prefix and require a valid JWT unless otherwise stated.
Enums are serialized as strings (for example, `"Recruiting"`, `"Accepted"`, `"InProgress"`).

## Module 5 — Projects & Teams

| Method | Route | Access rule | Result |
|---|---|---|---|
| GET | `/projects?keyword=&status=&skillId=&page=1&pageSize=20` | Authenticated | Paginated project list |
| POST | `/projects` | Authenticated | `201 Created`; creator becomes `Team Lead` |
| GET | `/projects/{id}` | Authenticated | Project, members, required skills |
| PATCH | `/projects/{id}` | Creator | Updated project |
| DELETE | `/projects/{id}` | Creator | `204`; soft-deletes project and its tasks |
| GET | `/projects/{id}/required-skills` | Authenticated | Required skill list |
| PUT | `/projects/{id}/required-skills/{skillId}` | Creator | Upsert/restore required skill |
| DELETE | `/projects/{id}/required-skills/{skillId}` | Creator | `204`; soft delete |
| GET | `/projects/{id}/members` | Authenticated | Confirmed members and roles |
| PATCH | `/projects/{id}/members/{studentId}` | Creator | Update member role |
| DELETE | `/projects/{id}/members/{studentId}` | Creator or that member | `204`; soft delete membership and unassign their tasks |

Create project body:

```json
{
  "title": "Campus Marketplace",
  "description": "A platform for students to exchange services.",
  "maxTeamSize": 5
}
```

Update project accepts any subset of `title`, `description`, `status`, and `maxTeamSize`.

Required skill body:

```json
{
  "requiredLevel": "Intermediate",
  "isMandatory": true
}
```

## Module 6 — Project Applications

| Method | Route | Access rule | Result |
|---|---|---|---|
| GET | `/projects/{id}/applications` | Creator | Application list |
| POST | `/projects/{id}/applications` | Authenticated student | `201 Created` |
| PATCH | `/projects/{id}/applications/{appId}` | Creator | Accept or reject |
| DELETE | `/projects/{id}/applications/{appId}` | Applicant | `204`; marks withdrawn and soft-deletes |

Application body:

```json
{
  "message": "I have experience with ASP.NET Core and SQL Server."
}
```

Review body:

```json
{
  "status": "Accepted"
}
```

Acceptance creates or restores the `ProjectMember` in the same database transaction, checks recruiting status and maximum team size, and assigns the default role `Member`. Rejected/withdrawn applications do not block a later application. Only one pending application per student/project is allowed.

## Module 7 — Project Tasks

| Method | Route | Access rule | Result |
|---|---|---|---|
| GET | `/projects/{id}/tasks` | Confirmed project member | Task board |
| POST | `/projects/{id}/tasks` | Confirmed project member | `201 Created` |
| GET | `/tasks/{id}` | Confirmed project member | Task details |
| PATCH | `/tasks/{id}` | Confirmed project member | Updated task |
| PUT | `/tasks/{id}/assignee` | Project creator | Assign to confirmed member |
| DELETE | `/tasks/{id}/assignee` | Project creator | Unassign |
| DELETE | `/tasks/{id}` | Project creator | `204`; soft delete |

Create task body:

```json
{
  "title": "Implement project endpoints",
  "description": "Add controller and service logic.",
  "dueDate": "2026-11-20T18:00:00Z",
  "priority": "High"
}
```

Update task accepts any subset of `title`, `description`, `status`, `priority`, and `dueDate`.

Assignment body:

```json
{
  "studentId": 12
}
```

Task status values: `Todo`, `InProgress`, `InReview`, `Done`.
Priority values: `Low`, `Medium`, `High`, `Urgent`.

## Shared behavior

- Deleted rows are hidden by the existing EF Core global soft-delete query filters.
- Project deletion soft-deletes its tasks in one transaction.
- Removing a member soft-deletes the membership and clears that student's task assignments in one transaction.
- Project creator-only actions are enforced in the service layer, not only in the UI/controller.
- Task board access requires an active `ProjectMember` record.
- Domain errors use the existing RFC 7807 exception handler (`400`, `401`, `404`, `409`).
- Integration coverage is added for the project → application → acceptance → task → assignment → member removal → soft-delete lifecycle.
