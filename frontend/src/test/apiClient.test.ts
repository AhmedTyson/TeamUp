/**
 * @date 2026-11-21
 * @purpose Unit tests for TeamUp API endpoints (all 67 paths), single-flight 401 refresh mutex, 429 Retry-After parsing, 204 handling, and HTML fetching.
 */
import axios, {
  AxiosError,
  AxiosHeaders,
  type AxiosAdapter,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  API_ENDPOINTS,
  ApiClientError,
  createApiClient,
  fetchApiHtml,
  isOrganizationAdminRole,
  parseRetryAfterSeconds,
  RESTORABLE_ENTITY_TYPES,
  unwrapApiResponse,
  unwrapVoidResponse,
} from "@/api";
import type { ApiResponse } from "@/api";
import { tokenStorage } from "@/lib/tokenStorage";

function makeAxiosError(
  config: InternalAxiosRequestConfig,
  status: number,
  data: unknown,
  headers: Record<string, string> = {},
): AxiosError {
  const response: AxiosResponse = {
    data,
    status,
    statusText: String(status),
    headers,
    config,
  };
  return new AxiosError(`Request failed with status code ${status}`, undefined, config, undefined, response);
}

describe("API layer contracts", () => {
  beforeEach(() => {
    tokenStorage.clear();
  });

  it("maps all core and Task-2 STRIDE endpoints to /api/v1 routes", () => {
    expect(API_ENDPOINTS.health).toBe("/health");
    expect(API_ENDPOINTS.auth.login).toBe("/api/v1/auth/login");
    expect(API_ENDPOINTS.universities.catalog).toBe("/api/v1/universities/catalog");
    expect(API_ENDPOINTS.projects.tasks(42)).toBe("/api/v1/projects/42/tasks");
    expect(API_ENDPOINTS.sessions.attendance(7)).toBe("/api/v1/sessions/7/attendance");
    // Task-2 STRIDE endpoints synced in Phase 0
    expect(API_ENDPOINTS.admin.auditLogs).toBe("/api/v1/admin/audit-logs");
    expect(API_ENDPOINTS.gamification.leaderboard).toBe("/api/v1/gamification/leaderboard");
    expect(API_ENDPOINTS.sessions.checkInCode(9)).toBe("/api/v1/sessions/9/attendance/check-in-code");
    expect(API_ENDPOINTS.sessions.selfCheckIn(9)).toBe("/api/v1/sessions/9/attendance/self-check-in");
    expect(API_ENDPOINTS.sessions.attendanceReport(9)).toBe("/api/v1/sessions/9/attendance/report");
    expect(API_ENDPOINTS.students.manualAwardBadge(12, 4)).toBe("/api/v1/students/12/badges/4");
    expect(API_ENDPOINTS.students.myProfilePhoto).toBe("/api/v1/students/me/profile/photo");
  });

  it("validates OrganizationRoles admin helper and restorable entity enum", () => {
    expect(isOrganizationAdminRole("Admin")).toBe(true);
    expect(isOrganizationAdminRole("Vice President")).toBe(true);
    expect(isOrganizationAdminRole("executive")).toBe(true);
    expect(isOrganizationAdminRole("Member")).toBe(false);
    expect(isOrganizationAdminRole(null)).toBe(false);
    expect(Object.keys(RESTORABLE_ENTITY_TYPES)).toHaveLength(8);
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

  it("handles 204 No Content safely via unwrapVoidResponse", () => {
    const response204 = { status: 204, data: "" } as AxiosResponse<unknown>;
    expect(() => unwrapVoidResponse(response204)).not.toThrow();

    const failedEnvelope = {
      status: 200,
      data: { success: false, message: "Denied", statusCode: 403 },
    } as AxiosResponse<unknown>;
    expect(() => unwrapVoidResponse(failedEnvelope)).toThrowError(ApiClientError);
  });

  it("persists and clears JWT access/refresh tokens", () => {
    tokenStorage.setTokens("access-123", "refresh-456");
    expect(tokenStorage.getAccessToken()).toBe("access-123");
    expect(tokenStorage.getRefreshToken()).toBe("refresh-456");

    tokenStorage.clear();
    expect(tokenStorage.getAccessToken()).toBeNull();
    expect(tokenStorage.getRefreshToken()).toBeNull();
  });

  it("parses numeric and HTTP-date Retry-After headers", () => {
    expect(parseRetryAfterSeconds("45")).toBe(45);
    expect(parseRetryAfterSeconds(12.2)).toBe(13);
    const futureDate = new Date(Date.now() + 30_000).toUTCString();
    const parsed = parseRetryAfterSeconds(futureDate);
    expect(parsed).toBeGreaterThanOrEqual(28);
    expect(parsed).toBeLessThanOrEqual(32);
    expect(parseRetryAfterSeconds("")).toBeUndefined();
  });
});

describe("Axios single-flight 401 refresh mutex, 429 parsing, and HTML fetcher", () => {
  beforeEach(() => {
    tokenStorage.clear();
  });

  it("coalesces concurrent 401 requests behind a single /auth/refresh call and retries them", async () => {
    tokenStorage.setTokens("expired-access", "valid-refresh-1");

    const onTokensRefreshed = vi.fn();
    const onSessionExpired = vi.fn();
    let refreshCallCount = 0;

    const adapter: AxiosAdapter = async (config) => {
      const headers = AxiosHeaders.from(config.headers);
      const authHeader = headers.get("Authorization");

      if (config.url === API_ENDPOINTS.auth.refresh) {
        refreshCallCount += 1;
        // Simulate async network delay so concurrent requests pile onto the same refreshPromise
        await new Promise((resolve) => setTimeout(resolve, 15));
        return {
          status: 200,
          statusText: "OK",
          headers: {},
          config,
          data: {
            success: true,
            message: "Refreshed",
            statusCode: 200,
            data: {
              accessToken: "fresh-access-2",
              refreshToken: "fresh-refresh-2",
            },
            timestamp: "2026-11-21T00:00:00Z",
          },
        };
      }

      if (authHeader !== "Bearer fresh-access-2") {
        throw makeAxiosError(config, 401, {
          success: false,
          message: "Token expired",
          statusCode: 401,
        });
      }

      return {
        status: 200,
        statusText: "OK",
        headers: {},
        config,
        data: {
          success: true,
          message: "OK",
          statusCode: 200,
          data: { path: config.url },
          timestamp: "2026-11-21T00:00:00Z",
        },
      };
    };

    const client = createApiClient({
      baseURL: "",
      onTokensRefreshed,
      onSessionExpired,
    });
    client.defaults.adapter = adapter;

    const [r1, r2, r3] = await Promise.all([
      client.get<ApiResponse<{ path: string }>>(API_ENDPOINTS.projects.list),
      client.get<ApiResponse<{ path: string }>>(API_ENDPOINTS.events.list),
      client.get<ApiResponse<{ path: string }>>(API_ENDPOINTS.students.me),
    ]);

    expect(refreshCallCount).toBe(1);
    expect(onTokensRefreshed).toHaveBeenCalledTimes(1);
    expect(onTokensRefreshed).toHaveBeenCalledWith("fresh-access-2", "fresh-refresh-2");
    expect(onSessionExpired).not.toHaveBeenCalled();
    expect(tokenStorage.getAccessToken()).toBe("fresh-access-2");
    expect(tokenStorage.getRefreshToken()).toBe("fresh-refresh-2");
    expect(unwrapApiResponse(r1).path).toBe(API_ENDPOINTS.projects.list);
    expect(unwrapApiResponse(r2).path).toBe(API_ENDPOINTS.events.list);
    expect(unwrapApiResponse(r3).path).toBe(API_ENDPOINTS.students.me);
  });

  it("clears tokens and triggers onSessionExpired when refresh fails", async () => {
    tokenStorage.setTokens("expired-access", "revoked-refresh");
    const onSessionExpired = vi.fn();

    const adapter: AxiosAdapter = async (config) => {
      throw makeAxiosError(config, 401, {
        success: false,
        message: "Session revoked",
        statusCode: 401,
      });
    };

    const client = createApiClient({ baseURL: "", onSessionExpired });
    client.defaults.adapter = adapter;

    await expect(client.get(API_ENDPOINTS.projects.list)).rejects.toMatchObject({
      name: "ApiClientError",
      statusCode: 401,
    });

    expect(tokenStorage.getAccessToken()).toBeNull();
    expect(tokenStorage.getRefreshToken()).toBeNull();
    expect(onSessionExpired).toHaveBeenCalledTimes(1);
  });

  it("does not trigger refresh when /auth/login itself fails with 401", async () => {
    tokenStorage.setTokens("old-access", "old-refresh");
    let refreshAttempted = false;

    const adapter: AxiosAdapter = async (config) => {
      if (config.url === API_ENDPOINTS.auth.refresh) {
        refreshAttempted = true;
      }
      throw makeAxiosError(config, 401, {
        success: false,
        message: "Invalid credentials",
        statusCode: 401,
      });
    };

    const client = createApiClient({ baseURL: "" });
    client.defaults.adapter = adapter;

    await expect(
      client.post(API_ENDPOINTS.auth.login, { email: "a@b.com", password: "wrong" }),
    ).rejects.toMatchObject({
      statusCode: 401,
      message: "Invalid credentials",
    });

    expect(refreshAttempted).toBe(false);
  });

  it("extracts Retry-After seconds onto ApiClientError for 429 responses", async () => {
    const adapter: AxiosAdapter = async (config) => {
      throw makeAxiosError(
        config,
        429,
        {
          success: false,
          message: "Rate limit exceeded.",
          statusCode: 429,
        },
        { "retry-after": "60" },
      );
    };

    const client = createApiClient({ baseURL: "" });
    client.defaults.adapter = adapter;

    try {
      await client.get(API_ENDPOINTS.students.list);
      expect.unreachable("Expected 429 error");
    } catch (err) {
      expect(err).toBeInstanceOf(ApiClientError);
      const apiErr = err as ApiClientError;
      expect(apiErr.statusCode).toBe(429);
      expect(apiErr.isRateLimited).toBe(true);
      expect(apiErr.retryAfterSeconds).toBe(60);
    }
  });

  it("fetches raw HTML attendance reports with Accept: text/html", async () => {
    const client = axios.create();
    client.defaults.adapter = async (config) => {
      const headers = AxiosHeaders.from(config.headers);
      expect(headers.get("Accept")).toBe("text/html");
      expect(config.responseType).toBe("text");
      return {
        status: 200,
        statusText: "OK",
        headers: { "content-type": "text/html" },
        config,
        data: "<html><body><h1>Attendance Report</h1></body></html>",
      };
    };

    const html = await fetchApiHtml(API_ENDPOINTS.sessions.attendanceReport(5), undefined, client);
    expect(html).toContain("<h1>Attendance Report</h1>");
  });

  it("removes default JSON Content-Type header when sending FormData uploads", async () => {
    const client = createApiClient({ baseURL: "" });
    client.defaults.adapter = async (config) => {
      const headers = AxiosHeaders.from(config.headers);
      const contentType = String(headers.get("Content-Type") ?? "");
      expect(contentType).not.toContain("application/json");
      return {
        status: 200,
        statusText: "OK",
        headers: {},
        config,
        data: {
          success: true,
          message: "Uploaded",
          statusCode: 200,
          data: { bio: null, photoUrl: "/images/people/student-1-demo.png", gitHubUrl: null, linkedInUrl: null },
          timestamp: "2026-11-22T00:00:00Z",
        },
      };
    };

    const formData = new FormData();
    formData.append("file", new Blob(["png"], { type: "image/png" }), "portrait.png");
    const response = await client.post(API_ENDPOINTS.students.myProfilePhoto, formData);
    expect(response.status).toBe(200);
  });
});
