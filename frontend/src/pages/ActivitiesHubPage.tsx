/**
 * @date 2026-11-22
 * @purpose Handshake Students-archetype Student Activities & Organizations directory featuring a full-bleed photographic banner with lime display typography and paginated campus chapters.
 */
import { ArrowUpRight, Users } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { Badge, Pagination, SearchInput, UniversityLogo } from "@/components/ui";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { cn } from "@/lib/cn";

export interface ActivitiesHubPageProps {
  mode?: "directory" | "detail";
}

type ChapterCategory =
  | "All Chapters"
  | "Technical Societies"
  | "Developer Guilds"
  | "Robotics & Hardware"
  | "Design & Business";

interface StudentChapterItem {
  id: string;
  name: string;
  university: string;
  category: Exclude<ChapterCategory, "All Chapters">;
  summary: string;
  openRoles: number;
  membersLabel: string;
  xpBonus: number;
}

const CHAPTER_CATEGORIES: readonly ChapterCategory[] = [
  "All Chapters",
  "Technical Societies",
  "Developer Guilds",
  "Robotics & Hardware",
  "Design & Business",
];

const STUDENT_CHAPTERS: readonly StudentChapterItem[] = [
  {
    id: "ieee-cu",
    name: "IEEE Computer Society · Student Branch",
    university: "Cairo University",
    category: "Technical Societies",
    summary: "Weekly system design workshops, competitive programming tracks, and annual graduation project showcases.",
    openRoles: 6,
    membersLabel: "340+ Members",
    xpBonus: 150,
  },
  {
    id: "gdg-asu",
    name: "GDG on Campus · Cloud & AI Chapter",
    university: "Ain Shams University",
    category: "Developer Guilds",
    summary: "Hands-on study jams in Google Cloud, Android Kotlin, Flutter, and Retrieval-Augmented AI engineering.",
    openRoles: 4,
    membersLabel: "410+ Members",
    xpBonus: 150,
  },
  {
    id: "capital-robotics",
    name: "Capital Robotics & Autonomous Systems Lab",
    university: "Capital University",
    category: "Robotics & Hardware",
    summary: "ROS2 navigation squads, PCB fabrication clinics, and autonomous aerial/rover field competitions.",
    openRoles: 5,
    membersLabel: "185+ Builders",
    xpBonus: 200,
  },
  {
    id: "acm-alex",
    name: "ACM Student Chapter · Problem Solving",
    university: "Alexandria University",
    category: "Technical Societies",
    summary: "ECPC & ICPC regional coaching groups, weekly mock contests, and algorithmic graph theory mentorship.",
    openRoles: 8,
    membersLabel: "520+ Members",
    xpBonus: 150,
  },
  {
    id: "guc-design-craft",
    name: "Design Craft & Product Research Guild",
    university: "German University in Cairo (GUC)",
    category: "Design & Business",
    summary: "Interface critique sessions, Figma component libraries, and usability testing partnerships with campus tech teams.",
    openRoles: 3,
    membersLabel: "210+ Designers",
    xpBonus: 150,
  },
  {
    id: "enactus-auc",
    name: "Enactus Campus Impact Venture Studio",
    university: "The American University in Cairo (AUC)",
    category: "Design & Business",
    summary: "Student social entrepreneurship teams turning engineering sustainability ideas into community startups.",
    openRoles: 5,
    membersLabel: "260+ Innovators",
    xpBonus: 175,
  },
  {
    id: "mansoura-opensource",
    name: "Mansoura Open Source & Security Collective",
    university: "Mansoura University",
    category: "Developer Guilds",
    summary: "Capture-the-Flag workshops, Linux packaging clinics, and open-source pull request sprints.",
    openRoles: 4,
    membersLabel: "290+ Builders",
    xpBonus: 160,
  },
];

export function ActivitiesHubPage({ mode = "directory" }: ActivitiesHubPageProps) {
  const { resolveLogo } = useUniversityCatalog();
  const [category, setCategory] = useState<ChapterCategory>("All Chapters");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 6;

  const q = search.trim().toLowerCase();
  const filtered = STUDENT_CHAPTERS.filter((ch) => {
    const matchesCat = category === "All Chapters" || ch.category === category;
    const matchesQ =
      !q ||
      ch.name.toLowerCase().includes(q) ||
      ch.university.toLowerCase().includes(q) ||
      ch.summary.toLowerCase().includes(q);
    return matchesCat && matchesQ;
  });

  const totalCount = filtered.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const safePage = Math.min(page, totalPages);
  const paged = filtered.slice((safePage - 1) * pageSize, safePage * pageSize);

  return (
    <div className="space-y-8">
      {/* Handshake /students/ Archetype: Full-Bleed Campus Photography + Lime Antonio Display Type */}
      <section className="relative flex min-h-[340px] items-end overflow-hidden rounded-[28px] border border-white/12 bg-[#14151c] p-5 shadow-2xl sm:min-h-[400px] sm:p-10">
        <img
          src="/images/hero/hero-club-meetup.jpg"
          alt="Egyptian university club members brainstorming outdoors around laptops and robotics hardware"
          loading="eager"
          decoding="async"
          className="absolute inset-0 h-full w-full object-cover object-center"
        />
        <div
          aria-hidden="true"
          className="absolute inset-0 bg-gradient-to-t from-[#14151c]/95 via-[#14151c]/55 to-black/25"
        />

        <div className="relative z-10 flex w-full flex-col justify-between gap-6 lg:flex-row lg:items-end">
          <div className="min-w-0 space-y-3">
            <span className="inline-block rounded-full border border-white/20 bg-black/40 px-3 py-1 text-xs font-semibold text-white backdrop-blur-md">
              {mode === "detail" ? "Featured Chapter Workspace" : "50+ Accredited Student Organizations"}
            </span>
            <h1
              className="font-bold uppercase text-[#d3fb52]"
              style={{
                fontFamily: "var(--font-display)",
                fontSize: "clamp(2.4rem, 6.8vw, 5.25rem)",
                lineHeight: 0.9,
                letterSpacing: "-0.025em",
              }}
            >
              SHOW UP.
              <br />
              BUILD TOGETHER.
            </h1>
            <p className="max-w-[540px] text-sm leading-relaxed text-white/90 sm:text-base">
              Join engineering chapters, developer guilds, and creative student societies active
              across Egypt&apos;s partner universities.
            </p>
          </div>

          <Link
            to="/projects"
            className="inline-flex h-11 shrink-0 items-center gap-2 rounded-full bg-[#d3fb52] px-6 text-sm font-bold text-[#14151c] shadow-lg transition-colors hover:bg-[#deff6e]"
          >
            <span>Browse Project Squads</span>
            <ArrowUpRight className="size-4" aria-hidden="true" />
          </Link>
        </div>
      </section>

      {/* Filterable Chapters Grid */}
      <section className="space-y-6 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:p-7">
        <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-center">
          <div className="flex flex-wrap items-center gap-2">
            {CHAPTER_CATEGORIES.map((cat) => {
              const active = category === cat;
              return (
                <button
                  key={cat}
                  type="button"
                  onClick={() => {
                    setCategory(cat);
                    setPage(1);
                  }}
                  className={cn(
                    "cursor-pointer rounded-full px-3.5 py-1.5 text-xs font-semibold transition-colors",
                    active
                      ? "bg-[#d3fb52] text-[#14151c]"
                      : "border border-white/12 bg-[#14151c] text-white/75 hover:border-white/30 hover:text-white",
                  )}
                >
                  {cat}
                </button>
              );
            })}
          </div>

          <SearchInput
            value={search}
            onChange={(val) => {
              setSearch(val);
              setPage(1);
            }}
            placeholder="Search chapters, IEEE, GDG, Capital..."
            containerClassName="w-full lg:w-80"
          />
        </div>

        <div className="grid grid-cols-1 gap-5 md:grid-cols-2 lg:grid-cols-3">
          {paged.map((chapter) => (
            <article
              key={chapter.id}
              className="flex min-w-0 flex-col justify-between gap-4 rounded-2xl border border-white/[0.085] bg-[#14151c] p-4 transition-colors hover:border-[#d3fb52]/40 sm:p-5"
            >
              <div className="space-y-3">
                <div className="flex items-start justify-between gap-2">
                  <div className="flex min-w-0 flex-1 items-center gap-3">
                    <UniversityLogo
                      name={chapter.university}
                      logoUrl={resolveLogo(chapter.university)}
                      size="md"
                    />
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-xs font-semibold text-white">
                        {chapter.university}
                      </p>
                      <p className="text-[11px] text-[#9ca3af]">{chapter.category}</p>
                    </div>
                  </div>
                  <span className="shrink-0">
                    <Badge variant="success" size="sm">
                      {chapter.openRoles} roles
                    </Badge>
                  </span>
                </div>

                <h2
                  className="text-base font-bold leading-snug text-white"
                  style={{ fontFamily: "var(--font-heading)" }}
                >
                  {chapter.name}
                </h2>
                <p className="text-xs leading-relaxed text-[#9ca3af]">{chapter.summary}</p>
              </div>

              <div className="flex flex-wrap items-center justify-between gap-2 border-t border-white/[0.08] pt-3">
                <span className="inline-flex items-center gap-1.5 text-xs text-white/75">
                  <Users className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
                  {chapter.membersLabel}
                </span>
                <Link
                  to={`/organizations/${chapter.id}`}
                  className="inline-flex items-center gap-1 rounded-xl bg-[#d3fb52] px-3.5 py-1.5 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
                >
                  <span>Join Chapter (+{chapter.xpBonus} XP)</span>
                </Link>
              </div>
            </article>
          ))}
        </div>

        {totalPages > 1 && (
          <Pagination
            page={safePage}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={pageSize}
            onPageChange={setPage}
          />
        )}
      </section>
    </div>
  );
}
