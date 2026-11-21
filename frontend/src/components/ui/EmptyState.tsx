/**
 * @date 2026-11-21
 * @purpose Reusable EmptyState callout for empty lists, 404 resources, or zero search results.
 */
import type { ReactNode } from "react";
import { cn } from "@/lib/cn";

export interface EmptyStateProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  action?: ReactNode;
  className?: string;
}

export function EmptyState({ icon, title, description, action, className }: EmptyStateProps) {
  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center rounded-2xl border border-dashed border-white/12 bg-[#1b1d26]/70 px-6 py-12 text-center",
        className,
      )}
    >
      {icon && (
        <div
          aria-hidden="true"
          className="mb-4 flex size-12 items-center justify-center rounded-2xl border border-[#d3fb52]/25 bg-[#d3fb52]/10 text-[#d3fb52]"
        >
          {icon}
        </div>
      )}
      <h3
        className="text-base font-semibold text-white"
        style={{ fontFamily: "var(--font-heading)" }}
      >
        {title}
      </h3>
      {description && <p className="mt-1.5 max-w-md text-sm text-[#9ca3af]">{description}</p>}
      {action && <div className="mt-5">{action}</div>}
    </div>
  );
}
