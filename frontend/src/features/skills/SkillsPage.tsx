import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useState } from "react";
import { Pencil, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { skillsApi, type SkillDto } from "@/api";
import type { SkillPayload } from "@/api/skills.api";
import { Button, ErrorBanner, Modal, SearchInput } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { queryKeys } from "@/lib/queryKeys";
import { StudentSkillsPanel } from "./StudentSkillsPanel";

const EMPTY_SKILL: SkillPayload = { name: "", category: "", description: "" };

export function SkillsPage() {
  const { user, isPlatformAdmin } = useAuth();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [category, setCategory] = useState("");
  const [editing, setEditing] = useState<SkillDto | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState<SkillPayload>(EMPTY_SKILL);
  const [formError, setFormError] = useState("");
  useEffect(() => {
    const timer = window.setTimeout(() => setDebouncedSearch(search.trim()), 300);
    return () => window.clearTimeout(timer);
  }, [search]);

  const skillsQuery = useQuery({
    queryKey: queryKeys.skills.list({ search: debouncedSearch, category }),
    queryFn: ({ signal }) =>
      skillsApi.list(
        { search: debouncedSearch || undefined, category: category || undefined },
        signal,
      ),
  });
  const skills = skillsQuery.data ?? [];
  const categories = useMemo(
    () => Array.from(new Set(skills.map((skill) => skill.category))).sort(),
    [skills],
  );

  const refresh = async () => {
    await queryClient.invalidateQueries({ queryKey: queryKeys.skills.all });
  };

  const saveMutation = useMutation({
    mutationFn: () =>
      editing ? skillsApi.update(editing.skillId, form) : skillsApi.create(form),
    onSuccess: async () => {
      await refresh();
      toast.success(editing ? "Skill updated." : "Skill created.");
      setModalOpen(false);
    },
    onError: (error) => setFormError(error instanceof Error ? error.message : "Could not save skill."),
  });
  const deleteMutation = useMutation({
    mutationFn: (skillId: number) => skillsApi.remove(skillId),
    onSuccess: async () => {
      await refresh();
      toast.success("Skill deleted.");
    },
    onError: (error) => toast.error(error instanceof Error ? error.message : "Could not delete skill."),
  });

  const startCreate = () => {
    setEditing(null);
    setForm(EMPTY_SKILL);
    setFormError("");
    setModalOpen(true);
  };
  const startEdit = (skill: SkillDto) => {
    setEditing(skill);
    setForm({ name: skill.name, category: skill.category, description: skill.description ?? "" });
    setFormError("");
    setModalOpen(true);
  };
  const save = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!form.name.trim() || !form.category.trim()) {
      setFormError("Name and category are required.");
      return;
    }
    saveMutation.mutate();
  };

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-white/10 bg-[#1b1d26] p-6 sm:p-9">
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[#d3fb52]">TeamUp Skills Matrix</p>
        <div className="mt-3 flex flex-wrap items-end justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold text-white sm:text-4xl">Skills catalog</h1>
            <p className="mt-2 max-w-2xl text-sm text-white/60">
              Explore the shared catalog and keep your profile skills up to date.
            </p>
          </div>
          {isPlatformAdmin && <Button leftIcon={<Plus size={16} />} onClick={startCreate}>Add skill</Button>}
        </div>
      </header>

      {user && <StudentSkillsPanel studentId={user.studentId} editable />}

      <section className="space-y-5">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
          <SearchInput value={search} onChange={setSearch} placeholder="Search skill names…" containerClassName="w-full sm:max-w-sm" />
          <label className="space-y-1.5 text-[13px] font-medium text-white/80">
            Category
            <select
              value={category}
              onChange={(event) => setCategory(event.target.value)}
              className="h-10 min-w-48 rounded-xl border border-white/12 bg-[#14151c] px-3 text-sm text-white"
            >
              <option value="">All categories</option>
              {categories.map((item) => <option key={item} value={item}>{item}</option>)}
            </select>
          </label>
        </div>

        {skillsQuery.isError && <ErrorBanner error={skillsQuery.error} onRetry={() => void skillsQuery.refetch()} />}
        {skillsQuery.isLoading && <p role="status" className="text-sm text-white/60">Loading skill catalog…</p>}
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {skills.map((skill) => (
            <article key={skill.skillId} className="rounded-2xl border border-white/10 bg-[#1b1d26] p-5">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <span className="text-xs text-[#d3fb52]">{skill.category}</span>
                  <h2 className="mt-1 font-semibold text-white">{skill.name}</h2>
                  {skill.description && <p className="mt-2 text-sm text-white/55">{skill.description}</p>}
                </div>
                {isPlatformAdmin && (
                  <div className="flex gap-1">
                    <Button size="icon" variant="ghost" aria-label={`Edit ${skill.name}`} onClick={() => startEdit(skill)}>
                      <Pencil size={15} />
                    </Button>
                    <Button
                      size="icon"
                      variant="ghost"
                      aria-label={`Delete ${skill.name}`}
                      disabled={deleteMutation.isPending}
                      onClick={() => {
                        if (window.confirm(`Delete "${skill.name}" from the catalog?`)) {
                          deleteMutation.mutate(skill.skillId);
                        }
                      }}
                    >
                      <Trash2 size={15} />
                    </Button>
                  </div>
                )}
              </div>
            </article>
          ))}
        </div>
        {!skillsQuery.isLoading && !skillsQuery.isError && skills.length === 0 && (
          <p className="rounded-xl border border-dashed border-white/15 p-6 text-center text-sm text-white/55">No skills match these filters.</p>
        )}
      </section>

      <Modal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        title={editing ? "Edit skill" : "Create skill"}
      >
        <form className="space-y-4" onSubmit={save}>
          <label className="block space-y-1.5 text-sm text-white/80">
            Name
            <input required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} className="h-10 w-full rounded-xl border border-white/12 bg-[#14151c] px-3 text-white" />
          </label>
          <label className="block space-y-1.5 text-sm text-white/80">
            Category
            <input required value={form.category} onChange={(event) => setForm({ ...form, category: event.target.value })} className="h-10 w-full rounded-xl border border-white/12 bg-[#14151c] px-3 text-white" />
          </label>
          <label className="block space-y-1.5 text-sm text-white/80">
            Description
            <textarea value={form.description ?? ""} onChange={(event) => setForm({ ...form, description: event.target.value })} rows={3} className="w-full rounded-xl border border-white/12 bg-[#14151c] p-3 text-white" />
          </label>
          {formError && <p role="alert" className="text-sm text-red-300">{formError}</p>}
          <div className="flex justify-end gap-2">
            <Button type="button" variant="ghost" onClick={() => setModalOpen(false)}>Cancel</Button>
            <Button type="submit" isLoading={saveMutation.isPending}>{editing ? "Save changes" : "Create skill"}</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
