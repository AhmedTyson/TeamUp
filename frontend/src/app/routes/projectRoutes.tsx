/**
 * @date 2026-11-22
 * @purpose M2 (Ossama) route fragment owning Skills Matrix, Projects Discovery, Project Creation, and Squad Workspace routes.
 */
import { lazy } from "react";
import type { RouteObject } from "react-router-dom";
import { ProtectedRoute } from "@/features/auth";

const SkillsPage = lazy(() =>
  import("@/features/skills/SkillsPage").then((m) => ({ default: m.SkillsPage })),
);
const ProjectsDiscoveryPage = lazy(() =>
  import("@/features/projects/ProjectsDiscoveryPage").then((m) => ({
    default: m.ProjectsDiscoveryPage,
  })),
);
const ProjectCreatePage = lazy(() =>
  import("@/features/projects/ProjectCreatePage").then((m) => ({ default: m.ProjectCreatePage })),
);
const ProjectDetailPage = lazy(() =>
  import("@/features/projects/ProjectDetailPage").then((m) => ({ default: m.ProjectDetailPage })),
);

export const projectRoutes: RouteObject[] = [
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: "skills",
        element: <SkillsPage />,
      },
      {
        path: "projects",
        element: <ProjectsDiscoveryPage />,
      },
      {
        path: "projects/new",
        element: <ProjectCreatePage />,
      },
      {
        path: "projects/:id",
        element: <ProjectDetailPage />,
      },
    ],
  },
];
