/**
 * @date 2026-11-24
 * @purpose Handshake Rosetta dark burst footer for TeamUp landing page.
 */
import { forwardRef, type CSSProperties } from "react";
import { Link } from "react-router-dom";

export interface LandingFooterSectionProps {
  footerVisible: boolean;
  footerLinksRevealed: boolean;
}

export const LandingFooterSection = forwardRef<HTMLElement, LandingFooterSectionProps>(
  function LandingFooterSection({ footerVisible, footerLinksRevealed }, ref) {
    return (
      <footer
        id="footer-section"
        ref={ref}
        className={`relative z-20 select-none overflow-hidden bg-[#14151c] px-4 pb-12 pt-12 sm:px-6 sm:pb-16 sm:pt-16 md:px-12 md:pb-24 md:pt-24 ${
          footerVisible ? "is-footer-looping" : ""
        }`}
      >
        <div aria-hidden="true" className="footer-ambient-glow" />
        <div className="mx-auto max-w-[1220px]">
          <div className="grid grid-cols-1 gap-10 pb-10 sm:gap-12 sm:pb-14 lg:grid-cols-12 lg:gap-8 md:pb-20">
            {/* Left Column: Tagline */}
            <div
              id="footer-left-col"
              className={`footer-left-reveal-ltr lg:col-span-5 ${
                footerLinksRevealed ? "is-visible" : ""
              }`}
            >
              <h2
                className="text-[28px] font-normal leading-[1.14] tracking-[-0.025em] text-white sm:text-[38px] md:text-[44px]"
                style={{ fontFamily: "'Plus Jakarta Sans', 'DM Sans', sans-serif" }}
              >
                The collaboration network
                <br />
                for students &amp; communities
              </h2>
            </div>

            {/* Right Column: 3 Directory Columns */}
            <div
              id="footer-right-col"
              className={`footer-right-reveal lg:col-span-7 ${
                footerLinksRevealed ? "is-visible" : ""
              }`}
            >
              <div className="grid grid-cols-2 gap-x-6 gap-y-8 text-left sm:grid-cols-3 sm:gap-x-10 sm:gap-y-12">
                <div className="flex flex-col gap-8 sm:gap-12">
                  <div>
                    <div className="mb-3 text-[13.5px] font-normal text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                      Students &amp; Builders
                    </div>
                    <ul className="space-y-2.5 text-[14px] font-normal text-white sm:space-y-3.5 sm:text-[15px]">
                      <li>
                        <Link to="/students" className="hover:text-[#d3fb52]">
                          Find teammates
                        </Link>
                      </li>
                      <li>
                        <Link to="/projects" className="hover:text-[#d3fb52]">
                          Explore projects
                        </Link>
                      </li>
                    </ul>
                  </div>

                  <div>
                    <div className="mb-3 text-[13.5px] font-normal text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                      TeamUp Scoring
                    </div>
                    <ul className="space-y-2.5 text-[14px] font-normal text-white sm:space-y-3.5 sm:text-[15px]">
                      <li>Verified score history</li>
                      <li>Proficiency levels</li>
                      <li>Achievement badges</li>
                      <li>
                        <Link to="/leaderboard" className="hover:text-[#d3fb52]">
                          Campus leaderboards
                        </Link>
                      </li>
                    </ul>
                  </div>
                </div>

                <div className="flex flex-col gap-8 sm:gap-12">
                  <div>
                    <div className="mb-3 text-[13.5px] font-normal text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                      Student Activities
                    </div>
                    <ul className="space-y-2.5 text-[14px] font-normal text-white sm:space-y-3.5 sm:text-[15px]">
                      <li>
                        <Link to="/organizations" className="hover:text-[#d3fb52]">
                          Clubs &amp; open communities
                        </Link>
                      </li>
                      <li>
                        <Link to="/organizations?search=IEEE" className="hover:text-[#d3fb52]">
                          IEEE Branches &amp; Recruitment
                        </Link>
                      </li>
                    </ul>
                  </div>

                  <div>
                    <div className="mb-3 text-[13.5px] font-normal text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                      Events &amp; Workshops
                    </div>
                    <ul className="space-y-2.5 text-[14px] font-normal text-white sm:space-y-3.5 sm:text-[15px]">
                      <li>
                        <Link to="/events" className="hover:text-[#d3fb52]">
                          Multi-session bootcamps
                        </Link>
                      </li>
                      <li>
                        <Link to="/check-in" className="hover:text-[#d3fb52]">
                          QR &amp; camera check-in
                        </Link>
                      </li>
                      <li>
                        <Link to="/events" className="hover:text-[#d3fb52]">
                          Host an event
                        </Link>
                      </li>
                    </ul>
                  </div>
                </div>

                <div className="col-span-2 grid grid-cols-2 gap-6 sm:col-span-1 sm:flex sm:flex-col sm:gap-12">
                  <div>
                    <div className="mb-3 text-[13.5px] font-normal text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                      Platform
                    </div>
                    <ul className="space-y-2.5 text-[14px] font-normal text-white sm:space-y-3.5 sm:text-[15px]">
                      <li>About TeamUp</li>
                      <li>University &amp; open network</li>
                      <li>Project task boards</li>
                      <li>
                        <Link to="/skills" className="hover:text-[#d3fb52]">
                          Skill profiles
                        </Link>
                      </li>
                    </ul>
                  </div>

                  <div>
                    <div className="mb-3 text-[13.5px] font-normal text-[#8e929b] sm:mb-4 sm:text-[14.5px]">
                      Support
                    </div>
                    <ul className="space-y-2.5 text-[14px] font-normal text-white sm:space-y-3.5 sm:text-[15px]">
                      <li>Student help center</li>
                      <li>Contact</li>
                      <li>Academic integrity</li>
                      <li>
                        <Link to="/privacy" className="hover:text-[#d3fb52]">
                          Privacy &amp; Policy
                        </Link>
                      </li>
                    </ul>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Footer Horizon Divider & Caption */}
        <div className="mx-auto max-w-[1220px]">
          <div className="flex flex-wrap items-center justify-between gap-3 border-t border-white/[0.08] py-4 text-xs text-[#9ca3af]">
            <span>TeamUp · Egyptian Campus Collaboration Network</span>
          </div>
          <div aria-hidden="true" className="footer-horizon-line" />
        </div>

        {/* Bottom Giant Full-Width Brand Wordmark (Scroll-Stage & Continuous Loop) */}
        <div className="footer-scroll-stage relative flex w-full items-center justify-center px-4 py-4 sm:px-8 sm:py-6 md:px-14 md:py-10">
          <div
            id="footer-brand-burst"
            className={`relative inline-flex origin-center scale-x-[1.14] items-baseline justify-center whitespace-nowrap pr-[0.08em] text-center font-black italic leading-[1.15] tracking-[0.02em] text-[#d3fb52] sm:scale-x-[1.24] sm:tracking-[0.03em] ${
              footerVisible ? "is-burst-visible is-footer-looping" : ""
            }`}
            style={{
              fontFamily: "'Barlow Condensed', sans-serif",
              fontSize: "clamp(2.85rem, 18.2vw, 18.2rem)",
            }}
          >
            <span className="burst-letter relative inline-block" style={{ "--letter-delay": "0ms" } as CSSProperties}>
              T
            </span>
            <span className="burst-letter relative inline-block" style={{ "--letter-delay": "95ms" } as CSSProperties}>
              e
            </span>
            <span className="burst-letter relative inline-block" style={{ "--letter-delay": "190ms" } as CSSProperties}>
              a
            </span>
            <span className="burst-letter relative inline-block" style={{ "--letter-delay": "285ms" } as CSSProperties}>
              m
            </span>
            <span className="burst-letter relative inline-block" style={{ "--letter-delay": "380ms" } as CSSProperties}>
              U
            </span>
            <span className="burst-letter relative inline-block" style={{ "--letter-delay": "475ms" } as CSSProperties}>
              p
              <span
                className="burst-rays absolute -top-[10%] left-1/2 flex -translate-x-1/2 items-center gap-2 sm:gap-3"
                aria-hidden="true"
              >
                <span className="h-3.5 w-1 origin-bottom -rotate-35 rounded-full bg-[#d3fb52] sm:h-5 sm:w-1.5 md:h-7 md:w-2" />
                <span className="h-4 w-1 -translate-y-1 rounded-full bg-[#d3fb52] sm:h-6 sm:w-1.5 md:h-8 md:w-2" />
                <span className="h-3.5 w-1 origin-bottom rotate-35 rounded-full bg-[#d3fb52] sm:h-5 sm:w-1.5 md:h-7 md:w-2" />
              </span>
            </span>
          </div>
        </div>
      </footer>
    );
  },
);
