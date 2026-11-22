/**
 * @date 2026-11-22
 * @purpose Handshake Career-Centers & Editorial Builders Community hub powering /students, /leaderboard, /gamification, /profile/me, and /notifications.
 */
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Award, Bell, Camera, CheckCircle2, Compass, GraduationCap, Loader2, Trophy } from "lucide-react";
import { useRef, useState, type ChangeEvent } from "react";
import { Link } from "react-router-dom";
import { studentsApi, type PublicStudentShowcaseItem } from "@/api";
import {
  Avatar,
  Badge,
  DataTable,
  Pagination,
  SearchInput,
  UniversityLogo,
  type DataTableColumn,
} from "@/components/ui";
import { useAuth } from "@/features/auth";
import { usePublicPeopleShowcase } from "@/features/catalog/usePublicPeopleShowcase";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { cn } from "@/lib/cn";
import { queryKeys } from "@/lib/queryKeys";

const ALLOWED_PHOTO_TYPES = new Set(["image/jpeg", "image/png", "image/webp"]);
const MAX_PHOTO_BYTES = 2 * 1024 * 1024;

export interface CommunityHubPageProps {
  mode: "students" | "portfolio" | "leaderboard" | "gamification" | "notifications";
}

interface RankedShowcaseBuilder extends PublicStudentShowcaseItem {
  rank: number;
}

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
  const { user, isAuthenticated, refreshSession } = useAuth();
  const queryClient = useQueryClient();
  const { resolveLogo } = useUniversityCatalog();
  const { students } = usePublicPeopleShowcase(12);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [photoError, setPhotoError] = useState<string | null>(null);
  const [photoSuccess, setPhotoSuccess] = useState<string | null>(null);
  const [uploadedPreviewUrl, setUploadedPreviewUrl] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const pageSize = 6;

  const photoUploadMutation = useMutation({
    mutationFn: (file: File) => studentsApi.uploadProfilePhoto(file),
    onSuccess: async (updatedProfile) => {
      setPhotoError(null);
      if (updatedProfile.photoUrl) {
        setUploadedPreviewUrl(updatedProfile.photoUrl);
      }
      setPhotoSuccess("Profile photo updated and published.");
      await Promise.all([
        refreshSession(),
        queryClient.invalidateQueries({ queryKey: queryKeys.students.all }),
      ]);
    },
    onError: (err: unknown) => {
      setPhotoSuccess(null);
      const message =
        err instanceof Error && err.message
          ? err.message
          : "Unable to upload profile photo. Please use JPEG, PNG, or WebP under 2 MB.";
      setPhotoError(message);
    },
  });

  const handlePhotoSelected = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) {
      return;
    }
    setPhotoSuccess(null);
    if (!ALLOWED_PHOTO_TYPES.has(file.type)) {
      setPhotoError("Please select a valid JPEG, PNG, or WebP image.");
      return;
    }
    if (file.size <= 0 || file.size > MAX_PHOTO_BYTES) {
      setPhotoError("Profile photo must not exceed 2 MB.");
      return;
    }
    setPhotoError(null);
    photoUploadMutation.mutate(file);
  };

  const rankedBuilders: RankedShowcaseBuilder[] = students.map((student, index) => ({
    ...student,
    rank: index + 1,
  }));

  const q = search.trim().toLowerCase();
  const filteredBuilders = rankedBuilders.filter(
    (b) =>
      !q ||
      `${b.firstName} ${b.lastName}`.toLowerCase().includes(q) ||
      b.universityName.toLowerCase().includes(q) ||
      b.facultyCode.toLowerCase().includes(q) ||
      b.topSkills.some((sk) => sk.toLowerCase().includes(q)),
  );

  const totalCount = filteredBuilders.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const safePage = Math.min(page, totalPages);
  const pagedBuilders = filteredBuilders.slice((safePage - 1) * pageSize, safePage * pageSize);

  const columns: readonly DataTableColumn<RankedShowcaseBuilder>[] = [
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
      cell: (row) => {
        const fullName = `${row.firstName} ${row.lastName}`;
        return (
          <div className="flex items-center gap-3">
            <img
              src={row.photoUrl}
              alt={fullName}
              className="size-9 rounded-full object-cover object-top ring-2 ring-white/15"
            />
            <div>
              <p className="font-semibold text-white">{fullName}</p>
              <p className="text-xs text-[#9ca3af]">
                {row.facultyCode} · Year {row.academicYear}
              </p>
            </div>
          </div>
        );
      },
    },
    {
      key: "university",
      header: "University",
      cell: (row) => (
        <div className="flex items-center gap-2">
          <UniversityLogo
            name={row.universityName}
            logoUrl={row.universityLogoUrl ?? resolveLogo(row.universityName)}
            size="sm"
          />
          <span className="text-xs font-medium text-white/90">{row.universityName}</span>
        </div>
      ),
    },
    {
      key: "xp",
      header: "Verified XP",
      cell: (row) => (
        <span className="font-mono text-xs font-bold tabular-nums text-[#d3fb52]">
          {row.totalPoints} XP · Lvl {row.level}
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
            {rankedBuilders.slice(0, 3).map((leader, idx) => {
              const fullName = `${leader.firstName} ${leader.lastName}`;
              return (
                <div
                  key={leader.studentId}
                  className={cn(
                    "flex items-center gap-3 rounded-2xl border bg-[#14151c]/88 p-4 text-left backdrop-blur-md",
                    idx === 0 ? "border-[#d3fb52]/60" : "border-white/12",
                  )}
                >
                  <img
                    src={leader.photoUrl}
                    alt={fullName}
                    className="size-12 shrink-0 rounded-2xl object-cover object-top"
                  />
                  <div className="min-w-0">
                    <div className="flex items-center gap-1.5">
                      <Trophy className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
                      <span className="font-mono text-[11px] font-bold text-[#d3fb52]">
                        #{leader.rank} · {leader.totalPoints} XP
                      </span>
                    </div>
                    <p className="truncate text-sm font-bold text-white">{fullName}</p>
                    <p className="truncate text-[11px] text-[#9ca3af]">{leader.universityName}</p>
                  </div>
                </div>
              );
            })}
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
            keyExtractor={(row) => row.studentId}
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
    const activePhotoUrl = uploadedPreviewUrl ?? user?.photoUrl ?? user?.profile?.photoUrl ?? null;

    return (
      <section className="space-y-6 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-6 sm:p-8">
        <div className="teamup-hero-bg flex flex-col justify-between gap-6 rounded-2xl border border-white/12 p-6 sm:flex-row sm:items-center">
          <div className="flex items-center gap-4">
            {isAuthenticated && user && (
              <Avatar
                src={activePhotoUrl}
                name={`${user.firstName} ${user.lastName}`}
                size="lg"
              />
            )}
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
          </div>

          {isAuthenticated && user ? (
            <div className="flex shrink-0 flex-col items-start gap-2 sm:items-end">
              <input
                ref={fileInputRef}
                type="file"
                aria-label="Upload profile photo"
                accept="image/jpeg,image/png,image/webp"
                onChange={handlePhotoSelected}
                className="sr-only"
              />
              <button
                type="button"
                disabled={photoUploadMutation.isPending}
                onClick={() => fileInputRef.current?.click()}
                className="inline-flex h-11 cursor-pointer items-center gap-2 rounded-full bg-[#d3fb52] px-5 text-sm font-bold text-[#14151c] transition-colors hover:bg-[#deff6e] disabled:cursor-wait disabled:opacity-60"
              >
                {photoUploadMutation.isPending ? (
                  <Loader2 className="size-4 animate-spin" aria-hidden="true" />
                ) : (
                  <Camera className="size-4" aria-hidden="true" />
                )}
                <span>{photoUploadMutation.isPending ? "Uploading..." : "Upload Photo"}</span>
              </button>
              <span className="text-[11px] text-white/70">JPEG, PNG or WebP · max 2 MB</span>
            </div>
          ) : (
            <Link
              to="/auth/login"
              className="inline-flex h-11 shrink-0 items-center gap-2 rounded-full bg-[#d3fb52] px-6 text-sm font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
            >
              Sign In to Manage Profile
            </Link>
          )}
        </div>

        {photoError && (
          <div
            role="alert"
            className="rounded-2xl border border-red-400/35 bg-red-500/10 px-4 py-3 text-sm text-red-200"
          >
            {photoError}
          </div>
        )}

        {photoSuccess && (
          <div
            role="status"
            className="rounded-2xl border border-[#d3fb52]/35 bg-[#d3fb52]/10 px-4 py-3 text-sm text-[#d3fb52]"
          >
            {photoSuccess}
          </div>
        )}

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
          {pagedBuilders.map((builder) => {
            const fullName = `${builder.firstName} ${builder.lastName}`;
            return (
              <article
                key={builder.studentId}
                className="flex flex-col justify-between gap-4 rounded-2xl border border-white/[0.085] bg-[#14151c] p-5 transition-colors hover:border-[#d3fb52]/40"
              >
                <div className="space-y-3">
                  <div className="flex items-center justify-between gap-3">
                    <div className="flex items-center gap-3">
                      <img
                        src={builder.photoUrl}
                        alt={fullName}
                        className="size-12 rounded-2xl object-cover object-top"
                      />
                      <div>
                        <h3
                          className="text-base font-bold text-white"
                          style={{ fontFamily: "var(--font-heading)" }}
                        >
                          {fullName}
                        </h3>
                        <p className="font-mono text-xs text-[#9ca3af]">
                          {builder.facultyCode} · Year {builder.academicYear}
                        </p>
                      </div>
                    </div>
                    <UniversityLogo
                      name={builder.universityName}
                      logoUrl={builder.universityLogoUrl ?? resolveLogo(builder.universityName)}
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
                    {builder.totalPoints} XP · {builder.badgesCount} badges
                  </span>
                  <Link
                    to={`/students/${builder.studentId}`}
                    className="font-semibold text-white/80 hover:text-[#d3fb52]"
                  >
                    View Profile →
                  </Link>
                </div>
              </article>
            );
          })}
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
