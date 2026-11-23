import { useMutation, useQuery } from "@tanstack/react-query";
import { z } from "zod";
import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { projectsApi, skillsApi, PROFICIENCY_LEVELS, type ProficiencyLevel } from "@/api";
import type { SkillDto } from "@/api";
import { Button, ErrorBanner, Select } from "@/components/ui";
import { queryKeys } from "@/lib/queryKeys";

const createProjectSchema = z.object({
  title: z.string().trim().min(3, "Use at least 3 characters for the project title."),
  description: z.string().trim().min(10, "Describe your project in at least 10 characters."),
  maxTeamSize: z.number().int().min(1, "Team size must be at least one."),
});

interface RequiredSkillDraft {
  skillId: number;
  requiredLevel: ProficiencyLevel;
  isMandatory: boolean;
}

class ProjectSkillAttachError extends Error {
  readonly projectId: number;

  constructor(projectId: number, cause: unknown) {
    super(cause instanceof Error ? cause.message : "The project was created, but some skills could not be attached.");
    this.name = "ProjectSkillAttachError";
    this.projectId = projectId;
  }
}

export function ProjectCreatePage() {
  const navigate = useNavigate();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [maxTeamSize, setMaxTeamSize] = useState("4");
  const [skillId, setSkillId] = useState("");
  const [level, setLevel] = useState<ProficiencyLevel>(PROFICIENCY_LEVELS.Beginner);
  const [isMandatory, setIsMandatory] = useState(true);
  const [requiredSkills, setRequiredSkills] = useState<RequiredSkillDraft[]>([]);
  const [validationMessage, setValidationMessage] = useState("");
  const skillsQuery = useQuery({
    queryKey: queryKeys.skills.list(),
    queryFn: ({ signal }) => skillsApi.list({}, signal),
  });

  const createMutation = useMutation({
    mutationFn: async () => {
      const project = await projectsApi.create({
        title: title.trim(),
        description: description.trim(),
        maxTeamSize: Number(maxTeamSize),
      });
      try {
        await Promise.all(
          requiredSkills.map(({ skillId: requiredSkillId, ...payload }) =>
            projectsApi.setRequiredSkill(project.projectId, requiredSkillId, payload),
          ),
        );
      } catch (error) {
        throw new ProjectSkillAttachError(project.projectId, error);
      }
      return project;
    },
    onSuccess: (project) => {
      toast.success("Project created.");
      navigate(`/projects/${project.projectId}`);
    },
    onError: (error) => {
      if (error instanceof ProjectSkillAttachError) {
        toast.error(`Project created, but a required skill could not be added: ${error.message}`);
        navigate(`/projects/${error.projectId}`);
        return;
      }
      toast.error(error instanceof Error ? error.message : "Could not create project.");
    },
  });

  const addSkill = () => {
    const selected = Number(skillId);
    if (!Number.isInteger(selected) || requiredSkills.some((item) => item.skillId === selected)) return;
    setRequiredSkills((items) => [...items, { skillId: selected, requiredLevel: level, isMandatory }]);
    setSkillId("");
  };

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const parsed = createProjectSchema.safeParse({ title, description, maxTeamSize: Number(maxTeamSize) });
    if (!parsed.success) {
      setValidationMessage(parsed.error.issues[0]?.message ?? "Check the form fields.");
      return;
    }
    setValidationMessage("");
    createMutation.mutate();
  };

  const skillName = (id: number, options: SkillDto[]) => options.find((item) => item.skillId === id)?.name ?? `Skill #${id}`;

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <header>
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[#d3fb52]">Project workspace</p>
        <h1 className="mt-2 text-3xl font-bold text-white">Create a project</h1>
        <p className="mt-2 text-sm text-white/55">Describe your idea and define the teammates you need.</p>
      </header>
      <form className="space-y-6 rounded-3xl border border-white/10 bg-[#1b1d26] p-5 sm:p-7" onSubmit={submit}>
        <label className="block space-y-1.5 text-sm text-white/80">
          Project title
          <input required minLength={3} value={title} onChange={(event) => setTitle(event.target.value)} className="h-11 w-full rounded-xl border border-white/12 bg-[#14151c] px-3 text-white" />
        </label>
        <label className="block space-y-1.5 text-sm text-white/80">
          Description
          <textarea required minLength={10} rows={5} value={description} onChange={(event) => setDescription(event.target.value)} className="w-full rounded-xl border border-white/12 bg-[#14151c] p-3 text-white" />
        </label>
        <label className="block max-w-xs space-y-1.5 text-sm text-white/80">
          Maximum team size
          <input type="number" min="1" step="1" required value={maxTeamSize} onChange={(event) => setMaxTeamSize(event.target.value)} className="h-11 w-full rounded-xl border border-white/12 bg-[#14151c] px-3 text-white" />
        </label>

        <section className="space-y-3 border-t border-white/10 pt-5">
          <div>
            <h2 className="font-semibold text-white">Required skills</h2>
            <p className="mt-1 text-xs text-white/50">Add skills now or later from the project workspace.</p>
          </div>
          {skillsQuery.isError && <ErrorBanner error={skillsQuery.error} title="Could not load skills" />}
          <div className="grid gap-3 sm:grid-cols-[1.4fr_1fr_auto] sm:items-end">
            <Select
              label="Skill"
              value={skillId}
              placeholder={skillsQuery.isLoading ? "Loading skills…" : "Choose a skill"}
              options={(skillsQuery.data ?? [])
                .filter((skill) => !requiredSkills.some((item) => item.skillId === skill.skillId))
                .map((skill) => ({ value: skill.skillId, label: `${skill.name} · ${skill.category}` }))}
              onChange={(event) => setSkillId(event.target.value)}
            />
            <Select
              label="Required proficiency"
              value={level}
              options={Object.values(PROFICIENCY_LEVELS).map((item) => ({ value: item, label: item }))}
              onChange={(event) => setLevel(event.target.value as ProficiencyLevel)}
            />
            <Button type="button" variant="secondary" disabled={!skillId} onClick={addSkill}>Add skill</Button>
          </div>
          <label className="inline-flex items-center gap-2 text-sm text-white/70">
            <input type="checkbox" checked={isMandatory} onChange={(event) => setIsMandatory(event.target.checked)} />
            Mandatory skill
          </label>
          {requiredSkills.map((item) => (
            <div key={item.skillId} className="flex items-center justify-between gap-3 rounded-xl bg-[#14151c] px-3 py-2 text-sm">
              <span className="text-white">{skillName(item.skillId, skillsQuery.data ?? [])} · {item.requiredLevel} · {item.isMandatory ? "Mandatory" : "Preferred"}</span>
              <button type="button" className="text-xs text-red-300 hover:underline" onClick={() => setRequiredSkills((items) => items.filter((skill) => skill.skillId !== item.skillId))}>Remove</button>
            </div>
          ))}
        </section>

        {validationMessage && <p role="alert" className="text-sm text-red-300">{validationMessage}</p>}
        {createMutation.isError && !(createMutation.error instanceof ProjectSkillAttachError) && (
          <ErrorBanner error={createMutation.error} />
        )}
        <div className="flex flex-wrap justify-end gap-2">
          <Button type="button" variant="ghost" onClick={() => navigate("/projects")}>Cancel</Button>
          <Button type="submit" isLoading={createMutation.isPending}>Create project</Button>
        </div>
      </form>
    </div>
  );
}
