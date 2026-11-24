/**
 * @date 2026-11-24
 * @purpose Typed API client for student organizations, public previews, chapters/committees, 4-tier hierarchy, recruitment pipeline, participants, and CSV exports.
 */
import { apiClient, fetchApiCsv, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type {
  ApplicationStatus,
  EventDeliveryMode,
  OrganizationHierarchyTier,
  OrganizationScope,
  OrganizationUnitType,
  RecruitmentStage,
} from "./enums";
import type { ApiResponse, PagedResult } from "./types";

export interface OrganizationUnit {
  organizationUnitId: number;
  organizationId: number;
  name: string;
  code: string;
  description: string | null;
  unitType: OrganizationUnitType;
  isRecruiting: boolean;
  openSeats: number | null;
  memberCount: number;
}

export interface OrganizationSummary {
  organizationId: number;
  name: string;
  description: string;
  category: string;
  logoUrl: string | null;
  memberCount: number;
  scope?: OrganizationScope;
  universityId?: number | null;
  universityName?: string | null;
  city?: string | null;
  coverImageUrl?: string | null;
  websiteUrl?: string | null;
  isRecruiting?: boolean;
  currentSeasonName?: string | null;
  unitCount?: number;
}

export interface OrganizationMember {
  studentId: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  joinedAt: string;
  organizationUnitId?: number | null;
  unitName?: string | null;
  unitCode?: string | null;
  hierarchyTier?: OrganizationHierarchyTier;
}

export interface OrganizationEventSummary {
  eventId: number;
  title: string;
  type: string;
  location: string;
  startDate: string;
  endDate: string;
  deliveryMode?: EventDeliveryMode;
  isPublic?: boolean;
}

export interface OrganizationDetail {
  organizationId: number;
  name: string;
  description: string;
  category: string;
  logoUrl: string | null;
  members: OrganizationMember[];
  events: OrganizationEventSummary[];
  scope?: OrganizationScope;
  universityId?: number | null;
  universityName?: string | null;
  city?: string | null;
  coverImageUrl?: string | null;
  websiteUrl?: string | null;
  isRecruiting?: boolean;
  currentSeasonName?: string | null;
  units?: OrganizationUnit[];
}

export interface MembershipApplication {
  applicationId: number;
  organizationId: number;
  studentId: number;
  studentFirstName: string;
  studentLastName: string;
  studentEmail: string;
  message: string | null;
  status: ApplicationStatus;
  appliedAt: string;
  reviewedAt: string | null;
  organizationUnitId?: number | null;
  unitName?: string | null;
  unitCode?: string | null;
  seasonName?: string | null;
  stage?: RecruitmentStage;
  evaluationScore?: number | null;
  interviewNotes?: string | null;
  organizationName?: string | null;
}

export interface OrganizationHierarchy {
  organizationId: number;
  organizationName: string;
  scope: OrganizationScope;
  highBoard: OrganizationMember[];
  committeeLeadership: OrganizationMember[];
  organizingCrew: OrganizationMember[];
  units: OrganizationUnit[];
  participantsCount: number;
}

export interface OrganizationParticipant {
  studentId: number;
  firstName: string;
  lastName: string;
  affiliationLabel: string;
  eventsRegisteredCount: number;
  sessionsAttendedCount: number;
  lastEventTitle: string | null;
  lastActiveAt: string | null;
}

export interface OrganizationListParams extends Record<string, unknown> {
  search?: string;
  category?: string;
  scope?: OrganizationScope;
  universityId?: number;
  recruitingOnly?: boolean;
  page?: number;
  pageSize?: number;
}

export interface OrganizationPayload {
  name: string;
  description: string;
  category: string;
  logoUrl: string | null;
  scope?: OrganizationScope;
  universityId?: number | null;
  city?: string | null;
  coverImageUrl?: string | null;
  websiteUrl?: string | null;
  isRecruiting?: boolean;
  currentSeasonName?: string | null;
}

export interface OrganizationUnitPayload {
  name: string;
  code: string;
  description: string | null;
  unitType: OrganizationUnitType;
  isRecruiting: boolean;
  openSeats: number | null;
}

export interface OrganizationApplicationPayload {
  message: string | null;
  organizationUnitId?: number | null;
  seasonName?: string | null;
}

export interface UpdateMemberRolePayload {
  role: string;
  organizationUnitId?: number | null;
  hierarchyTier?: OrganizationHierarchyTier | null;
}

export interface UpdateApplicationStagePayload {
  stage: RecruitmentStage;
  evaluationScore?: number | null;
  interviewNotes?: string | null;
  organizationUnitId?: number | null;
}

export const organizationsApi = {
  async list(
    params: OrganizationListParams = {},
    signal?: AbortSignal,
  ): Promise<PagedResult<OrganizationSummary>> {
    const response = await apiClient.get<ApiResponse<PagedResult<OrganizationSummary>>>(
      API_ENDPOINTS.organizations.list,
      { params, signal },
    );
    return unwrapApiResponse(response);
  },

  async publicList(
    params: OrganizationListParams = {},
    signal?: AbortSignal,
  ): Promise<PagedResult<OrganizationSummary>> {
    const response = await apiClient.get<ApiResponse<PagedResult<OrganizationSummary>>>(
      API_ENDPOINTS.organizations.publicList,
      { params, signal },
    );
    return unwrapApiResponse(response);
  },

  async detail(id: number, signal?: AbortSignal): Promise<OrganizationDetail> {
    const response = await apiClient.get<ApiResponse<OrganizationDetail>>(
      API_ENDPOINTS.organizations.byId(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async publicDetail(id: number, signal?: AbortSignal): Promise<OrganizationDetail> {
    const response = await apiClient.get<ApiResponse<OrganizationDetail>>(
      API_ENDPOINTS.organizations.publicById(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async myApplications(signal?: AbortSignal): Promise<MembershipApplication[]> {
    const response = await apiClient.get<ApiResponse<MembershipApplication[]>>(
      API_ENDPOINTS.organizations.myApplications,
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async create(payload: OrganizationPayload): Promise<OrganizationDetail> {
    const response = await apiClient.post<ApiResponse<OrganizationDetail>>(
      API_ENDPOINTS.organizations.list,
      payload,
    );
    return unwrapApiResponse(response);
  },

  async update(id: number, payload: OrganizationPayload): Promise<OrganizationDetail> {
    const response = await apiClient.patch<ApiResponse<OrganizationDetail>>(
      API_ENDPOINTS.organizations.byId(id),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async remove(id: number): Promise<void> {
    const response = await apiClient.delete(API_ENDPOINTS.organizations.byId(id));
    unwrapVoidResponse(response);
  },

  async members(id: number, signal?: AbortSignal): Promise<OrganizationMember[]> {
    const response = await apiClient.get<ApiResponse<OrganizationMember[]>>(
      API_ENDPOINTS.organizations.members(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async updateMemberRole(
    id: number,
    studentId: number,
    roleOrPayload: string | UpdateMemberRolePayload,
  ): Promise<OrganizationMember> {
    const body = typeof roleOrPayload === "string" ? { role: roleOrPayload } : roleOrPayload;
    const response = await apiClient.patch<ApiResponse<OrganizationMember>>(
      API_ENDPOINTS.organizations.member(id, studentId),
      body,
    );
    return unwrapApiResponse(response);
  },

  async removeMember(id: number, studentId: number): Promise<void> {
    const response = await apiClient.delete(API_ENDPOINTS.organizations.member(id, studentId));
    unwrapVoidResponse(response);
  },

  async units(id: number, signal?: AbortSignal): Promise<OrganizationUnit[]> {
    const response = await apiClient.get<ApiResponse<OrganizationUnit[]>>(
      API_ENDPOINTS.organizations.units(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async createUnit(id: number, payload: OrganizationUnitPayload): Promise<OrganizationUnit> {
    const response = await apiClient.post<ApiResponse<OrganizationUnit>>(
      API_ENDPOINTS.organizations.units(id),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async updateUnit(
    id: number,
    unitId: number,
    payload: OrganizationUnitPayload,
  ): Promise<OrganizationUnit> {
    const response = await apiClient.patch<ApiResponse<OrganizationUnit>>(
      API_ENDPOINTS.organizations.unitById(id, unitId),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async removeUnit(id: number, unitId: number): Promise<void> {
    const response = await apiClient.delete(API_ENDPOINTS.organizations.unitById(id, unitId));
    unwrapVoidResponse(response);
  },

  async hierarchy(id: number, signal?: AbortSignal): Promise<OrganizationHierarchy> {
    const response = await apiClient.get<ApiResponse<OrganizationHierarchy>>(
      API_ENDPOINTS.organizations.hierarchy(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async publicHierarchy(id: number, signal?: AbortSignal): Promise<OrganizationHierarchy> {
    const response = await apiClient.get<ApiResponse<OrganizationHierarchy>>(
      API_ENDPOINTS.organizations.publicHierarchy(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async participants(id: number, signal?: AbortSignal): Promise<OrganizationParticipant[]> {
    const response = await apiClient.get<ApiResponse<OrganizationParticipant[]>>(
      API_ENDPOINTS.organizations.participants(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async applications(id: number, signal?: AbortSignal): Promise<MembershipApplication[]> {
    const response = await apiClient.get<ApiResponse<MembershipApplication[]>>(
      API_ENDPOINTS.organizations.applications(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async apply(
    id: number,
    payload: OrganizationApplicationPayload,
  ): Promise<MembershipApplication> {
    const response = await apiClient.post<ApiResponse<MembershipApplication>>(
      API_ENDPOINTS.organizations.applications(id),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async reviewApplication(
    id: number,
    appId: number,
    status: "Accepted" | "Rejected",
  ): Promise<MembershipApplication> {
    const response = await apiClient.patch<ApiResponse<MembershipApplication>>(
      API_ENDPOINTS.organizations.application(id, appId),
      { status },
    );
    return unwrapApiResponse(response);
  },

  async updateApplicationStage(
    id: number,
    appId: number,
    payload: UpdateApplicationStagePayload,
  ): Promise<MembershipApplication> {
    const response = await apiClient.patch<ApiResponse<MembershipApplication>>(
      API_ENDPOINTS.organizations.applicationStage(id, appId),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async withdrawApplication(id: number, appId: number): Promise<void> {
    const response = await apiClient.delete(API_ENDPOINTS.organizations.application(id, appId));
    unwrapVoidResponse(response);
  },

  async exportApplicationsCsv(id: number): Promise<string> {
    return fetchApiCsv(API_ENDPOINTS.organizations.applicationsCsv(id));
  },
};
