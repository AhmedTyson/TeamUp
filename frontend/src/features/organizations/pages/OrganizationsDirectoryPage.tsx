/**
 * @date 2026-11-24
 * @purpose Flexible Egyptian Activities & Open Tech Communities Hub with Scope filters, single category strip, university selector, zero-overlap SpotlightCards, and OrgAdmin create modal.
 */
import { Building2, ExternalLink, Layers, MapPin, Plus, Sparkles, Users } from "lucide-react";
import { useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { ORGANIZATION_SCOPES, type OrganizationScope } from "@/api";
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
} from "@/components/ui";
import { useAuth } from "@/features/auth/AuthContext";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { useI18n, type MessageKey } from "@/lib/i18n";
import { OrganizationFormModal } from "../components/OrganizationFormModal";
import { OrganizationMark } from "../components/OrganizationMark";
import { useOrganizationMutations, useOrganizations } from "../hooks/useOrganizationHooks";
import { ORGANIZATION_CATEGORIES } from "../schemas/organizationSchemas";

const SCOPE_FILTERS: readonly { value: string; labelKey: MessageKey; scope?: OrganizationScope; recruitingOnly?: boolean }[] = [
  { value: "", labelKey: "orgs.scope.all" },
  { value: ORGANIZATION_SCOPES.UniversityBranch, labelKey: "orgs.scope.UniversityBranch", scope: ORGANIZATION_SCOPES.UniversityBranch },
  { value: ORGANIZATION_SCOPES.CrossUniversity, labelKey: "orgs.scope.CrossUniversity", scope: ORGANIZATION_SCOPES.CrossUniversity },
  { value: ORGANIZATION_SCOPES.Independent, labelKey: "orgs.scope.Independent", scope: ORGANIZATION_SCOPES.Independent },
  { value: "recruiting", labelKey: "orgs.scope.recruiting", recruitingOnly: true },
];

export function OrganizationsDirectoryPage() {
  const navigate = useNavigate();
  const [params, setParams] = useSearchParams();
  const { t } = useI18n();

  const searchParam = params.get("search") ?? "";
  const category = params.get("category") ?? "";
  const scopeFilter = params.get("scope") ?? "";
  const universityIdParam = params.get("universityId") ?? "";
  const page = Math.max(1, Number(params.get("page") ?? 1) || 1);

  const [searchDraft, setSearchDraft] = useState(searchParam);
  const [createOpen, setCreateOpen] = useState(false);

  const { isOrgAdmin, isPlatformAdmin } = useAuth();
  const canCreate = isOrgAdmin || isPlatformAdmin;
  const { universities } = useUniversityCatalog();

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
          <Badge variant="accent">{t("orgs.eyebrow")}</Badge>
          <h1
            className="text-4xl font-bold uppercase leading-[0.95] text-[#d3fb52] sm:text-6xl"
            style={{ fontFamily: "var(--font-display)" }}
          >
            {t("orgs.title")}
          </h1>
          <p className="max-w-2xl text-sm leading-relaxed text-white/70 sm:text-base">
            {t("orgs.subtitle")}
          </p>
          {canCreate && (
            <div className="pt-1">
              <Button
                onClick={() => setCreateOpen(true)}
                leftIcon={<Plus className="size-4" />}
              >
                {t("orgs.create")}
              </Button>
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
            placeholder={t("orgs.searchPlaceholder")}
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
                {t(item.labelKey)}
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

                <div className="mt-4 flex-1 space-y-2">
                  <h2 className="text-lg font-semibold leading-snug text-white">
                    <Link
                      to={`/organizations/${org.organizationId}`}
                      className="transition-colors hover:text-[#d3fb52]"
                    >
                      {org.name}
                    </Link>
                  </h2>
                  {(org.universityName || org.city || org.scope) && (
                    <p className="inline-flex flex-wrap items-center gap-1.5 text-xs font-medium text-[#539886]">
                      <MapPin className="size-3.5 shrink-0" aria-hidden="true" />
                      <span>
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
    </div>
  );
}
