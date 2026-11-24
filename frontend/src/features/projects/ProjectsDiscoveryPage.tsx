import { useQuery } from "@tanstack/react-query";
import { useMemo } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { ArrowRight, Plus, Users } from "lucide-react";
import { projectsApi, skillsApi, type ProjectStatus } from "@/api";
import { PROJECT_STATUSES } from "@/api";
import { Button, ErrorBanner, Pagination, SearchInput, SpotlightCard, StatusPill } from "@/components/ui";
import { queryKeys } from "@/lib/queryKeys";

const PROJECT_STATUS_OPTIONS: readonly (ProjectStatus | "")[] = [
  "",
  PROJECT_STATUSES.Recruiting,
  PROJECT_STATUSES.InProgress,
  PROJECT_STATUSES.Completed,
  PROJECT_STATUSES.Archived,
];

export function ProjectsDiscoveryPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const keyword = searchParams.get("keyword") ?? "";
  const status = (searchParams.get("status") ?? "") as ProjectStatus | "";
  const skillIdParam = searchParams.get("skillId") ?? "";
  const skillId = /^\d+$/.test(skillIdParam) ? Number(skillIdParam) : undefined;
  const pageParam = searchParams.get("page") ?? "1";
  const page = /^\d+$/.test(pageParam) ? Math.max(1, Number(pageParam)) : 1;
  const pageSize = 12;

  const updateParams = (changes: Record<string, string>) => {
    const next = new URLSearchParams(searchParams);
    for (const [key, value] of Object.entries(changes)) {
      if (value) next.set(key, value);
      else next.delete(key);
    }
    setSearchParams(next, { replace: true });
  };

  const projectsQuery = useQuery({
    queryKey: queryKeys.projects.list({ keyword, status, skillId, page, pageSize }),
    queryFn: ({ signal }) =>
      projectsApi.list(
        { keyword: keyword || undefined, status: status || undefined, skillId, page, pageSize },
        signal,
      ),
  });
  const skillsQuery = useQuery({
    queryKey: queryKeys.skills.list(),
    queryFn: ({ signal }) => skillsApi.list({}, signal),
  });

  const items = projectsQuery.data?.items ?? [];
  const totalPages = Math.max(1, projectsQuery.data?.totalPages ?? 1);
  const safePage = Math.min(page, totalPages);
  const selectedSkill = useMemo(
    () => skillsQuery.data?.find((skill) => skill.skillId === skillId),
    [skillsQuery.data, skillId],
  );

  return (
    <div className="space-y-7">
      <header className="rounded-3xl border border-white/10 bg-[#1b1d26] p-6 sm:p-9">
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[#d3fb52]">TeamUp Project Hub</p>
        <div className="mt-3 flex flex-wrap items-end justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold text-white sm:text-4xl">Find your next team</h1>
            <p className="mt-2 max-w-2xl text-sm text-white/60">
              Discover student-led projects, find a role that fits your skills, and build something together.
            </p>
          </div>
          <Link to="/projects/new">
            <Button leftIcon={<Plus size={16} />}>Create a project</Button>
          </Link>
        </div>
      </header>

      <section className="space-y-5">
        <div className="grid gap-3 rounded-2xl border border-white/10 bg-[#1b1d26] p-4 md:grid-cols-[minmax(0,1fr)_12rem_16rem] md:items-end">
          <SearchInput
            value={keyword}
            onChange={(value) => updateParams({ keyword: value, page: "" })}
            placeholder="Search project ideas…"
          />
          <label className="min-w-0 space-y-1.5 text-[13px] font-medium text-white/80">
            Status
            <select
              value={status}
              onChange={(event) => updateParams({ status: event.target.value, page: "" })}
              className="min-h-[42px] w-full rounded-xl border border-white/12 bg-[#14151c] px-3 py-2 text-base text-white sm:min-h-[40px] sm:text-sm"
            >
              {PROJECT_STATUS_OPTIONS.map((item) => (
                <option key={item || "all"} value={item}>{item || "All statuses"}</option>
              ))}
            </select>
          </label>
          <label className="min-w-0 space-y-1.5 text-[13px] font-medium text-white/80">
            Skill
            <select
              value={skillIdParam}
              onChange={(event) => updateParams({ skillId: event.target.value, page: "" })}
              className="min-h-[42px] w-full rounded-xl border border-white/12 bg-[#14151c] px-3 py-2 text-base text-white sm:min-h-[40px] sm:text-sm"
            >
              <option value="">All skills</option>
              {(skillsQuery.data ?? []).map((skill) => (
                <option key={skill.skillId} value={skill.skillId}>{skill.name}</option>
              ))}
            </select>
          </label>
        </div>

        {projectsQuery.isError && <ErrorBanner error={projectsQuery.error} onRetry={() => void projectsQuery.refetch()} />}
        {skillsQuery.isError && <ErrorBanner error={skillsQuery.error} title="Could not load skill filters" />}
        {projectsQuery.isLoading && <p role="status" className="text-sm text-white/60">Loading projects…</p>}
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {items.map((project) => {
            const capacity = project.maxTeamSize > 0
              ? Math.min(100, Math.round((project.teamSize / project.maxTeamSize) * 100))
              : 0;
            return (
              <SpotlightCard key={project.projectId} className="flex flex-col justify-between rounded-2xl border border-white/10 bg-[#1b1d26] p-5 transition-colors hover:border-[#d3fb52]/40" showCornerArrow>
                <div>
                  <div className="flex items-start justify-between gap-3 pr-12">
                    <StatusPill status={project.status} />
                    <span className="text-xs text-white/50 tabular-nums">{new Date(project.createdAt).toLocaleDateString()}</span>
                  </div>
                  <h2 className="mt-4 text-lg font-bold text-white">{project.title}</h2>
                  <p className="mt-2 line-clamp-3 text-sm leading-relaxed text-white/60">{project.description}</p>
                  <p className="mt-4 text-xs text-white/50">Created by {project.creatorName}</p>
                </div>
                <div className="mt-5 space-y-3">
                  <div className="flex items-center justify-between text-xs text-white/65">
                    <span className="inline-flex items-center gap-1.5"><Users size={14} />Team capacity</span>
                    <span>{project.teamSize}/{project.maxTeamSize}</span>
                  </div>
                  <div
                    role="progressbar"
                    aria-label={`${project.title} team capacity`}
                    aria-valuemin={0}
                    aria-valuemax={100}
                    aria-valuenow={capacity}
                    className="h-1.5 overflow-hidden rounded-full bg-white/10"
                  >
                    <div className="h-full rounded-full bg-[#d3fb52]" style={{ width: `${capacity}%` }} />
                  </div>
                  {selectedSkill && <p className="text-xs text-white/45">Filtered by {selectedSkill.name}</p>}
                  <Link to={`/projects/${project.projectId}`} className="inline-flex items-center gap-2 text-sm font-semibold text-[#d3fb52] hover:underline">
                    View project <ArrowRight size={15} />
                  </Link>
                </div>
              </SpotlightCard>
            );
          })}
        </div>
        {!projectsQuery.isLoading && !projectsQuery.isError && items.length === 0 && (
          <p className="rounded-2xl border border-dashed border-white/15 p-8 text-center text-sm text-white/55">No projects match your filters.</p>
        )}
        {projectsQuery.data && projectsQuery.data.totalPages > 1 && (
          <Pagination
            page={safePage}
            totalPages={totalPages}
            totalCount={projectsQuery.data.totalCount}
            pageSize={pageSize}
            onPageChange={(nextPage) => updateParams({ page: String(nextPage) })}
          />
        )}
      </section>
    </div>
  );
}
