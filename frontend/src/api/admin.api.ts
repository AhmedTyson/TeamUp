/**
 * @date 2026-11-22
 * @purpose PlatformAdmin governance API client covering dashboard KPIs, role management, 8-entity restore, immutable audit logs, and directory lookup.
 */
import type { AuthStudentSummary } from "./auth.api";
import { apiClient, fetchApiHtml, unwrapApiResponse } from "./client";
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

export interface CampusNewsItem {
  category: string;
  headline: string;
  summary: string;
}

export interface AdminReportSummaryDto {
  reportPeriod: string;
  generatedAtUtc: string;
  generatedByEmail: string;
  activeStudents: number;
  softDeletedStudents: number;
  activeUniversities: number;
  activeFaculties: number;
  activeProjects: number;
  recruitingProjects: number;
  activeSkills: number;
  activeOrganizations: number;
  activeEvents: number;
  totalAuditLogs: number;
  fileName: string;
}

export interface AdminReportEmailResultDto {
  recipientEmail: string;
  fileName: string;
  pdfSizeBytes: number;
  summary: AdminReportSummaryDto;
}

export interface SendAdminReportPayload {
  recipientEmail?: string;
  reportPeriod?: string;
}

export type DigestDispatchMode = "TestToMe" | "Broadcast";

export interface AdminLeaderboardDigestPayload {
  mode: DigestDispatchMode;
  periodLabel?: string;
  newsItems?: CampusNewsItem[];
  dryRun?: boolean;
}

export interface AdminProjectMatchDigestPayload {
  mode: DigestDispatchMode;
  maxMatchesPerStudent?: number;
  minMatchPercentage?: number;
  dryRun?: boolean;
}

export interface DigestDispatchResultDto {
  digestType: string;
  dryRun: boolean;
  isTestMode: boolean;
  eligibleRecipientsCount: number;
  sentCount: number;
  skippedCount: number;
  previewHtml: string | null;
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

  async downloadPlatformSummaryPdf(
    reportPeriod?: string,
    signal?: AbortSignal,
  ): Promise<{ blob: Blob; fileName: string }> {
    const res = await apiClient.get<Blob>(API_ENDPOINTS.admin.platformSummaryPdf, {
      params: reportPeriod ? { reportPeriod } : undefined,
      responseType: "blob",
      headers: { Accept: "application/pdf" },
      signal,
    });

    const disposition = String(res.headers?.["content-disposition"] ?? "");
    const match = disposition.match(/filename\*?=(?:UTF-8'')?["']?([^"';]+)["']?/i);
    const fileName = match?.[1]?.trim() || "TeamUp_Platform_Governance.pdf";

    return { blob: res.data, fileName };
  },

  async sendPlatformSummaryReport(
    payload?: SendAdminReportPayload,
  ): Promise<AdminReportEmailResultDto> {
    const res = await apiClient.post<ApiResponse<AdminReportEmailResultDto>>(
      API_ENDPOINTS.admin.sendSummaryReport,
      payload ?? {},
    );
    return unwrapApiResponse(res);
  },

  async getEmailTemplates(signal?: AbortSignal): Promise<string[]> {
    const res = await apiClient.get<ApiResponse<string[]>>(
      API_ENDPOINTS.admin.emailTemplates,
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async getEmailTemplatePreview(
    templateName: string,
    signal?: AbortSignal,
  ): Promise<string> {
    return fetchApiHtml(API_ENDPOINTS.admin.emailTemplatePreview(templateName), { signal });
  },

  async dispatchLeaderboardDigest(
    payload: AdminLeaderboardDigestPayload,
  ): Promise<DigestDispatchResultDto> {
    const res = await apiClient.post<ApiResponse<DigestDispatchResultDto>>(
      API_ENDPOINTS.admin.leaderboardDigest,
      payload,
    );
    return unwrapApiResponse(res);
  },

  async dispatchProjectMatchDigest(
    payload: AdminProjectMatchDigestPayload,
  ): Promise<DigestDispatchResultDto> {
    const res = await apiClient.post<ApiResponse<DigestDispatchResultDto>>(
      API_ENDPOINTS.admin.projectMatchDigest,
      payload,
    );
    return unwrapApiResponse(res);
  },
};
