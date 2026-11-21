/**
 * @date 2026-11-22
 * @purpose PlatformAdmin overview dashboard displaying active/soft-deleted entity KPIs, role distribution bars, and STRIDE posture links.
 */
import {
  ArrowRight,
  Building2,
  ClipboardList,
  GraduationCap,
  RotateCcw,
  ShieldCheck,
  Trash2,
  Users,
} from "lucide-react";
import { Link } from "react-router-dom";
import {
  Badge,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  ErrorBanner,
  SkeletonCard,
} from "@/components/ui";
import { AdminConsoleLayout } from "../components/AdminConsoleLayout";
import { useAdminDashboardQuery } from "../hooks/useAdminHooks";

const ROLE_ORDER = ["Student", "OrgAdmin", "PlatformAdmin"] as const;

export function AdminDashboardPage() {
  const dashboardQuery = useAdminDashboardQuery();
  const stats = dashboardQuery.data;

  const totalRolesCount = stats
    ? Object.values(stats.roleDistribution).reduce((sum, count) => sum + count, 0)
    : 0;

  return (
    <AdminConsoleLayout
      title="Platform Overview"
      description="Monitor active academic records, soft-deleted items eligible for recovery, and student role distribution."
      actions={
        <Link
          to="/admin/audit-logs"
          className="inline-flex h-9 items-center gap-2 rounded-xl border border-white/12 bg-[#1b1d26] px-3.5 text-xs font-semibold text-white transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
        >
          <ClipboardList className="size-4" aria-hidden="true" />
          Inspect Audit Trail
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
            <section aria-label="Active entity metrics" className="grid gap-4 sm:grid-cols-3">
              <Card>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardDescription>Active Students</CardDescription>
                    <Users className="size-4 text-[#d3fb52]" aria-hidden="true" />
                  </div>
                  <CardTitle className="text-3xl font-bold tabular-nums">
                    {stats.activeStudentsCount}
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <Link
                    to="/admin/roles"
                    className="inline-flex items-center gap-1.5 text-xs font-semibold text-[#d3fb52] hover:underline"
                  >
                    Manage roles <ArrowRight className="size-3.5" aria-hidden="true" />
                  </Link>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardDescription>Active Universities</CardDescription>
                    <Building2 className="size-4 text-emerald-400" aria-hidden="true" />
                  </div>
                  <CardTitle className="text-3xl font-bold tabular-nums">
                    {stats.activeUniversitiesCount}
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <Link
                    to="/admin/catalog"
                    className="inline-flex items-center gap-1.5 text-xs font-semibold text-[#d3fb52] hover:underline"
                  >
                    Manage academic catalog <ArrowRight className="size-3.5" aria-hidden="true" />
                  </Link>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardDescription>Active Faculties</CardDescription>
                    <GraduationCap className="size-4 text-sky-400" aria-hidden="true" />
                  </div>
                  <CardTitle className="text-3xl font-bold tabular-nums">
                    {stats.activeFacultiesCount}
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-xs text-[#9ca3af]">
                    Cached anonymously via <span className="font-mono">/universities/catalog</span>
                  </p>
                </CardContent>
              </Card>
            </section>

            <section aria-label="Role distribution and recycle bin" className="grid gap-6 lg:grid-cols-12">
              <Card className="lg:col-span-7">
                <CardHeader>
                  <CardTitle>Role Distribution</CardTitle>
                  <CardDescription>
                    Breakdown across active student accounts ({totalRolesCount} total).
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {ROLE_ORDER.map((roleName) => {
                    const count = stats.roleDistribution[roleName] ?? 0;
                    const pct =
                      totalRolesCount > 0 ? Math.round((count / totalRolesCount) * 100) : 0;

                    return (
                      <div key={roleName} className="space-y-1.5">
                        <div className="flex items-center justify-between text-xs">
                          <span className="font-semibold text-white">{roleName}</span>
                          <span className="font-mono tabular-nums text-[#9ca3af]">
                            {count} ({pct}%)
                          </span>
                        </div>
                        <div className="h-2.5 overflow-hidden rounded-full bg-[#14151c]">
                          <div
                            className="h-full rounded-full bg-[#d3fb52] transition-all"
                            style={{ width: `${pct}%` }}
                          />
                        </div>
                      </div>
                    );
                  })}
                </CardContent>
              </Card>

              <Card className="lg:col-span-5">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle>Soft-Deleted Summary</CardTitle>
                    <Trash2 className="size-4 text-amber-400" aria-hidden="true" />
                  </div>
                  <CardDescription>
                    Recoverable core entities currently in the recycle bin.
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="flex items-center justify-between rounded-xl border border-white/8 bg-[#14151c]/80 px-3.5 py-2.5 text-sm">
                    <span className="text-white/80">Students</span>
                    <Badge variant="warning">{stats.softDeletedStudentsCount}</Badge>
                  </div>
                  <div className="flex items-center justify-between rounded-xl border border-white/8 bg-[#14151c]/80 px-3.5 py-2.5 text-sm">
                    <span className="text-white/80">Universities</span>
                    <Badge variant="warning">{stats.softDeletedUniversitiesCount}</Badge>
                  </div>
                  <div className="flex items-center justify-between rounded-xl border border-white/8 bg-[#14151c]/80 px-3.5 py-2.5 text-sm">
                    <span className="text-white/80">Faculties</span>
                    <Badge variant="warning">{stats.softDeletedFacultiesCount}</Badge>
                  </div>

                  <Link
                    to="/admin/restore"
                    className="mt-2 inline-flex h-10 w-full items-center justify-center gap-2 rounded-xl bg-[#d3fb52] text-sm font-semibold text-[#14151c] transition-colors hover:bg-[#deff6e]"
                  >
                    <RotateCcw className="size-4" aria-hidden="true" />
                    Open 8-Entity Recycle Bin
                  </Link>
                </CardContent>
              </Card>
            </section>

            <Card variant="elevated">
              <CardContent className="flex flex-col gap-3 pt-5 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex items-start gap-3">
                  <ShieldCheck className="mt-0.5 size-5 shrink-0 text-emerald-400" aria-hidden="true" />
                  <div>
                    <p className="text-sm font-semibold text-white">STRIDE Security Enforcement</p>
                    <p className="text-xs text-[#9ca3af]">
                      Role updates rotate target SecurityStamp tokens (`THR-E-02`), runtime
                      PlatformAdmin promotion is server-guarded (`THR-E-03`), and every recovery or
                      role mutation writes an immutable AuditLog (`THR-R-01`).
                    </p>
                  </div>
                </div>
                <Badge variant="success">Verified</Badge>
              </CardContent>
            </Card>
          </>
        )
      )}
    </AdminConsoleLayout>
  );
}
