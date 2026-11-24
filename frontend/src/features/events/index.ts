/**
 * @date 2026-11-24
 * @purpose Barrel exports for TeamUp's events, sessions, attendance console, Digital Pass, Stage Mode, and student self check-in module (M3).
 */
export { EventsDirectoryPage } from "./pages/EventsDirectoryPage";
export { EventDetailPage } from "./pages/EventDetailPage";
export { SessionAttendancePage } from "./pages/SessionAttendancePage";
export { StudentCheckInPage } from "./pages/StudentCheckInPage";
export { EventDigitalPassCard } from "./components/EventDigitalPassCard";
export { StageProjectorOverlay } from "./components/StageProjectorOverlay";
export { QrCameraScannerModal, parseScannedCheckInPayload } from "./components/QrCameraScannerModal";
export {
  DEFAULT_CHECKIN_WINDOW_SECONDS,
  formatAttendanceRecorder,
  fromLocalDateTimeInput,
  myEventRegistrationStorage,
  nextCountdownTick,
  secondsUntilExpiry,
  toLocalDateTimeInput,
} from "./utils/attendance";
export {
  useAttendanceMutations,
  useEvent,
  useEventMutations,
  useEventRegistrations,
  useEvents,
  useEventSessions,
  useMyEventRegistrations,
  useSessionAttendance,
} from "./hooks/useEventHooks";
