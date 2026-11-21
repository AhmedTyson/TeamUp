/**
 * @date 2026-11-21
 * @purpose M3 (Ibrahim) route fragment owning Student Organizations, Multi-Session Events, and Live QR Attendance routes.
 */
import type { RouteObject } from "react-router-dom";
import { API_ENDPOINTS } from "@/api";
import { ModulePlaceholderPage } from "@/pages/ModulePlaceholderPage";

export const orgEventRoutes: RouteObject[] = [
  {
    path: "organizations",
    element: (
      <ModulePlaceholderPage
        title="Student Organizations"
        description="University clubs, student chapters, leadership rosters, and membership applications."
        endpoint={API_ENDPOINTS.organizations.list}
      />
    ),
  },
  {
    path: "organizations/:id",
    element: (
      <ModulePlaceholderPage
        title="Organization Profile"
        description="Club leadership roster, membership recruitment, and hosted campus events."
        endpoint={API_ENDPOINTS.organizations.list}
      />
    ),
  },
  {
    path: "events",
    element: (
      <ModulePlaceholderPage
        title="Campus Events & Sessions"
        description="Multi-session workshops, hackathons, seat capacity registration, and waitlist promotion."
        endpoint={API_ENDPOINTS.events.list}
      />
    ),
  },
  {
    path: "events/:id",
    element: (
      <ModulePlaceholderPage
        title="Event Details"
        description="Real-time seat capacity, session timetable, and student registration."
        endpoint={API_ENDPOINTS.events.list}
      />
    ),
  },
  {
    path: "events/:id/sessions/:sessionId/attendance",
    element: (
      <ModulePlaceholderPage
        title="Live QR Attendance Console"
        description="60-second rotating QR/PIN projector, manual check-in, and sandboxed HTML attendance reports."
        endpoint={API_ENDPOINTS.sessions.checkInCode(0)}
      />
    ),
  },
  {
    path: "check-in",
    element: (
      <ModulePlaceholderPage
        title="Student Self Check-In"
        description="Verify your attendance with the active 60-second session PIN or scanned QR link."
        endpoint={API_ENDPOINTS.sessions.selfCheckIn(0)}
      />
    ),
  },
];
