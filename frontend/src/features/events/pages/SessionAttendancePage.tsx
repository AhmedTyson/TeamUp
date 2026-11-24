/**
 * @date 2026-11-24
 * @purpose Scoped organizer attendance console with rotating QR projector, Stage Mode fullscreen, manual check-in, verified log, CSV export, and printable HTML report.
 */
import { ArrowLeft, ClipboardList, Download, Printer } from "lucide-react";
import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import {
  Badge,
  Button,
  Card,
  ConfirmDialog,
  EmptyState,
  ErrorBanner,
  ForbiddenBanner,
  Modal,
  Skeleton,
  StatusPill,
} from "@/components/ui";
import { useAuth } from "@/features/auth/AuthContext";
import { canManageOrganization, useOrganization } from "@/features/organizations";
import { triggerFileDownload } from "@/lib/downloadFile";
import { ManualCheckInCard } from "../components/ManualCheckInCard";
import { QrProjectorCard } from "../components/QrProjectorCard";
import {
  useAttendanceMutations,
  useEvent,
  useEventRegistrations,
  useEventSessions,
  useSessionAttendance,
} from "../hooks/useEventHooks";
import { formatAttendanceRecorder } from "../utils/attendance";

export function SessionAttendancePage() {
  const { id: rawEventId, sessionId: rawSessionId } = useParams();
  const eventId = Number(rawEventId);
  const sessionId = Number(rawSessionId);
  const navigate = useNavigate();
  const { user, isPlatformAdmin, isOrgAdminOf } = useAuth();

  const isValidIds =
    Number.isInteger(eventId) && eventId > 0 && Number.isInteger(sessionId) && sessionId > 0;

  const eventQuery = useEvent(eventId);
  const event = eventQuery.data;
  const orgQuery = useOrganization(event?.organizationId ?? 0);
  const sessionsQuery = useEventSessions(eventId);

  const canManage = Boolean(
    event &&
      canManageOrganization(
        event.organizationId,
        user?.studentId,
        isPlatformAdmin,
        isOrgAdminOf,
        orgQuery.data?.members ?? [],
      ),
  );

  const attendanceQuery = useSessionAttendance(sessionId, Boolean(event && canManage));
  const registrationsQuery = useEventRegistrations(eventId, Boolean(event && canManage));
  const mutations = useAttendanceMutations(sessionId);

  const [revokeStudentId, setRevokeStudentId] = useState<number | null>(null);
  const [reportOpen, setReportOpen] = useState(false);
  const [reportHtml, setReportHtml] = useState("");

  if (!isValidIds) {
    return (
      <EmptyState
        icon={<ClipboardList className="size-6" />}
        title="Invalid session link"
        description="The requested event or session identifier is invalid."
        action={
          <Button variant="outline" onClick={() => navigate("/events")}>
            Back to events
          </Button>
        }
      />
    );
  }

  if (eventQuery.isLoading || sessionsQuery.isLoading || (event && orgQuery.isLoading)) {
    return <Skeleton className="h-[520px] rounded-3xl" />;
  }

  if (eventQuery.isError || sessionsQuery.isError || !event) {
    return (
      <div className="space-y-4">
        <ErrorBanner
          error={eventQuery.error ?? sessionsQuery.error}
          message="Could not load event session attendance details."
          onRetry={() => {
            void eventQuery.refetch();
            void sessionsQuery.refetch();
          }}
        />
        <Button variant="outline" onClick={() => navigate(`/events/${eventId}`)}>
          Back to event
        </Button>
      </div>
    );
  }

  const session = (sessionsQuery.data ?? []).find((s) => s.sessionId === sessionId);
  if (!session) {
    return (
      <EmptyState
        icon={<ClipboardList className="size-6" />}
        title="Session not found"
        description={`Session #${sessionId} does not belong to this event or no longer exists.`}
        action={
          <Button variant="outline" onClick={() => navigate(`/events/${eventId}`)}>
            Back to event
          </Button>
        }
      />
    );
  }

  if (!canManage) {
    return (
      <div className="space-y-4">
        <Button
          variant="ghost"
          size="sm"
          leftIcon={<ArrowLeft className="size-4" />}
          onClick={() => navigate(`/events/${eventId}`)}
        >
          Back to event
        </Button>
        <ForbiddenBanner message="You do not have permission to manage session attendance for this organization." />
      </div>
    );
  }

  const attendance = attendanceQuery.data ?? [];
  const registrations = registrationsQuery.data ?? [];
  const confirmedCount = registrations.filter((r) => r.status === "Confirmed").length;
  const pendingConfirmed = registrations.filter(
    (reg) =>
      reg.status === "Confirmed" &&
      !attendance.some((record) => record.studentId === reg.studentId),
  );

  const handleOpenReport = async () => {
    try {
      const html = await mutations.report.mutateAsync();
      setReportHtml(html);
      setReportOpen(true);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Attendance report unavailable.");
    }
  };

  const handleExportCsv = async () => {
    try {
      const csv = await mutations.exportCsv.mutateAsync();
      triggerFileDownload(csv, `attendance-session-${sessionId}.csv`, "text/csv;charset=utf-8");
      toast.success("Attendance CSV exported.");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not export CSV.");
    }
  };

  return (
    <div className="space-y-6">
      <Button
        variant="ghost"
        size="sm"
        leftIcon={<ArrowLeft className="size-4" />}
        onClick={() => navigate(`/events/${eventId}`)}
      >
        Back to event
      </Button>

      <section className="relative overflow-hidden rounded-[28px] border border-white/10 bg-[radial-gradient(ellipse_at_80%_0%,rgba(83,152,134,0.35),transparent_50%),linear-gradient(135deg,#1b1d26,#14151c)] p-6 shadow-2xl sm:p-9">
        <div className="max-w-3xl space-y-3">
          <Badge variant="accent">Organizer workspace</Badge>
          <h1
            className="text-4xl font-bold uppercase leading-[0.95] text-[#d3fb52] sm:text-6xl"
            style={{ fontFamily: "var(--font-display)" }}
          >
            Live check-in.
          </h1>
          <p className="text-sm text-white/70 sm:text-base">
            {event.title} · #{session.sessionNo} {session.title} ·{" "}
            {session.room ?? "Room not specified"}
          </p>
        </div>
      </section>

      <div className="grid gap-5 lg:grid-cols-[0.95fr_1.05fr]">
        <QrProjectorCard
          sessionId={sessionId}
          sessionTitle={`#${session.sessionNo} · ${session.title}`}
          eventTitle={event.title}
          roomLabel={session.room ?? event.location}
          checkedInCount={attendance.length}
          confirmedCount={confirmedCount}
          recentAttendees={attendance}
          isGeneratingCode={mutations.generateCode.isPending}
          isLoadingReport={mutations.report.isPending}
          onGenerateCode={() => mutations.generateCode.mutateAsync()}
          onOpenReport={handleOpenReport}
        />

        <ManualCheckInCard
          pendingConfirmedRegistrants={pendingConfirmed}
          isSubmitting={mutations.manualCheckIn.isPending}
          onSubmitCheckIn={async (payload) => {
            await mutations.manualCheckIn.mutateAsync(payload);
          }}
        />
      </div>

      <Card className="p-5 sm:p-6">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <h2 className="text-xl font-bold text-white">Verified attendance</h2>
            <Badge variant="success">{attendance.length} checked in</Badge>
          </div>

          <Button
            size="sm"
            variant="outline"
            isLoading={mutations.exportCsv.isPending}
            leftIcon={<Download className="size-3.5" />}
            onClick={() => void handleExportCsv()}
          >
            Export CSV
          </Button>
        </div>

        {attendanceQuery.isError ? (
          <div className="mt-4">
            <ErrorBanner error={attendanceQuery.error} />
          </div>
        ) : attendance.length > 0 ? (
          <div className="mt-4 overflow-x-auto">
            <table className="w-full min-w-[680px] text-left text-sm">
              <thead className="text-xs text-white/50">
                <tr>
                  <th className="p-3">Student</th>
                  <th className="p-3">Time</th>
                  <th className="p-3">Method</th>
                  <th className="p-3">Recorded by</th>
                  <th className="p-3">Action</th>
                </tr>
              </thead>
              <tbody>
                {attendance.map((record) => (
                  <tr key={record.studentId} className="border-t border-white/10">
                    <td className="p-3 font-medium text-white">
                      {record.firstName} {record.lastName}
                    </td>
                    <td className="p-3 text-white/75">
                      {new Date(record.checkInTime).toLocaleTimeString()}
                    </td>
                    <td className="p-3">
                      <StatusPill status={record.verificationMethod} />
                    </td>
                    <td className="p-3 text-xs text-white/75">
                      {formatAttendanceRecorder(record)}
                    </td>
                    <td className="p-3">
                      <Button
                        size="sm"
                        variant="danger"
                        onClick={() => setRevokeStudentId(record.studentId)}
                      >
                        Revoke
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="mt-4">
            <EmptyState
              icon={<ClipboardList className="size-6" />}
              title="No attendance records yet"
              description="Checked-in students will appear here in real time."
            />
          </div>
        )}
      </Card>

      <Modal
        open={reportOpen}
        onClose={() => setReportOpen(false)}
        title="Session attendance report"
        size="xl"
        footer={
          <Button
            leftIcon={<Printer className="size-4" />}
            onClick={() => {
              const frame = document.getElementById(
                "attendance-report-frame",
              ) as HTMLIFrameElement | null;
              frame?.contentWindow?.print();
            }}
          >
            Print / Export PDF
          </Button>
        }
      >
        <iframe
          id="attendance-report-frame"
          title="Attendance report"
          srcDoc={reportHtml}
          sandbox="allow-same-origin allow-modals"
          className="h-[65vh] w-full rounded-xl bg-white"
        />
      </Modal>

      <ConfirmDialog
        open={revokeStudentId !== null}
        onClose={() => setRevokeStudentId(null)}
        isLoading={mutations.revoke.isPending}
        title="Revoke attendance record?"
        description="Removing this check-in record may revert awarded session attendance status."
        confirmLabel="Revoke attendance"
        onConfirm={async () => {
          if (revokeStudentId === null) {
            return;
          }
          try {
            await mutations.revoke.mutateAsync(revokeStudentId);
            setRevokeStudentId(null);
            toast.success("Attendance revoked.");
          } catch (error) {
            toast.error(error instanceof Error ? error.message : "Could not revoke attendance.");
          }
        }}
      />
    </div>
  );
}
