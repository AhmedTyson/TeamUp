/**
 * @date 2026-11-22
 * @purpose PlatformAdmin governance API client covering dashboard KPIs, role management, 8-entity restore, immutable audit logs, and directory lookup.
 */
import type { AuthStudentSummary } from "./auth.api";
import { apiClient, unwrapApiResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { RestorableEntityType, UserRole } from "./enums";
import type { ApiResponse, PagedResult } from "./types";

export interface AdminDashboardStats {
  activeStudentsCount: number;
  activeUniversitiesCount: number;
  activeFacultiesCount: number;
  softDeletedStudentsCount: number;
  softDeletedUniversitiesCount: number;
  softDeletedFacultiesCount: number;
  roleDistribution: Record<string, number>;
}

export interface UpdateUserRolePayload {
  role: UserRole;
}

export interface SoftDeletedItem {
  id: number;
  entityType: string;
  displayName: string;
  deletedAt: string;
}

export interface AuditLogItem {
  auditLogId: number;
  actorStudentId: number | null;
  actorEmail: string | null;
  action: string;
  entityType: string;
  entityId: string;
  oldValues: string | null;
  newValues: string | null;
  clientIp: string | null;
  occurredAt: string;
}

export interface AuditLogQueryParams {
  action?: string;
  entityType?: string;
  actorStudentId?: number;
  page?: number;
  pageSize?: number;
}

export interface AdminStudentDirectoryItem {
  studentId: number;
  firstName: string;
  lastName: string;
  email: string | null;
  academicYear: number;
  role: UserRole;
  facultyId: number;
  facultyName: string;
  universityName: string;
  interests: string[];
}

export interface AdminStudentSearchParams {
  search?: string;
  facultyId?: number;
  academicYear?: number;
  page?: number;
  pageSize?: number;
}

export const adminApi = {
  async getDashboard(signal?: AbortSignal): Promise<AdminDashboardStats> {
    const res = await apiClient.get<ApiResponse<AdminDashboardStats>>(
      API_ENDPOINTS.admin.dashboard,
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async updateStudentRole(
    studentId: number,
    payload: UpdateUserRolePayload,
  ): Promise<AuthStudentSummary> {
    const res = await apiClient.patch<ApiResponse<AuthStudentSummary>>(
      API_ENDPOINTS.admin.studentRole(studentId),
      payload,
    );
    return unwrapApiResponse(res);
  },

  async getSoftDeleted(
    entityType?: RestorableEntityType | string,
    signal?: AbortSignal,
  ): Promise<SoftDeletedItem[]> {
    const params = entityType ? { entityType } : undefined;
    const res = await apiClient.get<ApiResponse<SoftDeletedItem[]>>(
      API_ENDPOINTS.admin.softDeleted,
      { params, signal },
    );
    return unwrapApiResponse(res);
  },

  async restoreEntity(
    entityType: RestorableEntityType | string,
    id: number,
  ): Promise<string> {
    const res = await apiClient.post<ApiResponse<undefined>>(
      API_ENDPOINTS.admin.restore(entityType, id),
    );
    return res.data.message || `${entityType} #${id} restored successfully.`;
  },

  async getAuditLogs(
    params?: AuditLogQueryParams,
    signal?: AbortSignal,
  ): Promise<PagedResult<AuditLogItem>> {
    const res = await apiClient.get<ApiResponse<PagedResult<AuditLogItem>>>(
      API_ENDPOINTS.admin.auditLogs,
      { params, signal },
    );
    return unwrapApiResponse(res);
  },

  async searchStudents(
    params?: AdminStudentSearchParams,
    signal?: AbortSignal,
  ): Promise<PagedResult<AdminStudentDirectoryItem>> {
    const clampedPageSize = Math.min(Math.max(params?.pageSize ?? 10, 1), 20);
    const res = await apiClient.get<ApiResponse<PagedResult<AdminStudentDirectoryItem>>>(
      API_ENDPOINTS.students.list,
      {
        params: {
          ...params,
          pageSize: clampedPageSize,
        },
        signal,
      },
    );
    return unwrapApiResponse(res);
  },
};
