/**
 * @date 2026-11-24
 * @purpose RFC-5545 .ics calendar file builder for exporting TeamUp campus & online events to Google/Outlook/Apple Calendar.
 */
import type { EventSummary } from "@/api";
import { triggerFileDownload } from "./downloadFile";

function formatIcsUtcTimestamp(isoString: string): string {
  const date = new Date(isoString);
  const pad = (n: number) => String(n).padStart(2, "0");
  return (
    `${date.getUTCFullYear()}${pad(date.getUTCMonth() + 1)}${pad(date.getUTCDate())}` +
    `T${pad(date.getUTCHours())}${pad(date.getUTCMinutes())}${pad(date.getUTCSeconds())}Z`
  );
}

function escapeIcsText(value: string): string {
  return value
    .replace(/\\/g, "\\\\")
    .replace(/\r?\n/g, "\\n")
    .replace(/,/g, "\\,")
    .replace(/;/g, "\\;");
}

export function buildEventIcsContent(event: Pick<
  EventSummary,
  "eventId" | "title" | "description" | "location" | "startDate" | "endDate" | "organizationName"
>): string {
  const nowStamp = formatIcsUtcTimestamp(new Date().toISOString());
  const startStamp = formatIcsUtcTimestamp(event.startDate);
  const endStamp = formatIcsUtcTimestamp(event.endDate);

  return [
    "BEGIN:VCALENDAR",
    "VERSION:2.0",
    "PRODID:-//TeamUp//Campus Events//EN",
    "CALSCALE:GREGORIAN",
    "METHOD:PUBLISH",
    "BEGIN:VEVENT",
    `UID:teamup-event-${event.eventId}@teamup.local`,
    `DTSTAMP:${nowStamp}`,
    `DTSTART:${startStamp}`,
    `DTEND:${endStamp}`,
    `SUMMARY:${escapeIcsText(event.title)} (${escapeIcsText(event.organizationName)})`,
    `DESCRIPTION:${escapeIcsText(event.description)}`,
    `LOCATION:${escapeIcsText(event.location)}`,
    "END:VEVENT",
    "END:VCALENDAR",
    "",
  ].join("\r\n");
}

export function downloadEventIcs(
  event: Pick<
    EventSummary,
    "eventId" | "title" | "description" | "location" | "startDate" | "endDate" | "organizationName"
  >,
): void {
  const slug = event.title
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/(^-|-$)/g, "")
    .slice(0, 48);
  triggerFileDownload(
    buildEventIcsContent(event),
    `${slug || `teamup-event-${event.eventId}`}.ics`,
    "text/calendar;charset=utf-8",
  );
}
