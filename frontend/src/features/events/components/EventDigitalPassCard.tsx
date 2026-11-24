/**
 * @date 2026-11-24
 * @purpose Crisp Luma-inspired paper-white TeamUp Digital Attendee Pass card with QR check-in ticket, .ics calendar export, and confirmed meeting URL access.
 */
import { CalendarPlus, ExternalLink, Ticket, Video } from "lucide-react";
import { QRCodeSVG } from "qrcode.react";
import type { EventSummary, RegistrationStatus } from "@/api";
import { Badge, Button, StatusPill } from "@/components/ui";
import { downloadEventIcs } from "@/lib/calendarExport";
import { useI18n } from "@/lib/i18n";
import { isSafeHttpUrl } from "@/lib/urlValidation";

export interface EventDigitalPassCardProps {
  event: EventSummary;
  studentName: string;
  studentId?: number;
  status: RegistrationStatus | null;
  sessionsAttendedCount?: number;
}

export function EventDigitalPassCard({
  event,
  studentName,
  studentId,
  status,
  sessionsAttendedCount = 0,
}: EventDigitalPassCardProps) {
  const { t } = useI18n();
  const ticketPayload = JSON.stringify({
    eventId: event.eventId,
    studentId: studentId ?? 0,
    status: status ?? "GuestPreview",
  });

  const showMeetingUrl =
    status === "Confirmed" &&
    event.meetingUrl &&
    isSafeHttpUrl(event.meetingUrl);

  return (
    <div className="overflow-hidden rounded-3xl border border-black/10 bg-white text-[#14151c] shadow-xl">
      <div className="flex items-center justify-between gap-2 bg-[#14151c] px-5 py-3.5 text-white">
        <div className="flex items-center gap-2">
          <Ticket className="size-4 text-[#d3fb52]" aria-hidden="true" />
          <span className="font-mono text-xs font-bold uppercase tracking-widest text-[#d3fb52]">
            {t("events.digitalPass")}
          </span>
        </div>
        {status ? (
          <StatusPill status={status} />
        ) : (
          <Badge variant="outline" size="sm">
            Preview
          </Badge>
        )}
      </div>

      <div className="space-y-4 p-5">
        <div className="flex items-start justify-between gap-4">
          <div className="min-w-0 space-y-1">
            <p className="font-mono text-[11px] uppercase tracking-wider text-[#6b7280]">
              Attendee Passholder
            </p>
            <p className="truncate text-lg font-bold text-[#14151c]">{studentName}</p>
            <p className="text-xs font-medium text-[#15733f]">
              {event.organizationName} · {event.deliveryMode ?? "InPerson"}
            </p>
          </div>

          <div className="shrink-0 rounded-xl border border-black/10 bg-[#f8faf9] p-2">
            <QRCodeSVG value={ticketPayload} size={76} level="M" />
          </div>
        </div>

        {/* Perforated ticket divider */}
        <div className="border-t border-dashed border-black/15" aria-hidden="true" />

        <div className="grid grid-cols-2 gap-3 text-xs">
          <div>
            <p className="font-mono text-[10px] uppercase tracking-wider text-[#6b7280]">
              Event ID
            </p>
            <p className="font-mono font-bold text-[#14151c]">#TU-{event.eventId}</p>
          </div>
          <div>
            <p className="font-mono text-[10px] uppercase tracking-wider text-[#6b7280]">
              Sessions Attended
            </p>
            <p className="font-mono font-bold text-[#15733f]">
              {sessionsAttendedCount} / {event.sessions.length}
            </p>
          </div>
        </div>

        <div className="flex flex-wrap gap-2 pt-1">
          <Button
            type="button"
            size="sm"
            variant="ghost"
            className="!border !border-black/20 !bg-[#f3f6f4] !text-[#14151c] hover:!bg-[#d3fb52] hover:!text-[#14151c]"
            leftIcon={<CalendarPlus className="size-3.5" />}
            onClick={() => downloadEventIcs(event)}
          >
            {t("events.downloadIcs")}
          </Button>

          {showMeetingUrl && (
            <a
              href={event.meetingUrl!}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex h-8 items-center gap-1.5 rounded-lg bg-[#15733f] px-3 text-xs font-semibold text-white hover:opacity-90"
            >
              <Video className="size-3.5" aria-hidden="true" />
              Join Live Stream
              <ExternalLink className="size-3" aria-hidden="true" />
            </a>
          )}
        </div>
      </div>
    </div>
  );
}
