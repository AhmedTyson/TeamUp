/**
 * @date 2026-11-22
 * @purpose M4 (Youssef) route fragment owning Landing, Student Directory, Profiles, Leaderboard, Gamification, and Notifications routes.
 */
import type { RouteObject } from "react-router-dom";
import { ProposedHeroDemo } from "@/components/home/ProposedHeroDemo";
import { ProtectedRoute } from "@/features/auth";
import {
  CampusLeaderboardPage,
  GamificationHubPage,
} from "@/features/gamification";
import { NotificationsInboxPage } from "@/features/notifications";
import {
  SelfProfileStudioPage,
  StudentDirectoryPage,
  StudentPublicProfilePage,
} from "@/features/profile";
import { HomePage } from "@/pages/HomePage";

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
    ],
  },
];
