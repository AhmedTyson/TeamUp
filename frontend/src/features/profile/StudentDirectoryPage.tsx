/**
 * @date 2026-11-22
 * @purpose Student Directory page with academic year filter, faculty picker, debounced keyword search, and capped <=20 pagination (STRIDE THR-I-01).
 */
import { useQuery } from "@tanstack/react-query";
import { AlertCircle, Filter, GraduationCap, Loader2, RefreshCw } from "lucide-react";
import { useMemo, useState } from "react";
import { ApiClientError, studentsApi, universitiesApi } from "@/api";
import {
  Button,
  EmptyState,
  ErrorBanner,
  Pagination,
  SearchInput,
  Select,
} from "@/components/ui";
import { queryKeys } from "@/lib/queryKeys";
import { StudentCard } from "./StudentCard";

export function StudentDirectoryPage() {
  const [search, setSearch] = useState("");
  const [facultyId, setFacultyId] = useState<number | undefined>(undefined);
  const [academicYear, setAcademicYear] = useState<number | undefined>(undefined);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // Load universities & faculties catalog for dropdown filter
  const catalogQuery = useQuery({
    queryKey: queryKeys.universities.catalog,
    queryFn: ({ signal }) => universitiesApi.getCatalog(signal),
    staleTime: 5 * 60_000,
  });

  const facultyOptions = useMemo(() => {
    const list = catalogQuery.data ?? [];
    const allFaculties: { label: string; value: string }[] = [
      { label: "All Faculties & Departments", value: "" },
    ];
    for (const uni of list) {
      for (const fac of uni.faculties) {
        allFaculties.push({
          label: `${uni.name} — ${fac.name} (${fac.code})`,
          value: String(fac.facultyId),
        });
      }
    }
    return allFaculties;
  }, [catalogQuery.data]);

  const directoryQuery = useQuery({
    queryKey: queryKeys.students.directory({
      search: search.trim() || undefined,
      facultyId,
      academicYear,
      page,
      pageSize,
    }),
    queryFn: async ({ signal }) => {
      return studentsApi.getDirectory(
        {
          search: search.trim() || undefined,
          facultyId,
          academicYear,
          page,
          pageSize,
        },
        signal,
      );
    },
    staleTime: 30_000,
    retry: (failureCount, error) => {
      // Do not retry 429 rate limit
      if (error instanceof ApiClientError && error.isRateLimited) {
        return false;
      }
      return failureCount < 2;
    },
  });

  const isRateLimited =
    directoryQuery.error instanceof ApiClientError && directoryQuery.error.isRateLimited;
  const retryAfter =
    directoryQuery.error instanceof ApiClientError
      ? directoryQuery.error.retryAfterSeconds
      : undefined;

  const data = directoryQuery.data;
  const items = data?.items ?? [];
  const totalPages = data?.totalPages ?? 1;
  const totalCount = data?.totalCount ?? 0;

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <div className="inline-flex items-center gap-1.5 rounded-full border border-[#d3fb52]/30 bg-[#d3fb52]/10 px-2.5 py-0.5 text-xs font-bold text-[#d3fb52]">
            <GraduationCap className="size-3.5" aria-hidden="true" />
            Verified Campus Directory
          </div>
          <h1
            className="mt-2 text-2xl font-bold tracking-tight text-white sm:text-3xl"
            style={{ fontFamily: "var(--font-heading)" }}
          >
            Student Directory
          </h1>
          <p className="mt-1 text-sm text-[#9ca3af]">
            Explore verified student builders across partner Egyptian universities and faculties.
          </p>
        </div>

        <button
          type="button"
          onClick={() => void directoryQuery.refetch()}
          disabled={directoryQuery.isFetching}
          className="inline-flex items-center gap-1.5 rounded-xl border border-white/10 bg-[#1b1d26] px-3 py-2 text-xs font-semibold text-white/80 hover:border-[#d3fb52]/40 hover:text-white transition-colors self-start sm:self-auto cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] disabled:opacity-50"
        >
          <RefreshCw
            className={`size-3.5 ${directoryQuery.isFetching ? "animate-spin text-[#d3fb52]" : ""}`}
            aria-hidden="true"
          />
          Refresh
        </button>
      </div>

      {/* Rate Limit Alert */}
      {isRateLimited && (
        <div
          role="alert"
          className="flex items-start gap-3 rounded-2xl border border-amber-500/35 bg-amber-500/10 p-4 text-sm text-amber-200"
        >
          <AlertCircle className="size-5 shrink-0 text-amber-400 mt-0.5" aria-hidden="true" />
          <div className="space-y-1">
            <p className="font-semibold text-amber-300">Directory Query Rate Limit Reached (429)</p>
            <p className="text-xs text-amber-200/90 leading-relaxed">
              Student directory searches are rate-limited to protect campus member privacy.
              {retryAfter ? ` Please wait ${retryAfter} seconds before requesting more records.` : " Please try again in a few moments."}
            </p>
          </div>
        </div>
      )}

      {/* Filter Toolbar */}
      <div className="grid grid-cols-1 gap-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:grid-cols-12 sm:items-center">
        {/* Keyword Search */}
        <div className="sm:col-span-5">
          <SearchInput
            value={search}
            onChange={(val) => {
              setSearch(val);
              setPage(1);
            }}
            placeholder="Search by student name..."
          />
        </div>

        {/* Faculty Select */}
        <div className="sm:col-span-4">
          <Select
            value={facultyId !== undefined ? String(facultyId) : ""}
            onChange={(e) => {
              const val = e.target.value;
              setFacultyId(val ? Number(val) : undefined);
              setPage(1);
            }}
            options={facultyOptions}
            className="w-full text-xs"
          />
        </div>

        {/* Academic Year Select */}
        <div className="sm:col-span-3">
          <Select
            value={academicYear !== undefined ? String(academicYear) : ""}
            onChange={(e) => {
              const val = e.target.value;
              setAcademicYear(val ? Number(val) : undefined);
              setPage(1);
            }}
            options={[
              { label: "All Academic Years (1–6)", value: "" },
              { label: "Year 1 (Freshman)", value: "1" },
              { label: "Year 2 (Sophomore)", value: "2" },
              { label: "Year 3 (Junior)", value: "3" },
              { label: "Year 4 (Senior)", value: "4" },
              { label: "Year 5", value: "5" },
              { label: "Year 6", value: "6" },
            ]}
            className="w-full text-xs"
          />
        </div>
      </div>

      {/* Results Area */}
      {directoryQuery.isLoading ? (
        <div className="flex min-h-[300px] flex-col items-center justify-center gap-3 text-sm text-[#9ca3af]">
          <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
          <span>Searching verified campus students…</span>
        </div>
      ) : directoryQuery.isError && !isRateLimited ? (
        <ErrorBanner
          message={
            directoryQuery.error instanceof Error
              ? directoryQuery.error.message
              : "Unable to load student directory."
          }
          onRetry={() => void directoryQuery.refetch()}
        />
      ) : items.length === 0 ? (
        <EmptyState
          icon={<Filter className="size-6" />}
          title="No students found"
          description="Try broadening your keyword search, selecting all faculties, or clearing academic year filters."
          action={
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                setSearch("");
                setFacultyId(undefined);
                setAcademicYear(undefined);
                setPage(1);
              }}
            >
              Clear Filters
            </Button>
          }
        />
      ) : (
        <div className="space-y-6">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((student) => (
              <StudentCard key={student.studentId} student={student} />
            ))}
          </div>

          <Pagination
            page={page}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={pageSize}
            pageSizeOptions={[10, 20]}
            onPageChange={setPage}
            onPageSizeChange={(newSize) => {
              // Cap at <= 20
              setPageSize(Math.min(newSize, 20));
              setPage(1);
            }}
          />
        </div>
      )}
    </div>
  );
}
