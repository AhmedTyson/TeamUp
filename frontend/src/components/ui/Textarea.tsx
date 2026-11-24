/**
 * @date 2026-11-21
 * @purpose Accessible multi-line Textarea primitive styled to match TeamUp dark surfaces.
 */
import { useId, type Ref, type TextareaHTMLAttributes } from "react";
import { cn } from "@/lib/cn";

export interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  hint?: string;
  error?: string;
  containerClassName?: string;
  ref?: Ref<HTMLTextAreaElement>;
}

export function Textarea({
  label,
  hint,
  error,
  id,
  rows = 4,
  required,
  className,
  containerClassName,
  ref,
  ...props
}: TextareaProps) {
  const generatedId = useId();
  const textareaId = id ?? generatedId;
  const errorId = error ? `${textareaId}-error` : undefined;
  const hintId = hint && !error ? `${textareaId}-hint` : undefined;
  const describedBy = [errorId, hintId].filter(Boolean).join(" ") || undefined;

  return (
    <div className={cn("min-w-0 space-y-1.5", containerClassName)}>
      {label && (
        <label htmlFor={textareaId} className="block text-[13px] font-medium text-white/85">
          {label}
          {required && (
            <span className="ml-1 text-[#d3fb52]" aria-hidden="true">
              *
            </span>
          )}
        </label>
      )}

      <textarea
        ref={ref}
        id={textareaId}
        rows={rows}
        required={required}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        className={cn(
          "w-full rounded-xl border bg-[#14151c] px-3.5 py-2.5 text-base text-white placeholder:text-white/40 transition-colors sm:text-sm",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] focus-visible:ring-offset-2 focus-visible:ring-offset-[#14151c]",
          "disabled:cursor-not-allowed disabled:opacity-50",
          error
            ? "border-red-500/70 focus-visible:border-red-400"
            : "border-white/12 hover:border-white/25 focus-visible:border-[#d3fb52]/60",
          className,
        )}
        {...props}
      />

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
