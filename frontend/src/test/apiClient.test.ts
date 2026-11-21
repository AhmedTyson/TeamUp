/**
 * @date 2026-11-21
 * @purpose Unit tests for TeamUp API endpoints, Bearer token storage, and envelope unwrapping.
 */
import type { AxiosResponse } from "axios";
import { beforeEach, describe, expect, it } from "vitest";
import { API_ENDPOINTS, ApiClientError, unwrapApiResponse } from "@/api";
import type { ApiResponse } from "@/api";
import { tokenStorage } from "@/lib/tokenStorage";

describe("API layer contracts", () => {
  beforeEach(() => {
    tokenStorage.clear();
  });

  it("maps core endpoints to /api/v1 routes", () => {
    expect(API_ENDPOINTS.health).toBe("/health");
    expect(API_ENDPOINTS.auth.login).toBe("/api/v1/auth/login");
    expect(API_ENDPOINTS.universities.catalog).toBe("/api/v1/universities/catalog");
    expect(API_ENDPOINTS.projects.tasks(42)).toBe("/api/v1/projects/42/tasks");
    expect(API_ENDPOINTS.sessions.attendance(7)).toBe("/api/v1/sessions/7/attendance");
  });

  it("unwraps successful ApiResponse<T> envelopes", () => {
    const fakeResponse = {
      status: 200,
      data: {
        success: true,
        message: "OK",
        statusCode: 200,
        data: { count: 3 },
        timestamp: "2026-11-21T00:00:00Z",
      },
    } as AxiosResponse<ApiResponse<{ count: number }>>;

    expect(unwrapApiResponse(fakeResponse)).toEqual({ count: 3 });
  });

  it("throws ApiClientError when envelope reports failure", () => {
    const fakeResponse = {
      status: 400,
      data: {
        success: false,
        message: "Validation failed",
        statusCode: 400,
        errors: [{ field: "email", message: "Invalid email" }],
        timestamp: "2026-11-21T00:00:00Z",
      },
    } as AxiosResponse<ApiResponse<unknown>>;

    expect(() => unwrapApiResponse(fakeResponse)).toThrowError(ApiClientError);
  });

  it("persists and clears JWT access/refresh tokens", () => {
    tokenStorage.setTokens("access-123", "refresh-456");
    expect(tokenStorage.getAccessToken()).toBe("access-123");
    expect(tokenStorage.getRefreshToken()).toBe("refresh-456");

    tokenStorage.clear();
    expect(tokenStorage.getAccessToken()).toBeNull();
    expect(tokenStorage.getRefreshToken()).toBeNull();
  });
});
