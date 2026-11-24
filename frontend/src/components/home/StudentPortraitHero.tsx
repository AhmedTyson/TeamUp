/**
 * @date 2026-11-22
 * @purpose Handshake Rosetta-locked Student Portrait Stage hero showcase paired with campus hero line-art icons and circular live DB university seals.
 */
import {
  ArrowRight,
  CalendarCheck,
  Compass,
  Sparkles,
  Users,
  type LucideIcon,
} from "lucide-react";
import { useMemo, type CSSProperties } from "react";
import { Link } from "react-router-dom";
import type { PublicStudentShowcaseItem, UniversityCatalogItem } from "@/api";
import { UniversityLogo } from "@/components/ui";
import {
  FALLBACK_SHOWCASE_STUDENTS,
  usePublicPeopleShowcase,
} from "@/features/catalog/usePublicPeopleShowcase";
import { cn } from "@/lib/cn";

export interface StudentPortraitStageCard {
  id: string;
  studentId: number;
  imageSrc: string;
  imageAlt: string;
  university: string;
  universityLogoUrl: string | null;
  badgeText: string;
  href: string;
  surfaceClass: string;
  cardTiltClass: string;
  wireframeClass: string;
  Icon: LucideIcon;
}

const STAGE_SLOT_STYLES = [
  {
    preferredIndex: 1,
    id: "student-teal",
    href: "/projects",
    surfaceClass: "bg-[#3b8f82]",
    cardTiltClass: "sm:-rotate-6 sm:hover:-rotate-3 sm:hover:-translate-y-1.5",
    wireframeClass:
      "translate-x-2 -translate-y-2 group-hover:translate-x-1 group-hover:-translate-y-1",
    Icon: Users,
    formatBadge: (student: PublicStudentShowcaseItem) =>
      `${student.firstName} · ${student.facultyCode}`,
  },
  {
    preferredIndex: 0,
    id: "student-green",
    href: "/organizations",
    surfaceClass: "bg-[#3fc970]",
    cardTiltClass: "z-10 sm:scale-[1.04] sm:hover:-translate-y-2",
    wireframeClass: "-translate-y-2.5 group-hover:-translate-y-1.5",
    Icon: Sparkles,
    formatBadge: (student: PublicStudentShowcaseItem) =>
      `${student.firstName} · ${student.facultyCode}`,
  },
  {
    preferredIndex: 2,
    id: "student-lime",
    href: "/events",
    surfaceClass: "bg-[#a9dc48]",
    cardTiltClass: "sm:rotate-6 sm:hover:rotate-3 sm:hover:-translate-y-1.5",
    wireframeClass:
      "-translate-x-2 -translate-y-2 group-hover:-translate-x-1 group-hover:-translate-y-1",
    Icon: CalendarCheck,
    formatBadge: (student: PublicStudentShowcaseItem) =>
      `${student.firstName} · ${student.totalPoints} XP`,
  },
] as const;

export function buildStudentStageCards(
  students: readonly PublicStudentShowcaseItem[],
): readonly StudentPortraitStageCard[] {
  const pool: PublicStudentShowcaseItem[] = [...students];
  for (const fallback of FALLBACK_SHOWCASE_STUDENTS) {
    if (pool.length >= 3) {
      break;
    }
    if (!pool.some((item) => item.studentId === fallback.studentId)) {
      pool.push(fallback);
    }
  }

  return STAGE_SLOT_STYLES.map((slot) => {
    const student = pool[slot.preferredIndex] ?? pool[0];
    return {
      id: slot.id,
      studentId: student.studentId,
      imageSrc: student.photoUrl,
      imageAlt: `${student.firstName} ${student.lastName} — ${student.universityName} student portrait`,
      university: student.universityName,
      universityLogoUrl: student.universityLogoUrl,
      badgeText: slot.formatBadge(student),
      href: slot.href,
      surfaceClass: slot.surfaceClass,
      cardTiltClass: slot.cardTiltClass,
      wireframeClass: slot.wireframeClass,
      Icon: slot.Icon,
    };
  });
}

export function HeroCampusIcons() {
  return (
    <div
      aria-hidden="true"
      className="pointer-events-none absolute inset-0 overflow-hidden select-none text-white/[0.16]"
    >
      {/* Upper-right Graduation Cap */}
      <svg
        viewBox="0 0 88 64"
        fill="none"
        style={
          {
            "--icon-rot": "-12deg",
            "--icon-duration": "7.8s",
            "--icon-delay": "0s",
            "--icon-dx": "4px",
            "--icon-dy": "-10px",
          } as CSSProperties
        }
        className="hero-icon-float absolute right-[7%] top-[14%] hidden size-20 md:block"
      >
        <path
          d="M6 26L44 10L82 26L44 42L6 26Z"
          stroke="currentColor"
          strokeWidth="1.6"
          strokeLinejoin="round"
        />
        <path
          d="M22 33V48C22 52 32 56 44 56C56 56 66 52 66 48V33"
          stroke="currentColor"
          strokeWidth="1.6"
          strokeLinecap="round"
        />
        <path
          d="M76 29V47"
          stroke="#d3fb52"
          strokeOpacity="0.32"
          strokeWidth="1.8"
          strokeLinecap="round"
        />
      </svg>

      {/* Upper-left Open Textbook */}
      <svg
        viewBox="0 0 80 60"
        fill="none"
        style={
          {
            "--icon-rot": "-12deg",
            "--icon-duration": "8.6s",
            "--icon-delay": "0.7s",
            "--icon-dx": "-5px",
            "--icon-dy": "-11px",
          } as CSSProperties
        }
        className="hero-icon-float absolute left-[8%] top-[18%] hidden size-16 lg:block"
      >
        <path
          d="M8 14C20 10 34 12 40 18C46 12 60 10 72 14V48C60 44 46 46 40 52C34 46 20 44 8 48V14Z"
          stroke="currentColor"
          strokeWidth="1.6"
          strokeLinejoin="round"
        />
        <path d="M40 18V52" stroke="#d3fb52" strokeOpacity="0.28" strokeWidth="1.5" />
      </svg>

      {/* Upper-left-center Code Brackets */}
      <svg
        viewBox="0 0 68 56"
        fill="none"
        style={
          {
            "--icon-rot": "6deg",
            "--icon-duration": "7.2s",
            "--icon-delay": "1.4s",
            "--icon-dx": "6px",
            "--icon-dy": "-9px",
          } as CSSProperties
        }
        className="hero-icon-float absolute left-[22%] top-[14%] hidden size-12 xl:block"
      >
        <path
          d="M22 14L8 28L22 42"
          stroke="currentColor"
          strokeWidth="1.7"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M46 14L60 28L46 42"
          stroke="currentColor"
          strokeWidth="1.7"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M38 11L30 45"
          stroke="#d3fb52"
          strokeOpacity="0.34"
          strokeWidth="1.7"
          strokeLinecap="round"
        />
      </svg>

      {/* Lower-left Project Launch Rocket */}
      <svg
        viewBox="0 0 76 76"
        fill="none"
        style={
          {
            "--icon-rot": "12deg",
            "--icon-duration": "9.2s",
            "--icon-delay": "0.4s",
            "--icon-dx": "5px",
            "--icon-dy": "-12px",
          } as CSSProperties
        }
        className="hero-icon-float absolute bottom-[20%] left-[21%] hidden size-16 xl:block"
      >
        <path
          d="M48 12C57 18 61 32 55 44L34 55L21 42L32 21C37 15 43 13 48 12Z"
          stroke="currentColor"
          strokeWidth="1.6"
          strokeLinejoin="round"
        />
        <circle cx="44" cy="29" r="4.5" stroke="#d3fb52" strokeOpacity="0.35" strokeWidth="1.5" />
        <path
          d="M21 42L14 55L27 48"
          stroke="#d3fb52"
          strokeOpacity="0.35"
          strokeWidth="1.5"
          strokeLinejoin="round"
        />
      </svg>

      {/* Mid-right Spiral Notebook */}
      <svg
        viewBox="0 0 72 84"
        fill="none"
        style={
          {
            "--icon-rot": "12deg",
            "--icon-duration": "8.4s",
            "--icon-delay": "1.9s",
            "--icon-dx": "-4px",
            "--icon-dy": "-10px",
          } as CSSProperties
        }
        className="hero-icon-float absolute bottom-[36%] right-[9%] hidden size-16 lg:block"
      >
        <rect
          x="16"
          y="10"
          width="44"
          height="64"
          rx="6"
          stroke="currentColor"
          strokeWidth="1.6"
        />
        <path
          d="M11 22H21M11 36H21M11 50H21M11 64H21"
          stroke="currentColor"
          strokeWidth="1.6"
          strokeLinecap="round"
        />
        <path
          d="M28 28H48M28 40H44"
          stroke="#d3fb52"
          strokeOpacity="0.32"
          strokeWidth="1.5"
          strokeLinecap="round"
        />
      </svg>

      {/* Lower-right Trophy Cup */}
      <svg
        viewBox="0 0 68 68"
        fill="none"
        style={
          {
            "--icon-rot": "-6deg",
            "--icon-duration": "9.8s",
            "--icon-delay": "1.1s",
            "--icon-dx": "4px",
            "--icon-dy": "-9px",
          } as CSSProperties
        }
        className="hero-icon-float absolute bottom-[16%] right-[21%] hidden size-14 xl:block"
      >
        <path
          d="M20 14H48V28C48 36 42 42 34 42C26 42 20 36 20 28V14Z"
          stroke="currentColor"
          strokeWidth="1.6"
          strokeLinejoin="round"
        />
        <path
          d="M20 18H12C12 25 15 29 20 29M48 18H56C56 25 53 29 48 29"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinecap="round"
        />
        <path
          d="M34 42V53M24 54H44"
          stroke="#d3fb52"
          strokeOpacity="0.34"
          strokeWidth="1.7"
          strokeLinecap="round"
        />
      </svg>
    </div>
  );
}

export interface CampusEmblemClusterProps {
  universities: readonly UniversityCatalogItem[];
  label: string;
}

export function CampusEmblemCluster({ universities, label }: CampusEmblemClusterProps) {
  const visible = universities.slice(0, 6);
  const remaining = Math.max(0, universities.length - visible.length);

  return (
    <div className="animate-hero-emblems flex flex-col items-center gap-2 text-center">
      <span className="text-xs font-medium text-white/80">{label}</span>
      <div className="flex items-center -space-x-2">
        {visible.map((uni) => (
          <div
            key={uni.universityId}
            className="size-8 overflow-hidden rounded-full bg-white p-1 ring-2 ring-[#14151c] shadow-md transition-transform duration-200 hover:z-10 hover:-translate-y-1"
          >
            <UniversityLogo
              name={uni.name}
              logoUrl={uni.logoUrl}
              size="sm"
              className="size-full rounded-full border-0 bg-transparent p-0 shadow-none"
            />
          </div>
        ))}
        {remaining > 0 && (
          <span className="inline-flex size-8 items-center justify-center rounded-full bg-[#d3fb52] text-[11px] font-extrabold text-[#14151c] ring-2 ring-[#14151c] shadow-md">
            +{remaining}
          </span>
        )}
      </div>
    </div>
  );
}

export interface StudentPortraitCardProps {
  card: StudentPortraitStageCard;
  logoUrl: string | null;
  index?: number;
}

export function StudentPortraitCard({ card, logoUrl, index = 0 }: StudentPortraitCardProps) {
  const Icon = card.Icon;

  return (
    <div
      className="hero-portrait-entrance mx-auto w-full min-w-0 max-w-[260px] sm:max-w-none"
      style={{ "--card-delay": `${460 + index * 105}ms` } as CSSProperties}
    >
      <div
        className="hero-portrait-float"
        style={{ "--float-delay": `${index * 0.85}s` } as CSSProperties}
      >
        <div
          className={cn(
            "portrait-tilt-card group relative w-full transition-transform duration-500 ease-[cubic-bezier(0.22,1,0.36,1)] will-change-transform",
            card.cardTiltClass,
          )}
        >
          <div
            aria-hidden="true"
            className={cn(
              "portrait-wireframe pointer-events-none absolute inset-0 rounded-[24px] border-[1.5px] border-white/75 transition-transform duration-500 ease-[cubic-bezier(0.22,1,0.36,1)]",
              card.wireframeClass,
            )}
          />
          <Link
            to={card.href}
            className={cn(
              "relative isolate block overflow-hidden rounded-[24px] [clip-path:inset(0_round_24px)] transform-gpu shadow-[0_28px_60px_-14px_rgba(0,0,0,0.65)]",
              card.surfaceClass,
            )}
          >
            <img
              src={card.imageSrc}
              alt={card.imageAlt}
              loading="eager"
              decoding="async"
              className="portrait-photo aspect-[4/5] w-full rounded-[24px] object-cover object-top transition-transform duration-500 ease-[cubic-bezier(0.22,1,0.36,1)] will-change-transform group-hover:scale-[1.03]"
            />
            <div className="absolute inset-x-3 bottom-3 flex items-center gap-2 rounded-full border border-white/20 bg-[#14151c]/90 px-2.5 py-1.5 backdrop-blur-md shadow-lg">
              <div className="size-6 shrink-0 overflow-hidden rounded-full bg-white p-0.5">
                <UniversityLogo
                  name={card.university}
                  logoUrl={logoUrl}
                  size="sm"
                  className="size-full rounded-full border-0 bg-transparent p-0 shadow-none"
                />
              </div>
              <Icon className="size-3.5 shrink-0 text-[#d3fb52]" aria-hidden="true" />
              <span className="truncate text-[11px] font-bold tracking-tight text-white">
                {card.badgeText}
              </span>
            </div>
          </Link>
        </div>
      </div>
    </div>
  );
}

export interface StudentPortraitHeroProps {
  universities: readonly UniversityCatalogItem[];
  students?: readonly PublicStudentShowcaseItem[];
}

export function StudentPortraitHero({
  universities,
  students: studentsProp,
}: StudentPortraitHeroProps) {
  const { students: liveStudents } = usePublicPeopleShowcase(6, !studentsProp);
  const stageCards = useMemo(
    () => buildStudentStageCards(studentsProp ?? liveStudents),
    [studentsProp, liveStudents],
  );

  const logoByName = useMemo(() => {
    const map = new Map<string, string | null>();
    universities.forEach((uni) => {
      map.set(uni.name.toLowerCase(), uni.logoUrl ?? null);
    });
    return map;
  }, [universities]);

  return (
    <section
      aria-label="TeamUp Campus Student Hero"
      className="teamup-hero-bg relative z-10 flex min-h-[100dvh] w-full flex-col justify-between overflow-hidden px-4 pb-8 pt-[calc(5.75rem+env(safe-area-inset-top))] sm:px-8 sm:pt-28"
    >
      <div aria-hidden="true" className="hero-aurora-layer" />
      <HeroCampusIcons />

      <div className="relative z-10 mx-auto my-auto flex w-full max-w-[1160px] flex-1 flex-col justify-between pt-2 sm:pt-4">
        {/* Upper Centered Handshake Rosetta Display Lockup */}
        <div className="flex flex-col items-center text-center">
          <h1
            className="flex select-none flex-col items-center font-bold uppercase text-white"
            style={{
              fontFamily: "'Antonio', 'Barlow Condensed', sans-serif",
              fontSize: "clamp(2.25rem, 8.2vw, 5.4rem)",
              lineHeight: 0.9,
              letterSpacing: "-0.025em",
            }}
          >
            <span className="animate-hero-line-1 block">LET&apos;S FIND</span>{" "}
            <span className="animate-hero-line-2 mt-1.5 block">YOUR NEXT TEAM</span>
          </h1>

          <p className="animate-hero-sub mt-4 max-w-[560px] text-base leading-relaxed text-white/90 sm:text-lg">
            Connect with cross-faculty builders, student technical societies, and verified campus
            workshops from Year One.
          </p>

          {/* Action Row + Right-Flanked Circular DB University Emblems */}
          <div className="relative mt-6 flex w-full max-w-[900px] flex-col items-center justify-center gap-5 xl:flex-row">
            <div className="animate-hero-cta flex flex-wrap items-center justify-center gap-3.5">
              <Link
                to="/auth/register"
                className="group inline-flex h-12 items-center gap-2 rounded-full bg-[#d3fb52] px-7 text-sm font-bold text-[#14151c] shadow-[0_14px_34px_rgba(211,251,82,0.28)] transition-colors hover:bg-[#c5f03a]"
              >
                <span>Join Your Campus</span>
                <ArrowRight
                  className="size-4 transition-transform duration-200 group-hover:translate-x-1"
                  aria-hidden="true"
                />
              </Link>
              <Link
                to="/projects"
                className="inline-flex h-12 items-center gap-2 rounded-full border border-white/25 bg-white/[0.06] px-6 text-sm font-semibold text-white backdrop-blur-sm transition-colors hover:border-white/55 hover:bg-white/[0.12]"
              >
                <Compass className="size-4 text-[#d3fb52]" aria-hidden="true" />
                <span>Explore Squads</span>
              </Link>
            </div>

            <div className="xl:absolute xl:right-0 xl:top-1/2 xl:-translate-y-1/2">
              <CampusEmblemCluster
                universities={universities}
                label={`${universities.length} Partner Campuses`}
              />
            </div>
          </div>
        </div>

        {/* Lower 3-Student Tilted Portrait Stage */}
        <div className="mx-auto mt-8 grid w-full max-w-[840px] grid-cols-1 items-end gap-6 px-3 sm:mt-10 sm:grid-cols-3 lg:gap-8">
          {stageCards.map((card, index) => (
            <StudentPortraitCard
              key={card.id}
              index={index}
              card={card}
              logoUrl={
                card.universityLogoUrl ?? logoByName.get(card.university.toLowerCase()) ?? null
              }
            />
          ))}
        </div>
      </div>

      {/* Smooth Scroll Down Cue */}
      <div className="relative z-10 flex w-full justify-center pt-5">
        <a
          href="#services"
          aria-label="Scroll to services"
          className="group flex cursor-pointer flex-col items-center gap-1.5 text-white/60 transition-colors duration-200 hover:text-white"
        >
          <span className="text-[13px] font-medium tracking-wide">Explore platform modules</span>
          <svg
            className="hero-scroll-cue h-5 w-5 transition-transform duration-300 group-hover:translate-y-1"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden="true"
          >
            <path d="m6 9 6 6 6-6" />
          </svg>
        </a>
      </div>
    </section>
  );
}
