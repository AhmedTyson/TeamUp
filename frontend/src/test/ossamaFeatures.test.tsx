import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  APPLICATION_STATUSES,
  ApiClientError,
  PROFICIENCY_LEVELS,
  PROJECT_STATUSES,
  PROJECT_TASK_STATUSES,
  TASK_PRIORITIES,
  USER_ROLES,
  projectsApi,
  skillsApi,
  tasksApi,
  type AuthStudentSummary,
  type ProjectApplicationDto,
  type ProjectDetailDto,
  type ProjectTaskDto,
} from "@/api";
import { AppCoreProviders } from "@/app/providers";
import { ProjectDetailPage } from "@/features/projects/ProjectDetailPage";
import { ProjectsDiscoveryPage } from "@/features/projects/ProjectsDiscoveryPage";
import { StudentSkillsPanel } from "@/features/skills/StudentSkillsPanel";
import { toast } from "sonner";

const STUDENT: AuthStudentSummary = {
  studentId: 7,
  firstName: "Ossama",
  lastName: "Student",
  email: "ossama@example.test",
  academicYear: 3,
  role: USER_ROLES.Student,
  facultyId: 2,
  facultyName: "Computing",
  universityName: "TeamUp University",
};

const PROJECT: ProjectDetailDto = {
  projectId: 42,
  title: "Campus Planner",
  description: "A shared project for testing the frontend workspace.",
  status: PROJECT_STATUSES.Recruiting,
  maxTeamSize: 4,
  teamSize: 2,
  creatorId: STUDENT.studentId,
  creatorName: `${STUDENT.firstName} ${STUDENT.lastName}`,
  createdAt: "2026-11-20T10:00:00Z",
  members: [
    {
      studentId: STUDENT.studentId,
      firstName: STUDENT.firstName,
      lastName: STUDENT.lastName,
      role: "Lead",
      joinedAt: "2026-11-20T10:00:00Z",
    },
    {
      studentId: 8,
      firstName: "Mona",
      lastName: "Member",
      role: "Member",
      joinedAt: "2026-11-20T11:00:00Z",
    },
  ],
  requiredSkills: [],
};

const TASK: ProjectTaskDto = {
  taskId: 90,
  title: "Build API integration",
  description: "Connect project tasks to the service.",
  status: PROJECT_TASK_STATUSES.Todo,
  priority: TASK_PRIORITIES.High,
  dueDate: null,
  projectId: PROJECT.projectId,
  assignedStudentId: STUDENT.studentId,
  assignedStudentName: `${STUDENT.firstName} ${STUDENT.lastName}`,
};

function renderAt(
  path: string,
  routes: React.ReactNode,
  initialUser: AuthStudentSummary | null = STUDENT,
) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <AppCoreProviders initialUser={initialUser}>{routes}</AppCoreProviders>
    </MemoryRouter>,
  );
}

function renderProject(path = "/projects/42") {
  return renderAt(
    path,
    <Routes>
      <Route path="/projects/:id" element={<ProjectDetailPage />} />
    </Routes>,
  );
}

describe("M2 Ossama skills and project workflows", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("sends URL-synced discovery filters and pagination to the projects API", async () => {
    const listSpy = vi.spyOn(projectsApi, "list").mockResolvedValue({
      items: [],
      page: 2,
      pageSize: 12,
      totalCount: 0,
      totalPages: 1,
    });
    vi.spyOn(skillsApi, "list").mockResolvedValue([
      { skillId: 3, name: "React", category: "Frontend", description: null },
    ]);

    renderAt(
      "/projects?keyword=react&status=Recruiting&skillId=3&page=2",
      <Routes><Route path="/projects" element={<ProjectsDiscoveryPage />} /></Routes>,
    );

    await waitFor(() => {
      expect(listSpy).toHaveBeenCalledWith(
        { keyword: "react", status: "Recruiting", skillId: 3, page: 2, pageSize: 12 },
        expect.any(AbortSignal),
      );
    });
    fireEvent.change(screen.getByLabelText("Status"), { target: { value: "Completed" } });
    await waitFor(() => {
      expect(listSpy).toHaveBeenLastCalledWith(
        { keyword: "react", status: "Completed", skillId: 3, page: 1, pageSize: 12 },
        expect.any(AbortSignal),
      );
    });
  });

  it("shows a clear capacity message when accepting an application conflicts", async () => {
    const applicant: ProjectApplicationDto = {
      applicationId: 11,
      projectId: PROJECT.projectId,
      studentId: 12,
      studentName: "Rana Applicant",
      message: "I can help with frontend.",
      status: APPLICATION_STATUSES.Pending,
      appliedAt: "2026-11-22T09:00:00Z",
    };
    vi.spyOn(projectsApi, "getById").mockResolvedValue(PROJECT);
    vi.spyOn(projectsApi, "getRequiredSkills").mockResolvedValue([]);
    vi.spyOn(projectsApi, "getMembers").mockResolvedValue(PROJECT.members);
    vi.spyOn(projectsApi, "getApplications").mockResolvedValue([applicant]);
    vi.spyOn(skillsApi, "list").mockResolvedValue([]);
    vi.spyOn(skillsApi, "getStudentSkills").mockResolvedValue([]);
    vi.spyOn(projectsApi, "updateApplication").mockRejectedValue(
      new ApiClientError("The project team is full.", 409),
    );
    const toastError = vi.spyOn(toast, "error");

    renderProject();
    fireEvent.click(await screen.findByRole("button", { name: "Applications" }));
    fireEvent.click(await screen.findByRole("button", { name: "Accept" }));

    await waitFor(() => {
      expect(toastError).toHaveBeenCalledWith(
        "The team is full. Refresh the project before accepting another applicant.",
      );
    });
    expect(projectsApi.updateApplication).toHaveBeenCalledWith(
      PROJECT.projectId,
      applicant.applicationId,
      APPLICATION_STATUSES.Accepted,
    );
  });

  it("creates a task, moves it to Done, and invalidates the XP feedback", async () => {
    vi.spyOn(projectsApi, "getById").mockResolvedValue(PROJECT);
    vi.spyOn(projectsApi, "getRequiredSkills").mockResolvedValue([]);
    vi.spyOn(projectsApi, "getMembers").mockResolvedValue(PROJECT.members);
    vi.spyOn(projectsApi, "getApplications").mockResolvedValue([]);
    vi.spyOn(skillsApi, "list").mockResolvedValue([]);
    vi.spyOn(skillsApi, "getStudentSkills").mockResolvedValue([]);
    vi.spyOn(tasksApi, "listForProject").mockResolvedValue([TASK]);
    const createTask = vi.spyOn(tasksApi, "createForProject").mockResolvedValue({
      ...TASK,
      taskId: 91,
      title: "Add task form",
    });
    const updateTask = vi.spyOn(tasksApi, "update").mockResolvedValue({
      ...TASK,
      status: PROJECT_TASK_STATUSES.Done,
    });
    const toastSuccess = vi.spyOn(toast, "success");

    renderProject();
    fireEvent.click(await screen.findByRole("button", { name: "Sprint board" }));
    fireEvent.click(await screen.findByRole("button", { name: "Create task" }));
    const dialog = await screen.findByRole("dialog");
    fireEvent.change(within(dialog).getByLabelText("Title"), { target: { value: "Add task form" } });
    fireEvent.change(within(dialog).getByLabelText("Description"), { target: { value: "Implement task creation." } });
    fireEvent.click(within(dialog).getByRole("button", { name: "Create task" }));
    await waitFor(() => expect(createTask).toHaveBeenCalledWith(PROJECT.projectId, {
      title: "Add task form",
      description: "Implement task creation.",
      dueDate: null,
      priority: TASK_PRIORITIES.Medium,
    }));

    fireEvent.change(await screen.findByLabelText("Move Build API integration"), {
      target: { value: PROJECT_TASK_STATUSES.Done },
    });
    await waitFor(() => expect(updateTask).toHaveBeenCalledWith(TASK.taskId, {
      title: TASK.title,
      description: TASK.description,
      dueDate: null,
      priority: TASK.priority,
      status: PROJECT_TASK_STATUSES.Done,
    }));
    await waitFor(() => expect(toastSuccess).toHaveBeenCalledWith("+50 XP awarded!"));
  });

  it("upserts a skill claim and supports removing a claimed skill", async () => {
    vi.spyOn(skillsApi, "list").mockResolvedValue([
      { skillId: 4, name: "TypeScript", category: "Frontend", description: null },
      { skillId: 5, name: "Python", category: "Backend", description: null },
    ]);
    vi.spyOn(skillsApi, "getStudentSkills").mockResolvedValue([
      {
        skillId: 4,
        name: "TypeScript",
        category: "Frontend",
        proficiencyLevel: PROFICIENCY_LEVELS.Intermediate,
        yearsOfExperience: 2,
      },
    ]);
    const claimSpy = vi.spyOn(skillsApi, "claimMine").mockResolvedValue();
    const removeSpy = vi.spyOn(skillsApi, "removeMine").mockResolvedValue();

    renderAt("/profile/me", <StudentSkillsPanel studentId={STUDENT.studentId} editable />);

    fireEvent.change(await screen.findByLabelText("Add a skill"), { target: { value: "5" } });
    fireEvent.change(screen.getAllByRole("spinbutton", { name: "Years of experience" })[0]!, {
      target: { value: "1.5" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Claim skill" }));
    await waitFor(() => {
      expect(claimSpy).toHaveBeenCalledWith(5, {
        proficiencyLevel: PROFICIENCY_LEVELS.Beginner,
        yearsOfExperience: 1.5,
      });
    });

    fireEvent.click(screen.getByRole("button", { name: "Remove" }));
    await waitFor(() => expect(removeSpy).toHaveBeenCalledWith(4));
  });
});
