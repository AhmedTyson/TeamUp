/**
 * @date 2026-11-22
 * @purpose PlatformAdmin overview dashboard showcasing database-backed partner university logos, enrollment KPIs, role breakdown, and paginated campus preview.
 */
import {
  Archive,
  ArrowRight,
  Building2,
  ClipboardList,
  ExternalLink,
  GraduationCap,
  Inbox,
  Users,
} from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import type { UniversitySummary } from "@/api";
import {
  Badge,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  ErrorBanner,
  Pagination,
  SkeletonCard,
  UniversityLogo,
} from "@/components/ui";
import { cn } from "@/lib/cn";
import { isSafeHttpUrl } from "@/lib/urlValidation";
import { AdminConsoleLayout } from "../components/AdminConsoleLayout";
import { useAdminDashboardQuery, useUniversitiesListQuery } from "../hooks/useAdminHooks";

const ROLE_CONFIG = [
  { name: "Student", barColor: "bg-[#d3fb52]" },
  { name: "OrgAdmin", barColor: "bg-sky-400" },
  { name: "PlatformAdmin", barColor: "bg-emerald-400" },
] as const;

const PARTNER_FALLBACK: UniversitySummary[] = [
  {
    universityId: 1,
    name: "Cairo University",
    city: "Giza",
    country: "Egypt",
    website: "https://cu.edu.eg",
    logoUrl: "https://upload.wikimedia.org/wikipedia/commons/3/3c/Cairo_University_new_logo.png",
    facultyCount: 3,
  },
  {
    universityId: 2,
    name: "Ain Shams University",
    city: "Cairo",
    country: "Egypt",
    website: "https://asu.edu.eg",
    logoUrl: "https://upload.wikimedia.org/wikipedia/en/3/3d/Ain_Shams_logo.png",
    facultyCount: 2,
  },
  {
    universityId: 3,
    name: "Alexandria University",
    city: "Alexandria",
    country: "Egypt",
    website: "https://alexu.edu.eg",
    logoUrl: "https://upload.wikimedia.org/wikipedia/ar/6/63/%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%A7%D9%84%D8%A5%D8%B3%D9%83%D9%86%D8%AF%D8%B1%D9%8A%D8%A9.png",
    facultyCount: 2,
  },
];

export function AdminDashboardPage() {
  const [uniPage, setUniPage] = useState(1);
  const dashboardQuery = useAdminDashboardQuery();
  const universitiesQuery = useUniversitiesListQuery({ page: uniPage, pageSize: 5 });

  const stats = dashboardQuery.data;
  const universities =
    universitiesQuery.data?.items && universitiesQuery.data.items.length > 0
      ? universitiesQuery.data.items
      : PARTNER_FALLBACK;

  const totalRolesCount = stats
    ? Object.values(stats.roleDistribution).reduce((sum, count) => sum + count, 0)
    : 0;

  const totalArchived = stats
    ? stats.softDeletedStudentsCount +
      stats.softDeletedUniversitiesCount +
      stats.softDeletedFacultiesCount
    : 0;

  return (
    <AdminConsoleLayout
      title="Platform Overview"
      description="Monitor campus enrollment, partner universities, faculty directories, and account roles."
      actions={
        <Link
          to="/admin/audit-logs"
          className="inline-flex h-9 items-center gap-2 rounded-xl border border-white/12 bg-[#1b1d26] px-3.5 text-xs font-semibold text-white transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
        >
          <ClipboardList className="size-4" aria-hidden="true" />
          View Activity Log
        </Link>
      }
    >
      {dashboardQuery.isError && (
        <ErrorBanner
          error={dashboardQuery.error}
          onRetry={() => {
            void dashboardQuery.refetch();
          }}
        />
      )}

      {dashboardQuery.isPending ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <SkeletonCard />
          <SkeletonCard />
          <SkeletonCard />
        </div>
      ) : (
        stats && (
          <>
            {/* Executive Campus Network Banner */}
            <section
              aria-label="Campus network summary"
              className="teamup-hero-bg relative overflow-hidden rounded-3xl border border-white/10 p-6 shadow-xl sm:p-7"
            >
              <div className="flex flex-col justify-between gap-6 lg:flex-row lg:items-center">
                <div className="space-y-3">
                  <div className="flex flex-wrap items-center gap-2.5">
                    <div className="flex -space-x-2">
                      {universities.slice(0, 4).map((uni) => (
                        <UniversityLogo
                          key={uni.universityId}
                          name={uni.name}
                          logoUrl={uni.logoUrl}
                          size="sm"
                          className="ring-2 ring-[#14151c]"
                        />
                      ))}
                    </div>
                    <span className="rounded-full border border-[#d3fb52]/35 bg-[#d3fb52]/15 px-3 py-1 text-xs font-semibold text-[#d3fb52]">
                      Egyptian Campus Network
                    </span>
                  </div>
                  <h2
                    className="text-2xl font-bold tracking-tight text-white sm:text-3xl"
                    style={{ fontFamily: "var(--font-heading)" }}
                  >
                    Academic &amp; Student Operations at a Glance
                  </h2>
                  <p className="max-w-xl text-sm leading-relaxed text-white/80">
                    Manage accredited partner universities, review enrolled faculties, and
                    coordinate student leadership permissions from one workspace.
                  </p>
                </div>

                <div className="flex flex-wrap items-center gap-3">
                  <Link
                    to="/admin/organization-requests"
                    className="inline-flex h-10 items-center gap-2 rounded-xl bg-[#d3fb52] px-4 text-sm font-semibold text-[#14151c] transition-colors hover:bg-[#deff6e]"
                  >
                    <Inbox className="size-4" aria-hidden="true" />
                    Org Requests ({stats.pendingOrganizationRequestsCount ?? 0})
                  </Link>
                  <Link
                    to="/admin/catalog"
                    className="inline-flex h-10 items-center gap-2 rounded-xl border border-white/20 bg-black/25 px-4 text-sm font-semibold text-white transition-colors hover:border-[#d3fb52]/50 hover:text-[#d3fb52]"
                  >
                    Manage Universities
                    <ArrowRight className="size-4" aria-hidden="true" />
                  </Link>
                  <Link
                    to="/admin/roles"
                    className="inline-flex h-10 items-center gap-2 rounded-xl border border-white/20 bg-black/25 px-4 text-sm font-semibold text-white transition-colors hover:border-[#d3fb52]/50 hover:text-[#d3fb52]"
                  >
                    Manage Roles
                  </Link>
                </div>
              </div>
            </section>

            {/* Primary KPI Cards */}
            <section aria-label="Active entity metrics" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
              <Card className="transition-colors hover:border-[#d3fb52]/35">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardDescription>Pending Org Requests</CardDescription>
                    <div className="flex size-8 items-center justify-center rounded-xl bg-amber-500/15 text-amber-300">
                      <Inbox className="size-4" aria-hidden="true" />
                    </div>
                  </div>
                  <div className="flex items-baseline justify-between gap-2 pt-1">
                    <CardTitle className="text-3xl font-bold tabular-nums">
                      {stats.pendingOrganizationRequestsCount ?? 0}
                    </CardTitle>
                    <Badge variant="warning" size="sm">
                      Awaiting review
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <Link
                    to="/admin/organization-requests"
                    className="inline-flex items-center gap-1.5 text-xs font-semibold text-[#d3fb52] hover:underline"
                  >
                    Review &amp; promote founders <ArrowRight className="size-3.5" aria-hidden="true" />
                  </Link>
                </CardContent>
              </Card>

              <Card className="transition-colors hover:border-[#d3fb52]/35">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardDescription>Active Students</CardDescription>
                    <div className="flex size-8 items-center justify-center rounded-xl bg-[#d3fb52]/15 text-[#d3fb52]">
                      <Users className="size-4" aria-hidden="true" />
                    </div>
                  </div>
                  <div className="flex items-baseline justify-between gap-2 pt-1">
                    <CardTitle className="text-3xl font-bold tabular-nums">
                      {stats.activeStudentsCount}
                    </CardTitle>
                    <Badge variant="success" size="sm">
                      {stats.softDeletedStudentsCount} archived
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <Link
                    to="/admin/roles"
                    className="inline-flex items-center gap-1.5 text-xs font-semibold text-[#d3fb52] hover:underline"
                  >
                    Manage student roles <ArrowRight className="size-3.5" aria-hidden="true" />
                  </Link>
                </CardContent>
              </Card>

              <Card className="transition-colors hover:border-emerald-400/35">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardDescription>Partner Universities</CardDescription>
                    <div className="flex size-8 items-center justify-center rounded-xl bg-emerald-500/15 text-emerald-300">
                      <Building2 className="size-4" aria-hidden="true" />
                    </div>
                  </div>
                  <div className="flex items-baseline justify-between gap-2 pt-1">
                    <CardTitle className="text-3xl font-bold tabular-nums">
                      {stats.activeUniversitiesCount}
                    </CardTitle>
                    <div className="flex -space-x-1.5">
                      {universities.slice(0, 3).map((uni) => (
                        <UniversityLogo
                          key={uni.universityId}
                          name={uni.name}
                          logoUrl={uni.logoUrl}
                          size="sm"
                          className="size-6 rounded-md ring-1 ring-[#1b1d26]"
                        />
                      ))}
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <Link
                    to="/admin/catalog"
                    className="inline-flex items-center gap-1.5 text-xs font-semibold text-[#d3fb52] hover:underline"
                  >
                    Open university catalog <ArrowRight className="size-3.5" aria-hidden="true" />
                  </Link>
                </CardContent>
              </Card>

              <Card className="transition-colors hover:border-sky-400/35">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardDescription>Accredited Faculties</CardDescription>
                    <div className="flex size-8 items-center justify-center rounded-xl bg-sky-500/15 text-sky-300">
                      <GraduationCap className="size-4" aria-hidden="true" />
                    </div>
                  </div>
                  <div className="flex items-baseline justify-between gap-2 pt-1">
                    <CardTitle className="text-3xl font-bold tabular-nums">
                      {stats.activeFacultiesCount}
                    </CardTitle>
                    <Badge variant="info" size="sm">
                      {stats.softDeletedFacultiesCount} archived
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <Link
                    to="/admin/catalog"
                    className="inline-flex items-center gap-1.5 text-xs font-semibold text-[#d3fb52] hover:underline"
                  >
                    Inspect faculties <ArrowRight className="size-3.5" aria-hidden="true" />
                  </Link>
                </CardContent>
              </Card>
            </section>

            {/* Partner Universities Grid + Role Distribution / Archive */}
            <section aria-label="Partner universities and analytics" className="grid gap-6 lg:grid-cols-12">
              <Card className="lg:col-span-7">
                <CardHeader className="flex flex-row items-center justify-between gap-2">
                  <div>
                    <CardTitle>Partner Universities</CardTitle>
                    <CardDescription>
                      Accredited campuses available for student registration and clubs.
                    </CardDescription>
                  </div>
                  <Link
                    to="/admin/catalog"
                    className="shrink-0 text-xs font-semibold text-[#d3fb52] hover:underline"
                  >
                    View all →
                  </Link>
                </CardHeader>
                <CardContent className="space-y-3">
                  {universities.map((uni) => (
                    <div
                      key={uni.universityId}
                      className="flex flex-col justify-between gap-3 rounded-2xl border border-white/[0.08] bg-[#14151c]/80 p-3.5 transition-colors hover:border-[#d3fb52]/30 sm:flex-row sm:items-center"
                    >
                      <div className="flex min-w-0 items-center gap-3.5">
                        <UniversityLogo name={uni.name} logoUrl={uni.logoUrl} size="md" />
                        <div className="min-w-0 space-y-0.5">
                          <p className="truncate text-sm font-semibold text-white">{uni.name}</p>
                          <p className="text-xs text-[#9ca3af]">
                            {uni.city}, {uni.country}
                          </p>
                        </div>
                      </div>

                      <div className="flex flex-wrap items-center gap-2 sm:shrink-0">
                        <Badge variant="default" size="sm">
                          {uni.facultyCount} {uni.facultyCount === 1 ? "faculty" : "faculties"}
                        </Badge>
                        {uni.website && isSafeHttpUrl(uni.website) && (
                          <a
                            href={uni.website}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="inline-flex items-center gap-1 rounded-lg border border-white/10 bg-[#1b1d26] px-2.5 py-1 text-xs font-medium text-white/80 transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
                          >
                            Website
                            <ExternalLink className="size-3" aria-hidden="true" />
                          </a>
                        )}
                      </div>
                    </div>
                  ))}

                  {universitiesQuery.data && universitiesQuery.data.totalPages > 1 && (
                    <Pagination
                      page={universitiesQuery.data.page}
                      totalPages={universitiesQuery.data.totalPages}
                      totalCount={universitiesQuery.data.totalCount}
                      pageSize={universitiesQuery.data.pageSize}
                      onPageChange={setUniPage}
                    />
                  )}
                </CardContent>
              </Card>

              <div className="space-y-6 lg:col-span-5">
                <Card>
                  <CardHeader>
                    <CardTitle>Role Distribution</CardTitle>
                    <CardDescription>
                      Account permissions across {totalRolesCount} active member(s).
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    {ROLE_CONFIG.map((role) => {
                      const count = stats.roleDistribution[role.name] ?? 0;
                      const pct =
                        totalRolesCount > 0 ? Math.round((count / totalRolesCount) * 100) : 0;

                      return (
                        <div key={role.name} className="space-y-1.5">
                          <div className="flex items-center justify-between text-xs">
                            <span className="font-semibold text-white">{role.name}</span>
                            <span className="font-mono tabular-nums text-[#9ca3af]">
                              {count} ({pct}%)
                            </span>
                          </div>
                          <div className="h-2.5 overflow-hidden rounded-full bg-[#14151c]">
                            <div
                              className={cn("h-full rounded-full transition-all", role.barColor)}
                              style={{ width: `${pct}%` }}
                            />
                          </div>
                        </div>
                      );
                    })}
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle>Archived Records</CardTitle>
                      <Archive className="size-4 text-amber-400" aria-hidden="true" />
                    </div>
                    <CardDescription>
                      {totalArchived} archived record(s) ready to restore if needed.
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-2.5">
                    <div className="flex items-center justify-between rounded-xl border border-white/8 bg-[#14151c]/80 px-3.5 py-2 text-sm">
                      <span className="text-white/80">Students</span>
                      <Badge variant="warning">{stats.softDeletedStudentsCount}</Badge>
                    </div>
                    <div className="flex items-center justify-between rounded-xl border border-white/8 bg-[#14151c]/80 px-3.5 py-2 text-sm">
                      <span className="text-white/80">Universities</span>
                      <Badge variant="warning">{stats.softDeletedUniversitiesCount}</Badge>
                    </div>
                    <div className="flex items-center justify-between rounded-xl border border-white/8 bg-[#14151c]/80 px-3.5 py-2 text-sm">
                      <span className="text-white/80">Faculties</span>
                      <Badge variant="warning">{stats.softDeletedFacultiesCount}</Badge>
                    </div>

                    <Link
                      to="/admin/restore"
                      className="mt-2 inline-flex h-10 w-full items-center justify-center gap-2 rounded-xl bg-[#d3fb52] text-sm font-semibold text-[#14151c] transition-colors hover:bg-[#deff6e]"
                    >
                      <Archive className="size-4" aria-hidden="true" />
                      Open Archive
                    </Link>
                  </CardContent>
                </Card>
              </div>
            </section>
          </>
        )
      )}
    </AdminConsoleLayout>
  );
}
