/**
 * @date 2026-11-21
 * @purpose Accessible Tabs component with ARIA tablist/tabpanel semantics and keyboard Arrow navigation.
 */
import { useId, useState, type KeyboardEvent, type ReactNode } from "react";
import { cn } from "@/lib/cn";

export interface TabItem {
  id: string;
  label: string;
  icon?: ReactNode;
  badge?: string | number;
  disabled?: boolean;
  content: ReactNode;
}

export interface TabsProps {
  items: readonly TabItem[];
  value?: string;
  defaultValue?: string;
  onValueChange?: (id: string) => void;
  className?: string;
  listClassName?: string;
  tabClassName?: string;
  panelClassName?: string;
}

export function Tabs({
  items,
  value,
  defaultValue,
  onValueChange,
  className,
  listClassName,
  tabClassName,
  panelClassName,
}: TabsProps) {
  const baseId = useId();
  const firstEnabledId = items.find((item) => !item.disabled)?.id ?? items[0]?.id ?? "";
  const [internalValue, setInternalValue] = useState(defaultValue ?? firstEnabledId);
  const activeId = value ?? internalValue;

  const selectTab = (id: string) => {
    if (value === undefined) {
      setInternalValue(id);
    }
    onValueChange?.(id);
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLButtonElement>, currentIndex: number) => {
    const enabledIndices = items
      .map((item, idx) => (item.disabled ? -1 : idx))
      .filter((idx) => idx !== -1);
    if (enabledIndices.length === 0) {
      return;
    }

    const pos = enabledIndices.indexOf(currentIndex);
    let nextIndex: number | undefined;

    if (event.key === "ArrowRight") {
      nextIndex = enabledIndices[(pos + 1) % enabledIndices.length];
    } else if (event.key === "ArrowLeft") {
      nextIndex = enabledIndices[(pos - 1 + enabledIndices.length) % enabledIndices.length];
    } else if (event.key === "Home") {
      nextIndex = enabledIndices[0];
    } else if (event.key === "End") {
      nextIndex = enabledIndices[enabledIndices.length - 1];
    }

    if (nextIndex !== undefined) {
      event.preventDefault();
      const target = items[nextIndex];
      if (target) {
        selectTab(target.id);
        document.getElementById(`${baseId}-tab-${target.id}`)?.focus();
      }
    }
  };

  const activeItem = items.find((item) => item.id === activeId) ?? items[0];

  return (
    <div className={cn("space-y-4", className)}>
      <div
        role="tablist"
        aria-orientation="horizontal"
        className={cn(
          "no-scrollbar flex items-center gap-1 overflow-x-auto border-b border-white/[0.085] pb-px",
          listClassName,
        )}
      >
        {items.map((item, index) => {
          const isSelected = item.id === activeItem?.id;
          return (
            <button
              key={item.id}
              id={`${baseId}-tab-${item.id}`}
              role="tab"
              type="button"
              disabled={item.disabled}
              aria-selected={isSelected}
              aria-controls={`${baseId}-panel-${item.id}`}
              tabIndex={isSelected ? 0 : -1}
              onClick={() => selectTab(item.id)}
              onKeyDown={(e) => handleKeyDown(e, index)}
              className={cn(
                "relative inline-flex min-h-[40px] shrink-0 cursor-pointer items-center gap-2 rounded-t-xl px-3.5 py-2.5 text-sm font-medium transition-colors",
                "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] focus-visible:ring-inset",
                "disabled:cursor-not-allowed disabled:opacity-40",
                isSelected
                  ? "text-[#d3fb52] after:absolute after:inset-x-2 after:bottom-0 after:h-0.5 after:rounded-full after:bg-[#d3fb52]"
                  : "text-white/65 hover:bg-white/5 hover:text-white",
                tabClassName,
              )}
            >
              {item.icon && <span aria-hidden="true">{item.icon}</span>}
              <span>{item.label}</span>
              {item.badge !== undefined && (
                <span
                  className={cn(
                    "rounded-full px-1.5 py-0.5 text-[11px] font-semibold tabular-nums",
                    isSelected ? "bg-[#d3fb52]/15 text-[#d3fb52]" : "bg-white/10 text-white/70",
                  )}
                >
                  {item.badge}
                </span>
              )}
            </button>
          );
        })}
      </div>

      {activeItem && (
        <div
          id={`${baseId}-panel-${activeItem.id}`}
          role="tabpanel"
          aria-labelledby={`${baseId}-tab-${activeItem.id}`}
          tabIndex={0}
          className={cn("focus-visible:outline-none", panelClassName)}
        >
          {activeItem.content}
        </div>
      )}
    </div>
  );
}
