/**
 * @date 2026-11-21
 * @purpose Accessible dark-surface text Input primitive with label, hint, validation error, and adornments.
 */
import { useId, type InputHTMLAttributes, type ReactNode, type Ref } from "react";
import { cn } from "@/lib/cn";

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  hint?: string;
  error?: string;
  leftElement?: ReactNode;
  rightElement?: ReactNode;
  containerClassName?: string;
  ref?: Ref<HTMLInputElement>;
}

export function Input({
  label,
  hint,
  error,
  leftElement,
  rightElement,
  id,
  className,
  containerClassName,
  required,
  ref,
  ...props
}: InputProps) {
  const generatedId = useId();
  const inputId = id ?? generatedId;
  const errorId = error ? `${inputId}-error` : undefined;
  const hintId = hint && !error ? `${inputId}-hint` : undefined;
  const describedBy = [errorId, hintId].filter(Boolean).join(" ") || undefined;

  return (
    <div className={cn("space-y-1.5", containerClassName)}>
      {label && (
        <label htmlFor={inputId} className="block text-[13px] font-medium text-white/85">
          {label}
          {required && (
            <span className="ml-1 text-[#d3fb52]" aria-hidden="true">
              *
            </span>
          )}
        </label>
      )}

      <div className="relative flex items-center">
        {leftElement && (
          <span className="pointer-events-none absolute left-3 inline-flex items-center text-white/45">
            {leftElement}
          </span>
        )}
        <input
          ref={ref}
          id={inputId}
          required={required}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          className={cn(
            "h-10 w-full rounded-xl border bg-[#14151c] px-3.5 text-sm text-white placeholder:text-white/40 transition-colors",
            "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] focus-visible:ring-offset-2 focus-visible:ring-offset-[#14151c]",
            "disabled:cursor-not-allowed disabled:opacity-50",
            error
              ? "border-red-500/70 focus-visible:border-red-400"
              : "border-white/12 hover:border-white/25 focus-visible:border-[#d3fb52]/60",
            Boolean(leftElement) && "pl-10",
            Boolean(rightElement) && "pr-10",
            className,
          )}
          {...props}
        />
        {rightElement && (
          <span className="absolute right-3 inline-flex items-center text-white/55">
            {rightElement}
          </span>
        )}
      </div>

      {error ? (
        <p id={errorId} role="alert" className="text-xs font-medium text-red-400">
          {error}
        </p>
      ) : (
        hint && (
          <p id={hintId} className="text-xs text-[#9ca3af]">
            {hint}
          </p>
        )
      )}
    </div>
  );
}
