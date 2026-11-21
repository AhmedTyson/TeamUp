/**
 * @date 2026-11-21
 * @purpose Semantic Badge primitive aligned with TeamUp Handshake status and brand accent colors.
 */
import type { HTMLAttributes } from "react";
import { cn } from "@/lib/cn";

export type BadgeVariant =
  | "default"
  | "accent"
  | "success"
  | "warning"
  | "danger"
  | "info"
  | "outline";

export type BadgeSize = "sm" | "md";

export interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  variant?: BadgeVariant;
  size?: BadgeSize;
}

const VARIANT_CLASSES: Record<BadgeVariant, string> = {
  default: "border-white/12 bg-white/8 text-white/85",
  accent: "border-[#d3fb52]/40 bg-[#d3fb52]/15 text-[#d3fb52]",
  success: "border-emerald-500/35 bg-emerald-500/15 text-emerald-300",
  warning: "border-amber-500/35 bg-amber-500/15 text-amber-300",
  danger: "border-red-500/35 bg-red-500/15 text-red-300",
  info: "border-sky-400/35 bg-sky-400/15 text-sky-300",
  outline: "border-white/20 bg-transparent text-white/75",
};

const SIZE_CLASSES: Record<BadgeSize, string> = {
  sm: "px-2 py-0.5 text-[11px]",
  md: "px-2.5 py-1 text-xs",
};

export function Badge({
  variant = "default",
  size = "md",
  className,
  ...props
}: BadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full border font-medium leading-none whitespace-nowrap",
        VARIANT_CLASSES[variant],
        SIZE_CLASSES[size],
        className,
      )}
      {...props}
    />
  );
}
