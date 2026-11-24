/**
 * @date 2026-11-24
 * @purpose Pure helpers for QR countdown timers, timezone-safe datetime-local conversion, attendance origin labels, and student registration persistence.
 */
import type { RegistrationStatus, SessionAttendance } from "@/api";

export const DEFAULT_CHECKIN_WINDOW_SECONDS = 60;

export function secondsUntilExpiry(expiresAtUtc: string, nowMs = Date.now()): number {
  const parsed = Date.parse(expiresAtUtc);
  if (Number.isNaN(parsed)) {
    return DEFAULT_CHECKIN_WINDOW_SECONDS;
  }
  return Math.max(0, Math.ceil((parsed - nowMs) / 1000));
}

export function nextCountdownTick(currentSeconds: number): {
  seconds: number;
  shouldRefresh: boolean;
} {
  if (currentSeconds <= 1) {
    return { seconds: 0, shouldRefresh: true };
  }
  return { seconds: currentSeconds - 1, shouldRefresh: false };
}

/**
 * Converts a backend ISO-8601 UTC timestamp into local `YYYY-MM-DDTHH:mm` for `<input type="datetime-local">`
 * without silently shifting by the user's UTC timezone offset.
 */
export function toLocalDateTimeInput(isoString: string | null | undefined): string {
  if (!isoString) {
    return "";
  }
  const date = new Date(isoString);
  if (Number.isNaN(date.getTime())) {
    return "";
  }
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

/**
 * Converts a local `YYYY-MM-DDTHH:mm` input string to an unambiguous UTC ISO-8601 string for the backend.
 */
export function fromLocalDateTimeInput(localValue: string): string {
  const parsed = new Date(localValue);
  if (Number.isNaN(parsed.getTime())) {
    return localValue;
  }
  return parsed.toISOString();
}

/**
 * Accurately labels attendance origin: backend stores `recordedByStudentId === studentId` for student self check-ins.
 */
export function formatAttendanceRecorder(
  attendance: Pick<SessionAttendance, "studentId" | "recordedByStudentId">,
): string {
  if (
    attendance.recordedByStudentId === null ||
    attendance.recordedByStudentId === undefined ||
    attendance.recordedByStudentId === attendance.studentId
  ) {
    return "Self check-in";
  }
  return `Organizer #${attendance.recordedByStudentId}`;
}

export interface SavedEventRegistration {
  status: RegistrationStatus;
  registeredAt: string;
}

function regStorageKey(studentId: number, eventId: number): string {
  return `teamup.eventReg.${studentId}.${eventId}`;
}

export const myEventRegistrationStorage = {
  get(studentId: number | undefined, eventId: number): SavedEventRegistration | null {
    if (!studentId || eventId <= 0 || typeof window === "undefined") {
      return null;
    }
    try {
      const raw = window.localStorage.getItem(regStorageKey(studentId, eventId));
      if (!raw) {
        return null;
      }
      const parsed = JSON.parse(raw) as Partial<SavedEventRegistration>;
      if (parsed.status !== "Confirmed" && parsed.status !== "Waitlisted") {
        return null;
      }
      return {
        status: parsed.status,
        registeredAt:
          typeof parsed.registeredAt === "string" ? parsed.registeredAt : new Date().toISOString(),
      };
    } catch {
      return null;
    }
  },

  set(
    studentId: number | undefined,
    eventId: number,
    value: SavedEventRegistration,
  ): void {
    if (!studentId || eventId <= 0 || typeof window === "undefined") {
      return;
    }
    try {
      window.localStorage.setItem(regStorageKey(studentId, eventId), JSON.stringify(value));
    } catch {
      // Ignore storage quota failures
    }
  },

  clear(studentId: number | undefined, eventId: number): void {
    if (!studentId || eventId <= 0 || typeof window === "undefined") {
      return;
    }
    try {
      window.localStorage.removeItem(regStorageKey(studentId, eventId));
    } catch {
      // Ignore storage failures
    }
  },
};
