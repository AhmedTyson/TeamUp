/**
 * @date 2026-11-27
 * @purpose Typed client for all 9 composite single-request page endpoints (dashboards, explore, event/session/project/org workspaces, gamification hub, student portfolio).
 */
import { apiClient, unwrapApiResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { BadgeDto, StudentBadgeDto } from "./badges.api";
import type {
  EventRegistration,
  EventSession,
  EventSummary,
  MyEventRegistration,
} from "./events.api";
import type { ScoreTransactionDto, StudentScoreDto } from "./gamification.api";
import type { NotificationDto } from "./notifications.api";
import type {
  MembershipApplication,
  OrganizationDetail,
  OrganizationSummary,
} from "./organizations.api";
import type {
  ProjectApplicationDto,
  ProjectDetailDto,
  ProjectListItemDto,
} from "./projects.api";
import type { SkillDto, StudentSkillDto } from "./skills.api";
import type { StudentPublicProfileDto } from "./students.api";
import type { SessionAttendance } from "./attendance.api";
import type { ProjectTaskDto } from "./tasks.api";
import type { ApiResponse, PagedResult } from "./types";

export interface EventWorkspace {
  event: EventSummary;
  canManage: boolean;
  myRegistration: MyEventRegistration | null;
  registrations: EventRegistration[] | null;
}

export interface SessionAttendanceWorkspace {
  event: EventSummary;
  session: EventSession | null;
  canManage: boolean;
  registrations: EventRegistration[] | null;
  attendance: SessionAttendance[] | null;
}

export interface ProjectWorkspace {
  project: ProjectDetailDto;
  isCreator: boolean;
  isMember: boolean;
  applications: ProjectApplicationDto[] | null;
  myApplication: ProjectApplicationDto | null;
  tasks: ProjectTaskDto[] | null;
  mySkills: StudentSkillDto[];
  availableSkills: SkillDto[] | null;
}

export interface OrganizationWorkspace {
  organization: OrganizationDetail;
  canManage: boolean;
  canReviewRecruitment: boolean;
  isMember: boolean;
  adminApplications: MembershipApplication[] | null;
  myApplications: MembershipApplication[] | null;
}

export interface StudentDashboard {
  score: StudentScoreDto;
  myRegistrations: PagedResult<MyEventRegistration>;
  myApplications: MembershipApplication[];
  notifications: NotificationDto[];
  unreadNotificationsCount: number;
  recruitingProjects: PagedResult<ProjectListItemDto>;
}

export interface OrgAdminDashboard {
  managedOrganizations: OrganizationSummary[];
  activeOrganizationId: number | null;
  applications: MembershipApplication[];
  hostedEvents: PagedResult<EventSummary>;
}

export interface ExploreFeed {
  recruitingOrganizations: PagedResult<OrganizationSummary>;
  upcomingEvents: PagedResult<EventSummary>;
  projects: PagedResult<ProjectListItemDto>;
}

export interface GamificationHub {
  score: StudentScoreDto;
  badgeCatalog: BadgeDto[];
  earnedBadges: StudentBadgeDto[];
  ledger: PagedResult<ScoreTransactionDto>;
}

export interface StudentPortfolio {
  profile: StudentPublicProfileDto;
  skills: StudentSkillDto[];
  score: StudentScoreDto;
  badges: StudentBadgeDto[];
}

export const workspacesApi = {
  async eventWorkspace(id: number, signal?: AbortSignal): Promise<EventWorkspace> {
    const res = await apiClient.get<ApiResponse<EventWorkspace>>(
      API_ENDPOINTS.workspaces.eventWorkspace(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async sessionAttendanceWorkspace(
    eventId: number,
    sessionId: number,
    signal?: AbortSignal,
  ): Promise<SessionAttendanceWorkspace> {
    const res = await apiClient.get<ApiResponse<SessionAttendanceWorkspace>>(
      API_ENDPOINTS.workspaces.sessionAttendanceWorkspace(eventId, sessionId),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async projectWorkspace(id: number, signal?: AbortSignal): Promise<ProjectWorkspace> {
    const res = await apiClient.get<ApiResponse<ProjectWorkspace>>(
      API_ENDPOINTS.workspaces.projectWorkspace(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async organizationWorkspace(id: number, signal?: AbortSignal): Promise<OrganizationWorkspace> {
    const res = await apiClient.get<ApiResponse<OrganizationWorkspace>>(
      API_ENDPOINTS.workspaces.organizationWorkspace(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async studentDashboard(signal?: AbortSignal): Promise<StudentDashboard> {
    const res = await apiClient.get<ApiResponse<StudentDashboard>>(
      API_ENDPOINTS.workspaces.studentDashboard,
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async orgDashboard(
    organizationId?: number,
    signal?: AbortSignal,
  ): Promise<OrgAdminDashboard> {
    const res = await apiClient.get<ApiResponse<OrgAdminDashboard>>(
      API_ENDPOINTS.workspaces.orgDashboard,
      {
        params: organizationId ? { organizationId } : undefined,
        signal,
      },
    );
    return unwrapApiResponse(res);
  },

  async exploreFeed(pageSize: number, signal?: AbortSignal): Promise<ExploreFeed> {
    const res = await apiClient.get<ApiResponse<ExploreFeed>>(API_ENDPOINTS.workspaces.explore, {
      params: { pageSize },
      signal,
    });
    return unwrapApiResponse(res);
  },

  async gamificationHub(ledgerPageSize: number, signal?: AbortSignal): Promise<GamificationHub> {
    const res = await apiClient.get<ApiResponse<GamificationHub>>(
      API_ENDPOINTS.workspaces.gamificationHub,
      { params: { ledgerPageSize }, signal },
    );
    return unwrapApiResponse(res);
  },

  async studentPortfolio(id: number, signal?: AbortSignal): Promise<StudentPortfolio> {
    const res = await apiClient.get<ApiResponse<StudentPortfolio>>(
      API_ENDPOINTS.workspaces.studentPortfolio(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },
};
