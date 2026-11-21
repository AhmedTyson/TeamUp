/**
 * @date 2026-11-21
 * @purpose Authentication API client wrapping /api/v1/auth endpoints.
 */
import { apiClient, unwrapApiResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { ApiResponse } from "./types";

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  academicYear: number;
  facultyId: number;
}

export interface AuthStudentSummary {
  studentId: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface AuthResult {
  accessToken: string;
  refreshToken: string;
  student: AuthStudentSummary;
}

export const authApi = {
  async login(payload: LoginPayload): Promise<AuthResult> {
    const res = await apiClient.post<ApiResponse<AuthResult>>(API_ENDPOINTS.auth.login, payload);
    return unwrapApiResponse(res);
  },

  async register(payload: RegisterPayload): Promise<AuthResult> {
    const res = await apiClient.post<ApiResponse<AuthResult>>(API_ENDPOINTS.auth.register, payload);
    return unwrapApiResponse(res);
  },

  async refresh(refreshToken: string): Promise<AuthResult> {
    const res = await apiClient.post<ApiResponse<AuthResult>>(API_ENDPOINTS.auth.refresh, { refreshToken });
    return unwrapApiResponse(res);
  },

  async logout(): Promise<void> {
    await apiClient.post(API_ENDPOINTS.auth.logout);
  },
};
