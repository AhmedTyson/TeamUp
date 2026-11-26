# Appendix — Full API Route List

> Historical 116-route matrix preserved below alongside newly added platform endpoints. The authoritative current **OpenAPI 3.2** specification exposes **100 paths / 143 operations** plus `/health/live` and `/health/ready` in [`docs/api/openapi.json`](../docs/api/openapi.json).

| # | Method | URI | Controller Action | Middleware / Auth | Module | Owner | Status |
|---|---|---|---|---|---|---|---|
| 1 | GET | `/health` | MinimalApi (`Program.cs`) | 🌐 Public (rate-limit exempt) | System | Tyson | ✅ Done |
| 2 | POST | `/api/v1/auth/register` | `AuthController.Register` | 🌐 Public (`auth-policy`) | M1: Auth | Tyson | ✅ Done |
| 3 | POST | `/api/v1/auth/login` | `AuthController.Login` | 🌐 Public (`auth-policy`) | M1: Auth | Tyson | ✅ Done |
| 4 | POST | `/api/v1/auth/refresh` | `AuthController.Refresh` | 🌐 Public (`auth-policy`) | M1: Auth | Tyson | ✅ Done |
| 5 | POST | `/api/v1/auth/logout` | `AuthController.Logout` | 🔒 Auth (`auth-policy`) | M1: Auth | Tyson | ✅ Done |
| 6 | POST | `/api/v1/auth/forgot-password` | `AuthController.ForgotPassword` | 🌐 Public (`auth-policy`) | M1: Auth | Tyson | ✅ Done |
| 7 | POST | `/api/v1/auth/reset-password` | `AuthController.ResetPassword` | 🌐 Public (`auth-policy`) | M1: Auth | Tyson | ✅ Done |
| 8 | GET | `/api/v1/universities` | `UniversitiesController.GetUniversities` | 🌐 Public | M1: Catalog | Tyson | ✅ Done |
| 9 | GET | `/api/v1/universities/catalog` | `UniversitiesController.GetAcademicCatalog` | 🌐 Public (1h cache) | M1: Catalog | Tyson | ✅ Done |
| 10 | GET | `/api/v1/universities/{id}` | `UniversitiesController.GetUniversityById` | 🌐 Public | M1: Catalog | Tyson | ✅ Done |
| 11 | POST | `/api/v1/universities` | `UniversitiesController.CreateUniversity` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 12 | PUT | `/api/v1/universities/{id}` | `UniversitiesController.UpdateUniversity` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 13 | DELETE | `/api/v1/universities/{id}` | `UniversitiesController.DeleteUniversity` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 14 | GET | `/api/v1/universities/{universityId}/faculties` | `FacultiesController.GetFacultiesByUniversity` | 🌐 Public | M1: Catalog | Tyson | ✅ Done |
| 15 | POST | `/api/v1/universities/{universityId}/faculties` | `FacultiesController.CreateFaculty` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 16 | GET | `/api/v1/faculties/{id}` | `FacultiesController.GetFacultyById` | 🌐 Public | M1: Catalog | Tyson | ✅ Done |
| 17 | PUT | `/api/v1/faculties/{id}` | `FacultiesController.UpdateFaculty` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 18 | DELETE | `/api/v1/faculties/{id}` | `FacultiesController.DeleteFaculty` | 👑 PlatformAdmin | M1: Catalog | Tyson | ✅ Done |
| 19 | GET | `/api/v1/students/showcase` | `StudentsController.GetPublicShowcase` | 🌐 Public | M1: Students | Tyson | ✅ Done |
| 20 | GET | `/api/v1/students` | `StudentsController.GetStudentsDirectory` | 🔒 Auth (`directory-policy`) | M1: Students | Tyson | ✅ Done |
| 21 | GET | `/api/v1/students/{id}` | `StudentsController.GetStudentById` | 🔒 Auth | M1: Students | Tyson | ✅ Done |
| 22 | GET | `/api/v1/students/me` | `StudentsController.GetMe` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 23 | PATCH | `/api/v1/students/me` | `StudentsController.UpdateMe` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 24 | DELETE | `/api/v1/students/me` | `StudentsController.DeleteMe` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 25 | DELETE | `/api/v1/students/{id}` | `StudentsController.DeleteStudent` | 👑 PlatformAdmin | M1: Students | Tyson | ✅ Done |
| 26 | GET | `/api/v1/students/{id}/profile` | `StudentProfilesController.GetProfile` | 🔒 Auth | M1: Students | Tyson | ✅ Done |
| 27 | PUT | `/api/v1/students/me/profile` | `StudentProfilesController.UpsertMyProfile` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 28 | POST | `/api/v1/students/me/profile/photo` | `StudentProfilesController.UploadMyProfilePhoto` | 👤 Owner (3 MB limit) | M1: Students | Tyson | ✅ Done |
| 29 | GET | `/api/v1/students/{id}/interests` | `StudentInterestsController.GetInterests` | 🔒 Auth | M1: Students | Tyson | ✅ Done |
| 30 | POST | `/api/v1/students/me/interests` | `StudentInterestsController.AddInterest` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 31 | DELETE | `/api/v1/students/me/interests/{interest}` | `StudentInterestsController.RemoveInterest` | 👤 Owner | M1: Students | Tyson | ✅ Done |
| 32 | GET | `/api/v1/skills` | `SkillsController.GetSkills` | 🔒 Auth (1h cache) | M2: Skills | Ossama | ✅ Done |
| 33 | GET | `/api/v1/skills/{id}` | `SkillsController.GetSkillById` | 🔒 Auth | M2: Skills | Ossama | ✅ Done |
| 34 | POST | `/api/v1/skills` | `SkillsController.CreateSkill` | 👑 PlatformAdmin | M2: Skills | Ossama | ✅ Done |
| 35 | PUT | `/api/v1/skills/{id}` | `SkillsController.UpdateSkill` | 👑 PlatformAdmin | M2: Skills | Ossama | ✅ Done |
| 36 | DELETE | `/api/v1/skills/{id}` | `SkillsController.DeleteSkill` | 👑 PlatformAdmin | M2: Skills | Ossama | ✅ Done |
| 37 | GET | `/api/v1/students/{id}/skills` | `StudentSkillsController.GetStudentSkills` | 🔒 Auth | M2: Skills | Ossama | ✅ Done |
| 38 | PUT | `/api/v1/students/me/skills/{skillId}` | `StudentSkillsController.ClaimSkill` | 👤 Owner | M2: Skills | Ossama | ✅ Done |
| 39 | DELETE | `/api/v1/students/me/skills/{skillId}` | `StudentSkillsController.RemoveClaimedSkill` | 👤 Owner | M2: Skills | Ossama | ✅ Done |
| 40 | GET | `/api/v1/projects` | `ProjectsController.GetProjects` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 41 | POST | `/api/v1/projects` | `ProjectsController.CreateProject` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 42 | GET | `/api/v1/projects/{id}` | `ProjectsController.GetProject` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 43 | PATCH | `/api/v1/projects/{id}` | `ProjectsController.UpdateProject` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 44 | DELETE | `/api/v1/projects/{id}` | `ProjectsController.DeleteProject` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 45 | GET | `/api/v1/projects/{id}/required-skills` | `ProjectsController.GetRequiredSkills` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 46 | PUT | `/api/v1/projects/{id}/required-skills/{skillId}` | `ProjectsController.UpsertRequiredSkill` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 47 | DELETE | `/api/v1/projects/{id}/required-skills/{skillId}` | `ProjectsController.RemoveRequiredSkill` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 48 | GET | `/api/v1/projects/{id}/members` | `ProjectsController.GetMembers` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 49 | PATCH | `/api/v1/projects/{id}/members/{studentId}` | `ProjectsController.UpdateMemberRole` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 50 | DELETE | `/api/v1/projects/{id}/members/{studentId}` | `ProjectsController.RemoveMember` | 👤 Creator / Self | M2: Projects | Ossama | ✅ Done |
| 51 | GET | `/api/v1/projects/{id}/applications` | `ProjectsController.GetApplications` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 52 | POST | `/api/v1/projects/{id}/applications` | `ProjectsController.Apply` | 🔒 Auth | M2: Projects | Ossama | ✅ Done |
| 53 | PATCH | `/api/v1/projects/{id}/applications/{appId}` | `ProjectsController.ReviewApplication` | 👤 Creator | M2: Projects | Ossama | ✅ Done |
| 54 | DELETE | `/api/v1/projects/{id}/applications/{appId}` | `ProjectsController.WithdrawApplication` | 👤 Applicant | M2: Projects | Ossama | ✅ Done |
| 55 | GET | `/api/v1/projects/{id}/tasks` | `ProjectsController.GetTasks` | 👥 Member | M2: Tasks | Ossama | ✅ Done |
| 56 | POST | `/api/v1/projects/{id}/tasks` | `ProjectsController.CreateTask` | 👥 Member | M2: Tasks | Ossama | ✅ Done |
| 57 | GET | `/api/v1/tasks/{id}` | `TasksController.GetTask` | 👥 Member | M2: Tasks | Ossama | ✅ Done |
| 58 | PATCH | `/api/v1/tasks/{id}` | `TasksController.UpdateTask` | 👥 Member | M2: Tasks | Ossama | ✅ Done |
| 59 | PUT | `/api/v1/tasks/{id}/assignee` | `TasksController.AssignTask` | 👤 Creator | M2: Tasks | Ossama | ✅ Done |
| 60 | DELETE | `/api/v1/tasks/{id}/assignee` | `TasksController.UnassignTask` | 👤 Creator | M2: Tasks | Ossama | ✅ Done |
| 61 | DELETE | `/api/v1/tasks/{id}` | `TasksController.DeleteTask` | 👤 Creator | M2: Tasks | Ossama | ✅ Done |
| 62 | GET | `/api/v1/organizations` | `OrganizationsController.GetOrganizations` | 🔒 Auth | M3: Orgs | Ibrahim | ✅ Done |
| 63 | POST | `/api/v1/organizations` | `OrganizationsController.CreateOrganization` | 🛡️ OrgAdmin / 👑 PlatformAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 64 | GET | `/api/v1/organizations/{id}` | `OrganizationsController.GetOrganizationById` | 🔒 Auth | M3: Orgs | Ibrahim | ✅ Done |
| 65 | PATCH | `/api/v1/organizations/{id}` | `OrganizationsController.UpdateOrganization` | 🛡️ OrgAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 66 | DELETE | `/api/v1/organizations/{id}` | `OrganizationsController.DeleteOrganization` | 👑 PlatformAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 67 | GET | `/api/v1/organizations/{id}/members` | `OrganizationsController.GetMembers` | 🔒 Auth | M3: Orgs | Ibrahim | ✅ Done |
| 68 | PATCH | `/api/v1/organizations/{id}/members/{studentId}` | `OrganizationsController.UpdateMemberRole` | 🛡️ OrgAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 69 | DELETE | `/api/v1/organizations/{id}/members/{studentId}` | `OrganizationsController.RemoveMember` | 🛡️ OrgAdmin / Self | M3: Orgs | Ibrahim | ✅ Done |
| 70 | GET | `/api/v1/organizations/{id}/applications` | `OrganizationsController.GetApplications` | 🛡️ OrgAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 71 | POST | `/api/v1/organizations/{id}/applications` | `OrganizationsController.ApplyForMembership` | 🔒 Auth | M3: Orgs | Ibrahim | ✅ Done |
| 72 | PATCH | `/api/v1/organizations/{id}/applications/{appId}` | `OrganizationsController.ReviewApplication` | 🛡️ OrgAdmin | M3: Orgs | Ibrahim | ✅ Done |
| 73 | DELETE | `/api/v1/organizations/{id}/applications/{appId}` | `OrganizationsController.WithdrawApplication` | 👤 Applicant | M3: Orgs | Ibrahim | ✅ Done |
| 74 | GET | `/api/v1/events` | `EventsController.GetEvents` | 🔒 Auth | M3: Events | Ibrahim | ✅ Done |
| 75 | POST | `/api/v1/events` | `EventsController.CreateEvent` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 76 | GET | `/api/v1/events/{id}` | `EventsController.GetEventById` | 🔒 Auth | M3: Events | Ibrahim | ✅ Done |
| 77 | PATCH | `/api/v1/events/{id}` | `EventsController.UpdateEvent` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 78 | DELETE | `/api/v1/events/{id}` | `EventsController.DeleteEvent` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 79 | GET | `/api/v1/events/{id}/sessions` | `EventsController.GetSessions` | 🔒 Auth | M3: Events | Ibrahim | ✅ Done |
| 80 | POST | `/api/v1/events/{id}/sessions` | `EventsController.CreateSession` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 81 | PUT | `/api/v1/sessions/{id}` | `EventSessionsController.UpdateSession` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 82 | DELETE | `/api/v1/sessions/{id}` | `EventSessionsController.DeleteSession` | 🛡️ OrgAdmin | M3: Events | Ibrahim | ✅ Done |
| 83 | GET | `/api/v1/events/{id}/registrations` | `EventRegistrationsController.GetRegistrations` | 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 84 | POST | `/api/v1/events/{id}/registrations` | `EventRegistrationsController.Register` | 🔒 Auth | M3: Events | Youssef | ✅ Done |
| 85 | DELETE | `/api/v1/events/{id}/registrations` | `EventRegistrationsController.Cancel` | 👤 Self / 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 86 | GET | `/api/v1/sessions/{id}/attendance` | `SessionAttendanceController.GetAttendance` | 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 87 | POST | `/api/v1/sessions/{id}/attendance/check-in-code` | `SessionAttendanceController.GenerateCheckInCode` | 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 88 | POST | `/api/v1/sessions/{id}/attendance` | `SessionAttendanceController.CheckIn` | 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 89 | POST | `/api/v1/sessions/{id}/attendance/self-check-in` | `SessionAttendanceController.SelfCheckIn` | 🔒 Auth (60s rotating code) | M3: Events | Youssef | ✅ Done |
| 90 | DELETE | `/api/v1/sessions/{id}/attendance/{studentId}` | `SessionAttendanceController.Revoke` | 🛡️ OrgAdmin | M3: Events | Youssef | ✅ Done |
| 91 | GET | `/api/v1/sessions/{id}/attendance/report` | `SessionAttendanceController.GetAttendanceReport` | 🛡️ OrgAdmin (`text/html`) | M3: Events | Youssef | ✅ Done |
| 92 | GET | `/api/v1/students/{id}/score` | `GamificationController.GetStudentScore` | 🔒 Auth | M4: Gamification | Youssef | ✅ Done |
| 93 | GET | `/api/v1/students/{id}/score-transactions` | `GamificationController.GetScoreTransactions` | 👤 Self / 👑 PlatformAdmin | M4: Gamification | Youssef | ✅ Done |
| 94 | POST | `/api/v1/score-transactions` | `GamificationController.AddScoreTransaction` | 👑 PlatformAdmin | M4: Gamification | Youssef | ✅ Done |
| 95 | GET | `/api/v1/gamification/leaderboard` | `GamificationController.GetLeaderboard` | 🔒 Auth | M4: Gamification | Youssef | ✅ Done |
| 96 | GET | `/api/v1/badges` | `BadgesController.GetBadgeCatalog` | 🔒 Auth (1h cache) | M4: Badges | Youssef | ✅ Done |
| 97 | POST | `/api/v1/badges` | `BadgesController.CreateBadge` | 👑 PlatformAdmin | M4: Badges | Youssef | ✅ Done |
| 98 | PUT | `/api/v1/badges/{id}` | `BadgesController.UpdateBadge` | 👑 PlatformAdmin | M4: Badges | Youssef | ✅ Done |
| 99 | DELETE | `/api/v1/badges/{id}` | `BadgesController.DeleteBadge` | 👑 PlatformAdmin | M4: Badges | Youssef | ✅ Done |
| 100 | GET | `/api/v1/students/{id}/badges` | `BadgesController.GetStudentBadges` | 🔒 Auth | M4: Badges | Youssef | ✅ Done |
| 101 | POST | `/api/v1/students/{id}/badges/{badgeId}` | `BadgesController.ManualAwardBadge` | 👑 PlatformAdmin | M4: Badges | Youssef | ✅ Done |
| 102 | GET | `/api/v1/notifications` | `NotificationsController.GetNotifications` | 👤 Owner | M4: Alerts | Youssef | ✅ Done |
| 103 | PATCH | `/api/v1/notifications/{id}` | `NotificationsController.MarkAsRead` | 👤 Owner | M4: Alerts | Youssef | ✅ Done |
| 104 | PATCH | `/api/v1/notifications/mark-all-read` | `NotificationsController.MarkAllAsRead` | 👤 Owner | M4: Alerts | Youssef | ✅ Done |
| 105 | DELETE | `/api/v1/notifications/{id}` | `NotificationsController.DeleteNotification` | 👤 Owner | M4: Alerts | Youssef | ✅ Done |
| 106 | GET | `/api/v1/admin/dashboard` | `AdminController.GetDashboard` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 107 | PATCH | `/api/v1/admin/students/{id}/role` | `AdminController.UpdateStudentRole` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 108 | GET | `/api/v1/admin/soft-deleted` | `AdminController.GetSoftDeleted` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 109 | POST | `/api/v1/admin/restore/{entityType}/{id}` | `AdminController.RestoreEntity` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 110 | GET | `/api/v1/admin/audit-logs` | `AdminController.GetAuditLogs` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 111 | GET | `/api/v1/admin/reports/platform-summary.pdf` | `AdminController.DownloadPlatformSummaryPdf` | 👑 PlatformAdmin (`application/pdf`) | M1: Admin | Tyson | ✅ Done |
| 112 | POST | `/api/v1/admin/reports/send-summary` | `AdminController.SendPlatformSummaryEmail` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 113 | GET | `/api/v1/admin/templates` | `AdminController.ListEmailTemplates` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 114 | GET | `/api/v1/admin/templates/preview/{templateName}` | `AdminController.PreviewEmailTemplate` | 👑 PlatformAdmin (`text/html`) | M1: Admin | Tyson | ✅ Done |
| 115 | POST | `/api/v1/admin/communications/leaderboard-digest` | `AdminController.DispatchLeaderboardDigest` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| 116 | POST | `/api/v1/admin/communications/project-matches` | `AdminController.DispatchProjectMatchDigest` | 👑 PlatformAdmin | M1: Admin | Tyson | ✅ Done |
| + | GET | `/health/live` | Health Check (`HealthCheckExtensions`) | 🌐 Public (rate-limit exempt) | System | Platform | ✅ Done |
| + | GET | `/health/ready` | Health Check (`HealthCheckExtensions`) | 🌐 Public (rate-limit exempt) | System | Platform | ✅ Done |
| + | POST | `/api/v1/images` | `ImagesController.UploadImage` | 🔒 Auth (`upload-policy`, 2 MB) | System | Platform | ✅ Done |
| + | GET | `/api/v1/images/{id}` | `ImagesController.GetImage` | 🌐 Public (`cross-origin`) | System | Platform | ✅ Done |
| + | GET | `/api/v1/organizations/public/categories` | `OrganizationsController.GetPublicOrganizationCategories` | 🌐 Public (OutputCache) | M3: Orgs | Platform | ✅ Done |
| + | GET | `/api/v1/notifications/unread-count` | `NotificationsController.GetUnreadCount` | 👤 Owner | M4: Alerts | Platform | ✅ Done |
