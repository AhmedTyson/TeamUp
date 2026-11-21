/**
 * @date 2026-11-21
 * @purpose Factory creating a configured TanStack QueryClient with TeamUp-aware retry and cache defaults.
 */
import { QueryClient } from "@tanstack/react-query";
import { ApiClientError } from "@/api/client";

const NON_RETRYABLE_STATUSES = new Set([400, 401, 403, 404, 409, 422, 429]);

export function shouldRetryApiError(failureCount: number, error: unknown): boolean {
  if (failureCount >= 1) {
    return false;
  }
  if (error instanceof ApiClientError && NON_RETRYABLE_STATUSES.has(error.statusCode)) {
    return false;
  }
  return true;
}

export function createAppQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        gcTime: 5 * 60_000,
        retry: shouldRetryApiError,
        refetchOnWindowFocus: false,
      },
      mutations: {
        retry: false,
      },
    },
  });
}
