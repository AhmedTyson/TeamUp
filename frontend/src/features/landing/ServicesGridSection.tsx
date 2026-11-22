/**
 * @date 2026-11-22
 * @purpose 3x2 Spotlight Cards grid linking to core platform service routes with mouse-tracked coords.
 */
import { ArrowUpRight } from "lucide-react";
import { type MouseEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useInViewOnce } from "@/hooks/useInViewOnce";

interface PlatformServiceItem {
  id: string;
  to: string;
  title: string;
  subtitleMetric: string;
  tagsLine: string;
}

const PLATFORM_SERVICES: readonly PlatformServiceItem[] = [
  {
    id: "team-formation",
    to: "/projects",
    title: "Team Formation & Projects",
    subtitleMetric: "Up to +250 pts / project",
    tagsLine: "Create Projects · Publish Skills · Find Teammates · Task Board",
  },
  {
    id: "student-profiles",
    to: "/students",
    title: "Student Profiles & Scoring",
    subtitleMetric: "Verified Skill Portfolio",
    tagsLine: "First Year · Sophomores · Juniors · Seniors · Graduates",
  },
  {
    id: "student-activities",
    to: "/organizations",
    title: "Student Activities Hub",
    subtitleMetric: "50+ Campus Organizations",
    tagsLine: "Clubs Directory · Recruitment · Join Requests · Active Roles",
  },
  {
    id: "events-workshops",
    to: "/events",
    title: "Events & Workshops",
    subtitleMetric: "Up to +100 pts / workshop",
    tagsLine: "University Events · Workshops · Competitions · Multi-Session",
  },
  {
    id: "attendance-verification",
    to: "/events",
    title: "Attendance Verification",
    subtitleMetric: "Instant Session Check-In",
    tagsLine: "QR Code Scan · RFID Verification · Manual Check-In · Capacity",
  },
  {
    id: "gamification-engine",
    to: "/gamification",
    title: "Gamification & Leaderboards",
    subtitleMetric: "Points, Levels & Badges",
    tagsLine: "Score Ledger · Level Formula · Unlock Badges · Smart Inbox",
  },
];

function updateSpotlightCoords(event: MouseEvent<HTMLElement>) {
  const rect = event.currentTarget.getBoundingClientRect();
  event.currentTarget.style.setProperty("--mouse-x", `${event.clientX - rect.left}px`);
  event.currentTarget.style.setProperty("--mouse-y", `${event.clientY - rect.top}px`);
}

export function ServicesGridSection() {
  const navigate = useNavigate();
  const [ref, isVisible] = useInViewOnce<HTMLElement>({ threshold: 0.1 });

  return (
    <section
      id="services"
      ref={ref}
      className="relative z-10 w-full bg-[#14151c] px-4 pb-14 pt-14 sm:px-6 sm:pb-20 sm:pt-20 md:px-12"
    >
      <div className="mx-auto max-w-[1280px]">
        {/* Section Heading */}
        <div
          className={`mx-auto max-w-[820px] text-center transition-all duration-700 ease-out ${
            isVisible ? "translate-y-0 opacity-100" : "translate-y-8 opacity-0"
          }`}
        >
          <h2
            className="text-[26px] font-medium leading-[1.15] tracking-[-0.025em] text-white sm:text-[34px] md:text-[46px]"
            style={{ fontFamily: "'Plus Jakarta Sans', 'DM Sans', sans-serif" }}
          >
            Build skills, join teams, and level up your campus journey
          </h2>
          <p className="mt-4 text-[15.5px] font-normal leading-relaxed tracking-[-0.01em] text-white/90 sm:mt-5 sm:text-[17px] md:text-[20px]">
            Starting from your first year, collaborate on real-world projects with students, clubs,
            and faculties across your university.
          </p>
        </div>

        {/* 3x2 Services Cards Grid */}
        <div className="mt-10 grid grid-cols-1 gap-4 sm:mt-16 sm:gap-6 md:grid-cols-2 lg:grid-cols-3">
          {PLATFORM_SERVICES.map((service, index) => (
            <div
              key={service.id}
              style={{ transitionDelay: `${index * 80}ms` }}
              className={`transition-all duration-500 ${
                isVisible ? "translate-y-0 opacity-100" : "translate-y-6 opacity-0"
              }`}
            >
              <div
                role="link"
                tabIndex={0}
                onClick={() => void navigate(service.to)}
                onKeyDown={(event) => {
                  if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    void navigate(service.to);
                  }
                }}
                onMouseMove={updateSpotlightCoords}
                className="service-card group flex h-full min-h-[200px] cursor-pointer flex-col justify-between p-6 sm:min-h-[235px] sm:p-8 md:p-9 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
              >
                <div className="relative z-10 flex items-start justify-between gap-4">
                  <div>
                    <h3 className="text-[20px] font-medium leading-snug tracking-[-0.015em] text-white transition-colors duration-300 group-hover:text-[#d3fb52] sm:text-[22px] md:text-[24px]">
                      {service.title}
                    </h3>
                    <p className="mt-1.5 text-[16px] font-normal tracking-[-0.01em] text-[#9ca3af] sm:text-[18px]">
                      {service.subtitleMetric}
                    </p>
                  </div>
                  <span className="flex size-9 shrink-0 items-center justify-center rounded-full border border-white/[0.08] bg-white/[0.04] text-white/40 transition-all duration-300 group-hover:-translate-y-0.5 group-hover:translate-x-0.5 group-hover:border-[#d3fb52] group-hover:bg-[#d3fb52] group-hover:text-[#14151c]">
                    <ArrowUpRight className="size-4 stroke-[2.2]" aria-hidden="true" />
                  </span>
                </div>

                <div className="relative z-10 pt-8 sm:pt-10">
                  <p className="text-[13.5px] font-medium leading-relaxed tracking-[-0.01em] text-white group-hover:text-white/95 sm:text-[14.5px]">
                    {service.tagsLine}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
