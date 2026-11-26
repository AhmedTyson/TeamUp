/**
 * @date 2026-11-22
 * @purpose M4 (Youssef) route fragment owning Landing, Student Directory, Profiles, Leaderboard, Gamification, and Notifications routes.
 */
import { lazy } from "react";
import type { RouteObject } from "react-router-dom";
import { ProtectedRoute } from "@/features/auth";
import { HomePage } from "@/pages/HomePage";

const ProposedHeroDemo = lazy(() =>
  import("@/components/home/ProposedHeroDemo").then((m) => ({ default: m.ProposedHeroDemo })),
);
const PrivacyPolicyPage = lazy(() =>
  import("@/pages/PrivacyPolicyPage").then((m) => ({ default: m.PrivacyPolicyPage })),
);
const StudentDirectoryPage = lazy(() =>
  import("@/features/profile/StudentDirectoryPage").then((m) => ({
    default: m.StudentDirectoryPage,
  })),
);
const StudentPublicProfilePage = lazy(() =>
  import("@/features/profile/StudentPublicProfilePage").then((m) => ({
    default: m.StudentPublicProfilePage,
  })),
);
const SelfProfileStudioPage = lazy(() =>
  import("@/features/profile/SelfProfileStudioPage").then((m) => ({
    default: m.SelfProfileStudioPage,
  })),
);
const CampusLeaderboardPage = lazy(() =>
  import("@/features/gamification/CampusLeaderboardPage").then((m) => ({
    default: m.CampusLeaderboardPage,
  })),
);
const ExplorePage = lazy(() =>
  import("@/features/explore/ExplorePage").then((m) => ({ default: m.ExplorePage })),
);
const GamificationHubPage = lazy(() =>
  import("@/features/gamification/GamificationHubPage").then((m) => ({
    default: m.GamificationHubPage,
  })),
);
const NotificationsInboxPage = lazy(() =>
  import("@/features/notifications/NotificationsInboxPage").then((m) => ({
    default: m.NotificationsInboxPage,
  })),
);
const RoleDashboardRouterPage = lazy(() =>
  import("@/features/dashboard").then((m) => ({
    default: m.RoleDashboardRouterPage,
  })),
);
const StudentDashboardPage = lazy(() =>
  import("@/features/dashboard").then((m) => ({
    default: m.StudentDashboardPage,
  })),
);
const OrgAdminDashboardPage = lazy(() =>
  import("@/features/dashboard").then((m) => ({
    default: m.OrgAdminDashboardPage,
  })),
);

export const communityRoutes: RouteObject[] = [
  {
    index: true,
    element: <HomePage />,
  },
  {
    path: "demo/hero",
    element: <ProposedHeroDemo />,
  },
  {
    path: "privacy",
    element: <PrivacyPolicyPage />,
  },
  {
    path: "policy",
    element: <PrivacyPolicyPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: "students",
        element: <StudentDirectoryPage />,
      },
      {
        path: "students/:id",
        element: <StudentPublicProfilePage />,
      },
      {
        path: "profile/me",
        element: <SelfProfileStudioPage />,
      },
      {
        path: "leaderboard",
        element: <CampusLeaderboardPage />,
      },
      {
        path: "gamification",
        element: <GamificationHubPage />,
      },
      {
        path: "notifications",
        element: <NotificationsInboxPage />,
      },
      {
        path: "explore",
        element: <ExplorePage />,
      },
      {
        path: "dashboard",
        element: <RoleDashboardRouterPage />,
      },
      {
        path: "dashboard/student",
        element: <StudentDashboardPage />,
      },
      {
        path: "dashboard/org",
        element: <OrgAdminDashboardPage />,
      },
    ],
  },
];
