import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import {
  APPLICATION_STATUSES,
  ApiClientError,
  PROFICIENCY_LEVELS,
  PROJECT_STATUSES,
  PROJECT_TASK_STATUSES,
  TASK_PRIORITIES,
  projectsApi,
  skillsApi,
  tasksApi,
  type ApplicationStatus,
  type ProjectApplicationDto,
  type ProjectStatus,
  type ProjectTaskDto,
  type ProjectTaskStatus,
  type ProficiencyLevel,
  type TaskPriority,
} from "@/api";
import { Avatar, Button, ErrorBanner, Modal, Select, StatusPill } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { invalidateGamification, queryKeys } from "@/lib/queryKeys";

type WorkspaceTab = "overview" | "team" | "applications" | "board";

const TABS: readonly { value: WorkspaceTab; label: string }[] = [
  { value: "overview", label: "Overview & Skills" },
  { value: "team", label: "Team" },
  { value: "applications", label: "Applications" },
  { value: "board", label: "Sprint board" },
];

const TASK_STATUSES = Object.values(PROJECT_TASK_STATUSES);
const TASK_PRIORITIES_LIST = Object.values(TASK_PRIORITIES);
const PROJECT_STATUSES_LIST = Object.values(PROJECT_STATUSES);
const PROFICIENCY_LIST = Object.values(PROFICIENCY_LEVELS);

function isLeadRole(role: string | undefined): boolean {
  return Boolean(role && /lead/i.test(role));
}

function isApiForbidden(error: unknown): boolean {
  return error instanceof ApiClientError && error.isForbidden;
}

function dateInputValue(value: string | null): string {
  return value ? value.slice(0, 10) : "";
}

export function ProjectDetailPage() {
  const { id: idParam } = useParams();
  const projectId = Number(idParam);
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [tab, setTab] = useState<WorkspaceTab>("overview");
  const [editing, setEditing] = useState(false);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [maxTeamSize, setMaxTeamSize] = useState("");
  const [status, setStatus] = useState<ProjectStatus>(PROJECT_STATUSES.Recruiting);
  const [applicationMessage, setApplicationMessage] = useState("");
  const [applicationModalOpen, setApplicationModalOpen] = useState(false);
  const [requiredSkillId, setRequiredSkillId] = useState("");
  const [requiredLevel, setRequiredLevel] = useState<ProficiencyLevel>(PROFICIENCY_LEVELS.Beginner);
  const [requiredIsMandatory, setRequiredIsMandatory] = useState(true);
  const [taskModalOpen, setTaskModalOpen] = useState(false);
  const [editingTask, setEditingTask] = useState<ProjectTaskDto | null>(null);
  const [taskTitle, setTaskTitle] = useState("");
  const [taskDescription, setTaskDescription] = useState("");
  const [taskDueDate, setTaskDueDate] = useState("");
  const [taskPriority, setTaskPriority] = useState<TaskPriority>(TASK_PRIORITIES.Medium);

  const validProjectId = Number.isInteger(projectId) && projectId > 0;
  const projectKey = queryKeys.projects.detail(projectId);
  const projectQuery = useQuery({
    queryKey: projectKey,
    queryFn: ({ signal }) => projectsApi.getById(projectId, signal),
    enabled: validProjectId,
  });
  const project = projectQuery.data;
  const currentMember = project?.members.find((member) => member.studentId === user?.studentId);
  const isLead = Boolean(
    project && user && (project.creatorId === user.studentId || isLeadRole(currentMember?.role)),
  );
  const isMember = Boolean(currentMember);

  const requiredSkillsQuery = useQuery({
    queryKey: queryKeys.projects.requiredSkills(projectId),
    queryFn: ({ signal }) => projectsApi.getRequiredSkills(projectId, signal),
    enabled: validProjectId,
  });
  const membersQuery = useQuery({
    queryKey: queryKeys.projects.members(projectId),
    queryFn: ({ signal }) => projectsApi.getMembers(projectId, signal),
    enabled: validProjectId,
  });
  const applicationsQuery = useQuery({
    queryKey: queryKeys.projects.applications(projectId),
    queryFn: ({ signal }) => projectsApi.getApplications(projectId, signal),
    enabled: validProjectId,
  });
  const tasksQuery = useQuery({
    queryKey: queryKeys.projects.tasks(projectId),
    queryFn: ({ signal }) => tasksApi.listForProject(projectId, signal),
    enabled: validProjectId && isMember,
  });
  const skillsQuery = useQuery({
    queryKey: queryKeys.skills.list(),
    queryFn: ({ signal }) => skillsApi.list({}, signal),
    enabled: isLead,
  });
  const studentSkillsQuery = useQuery({
    queryKey: queryKeys.students.skills(user?.studentId ?? 0),
    queryFn: ({ signal }) => skillsApi.getStudentSkills(user!.studentId, signal),
    enabled: Boolean(user?.studentId),
  });

  useEffect(() => {
    if (!project) return;
    setTitle(project.title);
    setDescription(project.description);
    setMaxTeamSize(String(project.maxTeamSize));
    setStatus(project.status);
  }, [project]);

  const invalidateProjectData = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: projectKey }),
      queryClient.invalidateQueries({ queryKey: queryKeys.projects.all }),
      queryClient.invalidateQueries({ queryKey: queryKeys.projects.requiredSkills(projectId) }),
      queryClient.invalidateQueries({ queryKey: queryKeys.projects.members(projectId) }),
      queryClient.invalidateQueries({ queryKey: queryKeys.projects.applications(projectId) }),
      queryClient.invalidateQueries({ queryKey: queryKeys.projects.tasks(projectId) }),
    ]);
  };

  const updateProject = useMutation({
    mutationFn: () =>
      projectsApi.update(projectId, {
        title: title.trim(),
        description: description.trim(),
        maxTeamSize: Number(maxTeamSize),
        status,
      }),
    onSuccess: async (updated) => {
      await invalidateProjectData();
      setEditing(false);
      if (project?.status !== PROJECT_STATUSES.Completed && updated.status === PROJECT_STATUSES.Completed) {
        await invalidateGamification(queryClient, user?.studentId);
        toast.success("+50 XP awarded!");
      } else {
        toast.success("Project updated.");
      }
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not update project."),
  });

  const deleteProject = useMutation({
    mutationFn: () => projectsApi.remove(projectId),
    onSuccess: () => {
      toast.success("Project deleted.");
      navigate("/projects");
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not delete project."),
  });

  const setRequiredSkill = useMutation({
    mutationFn: ({
      skillId,
      payload,
    }: {
      skillId: number;
      payload: { requiredLevel: ProficiencyLevel; isMandatory: boolean };
    }) => projectsApi.setRequiredSkill(projectId, skillId, payload),
    onSuccess: async () => {
      setRequiredSkillId("");
      await invalidateProjectData();
      toast.success("Required skill updated.");
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not update required skill."),
  });
  const removeRequiredSkill = useMutation({
    mutationFn: (skillId: number) => projectsApi.removeRequiredSkill(projectId, skillId),
    onSuccess: invalidateProjectData,
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not remove required skill."),
  });

  const updateMemberRole = useMutation({
    mutationFn: ({ studentId, role }: { studentId: number; role: string }) =>
      projectsApi.updateMemberRole(projectId, studentId, role),
    onSuccess: invalidateProjectData,
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not update member role."),
  });
  const removeMember = useMutation({
    mutationFn: (studentId: number) => projectsApi.removeMember(projectId, studentId),
    onSuccess: async () => {
      await invalidateProjectData();
      toast.success("Member removed. Their task assignments were cleared.");
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not remove member."),
  });

  const apply = useMutation({
    mutationFn: () => projectsApi.apply(projectId, applicationMessage.trim()),
    onSuccess: async () => {
      setApplicationMessage("");
      await queryClient.invalidateQueries({ queryKey: queryKeys.projects.applications(projectId) });
      toast.success("Your application was sent.");
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not apply to this project."),
  });
  const updateApplication = useMutation({
    mutationFn: ({ appId, nextStatus }: { appId: number; nextStatus: Extract<ApplicationStatus, "Accepted" | "Rejected"> }) =>
      projectsApi.updateApplication(projectId, appId, nextStatus),
    onSuccess: invalidateProjectData,
    onError: (error) => {
      if (error instanceof ApiClientError && error.isConflict) {
        toast.error("The team is full. Refresh the project before accepting another applicant.");
        void invalidateProjectData();
        return;
      }
      toast.error(error instanceof Error ? error.message : "Could not update application.");
    },
  });
  const withdraw = useMutation({
    mutationFn: (appId: number) => projectsApi.withdrawApplication(projectId, appId),
    onSuccess: invalidateProjectData,
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not withdraw application."),
  });

  const invalidateTasks = async () => {
    await queryClient.invalidateQueries({ queryKey: queryKeys.projects.tasks(projectId) });
  };
  const saveTask = useMutation({
    mutationFn: async () => {
      const payload = {
        title: taskTitle.trim(),
        description: taskDescription.trim() || null,
        dueDate: taskDueDate ? new Date(`${taskDueDate}T23:59:59`).toISOString() : null,
        priority: taskPriority,
      };
      if (editingTask) {
        return tasksApi.update(editingTask.taskId, { ...payload, status: editingTask.status });
      }
      return tasksApi.createForProject(projectId, payload);
    },
    onSuccess: async () => {
      setTaskModalOpen(false);
      await invalidateTasks();
      toast.success(editingTask ? "Task updated." : "Task created.");
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not save task."),
  });
  const updateTask = useMutation({
    mutationFn: ({ task, nextStatus }: { task: ProjectTaskDto; nextStatus: ProjectTaskStatus }) =>
      tasksApi.update(task.taskId, {
        title: task.title,
        description: task.description,
        dueDate: task.dueDate,
        priority: task.priority,
        status: nextStatus,
      }),
    onSuccess: async (updated, variables) => {
      await invalidateTasks();
      if (variables.task.status !== PROJECT_TASK_STATUSES.Done && updated.status === PROJECT_TASK_STATUSES.Done) {
        await invalidateGamification(queryClient, user?.studentId);
        toast.success("+50 XP awarded!");
      }
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not move task."),
  });
  const deleteTask = useMutation({
    mutationFn: (taskId: number) => tasksApi.remove(taskId),
    onSuccess: async () => {
      await invalidateTasks();
      toast.success("Task deleted.");
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not delete task."),
  });
  const assignTask = useMutation({
    mutationFn: ({ taskId, studentId }: { taskId: number; studentId: number }) =>
      tasksApi.assign(taskId, studentId),
    onSuccess: invalidateTasks,
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not assign task."),
  });
  const unassignTask = useMutation({
    mutationFn: (taskId: number) => tasksApi.unassign(taskId),
    onSuccess: invalidateTasks,
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not unassign task."),
  });

  const tasksByStatus = useMemo(() => {
    const groups = new Map<ProjectTaskStatus, ProjectTaskDto[]>(
      TASK_STATUSES.map((item) => [item, []]),
    );
    for (const task of tasksQuery.data ?? []) {
      groups.get(task.status)?.push(task);
    }
    return groups;
  }, [tasksQuery.data]);
  const myApplication = (applicationsQuery.data ?? []).find(
    (application) => application.studentId === user?.studentId,
  );
  const availableSkills = (skillsQuery.data ?? []).filter(
    (skill) => !(requiredSkillsQuery.data ?? []).some((required) => required.skillId === skill.skillId),
  );
  const claimedSkillIds = new Set((studentSkillsQuery.data ?? []).map((skill) => skill.skillId));

  const openNewTask = () => {
    setEditingTask(null);
    setTaskTitle("");
    setTaskDescription("");
    setTaskDueDate("");
    setTaskPriority(TASK_PRIORITIES.Medium);
    setTaskModalOpen(true);
  };
  const openEditTask = (task: ProjectTaskDto) => {
    setEditingTask(task);
    setTaskTitle(task.title);
    setTaskDescription(task.description ?? "");
    setTaskDueDate(dateInputValue(task.dueDate));
    setTaskPriority(task.priority);
    setTaskModalOpen(true);
  };
  const submitTask = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!taskTitle.trim()) return;
    saveTask.mutate();
  };

  if (!validProjectId) {
    return <p role="alert" className="text-sm text-red-300">Invalid project ID.</p>;
  }
  if (projectQuery.isLoading) {
    return <p role="status" className="py-12 text-center text-sm text-white/60">Loading project…</p>;
  }
  if (projectQuery.isError || !project) {
    return <ErrorBanner error={projectQuery.error} onRetry={() => void projectQuery.refetch()} />;
  }

  const applicationsError = applicationsQuery.isError && !isApiForbidden(applicationsQuery.error);
  const applicationForbidden = applicationsQuery.isError && isApiForbidden(applicationsQuery.error);

  return (
    <div className="space-y-6">
      <header className="rounded-3xl border border-white/10 bg-[#1b1d26] p-5 sm:p-8">
        <Link to="/projects" className="text-xs font-medium text-white/50 hover:text-[#d3fb52]">← All projects</Link>
        <div className="mt-4 flex flex-wrap items-start justify-between gap-4">
          <div className="max-w-3xl">
            <div className="flex flex-wrap items-center gap-2">
              <StatusPill status={project.status} />
              <span className="text-xs text-white/45">Created by {project.creatorName}</span>
            </div>
            <h1 className="mt-3 text-3xl font-bold text-white">{project.title}</h1>
            <p className="mt-3 whitespace-pre-wrap text-sm leading-relaxed text-white/60">{project.description}</p>
            <p className="mt-4 text-xs text-white/45">{project.teamSize} of {project.maxTeamSize} team members</p>
          </div>
          {isLead && (
            <div className="flex flex-wrap gap-2">
              <Button variant="outline" onClick={() => setEditing((current) => !current)}>{editing ? "Cancel edit" : "Edit project"}</Button>
              <Button variant="danger" onClick={() => {
                if (window.confirm("Delete this project? This cannot be undone.")) deleteProject.mutate();
              }} isLoading={deleteProject.isPending}>Delete</Button>
            </div>
          )}
        </div>
        {editing && isLead && (
          <form
            className="mt-6 grid gap-3 border-t border-white/10 pt-5 md:grid-cols-2"
            onSubmit={(event) => {
              event.preventDefault();
              if (!title.trim() || !description.trim() || !Number.isInteger(Number(maxTeamSize)) || Number(maxTeamSize) < 1) {
                toast.error("Enter a title, description, and team size of at least one.");
                return;
              }
              updateProject.mutate();
            }}
          >
            <label className="space-y-1 text-xs text-white/60">Title<input value={title} onChange={(event) => setTitle(event.target.value)} className="h-10 w-full rounded-lg border border-white/10 bg-[#14151c] px-3 text-sm text-white" /></label>
            <label className="space-y-1 text-xs text-white/60">Maximum team size<input type="number" min="1" value={maxTeamSize} onChange={(event) => setMaxTeamSize(event.target.value)} className="h-10 w-full rounded-lg border border-white/10 bg-[#14151c] px-3 text-sm text-white" /></label>
            <label className="space-y-1 text-xs text-white/60 md:col-span-2">Description<textarea rows={3} value={description} onChange={(event) => setDescription(event.target.value)} className="w-full rounded-lg border border-white/10 bg-[#14151c] p-3 text-sm text-white" /></label>
            <Select
              label="Project status"
              value={status}
              options={PROJECT_STATUSES_LIST.map((item) => ({ value: item, label: item }))}
              onChange={(event) => setStatus(event.target.value as ProjectStatus)}
            />
            <div className="flex items-end justify-end"><Button type="submit" isLoading={updateProject.isPending}>Save project</Button></div>
          </form>
        )}
      </header>

      <nav className="flex gap-2 overflow-x-auto border-b border-white/10" aria-label="Project sections">
        {TABS.map((item) => (
          <button
            key={item.value}
            type="button"
            aria-current={tab === item.value ? "page" : undefined}
            onClick={() => setTab(item.value)}
            className={`shrink-0 border-b-2 px-4 py-3 text-sm font-medium ${tab === item.value ? "border-[#d3fb52] text-[#d3fb52]" : "border-transparent text-white/55 hover:text-white"}`}
          >
            {item.label}
          </button>
        ))}
      </nav>

      {tab === "overview" && (
        <section className="grid gap-5 lg:grid-cols-[1.4fr_1fr]">
          <div className="rounded-2xl border border-white/10 bg-[#1b1d26] p-5">
            <h2 className="text-lg font-semibold text-white">Required skills</h2>
            {requiredSkillsQuery.isError && <ErrorBanner className="mt-4" error={requiredSkillsQuery.error} />}
            {studentSkillsQuery.isError && <ErrorBanner className="mt-3" error={studentSkillsQuery.error} title="Could not compare your claimed skills" />}
            <div className="mt-4 flex flex-wrap gap-2">
              {(requiredSkillsQuery.data ?? project.requiredSkills).map((skill) => (
                <div key={skill.skillId} className="flex flex-wrap items-center gap-2 rounded-xl bg-[#14151c] px-3 py-2 text-xs text-white/80">
                  <span>{skill.name}</span>
                  {user && (
                    <span className={claimedSkillIds.has(skill.skillId) ? "rounded-full bg-[#d3fb52]/10 px-2 py-0.5 text-[#d3fb52]" : "rounded-full bg-white/5 px-2 py-0.5 text-white/40"}>
                      {claimedSkillIds.has(skill.skillId) ? "You have this skill" : "Not claimed"}
                    </span>
                  )}
                  {isLead ? (
                    <>
                      <select
                        aria-label={`Required level for ${skill.name}`}
                        value={skill.requiredLevel}
                        onChange={(event) => setRequiredSkill.mutate({
                          skillId: skill.skillId,
                          payload: { requiredLevel: event.target.value as ProficiencyLevel, isMandatory: skill.isMandatory },
                        })}
                        className="rounded-lg border border-white/10 bg-[#1b1d26] px-2 py-1 text-xs text-white"
                      >
                        {PROFICIENCY_LIST.map((item) => <option key={item} value={item}>{item}</option>)}
                      </select>
                      <label className="inline-flex items-center gap-1 text-white/55">
                        <input
                          type="checkbox"
                          checked={skill.isMandatory}
                          onChange={(event) => setRequiredSkill.mutate({
                            skillId: skill.skillId,
                            payload: { requiredLevel: skill.requiredLevel, isMandatory: event.target.checked },
                          })}
                        />
                        Mandatory
                      </label>
                      <button type="button" aria-label={`Remove ${skill.name}`} className="text-red-300" onClick={() => removeRequiredSkill.mutate(skill.skillId)}>×</button>
                    </>
                  ) : (
                    <span className={skill.isMandatory ? "text-[#d3fb52]" : "text-white/45"}>
                      {skill.requiredLevel} · {skill.isMandatory ? "Mandatory" : "Preferred"}
                    </span>
                  )}
                </div>
              ))}
              {(requiredSkillsQuery.data ?? project.requiredSkills).length === 0 && <p className="text-sm text-white/50">No skills added yet.</p>}
            </div>
            {isLead && (
              <div className="mt-5 grid gap-3 border-t border-white/10 pt-4 sm:grid-cols-2">
                {skillsQuery.isError && <ErrorBanner className="sm:col-span-2" error={skillsQuery.error} title="Could not load skill catalog" />}
                <Select
                  label="Add a required skill"
                  value={requiredSkillId}
                  placeholder="Choose skill"
                  options={availableSkills.map((skill) => ({ value: skill.skillId, label: skill.name }))}
                  onChange={(event) => setRequiredSkillId(event.target.value)}
                />
                <Select label="Required proficiency" value={requiredLevel} options={PROFICIENCY_LIST.map((item) => ({ value: item, label: item }))} onChange={(event) => setRequiredLevel(event.target.value as ProficiencyLevel)} />
                <label className="inline-flex items-center gap-2 text-sm text-white/70"><input type="checkbox" checked={requiredIsMandatory} onChange={(event) => setRequiredIsMandatory(event.target.checked)} /> Mandatory</label>
                <div className="flex items-center justify-end"><Button size="sm" disabled={!requiredSkillId} isLoading={setRequiredSkill.isPending} onClick={() => setRequiredSkill.mutate({
                  skillId: Number(requiredSkillId),
                  payload: { requiredLevel, isMandatory: requiredIsMandatory },
                })}>Add skill</Button></div>
              </div>
            )}
          </div>
          <div className="rounded-2xl border border-white/10 bg-[#1b1d26] p-5">
            <h2 className="text-lg font-semibold text-white">Join the project</h2>
            {isMember ? (
              <p className="mt-3 text-sm text-white/60">You are on this team as <strong className="text-white">{currentMember?.role ?? "Member"}</strong>.</p>
            ) : project.status !== PROJECT_STATUSES.Recruiting ? (
              <p className="mt-3 text-sm text-white/55">This project is not currently recruiting.</p>
            ) : myApplication?.status === APPLICATION_STATUSES.Pending ? (
              <div className="mt-3 space-y-3">
                <p className="text-sm text-white/60">Your application is pending review.</p>
                <Button size="sm" variant="outline" isLoading={withdraw.isPending} onClick={() => withdraw.mutate(myApplication.applicationId)}>Withdraw application</Button>
              </div>
            ) : myApplication?.status === APPLICATION_STATUSES.Accepted ? (
              <p className="mt-3 text-sm text-[#d3fb52]">Your application was accepted.</p>
            ) : (
              <div className="mt-3 space-y-3">
                {applicationsQuery.isError && !applicationForbidden && <ErrorBanner error={applicationsQuery.error} />}
                <Button onClick={() => setApplicationModalOpen(true)} disabled={project.teamSize >= project.maxTeamSize}>Apply to join</Button>
                {project.teamSize >= project.maxTeamSize && <p className="text-xs text-amber-200">This team has reached its capacity.</p>}
              </div>
            )}
          </div>
        </section>
      )}

      {tab === "team" && (
        <section className="space-y-4">
          <h2 className="text-xl font-semibold text-white">Team roster</h2>
          {membersQuery.isError && <ErrorBanner error={membersQuery.error} onRetry={() => void membersQuery.refetch()} />}
          {(membersQuery.data ?? project.members).map((member) => (
            <article key={member.studentId} className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-white/10 bg-[#1b1d26] p-4">
              <Link to={`/students/${member.studentId}`} className="flex items-center gap-3 hover:opacity-80">
                <Avatar name={`${member.firstName} ${member.lastName}`} size="md" />
                <span><strong className="block text-sm text-white">{member.firstName} {member.lastName}</strong><span className="text-xs text-white/50">{member.role}</span></span>
              </Link>
              <div className="flex items-center gap-2">
                {isLead && member.studentId !== project.creatorId && (
                  <>
                    <select
                      aria-label={`Role for ${member.firstName} ${member.lastName}`}
                      value={member.role}
                      onChange={(event) => updateMemberRole.mutate({ studentId: member.studentId, role: event.target.value })}
                      className="h-9 rounded-lg border border-white/10 bg-[#14151c] px-2 text-sm text-white"
                    >
                      {[member.role, "Member", "Lead"].filter((item, index, list) => list.indexOf(item) === index).map((role) => <option key={role} value={role}>{role}</option>)}
                    </select>
                    <Button size="sm" variant="danger" onClick={() => {
                      if (window.confirm(`Remove ${member.firstName} ${member.lastName}? Their task assignments will be cleared.`)) {
                        removeMember.mutate(member.studentId);
                      }
                    }}>Remove</Button>
                  </>
                )}
                {!isLead && member.studentId === user?.studentId && (
                  <Button size="sm" variant="outline" onClick={() => {
                    if (window.confirm("Leave this project? Your task assignments will be cleared.")) removeMember.mutate(member.studentId);
                  }}>Leave team</Button>
                )}
              </div>
            </article>
          ))}
          {(membersQuery.data ?? project.members).length === 0 && <p className="text-sm text-white/55">No team members yet.</p>}
        </section>
      )}

      {tab === "applications" && (
        <section className="space-y-4">
          <div>
            <h2 className="text-xl font-semibold text-white">{isLead ? "Applicant queue" : "Your application"}</h2>
            <p className="mt-1 text-sm text-white/50">{isLead ? "Review applicants and manage team capacity." : "Track your request to join this team."}</p>
          </div>
          {applicationsError && <ErrorBanner error={applicationsQuery.error} onRetry={() => void applicationsQuery.refetch()} />}
          {applicationForbidden && <p className="rounded-xl border border-white/10 p-4 text-sm text-white/55">Apply to this project to access your application status.</p>}
          {(applicationsQuery.data ?? [])
            .filter((application) => isLead || application.studentId === user?.studentId)
            .map((application) => (
              <ApplicationRow
                key={application.applicationId}
                application={application}
                isLead={isLead}
                isBusy={updateApplication.isPending || withdraw.isPending}
                onStatus={(nextStatus) => updateApplication.mutate({ appId: application.applicationId, nextStatus })}
                onWithdraw={() => withdraw.mutate(application.applicationId)}
              />
            ))}
          {!applicationsQuery.isLoading && !applicationsError && !applicationForbidden && (applicationsQuery.data ?? []).filter((application) => isLead || application.studentId === user?.studentId).length === 0 && (
            <p className="rounded-xl border border-dashed border-white/15 p-5 text-sm text-white/55">{isLead ? "No applications yet." : "You have not applied to this project."}</p>
          )}
        </section>
      )}

      {tab === "board" && (
        <section className="space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div><h2 className="text-xl font-semibold text-white">Sprint board</h2><p className="mt-1 text-sm text-white/50">Move tasks through the sprint with the status menu.</p></div>
            {isMember && <Button onClick={openNewTask}>Create task</Button>}
          </div>
          {!isMember && <p className="rounded-xl border border-white/10 p-5 text-sm text-white/55">Only project members can view the sprint board.</p>}
          {tasksQuery.isError && isMember && <ErrorBanner error={tasksQuery.error} onRetry={() => void tasksQuery.refetch()} />}
          {isMember && (
            <div className="grid gap-4 xl:grid-cols-4">
              {TASK_STATUSES.map((taskStatus) => (
                <div key={taskStatus} className="min-w-0 rounded-2xl border border-white/10 bg-[#1b1d26] p-3">
                  <div className="mb-3 flex items-center justify-between px-1">
                    <h3 className="text-sm font-semibold text-white">{taskStatus}</h3>
                    <span className="rounded-full bg-white/10 px-2 py-0.5 text-xs text-white/60">{tasksByStatus.get(taskStatus)?.length ?? 0}</span>
                  </div>
                  <div className="space-y-3">
                    {(tasksByStatus.get(taskStatus) ?? []).map((task) => {
                      const overdue = task.dueDate && new Date(task.dueDate).getTime() < Date.now() && task.status !== PROJECT_TASK_STATUSES.Done;
                      return (
                        <article key={task.taskId} className="rounded-xl border border-white/10 bg-[#14151c] p-3">
                          <div className="flex items-start justify-between gap-2">
                            <h4 className="text-sm font-semibold text-white">{task.title}</h4>
                            <StatusPill status={task.priority} showDot={false} className="px-2 py-0.5 text-[10px]" />
                          </div>
                          {task.description && <p className="mt-2 text-xs text-white/50">{task.description}</p>}
                          {task.dueDate && <p className={`mt-2 text-xs ${overdue ? "font-semibold text-red-300" : "text-white/45"}`}>{overdue ? "Overdue · " : "Due · "}{new Date(task.dueDate).toLocaleDateString()}</p>}
                          <div className="mt-2 flex items-center gap-2 text-xs text-white/50">
                            {task.assignedStudentName && <Avatar name={task.assignedStudentName} size="xs" />}
                            <span>Assignee: {task.assignedStudentName ?? "Unassigned"}</span>
                          </div>
                          <div className="mt-3 space-y-2">
                            <Select
                              label={`Move ${task.title}`}
                              value={task.status}
                              options={TASK_STATUSES.map((item) => ({ value: item, label: item }))}
                              onChange={(event) => updateTask.mutate({ task, nextStatus: event.target.value as ProjectTaskStatus })}
                            />
                            <Select
                              aria-label={`Assignee for ${task.title}`}
                              label="Assign to member"
                              value={task.assignedStudentId ?? ""}
                              placeholder="Unassigned"
                              options={(membersQuery.data ?? project.members).map((member) => ({ value: member.studentId, label: `${member.firstName} ${member.lastName}` }))}
                              onChange={(event) => {
                                const nextId = Number(event.target.value);
                                if (event.target.value && (membersQuery.data ?? project.members).some((member) => member.studentId === nextId)) {
                                  assignTask.mutate({ taskId: task.taskId, studentId: nextId });
                                } else {
                                  unassignTask.mutate(task.taskId);
                                }
                              }}
                            />
                            <div className="flex justify-between gap-2">
                              <Button size="sm" variant="ghost" onClick={() => openEditTask(task)}>Edit</Button>
                              <Button size="sm" variant="ghost" className="text-red-300" onClick={() => {
                                if (window.confirm(`Delete "${task.title}"?`)) deleteTask.mutate(task.taskId);
                              }}>Delete</Button>
                            </div>
                          </div>
                        </article>
                      );
                    })}
                    {(tasksByStatus.get(taskStatus)?.length ?? 0) === 0 && <p className="rounded-lg border border-dashed border-white/10 p-3 text-center text-xs text-white/35">No tasks</p>}
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      )}

      <Modal open={taskModalOpen} onClose={() => setTaskModalOpen(false)} title={editingTask ? "Edit sprint task" : "Create sprint task"}>
        <form className="space-y-4" onSubmit={submitTask}>
          <label className="block space-y-1.5 text-sm text-white/75">Title<input required value={taskTitle} onChange={(event) => setTaskTitle(event.target.value)} className="h-10 w-full rounded-xl border border-white/10 bg-[#14151c] px-3 text-white" /></label>
          <label className="block space-y-1.5 text-sm text-white/75">Description<textarea rows={3} value={taskDescription} onChange={(event) => setTaskDescription(event.target.value)} className="w-full rounded-xl border border-white/10 bg-[#14151c] p-3 text-white" /></label>
          <label className="block space-y-1.5 text-sm text-white/75">Due date<input type="date" value={taskDueDate} onChange={(event) => setTaskDueDate(event.target.value)} className="h-10 w-full rounded-xl border border-white/10 bg-[#14151c] px-3 text-white" /></label>
          <Select label="Priority" value={taskPriority} options={TASK_PRIORITIES_LIST.map((item) => ({ value: item, label: item }))} onChange={(event) => setTaskPriority(event.target.value as TaskPriority)} />
          <div className="flex justify-end gap-2"><Button type="button" variant="ghost" onClick={() => setTaskModalOpen(false)}>Cancel</Button><Button type="submit" isLoading={saveTask.isPending}>{editingTask ? "Save task" : "Create task"}</Button></div>
        </form>
      </Modal>

      <Modal open={applicationModalOpen} onClose={() => setApplicationModalOpen(false)} title="Apply to join">
        <form className="space-y-4" onSubmit={(event) => {
          event.preventDefault();
          apply.mutate(undefined, { onSuccess: () => setApplicationModalOpen(false) });
        }}>
          <label className="block space-y-1.5 text-sm text-white/70">
            Tell the team why you want to join
            <textarea rows={4} value={applicationMessage} onChange={(event) => setApplicationMessage(event.target.value)} className="w-full rounded-xl border border-white/10 bg-[#14151c] p-3 text-white" />
          </label>
          <div className="flex justify-end gap-2">
            <Button type="button" variant="ghost" onClick={() => setApplicationModalOpen(false)}>Cancel</Button>
            <Button type="submit" isLoading={apply.isPending}>Send application</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}

function ApplicationRow({
  application,
  isLead,
  isBusy,
  onStatus,
  onWithdraw,
}: {
  application: ProjectApplicationDto;
  isLead: boolean;
  isBusy: boolean;
  onStatus: (status: Extract<ApplicationStatus, "Accepted" | "Rejected">) => void;
  onWithdraw: () => void;
}) {
  return (
    <article className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-white/10 bg-[#1b1d26] p-4">
      <div>
        <h3 className="font-semibold text-white">{application.studentName}</h3>
        {application.message && <p className="mt-1 max-w-2xl text-sm text-white/55">{application.message}</p>}
        <p className="mt-1 text-xs text-white/40">{new Date(application.appliedAt).toLocaleDateString()} · {application.status}</p>
      </div>
      <div className="flex gap-2">
        {isLead && application.status === APPLICATION_STATUSES.Pending && (
          <>
            <Button size="sm" isLoading={isBusy} onClick={() => onStatus(APPLICATION_STATUSES.Accepted)}>Accept</Button>
            <Button size="sm" variant="outline" disabled={isBusy} onClick={() => onStatus(APPLICATION_STATUSES.Rejected)}>Reject</Button>
          </>
        )}
        {!isLead && application.status === APPLICATION_STATUSES.Pending && (
          <Button size="sm" variant="outline" isLoading={isBusy} onClick={onWithdraw}>Withdraw</Button>
        )}
      </div>
    </article>
  );
}
