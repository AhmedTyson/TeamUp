/**
 * @date 2026-11-24
 * @purpose TanStack Query hooks and mutations for events, sessions, gated registrations, gated attendance, and gamification sync.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  attendanceApi,
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

export function useEvent(eventId: number) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: [...queryKeys.events.detail(eventId), isAuthenticated],
    queryFn: ({ signal }) =>
      isAuthenticated ? eventsApi.detail(eventId, signal) : eventsApi.publicDetail(eventId, signal),
    enabled: Number.isInteger(eventId) && eventId > 0,
  });
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
    ]);
    if (eventId && eventId > 0) {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.events.detail(eventId) }),
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
      queryKey: queryKeys.sessions.attendance(sessionId),
    });
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
