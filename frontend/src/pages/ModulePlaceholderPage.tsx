/**
 * @date 2026-11-22
 * @purpose Handshake-styled interactive workspace module explorer with search, campus imagery, database university logos, and pagination.
 */
import { useQuery } from "@tanstack/react-query";
import { ArrowLeft, ArrowUpRight, Sparkles } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { universitiesApi, type UniversityCatalogItem } from "@/api";
import { Badge, Pagination, SearchInput, UniversityLogo } from "@/components/ui";

interface ModulePlaceholderPageProps {
  title: string;
  description: string;
  endpoint?: string;
}

const FALLBACK_CAMPUSES: UniversityCatalogItem[] = [
  {
    universityId: 1,
    name: "Cairo University",
    city: "Giza",
    country: "Egypt",
    logoUrl: "https://upload.wikimedia.org/wikipedia/commons/3/3c/Cairo_University_new_logo.png",
    faculties: [{ facultyId: 1, name: "FCAI", code: "FCAI" }],
  },
  {
    universityId: 2,
    name: "Ain Shams University",
    city: "Cairo",
    country: "Egypt",
    logoUrl: "https://upload.wikimedia.org/wikipedia/en/3/3d/Ain_Shams_logo.png",
    faculties: [{ facultyId: 2, name: "CIS", code: "CIS" }],
  },
  {
    universityId: 3,
    name: "Alexandria University",
    city: "Alexandria",
    country: "Egypt",
    logoUrl: "https://upload.wikimedia.org/wikipedia/ar/6/63/%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%A7%D9%84%D8%A5%D8%B3%D9%83%D9%86%D8%AF%D8%B1%D9%8A%D8%A9.png",
    faculties: [{ facultyId: 3, name: "ENG_ALEX", code: "ENG_ALEX" }],
  },
  {
    universityId: 4,
    name: "Mansoura University",
    city: "Mansoura",
    country: "Egypt",
    logoUrl: "https://upload.wikimedia.org/wikipedia/ar/5/5e/%D8%B4%D8%B9%D8%A7%D8%B1_%D8%AC%D8%A7%D9%85%D8%B9%D8%A9_%D8%A7%D9%84%D9%85%D9%86%D8%B5%D9%88%D8%B1%D8%A9.png",
    faculties: [{ facultyId: 4, name: "FCI_MANS", code: "FCI_MANS" }],
  },
  {
    universityId: 5,
    name: "Capital University",
    city: "Cairo",
    country: "Egypt",
    logoUrl: "https://upload.wikimedia.org/wikipedia/commons/c/c1/Capital_University_Logo.png",
    faculties: [{ facultyId: 5, name: "FCAI_CAPU", code: "FCAI_CAPU" }],
  },
  {
    universityId: 6,
    name: "The American University in Cairo (AUC)",
    city: "New Cairo",
    country: "Egypt",
    logoUrl: "https://upload.wikimedia.org/wikipedia/commons/1/1b/AUC_Arabic_Logo_2021.png",
    faculties: [{ facultyId: 6, name: "SSE_AUC", code: "SSE_AUC" }],
  },
];

function resolveBannerImage(title: string): string {
  const lower = title.toLowerCase();
  if (lower.includes("event") || lower.includes("workshop")) {
    return "/images/campus/showcase-events-qr.svg";
  }
  if (lower.includes("leaderboard") || lower.includes("gamification") || lower.includes("score")) {
    return "/images/campus/showcase-verified-badges.svg";
  }
  if (lower.includes("project") || lower.includes("skill")) {
    return "/images/campus/showcase-team-formation.svg";
  }
  return "/images/campus/hero-campus-collage.svg";
}

export function ModulePlaceholderPage({ title, description }: ModulePlaceholderPageProps) {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(6);

  const catalogQuery = useQuery({
    queryKey: ["public", "universities", "catalog"],
    queryFn: ({ signal }) => universitiesApi.getCatalog(signal),
    staleTime: 60_000,
    retry: false,
  });

  const universities =
    catalogQuery.data && catalogQuery.data.length > 0 ? catalogQuery.data : FALLBACK_CAMPUSES;

  const filtered = universities.filter((uni) => {
    const q = search.trim().toLowerCase();
    if (!q) return true;
    return (
      uni.name.toLowerCase().includes(q) ||
      uni.city.toLowerCase().includes(q) ||
      uni.faculties.some((f) => f.name.toLowerCase().includes(q) || f.code.toLowerCase().includes(q))
    );
  });

  const totalCount = filtered.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const safePage = Math.min(page, totalPages);
  const paged = filtered.slice((safePage - 1) * pageSize, safePage * pageSize);

  const bannerSrc = resolveBannerImage(title);

  return (
    <div className="space-y-6">
      <section className="grid items-center gap-6 overflow-hidden rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-6 sm:p-8 lg:grid-cols-12">
        <div className="space-y-4 lg:col-span-7">
          <Link
            to="/"
            className="inline-flex items-center gap-1.5 text-xs font-medium text-white/65 transition-colors hover:text-[#d3fb52]"
          >
            <ArrowLeft className="size-3.5" aria-hidden="true" />
            Back to overview
          </Link>

          <div className="flex flex-wrap items-center gap-2">
            <Badge variant="accent" size="sm">
              <Sparkles className="mr-1 size-3" aria-hidden="true" />
              Campus Workspace Preview
            </Badge>
            <span className="text-xs text-[#9ca3af]">
              Synced with {universities.length} partner universities
            </span>
          </div>

          <h1
            className="text-3xl font-bold tracking-tight text-white sm:text-4xl"
            style={{ fontFamily: "var(--font-heading)" }}
          >
            {title}
          </h1>
          <p className="max-w-2xl text-sm leading-relaxed text-white/80">{description}</p>

          <div className="flex flex-wrap items-center gap-3 pt-1">
            <Link
              to="/projects"
              className="inline-flex items-center gap-1.5 rounded-xl bg-[#d3fb52] px-4 py-2 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
            >
              Explore Active Squads
              <ArrowUpRight className="size-3.5" aria-hidden="true" />
            </Link>
            <Link
              to="/profile/me"
              className="inline-flex items-center gap-1.5 rounded-xl border border-white/15 bg-[#14151c] px-4 py-2 text-xs font-semibold text-white transition-colors hover:border-[#d3fb52]/40 hover:text-[#d3fb52]"
            >
              View Student Profile
            </Link>
          </div>
        </div>

        <div className="hs-photo-frame overflow-hidden bg-[#14151c] lg:col-span-5">
          <img
            src={bannerSrc}
            alt={`${title} visual campus preview`}
            loading="lazy"
            className="h-52 w-full object-cover sm:h-60"
          />
        </div>
      </section>

      <section className="space-y-4 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-6">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2
              className="text-lg font-bold text-white"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              Active Campus Hubs
            </h2>
            <p className="text-xs text-[#9ca3af]">
              Filter verified university chapters participating in {title}.
            </p>
          </div>

          <SearchInput
            value={search}
            onChange={(value) => {
              setSearch(value);
              setPage(1);
            }}
            placeholder="Filter by university, city, or faculty..."
            containerClassName="w-full sm:w-72"
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {paged.map((uni) => (
            <div
              key={uni.universityId}
              className="flex items-start gap-3.5 rounded-2xl border border-white/[0.08] bg-[#14151c] p-4 transition-colors hover:border-[#d3fb52]/35"
            >
              <UniversityLogo name={uni.name} logoUrl={uni.logoUrl} size="md" />
              <div className="min-w-0 space-y-1">
                <p className="truncate text-sm font-semibold text-white">{uni.name}</p>
                <p className="text-xs text-[#9ca3af]">
                  {uni.city}, {uni.country}
                </p>
                <div className="flex flex-wrap gap-1 pt-1">
                  {uni.faculties.slice(0, 3).map((fac) => (
                    <Badge key={fac.facultyId} variant="default" size="sm">
                      {fac.code}
                    </Badge>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>

        {totalCount > 0 && (
          <Pagination
            page={safePage}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={pageSize}
            pageSizeOptions={[6, 12]}
            onPageChange={setPage}
            onPageSizeChange={(nextSize) => {
              setPageSize(nextSize);
              setPage(1);
            }}
          />
        )}
      </section>
    </div>
  );
}
