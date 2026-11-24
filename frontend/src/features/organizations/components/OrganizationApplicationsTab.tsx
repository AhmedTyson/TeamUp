/**
 * @date 2026-11-24
 * @purpose Membership applications tab synced cross-device with backend /my-applications, supporting chapter selection, guest sign-in prompt, and OrgAdmin quick review.
 */
import { zodResolver } from "@hookform/resolvers/zod";
import { CheckCircle2, ClipboardCheck, LogIn, Send } from "lucide-react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useLocation } from "react-router-dom";
import { toast } from "sonner";
import { ApiClientError, type MembershipApplication, type OrganizationUnit } from "@/api";
import {
  Badge,
  Button,
  Card,
  ConfirmDialog,
  EmptyState,
  ErrorBanner,
  Select,
  Skeleton,
  StatusPill,
  Textarea,
} from "@/components/ui";
import {
  membershipApplicationSchema,
  type MembershipApplicationValues,
} from "../schemas/organizationSchemas";
import { myOrgApplicationStorage, type SavedOrgApplication } from "../utils/authorization";

export interface OrganizationApplicationsTabProps {
  organizationId: number;
  currentStudentId?: number;
  canManage: boolean;
  isMember: boolean;
  units?: readonly OrganizationUnit[];
  myApplications?: readonly MembershipApplication[];
  adminApplications: readonly MembershipApplication[];
  isLoadingAdminApps: boolean;
  adminAppsError: unknown;
  isSubmitting: boolean;
  onApply: (message: string | null, organizationUnitId?: number | null) => Promise<MembershipApplication>;
  onWithdraw: (applicationId: number) => Promise<void>;
  onReview: (applicationId: number, status: "Accepted" | "Rejected") => Promise<void>;
}

export function OrganizationApplicationsTab({
  organizationId,
  currentStudentId,
  canManage,
  isMember,
  units = [],
  myApplications,
  adminApplications,
  isLoadingAdminApps,
  adminAppsError,
  isSubmitting,
  onApply,
  onWithdraw,
  onReview,
}: OrganizationApplicationsTabProps) {
  const location = useLocation();
  const [savedApp, setSavedApp] = useState<SavedOrgApplication | null>(() =>
    myOrgApplicationStorage.get(currentStudentId, organizationId),
  );
  const [conflictNote, setConflictNote] = useState<string | null>(null);
  const [withdrawConfirmOpen, setWithdrawConfirmOpen] = useState(false);

  const backendPendingApp = myApplications?.find(
    (app) => app.organizationId === organizationId && app.status === "Pending",
  );

  useEffect(() => {
    if (backendPendingApp) {
      const synced: SavedOrgApplication = {
        applicationId: backendPendingApp.applicationId,
        message: backendPendingApp.message,
        appliedAt: backendPendingApp.appliedAt,
      };
      myOrgApplicationStorage.set(currentStudentId, organizationId, synced);
      setSavedApp(synced);
    } else if (myApplications !== undefined) {
      myOrgApplicationStorage.clear(currentStudentId, organizationId);
      setSavedApp(null);
    }
  }, [backendPendingApp, myApplications, currentStudentId, organizationId]);

  const form = useForm<MembershipApplicationValues>({
    resolver: zodResolver(membershipApplicationSchema),
    defaultValues: { organizationUnitId: "", message: "" },
  });

  if (canManage) {
    if (isLoadingAdminApps) {
      return <Skeleton className="h-44 rounded-2xl" />;
    }
    if (adminAppsError) {
      return <ErrorBanner error={adminAppsError} />;
    }
    if (adminApplications.length === 0) {
      return (
        <EmptyState
          icon={<ClipboardCheck className="size-6" />}
          title="No membership applications"
          description="Incoming student requests to join this organization will appear here for review."
        />
      );
    }

    return (
      <div className="space-y-3 pt-2">
        {adminApplications.map((app) => (
          <div
            key={app.applicationId}
            className="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-white/10 bg-[#14151c] p-4"
          >
            <div className="min-w-0 space-y-1">
              <div className="flex flex-wrap items-center gap-2">
                <p className="font-semibold text-white">
                  {app.studentFirstName} {app.studentLastName}
                </p>
                {app.unitCode && (
                  <Badge variant="outline" size="sm">
                    {app.unitCode}
                  </Badge>
                )}
              </div>
              <p className="text-xs text-white/50">
                {app.studentEmail} · Applied {new Date(app.appliedAt).toLocaleDateString()}
              </p>
              {app.message && <p className="pt-1 text-sm text-white/75">{app.message}</p>}
            </div>

            <div className="flex flex-wrap items-center gap-2">
              <StatusPill status={app.status} />
              {app.status === "Pending" && (
                <>
                  <Button
                    size="sm"
                    disabled={isSubmitting}
                    onClick={async () => {
                      try {
                        await onReview(app.applicationId, "Accepted");
                        toast.success("Application accepted.");
                      } catch (err) {
                        toast.error(err instanceof Error ? err.message : "Could not accept application.");
                      }
                    }}
                  >
                    Accept
                  </Button>
                  <Button
                    size="sm"
                    variant="danger"
                    disabled={isSubmitting}
                    onClick={async () => {
                      try {
                        await onReview(app.applicationId, "Rejected");
                        toast.success("Application rejected.");
                      } catch (err) {
                        toast.error(err instanceof Error ? err.message : "Could not reject application.");
                      }
                    }}
                  >
                    Reject
                  </Button>
                </>
              )}
            </div>
          </div>
        ))}
      </div>
    );
  }

  if (!currentStudentId) {
    return (
      <Card className="space-y-4 p-5">
        <div className="space-y-1">
          <h3 className="text-base font-semibold text-white">Join this community</h3>
          <p className="text-sm text-white/65">
            Sign in with your TeamUp account to apply for chapters, workshops, and crew roles across
            any device.
          </p>
        </div>
        <Link
          to={`/auth/login?redirect=${encodeURIComponent(location.pathname)}`}
          className="inline-flex h-10 items-center justify-center gap-2 rounded-xl bg-[#d3fb52] px-4 text-sm font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
        >
          <LogIn className="size-4" aria-hidden="true" />
          Sign in to apply
        </Link>
      </Card>
    );
  }

  if (isMember) {
    return (
      <Card className="border-emerald-500/25 bg-emerald-500/[0.06] p-5">
        <div className="flex items-start gap-3">
          <CheckCircle2 className="mt-0.5 size-5 shrink-0 text-emerald-400" aria-hidden="true" />
          <div className="space-y-1">
            <h3 className="text-base font-semibold text-white">Active organization member</h3>
            <p className="text-sm text-white/70">
              You are already an active member of this organization. Use the Members tab if you wish
              to leave this organization.
            </p>
          </div>
        </div>
      </Card>
    );
  }

  const activeApp = backendPendingApp
    ? {
        applicationId: backendPendingApp.applicationId,
        message: backendPendingApp.message,
        appliedAt: backendPendingApp.appliedAt,
        stage: backendPendingApp.stage,
        unitName: backendPendingApp.unitName,
      }
    : savedApp
      ? {
          ...savedApp,
          stage: undefined,
          unitName: undefined,
        }
      : null;

  const handleSubmitApplication = form.handleSubmit(async (values) => {
    setConflictNote(null);
    const trimmed = values.message?.trim() || null;
    const parsedUnit = values.organizationUnitId
      ? Number.parseInt(values.organizationUnitId, 10)
      : NaN;
    const unitId = Number.isInteger(parsedUnit) && parsedUnit > 0 ? parsedUnit : null;

    try {
      const created = await onApply(trimmed, unitId);
      const nextSaved: SavedOrgApplication = {
        applicationId: created.applicationId,
        message: created.message,
        appliedAt: created.appliedAt,
      };
      myOrgApplicationStorage.set(currentStudentId, organizationId, nextSaved);
      setSavedApp(nextSaved);
      form.reset({ organizationUnitId: "", message: "" });
      toast.success("Membership application submitted.");
    } catch (error) {
      if (error instanceof ApiClientError && error.isConflict) {
        setConflictNote(error.message);
        toast.info(error.message);
      } else {
        toast.error(error instanceof Error ? error.message : "Failed to submit application.");
      }
    }
  });

  if (activeApp) {
    return (
      <Card className="space-y-4 p-5">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="space-y-1">
            <div className="flex flex-wrap items-center gap-2">
              <h3 className="text-base font-semibold text-white">Your membership application</h3>
              <StatusPill status="Pending" label={activeApp.stage ?? "Pending"} />
            </div>
            <p className="text-xs text-white/55">
              Submitted {new Date(activeApp.appliedAt).toLocaleString()}
              {activeApp.unitName ? ` · Target: ${activeApp.unitName}` : ""}
            </p>
          </div>
          <Button
            size="sm"
            variant="danger"
            disabled={isSubmitting}
            onClick={() => setWithdrawConfirmOpen(true)}
          >
            Withdraw application
          </Button>
        </div>
        {activeApp.message && (
          <p className="rounded-xl border border-white/10 bg-[#14151c] p-3 text-sm text-white/80">
            {activeApp.message}
          </p>
        )}

        <ConfirmDialog
          open={withdrawConfirmOpen}
          onClose={() => setWithdrawConfirmOpen(false)}
          isLoading={isSubmitting}
          title="Withdraw application?"
          description="Your pending request will be cancelled. You can reapply later if openings remain."
          confirmLabel="Withdraw application"
          onConfirm={async () => {
            try {
              await onWithdraw(activeApp.applicationId);
              myOrgApplicationStorage.clear(currentStudentId, organizationId);
              setSavedApp(null);
              setWithdrawConfirmOpen(false);
              toast.success("Application withdrawn.");
            } catch (error) {
              if (error instanceof ApiClientError && (error.statusCode === 404 || error.isConflict)) {
                myOrgApplicationStorage.clear(currentStudentId, organizationId);
                setSavedApp(null);
                setWithdrawConfirmOpen(false);
              }
              toast.error(error instanceof Error ? error.message : "Could not withdraw application.");
            }
          }}
        />
      </Card>
    );
  }

  const recruitingUnits = units.filter((u) => u.isRecruiting);

  return (
    <Card className="space-y-4 p-5">
      <div className="space-y-1">
        <h3 className="text-base font-semibold text-white">Apply for membership</h3>
        <p className="text-sm text-white/65">
          Introduce yourself to chapter organizers to join upcoming teams, technical labs, and
          recruitment interviews.
        </p>
      </div>

      {conflictNote && (
        <ErrorBanner variant="warning" title="Application already active" message={conflictNote} />
      )}

      <form className="space-y-4" onSubmit={handleSubmitApplication} noValidate>
        {recruitingUnits.length > 0 && (
          <Select
            label="Preferred chapter or committee"
            placeholder="General / Organization-wide"
            options={recruitingUnits.map((u) => ({
              value: String(u.organizationUnitId),
              label: `${u.code} · ${u.name}${u.openSeats !== null ? ` (${u.openSeats} seats)` : ""}`,
            }))}
            {...form.register("organizationUnitId")}
          />
        )}
        <Textarea
          label="Application message"
          rows={3}
          maxLength={600}
          placeholder="Tell organizers about your skills, portfolio, or interests (optional)."
          error={form.formState.errors.message?.message}
          {...form.register("message")}
        />
        <Button type="submit" isLoading={isSubmitting} leftIcon={<Send className="size-4" />}>
          Apply to join
        </Button>
      </form>
    </Card>
  );
}
