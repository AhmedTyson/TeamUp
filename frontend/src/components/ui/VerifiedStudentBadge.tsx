/**
 * @date 2026-11-27
 * @purpose Reusable Verified Student email verification badge with pill, unverified fallback, and compact icon-only modes.
 */
import { BadgeCheck, Clock3 } from "lucide-react";
import { cn } from "@/lib/cn";

export interface VerifiedStudentBadgeProps {
  verified?: boolean;
  compact?: boolean;
  showUnverified?: boolean;
  size?: "sm" | "md";
  className?: string;
}

export function VerifiedStudentBadge({
  verified = false,
  compact = false,
  showUnverified = false,
  size = "sm",
  className,
}: VerifiedStudentBadgeProps) {
  if (!verified && !showUnverified) {
    return null;
  }

  if (compact) {
    return verified ? (
      <span
        title="Verified Student"
        aria-label="Verified Student"
        className={cn("inline-flex shrink-0 items-center text-[#d3fb52]", className)}
      >
        <BadgeCheck className={size === "md" ? "size-4" : "size-3.5"} aria-hidden="true" />
      </span>
    ) : null;
  }

  const sizeClass =
    size === "md"
      ? "gap-1.5 px-2.5 py-1 text-xs"
      : "gap-1 px-2 py-0.5 text-[11px]";

  if (verified) {
    return (
      <span
        className={cn(
          "inline-flex shrink-0 items-center rounded-full border border-[#d3fb52]/35 bg-[#d3fb52]/12 font-mono font-semibold text-[#d3fb52]",
          sizeClass,
          className,
        )}
      >
        <BadgeCheck className={size === "md" ? "size-3.5" : "size-3"} aria-hidden="true" />
        <span>Verified Student</span>
      </span>
    );
  }

  return (
    <span
      className={cn(
        "inline-flex shrink-0 items-center rounded-full border border-white/15 bg-white/[0.05] font-mono font-medium text-white/60",
        sizeClass,
        className,
      )}
    >
      <Clock3 className={size === "md" ? "size-3.5" : "size-3"} aria-hidden="true" />
      <span>Unverified</span>
    </span>
  );
}
