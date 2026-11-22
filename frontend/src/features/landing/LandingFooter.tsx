/**
 * @date 2026-11-22
 * @purpose Flagship Landing Footer with 3-column clickable sitemap links and one-shot burst TeamUp wordmark.
 */
import { Link } from "react-router-dom";
import { useInViewOnce } from "@/hooks/useInViewOnce";

export function LandingFooter() {
  const [footerRef, isVisible] = useInViewOnce<HTMLElement>({ threshold: 0.15 });

  return (
    <footer
      id="footer-section"
      ref={footerRef}
      className="relative z-20 bg-[#14151c] px-4 pb-12 pt-12 sm:px-6 sm:pb-16 sm:pt-16 md:px-12 md:pb-24 md:pt-24 select-none overflow-x-hidden border-t border-white/[0.08]"
    >
      <div className="mx-auto max-w-[1220px]">
        <div className="grid grid-cols-1 gap-10 pb-10 sm:gap-12 sm:pb-14 lg:grid-cols-12 lg:gap-8 md:pb-20">
          {/* Left Column: Tagline */}
          <div
            className={`transition-all duration-700 ease-out lg:col-span-5 ${
              isVisible ? "translate-x-0 opacity-100" : "-translate-x-8 opacity-0"
            }`}
          >
            <h2
              className="text-[28px] font-normal leading-[1.14] tracking-[-0.025em] text-white sm:text-[38px] md:text-[44px]"
              style={{ fontFamily: "'Plus Jakarta Sans', 'DM Sans', sans-serif" }}
            >
              The collaboration network
              <br />
              for university students
            </h2>
            <p className="mt-4 max-w-sm text-sm text-[#9ca3af]">
              Empowering Egyptian campus builders to assemble multidisciplinary project squads,
              track verified activities, and grow technical careers.
            </p>
          </div>

          {/* Right Column: Clickable SPA Sitemap Links */}
          <div
            className={`transition-all duration-700 delay-150 ease-out lg:col-span-7 ${
              isVisible ? "translate-x-0 opacity-100" : "translate-x-8 opacity-0"
            }`}
          >
            <div className="grid grid-cols-2 gap-x-6 gap-y-8 text-left sm:grid-cols-3 sm:gap-x-10 sm:gap-y-12">
              {/* Column 1: Students & Scoring */}
              <div className="flex flex-col gap-8 sm:gap-12">
                <div>
                  <div className="mb-3 text-[13.5px] font-semibold text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                    Students
                  </div>
                  <ul className="space-y-2.5 text-[14px] text-white/90 sm:space-y-3.5 sm:text-[15px]">
                    <li>
                      <Link to="/students" className="hover:text-[#d3fb52] transition-colors">
                        Find teammates
                      </Link>
                    </li>
                    <li>
                      <Link to="/projects" className="hover:text-[#d3fb52] transition-colors">
                        Explore projects
                      </Link>
                    </li>
                    <li>
                      <Link to="/profile/me" className="hover:text-[#d3fb52] transition-colors">
                        My profile studio
                      </Link>
                    </li>
                  </ul>
                </div>

                <div>
                  <div className="mb-3 text-[13.5px] font-semibold text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                    TeamUp Scoring
                  </div>
                  <ul className="space-y-2.5 text-[14px] text-white/90 sm:space-y-3.5 sm:text-[15px]">
                    <li>
                      <Link to="/gamification" className="hover:text-[#d3fb52] transition-colors">
                        Immutable score ledger
                      </Link>
                    </li>
                    <li>
                      <Link to="/gamification" className="hover:text-[#d3fb52] transition-colors">
                        Proficiency levels
                      </Link>
                    </li>
                    <li>
                      <Link to="/gamification" className="hover:text-[#d3fb52] transition-colors">
                        Achievement badges
                      </Link>
                    </li>
                    <li>
                      <Link to="/leaderboard" className="hover:text-[#d3fb52] transition-colors">
                        Campus leaderboards
                      </Link>
                    </li>
                  </ul>
                </div>
              </div>

              {/* Column 2: Activities & Events */}
              <div className="flex flex-col gap-8 sm:gap-12">
                <div>
                  <div className="mb-3 text-[13.5px] font-semibold text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                    Student Activities
                  </div>
                  <ul className="space-y-2.5 text-[14px] text-white/90 sm:space-y-3.5 sm:text-[15px]">
                    <li>
                      <Link to="/organizations" className="hover:text-[#d3fb52] transition-colors">
                        Clubs &amp; societies
                      </Link>
                    </li>
                    <li>
                      <Link to="/organizations" className="hover:text-[#d3fb52] transition-colors">
                        Recruitment tracks
                      </Link>
                    </li>
                  </ul>
                </div>

                <div>
                  <div className="mb-3 text-[13.5px] font-semibold text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                    Events &amp; Workshops
                  </div>
                  <ul className="space-y-2.5 text-[14px] text-white/90 sm:space-y-3.5 sm:text-[15px]">
                    <li>
                      <Link to="/events" className="hover:text-[#d3fb52] transition-colors">
                        Multi-session bootcamps
                      </Link>
                    </li>
                    <li>
                      <Link to="/check-in" className="hover:text-[#d3fb52] transition-colors">
                        QR attendance console
                      </Link>
                    </li>
                    <li>
                      <Link to="/events" className="hover:text-[#d3fb52] transition-colors">
                        Competitions
                      </Link>
                    </li>
                  </ul>
                </div>
              </div>

              {/* Column 3: Platform */}
              <div className="col-span-2 grid grid-cols-2 gap-6 sm:col-span-1 sm:flex sm:flex-col sm:gap-12">
                <div>
                  <div className="mb-3 text-[13.5px] font-semibold text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                    Platform
                  </div>
                  <ul className="space-y-2.5 text-[14px] text-white/90 sm:space-y-3.5 sm:text-[15px]">
                    <li>
                      <Link to="/" className="hover:text-[#d3fb52] transition-colors">
                        About TeamUp
                      </Link>
                    </li>
                    <li>
                      <Link to="/skills" className="hover:text-[#d3fb52] transition-colors">
                        Skills catalog
                      </Link>
                    </li>
                    <li>
                      <Link to="/projects" className="hover:text-[#d3fb52] transition-colors">
                        Project task boards
                      </Link>
                    </li>
                  </ul>
                </div>

                <div>
                  <div className="mb-3 text-[13.5px] font-semibold text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                    Account
                  </div>
                  <ul className="space-y-2.5 text-[14px] text-white/90 sm:space-y-3.5 sm:text-[15px]">
                    <li>
                      <Link to="/auth/login" className="hover:text-[#d3fb52] transition-colors">
                        Sign In
                      </Link>
                    </li>
                    <li>
                      <Link to="/auth/register" className="hover:text-[#d3fb52] transition-colors">
                        Register free
                      </Link>
                    </li>
                    <li>
                      <Link to="/notifications" className="hover:text-[#d3fb52] transition-colors">
                        Notifications
                      </Link>
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Bottom Giant Full-Width Brand Wordmark (One-Shot Burst Animation) */}
        <div className="flex w-full items-center justify-center px-4 py-6 sm:px-8 sm:py-8 md:px-14 md:py-12">
          <div
            id="footer-brand-burst"
            className="relative inline-flex origin-center scale-x-[1.14] items-baseline justify-center whitespace-nowrap text-center font-black italic leading-[1.15] tracking-[0.02em] text-[#d3fb52] sm:scale-x-[1.24] sm:tracking-[0.03em]"
            style={{
              fontFamily: "'Barlow Condensed', sans-serif",
              fontSize: "clamp(3.1rem, 18.2vw, 18.2rem)",
            }}
          >
            <span
              className={`relative inline-block ${isVisible ? "animate-[letterBurstPop_0.65s_cubic-bezier(0.16,1,0.3,1)_both]" : "opacity-0"}`}
              style={{ animationDelay: "0ms" }}
            >
              T
            </span>
            <span
              className={`relative inline-block ${isVisible ? "animate-[letterBurstPop_0.65s_cubic-bezier(0.16,1,0.3,1)_both]" : "opacity-0"}`}
              style={{ animationDelay: "70ms" }}
            >
              e
            </span>
            <span
              className={`relative inline-block ${isVisible ? "animate-[letterBurstPop_0.65s_cubic-bezier(0.16,1,0.3,1)_both]" : "opacity-0"}`}
              style={{ animationDelay: "140ms" }}
            >
              a
            </span>
            <span
              className={`relative inline-block ${isVisible ? "animate-[letterBurstPop_0.65s_cubic-bezier(0.16,1,0.3,1)_both]" : "opacity-0"}`}
              style={{ animationDelay: "210ms" }}
            >
              m
            </span>
            <span
              className={`relative inline-block ${isVisible ? "animate-[letterBurstPop_0.65s_cubic-bezier(0.16,1,0.3,1)_both]" : "opacity-0"}`}
              style={{ animationDelay: "280ms" }}
            >
              U
            </span>
            <span
              className={`relative inline-block ${isVisible ? "animate-[letterBurstPop_0.65s_cubic-bezier(0.16,1,0.3,1)_both]" : "opacity-0"}`}
              style={{ animationDelay: "350ms" }}
            >
              p
              <span
                className={`burst-rays absolute -top-[10%] left-1/2 flex -translate-x-1/2 items-center gap-2 sm:gap-3 ${
                  isVisible ? "animate-[burstRaysPop_0.6s_ease-out_both]" : "opacity-0"
                }`}
                style={{ animationDelay: "480ms" }}
                aria-hidden="true"
              >
                <span className="h-3.5 w-1 origin-bottom -rotate-35 rounded-full bg-[#d3fb52] sm:h-5 sm:w-1.5 md:h-7 md:w-2" />
                <span className="h-4 w-1 -translate-y-1 rounded-full bg-[#d3fb52] sm:h-6 sm:w-1.5 md:h-8 md:w-2" />
                <span className="h-3.5 w-1 origin-bottom rotate-35 rounded-full bg-[#d3fb52] sm:h-5 sm:w-1.5 md:h-7 md:w-2" />
              </span>
            </span>
          </div>
        </div>
      </div>
    </footer>
  );
}
