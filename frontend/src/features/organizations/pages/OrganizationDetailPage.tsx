/**
 * @date 2026-11-24
 * @purpose Organization workspace page featuring Scope/University/Recruitment hero, Chapters & Committees, interactive 4-tier Org-Chart, Kanban Recruitment, Participants, and Hosted Events.
 */
import {
  ArrowLeft,
  Building2,
  CalendarDays,
  Globe,
  Layers,
  MapPin,
  Sparkles,
  Users,
} from "lucide-react";
import { useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { organizationsApi } from "@/api";
import {
  Badge,
  Button,
  Card,
  ConfirmDialog,
  EmptyState,
  ErrorBanner,
  Skeleton,
  Tabs,
} from "@/components/ui";
import { useAuth } from "@/features/auth/AuthContext";
import { triggerFileDownload } from "@/lib/downloadFile";
import { isSafeHttpUrl } from "@/lib/urlValidation";
import { OrganizationApplicationsTab } from "../components/OrganizationApplicationsTab";
import { OrganizationFormModal } from "../components/OrganizationFormModal";
import { OrganizationHierarchyTab } from "../components/OrganizationHierarchyTab";
import { OrganizationMark } from "../components/OrganizationMark";
import { OrganizationMembersTab } from "../components/OrganizationMembersTab";
import { OrganizationParticipantsTab } from "../components/OrganizationParticipantsTab";
import { OrganizationUnitsTab } from "../components/OrganizationUnitsTab";
import { RecruitmentPipelineTab } from "../components/RecruitmentPipelineTab";
import {
  useMyOrganizationApplications,
  useOrganization,
  useOrganizationApplications,
  useOrganizationHierarchy,
  useOrganizationMutations,
  useOrganizationParticipants,
  useOrganizationUnits,
} from "../hooks/useOrganizationHooks";
import {
  canManageOrganization,
  canReviewOrgRecruitment,
  isOrganizationMember,
} from "../utils/authorization";

const VALID_ORG_TABS = new Set([
  "overview",
  "units",
  "hierarchy",
  "members",
  "applications",
  "participants",
  "events",
]);

export function OrganizationDetailPage() {
  const { id: rawId } = useParams();
  const id = Number(rawId);
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { user, isAuthenticated, isPlatformAdmin, isOrgAdminOf } = useAuth();

  const initialTab = searchParams.get("tab");
  const [tab, setTabState] = useState(() =>
    initialTab && VALID_ORG_TABS.has(initialTab) ? initialTab : "overview",
  );
  const setTab = (nextTab: string) => {
    setTabState(nextTab);
    const next = new URLSearchParams(searchParams);
    if (nextTab === "overview") {
      next.delete("tab");
    } else {
      next.set("tab", nextTab);
    }
    setSearchParams(next, { replace: true });
  };
  const [editOpen, setEditOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);

  const isValidId = Number.isInteger(id) && id > 0;
  const orgQuery = useOrganization(id);
  const org = orgQuery.data;
  const members = org?.members ?? [];

  const canManage = Boolean(
    org &&
      canManageOrganization(id, user?.studentId, isPlatformAdmin, isOrgAdminOf, members),
  );
  const canReviewRecruitment = Boolean(
    org &&
      canReviewOrgRecruitment(id, user?.studentId, isPlatformAdmin, isOrgAdminOf, members),
  );
  const isMember = isOrganizationMember(user?.studentId, members);

  const unitsQuery = useOrganizationUnits(id, Boolean(org) && isAuthenticated);
  const units = unitsQuery.data ?? org?.units ?? [];
  const hierarchyQuery = useOrganizationHierarchy(id, Boolean(org) && tab === "hierarchy");
  const participantsQuery = useOrganizationParticipants(
    id,
    Boolean(org) && isAuthenticated && tab === "participants",
  );
  const myAppsQuery = useMyOrganizationApplications(
    Boolean(org) && isAuthenticated && !canReviewRecruitment,
  );
  const appsQuery = useOrganizationApplications(id, Boolean(org && canReviewRecruitment));
  const mutations = useOrganizationMutations(id);

  if (!isValidId) {
    return (
      <EmptyState
        icon={<Building2 className="size-6" />}
        title="Invalid organization ID"
        description="The requested organization link is not valid."
        action={
          <Button variant="outline" onClick={() => navigate("/organizations")}>
            Back to organizations
          </Button>
        }
      />
    );
  }

  if (orgQuery.isLoading) {
    return <Skeleton className="h-[460px] rounded-3xl" />;
  }

  if (orgQuery.isError || !org) {
    return (
      <div className="space-y-4">
        <ErrorBanner
          error={orgQuery.error}
          message="Organization could not be loaded."
          onRetry={() => void orgQuery.refetch()}
        />
        <Button variant="outline" onClick={() => navigate("/organizations")}>
          Back to organizations
        </Button>
      </div>
    );
  }

  const adminApps = appsQuery.data ?? [];
  const pendingAdminAppsCount = adminApps.filter((a) => a.status === "Pending").length;

  const handleExportApplicationsCsv = async () => {
    try {
      const csv = await organizationsApi.exportApplicationsCsv(id);
      triggerFileDownload(csv, `applications-org-${id}.csv`, "text/csv;charset=utf-8");
      toast.success("Applications CSV exported.");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not export applications CSV.");
    }
  };

  return (
    <div className="space-y-6">
      <Button
        variant="ghost"
        size="sm"
        leftIcon={<ArrowLeft className="size-4" />}
        onClick={() => navigate("/organizations")}
      >
        All organizations
      </Button>

      <section className="rounded-[28px] border border-white/10 bg-[radial-gradient(ellipse_at_90%_0%,rgba(83,152,134,0.32),transparent_48%),#1b1d26] p-6 sm:p-8">
        <div className="flex flex-col gap-5 sm:flex-row sm:items-start">
          <OrganizationMark
            name={org.name}
            logoUrl={org.logoUrl}
            className="size-20 rounded-3xl text-3xl"
          />
          <div className="min-w-0 flex-1">
            <div className="flex flex-wrap items-center gap-2">
              <Badge variant="accent">{org.category}</Badge>
              {org.scope && <Badge variant="outline">{org.scope}</Badge>}
              {org.isRecruiting && (
                <Badge variant="success">
                  <Sparkles className="me-1 size-3" aria-hidden="true" />
                  {org.currentSeasonName ?? "Recruiting Open"}
                </Badge>
              )}
            </div>

            <h1 className="mt-3 text-3xl font-bold text-white sm:text-4xl">{org.name}</h1>
            <p className="mt-2 max-w-3xl text-sm leading-relaxed text-white/70">
              {org.description}
            </p>

            <div className="mt-4 flex flex-wrap gap-4 text-xs text-white/65">
              {(org.universityName || org.city) && (
                <span className="inline-flex items-center gap-1.5 text-[#539886]">
                  <MapPin className="size-3.5" aria-hidden="true" />
                  {[org.universityName, org.city].filter(Boolean).join(" · ")}
                </span>
              )}
              <span className="inline-flex items-center gap-1.5">
                <Users className="size-3.5" aria-hidden="true" />
                {members.length} crew members
              </span>
              <span className="inline-flex items-center gap-1.5">
                <Layers className="size-3.5" aria-hidden="true" />
                {units.length} chapters &amp; committees
              </span>
              <span className="inline-flex items-center gap-1.5">
                <CalendarDays className="size-3.5" aria-hidden="true" />
                {org.events.length} hosted events
              </span>
              {org.websiteUrl && isSafeHttpUrl(org.websiteUrl) && (
                <a
                  href={org.websiteUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-1 text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]"
                >
                  <Globe className="size-3.5" aria-hidden="true" />
                  Official website
                </a>
              )}
            </div>
          </div>

          {canManage && (
            <div className="flex flex-wrap gap-2">
              <Button variant="outline" onClick={() => setEditOpen(true)}>
                Edit organization
              </Button>
              {isPlatformAdmin && (
                <Button variant="danger" onClick={() => setDeleteOpen(true)}>
                  Delete
                </Button>
              )}
            </div>
          )}
        </div>
      </section>

      <div className="rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:p-6">
        <Tabs
          value={tab}
          onValueChange={setTab}
          listClassName="gap-0.5 sm:gap-1"
          tabClassName="px-2.5 sm:px-3 text-[13px]"
          items={[
            {
              id: "overview",
              label: "Overview",
              content: (
                <div className="grid gap-4 pt-2 lg:grid-cols-2">
                  <Card className="space-y-4 p-5">
                    <div>
                      <h2 className="text-base font-semibold text-white">About this community</h2>
                      <p className="mt-2 whitespace-pre-wrap text-sm leading-relaxed text-white/70">
                        {org.description}
                      </p>
                    </div>
                    {units.length > 0 && (
                      <div className="space-y-2 border-t border-white/10 pt-3">
                        <p className="text-xs font-semibold uppercase tracking-wider text-white/55">
                          Active chapters &amp; committees
                        </p>
                        <div className="flex flex-wrap gap-1.5">
                          {units.map((u) => (
                            <Badge key={u.organizationUnitId} variant="outline" size="sm">
                              {u.code} · {u.name}
                            </Badge>
                          ))}
                        </div>
                      </div>
                    )}
                  </Card>

                  <OrganizationApplicationsTab
                    organizationId={id}
                    currentStudentId={user?.studentId}
                    canManage={canManage}
                    isMember={isMember}
                    units={units}
                    myApplications={myAppsQuery.data}
                    adminApplications={adminApps}
                    isLoadingAdminApps={appsQuery.isLoading}
                    adminAppsError={appsQuery.isError ? appsQuery.error : null}
                    isSubmitting={
                      mutations.apply.isPending ||
                      mutations.withdraw.isPending ||
                      mutations.review.isPending
                    }
                    onApply={(message, organizationUnitId) =>
                      mutations.apply.mutateAsync({ message, organizationUnitId })
                    }
                    onWithdraw={(appId) => mutations.withdraw.mutateAsync(appId)}
                    onReview={async (applicationId, status) => {
                      await mutations.review.mutateAsync({ applicationId, status });
                    }}
                  />
                </div>
              ),
            },
            {
              id: "units",
              label: "Chapters & Committees",
              badge: units.length,
              content: (
                <OrganizationUnitsTab
                  units={units}
                  canManage={canManage}
                  isBusy={
                    mutations.createUnit.isPending ||
                    mutations.updateUnit.isPending ||
                    mutations.deleteUnit.isPending
                  }
                  onCreateUnit={(payload) => mutations.createUnit.mutateAsync(payload)}
                  onUpdateUnit={(unitId, payload) =>
                    mutations.updateUnit.mutateAsync({ unitId, payload })
                  }
                  onDeleteUnit={(unitId) => mutations.deleteUnit.mutateAsync(unitId)}
                  onApplyToUnit={() => setTab("overview")}
                />
              ),
            },
            {
              id: "hierarchy",
              label: "Org Hierarchy",
              content: (
                <OrganizationHierarchyTab
                  hierarchy={hierarchyQuery.data}
                  isLoading={hierarchyQuery.isLoading}
                />
              ),
            },
            {
              id: "members",
              label: "Members & Crew",
              badge: members.length,
              content: (
                <OrganizationMembersTab
                  members={members}
                  units={units}
                  currentStudentId={user?.studentId}
                  canManage={canManage}
                  isBusy={
                    mutations.updateRole.isPending || mutations.removeMember.isPending
                  }
                  onUpdateRole={async (studentId, roleOrPayload) => {
                    const payload =
                      typeof roleOrPayload === "string"
                        ? { role: roleOrPayload }
                        : roleOrPayload;
                    await mutations.updateRole.mutateAsync({ studentId, ...payload });
                  }}
                  onRemoveMember={(studentId) =>
                    mutations.removeMember.mutateAsync(studentId)
                  }
                />
              ),
            },
            {
              id: "applications",
              label: "Recruitment Pipeline",
              badge: canReviewRecruitment ? pendingAdminAppsCount : undefined,
              content: (
                <RecruitmentPipelineTab
                  organizationId={id}
                  currentStudentId={user?.studentId}
                  canManage={canManage}
                  canReviewRecruitment={canReviewRecruitment}
                  isMember={isMember}
                  units={units}
                  applications={adminApps}
                  myApplications={myAppsQuery.data}
                  isLoadingApps={appsQuery.isLoading}
                  appsError={appsQuery.isError ? appsQuery.error : null}
                  isBusy={
                    mutations.apply.isPending ||
                    mutations.withdraw.isPending ||
                    mutations.review.isPending ||
                    mutations.updateStage.isPending
                  }
                  onApply={(message, organizationUnitId) =>
                    mutations.apply.mutateAsync({ message, organizationUnitId })
                  }
                  onWithdraw={(appId) => mutations.withdraw.mutateAsync(appId)}
                  onReview={async (applicationId, status) => {
                    await mutations.review.mutateAsync({ applicationId, status });
                  }}
                  onUpdateStage={(applicationId, payload) =>
                    mutations.updateStage.mutateAsync({ applicationId, payload })
                  }
                  onExportCsv={handleExportApplicationsCsv}
                />
              ),
            },
            {
              id: "participants",
              label: "Participants",
              content: (
                <OrganizationParticipantsTab
                  organizationName={org.name}
                  participants={participantsQuery.data ?? []}
                  isLoading={participantsQuery.isLoading}
                  isAuthenticated={isAuthenticated}
                />
              ),
            },
            {
              id: "events",
              label: "Hosted Events",
              badge: org.events.length,
              content:
                org.events.length > 0 ? (
                  <div className="grid gap-3 pt-2 sm:grid-cols-2">
                    {org.events.map((ev) => (
                      <Link
                        key={ev.eventId}
                        to={`/events/${ev.eventId}`}
                        className="rounded-2xl border border-white/10 bg-[#14151c] p-4 transition-colors hover:border-[#d3fb52]/35"
                      >
                        <div className="flex items-center justify-between gap-2">
                          <p className="font-semibold text-white">{ev.title}</p>
                          {ev.deliveryMode && (
                            <Badge variant="outline" size="sm">
                              {ev.deliveryMode}
                            </Badge>
                          )}
                        </div>
                        <p className="mt-1 text-xs text-white/55">
                          {ev.type} · {new Date(ev.startDate).toLocaleDateString()} · {ev.location}
                        </p>
                      </Link>
                    ))}
                  </div>
                ) : (
                  <EmptyState
                    icon={<CalendarDays className="size-6" />}
                    title="No hosted events yet"
                    description="Upcoming workshops and competitions from this chapter will appear here."
                  />
                ),
            },
          ]}
        />
      </div>

      <OrganizationFormModal
        open={editOpen}
        onClose={() => setEditOpen(false)}
        initial={{
          name: org.name,
          description: org.description,
          category: org.category,
          logoUrl: org.logoUrl,
          scope: org.scope,
          universityId: org.universityId,
          city: org.city,
          websiteUrl: org.websiteUrl,
          isRecruiting: org.isRecruiting,
          currentSeasonName: org.currentSeasonName,
        }}
        isSubmitting={mutations.update.isPending}
        onSubmitPayload={async (payload) => {
          await mutations.update.mutateAsync(payload);
          setEditOpen(false);
          toast.success("Organization updated.");
        }}
      />

      <ConfirmDialog
        open={deleteOpen}
        onClose={() => setDeleteOpen(false)}
        isLoading={mutations.remove.isPending}
        title="Delete organization?"
        description={`This will permanently remove "${org.name}", its memberships, and hosted events.`}
        confirmLabel="Delete organization"
        confirmTextMatch={org.name}
        confirmTextLabel={`Type "${org.name}" to confirm deletion`}
        onConfirm={async () => {
          try {
            await mutations.remove.mutateAsync();
            setDeleteOpen(false);
            toast.success("Organization deleted.");
            navigate("/organizations");
          } catch (error) {
            toast.error(error instanceof Error ? error.message : "Could not delete organization.");
          }
        }}
      />
    </div>
  );
}
