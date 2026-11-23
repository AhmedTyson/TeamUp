/**
 * @date 2026-11-22
 * @purpose Campus Leaderboard (/leaderboard) with Top 3 Electric Lime podium cards and paginated ranking table.
 */
import { useQuery } from "@tanstack/react-query";
import { Loader2, Medal, RefreshCw, Trophy } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { gamificationApi, type LeaderboardEntryDto } from "@/api";
import {
  Avatar,
  Badge,
  DataTable,
  EmptyState,
  ErrorBanner,
  Pagination,
  SearchInput,
  type DataTableColumn,
} from "@/components/ui";
import { cn } from "@/lib/cn";
import { queryKeys } from "@/lib/queryKeys";

export function CampusLeaderboardPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const pageSize = 20;

  const leaderboardQuery = useQuery({
    queryKey: queryKeys.gamification.leaderboard({ page, pageSize }),
    queryFn: ({ signal }) => gamificationApi.getLeaderboard({ page, pageSize }, signal),
    staleTime: 60_000,
  });

  const rawItems = leaderboardQuery.data?.items ?? [];
  const totalCount = leaderboardQuery.data?.totalCount ?? 0;
  const totalPages = leaderboardQuery.data?.totalPages ?? 1;

  // Filter client-side if search text provided
  const q = search.trim().toLowerCase();
  const filteredItems = rawItems.filter(
    (item) =>
      !q ||
      `${item.firstName} ${item.lastName}`.toLowerCase().includes(q) ||
      item.universityName.toLowerCase().includes(q) ||
      item.facultyName.toLowerCase().includes(q),
  );

  const top3 = page === 1 && !q ? rawItems.slice(0, 3) : [];
  const tableItems = page === 1 && !q ? filteredItems.slice(3) : filteredItems;

  const columns: readonly DataTableColumn<LeaderboardEntryDto>[] = [
    {
      key: "rank",
      header: "Rank",
      cell: (row) => (
        <span
          className={cn(
            "inline-flex size-7 items-center justify-center rounded-lg font-mono text-xs font-bold tabular-nums",
            row.rank === 1
              ? "bg-[#d3fb52] text-[#14151c]"
              : row.rank === 2
                ? "bg-[#10b981] text-[#14151c]"
                : row.rank === 3
                  ? "bg-[#15733f] text-white"
                  : "bg-white/10 text-white/80",
          )}
        >
          #{row.rank}
        </span>
      ),
    },
    {
      key: "student",
      header: "Student Builder",
      cell: (row) => {
        const fullName = `${row.firstName} ${row.lastName}`;
        return (
          <Link
            to={`/students/${row.studentId}`}
            className="group flex items-center gap-3 focus-visible:outline-none"
          >
            <Avatar
              src={row.photoUrl ?? undefined}
              name={fullName}
              size="sm"
              className="shrink-0 ring-1 ring-white/15 group-hover:ring-[#d3fb52]"
            />
            <div className="min-w-0">
              <p className="font-semibold text-white group-hover:text-[#d3fb52] transition-colors truncate">
                {fullName}
              </p>
              <p className="text-[11px] text-[#9ca3af] truncate">{row.facultyName}</p>
            </div>
          </Link>
        );
      },
    },
    {
      key: "university",
      header: "University",
      cell: (row) => (
        <span className="text-xs text-white/80 font-medium truncate">{row.universityName}</span>
      ),
    },
    {
      key: "level",
      header: "Level",
      cell: (row) => (
        <Badge variant="accent" size="sm">
          Level {row.level}
        </Badge>
      ),
    },
    {
      key: "points",
      header: "Verified XP",
      cell: (row) => (
        <span className="font-mono text-xs font-bold tabular-nums text-[#d3fb52]">
          {row.totalPoints.toLocaleString()} XP
        </span>
      ),
    },
  ];

  return (
    <div className="space-y-8">
      {/* Top Hero Section */}
      <section className="teamup-hero-bg overflow-hidden rounded-[28px] border border-white/12 p-6 text-center shadow-2xl sm:p-10">
        <Badge variant="accent" size="sm">
          Campus Merit &amp; Achievement
        </Badge>
        <h1
          className="mt-3 font-bold uppercase text-[#d3fb52]"
          style={{
            fontFamily: "var(--font-display)",
            fontSize: "clamp(2.5rem, 6.5vw, 5rem)",
            lineHeight: 0.9,
            letterSpacing: "-0.025em",
          }}
        >
          CAMPUS LEADERBOARD
        </h1>
        <p className="mx-auto mt-3 max-w-xl text-sm leading-relaxed text-white/85 sm:text-base">
          Points awarded from completed sprint tasks, QR-verified workshop sessions, and shipped
          campus projects.
        </p>

        {/* Top 3 Podium Cards */}
        {top3.length > 0 && (
          <div className="mx-auto mt-8 grid max-w-4xl grid-cols-1 gap-4 sm:grid-cols-3">
            {/* Rank 2 (Silver / Emerald) */}
            {top3[1] && (
              <div className="order-2 flex flex-col items-center justify-between rounded-2xl border border-white/10 bg-[#1b1d26]/90 p-5 text-center shadow-lg backdrop-blur-md sm:order-1 sm:translate-y-3">
                <div className="relative">
                  <Avatar
                    src={top3[1].photoUrl ?? undefined}
                    name={`${top3[1].firstName} ${top3[1].lastName}`}
                    size="lg"
                    className="ring-4 ring-[#10b981]"
                  />
                  <span className="absolute -bottom-2 left-1/2 -translate-x-1/2 rounded-full bg-[#10b981] px-2 py-0.5 text-[10px] font-black text-[#14151c]">
                    #2
                  </span>
                </div>
                <div className="mt-4 space-y-1">
                  <Link
                    to={`/students/${top3[1].studentId}`}
                    className="block font-bold text-white hover:text-[#d3fb52] transition-colors"
                  >
                    {top3[1].firstName} {top3[1].lastName}
                  </Link>
                  <p className="truncate text-xs text-[#9ca3af]">{top3[1].universityName}</p>
                </div>
                <div className="mt-3 flex items-center gap-2">
                  <Badge variant="success" size="sm">
                    Lvl {top3[1].level}
                  </Badge>
                  <span className="font-mono text-xs font-bold text-[#10b981]">
                    {top3[1].totalPoints.toLocaleString()} XP
                  </span>
                </div>
              </div>
            )}

            {/* Rank 1 (Gold / Lime) */}
            {top3[0] && (
              <div className="order-1 flex flex-col items-center justify-between rounded-2xl border-2 border-[#d3fb52] bg-[#1b1d26] p-6 text-center shadow-[0_0_36px_rgba(211,251,82,0.18)] sm:order-2 sm:-translate-y-2">
                <div className="relative">
                  <Avatar
                    src={top3[0].photoUrl ?? undefined}
                    name={`${top3[0].firstName} ${top3[0].lastName}`}
                    size="lg"
                    className="ring-4 ring-[#d3fb52]"
                  />
                  <span className="absolute -bottom-2 left-1/2 -translate-x-1/2 flex items-center gap-1 rounded-full bg-[#d3fb52] px-2.5 py-0.5 text-xs font-black text-[#14151c]">
                    <Trophy className="size-3" aria-hidden="true" />
                    #1
                  </span>
                </div>
                <div className="mt-4 space-y-1">
                  <Link
                    to={`/students/${top3[0].studentId}`}
                    className="block text-base font-bold text-white hover:text-[#d3fb52] transition-colors"
                  >
                    {top3[0].firstName} {top3[0].lastName}
                  </Link>
                  <p className="truncate text-xs text-[#9ca3af]">{top3[0].universityName}</p>
                </div>
                <div className="mt-3 flex items-center gap-2">
                  <Badge variant="accent" size="sm">
                    Lvl {top3[0].level}
                  </Badge>
                  <span className="font-mono text-sm font-bold text-[#d3fb52]">
                    {top3[0].totalPoints.toLocaleString()} XP
                  </span>
                </div>
              </div>
            )}

            {/* Rank 3 (Bronze / Forest) */}
            {top3[2] && (
              <div className="order-3 flex flex-col items-center justify-between rounded-2xl border border-white/10 bg-[#1b1d26]/90 p-5 text-center shadow-lg backdrop-blur-md sm:translate-y-5">
                <div className="relative">
                  <Avatar
                    src={top3[2].photoUrl ?? undefined}
                    name={`${top3[2].firstName} ${top3[2].lastName}`}
                    size="lg"
                    className="ring-4 ring-[#15733f]"
                  />
                  <span className="absolute -bottom-2 left-1/2 -translate-x-1/2 rounded-full bg-[#15733f] px-2 py-0.5 text-[10px] font-black text-white">
                    #3
                  </span>
                </div>
                <div className="mt-4 space-y-1">
                  <Link
                    to={`/students/${top3[2].studentId}`}
                    className="block font-bold text-white hover:text-[#d3fb52] transition-colors"
                  >
                    {top3[2].firstName} {top3[2].lastName}
                  </Link>
                  <p className="truncate text-xs text-[#9ca3af]">{top3[2].universityName}</p>
                </div>
                <div className="mt-3 flex items-center gap-2">
                  <Badge variant="default" size="sm">
                    Lvl {top3[2].level}
                  </Badge>
                  <span className="font-mono text-xs font-bold text-[#10b981]">
                    {top3[2].totalPoints.toLocaleString()} XP
                  </span>
                </div>
              </div>
            )}
          </div>
        )}
      </section>

      {/* Standings Table Section */}
      <section className="space-y-4 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:p-7">
        <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center border-b border-white/[0.08] pb-4">
          <div>
            <h2
              className="text-lg font-bold text-white"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              All Ranked Student Builders
            </h2>
            <p className="text-xs text-[#9ca3af]">
              Standings update automatically as milestones are delivered and attendances verified.
            </p>
          </div>

          <div className="flex items-center gap-3">
            <SearchInput
              value={search}
              onChange={setSearch}
              placeholder="Search builder or university..."
              containerClassName="w-full sm:w-64"
            />
            <button
              type="button"
              aria-label="Refresh leaderboard"
              onClick={() => void leaderboardQuery.refetch()}
              className="inline-flex size-9 cursor-pointer items-center justify-center rounded-xl border border-white/10 bg-[#14151c] text-white/80 hover:border-[#d3fb52]/40 hover:text-white"
            >
              <RefreshCw
                className={`size-4 ${leaderboardQuery.isFetching ? "animate-spin text-[#d3fb52]" : ""}`}
              />
            </button>
          </div>
        </div>

        {leaderboardQuery.isLoading ? (
          <div className="flex min-h-[260px] flex-col items-center justify-center gap-3 text-sm text-[#9ca3af]">
            <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
            <span>Calculating campus standings…</span>
          </div>
        ) : leaderboardQuery.isError ? (
          <ErrorBanner
            message={
              leaderboardQuery.error instanceof Error
                ? leaderboardQuery.error.message
                : "Unable to load leaderboard standings."
            }
            onRetry={() => void leaderboardQuery.refetch()}
          />
        ) : tableItems.length === 0 ? (
          <EmptyState
            icon={<Medal className="size-6" />}
            title="No leaderboard entries found"
            description="No student rankings matched your search query."
          />
        ) : (
          <div className="space-y-4">
            <DataTable
              columns={columns}
              data={tableItems}
              keyExtractor={(row) => row.studentId}
              caption="Campus leaderboard standings"
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
    </div>
  );
}
