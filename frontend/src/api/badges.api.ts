/**
 * @date 2026-11-22
 * @purpose Badges catalog, student earned badges, and PlatformAdmin awards API client.
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { BadgeCriteriaType } from "./enums";
import type { ApiResponse } from "./types";

export interface BadgeDto {
  badgeId: number;
  name: string;
  description: string;
  iconUrl: string;
  criteriaType: BadgeCriteriaType;
  criteriaValue: number;
}

export interface StudentBadgeDto {
  studentId: number;
  badgeId: number;
  badgeName: string;
  description: string;
  iconUrl: string;
  criteriaType: BadgeCriteriaType;
  criteriaValue: number;
  awardedAt: string;
}

export interface CreateBadgeRequest {
  name: string;
  description: string;
  iconUrl: string;
  criteriaType: BadgeCriteriaType;
  criteriaValue: number;
}

export interface UpdateBadgeRequest {
  name?: string;
  description?: string;
  iconUrl?: string;
  criteriaType?: BadgeCriteriaType;
  criteriaValue?: number;
}

export const badgesApi = {
  async getBadgeCatalog(signal?: AbortSignal): Promise<BadgeDto[]> {
    const res = await apiClient.get<ApiResponse<BadgeDto[]>>(
      API_ENDPOINTS.gamification.badges,
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async getStudentBadges(studentId: number, signal?: AbortSignal): Promise<StudentBadgeDto[]> {
    const res = await apiClient.get<ApiResponse<StudentBadgeDto[]>>(
      API_ENDPOINTS.students.badges(studentId),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async createBadge(data: CreateBadgeRequest): Promise<BadgeDto> {
    const res = await apiClient.post<ApiResponse<BadgeDto>>(
      API_ENDPOINTS.gamification.badges,
      data,
    );
    return unwrapApiResponse(res);
  },

  async updateBadge(id: number, data: UpdateBadgeRequest): Promise<BadgeDto> {
    const res = await apiClient.put<ApiResponse<BadgeDto>>(
      API_ENDPOINTS.gamification.badgeById(id),
      data,
    );
    return unwrapApiResponse(res);
  },

  async deleteBadge(id: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.gamification.badgeById(id));
    unwrapVoidResponse(res);
  },

  async manualAwardBadge(studentId: number, badgeId: number): Promise<StudentBadgeDto> {
    const res = await apiClient.post<ApiResponse<StudentBadgeDto>>(
      API_ENDPOINTS.students.manualAwardBadge(studentId, badgeId),
    );
    return unwrapApiResponse(res);
  },
};

