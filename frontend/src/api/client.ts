/**
 * @date 2026-11-21
 * @purpose Configured Axios HTTP client with JWT Bearer injection and TeamUp envelope error normalization.
 */
import axios, { AxiosError, type AxiosInstance, type AxiosResponse } from "axios";
import { env } from "@/lib/env";
import { tokenStorage } from "@/lib/tokenStorage";
import type { ApiResponse, ErrorDetail } from "./types";

export class ApiClientError extends Error {
  readonly statusCode: number;
  readonly errors: ErrorDetail[];

  constructor(message: string, statusCode: number, errors: ErrorDetail[] = []) {
    super(message);
    this.name = "ApiClientError";
    this.statusCode = statusCode;
    this.errors = errors;
  }
}

export function createApiClient(baseURL: string = env.apiBaseUrl): AxiosInstance {
  const instance = axios.create({
    baseURL,
    timeout: 15_000,
    headers: {
      "Content-Type": "application/json",
      Accept: "application/json",
    },
  });

  instance.interceptors.request.use((config) => {
    const token = tokenStorage.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  instance.interceptors.response.use(
    (response) => response,
    (error: unknown) => {
      if (error instanceof AxiosError) {
        const status = error.response?.status ?? 0;
        const payload = error.response?.data as ApiResponse | undefined;
        const message = payload?.message || error.message || "Network request failed.";
        const errors = payload?.errors ?? [];
        return Promise.reject(new ApiClientError(message, status, errors));
      }
      return Promise.reject(error);
    },
  );

  return instance;
}

export const apiClient = createApiClient();

export function unwrapApiResponse<T>(response: AxiosResponse<ApiResponse<T>>): T {
  const payload = response.data;
  if (!payload.success || payload.data === undefined) {
    throw new ApiClientError(
      payload.message || "Unexpected empty response from server.",
      payload.statusCode || response.status,
      payload.errors ?? [],
    );
  }
  return payload.data;
}
