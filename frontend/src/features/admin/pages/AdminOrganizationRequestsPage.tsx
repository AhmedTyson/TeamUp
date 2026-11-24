/**
 * @date 2026-11-25
 * @purpose PlatformAdmin governance queue for reviewing student organization creation requests, automatically promoting accepted founders to OrgAdmin.
 */
import { Building2, Check, ExternalLink, MapPin, ShieldCheck, UserCheck, X } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import { APPLICATION_STATUSES, type ApplicationStatus } from "@/api";
import {
  Badge,
  Button,
  Card,
  CardContent,
  EmptyState,
  ErrorBanner,
  Input,
  Pagination,
  SearchInput,
  SkeletonCard,
  StatusPill,
} from "@/components/ui";
import { OrganizationMark } from "@/features/organizations/components/OrganizationMark";
import { formatDateTime } from "@/lib/formatters";
import { isSafeHttpUrl } from "@/lib/urlValidation";
import { AdminConsoleLayout } from "../components/AdminConsoleLayout";
import {
  useAdminOrganizationRequestsQuery,
  useReviewOrganizationRequestMutation,
} from "../hooks/useAdminHooks";

const STATUS_TABS: readonly { value: ApplicationStatus | ""; label: string }[] = [
  { value: APPLICATION_STATUSES.Pending, label: "Pending" },
  { value: APPLICATION_STATUSES.Accepted, label: "Approved" },
  { value: APPLICATION_STATUSES.Rejected, label: "Rejected" },
  { value: "", label: "All requests" },
];

export function AdminOrganizationRequestsPage() {
  const [statusFilter, setStatusFilter] = useState<ApplicationStatus | "">(
    APPLICATION_STATUSES.Pending,
  );
  const [searchDraft, setSearchDraft] = useState("");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [notesById, setNotesById] = useState<Record<number, string>>({});
  const [activeReviewId, setActiveReviewId] = useState<number | null>(null);

  const requestsQuery = useAdminOrganizationRequestsQuery({
    status: statusFilter || undefined,
    search: search.trim() || undefined,
    page,
    pageSize: 10,
  });

  const reviewMutation = useReviewOrganizationRequestMutation();

  const handleReview = async (requestId: number, approve: boolean) => {
    setActiveReviewId(requestId);
    try {
      const reviewNotes = notesById[requestId]?.trim() || undefined;
      const updated = await reviewMutation.mutateAsync({
        requestId,
        payload: { approve, reviewNotes },
      });
      toast.success(
        approve
          ? `Approved '${updated.name}' and promoted ${updated.requesterName} to Organization Admin.`
          : `Declined organization request '${updated.name}'.`,
      );
    } catch {
      toast.error("Unable to complete organization request review.");
    } finally {
      setActiveReviewId(null);
    }
  };

  const data = requestsQuery.data;

  return (
    <AdminConsoleLayout
      title="Organization Requests"
      description="Review student proposals to launch campus clubs or open communities. Approving creates the organization and automatically promotes the requester to Organization Admin."
    >
      <div className="flex flex-col gap-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex flex-wrap gap-1.5" role="group" aria-label="Filter by request status">
          {STATUS_TABS.map((tab) => {
            const active = statusFilter === tab.value;
            return (
              <button
                key={tab.value || "all"}
                type="button"
                onClick={() => {
                  setStatusFilter(tab.value);
                  setPage(1);
                }}
                className={
                  active
                    ? "cursor-pointer rounded-xl bg-[#d3fb52] px-3.5 py-1.5 text-xs font-bold text-[#14151c]"
                    : "cursor-pointer rounded-xl border border-white/10 bg-[#14151c] px-3.5 py-1.5 text-xs font-semibold text-white/75 transition-colors hover:border-[#d3fb52]/35 hover:text-white"
                }
              >
                {tab.label}
              </button>
            );
          })}
        </div>

        <SearchInput
          containerClassName="sm:w-72"
          value={searchDraft}
          onChange={setSearchDraft}
          onDebouncedChange={(val) => {
            setSearch(val);
            setPage(1);
          }}
          placeholder="Search activity or founder…"
          aria-label="Search organization requests"
        />
      </div>

      {requestsQuery.isError && (
        <ErrorBanner error={requestsQuery.error} onRetry={() => void requestsQuery.refetch()} />
      )}

      {requestsQuery.isPending ? (
        <div className="grid gap-4">
          <SkeletonCard />
          <SkeletonCard />
        </div>
      ) : data && data.items.length > 0 ? (
        <div className="space-y-4">
          {data.items.map((req) => {
            const isPending = req.status === APPLICATION_STATUSES.Pending;
            const isBusy = reviewMutation.isPending && activeReviewId === req.requestId;

            return (
              <Card key={req.requestId} className="overflow-hidden border-white/[0.09]">
                <CardContent className="space-y-4 p-5 sm:p-6">
                  <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-start">
                    <div className="flex min-w-0 items-start gap-4">
                      <OrganizationMark name={req.name} logoUrl={req.logoUrl} />
                      <div className="min-w-0 space-y-1.5">
                        <div className="flex flex-wrap items-center gap-2">
                          <h2 className="text-lg font-bold text-white">{req.name}</h2>
                          <Badge variant="outline" size="sm">
                            {req.category}
                          </Badge>
                          <Badge variant="default" size="sm">
                            {req.scope}
                          </Badge>
                          <StatusPill status={req.status} />
                        </div>

                        <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-[#9ca3af]">
                          <span className="inline-flex items-center gap-1 text-white/85">
                            <UserCheck className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
                            Founder: <strong className="font-semibold">{req.requesterName}</strong> ({req.requesterEmail})
                          </span>
                          {(req.universityName || req.city) && (
                            <span className="inline-flex items-center gap-1 text-[#539886]">
                              <MapPin className="size-3.5" aria-hidden="true" />
                              {req.universityName ?? req.city}
                            </span>
                          )}
                          <span>Submitted {formatDateTime(req.requestedAt)}</span>
                          {req.websiteUrl && isSafeHttpUrl(req.websiteUrl) && (
                            <a
                              href={req.websiteUrl}
                              target="_blank"
                              rel="noopener noreferrer"
                              className="inline-flex items-center gap-1 text-[#d3fb52] hover:underline"
                            >
                              Website <ExternalLink className="size-3" aria-hidden="true" />
                            </a>
                          )}
                        </div>
                      </div>
                    </div>

                    {req.status === APPLICATION_STATUSES.Accepted && req.createdOrganizationId && (
                      <div className="flex shrink-0 flex-wrap items-center gap-2">
                        <Badge variant="success" size="sm">
                          <ShieldCheck className="me-1 size-3.5" aria-hidden="true" />
                          OrgAdmin Promoted
                        </Badge>
                        <Link
                          to={`/organizations/${req.createdOrganizationId}`}
                          className="inline-flex h-9 items-center gap-1.5 rounded-xl bg-[#d3fb52] px-3.5 text-xs font-bold text-[#14151c] hover:bg-[#deff6e]"
                        >
                          Open Organization
                          <ExternalLink className="size-3.5" aria-hidden="true" />
                        </Link>
                      </div>
                    )}
                  </div>

                  <p className="text-sm leading-relaxed text-white/80">{req.description}</p>

                  {req.justification && (
                    <div className="rounded-xl border border-white/10 bg-[#14151c] px-3.5 py-2.5 text-xs text-white/75">
                      <span className="font-semibold text-white">Founding Justification: </span>
                      {req.justification}
                    </div>
                  )}

                  {isPending ? (
                    <div className="flex flex-col gap-3 border-t border-white/[0.08] pt-4 sm:flex-row sm:items-end">
                      <div className="flex-1">
                        <Input
                          label="Admin review notes (optional)"
                          placeholder="Add approval welcome note or reason if declining…"
                          maxLength={1000}
                          value={notesById[req.requestId] ?? ""}
                          onChange={(e) =>
                            setNotesById((prev) => ({ ...prev, [req.requestId]: e.target.value }))
                          }
                        />
                      </div>
                      <div className="flex shrink-0 flex-wrap gap-2">
                        <Button
                          variant="danger"
                          size="sm"
                          leftIcon={<X className="size-4" />}
                          isLoading={isBusy}
                          onClick={() => void handleReview(req.requestId, false)}
                        >
                          Reject
                        </Button>
                        <Button
                          size="sm"
                          leftIcon={<Check className="size-4" />}
                          isLoading={isBusy}
                          onClick={() => void handleReview(req.requestId, true)}
                        >
                          Approve &amp; Promote to OrgAdmin
                        </Button>
                      </div>
                    </div>
                  ) : (
                    (req.reviewNotes || req.reviewedByName) && (
                      <div className="flex flex-wrap items-center justify-between gap-2 border-t border-white/[0.08] pt-3 text-xs text-[#9ca3af]">
                        <span>
                          {req.reviewedByName ? `Reviewed by ${req.reviewedByName}` : "Reviewed"}
                          {req.reviewedAt ? ` on ${formatDateTime(req.reviewedAt)}` : ""}
                        </span>
                        {req.reviewNotes && (
                          <span className="text-white/80">Notes: {req.reviewNotes}</span>
                        )}
                      </div>
                    )
                  )}
                </CardContent>
              </Card>
            );
          })}

          {data.totalPages > 1 && (
            <Pagination
              page={data.page}
              totalPages={data.totalPages}
              totalCount={data.totalCount}
              pageSize={data.pageSize}
              onPageChange={setPage}
            />
          )}
        </div>
      ) : (
        <EmptyState
          icon={<Building2 className="size-6" />}
          title="No organization requests found"
          description="Student proposals to launch new organizations will appear here for approval."
        />
      )}
    </AdminConsoleLayout>
  );
}
