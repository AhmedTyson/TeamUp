/**
 * @date 2026-11-21
 * @purpose Health check API module querying TeamUp.Api's /health endpoint.
 */
import { apiClient } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { HealthStatus } from "./types";

export const healthApi = {
  async check(signal?: AbortSignal): Promise<HealthStatus> {
    const response = await apiClient.get<HealthStatus>(API_ENDPOINTS.health, { signal });
    return response.data;
  },
};
