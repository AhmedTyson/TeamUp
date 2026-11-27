/**
 * @date 2026-11-24
 * @purpose Luma-inspired Event Detail workspace featuring cross-device registration sync, paper-white Digital Pass, .ics calendar download, DeliveryMode pills, sessions timetable, and scoped organizer console.
 */
import { ArrowLeft, CalendarDays, CalendarPlus, LogIn, MapPin, QrCode } from "lucide-react";
import { useEffect, useState } from "react";
import { Link, useLocation, useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import { ApiClientError, type RegistrationStatus } from "@/api";
import {
  Badge,
  Button,
  ConfirmDialog,
  EmptyState,
  ErrorBanner,
  Skeleton,
  StatusPill,
} from "@/components/ui";
import { useAuth } from "@/features/auth/AuthContext";
import {
  useOrganizations,
} from "@/features/organizations";
import { downloadEventIcs } from "@/lib/calendarExport";
import { EventDigitalPassCard } from "../components/EventDigitalPassCard";
import { EventFormModal } from "../components/EventFormModal";
import { EventRegistrationsPanel } from "../components/EventRegistrationsPanel";
import { EventSessionsPanel } from "../components/EventSessionsPanel";
import { SessionFormModal } from "../components/SessionFormModal";
import {
  useEventMutations,
  useEventWorkspace,
} from "../hooks/useEventHooks";
import { myEventRegistrationStorage } from "../utils/attendance";

export function EventDetailPage() {
  const { id: rawId } = useParams();
  const eventId = Number(rawId);
  const navigate = useNavigate();
  const location = useLocation();
  const { user, isAuthenticated, isPlatformAdmin, orgAdminIds } = useAuth();

  const isValidId = Number.isInteger(eventId) && eventId > 0;
  const workspace = useEventWorkspace(eventId);
  const event = workspace.event;


  const canManage = workspace.canManage;

  const mutations = useEventMutations(eventId);

  const [editEventOpen, setEditEventOpen] = useState(false);
  const [deleteEventOpen, setDeleteEventOpen] = useState(false);
  const [sessionModalOpen, setSessionModalOpen] = useState(false);
  const [editingSessionId, setEditingSessionId] = useState<number | null>(null);
  const [deleteSessionTarget, setDeleteSessionTarget] = useState<number | null>(null);
  const [cancelTargetStudentId, setCancelTargetStudentId] = useState<number | null | "self">(null);

  // Only fetch the organization list when the organizer actually opens the edit modal;
  // previously every visitor (including anonymous students) pulled 100 organizations per page view.
  const orgsQuery = useOrganizations({ page: 1, pageSize: 100 }, canManage && editEventOpen);

  const [savedStudentStatus, setSavedStudentStatus] = useState<RegistrationStatus | null>(() =>
    myEventRegistrationStorage.get(user?.studentId, eventId)?.status ?? null,
  );

  const backendMyReg = workspace.myRegistration ?? null;

  useEffect(() => {
    if (backendMyReg) {
      myEventRegistrationStorage.set(user?.studentId, eventId, {
        status: backendMyReg.status,
        registeredAt: backendMyReg.registeredAt,
      });
      setSavedStudentStatus(backendMyReg.status);
    } else if (workspace.myRegistration !== undefined) {
      myEventRegistrationStorage.clear(user?.studentId, eventId);
      setSavedStudentStatus(null);
    } else {
      setSavedStudentStatus(
        myEventRegistrationStorage.get(user?.studentId, eventId)?.status ?? null,
      );
    }
  }, [backendMyReg, user?.studentId, eventId]);

  if (!isValidId) {
    return (
      <EmptyState
        icon={<CalendarDays className="size-6" />}
        title="Invalid event ID"
        description="The requested event URL parameter is not valid."
        action={
          <Button variant="outline" onClick={() => navigate("/events")}>
            Back to events
          </Button>
        }
      />
    );
  }

  if (workspace.isLoading) {
    return <Skeleton className="h-[460px] rounded-3xl" />;
  }

  if (workspace.isError || !event) {
    return (
      <div className="space-y-4">
        <ErrorBanner
          error={workspace.error}
          message="Event could not be loaded."
          onRetry={() => void workspace.refetch()}
        />
        <Button variant="outline" onClick={() => navigate("/events")}>
          Back to events
        </Button>
      </div>
    );
  }

  const sessions = event.sessions;
  const registrations = workspace.registrations ?? [];
  const adminOwnReg = canManage
    ? registrations.find((reg) => reg.studentId === user?.studentId)
    : undefined;
  const effectiveMyStatus = backendMyReg?.status ?? adminOwnReg?.status ?? savedStudentStatus;
  const isFull = event.registeredCount >= event.capacity;
  const capacityPct = Math.min(
    100,
    Math.round((event.registeredCount / Math.max(1, event.capacity)) * 100),
  );

  const editingSession =
    editingSessionId !== null
      ? sessions.find((s) => s.sessionId === editingSessionId)
      : undefined;

  const handleRegister = async () => {
    try {
      const created = await mutations.register.mutateAsync();
      myEventRegistrationStorage.set(user?.studentId, eventId, {
        status: created.status,
        registeredAt: created.registeredAt,
      });
      setSavedStudentStatus(created.status);
      toast.success(
        created.status === "Waitlisted"
          ? "Event is at capacity. You have been added to the waitlist."
          : "Event registration confirmed!",
      );
    } catch (error) {
      if (error instanceof ApiClientError && error.isConflict) {
        const inferredStatus: RegistrationStatus =
          event.registeredCount >= event.capacity ? "Waitlisted" : "Confirmed";
        myEventRegistrationStorage.set(user?.studentId, eventId, {
          status: inferredStatus,
          registeredAt: new Date().toISOString(),
        });
        setSavedStudentStatus(inferredStatus);
        toast.info(error.message);
      } else {
        toast.error(error instanceof Error ? error.message : "Could not complete registration.");
      }
    }
  };

  const handleConfirmCancelRegistration = async () => {
    const targetId = cancelTargetStudentId === "self" ? undefined : cancelTargetStudentId ?? undefined;
    try {
      await mutations.cancelRegistration.mutateAsync(targetId);
      if (cancelTargetStudentId === "self" || targetId === user?.studentId) {
        myEventRegistrationStorage.clear(user?.studentId, eventId);
        setSavedStudentStatus(null);
      }
      setCancelTargetStudentId(null);
      toast.success("Registration cancelled.");
    } catch (error) {
      if (
        error instanceof ApiClientError &&
        (error.statusCode === 404 || error.isConflict) &&
        cancelTargetStudentId === "self"
      ) {
        myEventRegistrationStorage.clear(user?.studentId, eventId);
        setSavedStudentStatus(null);
        setCancelTargetStudentId(null);
      }
      toast.error(error instanceof Error ? error.message : "Could not cancel registration.");
    }
  };

  return (
    <div className="space-y-6">
      <Button
        variant="ghost"
        size="sm"
        leftIcon={<ArrowLeft className="size-4" />}
        onClick={() => navigate("/events")}
      >
        All events
      </Button>

      <section className="rounded-[28px] border border-white/10 bg-[radial-gradient(ellipse_at_90%_0%,rgba(83,152,134,0.32),transparent_48%),#1b1d26] p-6 sm:p-8">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="max-w-3xl space-y-3">
            <div className="flex flex-wrap items-center gap-2">
              <StatusPill status={event.type} />
              {event.deliveryMode && <Badge variant="outline">{event.deliveryMode}</Badge>}
              <Link
                to={`/organizations/${event.organizationId}`}
                className="rounded-full border border-white/15 px-3 py-1 text-xs font-medium text-white/80 hover:border-[#d3fb52]/40 hover:text-white"
              >
                {event.organizationName}
              </Link>
              {effectiveMyStatus && <StatusPill status={effectiveMyStatus} />}
            </div>
            <h1 className="text-3xl font-bold text-white sm:text-4xl">{event.title}</h1>
            <p className="text-sm leading-relaxed text-white/70">{event.description}</p>
            <div className="flex flex-wrap gap-4 text-xs text-white/60">
              <span className="inline-flex items-center gap-1.5">
                <CalendarDays className="size-3.5" aria-hidden="true" />
                {new Date(event.startDate).toLocaleString()} – {new Date(event.endDate).toLocaleString()}
              </span>
              <span className="inline-flex items-center gap-1.5">
                <MapPin className="size-3.5" aria-hidden="true" />
                {event.location}
              </span>
            </div>
          </div>

          {canManage && (
            <div className="flex flex-wrap gap-2">
              <Button variant="outline" onClick={() => setEditEventOpen(true)}>
                Edit event
              </Button>
              <Button variant="danger" onClick={() => setDeleteEventOpen(true)}>
                Delete event
              </Button>
            </div>
          )}
        </div>

        <div className="mt-6 space-y-4">
          <div className="space-y-1.5">
            <div className="flex justify-between text-xs text-white/65">
              <span>Seat capacity</span>
              <span>
                {event.registeredCount} / {event.capacity} registered
              </span>
            </div>
            <div className="h-2.5 overflow-hidden rounded-full bg-white/10">
              <div
                className="h-full rounded-full bg-[#d3fb52] transition-all"
                style={{ width: `${capacityPct}%` }}
              />
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            {!isAuthenticated ? (
              <Link
                to={`/auth/login?redirect=${encodeURIComponent(location.pathname)}`}
                className="inline-flex h-10 items-center gap-2 rounded-xl bg-[#d3fb52] px-4 text-sm font-bold text-[#14151c] hover:bg-[#deff6e]"
              >
                <LogIn className="size-4" aria-hidden="true" />
                Sign in to register
              </Link>
            ) : effectiveMyStatus ? (
              <Button
                variant="danger"
                isLoading={mutations.cancelRegistration.isPending}
                onClick={() => setCancelTargetStudentId("self")}
              >
                Cancel registration
              </Button>
            ) : (
              <Button isLoading={mutations.register.isPending} onClick={() => void handleRegister()}>
                {isFull ? "Join waitlist" : "Register for event"}
              </Button>
            )}

            <Button
              type="button"
              variant="outline"
              leftIcon={<CalendarPlus className="size-4" />}
              onClick={() => downloadEventIcs(event)}
            >
              Add to Calendar (.ics)
            </Button>

            {isAuthenticated && (
              <Link
                to="/check-in"
                className="inline-flex h-10 items-center gap-2 rounded-xl border border-white/15 px-4 text-sm font-medium text-white/85 transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
              >
                <QrCode className="size-4" aria-hidden="true" />
                Self check-in
              </Link>
            )}
          </div>
        </div>
      </section>

      <div className="grid gap-6 lg:grid-cols-2">
        <EventSessionsPanel
          eventId={eventId}
          sessions={sessions}
          canManage={canManage}
          onAddSession={() => {
            setEditingSessionId(null);
            setSessionModalOpen(true);
          }}
          onEditSession={(sessionId) => {
            setEditingSessionId(sessionId);
            setSessionModalOpen(true);
          }}
          onDeleteSession={(sessionId) => setDeleteSessionTarget(sessionId)}
        />

        <div className="space-y-6">
          <EventDigitalPassCard
            event={event}
            studentName={user ? `${user.firstName} ${user.lastName}` : "Guest Visitor"}
            studentId={user?.studentId}
            status={effectiveMyStatus}
            sessionsAttendedCount={backendMyReg?.sessionsAttendedCount ?? 0}
          />

          {canManage && (
            <EventRegistrationsPanel
              registrations={registrations}
              isLoading={false}
              isError={false}
              error={null}
              onCancelRegistration={(studentId) => setCancelTargetStudentId(studentId)}
            />
          )}
        </div>
      </div>

      <EventFormModal
        open={editEventOpen}
        onClose={() => setEditEventOpen(false)}
        initial={event}
        organizations={orgsQuery.data?.items ?? []}
        managedOrgIds={orgAdminIds}
        isPlatformAdmin={isPlatformAdmin}
        isSubmitting={mutations.update.isPending}
        onSubmitPayload={async (payload) => {
          const { organizationId: _ignored, ...updateBody } = payload;
          await mutations.update.mutateAsync(updateBody);
          setEditEventOpen(false);
          toast.success("Event updated.");
        }}
      />

      <SessionFormModal
        open={sessionModalOpen}
        onClose={() => setSessionModalOpen(false)}
        initial={editingSession}
        nextSessionNo={sessions.length + 1}
        parentStartIso={event.startDate}
        parentEndIso={event.endDate}
        isSubmitting={mutations.createSession.isPending || mutations.updateSession.isPending}
        onSubmitPayload={async (payload) => {
          if (editingSession) {
            await mutations.updateSession.mutateAsync({
              sessionId: editingSession.sessionId,
              payload,
            });
          } else {
            await mutations.createSession.mutateAsync(payload);
          }
          setSessionModalOpen(false);
          toast.success("Session saved.");
        }}
      />

      <ConfirmDialog
        open={deleteEventOpen}
        onClose={() => setDeleteEventOpen(false)}
        isLoading={mutations.remove.isPending}
        title="Delete event?"
        description={`Permanently delete "${event.title}" and all scheduled sessions?`}
        confirmLabel="Delete event"
        confirmTextMatch={event.title}
        confirmTextLabel={`Type "${event.title}" to confirm`}
        onConfirm={async () => {
          try {
            await mutations.remove.mutateAsync();
            setDeleteEventOpen(false);
            toast.success("Event deleted.");
            navigate("/events");
          } catch (error) {
            toast.error(error instanceof Error ? error.message : "Could not delete event.");
          }
        }}
      />

      <ConfirmDialog
        open={deleteSessionTarget !== null}
        onClose={() => setDeleteSessionTarget(null)}
        isLoading={mutations.removeSession.isPending}
        title="Delete session?"
        description="Removing this session deletes its check-in schedule permanently."
        confirmLabel="Delete session"
        onConfirm={async () => {
          if (deleteSessionTarget === null) {
            return;
          }
          try {
            await mutations.removeSession.mutateAsync(deleteSessionTarget);
            setDeleteSessionTarget(null);
            toast.success("Session deleted.");
          } catch (error) {
            toast.error(error instanceof Error ? error.message : "Could not delete session.");
          }
        }}
      />

      <ConfirmDialog
        open={cancelTargetStudentId !== null}
        onClose={() => setCancelTargetStudentId(null)}
        isLoading={mutations.cancelRegistration.isPending}
        title="Cancel event registration?"
        description={
          cancelTargetStudentId === "self"
            ? "Your seat will be released and the first waitlisted student may be promoted automatically."
            : "Cancel this student's registration and promote the next waitlisted student if eligible?"
        }
        confirmLabel="Cancel registration"
        onConfirm={handleConfirmCancelRegistration}
      />
    </div>
  );
}
