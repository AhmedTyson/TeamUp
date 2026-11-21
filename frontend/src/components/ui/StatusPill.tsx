/**
 * @date 2026-11-21
 * @purpose Domain-aware StatusPill mapping TeamUp enum statuses to semantic Handshake dot colors and readable labels.
 */
import type { HTMLAttributes } from "react";
import { cn } from "@/lib/cn";
import type { BadgeVariant } from "./Badge";

export interface StatusPillProps extends HTMLAttributes<HTMLSpanElement> {
  status: string;
  label?: string;
  variant?: BadgeVariant;
  showDot?: boolean;
}

const STATUS_VARIANT_MAP: Record<string, BadgeVariant> = {
  // ProjectStatus
  Recruiting: "accent",
  InProgress: "info",
  Completed: "success",
  Archived: "default",

  // ApplicationStatus
  Pending: "warning",
  Accepted: "success",
  Rejected: "danger",
  Withdrawn: "default",

  // ProjectTaskStatus
  Todo: "default",
  InReview: "warning",
  Done: "success",

  // TaskPriority
  Low: "default",
  Medium: "info",
  High: "warning",
  Urgent: "danger",

  // RegistrationStatus
  Confirmed: "success",
  Waitlisted: "warning",
  Cancelled: "danger",

  // EventType
  Workshop: "info",
  Hackathon: "accent",
  Session: "default",
  Competition: "warning",

  // ScoreSourceType
  EventAttendance: "info",
  TaskCompleted: "success",
  ProjectDelivered: "accent",
  Manual: "warning",

  // UserRole
  Student: "default",
  OrgAdmin: "info",
  PlatformAdmin: "accent",
};

const STATUS_LABEL_MAP: Record<string, string> = {
  InProgress: "In Progress",
  InReview: "In Review",
  ManualByOrganizer: "Manual",
  EventAttendance: "Event Attendance",
  TaskCompleted: "Task Completed",
  ProjectDelivered: "Project Delivered",
  PlatformAdmin: "Platform Admin",
  OrgAdmin: "Org Admin",
};

const PILL_STYLES: Record<BadgeVariant, { container: string; dot: string }> = {
  default: { container: "border-white/12 bg-white/8 text-white/85", dot: "bg-white/60" },
  accent: { container: "border-[#d3fb52]/40 bg-[#d3fb52]/15 text-[#d3fb52]", dot: "bg-[#d3fb52]" },
  success: { container: "border-emerald-500/35 bg-emerald-500/15 text-emerald-300", dot: "bg-emerald-400" },
  warning: { container: "border-amber-500/35 bg-amber-500/15 text-amber-300", dot: "bg-amber-400" },
  danger: { container: "border-red-500/35 bg-red-500/15 text-red-300", dot: "bg-red-400" },
  info: { container: "border-sky-400/35 bg-sky-400/15 text-sky-300", dot: "bg-sky-400" },
  outline: { container: "border-white/20 bg-transparent text-white/75", dot: "bg-white/50" },
};

export function StatusPill({
  status,
  label,
  variant,
  showDot = true,
  className,
  ...props
}: StatusPillProps) {
  const resolvedVariant = variant ?? STATUS_VARIANT_MAP[status] ?? "default";
  const styles = PILL_STYLES[resolvedVariant];
  const displayLabel = label ?? STATUS_LABEL_MAP[status] ?? status;

  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-medium leading-none whitespace-nowrap",
        styles.container,
        className,
      )}
      {...props}
    >
      {showDot && <span className={cn("size-1.5 rounded-full", styles.dot)} aria-hidden="true" />}
      {displayLabel}
    </span>
  );
}
