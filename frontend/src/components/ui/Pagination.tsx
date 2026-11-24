/**
 * @date 2026-11-22
 * @purpose Handshake-styled modern Pagination control with first/last page jumps, range telemetry, and dynamic page-size selector.
 */
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from "lucide-react";
import { cn } from "@/lib/cn";
import { Button } from "./Button";

export interface PaginationProps {
  page: number;
  totalPages: number;
  totalCount?: number;
  pageSize?: number;
  pageSizeOptions?: readonly number[];
  onPageChange: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  className?: string;
}

function getVisiblePages(current: number, total: number): Array<number | "ellipsis"> {
  if (total <= 7) {
    return Array.from({ length: total }, (_, idx) => idx + 1);
  }
  const pages: Array<number | "ellipsis"> = [1];
  const start = Math.max(2, current - 1);
  const end = Math.min(total - 1, current + 1);

  if (start > 2) {
    pages.push("ellipsis");
  }
  for (let p = start; p <= end; p += 1) {
    pages.push(p);
  }
  if (end < total - 1) {
    pages.push("ellipsis");
  }
  pages.push(total);
  return pages;
}

export function Pagination({
  page,
  totalPages,
  totalCount,
  pageSize,
  pageSizeOptions,
  onPageChange,
  onPageSizeChange,
  className,
}: PaginationProps) {
  const safeTotalPages = Math.max(1, totalPages);
  const safePage = Math.min(Math.max(1, page), safeTotalPages);
  const pages = getVisiblePages(safePage, safeTotalPages);

  const rangeStart =
    totalCount !== undefined && pageSize !== undefined && totalCount > 0
      ? (safePage - 1) * pageSize + 1
      : 0;
  const rangeEnd =
    totalCount !== undefined && pageSize !== undefined
      ? Math.min(safePage * pageSize, totalCount)
      : 0;

  return (
    <nav
      aria-label="Pagination"
      className={cn(
        "flex flex-col items-center justify-between gap-3 pt-2 sm:flex-row",
        className,
      )}
    >
      <div className="flex flex-wrap items-center justify-center gap-2 text-xs text-[#9ca3af] tabular-nums sm:justify-start sm:gap-3">
        {totalCount !== undefined && pageSize !== undefined ? (
          <span>
            Showing <strong className="text-white">{rangeStart}</strong>–
            <strong className="text-white">{rangeEnd}</strong> of{" "}
            <strong className="text-white">{totalCount}</strong>
          </span>
        ) : (
          <span>
            Page <strong className="text-white">{safePage}</strong> of{" "}
            <strong className="text-white">{safeTotalPages}</strong>
          </span>
        )}

        {pageSizeOptions && pageSizeOptions.length > 0 && onPageSizeChange && pageSize !== undefined && (
          <label className="inline-flex items-center gap-1.5">
            <span>Rows:</span>
            <select
              aria-label="Items per page"
              value={pageSize}
              onChange={(e) => onPageSizeChange(Number(e.target.value))}
              className="rounded-lg border border-white/12 bg-[#14151c] px-2 py-1 text-xs text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              {pageSizeOptions.map((option) => (
                <option key={option} value={option} className="bg-[#1b1d26]">
                  {option}
                </option>
              ))}
            </select>
          </label>
        )}
      </div>

      <div className="flex flex-wrap items-center justify-center gap-1">
        {safeTotalPages > 3 && (
          <Button
            variant="secondary"
            size="sm"
            disabled={safePage <= 1}
            onClick={() => onPageChange(1)}
            aria-label="First page"
            className="hidden sm:inline-flex"
          >
            <ChevronsLeft className="size-3.5" aria-hidden="true" />
          </Button>
        )}

        <Button
          variant="secondary"
          size="sm"
          disabled={safePage <= 1}
          onClick={() => onPageChange(safePage - 1)}
          aria-label="Previous page"
          leftIcon={<ChevronLeft className="size-3.5" aria-hidden="true" />}
        >
          Prev
        </Button>

        {pages.map((item, idx) =>
          item === "ellipsis" ? (
            <span key={`ellipsis-${idx}`} className="px-2 text-xs text-white/45">
              …
            </span>
          ) : (
            <button
              key={item}
              type="button"
              onClick={() => onPageChange(item)}
              aria-current={item === safePage ? "page" : undefined}
              className={cn(
                "inline-flex min-h-9 min-w-8 cursor-pointer items-center justify-center rounded-lg px-2 text-xs font-semibold tabular-nums transition-all duration-150",
                "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                item === safePage
                  ? "bg-[#d3fb52] text-[#14151c] shadow-[0_4px_14px_rgba(211,251,82,0.35)]"
                  : "border border-transparent text-white/75 hover:border-white/15 hover:bg-white/10 hover:text-white",
              )}
            >
              {item}
            </button>
          ),
        )}

        <Button
          variant="secondary"
          size="sm"
          disabled={safePage >= safeTotalPages}
          onClick={() => onPageChange(safePage + 1)}
          aria-label="Next page"
          rightIcon={<ChevronRight className="size-3.5" aria-hidden="true" />}
        >
          Next
        </Button>

        {safeTotalPages > 3 && (
          <Button
            variant="secondary"
            size="sm"
            disabled={safePage >= safeTotalPages}
            onClick={() => onPageChange(safeTotalPages)}
            aria-label="Last page"
            className="hidden sm:inline-flex"
          >
            <ChevronsRight className="size-3.5" aria-hidden="true" />
          </Button>
        )}
      </div>
    </nav>
  );
}
