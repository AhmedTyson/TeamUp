/**
 * @date 2026-11-21
 * @purpose Mouse-tracked Handshake spotlight card (.service-card) with optional electric lime corner action badge.
 */
import { ArrowUpRight } from "lucide-react";
import type { HTMLAttributes, MouseEvent } from "react";
import { cn } from "@/lib/cn";

export interface SpotlightCardProps extends HTMLAttributes<HTMLDivElement> {
  showCornerArrow?: boolean;
}

export function SpotlightCard({
  showCornerArrow = false,
  className,
  children,
  onMouseMove,
  ...props
}: SpotlightCardProps) {
  const handleMouseMove = (event: MouseEvent<HTMLDivElement>) => {
    const rect = event.currentTarget.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;
    event.currentTarget.style.setProperty("--mouse-x", `${x}px`);
    event.currentTarget.style.setProperty("--mouse-y", `${y}px`);
    onMouseMove?.(event);
  };

  return (
    <div
      onMouseMove={handleMouseMove}
      className={cn("service-card group p-6 text-white", className)}
      {...props}
    >
      {showCornerArrow && (
        <span
          aria-hidden="true"
          className="pointer-events-none absolute end-5 top-5 inline-flex size-9 items-center justify-center rounded-full border border-white/10 bg-white/5 text-white/70 transition-all duration-300 group-hover:border-[#d3fb52] group-hover:bg-[#d3fb52] group-hover:text-[#14151c]"
        >
          <ArrowUpRight className="size-4" />
        </span>
      )}
      <div className="relative z-10">{children}</div>
    </div>
  );
}
