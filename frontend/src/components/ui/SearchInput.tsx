/**
 * @date 2026-11-21
 * @purpose Debounced search input with clear button and Escape-to-clear keyboard support.
 */
import { Search, X } from "lucide-react";
import { useEffect, useRef, type ChangeEvent, type InputHTMLAttributes, type KeyboardEvent } from "react";
import { cn } from "@/lib/cn";

export interface SearchInputProps
  extends Omit<InputHTMLAttributes<HTMLInputElement>, "onChange" | "value"> {
  value: string;
  onChange: (value: string) => void;
  onDebouncedChange?: (value: string) => void;
  debounceMs?: number;
  containerClassName?: string;
}

export function SearchInput({
  value,
  onChange,
  onDebouncedChange,
  debounceMs = 300,
  placeholder = "Search...",
  className,
  containerClassName,
  "aria-label": ariaLabel = "Search",
  ...props
}: SearchInputProps) {
  const lastCommittedValue = useRef(value);
  const latestCallback = useRef(onDebouncedChange);
  latestCallback.current = onDebouncedChange;

  useEffect(() => {
    if (!latestCallback.current || value === lastCommittedValue.current) {
      return;
    }
    const timer = window.setTimeout(() => {
      lastCommittedValue.current = value;
      latestCallback.current?.(value);
    }, debounceMs);

    return () => window.clearTimeout(timer);
  }, [value, debounceMs]);

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value);
  };

  const handleClear = () => {
    lastCommittedValue.current = "";
    onChange("");
    latestCallback.current?.("");
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Escape" && value.length > 0) {
      e.preventDefault();
      handleClear();
    }
    props.onKeyDown?.(e);
  };

  return (
    <div className={cn("relative flex min-w-0 items-center", containerClassName)}>
      <Search
        className="pointer-events-none absolute left-3.5 size-4 text-white/45"
        aria-hidden="true"
      />
      <input
        type="search"
        value={value}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        aria-label={ariaLabel}
        className={cn(
          "min-h-[42px] w-full rounded-xl border border-white/12 bg-[#14151c] py-2 pl-10 pr-10 text-base text-white placeholder:text-white/40 transition-colors sm:min-h-[40px] sm:text-sm",
          "hover:border-white/25 focus-visible:border-[#d3fb52]/60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] focus-visible:ring-offset-2 focus-visible:ring-offset-[#14151c]",
          "[&::-webkit-search-cancel-button]:hidden",
          className,
        )}
        {...props}
      />
      {value.length > 0 && (
        <button
          type="button"
          onClick={handleClear}
          aria-label="Clear search"
          className="absolute right-2 inline-flex size-8 cursor-pointer items-center justify-center rounded-md text-white/50 transition-colors hover:bg-white/10 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
        >
          <X className="size-3.5" aria-hidden="true" />
        </button>
      )}
    </div>
  );
}
