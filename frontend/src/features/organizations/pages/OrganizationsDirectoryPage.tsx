/**
 * @date 2026-11-24
 * @purpose Flexible Egyptian Activities & Open Tech Communities Hub with Scope filters, single category strip, university selector, zero-overlap SpotlightCards, and OrgAdmin create modal.
 */
import {
  Building2,
  ExternalLink,
  Layers,
  MapPin,
  Plus,
  Send,
  ShieldCheck,
  Sparkles,
  Users,
} from "lucide-react";
import { useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { APPLICATION_STATUSES, ORGANIZATION_SCOPES, type OrganizationScope } from "@/api";
import {
  Badge,
  Button,
  EmptyState,
  ErrorBanner,
  Pagination,
  SearchInput,
  Select,
  SkeletonCard,
  SpotlightCard,
  StatusPill,
} from "@/components/ui";
import { useAuth } from "@/features/auth/AuthContext";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { formatDateTime } from "@/lib/formatters";
import { OrganizationFormModal } from "../components/OrganizationFormModal";
import { OrganizationMark } from "../components/OrganizationMark";
import { OrganizationRequestModal } from "../components/OrganizationRequestModal";
import {
  useMyOrganizationCreationRequests,
  useOrganizationMutations,
  useOrganizations,
} from "../hooks/useOrganizationHooks";
import { ORGANIZATION_CATEGORIES } from "../schemas/organizationSchemas";

const SCOPE_FILTERS: readonly { value: string; label: string; scope?: OrganizationScope; recruitingOnly?: boolean }[] = [
  { value: "", label: "All scopes" },
  { value: ORGANIZATION_SCOPES.UniversityBranch, label: "University Branches", scope: ORGANIZATION_SCOPES.UniversityBranch },
  { value: ORGANIZATION_SCOPES.CrossUniversity, label: "Cross-University", scope: ORGANIZATION_SCOPES.CrossUniversity },
  { value: ORGANIZATION_SCOPES.Independent, label: "Open / Independent", scope: ORGANIZATION_SCOPES.Independent },
  { value: "recruiting", label: "Recruiting Now", recruitingOnly: true },
];

export function OrganizationsDirectoryPage() {
  const navigate = useNavigate();
  const [params, setParams] = useSearchParams();

  const searchParam = params.get("search") ?? "";
  const category = params.get("category") ?? "";
  const scopeFilter = params.get("scope") ?? "";
  const universityIdParam = params.get("universityId") ?? "";
  const page = Math.max(1, Number(params.get("page") ?? 1) || 1);

  const [searchDraft, setSearchDraft] = useState(searchParam);
  const [createOpen, setCreateOpen] = useState(false);
  const [requestOpen, setRequestOpen] = useState(params.get("request") === "1");

  const { isAuthenticated, isPlatformAdmin } = useAuth();
  const canCreate = isPlatformAdmin;
  const canRequest = isAuthenticated && !isPlatformAdmin;
  const { universities } = useUniversityCatalog();
  const myRequestsQuery = useMyOrganizationCreationRequests(canRequest);

  const activeScopeSpec = SCOPE_FILTERS.find((s) => s.value === scopeFilter);

  const { data, isLoading, isError, error, refetch } = useOrganizations({
    search: searchParam.trim() || undefined,
    category: category || undefined,
    scope: activeScopeSpec?.scope,
    recruitingOnly: activeScopeSpec?.recruitingOnly,
    universityId: universityIdParam ? Number(universityIdParam) : undefined,
    page,
    pageSize: 9,
  });
  const mutations = useOrganizationMutations();

  const universityOptions = useMemo(
    () =>
      universities.map((u) => ({
        value: String(u.universityId),
        label: u.name,
      })),
    [universities],
  );

  const updateFilter = (key: string, value: string) => {
    if ((params.get(key) ?? "") === value) {
      return;
    }
    const next = new URLSearchParams(params);
    if (value) {
      next.set(key, value);
    } else {
      next.delete(key);
    }
    if (key !== "page") {
      next.delete("page");
    }
    setParams(next);
  };

  return (
    <div className="space-y-7">
      <section className="relative overflow-hidden rounded-[28px] border border-white/10 bg-[radial-gradient(ellipse_at_80%_0%,rgba(83,152,134,0.35),transparent_50%),linear-gradient(135deg,#1b1d26,#14151c)] p-6 shadow-2xl sm:p-9">
        <div className="max-w-3xl space-y-3">
          <Badge variant="accent">Activities &amp; Open Communities</Badge>
          <h1
            className="break-words text-3xl font-bold uppercase leading-[0.95] text-[#d3fb52] sm:text-5xl lg:text-6xl"
            style={{ fontFamily: "var(--font-display)" }}
          >
            Find your people.
          </h1>
          <p className="max-w-2xl text-sm leading-relaxed text-white/70 sm:text-base">
            Explore university student branches, cross-campus initiatives, and open independent tech communities across Egypt.
          </p>
          {(canCreate || canRequest) && (
            <div className="flex flex-wrap items-center gap-2.5 pt-1">
              {canCreate && (
                <>
                  <Button
                    onClick={() => setCreateOpen(true)}
                    leftIcon={<Plus className="size-4" />}
                  >
                    Create organization
                  </Button>
                  <Link
                    to="/admin/organization-requests"
                    className="inline-flex h-10 items-center gap-2 rounded-xl border border-white/15 bg-white/[0.04] px-4 text-sm font-semibold text-white transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
                  >
                    Review Org Requests
                  </Link>
                </>
              )}
              {canRequest && (
                <Button
                  onClick={() => setRequestOpen(true)}
                  leftIcon={<Send className="size-4" />}
                >
                  Request new organization
                </Button>
              )}
            </div>
          )}
        </div>
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -end-12 -top-12 size-64 rounded-full border border-[#d3fb52]/10"
        />
      </section>

      <section className="space-y-4 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:p-6">
        <div className="flex flex-col gap-3 lg:flex-row lg:items-center">
          <SearchInput
            containerClassName="flex-1"
            value={searchDraft}
            onChange={setSearchDraft}
            onDebouncedChange={(val) => updateFilter("search", val.trim())}
            placeholder="Search communities, chapters, cities, or focus…"
            aria-label="Search organizations"
          />

          <Select
            aria-label="Filter by university"
            placeholder="All universities & cities"
            options={universityOptions}
            value={universityIdParam}
            onChange={(e) => updateFilter("universityId", e.target.value)}
            containerClassName="lg:w-64"
          />
        </div>

        {/* Scope filter strip */}
        <div className="flex flex-wrap gap-2" aria-label="Organization scope filters">
          {SCOPE_FILTERS.map((item) => {
            const active = scopeFilter === item.value;
            return (
              <button
                key={item.value || "all-scopes"}
                type="button"
                aria-pressed={active}
                onClick={() => updateFilter("scope", item.value)}
                className={
                  active
                    ? "cursor-pointer rounded-full bg-[#d3fb52] px-3.5 py-1.5 text-xs font-bold text-[#14151c]"
                    : "cursor-pointer rounded-full border border-white/12 bg-[#14151c] px-3.5 py-1.5 text-xs font-semibold text-white/75 transition-colors hover:border-[#d3fb52]/40 hover:text-white"
                }
              >
                {item.label}
              </button>
            );
          })}
        </div>

        {/* Single category pill strip */}
        <div className="flex flex-wrap gap-1.5 border-t border-white/[0.07] pt-3" aria-label="Organization category filters">
          {["", ...ORGANIZATION_CATEGORIES.slice(0, 7)].map((value) => {
            const active = category === value;
            return (
              <button
                key={value || "all-categories"}
                type="button"
                aria-pressed={active}
                onClick={() => updateFilter("category", value)}
                className={
                  active
                    ? "cursor-pointer rounded-lg border border-[#d3fb52]/40 bg-[#d3fb52]/15 px-3 py-1 text-xs font-semibold text-[#d3fb52]"
                    : "cursor-pointer rounded-lg border border-transparent px-3 py-1 text-xs font-medium text-white/60 transition-colors hover:bg-white/[0.05] hover:text-white"
                }
              >
                {value || "All topics"}
              </button>
            );
          })}
        </div>

        {isLoading ? (
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {Array.from({ length: 6 }, (_, i) => (
              <SkeletonCard key={i} />
            ))}
          </div>
        ) : isError ? (
          <ErrorBanner error={error} onRetry={() => void refetch()} />
        ) : data && data.items.length > 0 ? (
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {data.items.map((org) => (
              <SpotlightCard
                key={org.organizationId}
                showCornerArrow
                className="flex flex-col rounded-2xl border border-white/[0.085] bg-[#14151c] p-5 transition hover:-translate-y-0.5 hover:border-[#d3fb52]/30"
              >
                {/* Reserved pe-12 ensures the corner arrow never overlaps category/recruiting pills */}
                <div className="flex items-start justify-between gap-3 pe-12">
                  <OrganizationMark name={org.name} logoUrl={org.logoUrl} />
                  <div className="flex flex-wrap justify-end gap-1.5">
                    <Badge variant="outline" size="sm">
                      {org.category}
                    </Badge>
                    {org.isRecruiting && (
                      <Badge variant="success" size="sm">
                        <Sparkles className="me-1 size-3" aria-hidden="true" />
                        Recruiting
                      </Badge>
                    )}
                  </div>
                </div>

            <div className="mt-4 min-w-0 flex-1 space-y-2">
              <h2 className="break-words text-lg font-semibold leading-snug text-white">
                <Link
                  to={`/organizations/${org.organizationId}`}
                  className="transition-colors hover:text-[#d3fb52]"
                >
                  {org.name}
                </Link>
              </h2>
              {(org.universityName || org.city || org.scope) && (
                <p className="flex min-w-0 flex-wrap items-center gap-1.5 text-xs font-medium text-[#539886]">
                  <MapPin className="size-3.5 shrink-0" aria-hidden="true" />
                  <span className="min-w-0 break-words">
                    {org.universityName ?? org.city ?? "Egypt Open Community"}
                    {org.scope ? ` · ${org.scope}` : ""}
                  </span>
                </p>
              )}
                  <p className="line-clamp-3 text-sm leading-relaxed text-white/65">
                    {org.description}
                  </p>
                </div>

                <div className="mt-5 flex items-center justify-between gap-2 border-t border-white/10 pt-4">
                  <div className="flex flex-wrap items-center gap-3 text-xs text-white/60">
                    <span className="inline-flex items-center gap-1.5">
                      <Users className="size-3.5" aria-hidden="true" />
                      {org.memberCount} {org.memberCount === 1 ? "member" : "members"}
                    </span>
                    {typeof org.unitCount === "number" && org.unitCount > 0 && (
                      <span className="inline-flex items-center gap-1">
                        <Layers className="size-3.5" aria-hidden="true" />
                        {org.unitCount} units
                      </span>
                    )}
                  </div>
                  <Link
                    to={`/organizations/${org.organizationId}`}
                    className="inline-flex shrink-0 items-center gap-1 text-sm font-semibold text-[#d3fb52] hover:underline"
                  >
                    Explore <ExternalLink className="size-3.5" aria-hidden="true" />
                  </Link>
                </div>
              </SpotlightCard>
            ))}
          </div>
        ) : (
          <EmptyState
            icon={<Building2 className="size-6" />}
            title="No organizations found"
            description="No student chapters or open communities match your current filters."
          />
        )}

        {data && data.totalPages > 1 && (
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            totalCount={data.totalCount}
            pageSize={data.pageSize}
            onPageChange={(nextPage) => updateFilter("page", String(nextPage))}
          />
        )}
      </section>

      <OrganizationFormModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        isSubmitting={mutations.create.isPending}
        onSubmitPayload={async (payload) => {
          const created = await mutations.create.mutateAsync(payload);
          setCreateOpen(false);
          toast.success("Organization created.");
          navigate(`/organizations/${created.organizationId}`);
        }}
      />

      {canRequest && myRequestsQuery.data && myRequestsQuery.data.length > 0 && (
        <section
          aria-label="My organization creation requests"
          className="space-y-3 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:p-6"
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div>
              <h2 className="text-lg font-bold text-white">My Organization Requests</h2>
              <p className="text-xs text-[#9ca3af]">
                Proposals reviewed by Platform Admins. Approved proposals promote you to Organization Admin.
              </p>
            </div>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            {myRequestsQuery.data.map((req) => (
              <div
                key={req.requestId}
                className="flex min-w-0 flex-col justify-between gap-3 rounded-2xl border border-white/10 bg-[#14151c] p-4"
              >
                <div className="min-w-0 space-y-1.5">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <span className="min-w-0 break-words font-semibold text-white">{req.name}</span>
                    <StatusPill status={req.status} />
                  </div>
                  <p className="break-words text-xs text-[#9ca3af]">
                    {req.category} · {req.scope} · Submitted {formatDateTime(req.requestedAt)}
                  </p>
                  {req.reviewNotes && (
                    <p className="rounded-lg border border-white/10 bg-white/[0.03] px-2.5 py-1.5 text-xs text-white/80">
                      Admin note: {req.reviewNotes}
                    </p>
                  )}
                </div>

                <div className="flex flex-wrap items-center justify-between gap-2 border-t border-white/[0.07] pt-2.5">
                  {req.status === APPLICATION_STATUSES.Pending && (
                    <Button
                      variant="ghost"
                      size="sm"
                      isLoading={mutations.withdrawCreationRequest.isPending}
                      onClick={async () => {
                        await mutations.withdrawCreationRequest.mutateAsync(req.requestId);
                        toast.info("Organization request withdrawn.");
                      }}
                    >
                      Withdraw request
                    </Button>
                  )}
                  {req.status === APPLICATION_STATUSES.Accepted && req.createdOrganizationId && (
                    <div className="flex w-full flex-wrap items-center justify-between gap-2">
                      <Badge variant="success" size="sm">
                        <ShieldCheck className="me-1 size-3" aria-hidden="true" />
                        OrgAdmin Promoted
                      </Badge>
                      <Link
                        to={`/organizations/${req.createdOrganizationId}`}
                        className="inline-flex items-center gap-1 text-xs font-semibold text-[#d3fb52] hover:underline"
                      >
                        Open Organization <ExternalLink className="size-3" aria-hidden="true" />
                      </Link>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </section>
      )}

      <OrganizationRequestModal
        open={requestOpen}
        onClose={() => {
          setRequestOpen(false);
          if (params.get("request")) {
            const next = new URLSearchParams(params);
            next.delete("request");
            setParams(next, { replace: true });
          }
        }}
        isSubmitting={mutations.submitCreationRequest.isPending}
        onSubmitProposal={async (payload) => {
          await mutations.submitCreationRequest.mutateAsync(payload);
          setRequestOpen(false);
          if (params.get("request")) {
            const next = new URLSearchParams(params);
            next.delete("request");
            setParams(next, { replace: true });
          }
          toast.success(
            "Organization request submitted! Once accepted by an admin, you will automatically become Organization Admin.",
          );
        }}
      />
    </div>
  );
}
