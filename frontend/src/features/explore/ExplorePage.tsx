/**
 * @date 2026-11-25
 * @purpose Unified discovery feed (Handshake Explore pattern): curated rails mixing recruiting organizations,
 * upcoming events, and fresh projects from the live APIs, so discovery is data-driven instead of three siloed
 * directories. Every rail links to its full directory for filtering.
 */
import { useQuery, type UseQueryResult } from "@tanstack/react-query";
import { CalendarDays, Compass, FolderKanban, Users } from "lucide-react";
import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { projectsApi, type EventSummary, type OrganizationSummary, type PagedResult, type ProjectListItemDto } from "@/api";
import { Card, CoverImage, EmptyState, ErrorBanner, SkeletonCard } from "@/components/ui";
import { formatDateTime } from "@/lib/formatters";
import { queryKeys } from "@/lib/queryKeys";
import { useEvents } from "@/features/events/hooks/useEventHooks";
import { useOrganizations } from "@/features/organizations/hooks/useOrganizationHooks";

const RAIL_SIZE = 6;

function RailHeader({ icon, title, to }: { icon: ReactNode; title: string; to: string }) {
  return (
    <div className="flex items-center justify-between gap-3">
      <h2 className="flex items-center gap-2 text-base font-semibold text-white">
        {icon}
        {title}
      </h2>
      <Link
        to={to}
        className="rounded-lg px-2.5 py-1 text-xs font-semibold text-[#d3fb52] no-underline transition-colors hover:bg-[#d3fb52]/12 hover:text-[#e6ff8a]"
      >
        View all
      </Link>
    </div>
  );
}

interface DiscoveryRailProps<T> {
  ariaLabel: string;
  icon: ReactNode;
  title: string;
  viewAllTo: string;
  query: UseQueryResult<PagedResult<T>, unknown>;
  emptyTitle: string;
  emptyDescription: string;
  getKey: (item: T) => number | string;
  renderCard: (item: T) => ReactNode;
}

function DiscoveryRail<T>({
  ariaLabel,
  icon,
  title,
  viewAllTo,
  query,
  emptyTitle,
  emptyDescription,
  getKey,
  renderCard,
}: DiscoveryRailProps<T>) {
  const items = query.data?.items ?? [];

  return (
    <section aria-label={ariaLabel} className="space-y-3">
      <RailHeader icon={icon} title={title} to={viewAllTo} />
      {query.isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 3 }, (_, i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      ) : query.isError ? (
        <ErrorBanner error={query.error} onRetry={() => void query.refetch()} />
      ) : items.length === 0 ? (
        <EmptyState title={emptyTitle} description={emptyDescription} />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {items.map((item) => (
            <div key={getKey(item)}>{renderCard(item)}</div>
          ))}
        </div>
      )}
    </section>
  );
}

function OrganizationRailCard({ org }: { org: OrganizationSummary }) {
  return (
    <Link to={`/organizations/${org.organizationId}`} className="block">
      <Card className="h-full overflow-hidden transition-colors hover:border-[#d3fb52]/40">
        <CoverImage url={org.coverImageUrl} alt={`${org.name} cover`} className="aspect-[2/1]" />
        <div className="space-y-2 p-4">
          <p className="line-clamp-1 text-sm font-semibold text-white">{org.name}</p>
          <p className="line-clamp-2 text-xs text-white/60">{org.description}</p>
          <p className="text-[11px] font-medium text-[#d3fb52]">
            {org.category} · {org.memberCount} members
          </p>
        </div>
      </Card>
    </Link>
  );
}

function EventRailCard({ event }: { event: EventSummary }) {
  return (
    <Link to={`/events/${event.eventId}`} className="block">
      <Card className="h-full overflow-hidden transition-colors hover:border-[#d3fb52]/40">
        <CoverImage url={event.coverImageUrl} alt={`${event.title} cover`} className="aspect-[2/1]" />
        <div className="space-y-2 p-4">
          <p className="line-clamp-1 text-sm font-semibold text-white">{event.title}</p>
          <p className="line-clamp-1 text-xs text-white/60">{event.organizationName}</p>
          <p className="text-[11px] font-medium text-[#d3fb52]">
            {formatDateTime(event.startDate)} · {event.registeredCount}/{event.capacity} seats
          </p>
        </div>
      </Card>
    </Link>
  );
}

function ProjectRailCard({ project }: { project: ProjectListItemDto }) {
  return (
    <Card className="group relative h-full cursor-pointer overflow-hidden transition-all duration-200 hover:-translate-y-0.5 hover:border-[#d3fb52]/40">
      <CoverImage url={project.coverImageUrl} alt={`${project.title} cover`} className="aspect-[2/1]" />
      <div className="space-y-2 p-4">
        <p className="line-clamp-1 text-sm font-semibold text-white">
          <Link
            to={`/projects/${project.projectId}`}
            className="transition-colors group-hover:text-[#d3fb52] after:absolute after:inset-0 after:content-[''] focus-visible:outline-none"
          >
            {project.title}
          </Link>
        </p>
        <p className="line-clamp-2 text-xs text-white/60">{project.description}</p>
        <p className="text-[11px] font-medium text-[#d3fb52]">
          {project.teamSize}/{project.maxTeamSize} teammates · by{" "}
          <Link
            to={`/students/${project.creatorId}`}
            className="relative z-10 no-underline transition-colors hover:text-white"
          >
            {project.creatorName}
          </Link>
        </p>
      </div>
    </Card>
  );
}

export function ExplorePage() {
  const recruiting = useOrganizations({ recruitingOnly: true, pageSize: RAIL_SIZE, page: 1 });
  const events = useEvents({ pageSize: RAIL_SIZE, page: 1 });
  const projects = useQuery({
    queryKey: queryKeys.projects.list({ pageSize: RAIL_SIZE, page: 1 }),
    queryFn: ({ signal }) => projectsApi.list({ pageSize: RAIL_SIZE, page: 1 }, signal),
  });

  return (
    <div className="space-y-8">
      <header className="space-y-2">
        <span className="inline-flex items-center gap-1.5 rounded-full border border-[#d3fb52]/40 bg-[#d3fb52]/15 px-2.5 py-1 text-xs font-medium text-[#d3fb52]">
          <Compass className="size-3.5" aria-hidden="true" />
          Discover
        </span>
        <h1 className="text-2xl font-bold text-white sm:text-3xl">Explore what's happening</h1>
        <p className="max-w-2xl text-sm text-white/60">
          Live picks from every corner of TeamUp: communities recruiting now, events coming up, and
          projects looking for teammates. Everything below comes straight from the live directories.
        </p>
      </header>

      <DiscoveryRail
        ariaLabel="Organizations recruiting now"
        icon={<Users className="size-4 text-[#d3fb52]" aria-hidden="true" />}
        title="Recruiting now"
        viewAllTo="/organizations?scope=recruiting"
        query={recruiting}
        emptyTitle="No open recruitment right now"
        emptyDescription="When organizations open a recruitment season they appear here automatically."
        getKey={(org) => org.organizationId}
        renderCard={(org) => <OrganizationRailCard org={org} />}
      />

      <DiscoveryRail
        ariaLabel="Upcoming events"
        icon={<CalendarDays className="size-4 text-[#d3fb52]" aria-hidden="true" />}
        title="Upcoming events"
        viewAllTo="/events"
        query={events}
        emptyTitle="No events on the calendar yet"
        emptyDescription="New events from every organization show up here as soon as they are published."
        getKey={(event) => event.eventId}
        renderCard={(event) => <EventRailCard event={event} />}
      />

      <DiscoveryRail
        ariaLabel="Fresh projects"
        icon={<FolderKanban className="size-4 text-[#d3fb52]" aria-hidden="true" />}
        title="Projects looking for teammates"
        viewAllTo="/projects"
        query={projects}
        emptyTitle="No open projects yet"
        emptyDescription="New team projects appear here the moment they are created."
        getKey={(project) => project.projectId}
        renderCard={(project) => <ProjectRailCard project={project} />}
      />
    </div>
  );
}
