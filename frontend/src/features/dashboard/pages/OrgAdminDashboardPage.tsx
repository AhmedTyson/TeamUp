/**
 * @date 2026-11-24
 * @purpose Dedicated OrgAdmin / High-Board Leadership Command Dashboard featuring scoped managed chapters, 4-stage recruitment funnel counts, hosted events & live QR Stage attendance launchpad.
 */
import {
  ArrowRight,
  Building2,
  CalendarDays,
  KanbanSquare,
  Layers,
  Network,
  QrCode,
  Users,
} from "lucide-react";
import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { RECRUITMENT_STAGES } from "@/api";
import { Badge, Card, EmptyState, Select, Skeleton } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { useEvents } from "@/features/events";
import {
  useOrganizationApplications,
  useOrganizations,
} from "@/features/organizations";
import { DashboardRoleSwitcher } from "../components/DashboardRoleSwitcher";

export function OrgAdminDashboardPage() {
  const { isPlatformAdmin, isOrgAdmin, orgAdminIds } = useAuth();
  const canLead = isPlatformAdmin || isOrgAdmin || orgAdminIds.length > 0;

  const orgsQuery = useOrganizations({ page: 1, pageSize: 50 }, canLead);
  const allOrgs = orgsQuery.data?.items ?? [];

  const managedOrgs = useMemo(() => {
    if (orgAdminIds.length > 0) {
      const scoped = allOrgs.filter((org) => orgAdminIds.includes(org.organizationId));
      return scoped.length > 0 ? scoped : allOrgs;
    }
    return allOrgs;
  }, [allOrgs, orgAdminIds]);

  const [selectedOrgId, setSelectedOrgId] = useState<string>("");
  const activeOrg =
    managedOrgs.find((org) => String(org.organizationId) === selectedOrgId) ?? managedOrgs[0];
  const primaryOrgId = activeOrg?.organizationId ?? 0;

  const appsQuery = useOrganizationApplications(primaryOrgId, canLead && primaryOrgId > 0);
  const eventsQuery = useEvents(
    {
      organizationId: primaryOrgId || undefined,
      page: 1,
      pageSize: 6,
    },
    canLead && primaryOrgId > 0,
  );

  const applications = appsQuery.data ?? [];
  const events = eventsQuery.data?.items ?? [];

  const funnelCounts = useMemo(() => {
    const counts = {
      Applied: 0,
      HrScreening: 0,
      CommitteeInterview: 0,
      Accepted: 0,
    };
    for (const app of applications) {
      const stage =
        app.stage ??
        (app.status === "Accepted"
          ? RECRUITMENT_STAGES.Accepted
          : RECRUITMENT_STAGES.Applied);
      if (stage === RECRUITMENT_STAGES.Applied) counts.Applied += 1;
      else if (stage === RECRUITMENT_STAGES.HrScreening) counts.HrScreening += 1;
      else if (stage === RECRUITMENT_STAGES.CommitteeInterview) counts.CommitteeInterview += 1;
      else if (stage === RECRUITMENT_STAGES.Accepted) counts.Accepted += 1;
    }
    return counts;
  }, [applications]);

  if (!canLead) {
    return (
      <div className="space-y-6">
        <DashboardRoleSwitcher />
        <EmptyState
          icon={<Building2 className="size-7" />}
          title="No organization leadership role assigned yet"
          description="Apply to join a chapter's High Board or Committee Leadership, or return to your Student Dashboard."
          action={
            <div className="flex flex-wrap justify-center gap-2">
              <Link
                to="/dashboard/student"
                className="inline-flex h-10 items-center rounded-xl bg-[#d3fb52] px-4 text-sm font-bold text-[#14151c]"
              >
                Open Student Dashboard
              </Link>
              <Link
                to="/organizations"
                className="inline-flex h-10 items-center rounded-xl border border-white/15 px-4 text-sm font-semibold text-white"
              >
                Explore Communities
              </Link>
            </div>
          }
        />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <DashboardRoleSwitcher />

      {/* Leadership Header */}
      <section className="rounded-[28px] border border-white/10 bg-[radial-gradient(ellipse_at_85%_0%,rgba(83,152,134,0.28),transparent_50%),linear-gradient(135deg,#1b1d26,#14151c)] p-6 shadow-2xl sm:p-8">
        <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
          <div className="space-y-2">
            <Badge variant="accent">High Board &amp; Chapter Governance</Badge>
            <h1 className="text-3xl font-bold tracking-tight text-white sm:text-4xl">
              Org Leadership Command Center
            </h1>
            <p className="max-w-2xl text-sm text-[#9ca3af]">
              Monitor 4-stage recruitment pipelines, manage 4-tier chapters &amp; committees, and launch Stage QR attendance for your hosted events.
            </p>
          </div>

          {managedOrgs.length > 0 && (
            <div className="w-full sm:w-72">
              <Select
                label="Active Organization"
                aria-label="Select managed organization"
                value={String(primaryOrgId)}
                options={managedOrgs.map((org) => ({
                  value: String(org.organizationId),
                  label: org.name,
                }))}
                onChange={(e) => setSelectedOrgId(e.target.value)}
              />
            </div>
          )}
        </div>

        {/* Active Org Quick Deep-Links */}
        {activeOrg && (
          <div className="mt-6 flex flex-wrap gap-2 border-t border-white/10 pt-5">
            <Link
              to={`/organizations/${activeOrg.organizationId}`}
              className="inline-flex items-center gap-1.5 rounded-xl bg-[#d3fb52] px-3.5 py-2 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
            >
              <Building2 className="size-3.5" aria-hidden="true" />
              Open Workspace
            </Link>
            <Link
              to={`/organizations/${activeOrg.organizationId}?tab=units`}
              className="inline-flex items-center gap-1.5 rounded-xl border border-white/15 bg-[#14151c] px-3.5 py-2 text-xs font-semibold text-white hover:border-[#d3fb52]/40"
            >
              <Layers className="size-3.5" aria-hidden="true" />
              Chapters &amp; Committees ({activeOrg.unitCount ?? 0})
            </Link>
            <Link
              to={`/organizations/${activeOrg.organizationId}?tab=hierarchy`}
              className="inline-flex items-center gap-1.5 rounded-xl border border-white/15 bg-[#14151c] px-3.5 py-2 text-xs font-semibold text-white hover:border-[#d3fb52]/40"
            >
              <Network className="size-3.5" aria-hidden="true" />
              4-Tier Org Hierarchy
            </Link>
            <Link
              to={`/organizations/${activeOrg.organizationId}?tab=applications`}
              className="inline-flex items-center gap-1.5 rounded-xl border border-white/15 bg-[#14151c] px-3.5 py-2 text-xs font-semibold text-white hover:border-[#d3fb52]/40"
            >
              <KanbanSquare className="size-3.5" aria-hidden="true" />
              Recruitment Kanban
            </Link>
            <Link
              to={`/organizations/${activeOrg.organizationId}?tab=participants`}
              className="inline-flex items-center gap-1.5 rounded-xl border border-white/15 bg-[#14151c] px-3.5 py-2 text-xs font-semibold text-white hover:border-[#d3fb52]/40"
            >
              <Users className="size-3.5" aria-hidden="true" />
              Participants Roster
            </Link>
          </div>
        )}
      </section>

      {/* Live 4-Stage Recruitment Funnel */}
      <Card className="space-y-4 p-5 sm:p-6">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div>
            <h2 className="text-lg font-bold text-white">
              Live Recruitment Funnel {activeOrg ? `· ${activeOrg.name}` : ""}
            </h2>
            <p className="text-xs text-[#9ca3af]">
              Real-time candidate distribution across your chapter screening stages.
            </p>
          </div>
          {activeOrg && (
            <Link
              to={`/organizations/${activeOrg.organizationId}?tab=applications`}
              className="inline-flex items-center gap-1 text-xs font-semibold text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]"
            >
              Manage Kanban <ArrowRight className="size-3.5" aria-hidden="true" />
            </Link>
          )}
        </div>

        {orgsQuery.isLoading || appsQuery.isLoading ? (
          <Skeleton className="h-24 rounded-2xl" />
        ) : (
          <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
            <Link
              to={activeOrg ? `/organizations/${activeOrg.organizationId}?tab=applications` : "/organizations"}
              className="min-w-0 rounded-2xl border border-white/10 bg-[#14151c] p-4 transition-colors hover:border-[#d3fb52]/40"
            >
              <p className="text-xs font-medium text-[#9ca3af]">1. Applied</p>
              <p className="mt-1 font-mono text-xl font-bold text-white sm:text-2xl">{funnelCounts.Applied}</p>
            </Link>
            <Link
              to={activeOrg ? `/organizations/${activeOrg.organizationId}?tab=applications` : "/organizations"}
              className="min-w-0 rounded-2xl border border-white/10 bg-[#14151c] p-4 transition-colors hover:border-amber-300/45"
            >
              <p className="text-xs font-medium text-[#9ca3af]">2. HR Screening</p>
              <p className="mt-1 font-mono text-xl font-bold text-amber-300 sm:text-2xl">
                {funnelCounts.HrScreening}
              </p>
            </Link>
            <Link
              to={activeOrg ? `/organizations/${activeOrg.organizationId}?tab=applications` : "/organizations"}
              className="min-w-0 rounded-2xl border border-white/10 bg-[#14151c] p-4 transition-colors hover:border-sky-300/45"
            >
              <p className="text-xs font-medium text-[#9ca3af]">3. Committee Interview</p>
              <p className="mt-1 font-mono text-xl font-bold text-sky-300 sm:text-2xl">
                {funnelCounts.CommitteeInterview}
              </p>
            </Link>
            <Link
              to={activeOrg ? `/organizations/${activeOrg.organizationId}?tab=applications` : "/organizations"}
              className="min-w-0 rounded-2xl border border-white/10 bg-[#14151c] p-4 transition-colors hover:border-[#d3fb52]/45"
            >
              <p className="text-xs font-medium text-[#9ca3af]">4. Accepted</p>
              <p className="mt-1 font-mono text-xl font-bold text-[#d3fb52] sm:text-2xl">
                {funnelCounts.Accepted}
              </p>
            </Link>
          </div>
        )}
      </Card>

      {/* Hosted Events & Session QR Attendance Launchpad */}
      <Card className="space-y-4 p-5 sm:p-6">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div>
            <h2 className="text-lg font-bold text-white">Hosted Events &amp; Stage QR Launchpad</h2>
            <p className="text-xs text-[#9ca3af]">
              Jump straight to live session check-in consoles, stage QR projectors, or CSV exports.
            </p>
          </div>
          <Link to="/events" className="text-xs font-semibold text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]">
            All Events →
          </Link>
        </div>

        {orgsQuery.isLoading || eventsQuery.isLoading ? (
          <Skeleton className="h-36 rounded-2xl" />
        ) : events.length > 0 ? (
          <div className="grid gap-4 md:grid-cols-2">
            {events.map((ev) => (
              <div
                key={ev.eventId}
                className="group relative flex cursor-pointer flex-col justify-between gap-3 rounded-2xl border border-white/10 bg-[#14151c] p-4 transition-colors hover:border-[#d3fb52]/40"
              >
                <div className="space-y-1.5">
                  <div className="flex items-center justify-between gap-2">
                    <Link
                      to={`/events/${ev.eventId}`}
                      className="truncate text-sm font-bold text-white transition-colors group-hover:text-[#d3fb52] after:absolute after:inset-0 after:content-['']"
                    >
                      {ev.title}
                    </Link>
                    <Badge variant="outline" size="sm">
                      {ev.deliveryMode ?? "InPerson"}
                    </Badge>
                  </div>
                  <p className="text-xs text-[#9ca3af]">
                    {ev.location} · {ev.registeredCount}/{ev.capacity} registered
                  </p>
                </div>

                <div className="flex flex-wrap items-center gap-2 border-t border-white/10 pt-3">
                  <span className="rounded-lg border border-white/15 px-2.5 py-1 text-xs font-semibold text-white/85 transition-colors group-hover:border-[#d3fb52]/40 group-hover:text-[#d3fb52]">
                    Event Hub
                  </span>
                  {ev.sessions.slice(0, 2).map((session) => (
                    <Link
                      key={session.sessionId}
                      to={`/events/${ev.eventId}/sessions/${session.sessionId}/attendance`}
                      className="relative z-10 inline-flex items-center gap-1 rounded-lg bg-[#d3fb52]/15 px-2.5 py-1 text-xs font-semibold text-[#d3fb52] transition-colors hover:bg-[#d3fb52]/25"
                    >
                      <QrCode className="size-3" aria-hidden="true" />
                      Session #{session.sessionNo} QR
                    </Link>
                  ))}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            icon={<CalendarDays className="size-6" />}
            title="No hosted events yet"
            description="Launch a technical workshop or multi-session bootcamp from Events Directory."
          />
        )}
      </Card>
    </div>
  );
}
