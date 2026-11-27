import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { PROFICIENCY_LEVELS } from "@/api";
import { skillsApi, type ProficiencyLevel, type StudentSkillDto } from "@/api";
import { Button, ErrorBanner, Select } from "@/components/ui";
import { queryKeys } from "@/lib/queryKeys";

export interface StudentSkillsPanelProps {
  studentId: number;
  editable?: boolean;
  skillsData?: StudentSkillDto[];
}

const PROFICIENCY_OPTIONS = Object.values(PROFICIENCY_LEVELS).map((level) => ({
  value: level,
  label: level,
}));

function SkillClaimCard({
  skill,
  editable,
  onSave,
  onRemove,
  isSaving,
}: {
  skill: StudentSkillDto;
  editable: boolean;
  onSave: (skillId: number, proficiencyLevel: ProficiencyLevel, yearsOfExperience: number) => void;
  onRemove: (skillId: number) => void;
  isSaving: boolean;
}) {
  const [level, setLevel] = useState<ProficiencyLevel>(skill.proficiencyLevel);
  const [years, setYears] = useState(String(skill.yearsOfExperience));

  return (
    <article className="rounded-2xl border border-white/10 bg-[#14151c] p-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="font-semibold text-white">{skill.name}</h3>
          <p className="mt-1 text-xs text-white/55">{skill.category}</p>
        </div>
        {!editable && (
          <span className="rounded-full bg-[#d3fb52]/10 px-2.5 py-1 text-xs text-[#d3fb52]">
            {skill.proficiencyLevel} · {skill.yearsOfExperience} yrs
          </span>
        )}
      </div>
      {editable && (
        <div className="mt-4 flex flex-wrap items-end gap-3">
          <div className="min-w-[140px] flex-1">
            <Select
              label="Proficiency"
              value={level}
              options={PROFICIENCY_OPTIONS}
              onChange={(event) => setLevel(event.target.value as ProficiencyLevel)}
            />
          </div>
          <label className="min-w-[120px] flex-1 space-y-1.5 text-[13px] font-medium text-white/85">
            <span>Years of experience</span>
            <input
              type="number"
              min="0"
              max="80"
              step="0.5"
              value={years}
              onChange={(event) => setYears(event.target.value)}
              className="h-10 w-full rounded-xl border border-white/12 bg-[#14151c] px-3 text-sm text-white"
            />
          </label>
          <div className="flex shrink-0 items-center gap-2">
            <Button
              size="sm"
              isLoading={isSaving}
              disabled={!Number.isFinite(Number(years)) || Number(years) < 0}
              onClick={() => onSave(skill.skillId, level, Number(years))}
            >
              Save
            </Button>
            <Button size="sm" variant="danger" disabled={isSaving} onClick={() => onRemove(skill.skillId)}>
              Remove
            </Button>
          </div>
        </div>
      )}
    </article>
  );
}

export function StudentSkillsPanel({
  studentId,
  editable = false,
  skillsData,
}: StudentSkillsPanelProps) {
  const queryClient = useQueryClient();
  const [selectedSkillId, setSelectedSkillId] = useState("");
  const [newLevel, setNewLevel] = useState<ProficiencyLevel>(PROFICIENCY_LEVELS.Beginner);
  const [newYears, setNewYears] = useState("0");
  const studentSkillsKey = queryKeys.students.skills(studentId);

  const skillsQuery = useQuery({
    queryKey: queryKeys.skills.list(),
    queryFn: ({ signal }) => skillsApi.list({}, signal),
    enabled: editable,
    staleTime: 5 * 60_000,
  });
  const claimsQuery = useQuery({
    queryKey: studentSkillsKey,
    queryFn: ({ signal }) => skillsApi.getStudentSkills(studentId, signal),
    enabled: studentId > 0 && skillsData === undefined,
  });
  const claimedSkills = skillsData ?? claimsQuery.data ?? [];

  const saveClaim = useMutation({
    mutationFn: ({
      skillId,
      proficiencyLevel,
      yearsOfExperience,
    }: {
      skillId: number;
      proficiencyLevel: ProficiencyLevel;
      yearsOfExperience: number;
    }) => skillsApi.claimMine(skillId, { proficiencyLevel, yearsOfExperience }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: studentSkillsKey }),
        queryClient.invalidateQueries({ queryKey: queryKeys.students.portfolio(studentId) }),
      ]);
      toast.success("Skill claim saved.");
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not save skill."),
  });

  const removeClaim = useMutation({
    mutationFn: (skillId: number) => skillsApi.removeMine(skillId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: studentSkillsKey }),
        queryClient.invalidateQueries({ queryKey: queryKeys.students.portfolio(studentId) }),
      ]);
      toast.success("Skill removed from your profile.");
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not remove skill."),
  });

  const claimedIds = new Set(claimedSkills.map((skill) => skill.skillId));
  const availableSkills = (skillsQuery.data ?? []).filter((skill) => !claimedIds.has(skill.skillId));

  const claimSelected = () => {
    const skillId = Number(selectedSkillId);
    if (!Number.isInteger(skillId) || !Number.isFinite(Number(newYears)) || Number(newYears) < 0) {
      return;
    }
    saveClaim.mutate({ skillId, proficiencyLevel: newLevel, yearsOfExperience: Number(newYears) });
  };

  return (
    <section className="@container space-y-4" aria-labelledby={`student-skills-${studentId}`}>
      <div>
        <h2 id={`student-skills-${studentId}`} className="text-xl font-bold text-white">
          {editable ? "My Claimed Skills" : "Skills"}
        </h2>
        <p className="mt-1 text-sm text-white/55">
          {editable ? "Show your current strengths and experience to project teams." : "Verified skills on this student profile."}
        </p>
      </div>

      {(skillsQuery.isError || claimsQuery.isError) && (
        <ErrorBanner error={skillsQuery.error ?? claimsQuery.error} />
      )}
      {(skillsQuery.isLoading || claimsQuery.isLoading) && (
        <p role="status" className="text-sm text-white/60">Loading skills…</p>
      )}

      {editable && availableSkills.length > 0 && (
        <div className="grid gap-3 rounded-2xl border border-white/10 bg-[#1b1d26] p-4 @2xl:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)_minmax(0,1fr)_auto] @2xl:items-end">
          <div className="min-w-0">
            <Select
              label="Add a skill"
              value={selectedSkillId}
              placeholder="Choose a skill"
              options={availableSkills.map(({ skillId, name, category }) => ({
                value: skillId,
                label: `${name} · ${category}`,
              }))}
              onChange={(event) => setSelectedSkillId(event.target.value)}
            />
          </div>
          <div className="min-w-0">
            <Select
              label="Proficiency"
              value={newLevel}
              options={PROFICIENCY_OPTIONS}
              onChange={(event) => setNewLevel(event.target.value as ProficiencyLevel)}
            />
          </div>
          <label className="min-w-0 space-y-1.5 text-[13px] font-medium text-white/85">
            <span>Years of experience</span>
            <input
              type="number"
              min="0"
              max="80"
              step="0.5"
              value={newYears}
              onChange={(event) => setNewYears(event.target.value)}
              className="h-10 w-full rounded-xl border border-white/12 bg-[#14151c] px-3 text-sm text-white"
            />
          </label>
          <Button
            onClick={claimSelected}
            isLoading={saveClaim.isPending}
            disabled={!selectedSkillId || !Number.isFinite(Number(newYears)) || Number(newYears) < 0}
          >
            Claim skill
          </Button>
        </div>
      )}

      <div className="grid gap-3 @2xl:grid-cols-2">
        {claimedSkills.map((skill) => (
          <SkillClaimCard
            key={skill.skillId}
            skill={skill}
            editable={editable}
            isSaving={saveClaim.isPending || removeClaim.isPending}
            onSave={(skillId, proficiencyLevel, yearsOfExperience) =>
              saveClaim.mutate({ skillId, proficiencyLevel, yearsOfExperience })
            }
            onRemove={(skillId) => removeClaim.mutate(skillId)}
          />
        ))}
      </div>
      {!(skillsData === undefined && claimsQuery.isLoading) &&
        !claimsQuery.isError &&
        claimedSkills.length === 0 && (
        <p className="rounded-xl border border-dashed border-white/15 p-5 text-sm text-white/55">
          {editable ? "You haven't claimed any skills yet." : "No skills have been added to this profile."}
        </p>
      )}
    </section>
  );
}
