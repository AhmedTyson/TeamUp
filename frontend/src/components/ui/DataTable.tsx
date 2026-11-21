/**
 * @date 2026-11-21
 * @purpose Generic responsive DataTable primitive with loading skeletons, empty state, and optional row click handling.
 */
import type { ReactNode } from "react";
import { cn } from "@/lib/cn";
import { Skeleton } from "./Skeleton";

export interface DataTableColumn<T> {
  key: string;
  header: ReactNode;
  cell: (row: T, index: number) => ReactNode;
  headerClassName?: string;
  cellClassName?: string;
}

export interface DataTableProps<T> {
  columns: readonly DataTableColumn<T>[];
  data: readonly T[];
  keyExtractor: (row: T, index: number) => string | number;
  isLoading?: boolean;
  loadingRows?: number;
  emptyState?: ReactNode;
  onRowClick?: (row: T) => void;
  className?: string;
  caption?: string;
}

export function DataTable<T>({
  columns,
  data,
  keyExtractor,
  isLoading = false,
  loadingRows = 5,
  emptyState,
  onRowClick,
  className,
  caption,
}: DataTableProps<T>) {
  if (!isLoading && data.length === 0 && emptyState) {
    return <>{emptyState}</>;
  }

  return (
    <div
      className={cn(
        "overflow-hidden rounded-2xl border border-white/[0.085] bg-[#1b1d26]",
        className,
      )}
    >
      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-left text-sm">
          {caption && <caption className="sr-only">{caption}</caption>}
          <thead>
            <tr className="border-b border-white/[0.085] bg-[#14151c]/70">
              {columns.map((column) => (
                <th
                  key={column.key}
                  scope="col"
                  className={cn(
                    "px-4 py-3.5 text-xs font-semibold uppercase tracking-wider text-[#9ca3af] whitespace-nowrap",
                    column.headerClassName,
                  )}
                >
                  {column.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-white/[0.06]">
            {isLoading ? (
              Array.from({ length: loadingRows }).map((_, rowIdx) => (
                <tr key={`skeleton-row-${rowIdx}`}>
                  {columns.map((column) => (
                    <td key={`${column.key}-sk-${rowIdx}`} className="px-4 py-3.5">
                      <Skeleton className="h-4 w-3/4" />
                    </td>
                  ))}
                </tr>
              ))
            ) : data.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="px-4 py-10 text-center text-sm text-[#9ca3af]">
                  No records to display.
                </td>
              </tr>
            ) : (
              data.map((row, index) => (
                <tr
                  key={keyExtractor(row, index)}
                  onClick={onRowClick ? () => onRowClick(row) : undefined}
                  className={cn(
                    "transition-colors hover:bg-[#222532]/75",
                    onRowClick && "cursor-pointer",
                  )}
                >
                  {columns.map((column) => (
                    <td
                      key={column.key}
                      className={cn("px-4 py-3.5 text-white/90", column.cellClassName)}
                    >
                      {column.cell(row, index)}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
