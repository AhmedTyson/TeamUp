/**
 * Typed API wrappers for the master skill catalog and student skill claims.
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { ProficiencyLevel } from "./enums";
import type { ApiResponse } from "./types";

export interface SkillDto {
  skillId: number;
  name: string;
  category: string;
  description: string | null;
}

export interface StudentSkillDto {
  skillId: number;
  name: string;
  category: string;
  proficiencyLevel: ProficiencyLevel;
  yearsOfExperience: number;
}

export interface SkillQueryParams {
  category?: string;
  search?: string;
}

export interface SkillPayload {
  name: string;
  category: string;
  description: string | null;
}

export interface StudentSkillPayload {
  proficiencyLevel: ProficiencyLevel;
  yearsOfExperience: number;
}

export const skillsApi = {
  async list(params: SkillQueryParams = {}, signal?: AbortSignal): Promise<SkillDto[]> {
    const res = await apiClient.get<ApiResponse<SkillDto[]>>(API_ENDPOINTS.skills.list, {
      params,
      signal,
    });
    return unwrapApiResponse(res);
  },

  async getById(id: number, signal?: AbortSignal): Promise<SkillDto> {
    const res = await apiClient.get<ApiResponse<SkillDto>>(API_ENDPOINTS.skills.byId(id), {
      signal,
    });
    return unwrapApiResponse(res);
  },

  async create(payload: SkillPayload): Promise<SkillDto> {
    const res = await apiClient.post<ApiResponse<SkillDto>>(API_ENDPOINTS.skills.list, payload);
    return unwrapApiResponse(res);
  },

  async update(id: number, payload: SkillPayload): Promise<SkillDto> {
    const res = await apiClient.put<ApiResponse<SkillDto>>(API_ENDPOINTS.skills.byId(id), payload);
    return unwrapApiResponse(res);
  },

  async remove(id: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.skills.byId(id));
    unwrapVoidResponse(res);
  },

  async getStudentSkills(studentId: number, signal?: AbortSignal): Promise<StudentSkillDto[]> {
    const res = await apiClient.get<ApiResponse<StudentSkillDto[]>>(
      API_ENDPOINTS.students.skills(studentId),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async claimMine(skillId: number, payload: StudentSkillPayload): Promise<void> {
    const res = await apiClient.put(API_ENDPOINTS.students.mySkill(skillId), payload);
    unwrapVoidResponse(res);
  },

  async removeMine(skillId: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.students.mySkill(skillId));
    unwrapVoidResponse(res);
  },
};
