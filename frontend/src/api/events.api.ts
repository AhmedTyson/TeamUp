/**
 * @date 2026-11-24
 * @purpose Typed API client for event discovery, event lifecycle, registration, and waitlist operations.
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { EventDeliveryMode, EventType, RegistrationStatus } from "./enums";
import type { ApiResponse, PagedResult } from "./types";

export interface EventSession {
  sessionId: number;
  eventId: number;
  sessionNo: number;
  title: string;
  startTime: string;
  endTime: string;
  room: string | null;
}

export interface EventSummary {
  eventId: number;
  title: string;
  description: string;
  type: EventType;
  location: string;
  startDate: string;
  endDate: string;
  capacity: number;
  organizationId: number;
  organizationName: string;
  registeredCount: number;
  sessions: EventSession[];
  deliveryMode?: EventDeliveryMode;
  isPublic?: boolean;
  meetingUrl?: string | null;
  organizationUnitId?: number | null;
  organizationUnitName?: string | null;
}

export interface MyEventRegistration {
  eventId: number;
  eventTitle: string;
  organizationId: number;
  organizationName: string;
  eventType: EventType;
  deliveryMode: EventDeliveryMode;
  location: string;
  startDate: string;
  endDate: string;
  status: RegistrationStatus;
  registeredAt: string;
  sessionsAttendedCount: number;
  totalSessionsCount: number;
}

export interface EventRegistration {
  eventId: number;
  studentId: number;
  firstName: string;
  lastName: string;
  email: string;
  registeredAt: string;
  status: RegistrationStatus;
}

export interface EventListParams extends Record<string, unknown> {
  search?: string;
  organizationId?: number;
  type?: EventType;
  deliveryMode?: EventDeliveryMode;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface EventPayload {
  title: string;
  description: string;
  type: EventType;
  location: string;
  startDate: string;
  endDate: string;
  capacity: number;
  organizationId?: number;
  deliveryMode?: EventDeliveryMode;
  isPublic?: boolean;
  meetingUrl?: string | null;
  organizationUnitId?: number | null;
}

export interface SessionPayload {
  sessionNo: number;
  title: string;
  startTime: string;
  endTime: string;
  room: string | null;
}

export const eventsApi = {
  async list(
    params: EventListParams = {},
    signal?: AbortSignal,
  ): Promise<PagedResult<EventSummary>> {
    const response = await apiClient.get<ApiResponse<PagedResult<EventSummary>>>(
      API_ENDPOINTS.events.list,
      { params, signal },
    );
    return unwrapApiResponse(response);
  },

  async publicList(
    params: EventListParams = {},
    signal?: AbortSignal,
  ): Promise<PagedResult<EventSummary>> {
    const response = await apiClient.get<ApiResponse<PagedResult<EventSummary>>>(
      API_ENDPOINTS.events.publicList,
      { params, signal },
    );
    return unwrapApiResponse(response);
  },

  async detail(id: number, signal?: AbortSignal): Promise<EventSummary> {
    const response = await apiClient.get<ApiResponse<EventSummary>>(
      API_ENDPOINTS.events.byId(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async publicDetail(id: number, signal?: AbortSignal): Promise<EventSummary> {
    const response = await apiClient.get<ApiResponse<EventSummary>>(
      API_ENDPOINTS.events.publicById(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async myRegistrations(signal?: AbortSignal): Promise<MyEventRegistration[]> {
    const response = await apiClient.get<ApiResponse<MyEventRegistration[]>>(
      API_ENDPOINTS.events.myRegistrations,
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async create(payload: EventPayload): Promise<EventSummary> {
    const response = await apiClient.post<ApiResponse<EventSummary>>(
      API_ENDPOINTS.events.list,
      payload,
    );
    return unwrapApiResponse(response);
  },

  async update(
    id: number,
    payload: Omit<EventPayload, "organizationId">,
  ): Promise<EventSummary> {
    const response = await apiClient.patch<ApiResponse<EventSummary>>(
      API_ENDPOINTS.events.byId(id),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async remove(id: number): Promise<void> {
    const response = await apiClient.delete(API_ENDPOINTS.events.byId(id));
    unwrapVoidResponse(response);
  },

  async sessions(id: number, signal?: AbortSignal): Promise<EventSession[]> {
    const response = await apiClient.get<ApiResponse<EventSession[]>>(
      API_ENDPOINTS.events.sessions(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async createSession(id: number, payload: SessionPayload): Promise<EventSession> {
    const response = await apiClient.post<ApiResponse<EventSession>>(
      API_ENDPOINTS.events.sessions(id),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async updateSession(sessionId: number, payload: SessionPayload): Promise<EventSession> {
    const response = await apiClient.put<ApiResponse<EventSession>>(
      API_ENDPOINTS.sessions.byId(sessionId),
      payload,
    );
    return unwrapApiResponse(response);
  },

  async removeSession(sessionId: number): Promise<void> {
    const response = await apiClient.delete(API_ENDPOINTS.sessions.byId(sessionId));
    unwrapVoidResponse(response);
  },

  async registrations(id: number, signal?: AbortSignal): Promise<EventRegistration[]> {
    const response = await apiClient.get<ApiResponse<EventRegistration[]>>(
      API_ENDPOINTS.events.registrations(id),
      { signal },
    );
    return unwrapApiResponse(response);
  },

  async register(id: number): Promise<EventRegistration> {
    const response = await apiClient.post<ApiResponse<EventRegistration>>(
      API_ENDPOINTS.events.registrations(id),
    );
    return unwrapApiResponse(response);
  },

  async cancelRegistration(id: number, studentId?: number): Promise<void> {
    const response = await apiClient.delete(API_ENDPOINTS.events.registrations(id), {
      params: studentId ? { studentId } : undefined,
    });
    unwrapVoidResponse(response);
  },
};
