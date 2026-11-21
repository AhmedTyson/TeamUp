/**
 * @date 2026-11-21
 * @purpose Public academic catalog API module for university & faculty lookups.
 */
import { apiClient, unwrapApiResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { ApiResponse } from "./types";

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
  faculties: FacultyCatalogItem[];
}

export const universitiesApi = {
  async getCatalog(signal?: AbortSignal): Promise<UniversityCatalogItem[]> {
    const res = await apiClient.get<ApiResponse<UniversityCatalogItem[]>>(
      API_ENDPOINTS.universities.catalog,
      { signal },
    );
    return unwrapApiResponse(res);
  },
};
