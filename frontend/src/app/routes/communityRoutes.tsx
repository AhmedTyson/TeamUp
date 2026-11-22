/**
 * @date 2026-11-22
 * @purpose M4 (Youssef) route fragment owning Landing, Student Directory, Profiles, Leaderboard, Gamification, and Notifications routes.
 */
import type { RouteObject } from "react-router-dom";
import { ProposedHeroDemo } from "@/components/home/ProposedHeroDemo";
import { CommunityHubPage } from "@/pages/CommunityHubPage";
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
    path: "students",
    element: <CommunityHubPage mode="students" />,
  },
  {
    path: "students/:id",
    element: <CommunityHubPage mode="students" />,
  },
  {
    path: "profile/me",
    element: <CommunityHubPage mode="portfolio" />,
  },
  {
    path: "leaderboard",
    element: <CommunityHubPage mode="leaderboard" />,
  },
  {
    path: "gamification",
    element: <CommunityHubPage mode="gamification" />,
  },
  {
    path: "notifications",
    element: <CommunityHubPage mode="notifications" />,
  },
];
