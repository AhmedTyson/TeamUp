import { ProjectCreatePage } from "@/features/projects/ProjectCreatePage";
import { ProjectDetailPage } from "@/features/projects/ProjectDetailPage";
import { ProjectsDiscoveryPage } from "@/features/projects/ProjectsDiscoveryPage";
import { SkillsPage } from "@/features/skills/SkillsPage";

export interface ProjectsHubPageProps {
  mode?: "projects" | "create" | "detail" | "skills";
}

export function ProjectsHubPage({ mode = "projects" }: ProjectsHubPageProps) {
  if (mode === "skills") return <SkillsPage />;
  if (mode === "create") return <ProjectCreatePage />;
  if (mode === "detail") return <ProjectDetailPage />;
  return <ProjectsDiscoveryPage />;
}
