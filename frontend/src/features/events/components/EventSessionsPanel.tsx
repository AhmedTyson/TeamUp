/**
 * @date 2026-11-24
 * @purpose Multi-session schedule panel for EventDetailPage with organizer CRUD shortcuts and student Check-In links.
 */
import { CalendarDays, Clock, Plus, QrCode } from "lucide-react";
import { Link } from "react-router-dom";
import type { EventSession } from "@/api";
import { Button, Card, EmptyState } from "@/components/ui";

export interface EventSessionsPanelProps {
  eventId: number;
  sessions: readonly EventSession[];
  canManage: boolean;
  onAddSession: () => void;
  onEditSession: (sessionId: number) => void;
  onDeleteSession: (sessionId: number) => void;
}

export function EventSessionsPanel({
  eventId,
  sessions,
  canManage,
  onAddSession,
  onEditSession,
  onDeleteSession,
}: EventSessionsPanelProps) {
  return (
    <Card className="p-5 sm:p-6">
      <div className="flex items-center justify-between gap-2">
        <h2 className="text-xl font-bold text-white">Sessions timetable</h2>
        {canManage && (
          <Button
            size="sm"
            leftIcon={<Plus className="size-3.5" />}
            onClick={onAddSession}
          >
            Add session
          </Button>
        )}
      </div>

      <div className="mt-4 space-y-3">
        {sessions.length > 0 ? (
          sessions.map((session) => (
            <div
              key={session.sessionId}
              className="space-y-2 rounded-2xl border border-white/10 bg-[#14151c] p-4"
            >
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-semibold text-white">
                    #{session.sessionNo} · {session.title}
                  </p>
                  <p className="mt-1 inline-flex items-center gap-1.5 text-xs text-white/55">
                    <Clock className="size-3.5" aria-hidden="true" />
                    {new Date(session.startTime).toLocaleString()} –{" "}
                    {new Date(session.endTime).toLocaleTimeString()} ·{" "}
                    {session.room ?? "TBA"}
                  </p>
                </div>

                {canManage ? (
                  <Link
                    to={`/events/${eventId}/sessions/${session.sessionId}/attendance`}
                    className="inline-flex items-center gap-1.5 rounded-lg bg-[#d3fb52] px-3 py-1.5 text-xs font-bold text-[#14151c] transition-opacity hover:opacity-90"
                  >
                    <QrCode className="size-3.5" aria-hidden="true" />
                    Attendance console
                  </Link>
                ) : (
                  <Link
                    to={`/check-in?sessionId=${session.sessionId}`}
                    className="inline-flex items-center gap-1.5 rounded-lg border border-[#d3fb52]/35 px-3 py-1.5 text-xs font-semibold text-[#d3fb52] hover:bg-[#d3fb52]/10"
                  >
                    <QrCode className="size-3.5" aria-hidden="true" />
                    Check in
                  </Link>
                )}
              </div>

              {canManage && (
                <div className="flex gap-2 pt-1">
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => onEditSession(session.sessionId)}
                  >
                    Edit
                  </Button>
                  <Button
                    size="sm"
                    variant="danger"
                    onClick={() => onDeleteSession(session.sessionId)}
                  >
                    Delete
                  </Button>
                </div>
              )}
            </div>
          ))
        ) : (
          <EmptyState
            icon={<CalendarDays className="size-6" />}
            title="No sessions scheduled yet"
            description="Organizers will add check-in sessions inside this event window."
          />
        )}
      </div>
    </Card>
  );
}
