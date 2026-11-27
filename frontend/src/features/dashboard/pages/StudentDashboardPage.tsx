/**
 * @date 2026-11-24
 * @purpose Dedicated Student Dashboard summarizing XP & level progress, registered Event Digital Passes, active community applications, open squads, notifications, and full platform directory.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ArrowRight,
  Award,
  Bell,
  Building2,
  CalendarDays,
  FolderKanban,
  MailCheck,
  Plus,
  QrCode,
  Send,
  Sparkles,
  Trophy,
  UserCheck,
} from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import {
  authApi,
  gamificationApi,
  notificationsApi,
  projectsApi,
  PROJECT_STATUSES,
} from "@/api";
import {
  Badge,
  Button,
  Card,
  EmptyState,
  Skeleton,
  StatusPill,
} from "@/components/ui";
import { useAuth } from "@/features/auth";
import { useMyEventRegistrations } from "@/features/events";
import { useUnreadNotificationCount } from "@/features/notifications/useUnreadNotificationCount";
import { useMyOrganizationApplications } from "@/features/organizations";
import { queryKeys } from "@/lib/queryKeys";
import { DashboardRoleSwitcher } from "../components/DashboardRoleSwitcher";

const QUICK_WORKSPACE_LINKS = [
  { label: "Explore Projects", to: "/projects", hint: "Browse & join squads", icon: FolderKanban },
  { label: "Create Project", to: "/projects/new", hint: "Recruit teammates", icon: Plus },
  { label: "Skills Catalog", to: "/skills", hint: "Tag expertise", icon: Sparkles },
  { label: "Student Directory", to: "/students", hint: "Find peers", icon: UserCheck },
  { label: "Activities Hub", to: "/organizations", hint: "Branches & clubs", icon: Building2 },
  { label: "Events & Bootcamps", to: "/events", hint: "Passes & seats", icon: CalendarDays },
  { label: "Self Check-In", to: "/check-in", hint: "QR / camera pass", icon: QrCode },
  { label: "Gamification Hub", to: "/gamification", hint: "XP & badges", icon: Award },
  { label: "Leaderboards", to: "/leaderboard", hint: "Campus rankings", icon: Trophy },
  { label: "Profile Studio", to: "/profile/me", hint: "Bio & links", icon: UserCheck },
] as const;

export function StudentDashboardPage() {
  const { user, refreshSession } = useAuth();
  const queryClient = useQueryClient();
  const [verificationSent, setVerificationSent] = useState(false);
  const studentId = user?.studentId ?? 0;

  const sendVerificationMutation = useMutation({
    mutationFn: () => authApi.sendVerificationEmail(),
    onSuccess: async (res) => {
      setVerificationSent(true);
      toast.success(res.message);
      if (res.alreadyVerified) {
        await queryClient.invalidateQueries({ queryKey: queryKeys.auth.me });
        await refreshSession();
      }
    },
    onError: (err: unknown) => {
      toast.error(err instanceof Error ? err.message : "Could not send verification email.");
    },
  });

  const scoreQuery = useQuery({
    queryKey: queryKeys.gamification.score(studentId),
    queryFn: ({ signal }) => gamificationApi.getStudentScore(studentId, signal),
    enabled: studentId > 0,
  });

  const myEventsQuery = useMyEventRegistrations(studentId > 0);
  const myAppsQuery = useMyOrganizationApplications(studentId > 0);

  const notificationsQuery = useQuery({
    queryKey: queryKeys.notifications.inbox(false),
    queryFn: ({ signal }) => notificationsApi.getNotifications(false, 15, signal),
    enabled: studentId > 0,
  });

  const unreadCountQuery = useUnreadNotificationCount({ enabled: studentId > 0 });

  const openProjectsQuery = useQuery({
    queryKey: queryKeys.projects.list({ status: PROJECT_STATUSES.Recruiting, page: 1, pageSize: 4 }),
    queryFn: ({ signal }) =>
      projectsApi.list({ status: PROJECT_STATUSES.Recruiting, page: 1, pageSize: 4 }, signal),
  });

  const registrations = myEventsQuery.data?.items ?? [];
  const applications = myAppsQuery.data ?? [];
  const notifications = notificationsQuery.data ?? [];
  const unreadNotificationsCount = unreadCountQuery.data ?? notifications.filter((n) => !n.isRead).length;
  const openProjects = openProjectsQuery.data?.items ?? [];

  const affiliationText = user?.universityName
    ? `${user.universityName}${user.facultyName ? ` · ${user.facultyName}` : ""}`
    : `${user?.affiliationType ?? "IndependentLearner"}${user?.city ? ` · ${user.city}` : ""}`;

  return (
    <div className="space-y-6">
      <DashboardRoleSwitcher />

      {user && !user.isEmailVerified && (
        <div className="flex flex-col justify-between gap-3 rounded-2xl border border-amber-400/35 bg-amber-400/[0.08] px-5 py-4 sm:flex-row sm:items-center">
          <div className="flex items-start gap-3">
            <MailCheck className="mt-0.5 size-5 shrink-0 text-amber-300" aria-hidden="true" />
            <div>
              <p className="text-sm font-bold text-white">Verify your campus email address</p>
              <p className="text-xs text-white/75">
                {verificationSent
                  ? "Verification link sent! Check your inbox or enter your token to earn your Verified Student badge."
                  : `Confirm ${user.email} to activate your Verified Student badge across the directory and leaderboards.`}
              </p>
            </div>
          </div>
          <div className="flex shrink-0 flex-wrap items-center gap-2">
            <Button
              size="sm"
              variant="primary"
              isLoading={sendVerificationMutation.isPending}
              leftIcon={<Send className="size-3.5" aria-hidden="true" />}
              onClick={() => sendVerificationMutation.mutate()}
            >
              {verificationSent ? "Resend Verification Link" : "Send Verification Link"}
            </Button>
            <Link
              to="/auth/verify-email"
              className="inline-flex h-8 items-center rounded-xl border border-white/15 bg-[#14151c] px-3 text-xs font-semibold text-white transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
            >
              Verify Token
            </Link>
          </div>
        </div>
      )}

      {/* Student Welcome & KPI Header */}
      <section className="rounded-[28px] border border-white/10 bg-[radial-gradient(ellipse_at_85%_0%,rgba(211,251,82,0.16),transparent_48%),linear-gradient(135deg,#1b1d26,#14151c)] p-6 shadow-2xl sm:p-8">
        <div className="flex flex-col justify-between gap-5 lg:flex-row lg:items-center">
          <div className="space-y-2">
            <Badge variant="accent">Student Activity Hub</Badge>
            <h1 className="text-3xl font-bold tracking-tight text-white sm:text-4xl">
              Welcome back, {user?.firstName ?? "Builder"}
            </h1>
            <p className="text-sm text-[#9ca3af]">{affiliationText}</p>
          </div>

          <div className="flex flex-wrap gap-2.5">
            <Link
              to="/check-in"
              className="inline-flex h-10 items-center gap-2 rounded-xl bg-[#d3fb52] px-4 text-sm font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
            >
              <QrCode className="size-4" aria-hidden="true" />
              QR Check-In
            </Link>
            <Link
              to="/projects/new"
              className="inline-flex h-10 items-center gap-2 rounded-xl border border-white/15 bg-white/[0.04] px-4 text-sm font-semibold text-white transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
            >
              <Plus className="size-4" aria-hidden="true" />
              New Project
            </Link>
            <Link
              to="/organizations?request=1"
              className="inline-flex h-10 items-center gap-2 rounded-xl border border-white/15 bg-white/[0.04] px-4 text-sm font-semibold text-white transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
            >
              <Building2 className="size-4" aria-hidden="true" />
              Request Activity
            </Link>
            <Link
              to="/profile/me"
              className="inline-flex h-10 items-center gap-2 rounded-xl border border-white/15 bg-white/[0.04] px-4 text-sm font-semibold text-white transition-colors hover:border-[#d3fb52]/40"
            >
              Profile Studio
            </Link>
          </div>
        </div>

        {/* Live 4-Card Summary Strip */}
        <div className="mt-6 grid grid-cols-2 gap-3 lg:grid-cols-4">
          <Link
            to="/gamification"
            className="min-w-0 rounded-2xl border border-white/10 bg-[#14151c]/85 p-4 transition-colors hover:border-[#d3fb52]/40"
          >
            <p className="text-xs font-medium text-[#9ca3af]">Level &amp; Verified XP</p>
            <p className="mt-1.5 break-words font-mono text-lg font-bold text-[#d3fb52] sm:text-2xl">
              {scoreQuery.isLoading
                ? "…"
                : `Lvl ${scoreQuery.data?.level ?? 1} · ${scoreQuery.data?.totalPoints ?? 0} XP`}
            </p>
          </Link>

          <Link
            to="/events"
            className="min-w-0 rounded-2xl border border-white/10 bg-[#14151c]/85 p-4 transition-colors hover:border-[#d3fb52]/40"
          >
            <p className="text-xs font-medium text-[#9ca3af]">My Event Passes</p>
            <p className="mt-1.5 font-mono text-lg font-bold text-white sm:text-2xl">
              {myEventsQuery.isLoading ? "…" : (myEventsQuery.data?.totalCount ?? 0)}
            </p>
          </Link>

          <Link
            to="/organizations"
            className="min-w-0 rounded-2xl border border-white/10 bg-[#14151c]/85 p-4 transition-colors hover:border-[#d3fb52]/40"
          >
            <p className="text-xs font-medium text-[#9ca3af]">Club Applications</p>
            <p className="mt-1.5 font-mono text-lg font-bold text-white sm:text-2xl">
              {myAppsQuery.isLoading ? "…" : applications.length}
            </p>
          </Link>

          <Link
            to="/notifications"
            className="min-w-0 rounded-2xl border border-white/10 bg-[#14151c]/85 p-4 transition-colors hover:border-[#d3fb52]/40"
          >
            <p className="text-xs font-medium text-[#9ca3af]">Unread Alerts</p>
            <p className="mt-1.5 font-mono text-lg font-bold text-sky-300 sm:text-2xl">
              {unreadCountQuery.isLoading ? "…" : unreadNotificationsCount}
            </p>
          </Link>
        </div>
      </section>

      {/* Main Two-Column Activity Workspace */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Registered Events & Digital Passes */}
        <Card className="space-y-4 p-5 sm:p-6">
          <div className="flex items-center justify-between gap-2">
            <div>
              <h2 className="text-lg font-bold text-white">My Event Passes &amp; Workshops</h2>
              <p className="text-xs text-[#9ca3af]">
                Synced cross-device with attendance counts and calendar passes.
              </p>
            </div>
            <Link to="/events" className="text-xs font-semibold text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]">
              Browse events →
            </Link>
          </div>

          {myEventsQuery.isLoading ? (
            <Skeleton className="h-36 rounded-2xl" />
          ) : registrations.length > 0 ? (
            <div className="space-y-2.5">
              {registrations.slice(0, 4).map((reg) => (
                <Link
                  key={reg.eventId}
                  to={`/events/${reg.eventId}`}
                  className="flex items-center justify-between gap-3 rounded-2xl border border-white/10 bg-[#14151c] p-3.5 transition-colors hover:border-[#d3fb52]/40"
                >
                  <div className="min-w-0 space-y-1">
                    <p className="truncate text-sm font-semibold text-white">{reg.eventTitle}</p>
                    <p className="truncate text-xs text-[#9ca3af]">
                      {reg.organizationName} · {reg.deliveryMode} · Attended{" "}
                      {reg.sessionsAttendedCount}/{reg.totalSessionsCount}
                    </p>
                  </div>
                  <StatusPill status={reg.status} />
                </Link>
              ))}
            </div>
          ) : (
            <EmptyState
              icon={<CalendarDays className="size-6" />}
              title="No event registrations yet"
              description="Discover upcoming campus workshops and bootcamps to reserve your Digital Pass."
              action={
                <Link
                  to="/events"
                  className="inline-flex h-9 items-center rounded-xl bg-[#d3fb52] px-3.5 text-xs font-bold text-[#14151c]"
                >
                  Explore Events
                </Link>
              }
            />
          )}
        </Card>

        {/* Community Applications */}
        <Card className="space-y-4 p-5 sm:p-6">
          <div className="flex items-center justify-between gap-2">
            <div>
              <h2 className="text-lg font-bold text-white">My Community Applications</h2>
              <p className="text-xs text-[#9ca3af]">
                Track live HR screening and committee interview stages across chapters.
              </p>
            </div>
            <Link
              to="/organizations?scope=recruiting"
              className="text-xs font-semibold text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]"
            >
              Open recruitment →
            </Link>
          </div>

          {myAppsQuery.isLoading ? (
            <Skeleton className="h-36 rounded-2xl" />
          ) : applications.length > 0 ? (
            <div className="space-y-2.5">
              {applications.slice(0, 4).map((app) => (
                <Link
                  key={app.applicationId}
                  to={`/organizations/${app.organizationId}?tab=applications`}
                  className="flex items-center justify-between gap-3 rounded-2xl border border-white/10 bg-[#14151c] p-3.5 transition-colors hover:border-[#d3fb52]/40"
                >
                  <div className="min-w-0 space-y-1">
                    <p className="truncate text-sm font-semibold text-white">
                      {app.organizationName ?? `Organization #${app.organizationId}`}
                    </p>
                    <p className="truncate text-xs text-[#9ca3af]">
                      {app.unitName ? `${app.unitCode ?? ""} · ${app.unitName}` : "General Track"} · Stage:{" "}
                      <span className="text-white">{app.stage ?? app.status}</span>
                    </p>
                  </div>
                  <StatusPill status={app.status} />
                </Link>
              ))}
            </div>
          ) : (
            <EmptyState
              icon={<Building2 className="size-6" />}
              title="No active club applications"
              description="Apply to IEEE branches, Enactus, GDG chapters, or open collectives."
              action={
                <Link
                  to="/organizations"
                  className="inline-flex h-9 items-center rounded-xl bg-[#d3fb52] px-3.5 text-xs font-bold text-[#14151c]"
                >
                  Browse Communities
                </Link>
              }
            />
          )}
        </Card>
      </div>

      {/* Bottom Row: Recruiting Squads + Notifications */}
      <div className="grid gap-6 lg:grid-cols-2">
        <Card className="space-y-4 p-5 sm:p-6">
          <div className="flex items-center justify-between gap-2">
            <h2 className="text-lg font-bold text-white">Recruiting Project Squads</h2>
            <Link to="/projects" className="text-xs font-semibold text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]">
              All projects →
            </Link>
          </div>
          {openProjectsQuery.isLoading ? (
            <Skeleton className="h-32 rounded-2xl" />
          ) : openProjects.length > 0 ? (
            <div className="space-y-2.5">
              {openProjects.map((proj) => (
                <Link
                  key={proj.projectId}
                  to={`/projects/${proj.projectId}`}
                  className="flex items-center justify-between gap-3 rounded-2xl border border-white/10 bg-[#14151c] p-3.5 transition-colors hover:border-[#d3fb52]/40"
                >
                  <div className="min-w-0">
                    <p className="truncate text-sm font-semibold text-white">{proj.title}</p>
                    <p className="text-xs text-[#9ca3af]">
                      Lead: {proj.creatorName} · {proj.teamSize}/{proj.maxTeamSize} members
                    </p>
                  </div>
                  <ArrowRight className="size-4 shrink-0 text-[#d3fb52]" aria-hidden="true" />
                </Link>
              ))}
            </div>
          ) : (
            <p className="text-sm text-[#9ca3af]">No recruiting projects at the moment.</p>
          )}
        </Card>

        <Card className="space-y-4 p-5 sm:p-6">
          <div className="flex items-center justify-between gap-2">
            <h2 className="text-lg font-bold text-white">Recent Notifications</h2>
            <Link
              to="/notifications"
              className="text-xs font-semibold text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]"
            >
              Open inbox →
            </Link>
          </div>
          {notificationsQuery.isLoading ? (
            <Skeleton className="h-32 rounded-2xl" />
          ) : notifications.length > 0 ? (
            <div className="space-y-2.5">
              {notifications.slice(0, 3).map((item) => (
                <Link
                  key={item.notificationId}
                  to="/notifications"
                  className="block rounded-2xl border border-white/10 bg-[#14151c] p-3.5 transition-colors hover:border-[#d3fb52]/40"
                >
                  <div className="flex items-center justify-between gap-2">
                    <p className="truncate text-sm font-semibold text-white">{item.title}</p>
                    {!item.isRead && <Badge variant="accent" size="sm">New</Badge>}
                  </div>
                  <p className="mt-1 line-clamp-2 text-xs text-[#9ca3af]">{item.message}</p>
                </Link>
              ))}
            </div>
          ) : (
            <EmptyState
              icon={<Bell className="size-6" />}
              title="All caught up"
              description="No new alerts in your inbox."
            />
          )}
        </Card>
      </div>

      {/* Complete Workspace Directory Strip */}
      <Card className="space-y-4 p-5 sm:p-6">
        <div className="flex items-center justify-between gap-2">
          <div>
            <h2 className="text-base font-bold text-white">All Student Workspaces</h2>
            <p className="text-xs text-[#9ca3af]">
              Direct entry points to every TeamUp module, directory, and tool.
            </p>
          </div>
          <span className="hidden font-mono text-[11px] text-white/50 sm:inline">
            Tip: press Ctrl+K anywhere
          </span>
        </div>

        <div className="grid grid-cols-2 gap-2.5 sm:grid-cols-3 lg:grid-cols-5">
          {QUICK_WORKSPACE_LINKS.map((item) => {
            const Icon = item.icon;
            return (
              <Link
                key={item.to}
                to={item.to}
                className="flex items-center gap-2.5 rounded-xl border border-white/10 bg-[#14151c] p-3 transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
              >
                <Icon className="size-4 shrink-0 text-[#d3fb52]" aria-hidden="true" />
                <div className="min-w-0">
                  <p className="truncate text-xs font-semibold text-white">{item.label}</p>
                  <p className="truncate text-[11px] text-[#9ca3af]">{item.hint}</p>
                </div>
              </Link>
            );
          })}
        </div>
      </Card>
    </div>
  );
}
