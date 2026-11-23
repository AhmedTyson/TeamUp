/**
 * @date 2026-11-22
 * @purpose M2 (Ossama) route fragment owning Skills Matrix, Projects Discovery, Project Creation, and Squad Workspace routes.
 */
import type { RouteObject } from "react-router-dom";
import { ProtectedRoute } from "@/features/auth";
import { ProjectsHubPage } from "@/pages/ProjectsHubPage";

export const projectRoutes: RouteObject[] = [
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: "skills",
        element: <ProjectsHubPage mode="skills" />,
      },
      {
        path: "projects",
        element: <ProjectsHubPage mode="projects" />,
      },
      {
        path: "projects/new",
        element: <ProjectsHubPage mode="create" />,
      },
      {
        path: "projects/:id",
        element: <ProjectsHubPage mode="detail" />,
      },
    ],
  },
];
