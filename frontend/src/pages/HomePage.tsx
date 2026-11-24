/**
 * @date 2026-11-22
 * @purpose TeamUp Handshake ("Rosetta") landing page with Student Portrait Stage hero, spotlight services, database-backed official university logos with pagination, showcase imagery, and burst footer.
 */
import { useQuery } from "@tanstack/react-query";
import { ArrowUpRight } from "lucide-react";
import { useEffect, useRef, useState, type CSSProperties } from "react";
import { Link, useNavigate } from "react-router-dom";
import { universitiesApi } from "@/api";
import { StudentPortraitHero } from "@/components/home/StudentPortraitHero";
import {
  PARTNER_CAMPUSES_FALLBACK,
  PLATFORM_SERVICES,
  updateSpotlightCoords,
} from "@/features/landing/LandingConstants";
import { LandingFooterSection } from "@/features/landing/LandingFooterSection";
import { LandingMarqueeSection } from "@/features/landing/LandingMarqueeSection";
import { LandingPartnerUniversitiesSection } from "@/features/landing/LandingPartnerUniversitiesSection";

const FLAGSHIP_UNIVERSITY_ORDER = new Map<string, number>([
  ["Cairo University", 1],
  ["Helwan University", 2],
  ["Ain Shams University", 3],
  ["Alexandria University", 4],
  ["Mansoura University", 5],
  ["Assiut University", 6],
  ["Capital University", 7],
  ["German University in Cairo (GUC)", 8],
  ["The American University in Cairo (AUC)", 9],
  ["Zagazig University", 10],
  ["Tanta University", 11],
  ["Benha University", 12],
  ["Suez Canal University", 13],
]);

export function HomePage() {
  const navigate = useNavigate();

  const [campusPage, setCampusPage] = useState(1);
  const [campusPageSize, setCampusPageSize] = useState(9);

  const catalogQuery = useQuery({
    queryKey: ["public", "universities", "catalog"],
    queryFn: ({ signal }) => universitiesApi.getCatalog(signal),
    staleTime: 60_000,
    retry: false,
  });

  const rawUniversitiesList =
    catalogQuery.data && catalogQuery.data.length > 0
      ? catalogQuery.data
      : PARTNER_CAMPUSES_FALLBACK;
  const universitiesList = [...rawUniversitiesList].sort((a, b) => {
    const rankA = FLAGSHIP_UNIVERSITY_ORDER.get(a.name) ?? 999;
    const rankB = FLAGSHIP_UNIVERSITY_ORDER.get(b.name) ?? 999;
    if (rankA !== rankB) {
      return rankA - rankB;
    }
    return a.name.localeCompare(b.name);
  });
  const totalCampuses = universitiesList.length;
  const totalCampusPages = Math.max(1, Math.ceil(totalCampuses / campusPageSize));
  const safeCampusPage = Math.min(campusPage, totalCampusPages);
  const pagedUniversities = universitiesList.slice(
    (safeCampusPage - 1) * campusPageSize,
    safeCampusPage * campusPageSize,
  );

  // Scroll Reveal states
  const [servicesVisible, setServicesVisible] = useState(false);
  const [marqueeVisible, setMarqueeVisible] = useState(false);
  const [partnersVisible, setPartnersVisible] = useState(false);
  const [galleryVisible, setGalleryVisible] = useState(false);
  const [visibleRows, setVisibleRows] = useState<Record<number, boolean>>({});
  const [footerLinksRevealed, setFooterLinksRevealed] = useState(false);
  const [footerVisible, setFooterVisible] = useState(false);

  const servicesSectionRef = useRef<HTMLElement | null>(null);
  const marqueeSectionRef = useRef<HTMLDivElement | null>(null);
  const partnerSectionRef = useRef<HTMLDivElement | null>(null);
  const gallerySectionRef = useRef<HTMLDivElement | null>(null);
  const showcaseRowRefs = useRef<(HTMLDivElement | null)[]>([]);
  const footerSectionRef = useRef<HTMLElement | null>(null);

  useEffect(() => {
    if (typeof IntersectionObserver === "undefined") {
      setServicesVisible(true);
      setMarqueeVisible(true);
      setPartnersVisible(true);
      setGalleryVisible(true);
      setVisibleRows({ 0: true, 1: true, 2: true, 3: true });
      setFooterLinksRevealed(true);
      setFooterVisible(true);
      footerSectionRef.current?.style.setProperty("--footer-progress", "1");
      return;
    }

    const revealObserver = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (!entry.isIntersecting) {
            return;
          }
          if (entry.target === servicesSectionRef.current) {
            setServicesVisible(true);
            revealObserver.unobserve(entry.target);
          } else if (entry.target === marqueeSectionRef.current) {
            setMarqueeVisible(true);
            revealObserver.unobserve(entry.target);
          } else if (entry.target === partnerSectionRef.current) {
            setPartnersVisible(true);
            revealObserver.unobserve(entry.target);
          } else if (entry.target === gallerySectionRef.current) {
            setGalleryVisible(true);
            revealObserver.unobserve(entry.target);
          } else {
            const idxStr = entry.target.getAttribute("data-showcase-row");
            if (idxStr !== null) {
              const rowIdx = Number.parseInt(idxStr, 10);
              setVisibleRows((prev) => ({ ...prev, [rowIdx]: true }));
              revealObserver.unobserve(entry.target);
            }
          }
        });
      },
      { threshold: 0.12 },
    );

    if (servicesSectionRef.current) revealObserver.observe(servicesSectionRef.current);
    if (marqueeSectionRef.current) revealObserver.observe(marqueeSectionRef.current);
    if (partnerSectionRef.current) revealObserver.observe(partnerSectionRef.current);
    if (gallerySectionRef.current) revealObserver.observe(gallerySectionRef.current);
    showcaseRowRefs.current.forEach((rowEl) => {
      if (rowEl) revealObserver.observe(rowEl);
    });

    // Keep footerObserver as the final IntersectionObserver registered so persistent loop listeners align cleanly
    const footerObserver = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.target === footerSectionRef.current) {
            setFooterVisible(entry.isIntersecting);
            if (entry.isIntersecting) {
              setFooterLinksRevealed(true);
            }
          }
        });
      },
      { threshold: [0.08, 0.25] },
    );

    if (footerSectionRef.current) {
      footerObserver.observe(footerSectionRef.current);
    }

    let scrollRafId: number | null = null;
    const syncFooterProgress = () => {
      scrollRafId = null;
      const footerEl = footerSectionRef.current;
      if (!footerEl) {
        return;
      }
      const rect = footerEl.getBoundingClientRect();
      const vh = window.innerHeight || 900;
      const enterEnd = Math.max(0, vh - rect.height * 0.82);
      const travel = Math.max(1, vh - enterEnd);
      const rawProgress = (vh - rect.top) / travel;
      const doc = document.documentElement;
      const nearBottom = doc.scrollHeight - (window.scrollY + vh) <= 8;
      const clamped = nearBottom ? 1 : Math.max(0, Math.min(1, rawProgress));
      footerEl.style.setProperty("--footer-progress", clamped.toFixed(3));
    };

    const handleScrollOrResize = () => {
      if (scrollRafId === null) {
        scrollRafId = window.requestAnimationFrame(syncFooterProgress);
      }
    };

    syncFooterProgress();
    window.addEventListener("scroll", handleScrollOrResize, { passive: true });
    window.addEventListener("resize", handleScrollOrResize);

    return () => {
      revealObserver.disconnect();
      footerObserver.disconnect();
      if (scrollRafId !== null) {
        window.cancelAnimationFrame(scrollRafId);
      }
      window.removeEventListener("scroll", handleScrollOrResize);
      window.removeEventListener("resize", handleScrollOrResize);
    };
  }, []);

  return (
    <div id="top" className="min-h-screen w-full select-none overflow-x-clip bg-[#14151c]">
      {/* =====================================================================
           SECTION 1: HERO VIEWPORT (Handshake Rosetta Student Portrait Stage)
           ===================================================================== */}
      <StudentPortraitHero universities={universitiesList} />

      {/* =====================================================================
           SECTION 2: CORE PLATFORM SERVICES GRID & INFINITE MARQUEE
           ===================================================================== */}
      <section
        id="services"
        ref={servicesSectionRef}
        className="relative z-10 w-full bg-[#14151c] px-4 pb-14 pt-14 sm:px-6 sm:pb-20 sm:pt-20 md:px-12"
      >
        <div className="mx-auto max-w-[1280px]">
          <div
            id="services-heading"
            className={`mx-auto max-w-[820px] text-center transition-all duration-700 ease-out ${
              servicesVisible ? "translate-y-0 opacity-100" : "translate-y-8 opacity-0"
            }`}
          >
            <h2
              className="text-[26px] font-medium leading-[1.15] tracking-[-0.025em] text-white sm:text-[34px] md:text-[46px]"
              style={{ fontFamily: "'Plus Jakarta Sans', 'DM Sans', sans-serif" }}
            >
              Build skills, join teams, and level up your campus journey
            </h2>
            <p className="mt-4 text-[15.5px] font-normal leading-relaxed tracking-[-0.01em] text-white/90 sm:mt-5 sm:text-[17px] md:text-[20px]">
              Starting from your first year, collaborate on real-world projects with students,
              clubs, and faculties across your university.
            </p>
          </div>

          {/* 3x2 Services Cards Grid */}
          <div
            id="services-grid"
            className="mt-10 grid grid-cols-1 gap-4 sm:mt-16 sm:gap-6 md:grid-cols-2 lg:grid-cols-3"
          >
            {PLATFORM_SERVICES.map((service, index) => (
              <div
                key={service.id}
                style={{ "--reveal-delay": `${90 + index * 70}ms` } as CSSProperties}
                className={`reveal-stagger-item ${servicesVisible ? "is-visible" : ""}`}
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
                  className="service-card group flex h-full min-h-[200px] cursor-pointer flex-col justify-between p-6 sm:min-h-[235px] sm:p-8 md:p-9"
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
                    <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full border border-white/[0.08] bg-white/[0.04] text-white/40 transition-all duration-300 group-hover:-translate-y-0.5 group-hover:translate-x-0.5 group-hover:border-[#d3fb52] group-hover:bg-[#d3fb52] group-hover:text-[#14151c]">
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

        {/* Single-Row Seamless Right-to-Left Infinite Marquee */}
        <LandingMarqueeSection
          sectionRef={marqueeSectionRef}
          marqueeVisible={marqueeVisible}
          onNavigate={(to) => void navigate(to)}
        />

        {/* Database-Driven Official Egyptian University Network Directory with Modern Pagination */}
        <LandingPartnerUniversitiesSection
          sectionRef={partnerSectionRef}
          partnersVisible={partnersVisible}
          pagedUniversities={pagedUniversities}
          safeCampusPage={safeCampusPage}
          totalCampusPages={totalCampusPages}
          totalCampuses={totalCampuses}
          campusPageSize={campusPageSize}
          onPageChange={setCampusPage}
          onPageSizeChange={(nextSize) => {
            setCampusPageSize(nextSize);
            setCampusPage(1);
          }}
        />

        {/* Handshake Editorial Campus Gallery (High-Contrast White Inset Cards) */}
        <div ref={gallerySectionRef} className="mx-auto mt-14 max-w-[1280px]">
          <div className="grid grid-cols-1 gap-5 md:grid-cols-3">
            <div
              style={{ "--reveal-delay": "0ms" } as CSSProperties}
              className={`reveal-stagger-item ${galleryVisible ? "is-visible" : ""}`}
            >
              <Link
                to="/projects"
                className="hs-white-card group flex h-full flex-col justify-between overflow-hidden p-4 transition-transform duration-300 hover:-translate-y-1 sm:p-5"
              >
                <div className="overflow-hidden rounded-2xl bg-[#14151c]">
                  <img
                    src="/images/hero/hero-squad-lab.jpg"
                    alt="Students forming cross-faculty engineering squads"
                    loading="lazy"
                    className="h-44 w-full object-cover transition-transform duration-500 group-hover:scale-[1.03]"
                  />
                </div>
                <div className="mt-4 flex items-end justify-between gap-3">
                  <div>
                    <span className="hs-pill-tag bg-[#14151c] text-[#d3fb52]">
                      Project Matchmaking
                    </span>
                    <p className="mt-2 text-lg font-bold tracking-tight text-[#14151c]">
                      Cross-Faculty Squads
                    </p>
                    <p className="mt-0.5 text-xs text-[#4b5563]">
                      Connect engineering, AI, and design peers in minutes.
                    </p>
                  </div>
                  <span className="inline-flex size-9 shrink-0 items-center justify-center rounded-full bg-[#14151c] text-[#d3fb52] transition-transform duration-300 group-hover:-translate-y-0.5 group-hover:translate-x-0.5">
                    <ArrowUpRight className="size-4" aria-hidden="true" />
                  </span>
                </div>
              </Link>
            </div>

            <div
              style={{ "--reveal-delay": "110ms" } as CSSProperties}
              className={`reveal-stagger-item ${galleryVisible ? "is-visible" : ""}`}
            >
              <Link
                to="/events"
                className="hs-white-card group flex h-full flex-col justify-between overflow-hidden p-4 transition-transform duration-300 hover:-translate-y-1 sm:p-5"
              >
                <div className="overflow-hidden rounded-2xl bg-[#14151c]">
                  <img
                    src="/images/hero/hero-workshop-hall.jpg"
                    alt="University workshop check-in pass and QR verification"
                    loading="lazy"
                    className="h-44 w-full object-cover transition-transform duration-500 group-hover:scale-[1.03]"
                  />
                </div>
                <div className="mt-4 flex items-end justify-between gap-3">
                  <div>
                    <span className="hs-pill-tag bg-[#d3fb52] text-[#14151c]">
                      Live Campus Events
                    </span>
                    <p className="mt-2 text-lg font-bold tracking-tight text-[#14151c]">
                      Instant Workshop Check-In
                    </p>
                    <p className="mt-0.5 text-xs text-[#4b5563]">
                      Reserve bootcamp seats and verify campus attendance cleanly.
                    </p>
                  </div>
                  <span className="inline-flex size-9 shrink-0 items-center justify-center rounded-full bg-[#14151c] text-[#d3fb52] transition-transform duration-300 group-hover:-translate-y-0.5 group-hover:translate-x-0.5">
                    <ArrowUpRight className="size-4" aria-hidden="true" />
                  </span>
                </div>
              </Link>
            </div>

            <div
              style={{ "--reveal-delay": "220ms" } as CSSProperties}
              className={`reveal-stagger-item ${galleryVisible ? "is-visible" : ""}`}
            >
              <Link
                to="/leaderboard"
                className="hs-white-card group flex h-full flex-col justify-between overflow-hidden p-4 transition-transform duration-300 hover:-translate-y-1 sm:p-5"
              >
                <div className="overflow-hidden rounded-2xl bg-[#14151c]">
                  <img
                    src="/images/hero/hero-demo-day.jpg"
                    alt="Verified campus achievement badges and rankings"
                    loading="lazy"
                    className="h-44 w-full object-cover transition-transform duration-500 group-hover:scale-[1.03]"
                  />
                </div>
                <div className="mt-4 flex items-end justify-between gap-3">
                  <div>
                    <span className="hs-pill-tag bg-[#15733f] text-white">
                      Student Portfolios
                    </span>
                    <p className="mt-2 text-lg font-bold tracking-tight text-[#14151c]">
                      Verified Milestones &amp; XP
                    </p>
                    <p className="mt-0.5 text-xs text-[#4b5563]">
                      Showcase shipped projects, badges, and campus rank.
                    </p>
                  </div>
                  <span className="inline-flex size-9 shrink-0 items-center justify-center rounded-full bg-[#14151c] text-[#d3fb52] transition-transform duration-300 group-hover:-translate-y-0.5 group-hover:translate-x-0.5">
                    <ArrowUpRight className="size-4" aria-hidden="true" />
                  </span>
                </div>
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* =====================================================================
           SECTION 3: 4 ALTERNATING FEATURE SHOWCASE ROWS
           ===================================================================== */}
      <section className="relative z-10 w-full overflow-hidden bg-[#14151c] px-4 pb-20 pt-8 sm:px-6 sm:pb-32 sm:pt-12 md:px-12">
        <div className="mx-auto flex max-w-[1220px] flex-col gap-16 sm:gap-24 md:gap-36">
          {/* ROW 1: Visual Left · Text Right */}
          <div
            ref={(el) => {
              showcaseRowRefs.current[0] = el;
            }}
            data-showcase-row="0"
            className="showcase-row grid grid-cols-1 items-center gap-8 sm:gap-10 lg:grid-cols-12 lg:gap-16"
          >
            <div
              className={`showcase-visual-reveal lg:col-span-7 ${visibleRows[0] ? "is-visible" : ""}`}
            >
              <div className="showcase-gradient-1 group relative min-h-[310px] overflow-hidden rounded-[22px] p-4 shadow-[0_28px_60px_-15px_rgba(0,0,0,0.6)] sm:h-[360px] sm:rounded-[28px] sm:p-6 md:h-[400px] md:p-8">
                <div className="absolute bottom-[-10%] right-[-8%] flex h-[88%] w-[90%] flex-col justify-between rounded-tl-[20px] border border-white/60 bg-white/55 p-4 shadow-inner backdrop-blur-md sm:h-[86%] sm:w-[86%] sm:rounded-tl-[24px] sm:p-6">
                  <div className="flex items-center gap-3 opacity-85">
                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-[10px] bg-[#2e7d32] text-[15px] font-bold italic text-white sm:h-11 sm:w-11 sm:text-[17px]">
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
                      <p className="mt-0.5 truncate text-[11px] text-[#3b6149] sm:text-[12px]">
                        Faculty of Engineering (Year 2–4)
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center gap-3 opacity-80">
                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-[10px] bg-[#8ab882] text-[14px] font-bold text-[#14261a] sm:h-11 sm:w-11 sm:text-[15px]">
                      UX
                    </div>
                    <div className="min-w-0">
                      <p className="truncate text-[12px] font-medium text-[#1e3a28] sm:text-[13px]">
                        FinTech Student Startup
                      </p>
                      <p className="truncate text-[14px] font-semibold text-[#112419] sm:text-[16px]">
                        Product &amp; UI Designer
                      </p>
                    </div>
                  </div>
                </div>

                {/* Elevated Foreground White Match Card */}
                <div className="showcase-float-slow relative z-10 ml-0 mt-10 max-w-[420px] sm:ml-1 sm:mt-14 md:ml-3 md:mt-16">
                  <div className="rounded-[18px] bg-white p-4 shadow-[0_24px_48px_rgba(12,28,18,0.22)] transition-transform duration-500 group-hover:-translate-y-1.5 group-hover:scale-[1.015] sm:rounded-[20px] sm:p-5 md:p-6">
                    <div className="flex items-start gap-3 sm:gap-4">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-[10px] bg-[#9bb8ff] text-[18px] font-black italic text-[#101828] sm:h-12 sm:w-12 sm:rounded-[12px] sm:text-[22px]">
                        AI
                      </div>
                      <div className="min-w-0">
                        <p className="text-[12px] font-medium text-[#4b5563] sm:text-[13px]">
                          Graduation &amp; Hackathon Team
                        </p>
                        <h4 className="mt-0.5 text-[16.5px] font-bold tracking-[-0.015em] text-[#111827] sm:text-[19px]">
                          Full-Stack &amp; AI Teammate
                        </h4>
                        <p className="mt-1 text-[13.5px] font-medium text-[#1f2937] sm:text-[14.5px]">
                          +250 pts · 4 / 6 Members Joined
                        </p>
                        <p className="mt-1.5 text-[12px] text-[#6b7280] sm:mt-2 sm:text-[13px]">
                          Required: React · Python · FastAPI (Computer Science)
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div
              className={`showcase-text-reveal-rtl lg:col-span-5 ${
                visibleRows[0] ? "is-visible" : ""
              }`}
            >
              <div className="max-w-[430px] text-left lg:ml-auto">
                <h3
                  className="text-[26px] font-medium leading-[1.15] tracking-[-0.02em] text-white sm:text-[32px] md:text-[38px]"
                  style={{ fontFamily: "'Plus Jakarta Sans', 'DM Sans', sans-serif" }}
                >
                  Find your dream team
                </h3>
                <p className="mt-3 text-[15.5px] font-normal leading-relaxed text-white/85 sm:mt-4 sm:text-[16.5px] md:text-[17.5px]">
                  Publish your project idea, define required skills and proficiency levels, and
                  connect with collaborators across every faculty.
                </p>
                <Link
                  to="/projects"
                  className="mt-5 inline-block cursor-pointer rounded-[10px] bg-[#d3fb52] px-6 py-3 text-[15px] font-semibold tracking-[-0.01em] text-[#14151c] transition-all duration-200 hover:-translate-y-0.5 hover:bg-[#deff6e] active:scale-95 sm:mt-7"
                >
                  Get started
                </Link>
              </div>
            </div>
          </div>

          {/* ROW 2: Text Left · Visual Right */}
          <div
            ref={(el) => {
              showcaseRowRefs.current[1] = el;
            }}
            data-showcase-row="1"
            className="showcase-row grid grid-cols-1 items-center gap-8 sm:gap-10 lg:grid-cols-12 lg:gap-16"
          >
            <div
              className={`showcase-text-reveal-ltr order-2 lg:order-1 lg:col-span-5 ${
                visibleRows[1] ? "is-visible" : ""
              }`}
            >
              <div className="max-w-[430px] text-left">
                <h3
                  className="text-[26px] font-medium leading-[1.15] tracking-[-0.02em] text-white sm:text-[32px] md:text-[38px]"
                  style={{ fontFamily: "'Plus Jakarta Sans', 'DM Sans', sans-serif" }}
                >
                  Join student activities
                </h3>
                <p className="mt-3 text-[15.5px] font-normal leading-relaxed text-white/85 sm:mt-4 sm:text-[16.5px] md:text-[17.5px]">
                  Explore university organizations, apply during recruitment, and build real-world
                  experience—from your very first semester.
                </p>
                <Link
                  to="/organizations"
                  className="mt-5 inline-block cursor-pointer rounded-[10px] bg-[#d3fb52] px-6 py-3 text-[15px] font-semibold tracking-[-0.01em] text-[#14151c] transition-all duration-200 hover:-translate-y-0.5 hover:bg-[#deff6e] active:scale-95 sm:mt-7"
                >
                  Learn more
                </Link>
              </div>
            </div>

            <div
              className={`showcase-visual-reveal order-1 lg:order-2 lg:col-span-7 ${
                visibleRows[1] ? "is-visible" : ""
              }`}
            >
              <div className="showcase-gradient-2 group relative min-h-[340px] overflow-hidden rounded-[22px] pl-4 pt-5 shadow-[0_28px_60px_-15px_rgba(0,0,0,0.6)] sm:h-[380px] sm:rounded-[28px] sm:pl-8 sm:pt-8 md:h-[410px] md:pl-12 md:pt-10">
                <div className="flex h-full w-full flex-col justify-between rounded-tl-[20px] bg-white p-4 shadow-[0_20px_50px_rgba(0,0,0,0.28)] transition-transform duration-500 group-hover:translate-x-[-4px] group-hover:translate-y-[-4px] sm:rounded-tl-[24px] sm:p-6 md:p-8">
                  <div className="flex flex-wrap items-end justify-between gap-3 sm:gap-4">
                    <div>
                      <h4 className="text-[18px] font-medium leading-[1.15] tracking-[-0.02em] text-[#111827] sm:text-[22px] md:text-[26px]">
                        Open activities &amp; projects
                        <br />
                        to advance your skills
                      </h4>
                    </div>
                    <div className="flex items-center gap-4 sm:gap-6">
                      <div>
                        <p className="text-[17px] font-semibold tracking-tight text-[#111827] sm:text-[20px]">
                          50+
                        </p>
                        <p className="text-[11px] text-[#6b7280] sm:text-[11.5px]">Active clubs</p>
                      </div>
                      <div>
                        <p className="text-[17px] font-semibold tracking-tight text-[#111827] sm:text-[20px]">
                          10K+
                        </p>
                        <p className="text-[11px] text-[#6b7280] sm:text-[11.5px]">Students</p>
                      </div>
                    </div>
                  </div>

                  <div className="mt-3 flex items-center gap-2 sm:mt-4">
                    <div className="flex h-8 flex-1 items-center justify-between rounded-lg border border-gray-200 px-2.5 text-[11.5px] text-gray-400 sm:h-9 sm:px-3 sm:text-[12px]">
                      <span className="truncate">Search organizations &amp; roles</span>
                      <span className="flex h-5 w-5 shrink-0 items-center justify-center rounded bg-[#111827] text-white sm:h-6 sm:w-6">
                        <svg
                          className="h-3 w-3"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2"
                          aria-hidden="true"
                        >
                          <circle cx="11" cy="11" r="8" />
                          <path d="m21 21-4.3-4.3" />
                        </svg>
                      </span>
                    </div>
                    <div className="hidden h-9 items-center gap-1.5 rounded-lg border border-gray-200 px-3 text-[12px] text-gray-700 sm:flex">
                      <span>Faculty</span>
                      <svg
                        className="h-3 w-3"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                        aria-hidden="true"
                      >
                        <path d="m6 9 6 6 6-6" />
                      </svg>
                    </div>
                    <div className="flex h-8 shrink-0 items-center gap-1 rounded-lg border border-gray-200 px-2.5 text-[11.5px] text-gray-700 sm:h-9 sm:px-3 sm:text-[12px]">
                      <span>Category</span>
                      <svg
                        className="h-3 w-3"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                        aria-hidden="true"
                      >
                        <path d="m6 9 6 6 6-6" />
                      </svg>
                    </div>
                  </div>

                  <div className="mt-3 grid grid-cols-2 gap-2 sm:mt-4 sm:grid-cols-3 sm:gap-2.5">
                    <div
                      style={{ "--reveal-delay": "180ms" } as CSSProperties}
                      className="showcase-mini-item rounded-xl border border-gray-200 p-2.5 text-left sm:p-3"
                    >
                      <p className="text-[12px] font-semibold leading-snug text-[#111827] sm:text-[12.5px]">
                        IEEE Student Branch
                      </p>
                      <p className="mt-0.5 text-[10.5px] text-[#6b7280] sm:text-[11px]">
                        Recruiting · Tech
                      </p>
                      <p className="mt-2 text-[10px] font-medium text-[#111827] sm:mt-3 sm:text-[10.5px]">
                        Year 1 · Year 2 · Year 3
                      </p>
                    </div>
                    <div
                      style={{ "--reveal-delay": "280ms" } as CSSProperties}
                      className="showcase-mini-item rounded-xl border border-gray-200 p-2.5 text-left sm:p-3"
                    >
                      <p className="text-[12px] font-semibold leading-snug text-[#111827] sm:text-[12.5px]">
                        GDG Campus Lead
                      </p>
                      <p className="mt-0.5 text-[10.5px] text-[#6b7280] sm:text-[11px]">
                        Up to +150 pts
                      </p>
                      <p className="mt-2 text-[10px] font-medium text-[#111827] sm:mt-3 sm:text-[10.5px]">
                        CS · Engineering · AI
                      </p>
                    </div>
                    <div
                      style={{ "--reveal-delay": "380ms" } as CSSProperties}
                      className="showcase-mini-item hidden rounded-xl border border-gray-200 p-3 text-left sm:block"
                    >
                      <p className="text-[12.5px] font-semibold leading-snug text-[#111827]">
                        Enactus Entrepreneurs
                      </p>
                      <p className="mt-0.5 text-[11px] text-[#6b7280]">Open Join Request</p>
                      <p className="mt-3 text-[10.5px] font-medium text-[#111827]">
                        Business · All Faculties
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* ROW 3: Visual Left · Text Right */}
          <div
            ref={(el) => {
              showcaseRowRefs.current[2] = el;
            }}
            data-showcase-row="2"
            className="showcase-row grid grid-cols-1 items-center gap-8 sm:gap-10 lg:grid-cols-12 lg:gap-16"
          >
            <div
              className={`showcase-visual-reveal lg:col-span-7 ${visibleRows[2] ? "is-visible" : ""}`}
            >
              <div className="showcase-gradient-3 group relative min-h-[320px] overflow-hidden rounded-[22px] p-4 shadow-[0_28px_60px_-15px_rgba(0,0,0,0.6)] sm:h-[360px] sm:rounded-[28px] sm:p-6 md:h-[400px] md:p-8">
                <div className="absolute right-[-6%] top-[8%] flex h-[86%] w-[90%] flex-col justify-between rounded-tl-[20px] border border-white/60 bg-white/55 p-4 backdrop-blur-md sm:top-[10%] sm:h-[84%] sm:w-[82%] sm:rounded-tl-[24px] sm:p-6">
                  <div className="flex items-start justify-between gap-2 sm:items-center">
                    <div className="min-w-0">
                      <p className="text-[11.5px] font-semibold text-[#16382c] sm:text-[12.5px]">
                        Live Workshop · Hall B-204
                      </p>
                      <p className="truncate text-[14.5px] font-bold text-[#0f241d] sm:text-[17px]">
                        AI &amp; Cloud Architecture Workshop
                      </p>
                      <p className="mt-0.5 text-[11.5px] text-[#254d3e] sm:text-[13px]">
                        Capacity: 142 / 150 Registered · 2 Sessions
                      </p>
                    </div>
                    <span className="shrink-0 rounded-md bg-[#14221e]/10 px-2.5 py-1 text-[11px] font-semibold text-[#14221e] sm:text-[12px]">
                      Session #104
                    </span>
                  </div>

                  <div className="h-2.5 w-full overflow-hidden rounded-full bg-white/60">
                    <div className="showcase-capacity-fill h-full w-[88%] rounded-full bg-[#1b4d3e]" />
                  </div>
                </div>

                {/* Elevated Foreground White Attendance Check-In Card */}
                <div className="showcase-float-slow relative z-10 ml-0 mt-16 max-w-[430px] sm:ml-1 sm:mt-20 md:ml-4 md:mt-24">
                  <div className="rounded-[18px] bg-white p-4 shadow-[0_24px_48px_rgba(12,28,18,0.25)] transition-transform duration-500 group-hover:-translate-y-1.5 group-hover:scale-[1.015] sm:rounded-[20px] sm:p-5 md:p-6">
                    <div className="flex items-start gap-3 sm:gap-4">
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-[10px] bg-[#d3fb52] text-[#14151c] sm:h-12 sm:w-12 sm:rounded-[12px]">
                        <svg
                          className="h-5 w-5 sm:h-6 sm:w-6"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2.2"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          aria-hidden="true"
                        >
                          <rect width="5" height="5" x="3" y="3" rx="1" />
                          <rect width="5" height="5" x="16" y="3" rx="1" />
                          <rect width="5" height="5" x="3" y="16" rx="1" />
                          <path d="M21 16h-3a2 2 0 0 0-2 2v3" />
                          <path d="M21 21v.01" />
                          <path d="M12 7v3a2 2 0 0 1-2 2H7" />
                          <path d="M3 12h.01" />
                          <path d="M12 3h.01" />
                          <path d="M12 16v.01" />
                          <path d="M16 12h1" />
                          <path d="M21 12v.01" />
                          <path d="M12 21v-1" />
                        </svg>
                      </div>
                      <div className="min-w-0 flex-1">
                        <div className="flex flex-wrap items-center justify-between gap-2">
                          <p className="flex items-center gap-1.5 text-[11.5px] font-semibold text-[#16a34a] sm:text-[12.5px]">
                            <span className="relative flex size-2 shrink-0" aria-hidden="true">
                              <span className="live-status-ping absolute inline-flex h-full w-full rounded-full bg-[#16a34a]" />
                              <span className="relative inline-flex size-2 rounded-full bg-[#16a34a]" />
                            </span>
                            <span className="truncate">Attendance Verified</span>
                          </p>
                          <span className="shrink-0 font-mono text-[11px] text-[#6b7280] sm:text-[12px]">
                            QRCode · RFID
                          </span>
                        </div>
                        <h4 className="mt-1 text-[16px] font-bold tracking-[-0.015em] text-[#111827] sm:text-[18.5px]">
                          System Design Bootcamp — Session 1
                        </h4>
                        <p className="mt-1 text-[12.5px] text-[#4b5563] sm:text-[13.5px]">
                          Checked in at 10:02 AM · +100 pts awarded
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div
              className={`showcase-text-reveal-rtl lg:col-span-5 ${
                visibleRows[2] ? "is-visible" : ""
              }`}
            >
              <div className="max-w-[430px] text-left lg:ml-auto">
                <h3
                  className="text-[26px] font-medium leading-[1.15] tracking-[-0.02em] text-white sm:text-[32px] md:text-[38px]"
                  style={{ fontFamily: "'Plus Jakarta Sans', 'DM Sans', sans-serif" }}
                >
                  Register for workshops &amp; verify attendance
                </h3>
                <p className="mt-3 text-[15.5px] font-normal leading-relaxed text-white/85 sm:mt-4 sm:text-[16.5px] md:text-[17.5px]">
                  Discover campus events and multi-session workshops with real-time seat capacity and
                  instant QR, RFID, or manual check-in verification.
                </p>
                <Link
                  to="/events"
                  className="mt-5 inline-block cursor-pointer rounded-[10px] bg-[#d3fb52] px-6 py-3 text-[15px] font-semibold tracking-[-0.01em] text-[#14151c] transition-all duration-200 hover:-translate-y-0.5 hover:bg-[#deff6e] active:scale-95 sm:mt-7"
                >
                  Explore events
                </Link>
              </div>
            </div>
          </div>

          {/* ROW 4: Text Left · Visual Right */}
          <div
            ref={(el) => {
              showcaseRowRefs.current[3] = el;
            }}
            data-showcase-row="3"
            className="showcase-row grid grid-cols-1 items-center gap-8 sm:gap-10 lg:grid-cols-12 lg:gap-16"
          >
            <div
              className={`showcase-text-reveal-ltr order-2 lg:order-1 lg:col-span-5 ${
                visibleRows[3] ? "is-visible" : ""
              }`}
            >
              <div className="max-w-[430px] text-left">
                <h3
                  className="text-[26px] font-medium leading-[1.15] tracking-[-0.02em] text-white sm:text-[32px] md:text-[38px]"
                  style={{ fontFamily: "'Plus Jakarta Sans', 'DM Sans', sans-serif" }}
                >
                  Level up with verified points &amp; badges
                </h3>
                <p className="mt-3 text-[15.5px] font-normal leading-relaxed text-white/85 sm:mt-4 sm:text-[16.5px] md:text-[17.5px]">
                  Track every contribution in your immutable score ledger, unlock achievement
                  badges, and climb your university leaderboard.
                </p>
                <Link
                  to="/leaderboard"
                  className="mt-5 inline-block cursor-pointer rounded-[10px] bg-[#d3fb52] px-6 py-3 text-[15px] font-semibold tracking-[-0.01em] text-[#14151c] transition-all duration-200 hover:-translate-y-0.5 hover:bg-[#deff6e] active:scale-95 sm:mt-7"
                >
                  View leaderboard
                </Link>
              </div>
            </div>

            <div
              className={`showcase-visual-reveal order-1 lg:order-2 lg:col-span-7 ${
                visibleRows[3] ? "is-visible" : ""
              }`}
            >
              <div className="showcase-gradient-4 group relative min-h-[340px] overflow-hidden rounded-[22px] pl-4 pt-5 shadow-[0_28px_60px_-15px_rgba(0,0,0,0.6)] sm:h-[380px] sm:rounded-[28px] sm:pl-8 sm:pt-8 md:h-[410px] md:pl-12 md:pt-10">
                <div className="flex h-full w-full flex-col justify-between gap-3 rounded-tl-[20px] bg-white p-4 shadow-[0_20px_50px_rgba(0,0,0,0.28)] transition-transform duration-500 group-hover:translate-x-[-4px] group-hover:translate-y-[-4px] sm:rounded-tl-[24px] sm:p-6 md:p-8">
                  <div className="flex flex-wrap items-center justify-between gap-2 sm:gap-4">
                    <div>
                      <p className="text-[11px] font-semibold uppercase tracking-wider text-[#4b5563] sm:text-[12px]">
                        Student Scoring &amp; Achievements
                      </p>
                      <h4 className="mt-0.5 text-[19px] font-bold tracking-[-0.02em] text-[#111827] sm:text-[23px] md:text-[26px]">
                        Level 6 · 2,540 Total Points
                      </h4>
                    </div>
                    <div className="rounded-xl border border-black/[0.06] bg-[#f3f6f4] px-2.5 py-1.5 text-left sm:px-3.5 sm:py-2 sm:text-right">
                      <p className="font-mono text-[10px] text-[#4b5563] sm:text-[11px]">
                        Verified Rank
                      </p>
                      <p className="text-[12px] font-bold text-[#15733f] sm:text-[13px]">
                        Top 3% in Faculty
                      </p>
                    </div>
                  </div>

                  <div className="my-1 grid grid-cols-1 gap-2 min-[380px]:grid-cols-2 sm:my-2 sm:grid-cols-3 sm:gap-2.5">
                    <div
                      style={{ "--reveal-delay": "160ms" } as CSSProperties}
                      className="showcase-mini-item flex min-w-0 items-center gap-2 rounded-xl border border-gray-200/80 bg-[#f8faf9] p-2.5 sm:gap-2.5 sm:p-3"
                    >
                      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-[#d3fb52] text-[#14151c] sm:h-9 sm:w-9">
                        <svg
                          className="h-4 w-4"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2"
                          aria-hidden="true"
                        >
                          <path d="M6 9H4.5a2.5 2.5 0 0 1 0-5H6" />
                          <path d="M18 9h1.5a2.5 2.5 0 0 0 0-5H18" />
                          <path d="M4 22h16" />
                          <path d="M10 14.66V17c0 .55-.47.98-.97 1.21C7.85 18.75 7 20.24 7 22" />
                          <path d="M14 14.66V17c0 .55.47.98.97 1.21C16.15 18.75 17 20.24 17 22" />
                          <path d="M18 2H6v7a6 6 0 0 0 12 0V2Z" />
                        </svg>
                      </div>
                      <div className="overflow-hidden">
                        <p className="truncate text-[11.5px] font-bold text-[#111827] sm:text-[12px]">
                          Team Pioneer
                        </p>
                        <p className="truncate text-[10px] text-[#6b7280] sm:text-[10.5px]">
                          3+ Projects
                        </p>
                      </div>
                    </div>

                    <div
                      style={{ "--reveal-delay": "250ms" } as CSSProperties}
                      className="showcase-mini-item flex min-w-0 items-center gap-2 rounded-xl border border-gray-200/80 bg-[#f8faf9] p-2.5 sm:gap-2.5 sm:p-3"
                    >
                      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-[#15733f] text-white sm:h-9 sm:w-9">
                        <svg
                          className="h-4 w-4"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2"
                          aria-hidden="true"
                        >
                          <path d="M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z" />
                          <path d="m9 12 2 2 4-4" />
                        </svg>
                      </div>
                      <div className="overflow-hidden">
                        <p className="truncate text-[11.5px] font-bold text-[#111827] sm:text-[12px]">
                          Verified Attendee
                        </p>
                        <p className="truncate text-[10px] text-[#6b7280] sm:text-[10.5px]">
                          10+ Sessions
                        </p>
                      </div>
                    </div>

                    <div
                      style={{ "--reveal-delay": "340ms" } as CSSProperties}
                      className="showcase-mini-item hidden items-center gap-2.5 rounded-xl border border-gray-200/80 bg-[#f8faf9] p-3 sm:flex"
                    >
                      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-[#111827] text-[#d3fb52]">
                        <svg
                          className="h-4 w-4"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2"
                          aria-hidden="true"
                        >
                          <path d="M9.937 15.5A2 2 0 0 0 8.5 14.063l-6.135-1.582a.5.5 0 0 1 0-.962L8.5 9.936A2 2 0 0 0 9.937 8.5l1.582-6.135a.5.5 0 0 1 .963 0L14.063 8.5A2 2 0 0 0 15.5 9.937l6.135 1.581a.5.5 0 0 1 0 .964L15.5 14.063a2 2 0 0 0-1.437 1.437l-1.582 6.135a.5.5 0 0 1-.963 0z" />
                        </svg>
                      </div>
                      <div className="overflow-hidden">
                        <p className="truncate text-[12px] font-bold text-[#111827]">
                          Skill Master
                        </p>
                        <p className="truncate text-[10.5px] text-[#6b7280]">5+ Verified Skills</p>
                      </div>
                    </div>
                  </div>

                  <div className="flex flex-col gap-1.5 border-t border-gray-100 pt-2.5 sm:pt-3">
                    <div
                      style={{ "--reveal-delay": "420ms" } as CSSProperties}
                      className="showcase-mini-item flex items-center justify-between gap-2 text-[11.5px] sm:text-[12.5px]"
                    >
                      <span className="flex items-center gap-1.5 truncate font-medium text-[#111827]">
                        <svg
                          className="h-3.5 w-3.5 shrink-0 text-[#15733f]"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2"
                          aria-hidden="true"
                        >
                          <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
                          <circle cx="9" cy="7" r="4" />
                          <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
                          <path d="M16 3.13a4 4 0 0 1 0 7.75" />
                        </svg>
                        <span className="truncate">Completed Task: API Architecture &amp; ERD</span>
                      </span>
                      <span className="shrink-0 font-mono font-semibold text-[#16a34a]">
                        +250 pts
                      </span>
                    </div>
                    <div
                      style={{ "--reveal-delay": "500ms" } as CSSProperties}
                      className="showcase-mini-item flex items-center justify-between gap-2 text-[11.5px] sm:text-[12.5px]"
                    >
                      <span className="flex items-center gap-1.5 truncate font-medium text-[#111827]">
                        <svg
                          className="h-3.5 w-3.5 shrink-0 text-[#15733f]"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="2"
                          aria-hidden="true"
                        >
                          <circle cx="12" cy="12" r="10" />
                          <path d="m9 12 2 2 4-4" />
                        </svg>
                        <span className="truncate">Attended Workshop: Cloud &amp; AI Session</span>
                      </span>
                      <span className="shrink-0 font-mono font-semibold text-[#16a34a]">
                        +100 pts
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* =====================================================================
           SECTION 4: PROTOTYPE FOOTER SECTION WITH GIANT BURST WORDMARK
           ===================================================================== */}
      <LandingFooterSection
        ref={footerSectionRef}
        footerVisible={footerVisible}
        footerLinksRevealed={footerLinksRevealed}
      />
    </div>
  );
}
