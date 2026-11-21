/**
 * @date 2026-11-21
 * @purpose Accessible Select dropdown primitive supporting options prop or native <option> children.
 */
import { ChevronDown } from "lucide-react";
import { useId, type Ref, type SelectHTMLAttributes } from "react";
import { cn } from "@/lib/cn";

export interface SelectOption {
  value: string | number;
  label: string;
  disabled?: boolean;
}

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  hint?: string;
  error?: string;
  placeholder?: string;
  options?: readonly SelectOption[];
  containerClassName?: string;
  ref?: Ref<HTMLSelectElement>;
}

export function Select({
  label,
  hint,
  error,
  placeholder,
  options,
  id,
  required,
  className,
  containerClassName,
  children,
  ref,
  ...props
}: SelectProps) {
  const generatedId = useId();
  const selectId = id ?? generatedId;
  const errorId = error ? `${selectId}-error` : undefined;
  const hintId = hint && !error ? `${selectId}-hint` : undefined;
  const describedBy = [errorId, hintId].filter(Boolean).join(" ") || undefined;

  return (
    <div className={cn("space-y-1.5", containerClassName)}>
      {label && (
        <label htmlFor={selectId} className="block text-[13px] font-medium text-white/85">
          {label}
          {required && (
            <span className="ml-1 text-[#d3fb52]" aria-hidden="true">
              *
            </span>
          )}
        </label>
      )}

      <div className="relative">
        <select
          ref={ref}
          id={selectId}
          required={required}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          className={cn(
            "h-10 w-full appearance-none rounded-xl border bg-[#14151c] pl-3.5 pr-10 text-sm text-white transition-colors",
            "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] focus-visible:ring-offset-2 focus-visible:ring-offset-[#14151c]",
            "disabled:cursor-not-allowed disabled:opacity-50",
            error
              ? "border-red-500/70 focus-visible:border-red-400"
              : "border-white/12 hover:border-white/25 focus-visible:border-[#d3fb52]/60",
            className,
          )}
          {...props}
        >
          {placeholder && (
            <option value="" className="bg-[#1b1d26] text-white/60">
              {placeholder}
            </option>
          )}
          {options
            ? options.map((opt) => (
                <option
                  key={String(opt.value)}
                  value={opt.value}
                  disabled={opt.disabled}
                  className="bg-[#1b1d26] text-white"
                >
                  {opt.label}
                </option>
              ))
            : children}
        </select>
        <ChevronDown
          className="pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2 text-white/50"
          aria-hidden="true"
        />
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
