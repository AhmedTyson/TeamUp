/**
 * @date 2026-11-22
 * @purpose Handshake Career-Centers & Editorial Builders Community hub powering /students, /leaderboard, /gamification, /profile/me, and /notifications.
 */
import { Award, Bell, CheckCircle2, Compass, GraduationCap, Trophy } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import {
  Badge,
  DataTable,
  Pagination,
  SearchInput,
  UniversityLogo,
  type DataTableColumn,
} from "@/components/ui";
import { useAuth } from "@/features/auth";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { cn } from "@/lib/cn";

export interface CommunityHubPageProps {
  mode: "students" | "portfolio" | "leaderboard" | "gamification" | "notifications";
}

interface CampusBuilderRow {
  id: string;
  rank: number;
  name: string;
  university: string;
  facultyCode: string;
  yearLabel: string;
  xp: number;
  level: number;
  badgesCount: number;
  topSkills: readonly string[];
  portraitSrc: string;
}

const CAMPUS_BUILDERS: readonly CampusBuilderRow[] = [
  {
    id: "b-1",
    rank: 1,
    name: "Salma Khaled",
    university: "Cairo University",
    facultyCode: "FCAI",
    yearLabel: "Year 4",
    xp: 2840,
    level: 7,
    badgesCount: 9,
    topSkills: ["PyTorch", "React", "FastAPI"],
    portraitSrc: "/images/hero/hero-student-green.jpg",
  },
  {
    id: "b-2",
    rank: 2,
    name: "Omar Mostafa",
    university: "Capital University",
    facultyCode: "ENG_CAPU",
    yearLabel: "Year 3",
    xp: 2490,
    level: 6,
    badgesCount: 8,
    topSkills: ["STM32", "ROS2", "TypeScript"],
    portraitSrc: "/images/hero/hero-student-teal.jpg",
  },
  {
    id: "b-3",
    rank: 3,
    name: "Youssef Adel",
    university: "Ain Shams University",
    facultyCode: "CIS",
    yearLabel: "Year 3",
    xp: 2310,
    level: 6,
    badgesCount: 7,
    topSkills: ["ASP.NET Core", "PostgreSQL", "Docker"],
    portraitSrc: "/images/hero/hero-student-lime.jpg",
  },
  {
    id: "b-4",
    rank: 4,
    name: "Hana El-Sayed",
    university: "Alexandria University",
    facultyCode: "CDS_ALEX",
    yearLabel: "Year 2",
    xp: 1920,
    level: 5,
    badgesCount: 6,
    topSkills: ["Computer Vision", "Python", "Figma"],
    portraitSrc: "/images/hero/hero-student-green.jpg",
  },
  {
    id: "b-5",
    rank: 5,
    name: "Karim Mansour",
    university: "German University in Cairo (GUC)",
    facultyCode: "MET_GUC",
    yearLabel: "Year 4",
    xp: 1780,
    level: 5,
    badgesCount: 6,
    topSkills: ["Go", "React", "UI Systems"],
    portraitSrc: "/images/hero/hero-student-teal.jpg",
  },
  {
    id: "b-6",
    rank: 6,
    name: "Mariam Nabil",
    university: "Mansoura University",
    facultyCode: "FCI_MANS",
    yearLabel: "Year 2",
    xp: 1540,
    level: 4,
    badgesCount: 5,
    topSkills: ["Transformers", "Python", "UI Research"],
    portraitSrc: "/images/hero/hero-student-green.jpg",
  },
];

const UNLOCKABLE_BADGES = [
  { id: "first-ship", title: "Squad Pioneer", rule: "Complete your first sprint milestone", xp: "+250 XP" },
  { id: "verified-check", title: "Workshop Regular", rule: "Verify 3+ campus event attendances", xp: "+150 XP" },
  { id: "chapter-mentor", title: "Club Collaborator", rule: "Hold an active role in a partner club", xp: "+200 XP" },
  { id: "cross-faculty", title: "Cross-Faculty Bridge", rule: "Team up across two university departments", xp: "+300 XP" },
] as const;

const SAMPLE_NOTIFICATIONS = [
  { id: "n1", title: "Squad invitation accepted", body: "Your seat on Bilingual RAG Study Assistant was confirmed.", time: "12m ago", unread: true },
  { id: "n2", title: "Workshop XP verified", body: "+100 XP credited for System Design Bootcamp Session #104.", time: "3h ago", unread: true },
  { id: "n3", title: "IEEE Computer Society update", body: "Spring mentorship applications close Thursday at 6:00 PM.", time: "1d ago", unread: false },
] as const;

export function CommunityHubPage({ mode }: CommunityHubPageProps) {
  const { user, isAuthenticated } = useAuth();
  const { resolveLogo } = useUniversityCatalog();
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 6;

  const q = search.trim().toLowerCase();
  const filteredBuilders = CAMPUS_BUILDERS.filter(
    (b) =>
      !q ||
      b.name.toLowerCase().includes(q) ||
      b.university.toLowerCase().includes(q) ||
      b.topSkills.some((sk) => sk.toLowerCase().includes(q)),
  );

  const totalCount = filteredBuilders.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const safePage = Math.min(page, totalPages);
  const pagedBuilders = filteredBuilders.slice((safePage - 1) * pageSize, safePage * pageSize);

  const columns: readonly DataTableColumn<CampusBuilderRow>[] = [
    {
      key: "rank",
      header: "Rank",
      cell: (row) => (
        <span className="inline-flex size-7 items-center justify-center rounded-lg bg-[#d3fb52]/15 font-mono text-xs font-bold text-[#d3fb52]">
          #{row.rank}
        </span>
      ),
    },
    {
      key: "builder",
      header: "Student Builder",
      cell: (row) => (
        <div className="flex items-center gap-3">
          <img
            src={row.portraitSrc}
            alt={row.name}
            className="size-9 rounded-full object-cover object-top ring-2 ring-white/15"
          />
          <div>
            <p className="font-semibold text-white">{row.name}</p>
            <p className="text-xs text-[#9ca3af]">
              {row.facultyCode} · {row.yearLabel}
            </p>
          </div>
        </div>
      ),
    },
    {
      key: "university",
      header: "University",
      cell: (row) => (
        <div className="flex items-center gap-2">
          <UniversityLogo name={row.university} logoUrl={resolveLogo(row.university)} size="sm" />
          <span className="text-xs font-medium text-white/90">{row.university}</span>
        </div>
      ),
    },
    {
      key: "xp",
      header: "Verified XP",
      cell: (row) => (
        <span className="font-mono text-xs font-bold tabular-nums text-[#d3fb52]">
          {row.xp} XP · Lvl {row.level}
        </span>
      ),
    },
  ];

  if (mode === "leaderboard" || mode === "gamification") {
    return (
      <div className="space-y-8">
        {/* Handshake /career-centers/ High-Contrast Lime Display Header */}
        <section className="teamup-hero-bg overflow-hidden rounded-[28px] border border-white/12 px-6 py-10 text-center shadow-2xl sm:px-10 sm:py-12">
          <Badge variant="accent" size="sm">
            Verified Participation &amp; XP Ledger
          </Badge>
          <h1
            className="mt-4 font-bold uppercase text-[#d3fb52]"
            style={{
              fontFamily: "var(--font-display)",
              fontSize: "clamp(2.6rem, 6.2vw, 4.9rem)",
              lineHeight: 0.9,
              letterSpacing: "-0.025em",
            }}
          >
            {mode === "leaderboard" ? "CAMPUS LEADERBOARD" : "BADGES & LEVEL SYSTEM"}
          </h1>
          <p className="mx-auto mt-3 max-w-[580px] text-sm leading-relaxed text-white/85 sm:text-base">
            Every shipped team milestone and QR-verified workshop awards immutable XP toward your
            campus rank and verified achievement portfolio.
          </p>

          {/* Top 3 Rosetta Student Podium Cards */}
          <div className="mx-auto mt-8 grid max-w-[820px] gap-4 sm:grid-cols-3">
            {CAMPUS_BUILDERS.slice(0, 3).map((leader, idx) => (
              <div
                key={leader.id}
                className={cn(
                  "flex items-center gap-3 rounded-2xl border bg-[#14151c]/88 p-4 text-left backdrop-blur-md",
                  idx === 0 ? "border-[#d3fb52]/60" : "border-white/12",
                )}
              >
                <img
                  src={leader.portraitSrc}
                  alt={leader.name}
                  className="size-12 shrink-0 rounded-2xl object-cover object-top"
                />
                <div className="min-w-0">
                  <div className="flex items-center gap-1.5">
                    <Trophy className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
                    <span className="font-mono text-[11px] font-bold text-[#d3fb52]">
                      #{leader.rank} · {leader.xp} XP
                    </span>
                  </div>
                  <p className="truncate text-sm font-bold text-white">{leader.name}</p>
                  <p className="truncate text-[11px] text-[#9ca3af]">{leader.university}</p>
                </div>
              </div>
            ))}
          </div>
        </section>

        {mode === "gamification" && (
          <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {UNLOCKABLE_BADGES.map((badge) => (
              <div
                key={badge.id}
                className="flex flex-col justify-between gap-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-5"
              >
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <Award className="size-5 text-[#d3fb52]" aria-hidden="true" />
                    <Badge variant="accent" size="sm">
                      {badge.xp}
                    </Badge>
                  </div>
                  <h2
                    className="text-base font-bold text-white"
                    style={{ fontFamily: "var(--font-heading)" }}
                  >
                    {badge.title}
                  </h2>
                  <p className="text-xs leading-relaxed text-[#9ca3af]">{badge.rule}</p>
                </div>
              </div>
            ))}
          </section>
        )}

        <section className="space-y-4 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-5 sm:p-7">
          <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
            <h2
              className="text-lg font-bold text-white"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              Standings Across Partner Campuses
            </h2>
            <SearchInput
              value={search}
              onChange={setSearch}
              placeholder="Search student or university..."
              containerClassName="w-full sm:w-72"
            />
          </div>

          <DataTable
            columns={columns}
            data={pagedBuilders}
            keyExtractor={(row) => row.id}
            caption="Campus leaderboard standings"
          />
        </section>
      </div>
    );
  }

  if (mode === "notifications") {
    return (
      <section className="space-y-6 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-6 sm:p-8">
        <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
          <div>
            <h1
              className="text-2xl font-bold text-white sm:text-3xl"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              Notifications Inbox
            </h1>
            <p className="mt-1 text-sm text-[#9ca3af]">
              Stay current on squad seat approvals, event check-ins, and club updates.
            </p>
          </div>
          <Badge variant="accent" size="sm">
            <Bell className="mr-1 size-3.5" aria-hidden="true" />
            Activity Feed
          </Badge>
        </div>

        <div className="space-y-3">
          {SAMPLE_NOTIFICATIONS.map((item) => (
            <div
              key={item.id}
              className="flex items-start justify-between gap-4 rounded-2xl border border-white/[0.08] bg-[#14151c] p-4"
            >
              <div className="flex items-start gap-3">
                <CheckCircle2
                  className={cn(
                    "mt-0.5 size-4 shrink-0",
                    item.unread ? "text-[#d3fb52]" : "text-white/40",
                  )}
                  aria-hidden="true"
                />
                <div>
                  <p className="text-sm font-semibold text-white">{item.title}</p>
                  <p className="mt-0.5 text-xs text-[#9ca3af]">{item.body}</p>
                </div>
              </div>
              <span className="shrink-0 font-mono text-[11px] text-white/50">{item.time}</span>
            </div>
          ))}
        </div>
      </section>
    );
  }

  if (mode === "portfolio") {
    return (
      <section className="space-y-6 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-6 sm:p-8">
        <div className="teamup-hero-bg flex flex-col justify-between gap-6 rounded-2xl border border-white/12 p-6 sm:flex-row sm:items-center">
          <div className="space-y-2">
            <p className="text-xs font-bold uppercase tracking-wider text-[#d3fb52]">
              Student Portfolio
            </p>
            <h1
              className="text-3xl font-bold text-white sm:text-4xl"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              {isAuthenticated && user
                ? `${user.firstName} ${user.lastName}`
                : "Claim Your Verified Student Portfolio"}
            </h1>
            <p className="text-sm text-white/80">
              {isAuthenticated && user
                ? `${user.universityName} · ${user.facultyName} · Year ${user.academicYear}`
                : "Sign in with your university email to display your verified skills, squad history, and workshop badges."}
            </p>
          </div>
          {!isAuthenticated && (
            <Link
              to="/auth/login"
              className="inline-flex h-11 shrink-0 items-center gap-2 rounded-full bg-[#d3fb52] px-6 text-sm font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
            >
              Sign In to Manage Profile
            </Link>
          )}
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          <div className="rounded-2xl border border-white/[0.08] bg-[#14151c] p-5">
            <GraduationCap className="size-5 text-[#d3fb52]" aria-hidden="true" />
            <p className="mt-3 text-sm font-bold text-white">Verified Skills</p>
            <p className="mt-1 text-xs text-[#9ca3af]">
              Showcase endorsed stack skills for project leads recruiting across campus.
            </p>
          </div>
          <div className="rounded-2xl border border-white/[0.08] bg-[#14151c] p-5">
            <Compass className="size-5 text-[#d3fb52]" aria-hidden="true" />
            <p className="mt-3 text-sm font-bold text-white">Squad Contributions</p>
            <p className="mt-1 text-xs text-[#9ca3af]">
              Every completed sprint task and project milestone links directly here.
            </p>
          </div>
          <div className="rounded-2xl border border-white/[0.08] bg-[#14151c] p-5">
            <Award className="size-5 text-[#d3fb52]" aria-hidden="true" />
            <p className="mt-3 text-sm font-bold text-white">Earned Campus Badges</p>
            <p className="mt-1 text-xs text-[#9ca3af]">
              QR-checked workshops and club roles unlock permanent achievement badges.
            </p>
          </div>
        </div>
      </section>
    );
  }

  /* Default: mode === "students" */
  return (
    <div className="space-y-8">
      <section className="grid items-center gap-8 rounded-[28px] border border-white/12 bg-[#1b1d26] p-6 shadow-2xl sm:p-9 lg:grid-cols-12">
        <div className="space-y-4 lg:col-span-6">
          <Badge variant="accent" size="sm">
            Verified Student Directory
          </Badge>
          <h1
            className="font-bold uppercase text-white"
            style={{
              fontFamily: "var(--font-display)",
              fontSize: "clamp(2.5rem, 5.5vw, 4.4rem)",
              lineHeight: 0.9,
              letterSpacing: "-0.025em",
            }}
          >
            DISCOVER BUILDERS
            <br />
            ACROSS CAMPUSES
          </h1>
          <p className="max-w-lg text-sm leading-relaxed text-white/80 sm:text-base">
            Filter verified builders by skill stack, faculty, and academic year to assemble
            balanced cross-disciplinary teams.
          </p>
        </div>

        <div className="relative lg:col-span-6">
          <div
            aria-hidden="true"
            className="pointer-events-none absolute -inset-1 translate-x-2 -translate-y-2 rounded-[24px] border-[1.5px] border-white/55"
          />
          <div className="relative overflow-hidden rounded-[22px] border border-white/20 bg-[#14151c] shadow-2xl">
            <img
              src="/images/hero/hero-demo-day.jpg"
              alt="University students showcasing their engineering prototype during Demo Day"
              loading="eager"
              decoding="async"
              className="aspect-[16/10] w-full object-cover object-center"
            />
          </div>
        </div>
      </section>

      <section className="space-y-6 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-5 sm:p-7">
        <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
          <div>
            <h2
              className="text-xl font-bold text-white"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              Featured Student Builders
            </h2>
            <p className="text-xs text-[#9ca3af]">
              Privacy-respecting public profiles synced with verified partner universities.
            </p>
          </div>
          <SearchInput
            value={search}
            onChange={(val) => {
              setSearch(val);
              setPage(1);
            }}
            placeholder="Search builder, skill, or university..."
            containerClassName="w-full sm:w-80"
          />
        </div>

        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {pagedBuilders.map((builder) => (
            <article
              key={builder.id}
              className="flex flex-col justify-between gap-4 rounded-2xl border border-white/[0.085] bg-[#14151c] p-5 transition-colors hover:border-[#d3fb52]/40"
            >
              <div className="space-y-3">
                <div className="flex items-center justify-between gap-3">
                  <div className="flex items-center gap-3">
                    <img
                      src={builder.portraitSrc}
                      alt={builder.name}
                      className="size-12 rounded-2xl object-cover object-top"
                    />
                    <div>
                      <h3
                        className="text-base font-bold text-white"
                        style={{ fontFamily: "var(--font-heading)" }}
                      >
                        {builder.name}
                      </h3>
                      <p className="font-mono text-xs text-[#9ca3af]">
                        {builder.facultyCode} · {builder.yearLabel}
                      </p>
                    </div>
                  </div>
                  <UniversityLogo
                    name={builder.university}
                    logoUrl={resolveLogo(builder.university)}
                    size="sm"
                  />
                </div>

                <div className="flex flex-wrap gap-1.5">
                  {builder.topSkills.map((sk) => (
                    <Badge key={sk} variant="default" size="sm">
                      {sk}
                    </Badge>
                  ))}
                </div>
              </div>

              <div className="flex items-center justify-between border-t border-white/[0.08] pt-3 text-xs">
                <span className="font-mono font-semibold text-[#d3fb52]">
                  {builder.xp} XP · {builder.badgesCount} badges
                </span>
                <Link
                  to={`/students/${builder.id}`}
                  className="font-semibold text-white/80 hover:text-[#d3fb52]"
                >
                  View Profile →
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
