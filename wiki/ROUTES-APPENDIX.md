# Appendix — Full API Route List (99 routes)

> Complete audit tracking matrix for all 99 RESTful routes in TeamUp, verified against `TeamUp.Api` controllers and `docs/api/API-PROPOSAL.md`.

| # | Method | URI | Controller Action | Middleware / Auth | Module | Owner | Status |
|---|---|---|---|---|---|---|---|
| 1 | GET | `/health` | MinimalApi / HealthCheck | 🌐 Public | System | Tyson | ✅ Done |
| 2 | POST | `/api/v1/auth/register` | `AuthController.Register` | 🌐 Public (rate-limited) | M1: Auth | Tyson | ✅ Done |
| 3 | POST | `/api/v1/auth/login` | `AuthController.Login` | 🌐 Public (rate-limited) | M1: Auth | Tyson | ✅ Done |
| 4 | POST | `/api/v1/auth/refresh` | `AuthController.Refresh` | 🌐 Public (rate-limited) | M1: Auth | Tyson | ✅ Done |
| 5 | POST | `/api/v1/auth/logout` | `AuthController.Logout` | 🔒 Auth | M1: Auth | Tyson | ✅ Done |
| 6 | POST | `/api/v1/auth/forgot-password` | `AuthController.ForgotPassword` | 🌐 Public (rate-limited) | M1: Auth | Tyson | ✅ Done |
| 7 | POST | `/api/v1/auth/reset-password` | `AuthController.ResetPassword` | 🌐 Public (rate-limited) | M1: Auth | Tyson | ✅ Done |
| 8 | GET | `/api/v1/universities` | `UniversitiesController.GetUniversities` | 🔒 Auth | M1: Catalog | Tyson | ✅ Done |
| 9 | POST | `/api/v1/universities` | `UniversitiesController.CreateUniversity` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 10 | GET | `/api/v1/universities/{id}` | `UniversitiesController.GetUniversityById` | 🔒 Auth | M1: Catalog | Tyson | ✅ Done |
| 11 | PUT | `/api/v1/universities/{id}` | `UniversitiesController.UpdateUniversity` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 12 | DELETE | `/api/v1/universities/{id}` | `UniversitiesController.DeleteUniversity` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 13 | GET | `/api/v1/universities/{id}/faculties` | `FacultiesController.GetFacultiesByUniversity` | 🔒 Auth | M1: Catalog | Tyson | ✅ Done |
| 14 | POST | `/api/v1/universities/{id}/faculties` | `FacultiesController.CreateFaculty` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 15 | PUT | `/api/v1/faculties/{id}` | `FacultiesController.UpdateFaculty` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 16 | DELETE | `/api/v1/faculties/{id}` | `FacultiesController.DeleteFaculty` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 17 | GET | `/api/v1/students` | `StudentsController.SearchStudents` | 🔒 Auth | M1: Students | Tyson | ✅ Done |
| 18 | GET | `/api/v1/students/{id}` | `StudentsController.GetStudentCard` | 🔒 Auth | M1: Students | Tyson | ✅ Done |
| 19 | GET | `/api/v1/students/me` | `StudentsController.GetMyAccount` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 20 | PATCH | `/api/v1/students/me` | `StudentsController.UpdateMyAccount` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 21 | DELETE | `/api/v1/students/me` | `StudentsController.DeleteMyAccount` | 👤 Owner / 👑 Admin | M1: Students | Tyson | ✅ Done |
| 22 | GET | `/api/v1/students/{id}/profile` | `StudentProfilesController.GetProfile` | 🔒 Auth | M1: Students | Tyson | ✅ Done |
| 23 | PUT | `/api/v1/students/me/profile` | `StudentProfilesController.UpsertMyProfile` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 24 | GET | `/api/v1/students/{id}/interests` | `StudentInterestsController.GetInterests` | 🔒 Auth | M1: Students | Tyson | ✅ Done |
| 25 | POST | `/api/v1/students/me/interests` | `StudentInterestsController.AddInterest` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 26 | DELETE | `/api/v1/students/me/interests/{tag}` | `StudentInterestsController.RemoveInterest` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 27 | GET | `/api/v1/skills` | `SkillsController.GetSkills` | 🔒 Auth | M2: Skills | Ossama | ✅ Done |
| 28 | POST | `/api/v1/skills` | `SkillsController.CreateSkill` | 👑 PlatformAdmin | M2: Skills | Ossama | ✅ Done |
| 29 | PUT | `/api/v1/skills/{id}` | `SkillsController.UpdateSkill` | 👑 PlatformAdmin | M2: Skills | Ossama | ✅ Done |
| 30 | DELETE | `/api/v1/skills/{id}` | `SkillsController.DeleteSkill` | 👑 PlatformAdmin | M2: Skills | Ossama | ✅ Done |
| 31 | GET | `/api/v1/students/{id}/skills` | `StudentSkillsController.GetStudentSkills` | 🔒 Auth | M2: Skills | Ossama | ✅ Done |
| 32 | PUT | `/api/v1/students/me/skills/{skillId}` | `StudentSkillsController.UpsertSkillClaim` | 👤 Owner | M2: Skills | Ossama | ✅ Done |
| 33 | DELETE | `/api/v1/students/me/skills/{skillId}` | `StudentSkillsController.RemoveSkillClaim` | 👤 Owner | M2: Skills | Ossama | ✅ Done |
| 34 | GET | `/api/v1/projects` | `ProjectsController.SearchProjects` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 35 | POST | `/api/v1/projects` | `ProjectsController.CreateProject` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 36 | GET | `/api/v1/projects/{id}` | `ProjectsController.GetProjectById` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 37 | PATCH | `/api/v1/projects/{id}` | `ProjectsController.UpdateProject` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 38 | DELETE | `/api/v1/projects/{id}` | `ProjectsController.DeleteProject` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 39 | GET | `/api/v1/projects/{id}/required-skills` | `ProjectsController.GetRequiredSkills` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 40 | PUT | `/api/v1/projects/{id}/required-skills/{skillId}` | `ProjectsController.UpsertRequiredSkill` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 41 | DELETE | `/api/v1/projects/{id}/required-skills/{skillId}` | `ProjectsController.RemoveRequiredSkill` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 42 | GET | `/api/v1/projects/{id}/members` | `ProjectsController.GetMembers` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 43 | PATCH | `/api/v1/projects/{id}/members/{studentId}` | `ProjectsController.UpdateMemberRole` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 44 | DELETE | `/api/v1/projects/{id}/members/{studentId}` | `ProjectsController.RemoveMember` | 👤 Creator / Self | M2: Projects | Ossama | ✅ Done |
| 45 | GET | `/api/v1/projects/{id}/applications` | `ProjectsController.GetApplications` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 46 | POST | `/api/v1/projects/{id}/applications` | `ProjectsController.SubmitApplication` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 47 | PATCH | `/api/v1/projects/{id}/applications/{appId}` | `ProjectsController.ReviewApplication` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 48 | DELETE | `/api/v1/projects/{id}/applications/{appId}` | `ProjectsController.WithdrawApplication` | 👤 Applicant | M2: Projects | Ossama | ✅ Done |
| 49 | GET | `/api/v1/projects/{id}/tasks` | `TasksController.GetProjectTasks` | 👥 Member | M2: Tasks | Ossama | ✅ Done |
| 50 | POST | `/api/v1/projects/{id}/tasks` | `TasksController.CreateTask` | 👥 Member | M2: Tasks | Ossama | ✅ Done |
| 51 | GET | `/api/v1/tasks/{id}` | `TasksController.GetTaskById` | 👥 Member | M2: Tasks | Ossama | ✅ Done |
| 52 | PATCH | `/api/v1/tasks/{id}` | `TasksController.UpdateTask` | 👥 Member | M2: Tasks | Ossama | ✅ Done |
| 53 | PUT | `/api/v1/tasks/{id}/assignee` | `TasksController.AssignTask` | 👤 Creator | M2: Tasks | Ossama | ✅ Done |
| 54 | DELETE | `/api/v1/tasks/{id}/assignee` | `TasksController.UnassignTask` | 👤 Creator | M2: Tasks | Ossama | ✅ Done |
| 55 | DELETE | `/api/v1/tasks/{id}` | `TasksController.DeleteTask` | 👤 Creator | M2: Tasks | Ossama | ✅ Done |
| 56 | GET | `/api/v1/organizations` | `OrganizationsController.GetOrganizations` | 🔒 Auth | M3: Orgs | Ibrahim | ✅ Done |
| 57 | POST | `/api/v1/organizations` | `OrganizationsController.CreateOrganization` | 👑 Admin / Verified | M3: Orgs | Ibrahim | ✅ Done |
| 58 | GET | `/api/v1/organizations/{id}` | `OrganizationsController.GetOrganizationById` | 🔒 Auth | M3: Orgs | Ibrahim | ✅ Done |
| 59 | PATCH | `/api/v1/organizations/{id}` | `OrganizationsController.UpdateOrganization` | 🛡️ OrgAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 60 | DELETE | `/api/v1/organizations/{id}` | `OrganizationsController.DeleteOrganization` | 👑 PlatformAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 61 | GET | `/api/v1/organizations/{id}/members` | `OrganizationsController.GetMembers` | 🔒 Auth | M3: Orgs | Ibrahim | ✅ Done |
| 62 | PATCH | `/api/v1/organizations/{id}/members/{studentId}` | `OrganizationsController.UpdateMemberRole` | 🛡️ OrgAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 63 | DELETE | `/api/v1/organizations/{id}/members/{studentId}` | `OrganizationsController.RemoveMember` | 🛡️ OrgAdmin / Self | M3: Orgs | Ibrahim | ✅ Done |
| 64 | GET | `/api/v1/organizations/{id}/applications` | `OrganizationsController.GetApplications` | 🛡️ OrgAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 65 | POST | `/api/v1/organizations/{id}/applications` | `OrganizationsController.SubmitApplication` | 🔒 Auth | M3: Orgs | Ibrahim | ✅ Done |
| 66 | PATCH | `/api/v1/organizations/{id}/applications/{appId}` | `OrganizationsController.ReviewApplication` | 🛡️ OrgAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 67 | DELETE | `/api/v1/organizations/{id}/applications/{appId}` | `OrganizationsController.WithdrawApplication` | 👤 Applicant | M3: Orgs | Ibrahim | ✅ Done |
| 68 | GET | `/api/v1/events` | `EventsController.GetEvents` | 🔒 Auth | M3: Events | Ibrahim | ✅ Done |
| 69 | POST | `/api/v1/events` | `EventsController.CreateEvent` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 70 | GET | `/api/v1/events/{id}` | `EventsController.GetEventById` | 🔒 Auth | M3: Events | Ibrahim | ✅ Done |
| 71 | PATCH | `/api/v1/events/{id}` | `EventsController.UpdateEvent` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 72 | DELETE | `/api/v1/events/{id}` | `EventsController.DeleteEvent` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 73 | GET | `/api/v1/events/{id}/sessions` | `EventSessionsController.GetSessions` | 🔒 Auth | M3: Events | Ibrahim | ✅ Done |
| 74 | POST | `/api/v1/events/{id}/sessions` | `EventSessionsController.CreateSession` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 75 | PUT | `/api/v1/sessions/{id}` | `EventSessionsController.UpdateSession` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 76 | DELETE | `/api/v1/sessions/{id}` | `EventSessionsController.DeleteSession` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 77 | GET | `/api/v1/events/{id}/registrations` | `EventRegistrationsController.GetRegistrations` | 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 78 | POST | `/api/v1/events/{id}/registrations` | `EventRegistrationsController.Register` | 🔒 Auth | M3: Events | Youssef | ✅ Done |
| 79 | DELETE | `/api/v1/events/{id}/registrations` | `EventRegistrationsController.CancelRegistration` | 👤 Self / 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 80 | GET | `/api/v1/sessions/{id}/attendance` | `SessionAttendanceController.GetAttendance` | 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 81 | POST | `/api/v1/sessions/{id}/attendance` | `SessionAttendanceController.CheckIn` | 🛡️ OrgAdmin / 🔒 Auth | M3: Events | Youssef | ✅ Done |
| 82 | DELETE | `/api/v1/sessions/{id}/attendance/{studentId}` | `SessionAttendanceController.RevokeAttendance` | 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 83 | GET | `/api/v1/students/{id}/score` | `GamificationController.GetStudentScore` | 🔒 Auth | M4: Gamification | Youssef | ✅ Done |
| 84 | GET | `/api/v1/students/{id}/score-transactions` | `GamificationController.GetTransactions` | 👤 Self / 👑 Admin | M4: Gamification | Youssef | ✅ Done |
| 85 | POST | `/api/v1/score-transactions` | `GamificationController.AwardScore` | 👑 PlatformAdmin | M4: Gamification | Youssef | ✅ Done |
| 86 | GET | `/api/v1/badges` | `BadgesController.GetBadges` | 🔒 Auth | M4: Badges | Youssef | ✅ Done |
| 87 | POST | `/api/v1/badges` | `BadgesController.CreateBadge` | 👑 PlatformAdmin | M4: Badges | Youssef | ✅ Done |
| 88 | PUT | `/api/v1/badges/{id}` | `BadgesController.UpdateBadge` | 👑 PlatformAdmin | M4: Badges | Youssef | ✅ Done |
| 89 | DELETE | `/api/v1/badges/{id}` | `BadgesController.DeleteBadge` | 👑 PlatformAdmin | M4: Badges | Youssef | ✅ Done |
| 90 | GET | `/api/v1/students/{id}/badges` | `BadgesController.GetStudentBadges` | 🔒 Auth | M4: Badges | Youssef | ✅ Done |
| 91 | POST | `/api/v1/students/{id}/badges/{badgeId}` | `BadgesController.AwardBadge` | 👑 PlatformAdmin | M4: Badges | Youssef | ✅ Done |
| 92 | GET | `/api/v1/notifications` | `NotificationsController.GetNotifications` | 👤 Owner | M4: Alerts | Youssef | ✅ Done |
| 93 | PATCH | `/api/v1/notifications/{id}` | `NotificationsController.MarkAsRead` | 👤 Owner | M4: Alerts | Youssef | ✅ Done |
| 94 | PATCH | `/api/v1/notifications/mark-all-read` | `NotificationsController.MarkAllAsRead` | 👤 Owner | M4: Alerts | Youssef | ✅ Done |
| 95 | DELETE | `/api/v1/notifications/{id}` | `NotificationsController.DeleteNotification` | 👤 Owner | M4: Alerts | Youssef | ✅ Done |
| 96 | GET | `/api/v1/admin/dashboard` | `AdminController.GetDashboard` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 97 | PATCH | `/api/v1/admin/students/{id}/role` | `AdminController.UpdateStudentRole` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 98 | GET | `/api/v1/admin/soft-deleted` | `AdminController.GetSoftDeletedRecords` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 99 | POST | `/api/v1/admin/restore/{entityType}/{id}` | `AdminController.RestoreEntity` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
