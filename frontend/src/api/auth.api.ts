/**
 * @date 2026-11-21
 * @purpose Authentication API client wrapping /api/v1/auth endpoints and current student session hydration.
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { UserRole } from "./enums";
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
  emailNotificationsEnabled?: boolean;
  receiveLeaderboardDigests?: boolean;
  receiveProjectMatchEmails?: boolean;
}

export interface ForgotPasswordPayload {
  email: string;
}

export interface ForgotPasswordResult {
  message: string;
}

export interface ResetPasswordPayload {
  email: string;
  token: string;
  newPassword: string;
}

export interface AuthStudentProfileSummary {
  bio: string | null;
  photoUrl: string | null;
  gitHubUrl: string | null;
  linkedInUrl: string | null;
}

export interface AuthStudentSummary {
  studentId: number;
  firstName: string;
  lastName: string;
  email: string;
  academicYear: number;
  role: UserRole;
  facultyId: number;
  facultyName: string;
  universityName: string;
  photoUrl?: string | null;
  profile?: AuthStudentProfileSummary | null;
  showEmailInDirectory?: boolean;
  emailNotificationsEnabled?: boolean;
  receiveLeaderboardDigests?: boolean;
  receiveProjectMatchEmails?: boolean;
}

export interface AuthResult {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
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
    const res = await apiClient.post<ApiResponse<AuthResult>>(API_ENDPOINTS.auth.refresh, {
      refreshToken,
    });
    return unwrapApiResponse(res);
  },

  async logout(): Promise<void> {
    const res = await apiClient.post(API_ENDPOINTS.auth.logout);
    unwrapVoidResponse(res);
  },

  async forgotPassword(payload: ForgotPasswordPayload): Promise<ForgotPasswordResult> {
    const res = await apiClient.post<ApiResponse<ForgotPasswordResult>>(
      API_ENDPOINTS.auth.forgotPassword,
      payload,
    );
    return unwrapApiResponse(res);
  },

  async resetPassword(payload: ResetPasswordPayload): Promise<string> {
    const res = await apiClient.post<ApiResponse<undefined>>(
      API_ENDPOINTS.auth.resetPassword,
      payload,
    );
    return res.data.message || "Password reset successfully.";
  },

  async getCurrentStudent(signal?: AbortSignal): Promise<AuthStudentSummary> {
    const res = await apiClient.get<ApiResponse<AuthStudentSummary>>(API_ENDPOINTS.students.me, {
      signal,
    });
    return unwrapApiResponse(res);
  },
};
