/**
 * @date 2026-11-21
 * @purpose Handshake Card surface primitive supporting dark, elevated, and high-contrast light inset variants.
 */
import type { HTMLAttributes } from "react";
import { cn } from "@/lib/cn";

export type CardVariant = "default" | "elevated" | "light";

export interface CardProps extends HTMLAttributes<HTMLDivElement> {
  variant?: CardVariant;
}

const VARIANT_CLASSES: Record<CardVariant, string> = {
  default: "border border-white/[0.085] bg-[#1b1d26] text-white",
  elevated: "border border-white/12 bg-[#222532] text-white shadow-xl shadow-black/35",
  light: "border border-black/10 bg-white text-[#111827] shadow-lg shadow-black/20",
};

export function Card({ variant = "default", className, ...props }: CardProps) {
  return (
    <div
      className={cn("rounded-2xl transition-colors", VARIANT_CLASSES[variant], className)}
      {...props}
    />
  );
}

export function CardHeader({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn("flex flex-col gap-1.5 p-6", className)} {...props} />;
}

export function CardTitle({ className, ...props }: HTMLAttributes<HTMLHeadingElement>) {
  return (
    <h3
      className={cn("text-lg font-semibold tracking-tight", className)}
      style={{ fontFamily: "var(--font-heading)" }}
      {...props}
    />
  );
}

export function CardDescription({ className, ...props }: HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn("text-sm text-[#9ca3af]", className)} {...props} />;
}

export function CardContent({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn("px-6 pb-6", className)} {...props} />;
}

export function CardFooter({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn(
        "flex items-center justify-between gap-3 border-t border-white/[0.085] px-6 py-4",
        className,
      )}
      {...props}
    />
  );
}
