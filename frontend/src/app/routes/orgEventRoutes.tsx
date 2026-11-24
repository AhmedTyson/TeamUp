/**
 * @date 2026-11-22
 * @purpose M3 (Ibrahim) route fragment owning Student Organizations, Multi-Session Events, and Live QR Attendance routes.
 */
import { lazy } from "react";
import type { RouteObject } from "react-router-dom";

const ActivitiesHubPage = lazy(() =>
  import("@/pages/ActivitiesHubPage").then((m) => ({ default: m.ActivitiesHubPage })),
);
const EventsHubPage = lazy(() =>
  import("@/pages/EventsHubPage").then((m) => ({ default: m.EventsHubPage })),
);

export const orgEventRoutes: RouteObject[] = [
  {
    path: "organizations",
    element: <ActivitiesHubPage mode="directory" />,
  },
  {
    path: "organizations/:id",
    element: <ActivitiesHubPage mode="detail" />,
  },
  {
    path: "events",
    element: <EventsHubPage mode="events" />,
  },
  {
    path: "events/:id",
    element: <EventsHubPage mode="detail" />,
  },
  {
    path: "events/:id/sessions/:sessionId/attendance",
    element: <EventsHubPage mode="attendance" />,
  },
  {
    path: "check-in",
    element: <EventsHubPage mode="check-in" />,
  },
];
