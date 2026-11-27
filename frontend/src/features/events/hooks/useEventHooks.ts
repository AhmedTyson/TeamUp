/**
 * @date 2026-11-24
 * @purpose TanStack Query hooks and mutations for events, sessions, gated registrations, gated attendance, and gamification sync.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  attendanceApi,
  workspacesApi,
  eventsApi,
  sessionsApi,
  type EventListParams,
  type EventPayload,
  type ManualAttendancePayload,
  type SessionPayload,
} from "@/api";
import { useAuth } from "@/features/auth/AuthContext";
import { invalidateGamification, queryKeys } from "@/lib/queryKeys";

export function useEvents(params: EventListParams, enabled = true) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: [...queryKeys.events.list(params), isAuthenticated],
    queryFn: ({ signal }) =>
      isAuthenticated ? eventsApi.list(params, signal) : eventsApi.publicList(params, signal),
    enabled,
  });
}

export function useEvent(eventId: number, enabled = true) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: [...queryKeys.events.detail(eventId), isAuthenticated],
    queryFn: ({ signal }) =>
      isAuthenticated ? eventsApi.detail(eventId, signal) : eventsApi.publicDetail(eventId, signal),
    enabled: enabled && Number.isInteger(eventId) && eventId > 0,
  });
}

/**
 * One request for the whole event screen: event + sessions, whether the caller can manage it, their own
 * registration, and (organizers only) the registrations list. Anonymous visitors fall back to the public detail.
 * `myRegistration` stays undefined for anonymous visitors so callers can use their local pass cache instead.
 */
export function useEventWorkspace(eventId: number) {
  const { isAuthenticated } = useAuth();
  const isValid = Number.isInteger(eventId) && eventId > 0;
  const workspaceQuery = useQuery({
    queryKey: queryKeys.events.workspace(eventId),
    queryFn: ({ signal }) => workspacesApi.eventWorkspace(eventId, signal),
    enabled: isValid && isAuthenticated,
  });
  const publicQuery = useEvent(eventId, !isAuthenticated);
  const active = isAuthenticated ? workspaceQuery : publicQuery;

  return {
    event: isAuthenticated ? workspaceQuery.data?.event : publicQuery.data,
    canManage: isAuthenticated ? Boolean(workspaceQuery.data?.canManage) : false,
    myRegistration: isAuthenticated ? workspaceQuery.data?.myRegistration : undefined,
    registrations: isAuthenticated ? (workspaceQuery.data?.registrations ?? null) : null,
    isLoading: active.isLoading,
    isError: active.isError,
    error: active.error,
    refetch: active.refetch,
  };
}

/**
 * Single-request organizer attendance workspace: loads event, session, permission check,
 * registrations, and verified attendance in one round-trip.
 */
export function useSessionAttendanceWorkspace(eventId: number, sessionId: number) {
  const { isAuthenticated } = useAuth();
  const isValid =
    Number.isInteger(eventId) && eventId > 0 && Number.isInteger(sessionId) && sessionId > 0;

  const query = useQuery({
    queryKey: queryKeys.sessions.attendanceWorkspace(eventId, sessionId),
    queryFn: ({ signal }) => workspacesApi.sessionAttendanceWorkspace(eventId, sessionId, signal),
    enabled: isValid && isAuthenticated,
  });

  return {
    event: query.data?.event,
    session: query.data?.session ?? null,
    canManage: Boolean(query.data?.canManage),
    registrations: query.data?.registrations ?? [],
    attendance: query.data?.attendance ?? [],
    isLoading: query.isLoading,
    isError: query.isError,
    error: query.error,
    refetch: query.refetch,
  };
}
export function useMyEventRegistrations(enabled = true, page = 1, pageSize = 20) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: queryKeys.events.myRegistrations(page, pageSize),
    queryFn: ({ signal }) => eventsApi.myRegistrations(page, pageSize, signal),
    enabled: isAuthenticated && enabled,
  });
}

// Per-event "am I registered?" lookup. The paginated my-registrations list cannot answer this:
// a registration older than page 1 would look like "not registered".
export function useMyEventRegistration(eventId: number, enabled = true) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: queryKeys.events.myRegistration(eventId),
    queryFn: ({ signal }) => eventsApi.myRegistration(eventId, signal),
    enabled: isAuthenticated && enabled && Number.isInteger(eventId) && eventId > 0,
  });
}

export function useEventSessions(eventId: number) {
  return useQuery({
    queryKey: queryKeys.events.sessions(eventId),
    queryFn: ({ signal }) => sessionsApi.listForEvent(eventId, signal),
    enabled: Number.isInteger(eventId) && eventId > 0,
  });
}

export function useEventRegistrations(eventId: number, enabled: boolean) {
  return useQuery({
    queryKey: queryKeys.events.registrations(eventId),
    queryFn: ({ signal }) => eventsApi.registrations(eventId, signal),
    enabled: Number.isInteger(eventId) && eventId > 0 && enabled,
  });
}

export function useSessionAttendance(sessionId: number, enabled: boolean) {
  return useQuery({
    queryKey: queryKeys.sessions.attendance(sessionId),
    queryFn: ({ signal }) => attendanceApi.list(sessionId, signal),
    enabled: Number.isInteger(sessionId) && sessionId > 0 && enabled,
  });
}

export function useEventMutations(eventId?: number) {
  const queryClient = useQueryClient();

  const invalidateEvent = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: queryKeys.events.all }),
      queryClient.invalidateQueries({ queryKey: queryKeys.organizations.all }),
      queryClient.invalidateQueries({ queryKey: queryKeys.dashboards.all }),
      queryClient.invalidateQueries({ queryKey: queryKeys.sessions.all }),
    ]);
    if (eventId && eventId > 0) {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.events.detail(eventId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.events.workspace(eventId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.events.sessions(eventId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.events.registrations(eventId) }),
      ]);
    }
  };

  const create = useMutation({
    mutationFn: (payload: EventPayload) => eventsApi.create(payload),
    onSuccess: invalidateEvent,
  });

  const update = useMutation({
    mutationFn: (payload: Omit<EventPayload, "organizationId">) =>
      eventsApi.update(eventId ?? 0, payload),
    onSuccess: invalidateEvent,
  });

  const remove = useMutation({
    mutationFn: () => eventsApi.remove(eventId ?? 0),
    onSuccess: invalidateEvent,
  });

  const register = useMutation({
    mutationFn: () => eventsApi.register(eventId ?? 0),
    onSuccess: invalidateEvent,
  });

  const cancelRegistration = useMutation({
    mutationFn: (studentId?: number) =>
      eventsApi.cancelRegistration(eventId ?? 0, studentId),
    onSuccess: invalidateEvent,
  });

  const createSession = useMutation({
    mutationFn: (payload: SessionPayload) => sessionsApi.create(eventId ?? 0, payload),
    onSuccess: invalidateEvent,
  });

  const updateSession = useMutation({
    mutationFn: ({ sessionId, payload }: { sessionId: number; payload: SessionPayload }) =>
      sessionsApi.update(sessionId, payload),
    onSuccess: invalidateEvent,
  });

  const removeSession = useMutation({
    mutationFn: (sessionId: number) => sessionsApi.remove(sessionId),
    onSuccess: invalidateEvent,
  });

  return {
    create,
    update,
    remove,
    register,
    cancelRegistration,
    createSession,
    updateSession,
    removeSession,
  };
}

export function useAttendanceMutations(sessionId: number) {
  const queryClient = useQueryClient();
  const { user } = useAuth();

  const syncAttendance = async (affectedStudentId?: number) => {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.sessions.all,
    });
    await queryClient.invalidateQueries({ queryKey: queryKeys.events.all });
    await invalidateGamification(queryClient, affectedStudentId ?? user?.studentId);
  };

  const generateCode = useMutation({
    mutationFn: () => attendanceApi.generateCode(sessionId),
  });

  const manualCheckIn = useMutation({
    mutationFn: (payload: ManualAttendancePayload) =>
      attendanceApi.checkIn(sessionId, payload),
    onSuccess: async (record) => {
      await syncAttendance(record.studentId);
    },
  });

  const revoke = useMutation({
    mutationFn: (studentId: number) => attendanceApi.revoke(sessionId, studentId),
    onSuccess: async (_data, studentId) => {
      await syncAttendance(studentId);
    },
  });

  const report = useMutation({
    mutationFn: () => attendanceApi.getReportHtml(sessionId),
  });

  const exportCsv = useMutation({
    mutationFn: () => attendanceApi.exportCsv(sessionId),
  });

  return {
    generateCode,
    manualCheckIn,
    revoke,
    report,
    exportCsv,
  };
}
