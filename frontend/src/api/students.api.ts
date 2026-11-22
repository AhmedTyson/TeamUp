/**
 * @date 2026-11-22
 * @purpose Public and student directory API wrappers fetching database-backed student showcase cards and portraits.
 */
import { apiClient, unwrapApiResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { ApiResponse } from "./types";

export interface PublicStudentShowcaseItem {
  studentId: number;
  firstName: string;
  lastName: string;
  academicYear: number;
  facultyName: string;
  facultyCode: string;
  universityName: string;
  universityLogoUrl: string | null;
  bio: string | null;
  photoUrl: string;
  totalPoints: number;
  level: number;
  badgesCount: number;
  topSkills: readonly string[];
}

export interface StudentProfile {
  bio: string | null;
  photoUrl: string | null;
  gitHubUrl: string | null;
  linkedInUrl: string | null;
}

export const studentsApi = {
  async getPublicShowcase(
    limit = 6,
    signal?: AbortSignal,
  ): Promise<PublicStudentShowcaseItem[]> {
    const res = await apiClient.get<ApiResponse<PublicStudentShowcaseItem[]>>(
      API_ENDPOINTS.students.showcase,
      {
        params: { limit },
        signal,
      },
    );
    return unwrapApiResponse(res);
  },

  async uploadProfilePhoto(file: File): Promise<StudentProfile> {
    const formData = new FormData();
    formData.append("file", file);
    const res = await apiClient.post<ApiResponse<StudentProfile>>(
      API_ENDPOINTS.students.myProfilePhoto,
      formData,
    );
    return unwrapApiResponse(res);
  },
};
