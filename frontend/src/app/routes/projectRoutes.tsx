/**
 * @date 2026-11-21
 * @purpose M2 (Ossama) route fragment owning Skills Matrix, Projects Discovery, Project Workspace, and Kanban Board routes.
 */
import type { RouteObject } from "react-router-dom";
import { API_ENDPOINTS } from "@/api";
import { ModulePlaceholderPage } from "@/pages/ModulePlaceholderPage";

export const projectRoutes: RouteObject[] = [
  {
    path: "skills",
    element: (
      <ModulePlaceholderPage
        title="Skills Catalog & Claims"
        description="Browse campus technical skills, manage personal proficiency claims, and match into projects."
        endpoint={API_ENDPOINTS.skills.list}
      />
    ),
  },
  {
    path: "projects",
    element: (
      <ModulePlaceholderPage
        title="Projects & Teams"
        description="Cross-faculty project discovery, required skills matching, applications, and Kanban task boards."
        endpoint={API_ENDPOINTS.projects.list}
      />
    ),
  },
  {
    path: "projects/new",
    element: (
      <ModulePlaceholderPage
        title="Create Project"
        description="Launch a new student project and attach required skills for teammates."
        endpoint={API_ENDPOINTS.projects.list}
      />
    ),
  },
  {
    path: "projects/:id",
    element: (
      <ModulePlaceholderPage
        title="Project Workspace"
        description="Project overview, required skills matrix, team roster, applications queue, and sprint Kanban board."
        endpoint={API_ENDPOINTS.projects.list}
      />
    ),
  },
];
