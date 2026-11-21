/**
 * @date 2026-11-21
 * @purpose M4 (Youssef) route fragment owning Landing, Student Directory, Profiles, Leaderboard, Gamification, and Notifications routes.
 */
import type { RouteObject } from "react-router-dom";
import { API_ENDPOINTS } from "@/api";
import { HomePage } from "@/pages/HomePage";
import { ModulePlaceholderPage } from "@/pages/ModulePlaceholderPage";

export const communityRoutes: RouteObject[] = [
  {
    index: true,
    element: <HomePage />,
  },
  {
    path: "students",
    element: (
      <ModulePlaceholderPage
        title="Student Directory"
        description="Privacy-hardened campus student discovery with faculty and academic year filters."
        endpoint={API_ENDPOINTS.students.list}
      />
    ),
  },
  {
    path: "students/:id",
    element: (
      <ModulePlaceholderPage
        title="Student Portfolio"
        description="Public student profile, verified skills, interests, and earned campus badges."
        endpoint={API_ENDPOINTS.students.list}
      />
    ),
  },
  {
    path: "profile/me",
    element: (
      <ModulePlaceholderPage
        title="My Account & Portfolio"
        description="Manage your bio, external links, directory email privacy, skills, and interest tags."
        endpoint={API_ENDPOINTS.students.me}
      />
    ),
  },
  {
    path: "leaderboard",
    element: (
      <ModulePlaceholderPage
        title="Campus Leaderboard"
        description="Top student contributors ranked by verifiable XP across projects and events."
        endpoint={API_ENDPOINTS.gamification.leaderboard}
      />
    ),
  },
  {
    path: "gamification",
    element: (
      <ModulePlaceholderPage
        title="Gamification & Badges"
        description="Immutable score ledger, progressive level ranks, achievement badges, and notifications."
        endpoint={API_ENDPOINTS.gamification.badges}
      />
    ),
  },
  {
    path: "notifications",
    element: (
      <ModulePlaceholderPage
        title="Notifications Inbox"
        description="Stay updated on project applications, assigned sprint tasks, earned badges, and event reminders."
        endpoint={API_ENDPOINTS.gamification.notifications}
      />
    ),
  },
];
