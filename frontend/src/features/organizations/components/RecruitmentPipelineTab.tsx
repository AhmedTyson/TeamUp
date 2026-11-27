/**
 * @date 2026-11-24
 * @purpose Recruitment Season Kanban pipeline tab supporting 4-stage flow (Applied -> HR Screening -> Committee Interview -> Accepted), evaluation notes, delegated HR/Head access, and CSV export.
 */
import { ArrowRight, Download, Filter, KanbanSquare, Star } from "lucide-react";
import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import {
  RECRUITMENT_STAGES,
  type MembershipApplication,
  type OrganizationUnit,
  type RecruitmentStage,
  type UpdateApplicationStagePayload,
} from "@/api";
import {
  Badge,
  Button,
  EmptyState,
  ErrorBanner,
  Input,
  Modal,
  Select,
  Skeleton,
  StatusPill,
  Textarea,
} from "@/components/ui";
import { OrganizationApplicationsTab } from "./OrganizationApplicationsTab";

export interface RecruitmentPipelineTabProps {
  organizationId: number;
  currentStudentId?: number;
  canManage: boolean;
  canReviewRecruitment: boolean;
  isMember: boolean;
  units: readonly OrganizationUnit[];
  applications: readonly MembershipApplication[];
  myApplications?: readonly MembershipApplication[];
  isLoadingApps: boolean;
  appsError: unknown;
  isBusy: boolean;
  onApply: (message: string | null, organizationUnitId?: number | null) => Promise<MembershipApplication>;
  onWithdraw: (applicationId: number) => Promise<void>;
  onReview: (applicationId: number, status: "Accepted" | "Rejected") => Promise<void>;
  onUpdateStage: (
    applicationId: number,
    payload: UpdateApplicationStagePayload,
  ) => Promise<MembershipApplication>;
  onExportCsv: () => Promise<void>;
}

const KANBAN_COLUMNS: readonly { stage: RecruitmentStage; label: string; next?: RecruitmentStage }[] = [
  { stage: RECRUITMENT_STAGES.Applied, label: "1. Applied", next: RECRUITMENT_STAGES.HrScreening },
  {
    stage: RECRUITMENT_STAGES.HrScreening,
    label: "2. HR Screening",
    next: RECRUITMENT_STAGES.CommitteeInterview,
  },
  {
    stage: RECRUITMENT_STAGES.CommitteeInterview,
    label: "3. Committee Interview",
    next: RECRUITMENT_STAGES.Accepted,
  },
  { stage: RECRUITMENT_STAGES.Accepted, label: "4. Accepted" },
];

export function RecruitmentPipelineTab({
  organizationId,
  currentStudentId,
  canManage,
  canReviewRecruitment,
  isMember,
  units,
  applications,
  myApplications,
  isLoadingApps,
  appsError,
  isBusy,
  onApply,
  onWithdraw,
  onReview,
  onUpdateStage,
  onExportCsv,
}: RecruitmentPipelineTabProps) {
  const [unitFilter, setUnitFilter] = useState("");
  const [inspectApp, setInspectApp] = useState<MembershipApplication | null>(null);
  const [scoreDraft, setScoreDraft] = useState("");
  const [notesDraft, setNotesDraft] = useState("");
  const [unitDraft, setUnitDraft] = useState("");
  const [stageDraft, setStageDraft] = useState<RecruitmentStage>(RECRUITMENT_STAGES.Applied);

  const unitOptions = useMemo(
    () =>
      units.map((u) => ({
        value: String(u.organizationUnitId),
        label: `${u.code} · ${u.name}`,
      })),
    [units],
  );

  const filteredApps = useMemo(() => {
    if (!unitFilter) {
      return applications;
    }
    const id = Number(unitFilter);
    return applications.filter((a) => a.organizationUnitId === id);
  }, [applications, unitFilter]);

  if (!canReviewRecruitment) {
    return (
      <OrganizationApplicationsTab
        organizationId={organizationId}
        currentStudentId={currentStudentId}
        canManage={false}
        isMember={isMember}
        units={units}
        myApplications={myApplications}
        adminApplications={[]}
        isLoadingAdminApps={false}
        adminAppsError={null}
        isSubmitting={isBusy}
        onApply={onApply}
        onWithdraw={onWithdraw}
        onReview={onReview}
      />
    );
  }

  if (isLoadingApps) {
    return <Skeleton className="h-64 rounded-2xl" />;
  }

  if (appsError) {
    return <ErrorBanner error={appsError} />;
  }

  const openInspector = (app: MembershipApplication) => {
    setInspectApp(app);
    setScoreDraft(app.evaluationScore !== null && app.evaluationScore !== undefined ? String(app.evaluationScore) : "");
    setNotesDraft(app.interviewNotes ?? "");
    setUnitDraft(app.organizationUnitId ? String(app.organizationUnitId) : "");
    setStageDraft(app.stage ?? RECRUITMENT_STAGES.Applied);
  };

  const handleSaveInspection = async () => {
    if (!inspectApp) {
      return;
    }
    try {
      const numericScore = scoreDraft.trim() ? Number(scoreDraft.trim()) : null;
      await onUpdateStage(inspectApp.applicationId, {
        stage: stageDraft,
        evaluationScore:
          numericScore !== null && Number.isFinite(numericScore) ? numericScore : null,
        interviewNotes: notesDraft.trim() || null,
        organizationUnitId: unitDraft ? Number(unitDraft) : null,
      });
      toast.success(`Moved ${inspectApp.studentFirstName} to ${stageDraft}.`);
      setInspectApp(null);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not update recruitment stage.");
    }
  };

  const rejectedApps = filteredApps.filter(
    (a) => a.stage === RECRUITMENT_STAGES.Rejected || a.status === "Rejected",
  );

  return (
    <div className="space-y-5 pt-2">
      <div className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-white/10 bg-[#14151c] p-4">
        <div>
          <div className="flex items-center gap-2">
            <KanbanSquare className="size-4 text-[#d3fb52]" aria-hidden="true" />
            <h2 className="text-base font-bold text-white">Seasonal Recruitment Pipeline</h2>
            <Badge variant="accent" size="sm">
              {filteredApps.length} candidates
            </Badge>
          </div>
          <p className="mt-0.5 text-xs text-white/55">
            {canManage
              ? "High Board view across all chapters & committees."
              : "Delegated HR / Committee Head screening & interview console."}
          </p>
        </div>

        <div className="flex w-full flex-wrap items-center gap-2 sm:w-auto">
          <Filter className="size-3.5 text-white/50" aria-hidden="true" />
          <Select
            aria-label="Filter candidates by unit"
            placeholder="All chapters & committees"
            options={unitOptions}
            value={unitFilter}
            onChange={(e) => setUnitFilter(e.target.value)}
            containerClassName="min-w-0 flex-1 sm:w-auto sm:flex-initial"
            className="h-9 min-h-[36px] w-full bg-[#1b1d26] py-1.5 text-xs sm:min-h-[36px] sm:min-w-[190px]"
          />
          <Button
            size="sm"
            variant="outline"
            leftIcon={<Download className="size-3.5" />}
            disabled={isBusy}
            onClick={() => void onExportCsv()}
          >
            Export CSV
          </Button>
        </div>
      </div>

      {filteredApps.length === 0 ? (
        <EmptyState
          icon={<KanbanSquare className="size-6" />}
          title="No recruitment candidates in pipeline"
          description="Incoming student applications for this season will populate the 4 Kanban stages."
        />
      ) : (
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {KANBAN_COLUMNS.map((col) => {
            const colApps = filteredApps.filter((a) => {
              const effectiveStage =
                a.stage ??
                (a.status === "Accepted"
                  ? RECRUITMENT_STAGES.Accepted
                  : a.status === "Rejected"
                    ? RECRUITMENT_STAGES.Rejected
                    : RECRUITMENT_STAGES.Applied);
              return effectiveStage === col.stage;
            });

            return (
              <div
                key={col.stage}
                className="flex flex-col gap-3 rounded-2xl border border-white/10 bg-[#14151c] p-3.5"
              >
                <div className="flex items-center justify-between border-b border-white/10 pb-2">
                  <span className="text-xs font-bold uppercase tracking-wider text-white">
                    {col.label}
                  </span>
                  <Badge variant={col.stage === "Accepted" ? "success" : "default"} size="sm">
                    {colApps.length}
                  </Badge>
                </div>

                <div className="space-y-2.5">
                  {colApps.map((app) => (
                    <div
                      key={app.applicationId}
                      className="space-y-2 rounded-xl border border-white/10 bg-[#1b1d26] p-3 transition-colors hover:border-[#d3fb52]/35"
                    >
                      <div className="flex items-start justify-between gap-2">
                        <div className="min-w-0">
                          <Link
                            to={`/students/${app.studentId}`}
                            className="block truncate text-sm font-semibold text-white transition-colors hover:text-[#d3fb52]"
                          >
                            {app.studentFirstName} {app.studentLastName}
                          </Link>
                          <p className="truncate text-[11px] text-white/50">{app.studentEmail}</p>
                        </div>
                        {app.unitCode && (
                          <span className="shrink-0 rounded bg-[#d3fb52]/15 px-1.5 py-0.5 font-mono text-[10px] font-bold text-[#d3fb52]">
                            {app.unitCode}
                          </span>
                        )}
                      </div>

                      {app.message && (
                        <p className="line-clamp-2 text-xs text-white/70">{app.message}</p>
                      )}

                      <div className="flex flex-wrap items-center justify-between gap-2 pt-1 text-[11px] text-white/55">
                        <span>
                          {app.evaluationScore !== null && app.evaluationScore !== undefined ? (
                            <span className="inline-flex items-center gap-1 font-mono font-bold text-[#d3fb52]">
                              <Star className="size-3" aria-hidden="true" />
                              {app.evaluationScore}/10
                            </span>
                          ) : (
                            new Date(app.appliedAt).toLocaleDateString()
                          )}
                        </span>

                        <div className="flex items-center gap-1.5">
                          {app.status === "Pending" && (
                            <Button
                              size="sm"
                              variant="ghost"
                              disabled={isBusy}
                              onClick={() => openInspector(app)}
                            >
                              Evaluate
                            </Button>
                          )}
                          {col.next && app.status === "Pending" && (
                            <Button
                              size="sm"
                              disabled={isBusy}
                              rightIcon={<ArrowRight className="size-3" />}
                              onClick={async () => {
                                try {
                                  await onUpdateStage(app.applicationId, { stage: col.next! });
                                  toast.success(
                                    `${app.studentFirstName} moved to ${col.next}.`,
                                  );
                                } catch (error) {
                                  toast.error(
                                    error instanceof Error
                                      ? error.message
                                      : "Could not advance stage.",
                                  );
                                }
                              }}
                            >
                              Advance
                            </Button>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                  {colApps.length === 0 && (
                    <p className="py-6 text-center text-xs text-white/35">Empty stage</p>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {rejectedApps.length > 0 && (
        <details className="rounded-2xl border border-white/10 bg-[#14151c] p-4">
          <summary className="cursor-pointer text-xs font-semibold text-white/70">
            Archived / Rejected candidates ({rejectedApps.length})
          </summary>
          <div className="mt-3 grid gap-2 sm:grid-cols-2">
            {rejectedApps.map((app) => (
              <div
                key={app.applicationId}
                className="flex items-center justify-between rounded-xl border border-white/10 bg-[#1b1d26] px-3 py-2 text-xs"
              >
                <Link
                  to={`/students/${app.studentId}`}
                  className="font-medium text-white/85 transition-colors hover:text-[#d3fb52]"
                >
                  {app.studentFirstName} {app.studentLastName}
                </Link>
                <StatusPill status="Rejected" />
              </div>
            ))}
          </div>
        </details>
      )}

      <Modal
        open={inspectApp !== null}
        onClose={() => setInspectApp(null)}
        title={
          inspectApp
            ? `Evaluate ${inspectApp.studentFirstName} ${inspectApp.studentLastName}`
            : "Evaluate candidate"
        }
        description="Assign target chapter, stage, interview rubric score (0–10), and HR notes."
      >
        <div className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <Select
              label="Pipeline stage"
              value={stageDraft}
              onChange={(e) => setStageDraft(e.target.value as RecruitmentStage)}
              options={[
                { value: RECRUITMENT_STAGES.Applied, label: "Applied" },
                { value: RECRUITMENT_STAGES.HrScreening, label: "HR Screening" },
                { value: RECRUITMENT_STAGES.CommitteeInterview, label: "Committee Interview" },
                { value: RECRUITMENT_STAGES.Accepted, label: "Accepted (Onboard Member)" },
                { value: RECRUITMENT_STAGES.Rejected, label: "Rejected" },
              ]}
            />
            <Select
              label="Assigned chapter / committee"
              placeholder="General membership"
              value={unitDraft}
              onChange={(e) => setUnitDraft(e.target.value)}
              options={unitOptions}
            />
          </div>

          <Input
            label="Evaluation score (0 - 10)"
            type="number"
            step="0.5"
            min={0}
            max={10}
            value={scoreDraft}
            onChange={(e) => setScoreDraft(e.target.value)}
            placeholder="e.g. 8.5"
          />

          <Textarea
            label="HR & Interview notes"
            rows={3}
            maxLength={1000}
            value={notesDraft}
            onChange={(e) => setNotesDraft(e.target.value)}
            placeholder="Summarize technical fit, communication, and recommended crew role..."
          />

          <div className="flex justify-end gap-2 border-t border-white/10 pt-4">
            <Button type="button" variant="outline" onClick={() => setInspectApp(null)}>
              Cancel
            </Button>
            <Button type="button" isLoading={isBusy} onClick={() => void handleSaveInspection()}>
              Save evaluation
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
