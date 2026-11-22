/**
 * @date 2026-11-22
 * @purpose Handshake Rosetta-locked brand hero demo (/demo/hero) combining Antonio typography and campus emerald-obsidian mesh with studio student portraits, offset white wireframes, floating hero campus icons, and live DB official university emblems.
 */
import {
  ArrowRight,
  ArrowUpRight,
  Award,
  CalendarCheck,
  Compass,
  Search,
  Sparkles,
  Users,
  type LucideIcon,
} from "lucide-react";
import { useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  buildStudentStageCards,
  CampusEmblemCluster,
  HeroCampusIcons,
  StudentPortraitCard,
} from "@/components/home/StudentPortraitHero";
import { UniversityLogo } from "@/components/ui";
import { usePublicPeopleShowcase } from "@/features/catalog/usePublicPeopleShowcase";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { cn } from "@/lib/cn";

type HeroLayoutConcept = "orbit" | "stage";

interface EditorialOrbitTile {
  id: string;
  imageSrc: string;
  imageAlt: string;
  university: string;
  caption: string;
  href: string;
  aspectClass: string;
  objectPositionClass: string;
  tiltClass: string;
  wireframeOffsetClass: string;
  Icon: LucideIcon;
}

const ORBIT_TILES: readonly EditorialOrbitTile[] = [
  {
    id: "squad-portrait",
    imageSrc: "/images/people/salma-khaled.jpg",
    imageAlt: "Salma Khaled — Cairo University student portrait",
    university: "Cairo University",
    caption: "Salma · FCAI Squad",
    href: "/projects",
    aspectClass: "aspect-[4/5]",
    objectPositionClass: "object-top",
    tiltClass: "-rotate-3 hover:rotate-0",
    wireframeOffsetClass: "-translate-x-2 -translate-y-2",
    Icon: Users,
  },
  {
    id: "club-meetup",
    imageSrc: "/images/hero/hero-club-meetup.jpg",
    imageAlt: "Student club members brainstorming hardware and software ideas in a campus courtyard",
    university: "Capital University",
    caption: "50+ Campus Clubs",
    href: "/organizations",
    aspectClass: "aspect-[4/3]",
    objectPositionClass: "object-center",
    tiltClass: "rotate-2 xl:translate-x-4 hover:rotate-0",
    wireframeOffsetClass: "translate-x-2 -translate-y-2",
    Icon: Sparkles,
  },
  {
    id: "workshop-hall",
    imageSrc: "/images/hero/hero-workshop-hall.jpg",
    imageAlt: "Students attending a hands-on engineering workshop in a glass-walled campus auditorium",
    university: "Ain Shams University",
    caption: "Workshops · +100 XP",
    href: "/events",
    aspectClass: "aspect-[4/3]",
    objectPositionClass: "object-center",
    tiltClass: "rotate-3 hover:rotate-0",
    wireframeOffsetClass: "translate-x-2 -translate-y-2",
    Icon: CalendarCheck,
  },
  {
    id: "portfolio-portrait",
    imageSrc: "/images/people/omar-mostafa.jpg",
    imageAlt: "Omar Mostafa — Capital University student portrait",
    university: "Capital University",
    caption: "Omar · Verified Portfolio",
    href: "/students",
    aspectClass: "aspect-[4/5]",
    objectPositionClass: "object-top",
    tiltClass: "-rotate-2 xl:-translate-x-4 hover:rotate-0",
    wireframeOffsetClass: "-translate-x-2 translate-y-1.5",
    Icon: Award,
  },
];

interface OrbitCardProps {
  tile: EditorialOrbitTile;
  logoUrl: string | null;
  isMobileGrid?: boolean;
}

function EditorialOrbitCard({ tile, logoUrl, isMobileGrid = false }: OrbitCardProps) {
  const Icon = tile.Icon;

  return (
    <div
      className={cn(
        "relative group transition-transform duration-500 motion-reduce:transform-none",
        !isMobileGrid && tile.tiltClass,
      )}
    >
      {/* Offset crisp white architectural wireframe */}
      <div
        aria-hidden="true"
        className={cn(
          "pointer-events-none absolute inset-0 rounded-[22px] border-[1.5px] border-white/60 transition-transform duration-500 group-hover:translate-x-0 group-hover:translate-y-0 motion-reduce:transform-none",
          !isMobileGrid ? tile.wireframeOffsetClass : "-translate-x-1.5 -translate-y-1.5",
        )}
      />

      <Link
        to={tile.href}
        className="relative isolate block overflow-hidden rounded-[22px] [clip-path:inset(0_round_22px)] transform-gpu border border-white/25 bg-[#1b1d26] shadow-[0_24px_52px_-12px_rgba(0,0,0,0.58)] transition-colors hover:border-[#d3fb52]/65"
      >
        <img
          src={tile.imageSrc}
          alt={tile.imageAlt}
          loading="eager"
          decoding="async"
          className={cn(
            "w-full rounded-[22px] object-cover transition-transform duration-500 ease-[cubic-bezier(0.22,1,0.36,1)] will-change-transform group-hover:scale-[1.03] motion-reduce:transform-none",
            isMobileGrid ? "aspect-[4/5]" : tile.aspectClass,
            tile.objectPositionClass,
          )}
        />
        <div className="pointer-events-none absolute inset-x-0 bottom-0 h-20 bg-gradient-to-t from-[#14151c]/85 via-[#14151c]/35 to-transparent" />
        <div className="absolute inset-x-2.5 bottom-2.5 flex items-center gap-2 rounded-full border border-white/15 bg-[#14151c]/90 px-2.5 py-1.5 backdrop-blur-md">
          <div className="size-6 shrink-0 overflow-hidden rounded-full bg-white p-0.5">
            <UniversityLogo
              name={tile.university}
              logoUrl={logoUrl}
              size="sm"
              className="size-full rounded-full border-0 bg-transparent p-0 shadow-none"
            />
          </div>
          <Icon className="size-3.5 shrink-0 text-[#d3fb52]" aria-hidden="true" />
          <span className="text-[11px] font-semibold leading-tight text-white">
            {tile.caption}
          </span>
        </div>
      </Link>
    </div>
  );
}

export function ProposedHeroDemo() {
  const navigate = useNavigate();
  const [layout, setLayout] = useState<HeroLayoutConcept>("orbit");
  const [searchQuery, setSearchQuery] = useState("");

  const { universities, resolveLogo } = useUniversityCatalog();
  const { students: showcaseStudents } = usePublicPeopleShowcase(6);

  const stageCards = useMemo(
    () => buildStudentStageCards(showcaseStudents),
    [showcaseStudents],
  );

  const orbitTiles = useMemo<readonly EditorialOrbitTile[]>(() => {
    const salma = showcaseStudents[0];
    const omar = showcaseStudents[1] ?? showcaseStudents[0];

    return ORBIT_TILES.map((tile) => {
      if (tile.id === "squad-portrait" && salma) {
        return {
          ...tile,
          imageSrc: salma.photoUrl,
          imageAlt: `${salma.firstName} ${salma.lastName} — ${salma.universityName} student portrait`,
          university: salma.universityName,
          caption: `${salma.firstName} · ${salma.facultyCode} Squad`,
        };
      }
      if (tile.id === "portfolio-portrait" && omar) {
        return {
          ...tile,
          imageSrc: omar.photoUrl,
          imageAlt: `${omar.firstName} ${omar.lastName} — ${omar.universityName} student portrait`,
          university: omar.universityName,
          caption: `${omar.firstName} · Lvl ${omar.level} Portfolio`,
        };
      }
      return tile;
    });
  }, [showcaseStudents]);

  const leftOrbitTiles = orbitTiles.slice(0, 2);
  const rightOrbitTiles = orbitTiles.slice(2, 4);

  const handleHeroSearch = (event: React.FormEvent) => {
    event.preventDefault();
    const trimmed = searchQuery.trim();
    void navigate(trimmed ? `/projects?search=${encodeURIComponent(trimmed)}` : "/projects");
  };

  return (
    <div className="min-h-screen w-full overflow-x-hidden bg-[#14151c] text-white">
      <section
        aria-label="Handshake Rosetta Brand Hero Showcase"
        className="teamup-hero-bg relative flex min-h-screen flex-col justify-between px-4 pb-6 pt-24 sm:px-8 sm:pt-28"
      >
        <HeroCampusIcons />

        {layout === "orbit" ? (
          /* ================================================================
             CONCEPT A: EDITORIAL PHOTO ORBIT (Pure Rosetta + Portraits + Icons)
             ================================================================ */
          <div className="relative z-10 mx-auto my-auto w-full max-w-[1360px]">
            <div className="grid grid-cols-1 items-center justify-center gap-8 lg:grid-cols-[224px_minmax(0,1fr)_224px] xl:grid-cols-[256px_minmax(0,660px)_256px] xl:gap-12">
              {/* Left Wing Photo & Student Portrait Stack */}
              <div className="hidden lg:flex lg:-mt-4 lg:flex-col lg:gap-8">
                {leftOrbitTiles.map((tile) => (
                  <EditorialOrbitCard
                    key={tile.id}
                    tile={tile}
                    logoUrl={resolveLogo(tile.university)}
                  />
                ))}
              </div>

              {/* Centered Handshake Rosetta Display Lockup */}
              <div className="flex flex-col items-center text-center">
                <h1
                  className="flex flex-col items-center font-bold uppercase text-white"
                  style={{
                    fontFamily: "var(--font-display)",
                    fontSize: "clamp(3.25rem, 6.8vw, 5.85rem)",
                    lineHeight: 0.9,
                    letterSpacing: "-0.025em",
                  }}
                >
                  <span className="block">LET&apos;S FIND</span>{" "}
                  <span className="mt-1.5 block">YOUR NEXT TEAM</span>
                </h1>

                <p className="mt-5 max-w-[520px] text-base leading-relaxed text-white/90 sm:text-lg">
                  Build your skills, find your team, join student activities, and grow together
                  across Egypt&apos;s campuses.
                </p>

                <form onSubmit={handleHeroSearch} className="mt-7 w-full max-w-[540px]">
                  <div className="flex h-[56px] items-center gap-2 rounded-[20px] bg-white pl-5 pr-2 shadow-[0_24px_56px_rgba(12,24,22,0.34)]">
                    <Search className="size-5 shrink-0 text-[#14151c]" aria-hidden="true" />
                    <input
                      type="search"
                      aria-label="Search teammates, clubs, or workshops"
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      placeholder="Search teammates, clubs, or workshops..."
                      className="min-w-0 flex-1 bg-transparent text-[14.5px] font-medium text-[#14151c] outline-none placeholder:font-normal placeholder:text-[#6b7280] sm:text-base"
                    />
                    <button
                      type="submit"
                      className="inline-flex h-10 shrink-0 cursor-pointer items-center gap-1.5 rounded-[14px] bg-[#d3fb52] px-4 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#c5f03a] sm:px-5 sm:text-sm"
                    >
                      <span>Explore</span>
                      <ArrowRight className="size-4" aria-hidden="true" />
                    </button>
                  </div>
                </form>

                {/* Overlapping Circular Official DB University Seals + +N Pill */}
                <div className="mt-7">
                  <CampusEmblemCluster
                    universities={universities}
                    label={`Across ${universities.length} Egyptian partner universities`}
                  />
                </div>
              </div>

              {/* Right Wing Photo & Student Portrait Stack */}
              <div className="hidden lg:mt-4 lg:flex lg:flex-col lg:gap-8">
                {rightOrbitTiles.map((tile) => (
                  <EditorialOrbitCard
                    key={tile.id}
                    tile={tile}
                    logoUrl={resolveLogo(tile.university)}
                  />
                ))}
              </div>
            </div>

            {/* Clean Mobile / Tablet 2-Column Portrait & Photo Grid */}
            <div className="mx-auto mt-10 grid max-w-[600px] grid-cols-2 gap-4 lg:hidden">
              {orbitTiles.map((tile) => (
                <EditorialOrbitCard
                  key={tile.id}
                  tile={tile}
                  logoUrl={resolveLogo(tile.university)}
                  isMobileGrid
                />
              ))}
            </div>
          </div>
        ) : (
          /* ================================================================
             CONCEPT B: STUDENT PORTRAIT STAGE (Pure Rosetta 3-Portrait Layout)
             ================================================================ */
          <div className="relative z-10 mx-auto flex w-full max-w-[1160px] flex-1 flex-col justify-between pt-2 sm:pt-4">
            {/* Upper Centered Handshake Rosetta Headline & Action Row */}
            <div className="flex flex-col items-center text-center">
              <h1
                className="flex flex-col items-center font-bold uppercase text-white"
                style={{
                  fontFamily: "var(--font-display)",
                  fontSize: "clamp(3.1rem, 6.4vw, 5.4rem)",
                  lineHeight: 0.9,
                  letterSpacing: "-0.025em",
                }}
              >
                <span className="block">LET&apos;S FIND</span>{" "}
                <span className="mt-1.5 block">YOUR NEXT TEAM</span>
              </h1>

              <p className="mt-4 max-w-[560px] text-base leading-relaxed text-white/90 sm:text-lg">
                Connect with cross-faculty builders, student technical societies, and verified campus
                workshops from Year One.
              </p>

              <div className="relative mt-6 flex w-full max-w-[900px] flex-col items-center justify-center gap-5 lg:flex-row">
                <div className="flex flex-wrap items-center justify-center gap-3.5">
                  <Link
                    to="/auth/register"
                    className="inline-flex h-12 items-center gap-2 rounded-full bg-[#d3fb52] px-7 text-sm font-bold text-[#14151c] shadow-[0_14px_34px_rgba(211,251,82,0.28)] transition-colors hover:bg-[#c5f03a]"
                  >
                    <span>Join Your Campus</span>
                    <ArrowRight className="size-4" aria-hidden="true" />
                  </Link>
                  <Link
                    to="/projects"
                    className="inline-flex h-12 items-center gap-2 rounded-full border border-white/25 bg-white/[0.06] px-6 text-sm font-semibold text-white backdrop-blur-sm transition-colors hover:border-white/55 hover:bg-white/[0.12]"
                  >
                    <Compass className="size-4 text-[#d3fb52]" aria-hidden="true" />
                    <span>Explore Squads</span>
                  </Link>
                </div>

                <div className="lg:absolute lg:right-0 lg:top-1/2 lg:-translate-y-1/2">
                  <CampusEmblemCluster
                    universities={universities}
                    label={`${universities.length} Partner Campuses`}
                  />
                </div>
              </div>
            </div>

            {/* Lower Stage: 3 Tilted Rosetta-Toned Student Portraits */}
            <div className="mx-auto mt-8 grid w-full max-w-[840px] grid-cols-1 items-end gap-6 px-3 sm:mt-10 sm:grid-cols-3 lg:gap-8">
              {stageCards.map((card, index) => (
                <StudentPortraitCard
                  key={card.id}
                  index={index}
                  card={card}
                  logoUrl={card.universityLogoUrl ?? resolveLogo(card.university)}
                />
              ))}
            </div>
          </div>
        )}

        {/* Discreet Bottom Concept Switcher Dock */}
        <div className="relative z-20 mt-7 flex justify-center">
          <div className="inline-flex flex-wrap items-center justify-center gap-1 rounded-full border border-white/15 bg-[#14151c]/85 p-1.5 shadow-xl backdrop-blur-md">
            <div
              role="tablist"
              aria-label="Hero layout concept"
              className="inline-flex items-center gap-1"
            >
              <button
                type="button"
                role="tab"
                aria-selected={layout === "orbit"}
                onClick={() => setLayout("orbit")}
                className={cn(
                  "cursor-pointer rounded-full px-4 py-1.5 text-xs font-semibold transition-colors",
                  layout === "orbit"
                    ? "bg-[#d3fb52] text-[#14151c]"
                    : "text-white/80 hover:text-white",
                )}
              >
                Editorial Photo Orbit
              </button>
              <button
                type="button"
                role="tab"
                aria-selected={layout === "stage"}
                onClick={() => setLayout("stage")}
                className={cn(
                  "cursor-pointer rounded-full px-4 py-1.5 text-xs font-semibold transition-colors",
                  layout === "stage"
                    ? "bg-[#d3fb52] text-[#14151c]"
                    : "text-white/80 hover:text-white",
                )}
              >
                Student Portrait Stage
              </button>
            </div>
            <span aria-hidden="true" className="mx-1 h-4 w-px bg-white/15" />
            <Link
              to="/"
              className="inline-flex items-center gap-1 rounded-full px-3.5 py-1.5 text-xs font-medium text-white/80 transition-colors hover:text-[#d3fb52]"
            >
              <span>Compare Live /</span>
              <ArrowUpRight className="size-3.5" aria-hidden="true" />
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
