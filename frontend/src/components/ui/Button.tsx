/**
 * @date 2026-11-21
 * @purpose Handshake-styled accessible Button primitive supporting primary lime, secondary, outline, ghost, and danger variants.
 */
import { Loader2 } from "lucide-react";
import type { ButtonHTMLAttributes, ReactNode, Ref } from "react";
import { cn } from "@/lib/cn";

export type ButtonVariant = "primary" | "secondary" | "outline" | "ghost" | "danger";
export type ButtonSize = "sm" | "md" | "lg" | "icon";

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  isLoading?: boolean;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  ref?: Ref<HTMLButtonElement>;
}

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary:
    "bg-[#d3fb52] text-[#14151c] font-semibold hover:bg-[#deff6e] active:scale-[0.99] shadow-sm shadow-[#d3fb52]/10",
  secondary:
    "bg-[#222532] text-white font-medium border border-white/10 hover:bg-[#2a2e3e] hover:border-white/20",
  outline:
    "bg-transparent text-white font-medium border border-white/15 hover:border-[#d3fb52]/50 hover:bg-white/5",
  ghost: "bg-transparent text-white/80 font-medium hover:bg-white/8 hover:text-white",
  danger:
    "bg-red-500/15 text-red-300 font-semibold border border-red-500/35 hover:bg-red-500/25 hover:text-red-200",
};

const SIZE_CLASSES: Record<ButtonSize, string> = {
  sm: "min-h-[36px] rounded-lg px-3 py-1.5 text-xs gap-1.5",
  md: "min-h-[40px] rounded-xl px-4 py-2 text-sm gap-2",
  lg: "min-h-[48px] rounded-xl px-6 py-2.5 text-base gap-2.5",
  icon: "size-9 sm:size-10 rounded-xl p-0",
};

export function Button({
  variant = "primary",
  size = "md",
  isLoading = false,
  leftIcon,
  rightIcon,
  disabled,
  type = "button",
  className,
  children,
  ref,
  ...props
}: ButtonProps) {
  return (
    <button
      ref={ref}
      type={type}
      disabled={disabled || isLoading}
      aria-busy={isLoading || undefined}
      className={cn(
        "inline-flex shrink-0 cursor-pointer items-center justify-center whitespace-nowrap transition-all duration-150",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] focus-visible:ring-offset-2 focus-visible:ring-offset-[#14151c]",
        "disabled:pointer-events-none disabled:opacity-50",
        VARIANT_CLASSES[variant],
        SIZE_CLASSES[size],
        className,
      )}
      {...props}
    >
      {isLoading ? (
        <Loader2 className="size-4 animate-spin" aria-hidden="true" />
      ) : (
        leftIcon && <span className="inline-flex shrink-0 items-center">{leftIcon}</span>
      )}
      {children}
      {!isLoading && rightIcon && (
        <span className="inline-flex shrink-0 items-center">{rightIcon}</span>
      )}
    </button>
  );
}
