/**
 * @date 2026-11-24
 * @purpose Editorial partner campuses card grid and pagination directory for the TeamUp landing page (#partner-universities).
 */
import { ArrowUpRight, ExternalLink } from "lucide-react";
import type { CSSProperties, RefObject } from "react";
import { Link } from "react-router-dom";
import type { UniversityCatalogItem } from "@/api";
import { Badge, Pagination, UniversityLogo } from "@/components/ui";
import { isSafeHttpUrl } from "@/lib/urlValidation";

export interface LandingPartnerUniversitiesSectionProps {
  sectionRef: RefObject<HTMLDivElement | null>;
  partnersVisible: boolean;
  pagedUniversities: readonly UniversityCatalogItem[];
  safeCampusPage: number;
  totalCampusPages: number;
  totalCampuses: number;
  campusPageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
}

export function LandingPartnerUniversitiesSection({
  sectionRef,
  partnersVisible,
  pagedUniversities,
  safeCampusPage,
  totalCampusPages,
  totalCampuses,
  campusPageSize,
  onPageChange,
  onPageSizeChange,
}: LandingPartnerUniversitiesSectionProps) {
  return (
    <div
      id="partner-universities"
      ref={sectionRef}
      className={`section-reveal-up mx-auto mt-16 max-w-[1280px] rounded-[28px] border border-white/[0.085] bg-[#1b1d26] p-4 sm:p-8 md:p-10 ${
        partnersVisible ? "is-visible" : ""
      }`}
    >
      <div className="flex flex-col justify-between gap-4 border-b border-white/[0.08] pb-6 md:flex-row md:items-end">
        <div className="space-y-2">
          <span className="inline-flex items-center rounded-full border border-[#d3fb52]/35 bg-[#d3fb52]/10 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-[#d3fb52]">
            Verified Campus Network
          </span>
          <h3
            className="text-2xl font-bold tracking-tight text-white sm:text-3xl"
            style={{ fontFamily: "'Plus Jakarta Sans', 'DM Sans', sans-serif" }}
          >
            Partner Universities on TeamUp
          </h3>
          <p className="max-w-2xl text-sm text-[#9ca3af]">
            Explore accredited Egyptian universities and faculties synced directly from the live academic directory.
          </p>
        </div>
        <Link
          to="/auth/register"
          className="inline-flex w-fit max-w-full shrink-0 items-center gap-2 rounded-xl bg-[#d3fb52] px-4 py-2.5 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
        >
          Join With Your Campus Email
          <ArrowUpRight className="size-4" aria-hidden="true" />
        </Link>
      </div>

      <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {pagedUniversities.map((uni, index) => (
          <div
            key={`${safeCampusPage}-${uni.universityId}`}
            style={{ "--reveal-delay": `${index * 65}ms` } as CSSProperties}
            className={`min-w-0 ${partnersVisible ? "campus-card-enter" : "opacity-0"}`}
          >
            <div className="flex h-full min-w-0 flex-col justify-between gap-4 rounded-2xl border border-white/[0.08] bg-[#14151c] p-4 transition-all duration-200 hover:-translate-y-0.5 hover:border-[#d3fb52]/40 sm:p-5">
              <div className="flex min-w-0 items-start gap-3.5">
                <UniversityLogo name={uni.name} logoUrl={uni.logoUrl} size="lg" />
                <div className="min-w-0 flex-1 space-y-1">
                  <h4 className="truncate text-base font-semibold text-white">{uni.name}</h4>
                  <p className="text-xs text-[#9ca3af]">
                    {uni.city}, {uni.country}
                  </p>
                  <div className="flex flex-wrap gap-1.5 pt-1">
                    <Badge variant="accent" size="sm">
                      {uni.faculties.length} {uni.faculties.length === 1 ? "faculty" : "faculties"}
                    </Badge>
                    {uni.faculties.slice(0, 2).map((fac) => (
                      <Badge key={fac.facultyId} variant="default" size="sm">
                        {fac.code}
                      </Badge>
                    ))}
                  </div>
                </div>
              </div>

              <div className="flex flex-wrap items-center justify-between gap-2 border-t border-white/[0.06] pt-3 text-xs">
                <Link
                  to="/auth/register"
                  className="font-semibold text-[#d3fb52] hover:underline"
                >
                  Register from this campus →
                </Link>
                {uni.website && isSafeHttpUrl(uni.website) && (
                  <a
                    href={uni.website}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center gap-1 text-white/70 hover:text-white"
                  >
                    Site
                    <ExternalLink className="size-3" aria-hidden="true" />
                  </a>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>

      <Pagination
        className="mt-6 border-t border-white/[0.08] pt-4"
        page={safeCampusPage}
        totalPages={totalCampusPages}
        totalCount={totalCampuses}
        pageSize={campusPageSize}
        pageSizeOptions={[6, 9, 12]}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
      />
    </div>
  );
}
