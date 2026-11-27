// Date: 2026-11-27
// Purpose: Composite page-level payloads so screens load in a single HTTP request instead of multi-call waterfalls.

using TeamUp.Application.Common;
using TeamUp.Application.Events;
using TeamUp.Application.Gamification;
using TeamUp.Application.Organizations;
using TeamUp.Application.Projects;
using TeamUp.Application.Skills;
using TeamUp.Application.Students;

namespace TeamUp.Application.Workspaces;

#region Events & Attendance Workspaces

public sealed record EventWorkspaceDto(
    EventDto Event,
    bool CanManage,
    MyEventRegistrationDto? MyRegistration,
    IReadOnlyList<EventRegistrationDto>? Registrations);

public sealed record SessionAttendanceWorkspaceDto(
    EventDto Event,
    EventSessionDto? Session,
    bool CanManage,
    IReadOnlyList<EventRegistrationDto>? Registrations,
    IReadOnlyList<SessionAttendanceDto>? Attendance);

#endregion

#region Projects & Organizations Workspaces

public sealed record ProjectWorkspaceDto(
    ProjectDetailDto Project,
    bool IsCreator,
    bool IsMember,
    IReadOnlyList<ProjectApplicationDto>? Applications,
    ProjectApplicationDto? MyApplication,
    IReadOnlyList<ProjectTaskDto>? Tasks,
    IReadOnlyList<StudentSkillDto> MySkills,
    IReadOnlyList<SkillDto>? AvailableSkills);

public sealed record OrganizationWorkspaceDto(
    OrganizationDetailDto Organization,
    bool CanManage,
    bool CanReviewRecruitment,
    bool IsMember,
    IReadOnlyList<MembershipApplicationDto>? AdminApplications,
    IReadOnlyList<MembershipApplicationDto>? MyApplications);

#endregion

#region Dashboards, Explore, Gamification & Portfolio

public sealed record StudentDashboardDto(
    StudentScoreDto Score,
    PagedResult<MyEventRegistrationDto> MyRegistrations,
    IReadOnlyList<MembershipApplicationDto> MyApplications,
    IReadOnlyList<NotificationDto> Notifications,
    int UnreadNotificationsCount,
    PagedResult<ProjectListItemDto> RecruitingProjects);

public sealed record OrgAdminDashboardDto(
    IReadOnlyList<OrganizationSummaryDto> ManagedOrganizations,
    int? ActiveOrganizationId,
    IReadOnlyList<MembershipApplicationDto> Applications,
    PagedResult<EventDto> HostedEvents);

public sealed record ExploreFeedDto(
    PagedResult<OrganizationSummaryDto> RecruitingOrganizations,
    PagedResult<EventDto> UpcomingEvents,
    PagedResult<ProjectListItemDto> Projects);

public sealed record GamificationHubDto(
    StudentScoreDto Score,
    IReadOnlyList<BadgeDto> BadgeCatalog,
    IReadOnlyList<StudentBadgeDto> EarnedBadges,
    PagedResult<ScoreTransactionDto> Ledger);

public sealed record StudentPortfolioDto(
    StudentPublicProfileDto Profile,
    IReadOnlyList<StudentSkillDto> Skills,
    StudentScoreDto Score,
    IReadOnlyList<StudentBadgeDto> Badges);

#endregion
