/**
 * @date 2026-11-22
 * @purpose Read-only Append-Only Score Transactions Ledger table with colored badges for ScoreSourceType.
 */
import { useQuery } from "@tanstack/react-query";
import { History, Loader2 } from "lucide-react";
import { useState } from "react";
import {
  gamificationApi,
  type ScoreSourceType,
  type ScoreTransactionDto,
} from "@/api";
import {
  Badge,
  DataTable,
  EmptyState,
  ErrorBanner,
  Pagination,
  type DataTableColumn,
} from "@/components/ui";
import { formatDateTime } from "@/lib/formatters";
import { queryKeys } from "@/lib/queryKeys";

export interface ScoreLedgerTableProps {
  studentId: number;
}

function renderSourcePill(source: ScoreSourceType) {
  switch (source) {
    case "EventAttendance":
      return <Badge variant="success" size="sm">Event Attendance</Badge>;
    case "TaskCompleted":
      return <Badge variant="accent" size="sm">Task Completed</Badge>;
    case "ProjectDelivered":
      return <Badge variant="default" size="sm">Project Delivered</Badge>;
    case "Manual":
      return <Badge variant="warning" size="sm">Manual Adjustment</Badge>;
    default:
      return <Badge variant="default" size="sm">{source}</Badge>;
  }
}

export function ScoreLedgerTable({ studentId }: ScoreLedgerTableProps) {
  const [page, setPage] = useState(1);
  const pageSize = 15;

  const ledgerQuery = useQuery({
    queryKey: queryKeys.gamification.ledger(studentId),
    queryFn: ({ signal }) =>
      gamificationApi.getScoreTransactions(studentId, { page, pageSize }, signal),
    enabled: studentId > 0,
  });

  const items = ledgerQuery.data?.items ?? [];
  const totalCount = ledgerQuery.data?.totalCount ?? 0;
  const totalPages = ledgerQuery.data?.totalPages ?? 1;

  const columns: readonly DataTableColumn<ScoreTransactionDto>[] = [
    {
      key: "id",
      header: "Tx ID",
      cell: (row) => (
        <span className="font-mono text-xs text-white/50">#{row.transactionId}</span>
      ),
    },
    {
      key: "points",
      header: "Points Awarded",
      cell: (row) => (
        <span
          className={`font-mono text-xs font-bold tabular-nums ${
            row.points >= 0 ? "text-[#d3fb52]" : "text-red-400"
          }`}
        >
          {row.points >= 0 ? `+${row.points}` : row.points} pts
        </span>
      ),
    },
    {
      key: "source",
      header: "Source Type",
      cell: (row) => renderSourcePill(row.sourceType),
    },
    {
      key: "reason",
      header: "Reason & Milestone Description",
      cell: (row) => <span className="text-xs text-white/90">{row.reason}</span>,
    },
    {
      key: "createdAt",
      header: "Timestamp",
      cell: (row) => (
        <span className="font-mono text-xs text-[#9ca3af]">
          {formatDateTime(row.createdAt)}
        </span>
      ),
    },
  ];

  return (
    <section className="space-y-4 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-5 sm:p-7">
      <div className="flex flex-col justify-between gap-2 border-b border-white/[0.08] pb-4 sm:flex-row sm:items-center">
        <div>
          <h2
            className="text-lg font-bold text-white uppercase tracking-wider"
            style={{ fontFamily: "var(--font-heading)" }}
          >
            Append-Only Score Ledger
          </h2>
          <p className="text-xs text-[#9ca3af]">
            Immutable transaction history of verified XP points awarded to your account.
          </p>
        </div>
        <span className="font-mono text-xs text-[#d3fb52] bg-[#d3fb52]/10 border border-[#d3fb52]/30 px-3 py-1 rounded-full w-fit">
          Read-Only Ledger
        </span>
      </div>

      {ledgerQuery.isLoading ? (
        <div className="flex min-h-[200px] flex-col items-center justify-center gap-3 text-sm text-[#9ca3af]">
          <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
          <span>Fetching ledger transactions…</span>
        </div>
      ) : ledgerQuery.isError ? (
        <ErrorBanner
          message={
            ledgerQuery.error instanceof Error
              ? ledgerQuery.error.message
              : "Unable to load score transactions ledger."
          }
          onRetry={() => void ledgerQuery.refetch()}
        />
      ) : items.length === 0 ? (
        <EmptyState
          icon={<History className="size-6" />}
          title="No score transactions recorded yet"
          description="Participate in university events, complete sprint tasks, or check in to workshops to earn your first XP."
        />
      ) : (
        <div className="space-y-4">
          <DataTable
            columns={columns}
            data={items}
            keyExtractor={(row) => row.transactionId}
            caption="Score transactions ledger"
          />

          {totalPages > 1 && (
            <Pagination
              page={page}
              totalPages={totalPages}
              totalCount={totalCount}
              pageSize={pageSize}
              onPageChange={setPage}
            />
          )}
        </div>
      )}
    </section>
  );
}

