/**
 * @date 2026-11-21
 * @purpose Shared TypeScript contracts matching TeamUp.Api's BaseResponse<T> and PagedResult<T> envelopes.
 */
export interface ErrorDetail {
  field: string;
  message: string;
}

export interface ApiResponse<T = undefined> {
  success: boolean;
  message: string;
  statusCode: number;
  data?: T;
  errors?: ErrorDetail[] | null;
  timestamp: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface HealthStatus {
  status: string;
  database?: string;
}
