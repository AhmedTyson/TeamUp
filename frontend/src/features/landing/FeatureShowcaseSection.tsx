/**
 * @date 2026-11-22
 * @purpose 4 alternating gradient showcase rows with elevated cards and live SPA links.
 */
import { ArrowUpRight, Award, CheckCircle2, QrCode, Sparkles, Users } from "lucide-react";
import { Link } from "react-router-dom";
import { useInViewOnce } from "@/hooks/useInViewOnce";

export function FeatureShowcaseSection() {
  const [row0Ref, row0Visible] = useInViewOnce<HTMLDivElement>({ threshold: 0.1 });
  const [row1Ref, row1Visible] = useInViewOnce<HTMLDivElement>({ threshold: 0.1 });
  const [row2Ref, row2Visible] = useInViewOnce<HTMLDivElement>({ threshold: 0.1 });
  const [row3Ref, row3Visible] = useInViewOnce<HTMLDivElement>({ threshold: 0.1 });

  return (
    <section className="relative z-10 w-full overflow-hidden bg-[#14151c] px-4 pb-20 pt-8 sm:px-6 sm:pb-32 sm:pt-12 md:px-12">
      <div className="mx-auto flex max-w-[1220px] flex-col gap-16 sm:gap-24 md:gap-36">
        {/* ROW 1: Visual Left · Text Right (Team Formation & Project Matching) */}
        <div
          ref={row0Ref}
          className="showcase-row grid grid-cols-1 items-center gap-8 sm:gap-10 lg:grid-cols-12 lg:gap-16"
        >
          <div
            className={`showcase-visual-reveal lg:col-span-7 ${row0Visible ? "is-visible" : ""}`}
          >
            <div className="showcase-gradient-1 group relative min-h-[310px] overflow-hidden rounded-[22px] p-4 shadow-[0_28px_60px_-15px_rgba(0,0,0,0.6)] sm:h-[360px] sm:rounded-[28px] sm:p-6 md:h-[400px] md:p-8">
              <div className="absolute bottom-[-10%] right-[-8%] flex h-[88%] w-[90%] flex-col justify-between rounded-tl-[20px] border border-white/60 bg-white/55 p-4 shadow-inner backdrop-blur-md sm:h-[86%] sm:w-[86%] sm:rounded-tl-[24px] sm:p-6">
                <div className="flex items-center gap-3 opacity-85">
                  <div className="flex size-10 shrink-0 items-center justify-center rounded-[10px] bg-[#2e7d32] text-[15px] font-bold italic text-white sm:size-11 sm:text-[17px]">
                    IoT
                  </div>
                  <div className="min-w-0">
                    <p className="truncate text-[12px] font-medium text-[#1e3a28] sm:text-[13px]">
                      Smart Campus Lab
                    </p>
                    <p className="truncate text-[14px] font-semibold text-[#112419] sm:text-[16px]">
                      Embedded Systems Engineer
                    </p>
                    <p className="truncate text-[11.5px] text-[#284b36] sm:text-[13px]">
                      3 / 5 Team Members · Mandatory: C++ · Arduino
                    </p>
                  </div>
                </div>

                <div className="space-y-2">
                  <div className="flex items-center justify-between text-xs font-semibold text-[#1e3a28]">
                    <span>Sprint Milestone: Hardware Sensor Array</span>
                    <span className="font-mono text-[#2e7d32]">In Review</span>
                  </div>
                  <div className="h-2 w-full overflow-hidden rounded-full bg-black/10">
                    <div className="h-full w-4/5 rounded-full bg-[#2e7d32]" />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div
            className={`showcase-text-reveal-rtl lg:col-span-5 ${row0Visible ? "is-visible" : ""}`}
          >
            <div className="space-y-3">
              <span className="inline-flex items-center gap-1.5 rounded-full border border-[#d3fb52]/35 bg-[#d3fb52]/10 px-3 py-1 text-xs font-bold uppercase tracking-wider text-[#d3fb52]">
                <Users className="size-3.5" aria-hidden="true" />
                Team Formation
              </span>
              <h2
                className="text-[26px] font-medium leading-[1.18] tracking-[-0.02em] text-white sm:text-[32px] md:text-[38px]"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                Assemble Cross-Disciplinary Squads in Minutes
              </h2>
              <p className="text-[15px] leading-relaxed text-[#9ca3af] sm:text-[16px]">
                Create team projects, publish required skill proficiency levels, and invite
                collaborators from Computers &amp; AI, Engineering, and Design.
              </p>
              <div className="pt-2">
                <Link
                  to="/projects"
                  className="inline-flex items-center gap-2 rounded-xl bg-[#d3fb52] px-5 py-2.5 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
                >
                  Explore Open Projects
                  <ArrowUpRight className="size-4" aria-hidden="true" />
                </Link>
              </div>
            </div>
          </div>
        </div>

        {/* ROW 2: Text Left · Visual Right (Student Activities & Guilds) */}
        <div
          ref={row1Ref}
          className="showcase-row grid grid-cols-1 items-center gap-8 sm:gap-10 lg:grid-cols-12 lg:gap-16"
        >
          <div
            className={`showcase-text-reveal-ltr order-2 lg:order-1 lg:col-span-5 ${row1Visible ? "is-visible" : ""}`}
          >
            <div className="space-y-3">
              <span className="inline-flex items-center gap-1.5 rounded-full border border-[#10b981]/35 bg-[#10b981]/10 px-3 py-1 text-xs font-bold uppercase tracking-wider text-[#10b981]">
                <CheckCircle2 className="size-3.5" aria-hidden="true" />
                Student Organizations
              </span>
              <h2
                className="text-[26px] font-medium leading-[1.18] tracking-[-0.02em] text-white sm:text-[32px] md:text-[38px]"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                Verified Campus Student Activities &amp; Guilds
              </h2>
              <p className="text-[15px] leading-relaxed text-[#9ca3af] sm:text-[16px]">
                Browse accredited student organizations across partner universities, apply during
                open recruitment cycles, and track active leadership roles.
              </p>
              <div className="pt-2">
                <Link
                  to="/organizations"
                  className="inline-flex items-center gap-2 rounded-xl border border-white/20 bg-white/10 px-5 py-2.5 text-xs font-bold text-white transition-colors hover:bg-white/20 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
                >
                  Browse Student Organizations
                  <ArrowUpRight className="size-4" aria-hidden="true" />
                </Link>
              </div>
            </div>
          </div>

          <div
            className={`showcase-visual-reveal order-1 lg:order-2 lg:col-span-7 ${row1Visible ? "is-visible" : ""}`}
          >
            <div className="showcase-gradient-2 group relative min-h-[310px] overflow-hidden rounded-[22px] p-4 shadow-[0_28px_60px_-15px_rgba(0,0,0,0.6)] sm:h-[360px] sm:rounded-[28px] sm:p-6 md:h-[400px] md:p-8">
              <div className="absolute bottom-[-10%] left-[-8%] flex h-[88%] w-[90%] flex-col justify-between rounded-tr-[20px] border border-white/60 bg-white/55 p-4 shadow-inner backdrop-blur-md sm:h-[86%] sm:w-[86%] sm:rounded-tr-[24px] sm:p-6">
                <div className="flex items-center justify-between">
                  <span className="rounded-full bg-[#15733f] px-2.5 py-0.5 text-[10px] font-bold text-white">
                    Recruitment Open
                  </span>
                  <span className="text-xs font-bold text-[#15733f]">Fall Semester</span>
                </div>
                <div>
                  <h3 className="text-base font-bold text-[#14151c] sm:text-lg">
                    IEEE Computer Society Chapter
                  </h3>
                  <p className="text-xs text-[#284b36]">
                    Technical, PR, and Event Management tracks accepting applications.
                  </p>
                </div>
                <div className="rounded-xl border border-[#15733f]/20 bg-white/70 p-2.5 text-xs">
                  <p className="font-semibold text-[#14151c]">Verified Role: Technical Member</p>
                  <p className="text-[11px] text-[#284b36]">Verified by Faculty Advisor</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* ROW 3: Visual Left · Text Right (Events & Instant Check-In) */}
        <div
          ref={row2Ref}
          className="showcase-row grid grid-cols-1 items-center gap-8 sm:gap-10 lg:grid-cols-12 lg:gap-16"
        >
          <div
            className={`showcase-visual-reveal lg:col-span-7 ${row2Visible ? "is-visible" : ""}`}
          >
            <div className="showcase-gradient-3 group relative min-h-[310px] overflow-hidden rounded-[22px] p-4 shadow-[0_28px_60px_-15px_rgba(0,0,0,0.6)] sm:h-[360px] sm:rounded-[28px] sm:p-6 md:h-[400px] md:p-8">
              <div className="absolute bottom-[-10%] right-[-8%] flex h-[88%] w-[90%] flex-col justify-between rounded-tl-[20px] border border-white/60 bg-white/55 p-4 shadow-inner backdrop-blur-md sm:h-[86%] sm:w-[86%] sm:rounded-tl-[24px] sm:p-6">
                <div className="flex items-center justify-between">
                  <span className="flex items-center gap-1 text-xs font-bold text-[#14151c]">
                    <QrCode className="size-4" aria-hidden="true" />
                    Instant QR Check-In
                  </span>
                  <span className="font-mono text-xs font-bold text-[#15733f]">+100 XP</span>
                </div>
                <div>
                  <p className="text-xs font-semibold text-[#284b36]">Workshop Hall 104</p>
                  <h3 className="text-base font-bold text-[#14151c] sm:text-lg">
                    Cloud-Native Architecture Bootcamp
                  </h3>
                </div>
                <div className="flex items-center justify-between rounded-xl bg-white/80 p-2 text-xs">
                  <span className="text-[#4b5563]">Session Attendance</span>
                  <span className="font-bold text-[#15733f]">Verified Attendance</span>
                </div>
              </div>
            </div>
          </div>

          <div
            className={`showcase-text-reveal-rtl lg:col-span-5 ${row2Visible ? "is-visible" : ""}`}
          >
            <div className="space-y-3">
              <span className="inline-flex items-center gap-1.5 rounded-full border border-[#d3fb52]/35 bg-[#d3fb52]/10 px-3 py-1 text-xs font-bold uppercase tracking-wider text-[#d3fb52]">
                <QrCode className="size-3.5" aria-hidden="true" />
                Events &amp; Check-In
              </span>
              <h2
                className="text-[26px] font-medium leading-[1.18] tracking-[-0.02em] text-white sm:text-[32px] md:text-[38px]"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                Hands-on Bootcamps with Instant QR Attendance
              </h2>
              <p className="text-[15px] leading-relaxed text-[#9ca3af] sm:text-[16px]">
                Reserve seats at multi-session bootcamps, scan rotating 60-second QR codes or campus
                RFID cards, and earn verified participation points.
              </p>
              <div className="pt-2">
                <Link
                  to="/events"
                  className="inline-flex items-center gap-2 rounded-xl bg-[#d3fb52] px-5 py-2.5 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
                >
                  Discover Campus Events
                  <ArrowUpRight className="size-4" aria-hidden="true" />
                </Link>
              </div>
            </div>
          </div>
        </div>

        {/* ROW 4: Text Left · Visual Right (Gamification, XP & Badges) */}
        <div
          ref={row3Ref}
          className="showcase-row grid grid-cols-1 items-center gap-8 sm:gap-10 lg:grid-cols-12 lg:gap-16"
        >
          <div
            className={`showcase-text-reveal-ltr order-2 lg:order-1 lg:col-span-5 ${row3Visible ? "is-visible" : ""}`}
          >
            <div className="space-y-3">
              <span className="inline-flex items-center gap-1.5 rounded-full border border-[#10b981]/35 bg-[#10b981]/10 px-3 py-1 text-xs font-bold uppercase tracking-wider text-[#10b981]">
                <Award className="size-3.5" aria-hidden="true" />
                Gamification Engine
              </span>
              <h2
                className="text-[26px] font-medium leading-[1.18] tracking-[-0.02em] text-white sm:text-[32px] md:text-[38px]"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                Verified Contributions, XP &amp; Campus Leaderboard
              </h2>
              <p className="text-[15px] leading-relaxed text-[#9ca3af] sm:text-[16px]">
                Every sprint task marked Done and workshop attended earns immutable XP recorded in
                your append-only score ledger. Climb ranks and unlock badges.
              </p>
              <div className="pt-2">
                <Link
                  to="/leaderboard"
                  className="inline-flex items-center gap-2 rounded-xl bg-[#d3fb52] px-5 py-2.5 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
                >
                  View Campus Leaderboard
                  <ArrowUpRight className="size-4" aria-hidden="true" />
                </Link>
              </div>
            </div>
          </div>

          <div
            className={`showcase-visual-reveal order-1 lg:order-2 lg:col-span-7 ${row3Visible ? "is-visible" : ""}`}
          >
            <div className="showcase-gradient-4 group relative min-h-[310px] overflow-hidden rounded-[22px] p-4 shadow-[0_28px_60px_-15px_rgba(0,0,0,0.6)] sm:h-[360px] sm:rounded-[28px] sm:p-6 md:h-[400px] md:p-8">
              <div className="absolute bottom-[-10%] left-[-8%] flex h-[88%] w-[90%] flex-col justify-between rounded-tr-[20px] border border-white/60 bg-white/55 p-4 shadow-inner backdrop-blur-md sm:h-[86%] sm:w-[86%] sm:rounded-tr-[24px] sm:p-6">
                <div className="flex items-center justify-between">
                  <span className="flex items-center gap-1.5 text-xs font-bold text-[#14151c]">
                    <Sparkles className="size-4 text-[#2e7d32]" />
                    Level Progress · Lvl 4
                  </span>
                  <span className="font-mono text-xs font-bold text-[#15733f]">1,250 XP</span>
                </div>
                <div className="space-y-1">
                  <div className="flex justify-between text-xs text-[#284b36]">
                    <span>Points to Level 5</span>
                    <span className="font-mono font-bold">350 XP needed</span>
                  </div>
                  <div className="h-2 w-full overflow-hidden rounded-full bg-black/10">
                    <div className="h-full w-[78%] rounded-full bg-[#15733f]" />
                  </div>
                </div>
                <div className="flex items-center justify-between rounded-xl bg-white/80 p-2.5 text-xs">
                  <div>
                    <p className="font-bold text-[#14151c]">Sprint Milestone Delivered</p>
                    <p className="text-[11px] text-[#4b5563]">Bilingual RAG Platform</p>
                  </div>
                  <span className="font-mono font-bold text-[#16a34a]">+250 XP</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
