/**
 * Typed API wrappers and DTOs for project discovery, teams, required skills, and applications.
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type {
  ApplicationStatus,
  ProjectStatus,
  ProficiencyLevel,
} from "./enums";
import type { ApiResponse, PagedResult } from "./types";
import type { SkillDto } from "./skills.api";

export interface ProjectListItemDto {
  projectId: number;
  title: string;
  description: string;
  status: ProjectStatus;
  maxTeamSize: number;
  teamSize: number;
  creatorId: number;
  creatorName: string;
  createdAt: string;
  coverImageUrl?: string | null;
}

export interface ProjectMemberDto {
  studentId: number;
  firstName: string;
  lastName: string;
  role: string;
  joinedAt: string;
}

export interface ProjectSkillDto {
  skillId: number;
  name: string;
  category: string;
  requiredLevel: ProficiencyLevel;
  isMandatory: boolean;
}

export interface ProjectDetailDto extends ProjectListItemDto {
  members: ProjectMemberDto[];
  requiredSkills: ProjectSkillDto[];
}

export interface ProjectApplicationDto {
  applicationId: number;
  projectId: number;
  studentId: number;
  studentName: string;
  message: string | null;
  status: ApplicationStatus;
  appliedAt: string;
}

export interface ProjectListParams {
  keyword?: string;
  status?: ProjectStatus | "";
  skillId?: number;
  page?: number;
  pageSize?: number;
}

export interface CreateProjectPayload {
  title: string;
  description: string;
  maxTeamSize: number;
}

export interface UpdateProjectPayload extends Partial<CreateProjectPayload> {
  status?: ProjectStatus;
}

export interface RequiredSkillPayload {
  requiredLevel: ProficiencyLevel;
  isMandatory: boolean;
}

export const projectsApi = {
  async list(
    params: ProjectListParams = {},
    signal?: AbortSignal,
  ): Promise<PagedResult<ProjectListItemDto>> {
    const res = await apiClient.get<ApiResponse<PagedResult<ProjectListItemDto>>>(
      API_ENDPOINTS.projects.list,
      { params, signal },
    );
    return unwrapApiResponse(res);
  },

  async create(payload: CreateProjectPayload): Promise<ProjectDetailDto> {
    const res = await apiClient.post<ApiResponse<ProjectDetailDto>>(
      API_ENDPOINTS.projects.list,
      payload,
    );
    return unwrapApiResponse(res);
  },

  async getById(id: number, signal?: AbortSignal): Promise<ProjectDetailDto> {
    const res = await apiClient.get<ApiResponse<ProjectDetailDto>>(API_ENDPOINTS.projects.byId(id), {
      signal,
    });
    return unwrapApiResponse(res);
  },

  async update(id: number, payload: UpdateProjectPayload): Promise<ProjectDetailDto> {
    const res = await apiClient.patch<ApiResponse<ProjectDetailDto>>(
      API_ENDPOINTS.projects.byId(id),
      payload,
    );
    return unwrapApiResponse(res);
  },

  async remove(id: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.projects.byId(id));
    unwrapVoidResponse(res);
  },

  async getRequiredSkills(id: number, signal?: AbortSignal): Promise<ProjectSkillDto[]> {
    const res = await apiClient.get<ApiResponse<ProjectSkillDto[]>>(
      API_ENDPOINTS.projects.requiredSkills(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async setRequiredSkill(
    id: number,
    skillId: number,
    payload: RequiredSkillPayload,
  ): Promise<void> {
    const res = await apiClient.put(
      API_ENDPOINTS.projects.requiredSkill(id, skillId),
      payload,
    );
    unwrapVoidResponse(res);
  },

  async removeRequiredSkill(id: number, skillId: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.projects.requiredSkill(id, skillId));
    unwrapVoidResponse(res);
  },

  async getMembers(id: number, signal?: AbortSignal): Promise<ProjectMemberDto[]> {
    const res = await apiClient.get<ApiResponse<ProjectMemberDto[]>>(
      API_ENDPOINTS.projects.members(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async updateMemberRole(id: number, studentId: number, role: string): Promise<void> {
    const res = await apiClient.patch(
      API_ENDPOINTS.projects.member(id, studentId),
      { role },
    );
    unwrapVoidResponse(res);
  },

  async removeMember(id: number, studentId: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.projects.member(id, studentId));
    unwrapVoidResponse(res);
  },

  async getApplications(id: number, signal?: AbortSignal): Promise<ProjectApplicationDto[]> {
    const res = await apiClient.get<ApiResponse<ProjectApplicationDto[]>>(
      API_ENDPOINTS.projects.applications(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async getMyApplication(id: number, signal?: AbortSignal): Promise<ProjectApplicationDto | null> {
    const res = await apiClient.get<ApiResponse<ProjectApplicationDto | null>>(
      API_ENDPOINTS.projects.myApplication(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async apply(id: number, message: string): Promise<ProjectApplicationDto> {
    const res = await apiClient.post<ApiResponse<ProjectApplicationDto>>(
      API_ENDPOINTS.projects.applications(id),
      { message },
    );
    return unwrapApiResponse(res);
  },

  async updateApplication(
    id: number,
    appId: number,
    status: Extract<ApplicationStatus, "Accepted" | "Rejected">,
  ): Promise<void> {
    const res = await apiClient.patch(
      API_ENDPOINTS.projects.application(id, appId),
      { status },
    );
    unwrapVoidResponse(res);
  },

  async withdrawApplication(id: number, appId: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.projects.application(id, appId));
    unwrapVoidResponse(res);
  },

  async getSkills(signal?: AbortSignal): Promise<SkillDto[]> {
    const res = await apiClient.get<ApiResponse<SkillDto[]>>(API_ENDPOINTS.skills.list, {
      signal,
    });
    return unwrapApiResponse(res);
  },
};
