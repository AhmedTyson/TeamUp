/**
 * @date 2026-11-21
 * @purpose Configured Axios HTTP client with JWT Bearer injection, single-flight 401 refresh mutex, 429 Retry-After parsing, 204 safe helpers, and HTML text fetching.
 */
import axios, {
  AxiosError,
  AxiosHeaders,
  type AxiosInstance,
  type AxiosRequestConfig,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";
import { env } from "@/lib/env";
import { tokenStorage } from "@/lib/tokenStorage";
import { API_ENDPOINTS } from "./endpoints";
import type { ApiResponse, ErrorDetail } from "./types";

interface RefreshPayload {
  accessToken: string;
  refreshToken: string;
}

export interface TeamUpRequestConfig extends AxiosRequestConfig {
  _retry?: boolean;
  _skipAuthRefresh?: boolean;
}

interface InternalTeamUpRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
  _skipAuthRefresh?: boolean;
}

export interface CreateApiClientOptions {
  baseURL?: string;
  refreshEndpoint?: string;
  onTokensRefreshed?: (accessToken: string, refreshToken: string) => void;
  onSessionExpired?: () => void;
}

type SessionExpiredListener = () => void;
type TokensRefreshedListener = (accessToken: string, refreshToken: string) => void;

const sessionExpiredListeners = new Set<SessionExpiredListener>();
const tokensRefreshedListeners = new Set<TokensRefreshedListener>();

export const authClientEvents = {
  onSessionExpired(listener: SessionExpiredListener): () => void {
    sessionExpiredListeners.add(listener);
    return () => {
      sessionExpiredListeners.delete(listener);
    };
  },

  onTokensRefreshed(listener: TokensRefreshedListener): () => void {
    tokensRefreshedListeners.add(listener);
    return () => {
      tokensRefreshedListeners.delete(listener);
    };
  },

  emitSessionExpired(): void {
    for (const listener of sessionExpiredListeners) {
      listener();
    }
  },

  emitTokensRefreshed(accessToken: string, refreshToken: string): void {
    for (const listener of tokensRefreshedListeners) {
      listener(accessToken, refreshToken);
    }
  },
};

const NON_REFRESHABLE_AUTH_PATHS = [
  API_ENDPOINTS.auth.login,
  API_ENDPOINTS.auth.register,
  API_ENDPOINTS.auth.refresh,
  API_ENDPOINTS.auth.forgotPassword,
  API_ENDPOINTS.auth.resetPassword,
];

function isNonRefreshableAuthUrl(url?: string): boolean {
  if (!url) {
    return false;
  }
  const pathname = url.split("?")[0] ?? url;
  return NON_REFRESHABLE_AUTH_PATHS.some((path) => pathname.endsWith(path));
}

export function parseRetryAfterSeconds(headerValue: unknown): number | undefined {
  if (typeof headerValue === "number" && Number.isFinite(headerValue) && headerValue >= 0) {
    return Math.ceil(headerValue);
  }
  if (typeof headerValue !== "string") {
    return undefined;
  }
  const trimmed = headerValue.trim();
  if (!trimmed) {
    return undefined;
  }
  if (/^\d+(\.\d+)?$/.test(trimmed)) {
    return Math.ceil(Number.parseFloat(trimmed));
  }
  const parsedDate = Date.parse(trimmed);
  if (!Number.isNaN(parsedDate)) {
    const deltaMs = parsedDate - Date.now();
    return Math.max(0, Math.ceil(deltaMs / 1000));
  }
  return undefined;
}

function readHeaderValue(headers: unknown, name: string): unknown {
  if (!headers || typeof headers !== "object") {
    return undefined;
  }
  if ("get" in headers && typeof (headers as AxiosHeaders).get === "function") {
    return (headers as AxiosHeaders).get(name);
  }
  const record = headers as Record<string, unknown>;
  return record[name] ?? record[name.toLowerCase()] ?? record["Retry-After"];
}

export class ApiClientError extends Error {
  readonly statusCode: number;
  readonly errors: ErrorDetail[];
  readonly retryAfterSeconds?: number;

  constructor(
    message: string,
    statusCode: number,
    errors: ErrorDetail[] = [],
    retryAfterSeconds?: number,
  ) {
    super(message);
    this.name = "ApiClientError";
    this.statusCode = statusCode;
    this.errors = errors;
    this.retryAfterSeconds = retryAfterSeconds;
  }

  get isValidationError(): boolean {
    return this.statusCode === 400;
  }

  get isUnauthorized(): boolean {
    return this.statusCode === 401;
  }

  get isForbidden(): boolean {
    return this.statusCode === 403;
  }

  get isConflict(): boolean {
    return this.statusCode === 409;
  }

  get isRateLimited(): boolean {
    return this.statusCode === 429;
  }
}

function normalizeAxiosError(error: AxiosError): ApiClientError {
  const status = error.response?.status ?? 0;
  const rawData = error.response?.data;
  const payload =
    rawData && typeof rawData === "object" ? (rawData as Partial<ApiResponse>) : undefined;
  const message =
    payload?.message ||
    (typeof rawData === "string" && rawData.trim().length > 0 && rawData.length < 300
      ? rawData.trim()
      : error.message) ||
    "Network request failed.";
  const errors = Array.isArray(payload?.errors) ? payload.errors : [];
  const retryAfterSeconds =
    status === 429
      ? parseRetryAfterSeconds(readHeaderValue(error.response?.headers, "retry-after"))
      : undefined;

  return new ApiClientError(message, status, errors, retryAfterSeconds);
}

export function createApiClient(
  baseURLOrOptions: string | CreateApiClientOptions = env.apiBaseUrl,
): AxiosInstance {
  const options: CreateApiClientOptions =
    typeof baseURLOrOptions === "string" ? { baseURL: baseURLOrOptions } : baseURLOrOptions;

  const baseURL = options.baseURL ?? env.apiBaseUrl;
  const refreshEndpoint = options.refreshEndpoint ?? API_ENDPOINTS.auth.refresh;

  const instance = axios.create({
    baseURL,
    timeout: 15_000,
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
    },
  });

  // Single-flight refresh mutex scoped to this client instance (prevents refresh-token replay collisions).
  let refreshPromise: Promise<string> | null = null;

  const notifyTokensRefreshed = (accessToken: string, refreshToken: string) => {
    if (options.onTokensRefreshed) {
      options.onTokensRefreshed(accessToken, refreshToken);
    } else {
      authClientEvents.emitTokensRefreshed(accessToken, refreshToken);
    }
  };

  const notifySessionExpired = () => {
    if (options.onSessionExpired) {
      options.onSessionExpired();
    } else {
      authClientEvents.emitSessionExpired();
    }
  };

  const performSingleFlightRefresh = (): Promise<string> => {
    if (refreshPromise) {
      return refreshPromise;
    }

    const currentRefreshToken = tokenStorage.getRefreshToken();
    if (!currentRefreshToken) {
      tokenStorage.clear();
      notifySessionExpired();
      return Promise.reject(new ApiClientError("Session expired. Please sign in again.", 401));
    }

    refreshPromise = axios
      .post<ApiResponse<RefreshPayload>>(
        refreshEndpoint,
        { refreshToken: currentRefreshToken },
        {
          baseURL,
          timeout: 15_000,
          adapter: instance.defaults.adapter,
          headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
          },
        },
      )
      .then((res) => {
        const data = unwrapApiResponse(res);
        if (!data.accessToken || !data.refreshToken) {
          throw new ApiClientError("Invalid refresh response from server.", 401);
        }
        tokenStorage.setTokens(data.accessToken, data.refreshToken);
        notifyTokensRefreshed(data.accessToken, data.refreshToken);
        return data.accessToken;
      })
      .catch((err: unknown) => {
        tokenStorage.clear();
        notifySessionExpired();
        if (err instanceof ApiClientError) {
          throw err;
        }
        if (err instanceof AxiosError) {
          throw normalizeAxiosError(err);
        }
        throw new ApiClientError("Session expired. Please sign in again.", 401);
      })
      .finally(() => {
        refreshPromise = null;
      });

    return refreshPromise;
  };

  instance.interceptors.request.use((config) => {
    const token = tokenStorage.getAccessToken();
    if (token) {
      config.headers = AxiosHeaders.from(config.headers);
      if (!config.headers.has("Authorization")) {
        config.headers.set("Authorization", `Bearer ${token}`);
      }
    }
    return config;
  });

  instance.interceptors.response.use(
    (response) => response,
    async (error: unknown) => {
      if (!(error instanceof AxiosError)) {
        return Promise.reject(error);
      }

      const status = error.response?.status ?? 0;
      const originalConfig = error.config as InternalTeamUpRequestConfig | undefined;

      if (
        status === 401 &&
        originalConfig &&
        !originalConfig._retry &&
        !originalConfig._skipAuthRefresh &&
        !isNonRefreshableAuthUrl(originalConfig.url)
      ) {
        const hasRefreshToken = Boolean(tokenStorage.getRefreshToken());
        const hadAuthHeader = Boolean(readHeaderValue(originalConfig.headers, "Authorization"));

        if (!hasRefreshToken) {
          if (hadAuthHeader) {
            tokenStorage.clear();
            notifySessionExpired();
          }
          return Promise.reject(normalizeAxiosError(error));
        }

        try {
          const newAccessToken = await performSingleFlightRefresh();
          originalConfig._retry = true;
          originalConfig.headers = AxiosHeaders.from(originalConfig.headers);
          originalConfig.headers.set("Authorization", `Bearer ${newAccessToken}`);
          return await instance.request(originalConfig);
        } catch (refreshError: unknown) {
          return Promise.reject(refreshError);
        }
      }

      return Promise.reject(normalizeAxiosError(error));
    },
  );

  return instance;
}

export const apiClient = createApiClient();

export function unwrapApiResponse<T>(response: AxiosResponse<ApiResponse<T>>): T {
  const payload = response.data;
  if (!payload || !payload.success || payload.data === undefined) {
    throw new ApiClientError(
      payload?.message || "Unexpected empty response from server.",
      payload?.statusCode || response.status,
      payload?.errors ?? [],
    );
  }
  return payload.data;
}

/**
 * Safe helper for 204 No Content and empty-body 2xx responses (DELETE, logout, mark-read, unassign).
 */
export function unwrapVoidResponse(response: AxiosResponse<unknown>): void {
  if (response.status === 204) {
    return;
  }
  const payload = response.data as Partial<ApiResponse> | undefined;
  if (payload && typeof payload === "object" && payload.success === false) {
    throw new ApiClientError(
      payload.message || "Request failed.",
      payload.statusCode || response.status,
      payload.errors ?? [],
    );
  }
}

/**
 * Fetches an authenticated HTML document (e.g., GET /api/v1/sessions/{id}/attendance/report)
 * as raw text ready to be rendered in a sandboxed <iframe srcDoc={html} sandbox="allow-same-origin" />.
 */
export async function fetchApiHtml(
  url: string,
  config?: TeamUpRequestConfig,
  client: AxiosInstance = apiClient,
): Promise<string> {
  const response = await client.get<string>(url, {
    ...config,
    responseType: "text",
    headers: {
      ...config?.headers,
      Accept: "text/html",
    },
  });
  return typeof response.data === "string" ? response.data : String(response.data ?? "");
}
