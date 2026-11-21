/**
 * @date 2026-11-22
 * @purpose Academic catalog API module covering all 11 Universities and Faculties operations.
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { ApiResponse, PagedResult } from "./types";

export interface FacultyCatalogItem {
  facultyId: number;
  name: string;
  code: string;
}

export interface UniversityCatalogItem {
  universityId: number;
  name: string;
  city: string;
  country: string;
  website?: string | null;
  faculties: FacultyCatalogItem[];
}

export interface UniversitySummary {
  universityId: number;
  name: string;
  city: string;
  country: string;
  website: string | null;
  facultyCount: number;
}

export type UniversityDetail = UniversityCatalogItem;

export interface UniversityListParams {
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateUniversityPayload {
  name: string;
  city: string;
  country: string;
  website?: string | null;
}

export interface UpdateUniversityPayload {
  name: string;
  city: string;
  country: string;
  website?: string | null;
}

export interface FacultyDetail {
  facultyId: number;
  name: string;
  code: string;
  universityId: number;
  universityName: string;
  studentCount: number;
}

export interface CreateFacultyPayload {
  name: string;
  code: string;
}

export interface UpdateFacultyPayload {
  name: string;
  code: string;
}

export const universitiesApi = {
  async getCatalog(signal?: AbortSignal): Promise<UniversityCatalogItem[]> {
    const res = await apiClient.get<ApiResponse<UniversityCatalogItem[]>>(
      API_ENDPOINTS.universities.catalog,
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async list(
    params?: UniversityListParams,
    signal?: AbortSignal,
  ): Promise<PagedResult<UniversitySummary>> {
    const res = await apiClient.get<ApiResponse<PagedResult<UniversitySummary>>>(
      API_ENDPOINTS.universities.list,
      { params, signal },
    );
    return unwrapApiResponse(res);
  },

  async getById(id: number, signal?: AbortSignal): Promise<UniversityDetail> {
    const res = await apiClient.get<ApiResponse<UniversityDetail>>(
      API_ENDPOINTS.universities.byId(id),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async create(payload: CreateUniversityPayload): Promise<UniversityDetail> {
    const res = await apiClient.post<ApiResponse<UniversityDetail>>(
      API_ENDPOINTS.universities.list,
      payload,
    );
    return unwrapApiResponse(res);
  },

  async update(id: number, payload: UpdateUniversityPayload): Promise<UniversityDetail> {
    const res = await apiClient.put<ApiResponse<UniversityDetail>>(
      API_ENDPOINTS.universities.byId(id),
      payload,
    );
    return unwrapApiResponse(res);
  },

  async remove(id: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.universities.byId(id));
    unwrapVoidResponse(res);
  },

  async getFaculties(universityId: number, signal?: AbortSignal): Promise<FacultyDetail[]> {
    const res = await apiClient.get<ApiResponse<FacultyDetail[]>>(
      API_ENDPOINTS.universities.faculties(universityId),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async getFacultyById(facultyId: number, signal?: AbortSignal): Promise<FacultyDetail> {
    const res = await apiClient.get<ApiResponse<FacultyDetail>>(
      API_ENDPOINTS.faculties.byId(facultyId),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async createFaculty(
    universityId: number,
    payload: CreateFacultyPayload,
  ): Promise<FacultyDetail> {
    const res = await apiClient.post<ApiResponse<FacultyDetail>>(
      API_ENDPOINTS.universities.faculties(universityId),
      payload,
    );
    return unwrapApiResponse(res);
  },

  async updateFaculty(
    facultyId: number,
    payload: UpdateFacultyPayload,
  ): Promise<FacultyDetail> {
    const res = await apiClient.put<ApiResponse<FacultyDetail>>(
      API_ENDPOINTS.faculties.byId(facultyId),
      payload,
    );
    return unwrapApiResponse(res);
  },

  async removeFaculty(facultyId: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.faculties.byId(facultyId));
    unwrapVoidResponse(res);
  },
};
