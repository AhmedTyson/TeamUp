/**
 * @date 2026-11-22
 * @purpose M3 (Ibrahim) route fragment owning Student Organizations, Multi-Session Events, and Live QR Attendance routes.
 */
import type { RouteObject } from "react-router-dom";
import { ActivitiesHubPage } from "@/pages/ActivitiesHubPage";
import { EventsHubPage } from "@/pages/EventsHubPage";

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
