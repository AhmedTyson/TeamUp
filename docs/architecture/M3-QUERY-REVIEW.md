# Module 03 Query and Index Review

## Read-query conventions

All read endpoints added for Organizations, Membership Applications, Events, Sessions, Registrations, and Attendance use `AsNoTracking()` when materializing read-only data. Collection endpoints use stable ordering and bounded pagination where the API exposes list-wide browsing.

## Indexes added

| Table | Index | Query supported |
| --- | --- | --- |
| Organization | `(Category, Name)` filtered to active rows | Category-filtered organization browsing ordered by name |
| OrganizationMember | `(OrganizationId, Role, JoinedAt)` filtered to active rows | Organization member/board listing |
| MembershipApplication | `(OrganizationId, AppliedAt)` filtered to active rows | Organization recruitment inbox, newest applications first |
| Event | `(StartDate, Title)` filtered to active rows | Upcoming event listing and stable date/title ordering |
| Event | `(OrganizationId, StartDate)` filtered to active rows | Organization-specific event listing by date |
| EventRegistration | `(EventId, Status, RegisteredAt)` | Confirmed-capacity counts and waitlist promotion order |
| SessionAttendance | `(SessionId, CheckInTime)` filtered to active rows | Session attendance list ordered by check-in time |

Existing indexes are retained, including the unique active organization name, organization/session number uniqueness, and student lookup indexes.

## Notes

- `RegisteredCount` is calculated from registrations with `Status = Confirmed`; it is not stored as a mutable counter.
- Event registration is performed in a serializable transaction so concurrent requests cannot both consume the last confirmed seat.
- Cancelling a confirmed registration promotes the oldest waitlisted registration.
- Organization deletion soft-deletes its members, membership applications, events, and event sessions in the same transaction.
- Event deletion soft-deletes its sessions in the same transaction.
- `Contains` search over free-text names/descriptions is not expected to use ordinary B-tree indexes efficiently; a full-text search feature would be a separate enhancement.


Query-plan inspection against a running SQL Server instance remains a deployment-time verification step; the indexes above were selected from the actual LINQ filter/order patterns and still need `SET STATISTICS IO, TIME ON` / actual execution-plan review in the target database.
