/**
 * @date 2026-11-24
 * @purpose Typed API client for session scheduling operations under events.
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { EventSession, SessionPayload } from "./events.api";
import type { ApiResponse } from "./types";

export const sessionsApi = {
  async listForEvent(eventId: number, signal?: AbortSignal): Promise<EventSession[]> {
    const response = await apiClient.get<ApiResponse<EventSession[]>>(
      API_ENDPOINTS.events.sessions(eventId),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async create(eventId: number, payload: SessionPayload): Promise<EventSession> {
    const response = await apiClient.post<ApiResponse<EventSession>>(
      API_ENDPOINTS.events.sessions(eventId),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async update(sessionId: number, payload: SessionPayload): Promise<EventSession> {
    const response = await apiClient.put<ApiResponse<EventSession>>(
      API_ENDPOINTS.sessions.byId(sessionId),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async remove(sessionId: number): Promise<void> {
    const response = await apiClient.delete(API_ENDPOINTS.sessions.byId(sessionId));
    unwrapVoidResponse(response);
  },
};
