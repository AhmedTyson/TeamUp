/**
 * @date 2026-11-24
 * @purpose M3 (Ibrahim) lazy-loaded route fragment owning Student Organizations, Multi-Session Events, Live Attendance Console, and Student Self Check-In.
 */
import { lazy } from "react";
import type { RouteObject } from "react-router-dom";
import { ProtectedRoute } from "@/features/auth/ProtectedRoute";

const OrganizationsDirectoryPage = lazy(() =>
  import("@/features/organizations/pages/OrganizationsDirectoryPage").then((m) => ({
    default: m.OrganizationsDirectoryPage,
  })),
);
const OrganizationDetailPage = lazy(() =>
  import("@/features/organizations/pages/OrganizationDetailPage").then((m) => ({
    default: m.OrganizationDetailPage,
  })),
);
const EventsDirectoryPage = lazy(() =>
  import("@/features/events/pages/EventsDirectoryPage").then((m) => ({
    default: m.EventsDirectoryPage,
  })),
);
const EventDetailPage = lazy(() =>
  import("@/features/events/pages/EventDetailPage").then((m) => ({
    default: m.EventDetailPage,
  })),
);
const SessionAttendancePage = lazy(() =>
  import("@/features/events/pages/SessionAttendancePage").then((m) => ({
    default: m.SessionAttendancePage,
  })),
);
const StudentCheckInPage = lazy(() =>
  import("@/features/events/pages/StudentCheckInPage").then((m) => ({
    default: m.StudentCheckInPage,
  })),
);

export const orgEventRoutes: RouteObject[] = [
  {
    path: "organizations",
    element: <OrganizationsDirectoryPage />,
  },
  {
    path: "organizations/:id",
    element: <OrganizationDetailPage />,
  },
  {
    path: "events",
    element: <EventsDirectoryPage />,
  },
  {
    path: "events/:id",
    element: <EventDetailPage />,
  },
  {
    path: "events/:id/sessions/:sessionId/attendance",
    element: (
      <ProtectedRoute>
        <SessionAttendancePage />
      </ProtectedRoute>
    ),
  },
  {
    path: "check-in",
    element: (
      <ProtectedRoute>
        <StudentCheckInPage />
      </ProtectedRoute>
    ),
  },
];
