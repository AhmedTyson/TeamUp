/**
 * @date 2026-11-24
 * @purpose Events discovery directory with non-clipped date pickers, quick date presets, delivery mode filters, and zero-overlap SpotlightCards.
 */
import { CalendarDays, MapPin, Plus, QrCode, Users } from "lucide-react";
import { useMemo, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import {
  EVENT_DELIVERY_MODES,
  EVENT_TYPES,
  type EventDeliveryMode,
  type EventType,
} from "@/api";
import {
  Badge,
  Button,
  EmptyState,
  ErrorBanner,
  Input,
  Pagination,
  SearchInput,
  Select,
  SkeletonCard,
  SpotlightCard,
  StatusPill,
} from "@/components/ui";
import { useAuth } from "@/features/auth/AuthContext";
import { useOrganizations } from "@/features/organizations";
import { useI18n } from "@/lib/i18n";
import { EventFormModal } from "../components/EventFormModal";
import { useEventMutations, useEvents } from "../hooks/useEventHooks";

const TYPE_OPTIONS = Object.values(EVENT_TYPES).map((value) => ({ value, label: value }));
const DELIVERY_OPTIONS = Object.values(EVENT_DELIVERY_MODES).map((value) => ({
  value,
  label: value,
}));

export function EventsDirectoryPage() {
  const navigate = useNavigate();
  const [params, setParams] = useSearchParams();
  const { t } = useI18n();

  const searchParam = params.get("search") ?? "";
  const typeParam = (params.get("type") ?? "") as EventType | "";
  const deliveryParam = (params.get("deliveryMode") ?? "") as EventDeliveryMode | "";
  const organizationIdParam = params.get("organizationId") ?? "";
  const fromDate = params.get("from") ?? "";
  const toDate = params.get("to") ?? "";
  const page = Math.max(1, Number(params.get("page") ?? 1) || 1);

  const [searchDraft, setSearchDraft] = useState(searchParam);
  const [createOpen, setCreateOpen] = useState(false);

  const { isOrgAdmin, isPlatformAdmin, orgAdminIds } = useAuth();
  const canCreate = isOrgAdmin || isPlatformAdmin;

  const orgsQuery = useOrganizations({ page: 1, pageSize: 100 });
  const orgList = orgsQuery.data?.items ?? [];
  const orgFilterOptions = useMemo(
    () =>
      orgList.map((org) => ({
        value: String(org.organizationId),
        label: org.name,
      })),
    [orgList],
  );

  const { data, isLoading, isError, error, refetch } = useEvents({
    search: searchParam.trim() || undefined,
    type: typeParam || undefined,
    deliveryMode: deliveryParam || undefined,
    organizationId: organizationIdParam ? Number(organizationIdParam) : undefined,
    from: fromDate || undefined,
    to: toDate || undefined,
    page,
    pageSize: 9,
  });
  const mutations = useEventMutations();

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

  const applyDatePreset = (days: number | null) => {
    const next = new URLSearchParams(params);
    next.delete("page");
    if (days === null) {
      next.delete("from");
      next.delete("to");
    } else {
      const start = new Date();
      const end = new Date(Date.now() + days * 24 * 60 * 60 * 1000);
      next.set("from", start.toISOString().slice(0, 10));
      next.set("to", end.toISOString().slice(0, 10));
    }
    setParams(next);
  };

  return (
    <div className="space-y-7">
      <section className="relative overflow-hidden rounded-[28px] border border-white/10 bg-[radial-gradient(ellipse_at_80%_0%,rgba(83,152,134,0.35),transparent_50%),linear-gradient(135deg,#1b1d26,#14151c)] p-6 shadow-2xl sm:p-9">
        <div className="max-w-3xl space-y-3">
          <Badge variant="accent">{t("events.eyebrow")}</Badge>
          <h1
            className="text-4xl font-bold uppercase leading-[0.95] text-[#d3fb52] sm:text-6xl"
            style={{ fontFamily: "var(--font-display)" }}
          >
            {t("events.title")}
          </h1>
          <p className="max-w-2xl text-sm leading-relaxed text-white/70 sm:text-base">
            {t("events.subtitle")}
          </p>
          <div className="flex flex-wrap gap-3 pt-1">
            {canCreate && (
              <Button onClick={() => setCreateOpen(true)} leftIcon={<Plus className="size-4" />}>
                {t("events.create")}
              </Button>
            )}
            <Link
              to="/check-in"
              className="inline-flex h-10 items-center gap-2 rounded-xl border border-[#d3fb52]/35 bg-[#d3fb52]/10 px-4 text-sm font-semibold text-[#d3fb52] transition-colors hover:bg-[#d3fb52]/20"
            >
              <QrCode className="size-4" aria-hidden="true" />
              {t("events.checkIn")}
            </Link>
          </div>
        </div>
      </section>

      <section className="space-y-4 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:p-6">
        <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-4">
          <SearchInput
            containerClassName="md:col-span-2 lg:col-span-2"
            value={searchDraft}
            onChange={setSearchDraft}
            onDebouncedChange={(val) => updateFilter("search", val.trim())}
            placeholder="Search workshops, locations, or topics..."
            aria-label="Search events"
          />

          <Select
            aria-label="Filter by event type"
            placeholder="All event types"
            options={TYPE_OPTIONS}
            value={typeParam}
            onChange={(e) => updateFilter("type", e.target.value)}
          />

          <Select
            aria-label="Filter by delivery mode"
            placeholder="All modes"
            options={DELIVERY_OPTIONS}
            value={deliveryParam}
            onChange={(e) => updateFilter("deliveryMode", e.target.value)}
          />
        </div>

        <div className="grid gap-3 md:grid-cols-3">
          <Select
            aria-label="Filter by organization"
            placeholder="All organizations"
            options={orgFilterOptions}
            value={organizationIdParam}
            onChange={(e) => updateFilter("organizationId", e.target.value)}
          />

          <Input
            aria-label="From date"
            type="date"
            value={fromDate}
            onChange={(e) => updateFilter("from", e.target.value)}
          />

          <Input
            aria-label="To date"
            type="date"
            value={toDate}
            onChange={(e) => updateFilter("to", e.target.value)}
          />
        </div>

        <div className="flex flex-wrap items-center gap-2 border-t border-white/[0.07] pt-3">
          <span className="text-xs font-medium text-white/50">Quick dates:</span>
          <button
            type="button"
            onClick={() => applyDatePreset(null)}
            className={
              !fromDate && !toDate
                ? "cursor-pointer rounded-full bg-[#d3fb52] px-3 py-1 text-xs font-bold text-[#14151c]"
                : "cursor-pointer rounded-full border border-white/12 px-3 py-1 text-xs text-white/70 hover:text-white"
            }
          >
            Any Date
          </button>
          <button
            type="button"
            onClick={() => applyDatePreset(7)}
            className="cursor-pointer rounded-full border border-white/12 px-3 py-1 text-xs text-white/70 hover:border-[#d3fb52]/40 hover:text-white"
          >
            Next 7 Days
          </button>
          <button
            type="button"
            onClick={() => applyDatePreset(30)}
            className="cursor-pointer rounded-full border border-white/12 px-3 py-1 text-xs text-white/70 hover:border-[#d3fb52]/40 hover:text-white"
          >
            Next 30 Days
          </button>
        </div>

        {isLoading ? (
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {Array.from({ length: 6 }, (_, idx) => (
              <SkeletonCard key={idx} />
            ))}
          </div>
        ) : isError ? (
          <ErrorBanner error={error} onRetry={() => void refetch()} />
        ) : data && data.items.length > 0 ? (
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {data.items.map((ev) => {
              const seatsLeft = Math.max(0, ev.capacity - ev.registeredCount);
              return (
                <SpotlightCard
                  key={ev.eventId}
                  showCornerArrow
                  className="flex flex-col rounded-2xl border border-white/[0.085] bg-[#14151c] p-5 transition hover:-translate-y-0.5 hover:border-[#d3fb52]/30"
                >
                  {/* Reserved pe-12 prevents overlap with top-end corner arrow */}
                  <div className="flex flex-wrap items-center justify-between gap-2 pe-12">
                    <div className="flex flex-wrap items-center gap-1.5">
                      <StatusPill status={ev.type} />
                      {ev.deliveryMode && (
                        <Badge variant="outline" size="sm">
                          {ev.deliveryMode}
                        </Badge>
                      )}
                    </div>
                    <span className="text-xs text-white/55">
                      {new Date(ev.startDate).toLocaleDateString()}
                    </span>
                  </div>

                  <div className="mt-4 flex-1 space-y-2">
                    <h2 className="text-lg font-semibold leading-snug text-white">
                      <Link
                        to={`/events/${ev.eventId}`}
                        className="transition-colors hover:text-[#d3fb52]"
                      >
                        {ev.title}
                      </Link>
                    </h2>
                    <p className="text-xs font-medium text-[#539886]">{ev.organizationName}</p>
                    <p className="line-clamp-2 text-sm text-white/65">{ev.description}</p>
                  </div>

                  <div className="mt-4 space-y-2 border-t border-white/10 pt-3 text-xs text-white/60">
                    <p className="inline-flex items-center gap-1.5">
                      <MapPin className="size-3.5 shrink-0" aria-hidden="true" />
                      <span className="truncate">{ev.location}</span>
                    </p>
                    <div className="flex items-center justify-between gap-2">
                      <span className="inline-flex items-center gap-1.5">
                        <Users className="size-3.5" aria-hidden="true" />
                        {ev.registeredCount}/{ev.capacity} ·{" "}
                        {seatsLeft > 0 ? `${seatsLeft} left` : "Waitlist open"}
                      </span>
                      <Link
                        to={`/events/${ev.eventId}`}
                        className="font-semibold text-[#d3fb52] hover:underline"
                      >
                        View details
                      </Link>
                    </div>
                  </div>
                </SpotlightCard>
              );
            })}
          </div>
        ) : (
          <EmptyState
            icon={<CalendarDays className="size-6" />}
            title="No events match your filters"
            description="Try clearing search terms, dates, or selecting another host organization."
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

      <EventFormModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        organizations={orgList}
        managedOrgIds={orgAdminIds}
        isPlatformAdmin={isPlatformAdmin}
        isSubmitting={mutations.create.isPending}
        onSubmitPayload={async (payload) => {
          const created = await mutations.create.mutateAsync(payload);
          setCreateOpen(false);
          toast.success("Event created.");
          navigate(`/events/${created.eventId}`);
        }}
      />
    </div>
  );
}
