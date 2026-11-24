/**
 * @date 2026-11-24
 * @purpose Typed API client for session attendance rosters, rotating QR check-in codes, student self check-in, and HTML attendance reports.
 */
import { apiClient, fetchApiCsv, fetchApiHtml, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { VerificationMethod } from "./enums";
import type { ApiResponse } from "./types";

export interface SessionAttendance {
  sessionId: number;
  studentId: number;
  firstName: string;
  lastName: string;
  checkInTime: string;
  verificationMethod: VerificationMethod;
  recordedByStudentId: number | null;
}

export interface SessionCheckInCode {
  sessionId: number;
  code: string;
  expiresAtUtc: string;
}

export interface ManualAttendancePayload {
  studentId: number;
  verificationMethod: VerificationMethod;
  verificationCode: string | null;
}

export const attendanceApi = {
  async list(sessionId: number, signal?: AbortSignal): Promise<SessionAttendance[]> {
    const response = await apiClient.get<ApiResponse<SessionAttendance[]>>(
      API_ENDPOINTS.sessions.attendance(sessionId),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async generateCode(sessionId: number): Promise<SessionCheckInCode> {
    const response = await apiClient.post<ApiResponse<SessionCheckInCode>>(
      API_ENDPOINTS.sessions.checkInCode(sessionId),
    );
    return unwrapApiResponse(response);
  },

  async checkIn(
    sessionId: number,
    payload: ManualAttendancePayload,
  ): Promise<SessionAttendance> {
    const response = await apiClient.post<ApiResponse<SessionAttendance>>(
      API_ENDPOINTS.sessions.attendance(sessionId),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async selfCheckIn(
    sessionId: number,
    verificationCode: string,
  ): Promise<SessionAttendance> {
    const response = await apiClient.post<ApiResponse<SessionAttendance>>(
      API_ENDPOINTS.sessions.selfCheckIn(sessionId),
      { verificationCode },
    );
    return unwrapApiResponse(response);
  },

  async revoke(sessionId: number, studentId: number): Promise<void> {
    const response = await apiClient.delete(
      API_ENDPOINTS.sessions.studentAttendance(sessionId, studentId),
    );
    unwrapVoidResponse(response);
  },

  async getReportHtml(sessionId: number): Promise<string> {
    return fetchApiHtml(API_ENDPOINTS.sessions.attendanceReport(sessionId));
  },

  async exportCsv(sessionId: number): Promise<string> {
    return fetchApiCsv(API_ENDPOINTS.sessions.attendanceCsv(sessionId));
  },
};
