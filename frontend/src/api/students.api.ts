/**
 * @date 2026-11-22
 * @purpose Authenticated student directory, profile studio, showcase, and interest tags API client (STRIDE THR-I-01 & THR-T-03).
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { ApiResponse, PagedResult } from "./types";

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

export interface StudentProfileDto {
  bio: string | null;
  photoUrl: string | null;
  gitHubUrl: string | null;
  linkedInUrl: string | null;
}

export interface UpsertProfileRequest {
  bio?: string | null;
  photoUrl?: string | null;
  gitHubUrl?: string | null;
  linkedInUrl?: string | null;
}

export interface UpdateStudentRequest {
  firstName?: string;
  lastName?: string;
  academicYear?: number;
  showEmailInDirectory?: boolean;
}

export interface StudentDetailDto {
  studentId: number;
  firstName: string;
  lastName: string;
  email: string;
  academicYear: number;
  role: string;
  facultyId: number;
  facultyName: string;
  universityName: string;
  profile: StudentProfileDto | null;
  interests: string[];
  showEmailInDirectory: boolean;
}

export interface StudentCardDto {
  studentId: number;
  firstName: string;
  lastName: string;
  email: string | null;
  academicYear: number;
  role: string;
  facultyId: number;
  facultyName: string;
  universityName: string;
  interests: string[];
  photoUrl?: string | null;
}

export interface StudentPublicProfileDto {
  studentId: number;
  firstName: string;
  lastName: string;
  academicYear: number;
  facultyName: string;
  universityName: string;
  profile: StudentProfileDto | null;
  interests: string[];
}

export interface StudentDirectoryQueryParams {
  search?: string;
  facultyId?: number;
  academicYear?: number;
  page?: number;
  pageSize?: number;
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

  async getDirectory(
    params?: StudentDirectoryQueryParams,
    signal?: AbortSignal,
  ): Promise<PagedResult<StudentCardDto>> {
    const safePageSize = Math.min(Math.max(params?.pageSize ?? 20, 1), 20);
    const res = await apiClient.get<ApiResponse<PagedResult<StudentCardDto>>>(
      API_ENDPOINTS.students.list,
      {
        params: {
          Search: params?.search || undefined,
          FacultyId: params?.facultyId || undefined,
          AcademicYear: params?.academicYear || undefined,
          Page: params?.page ?? 1,
          PageSize: safePageSize,
        },
        signal,
      },
    );
    return unwrapApiResponse(res);
  },

  async getById(id: number, signal?: AbortSignal): Promise<StudentPublicProfileDto> {
    const res = await apiClient.get<ApiResponse<StudentPublicProfileDto>>(
      API_ENDPOINTS.students.byId(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async getMe(signal?: AbortSignal): Promise<StudentDetailDto> {
    const res = await apiClient.get<ApiResponse<StudentDetailDto>>(
      API_ENDPOINTS.students.me,
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async updateMe(data: UpdateStudentRequest): Promise<StudentDetailDto> {
    const res = await apiClient.patch<ApiResponse<StudentDetailDto>>(
      API_ENDPOINTS.students.me,
      data,
    );
    return unwrapApiResponse(res);
  },

  async deleteMe(): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.students.me);
    unwrapVoidResponse(res);
  },

  async deleteStudent(id: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.students.byId(id));
    unwrapVoidResponse(res);
  },

  async getProfile(id: number, signal?: AbortSignal): Promise<StudentProfileDto> {
    const res = await apiClient.get<ApiResponse<StudentProfileDto>>(
      API_ENDPOINTS.students.profile(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async upsertMyProfile(data: UpsertProfileRequest): Promise<StudentProfileDto> {
    const res = await apiClient.put<ApiResponse<StudentProfileDto>>(
      API_ENDPOINTS.students.myProfile,
      data,
    );
    return unwrapApiResponse(res);
  },

  async uploadProfilePhoto(file: File): Promise<StudentProfileDto> {
    const formData = new FormData();
    formData.append("file", file);
    const res = await apiClient.post<ApiResponse<StudentProfileDto>>(
      API_ENDPOINTS.students.myProfilePhoto,
      formData,
    );
    return unwrapApiResponse(res);
  },

  async getInterests(id: number, signal?: AbortSignal): Promise<string[]> {
    const res = await apiClient.get<ApiResponse<string[]>>(
      API_ENDPOINTS.students.interests(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async addMyInterest(interest: string): Promise<void> {
    const res = await apiClient.post(API_ENDPOINTS.students.myInterests, {
      interest: interest.trim(),
    });
    unwrapVoidResponse(res);
  },

  async removeMyInterest(interest: string): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.students.myInterest(interest));
    unwrapVoidResponse(res);
  },
};
