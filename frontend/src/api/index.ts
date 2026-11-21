/**
 * @date 2026-11-21
 * @purpose Barrel exports for the TeamUp frontend API layer.
 */
export { apiClient, createApiClient, ApiClientError, unwrapApiResponse } from "./client";
export { API_ENDPOINTS } from "./endpoints";
export { healthApi } from "./health.api";
export { authApi } from "./auth.api";
export { universitiesApi } from "./universities.api";
export type { ApiResponse, ErrorDetail, PagedResult, HealthStatus } from "./types";
export type { LoginPayload, RegisterPayload, AuthResult, AuthStudentSummary } from "./auth.api";
export type { UniversityCatalogItem, FacultyCatalogItem } from "./universities.api";
