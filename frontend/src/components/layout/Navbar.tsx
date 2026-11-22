/**
 * @date 2026-11-22
 * @purpose Handshake Rosetta scroll-folding navigation header with rAF-coalesced width morph, photo-backed 3D fold-in/out mega-panel, and clean route transitions.
 */
import {
  ArrowRight,
  ArrowUpRight,
  ChevronRight,
  Loader2,
  LogOut,
  Menu,
  Shield,
  UserPlus,
  X,
} from "lucide-react";
import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type CSSProperties,
  type ReactNode,
} from "react";
import { Link, NavLink, useLocation, useNavigate } from "react-router-dom";
import { Avatar } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { usePublicPeopleShowcase } from "@/features/catalog/usePublicPeopleShowcase";
import { cn } from "@/lib/cn";
import { NotificationBell } from "./NotificationBell";
import { XpLevelChip } from "./XpLevelChip";

export interface NavbarProps {
  /** Cross-module slot for M4's <XpLevelChip /> widget. */
  xpChipSlot?: ReactNode;
  /** Cross-module slot for M4's <NotificationBell /> widget. */
  notificationSlot?: ReactNode;
}

interface NavSubOption {
  label: string;
  to: string;
}

interface NavOptionGroup {
  id: string;
  label: string;
  to: string;
  highlightTag: string;
  highlightCopy: string;
  imageSrc: string;
  imageAlt: string;
  imagePosition: string;
  subOptions: readonly NavSubOption[];
}

const NAV_OPTIONS: readonly NavOptionGroup[] = [
  {
    id: "team-formation",
    label: "Team Formation",
    to: "/projects",
    highlightTag: "Cross-Faculty Squads",
    highlightCopy: "Match with React, AI, and Engineering builders across campus.",
    imageSrc: "/images/hero/hero-squad-lab.jpg",
    imageAlt: "University students collaborating around laptops in a sunlit campus lab",
    imagePosition: "object-center",
    subOptions: [
      { label: "Create a Project", to: "/projects/new" },
      { label: "Publish Required Skills", to: "/skills" },
      { label: "Search for Teammates", to: "/students" },
      { label: "Build Collaborative Teams", to: "/projects" },
      { label: "Project Applications", to: "/projects" },
      { label: "Project Tasks Board", to: "/projects" },
    ],
  },
  {
    id: "profiles-scoring",
    label: "Profiles & Scoring",
    to: "/students",
    highlightTag: "Verified Campus Identity",
    highlightCopy: "Showcase verified projects, skills, and faculty credentials.",
    imageSrc: "/images/people/omar-mostafa.jpg",
    imageAlt: "Engineering student builder portrait representing verified student profiles",
    imagePosition: "object-top",
    subOptions: [
      { label: "Student Profile & Bio", to: "/profile/me" },
      { label: "Skills & Proficiency", to: "/skills" },
      { label: "Achievements & Contributions", to: "/gamification" },
      { label: "Verified Participation", to: "/events" },
      { label: "Score Transactions Ledger", to: "/gamification" },
    ],
  },
  {
    id: "student-activities",
    label: "Student Activities",
    to: "/organizations",
    highlightTag: "50+ Campus Chapters",
    highlightCopy: "Join IEEE, GDG, Enactus, and robotics labs from semester one.",
    imageSrc: "/images/hero/hero-club-meetup.jpg",
    imageAlt: "Student club members brainstorming hardware and software ideas on campus",
    imagePosition: "object-center",
    subOptions: [
      { label: "Organizations Directory", to: "/organizations" },
      { label: "Showcase Activities", to: "/organizations" },
      { label: "Recruitment Opportunities", to: "/organizations" },
      { label: "Membership Applications", to: "/organizations" },
      { label: "Organization Members", to: "/organizations" },
    ],
  },
  {
    id: "events-workshops",
    label: "Events & Workshops",
    to: "/events",
    highlightTag: "Instant Session Check-In",
    highlightCopy: "Reserve workshop seats and check in via QR or campus pass.",
    imageSrc: "/images/hero/hero-workshop-hall.jpg",
    imageAlt: "Students attending a hands-on technical workshop in a university auditorium",
    imagePosition: "object-center",
    subOptions: [
      { label: "Discover University Events", to: "/events" },
      { label: "Register for Workshops", to: "/events" },
      { label: "Participate in Competitions", to: "/events" },
      { label: "Event Sessions Schedule", to: "/events" },
      { label: "Verify Attendance Check-In", to: "/check-in" },
    ],
  },
  {
    id: "gamification",
    label: "Gamification",
    to: "/gamification",
    highlightTag: "XP, Levels & Badges",
    highlightCopy: "Earn points for teamwork and climb your university leaderboard.",
    imageSrc: "/images/hero/hero-demo-day.jpg",
    imageAlt: "University teammates presenting a finished prototype during Campus Demo Day",
    imagePosition: "object-center",
    subOptions: [
      { label: "Points & Level System", to: "/gamification" },
      { label: "Badges & Achievements", to: "/gamification" },
      { label: "Campus Leaderboards", to: "/leaderboard" },
      { label: "Badge Unlock Criteria", to: "/gamification" },
      { label: "Notifications Inbox", to: "/notifications" },
    ],
  },
];

const MOBILE_NAV_LINKS = [
  { to: "/projects", label: "Projects" },
  { to: "/skills", label: "Skills" },
  { to: "/organizations", label: "Organizations" },
  { to: "/events", label: "Events" },
  { to: "/students", label: "Students" },
  { to: "/leaderboard", label: "Leaderboard" },
  { to: "/gamification", label: "Gamification" },
] as const;

interface GlidingPillRect {
  left: number;
  top: number;
  width: number;
  height: number;
  opacity: number;
}

export function Navbar({ xpChipSlot, notificationSlot }: NavbarProps) {
  const { user, isAuthenticated, isPlatformAdmin, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isSigningOut, setIsSigningOut] = useState(false);
  const [isScrolled, setIsScrolled] = useState(false);
  const [activeMenuId, setActiveMenuId] = useState<string | null>(null);
  const [pinnedMenuId, setPinnedMenuId] = useState<string | null>(null);
  const [displayedMenuId, setDisplayedMenuId] = useState<string | null>(null);
  const [isClosingMenu, setIsClosingMenu] = useState(false);
  const [swapOffsetPx, setSwapOffsetPx] = useState(14);
  const [pillRect, setPillRect] = useState<GlidingPillRect>({
    left: 0,
    top: 0,
    width: 0,
    height: 0,
    opacity: 0,
  });

  const navWrapperRefs = useRef<Record<string, HTMLDivElement | null>>({});
  const closeMenuTimerRef = useRef<number | null>(null);
  const exitAnimTimerRef = useRef<number | null>(null);
  const prevMenuIdxRef = useRef<number | null>(null);
  const openMenuIdRef = useRef<string | null>(null);
  const displayedMenuIdRef = useRef<string | null>(null);

  const openMenuId = pinnedMenuId ?? activeMenuId;
  openMenuIdRef.current = openMenuId;
  displayedMenuIdRef.current = displayedMenuId;

  const { students: showcaseStudents } = usePublicPeopleShowcase(6, Boolean(openMenuId));
  const featuredShowcaseStudent = showcaseStudents[1] ?? showcaseStudents[0] ?? null;

  const displayedOption = useMemo(() => {
    if (!displayedMenuId) {
      return null;
    }
    const baseOption = NAV_OPTIONS.find((item) => item.id === displayedMenuId) ?? null;
    if (!baseOption || baseOption.id !== "profiles-scoring" || !featuredShowcaseStudent) {
      return baseOption;
    }
    return {
      ...baseOption,
      imageSrc: featuredShowcaseStudent.photoUrl,
      imageAlt: `${featuredShowcaseStudent.firstName} ${featuredShowcaseStudent.lastName} — ${featuredShowcaseStudent.universityName} verified student portrait`,
    };
  }, [displayedMenuId, featuredShowcaseStudent]);

  const clearCloseTimer = useCallback(() => {
    if (closeMenuTimerRef.current !== null) {
      window.clearTimeout(closeMenuTimerRef.current);
      closeMenuTimerRef.current = null;
    }
  }, []);

  const clearExitTimer = useCallback(() => {
    if (exitAnimTimerRef.current !== null) {
      window.clearTimeout(exitAnimTimerRef.current);
      exitAnimTimerRef.current = null;
    }
  }, []);

  const forceCloseImmediately = useCallback(() => {
    clearCloseTimer();
    clearExitTimer();
    setPinnedMenuId(null);
    setActiveMenuId(null);
    setDisplayedMenuId(null);
    setIsClosingMenu(false);
    prevMenuIdxRef.current = null;
  }, [clearCloseTimer, clearExitTimer]);

  const syncGlidingPill = useCallback((targetId: string | null) => {
    if (!targetId) {
      setPillRect((prev) => (prev.opacity === 0 ? prev : { ...prev, opacity: 0 }));
      return;
    }
    const wrapper = navWrapperRefs.current[targetId];
    if (!wrapper) {
      setPillRect((prev) => (prev.opacity === 0 ? prev : { ...prev, opacity: 0 }));
      return;
    }
    setPillRect({
      left: wrapper.offsetLeft,
      top: wrapper.offsetTop,
      width: wrapper.offsetWidth,
      height: wrapper.offsetHeight,
      opacity: 1,
    });
  }, []);

  const activateMenu = useCallback(
    (nextId: string, pinned: boolean) => {
      clearExitTimer();
      const nextIdx = NAV_OPTIONS.findIndex((item) => item.id === nextId);
      if (prevMenuIdxRef.current !== null && nextIdx !== -1 && prevMenuIdxRef.current !== nextIdx) {
        setSwapOffsetPx(nextIdx > prevMenuIdxRef.current ? 14 : -14);
      }
      if (nextIdx !== -1) {
        prevMenuIdxRef.current = nextIdx;
      }
      if (pinned) {
        setPinnedMenuId(nextId);
      }
      setActiveMenuId(nextId);
    },
    [clearExitTimer],
  );

  // Coordinate Handshake 3D perspective fold-in & fold-out lifecycle
  useEffect(() => {
    if (openMenuId) {
      clearExitTimer();
      setIsClosingMenu(false);
      setDisplayedMenuId(openMenuId);
      return;
    }

    if (displayedMenuIdRef.current) {
      setIsClosingMenu(true);
      clearExitTimer();
      exitAnimTimerRef.current = window.setTimeout(() => {
        setDisplayedMenuId(null);
        setIsClosingMenu(false);
        prevMenuIdxRef.current = null;
        exitAnimTimerRef.current = null;
      }, 170);
    }
  }, [openMenuId, clearExitTimer]);

  useEffect(() => {
    const rafId = window.requestAnimationFrame(() => {
      syncGlidingPill(openMenuId);
    });
    const midTimer = window.setTimeout(() => {
      syncGlidingPill(openMenuIdRef.current);
    }, 180);
    return () => {
      window.cancelAnimationFrame(rafId);
      window.clearTimeout(midTimer);
    };
  }, [openMenuId, isScrolled, syncGlidingPill]);

  useEffect(() => {
    setIsMobileMenuOpen(false);
    forceCloseImmediately();
  }, [location.pathname, location.search, forceCloseImmediately]);

  useEffect(() => {
    let scrollRafId: number | null = null;
    const applyScrollState = () => {
      scrollRafId = null;
      const y = window.scrollY;
      setIsScrolled((prev) => {
        if (y > 72) return true;
        if (y < 32) return false;
        return prev;
      });
    };

    const handleScroll = () => {
      if (scrollRafId === null) {
        scrollRafId = window.requestAnimationFrame(applyScrollState);
      }
    };

    const handleResize = () => {
      syncGlidingPill(openMenuIdRef.current);
    };

    const handleDocumentClick = () => {
      clearCloseTimer();
      setPinnedMenuId(null);
      setActiveMenuId(null);
    };

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsMobileMenuOpen(false);
        clearCloseTimer();
        setPinnedMenuId(null);
        setActiveMenuId(null);
      }
    };

    applyScrollState();
    window.addEventListener("scroll", handleScroll, { passive: true });
    window.addEventListener("resize", handleResize);
    document.addEventListener("click", handleDocumentClick);
    window.addEventListener("keydown", handleKeyDown);

    return () => {
      if (scrollRafId !== null) {
        window.cancelAnimationFrame(scrollRafId);
      }
      window.removeEventListener("scroll", handleScroll);
      window.removeEventListener("resize", handleResize);
      document.removeEventListener("click", handleDocumentClick);
      window.removeEventListener("keydown", handleKeyDown);
      clearCloseTimer();
      clearExitTimer();
    };
  }, [clearCloseTimer, clearExitTimer, syncGlidingPill]);

  const handleLogout = async () => {
    setIsSigningOut(true);
    try {
      await logout();
      setIsMobileMenuOpen(false);
      void navigate("/auth/login");
    } finally {
      setIsSigningOut(false);
    }
  };

  return (
    <header
      id="main-header"
      className="pointer-events-none fixed inset-x-0 top-3 z-50 px-3 sm:top-6 sm:px-5 xl:px-6"
    >
      <div
        id="header-inner"
        className={cn(
          "pointer-events-auto mx-auto flex h-[56px] w-full items-center justify-between gap-2 rounded-[16px] border px-2.5 py-2 transition-[max-width,background-color,box-shadow,border-color] duration-[380ms] ease-[cubic-bezier(0.22,1,0.36,1)] sm:px-3 xl:px-5",
          isScrolled
            ? "max-w-[1040px] border-black/[0.06] bg-white/[0.985] shadow-[0_18px_46px_-8px_rgba(8,18,20,0.42)]"
            : "max-w-[1392px] border-transparent bg-white/0 shadow-none",
        )}
      >
        {/* Left Zone: Smooth Folding Brand Wordmark <-> Lime Square Icon */}
        <div className="flex shrink-0 items-center gap-2.5">
          <button
            type="button"
            onClick={(event) => {
              event.stopPropagation();
              setIsMobileMenuOpen((prev) => !prev);
            }}
            aria-label={isMobileMenuOpen ? "Close navigation menu" : "Toggle navigation menu"}
            aria-expanded={isMobileMenuOpen}
            aria-controls="mobile-navigation-drawer"
            className={cn(
              "inline-flex size-9 cursor-pointer items-center justify-center rounded-xl border transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] lg:hidden",
              isScrolled
                ? "border-black/15 bg-black/5 text-[#14151c] hover:bg-black/10"
                : "border-white/15 bg-[#1b1d26]/90 text-white/85 hover:border-[#d3fb52]/40 hover:text-white",
            )}
          >
            {isMobileMenuOpen ? (
              <X className="size-4" aria-hidden="true" />
            ) : (
              <Menu className="size-4" aria-hidden="true" />
            )}
          </button>

          <Link
            to="/"
            title="TeamUp"
            aria-label="TeamUp Home"
            className={cn(
              "inline-flex h-10 shrink-0 items-center overflow-hidden transition-[width] duration-[360ms] ease-[cubic-bezier(0.22,1,0.36,1)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
              isScrolled ? "w-10" : "w-[96px] sm:w-[104px] xl:w-[132px]",
            )}
          >
            {isScrolled ? (
              <span
                id="brand-icon-scrolled"
                style={{ fontFamily: "'Barlow Condensed', sans-serif" }}
                className="animate-brand-morph flex h-10 w-10 shrink-0 cursor-pointer items-center justify-center rounded-[11px] bg-[#d3fb52] text-[24px] font-black italic leading-none tracking-tighter text-[#14151c] transition-transform duration-200 hover:scale-105"
              >
                T
              </span>
            ) : (
              <span
                id="brand-wordmark"
                style={{ fontFamily: "'Barlow Condensed', sans-serif" }}
                className="animate-brand-morph inline-block shrink-0 cursor-pointer whitespace-nowrap text-[26px] font-black italic leading-none tracking-[-0.03em] text-[#d3fb52] transition-transform duration-200 hover:scale-[1.03] sm:text-[28px] xl:text-[36px]"
              >
                TeamUp
              </span>
            )}
          </Link>
        </div>

        {/* Center Zone: 5 Module Triggers + Gliding Pill + Shared Handshake Photo Mega-Panel */}
        <nav
          id="primary-nav"
          aria-label="Primary navigation"
          onClick={(event) => event.stopPropagation()}
          onMouseLeave={() => {
            if (pinnedMenuId) return;
            clearCloseTimer();
            closeMenuTimerRef.current = window.setTimeout(() => {
              setActiveMenuId(null);
            }, 180);
          }}
          className="relative hidden items-center justify-center gap-0 lg:flex xl:gap-1"
        >
          <div
            id="nav-gliding-pill"
            aria-hidden="true"
            style={{
              transform: `translate3d(${pillRect.left}px, ${pillRect.top}px, 0)`,
              width: `${pillRect.width}px`,
              height: `${pillRect.height}px`,
              opacity: pillRect.opacity,
            }}
            className={cn(
              "nav-gliding-pill pointer-events-none absolute left-0 top-0 rounded-[10px]",
              isScrolled
                ? "bg-[#d3fb52] shadow-[0_4px_14px_rgba(211,251,82,0.4)]"
                : "bg-white/[0.14] backdrop-blur-sm",
            )}
          />

          {NAV_OPTIONS.map((option) => {
            const isOpen = openMenuId === option.id;

            return (
              <div
                key={option.id}
                ref={(el) => {
                  navWrapperRefs.current[option.id] = el;
                }}
                onMouseEnter={() => {
                  clearCloseTimer();
                  activateMenu(option.id, Boolean(pinnedMenuId));
                }}
                className="animate-nav-item relative z-10"
              >
                <button
                  type="button"
                  aria-expanded={isOpen}
                  aria-haspopup="menu"
                  onClick={(event) => {
                    event.stopPropagation();
                    clearCloseTimer();
                    if (pinnedMenuId === option.id) {
                      setPinnedMenuId(null);
                      setActiveMenuId(null);
                    } else {
                      activateMenu(option.id, true);
                    }
                  }}
                  className={cn(
                    "nav-option-btn cursor-pointer whitespace-nowrap rounded-[10px] px-2 py-2 text-[12px] font-medium tracking-[-0.01em] transition-colors duration-150 xl:px-3 xl:text-[13.5px]",
                    isScrolled
                      ? isOpen
                        ? "font-semibold text-[#14151c]"
                        : "text-[#14151c]/85 hover:text-[#14151c]"
                      : isOpen
                        ? "font-semibold text-white"
                        : "text-white/90 hover:text-white",
                  )}
                >
                  {option.label}
                </button>
              </div>
            );
          })}

          {isPlatformAdmin && (
            <NavLink
              to="/admin"
              className={({ isActive }) =>
                cn(
                  "relative z-10 inline-flex shrink-0 items-center gap-1 whitespace-nowrap rounded-[10px] px-2 py-1.5 text-[12px] font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] xl:px-2.5 xl:text-[13.5px]",
                  isActive
                    ? "bg-[#d3fb52] font-semibold text-[#14151c]"
                    : isScrolled
                      ? "text-[#14151c] hover:bg-black/5"
                      : "text-[#d3fb52]/90 hover:bg-white/10 hover:text-[#d3fb52]",
                )
              }
            >
              <Shield className="size-3.5 shrink-0" aria-hidden="true" />
              Admin
            </NavLink>
          )}

          {/* Shared Handshake 3D Perspective Fold-In / Fold-Out Photo Mega-Panel */}
          {displayedOption && (
            <div
              onMouseEnter={clearCloseTimer}
              onMouseLeave={() => {
                if (pinnedMenuId) return;
                clearCloseTimer();
                closeMenuTimerRef.current = window.setTimeout(() => {
                  setActiveMenuId(null);
                }, 180);
              }}
              aria-hidden={isClosingMenu || undefined}
              className={cn(
                "nav-fold-viewport fixed left-1/2 top-[74px] z-[100] w-[min(calc(100vw-2rem),1024px)] -translate-x-1/2 sm:top-[86px]",
                isClosingMenu && "pointer-events-none",
              )}
            >
              {/* Invisible cursor hover bridge */}
              <div aria-hidden="true" className="absolute -top-4 inset-x-0 h-4" />

              <div
                role="menu"
                aria-label={`${displayedOption.label} submenu`}
                className={cn(
                  "max-h-[calc(100dvh-6rem)] overflow-y-auto rounded-[18px] border border-black/[0.07] bg-white p-2.5 shadow-[0_26px_64px_-12px_rgba(8,18,20,0.44)]",
                  isClosingMenu ? "animate-nav-fold-out" : "animate-nav-fold-in",
                )}
              >
                <div
                  key={displayedOption.id}
                  style={{ "--nav-swap-offset": `${swapOffsetPx}px` } as CSSProperties}
                  className="animate-nav-content-swap grid grid-cols-12 gap-3"
                >
                  {/* Left Editorial Photography Spotlight Card */}
                  <div className="col-span-5 flex flex-col justify-between rounded-xl bg-[#f3f6f4] p-2.5 lg:col-span-4">
                    <div
                      onClick={(event) => {
                        event.stopPropagation();
                        forceCloseImmediately();
                        void navigate(displayedOption.to);
                      }}
                      className="group/feature relative min-h-[168px] cursor-pointer overflow-hidden rounded-xl bg-[#14151c]"
                    >
                      <img
                        src={displayedOption.imageSrc}
                        alt={displayedOption.imageAlt}
                        decoding="async"
                        className={cn(
                          "absolute inset-0 h-full w-full object-cover transition-transform duration-500 ease-[cubic-bezier(0.22,1,0.36,1)] group-hover/feature:scale-[1.04]",
                          displayedOption.imagePosition,
                        )}
                      />
                      <div
                        aria-hidden="true"
                        className="absolute inset-0 bg-gradient-to-t from-[#14151c]/95 via-[#14151c]/45 to-black/10"
                      />
                      <div className="relative flex h-full min-h-[168px] flex-col justify-between p-4">
                        <span className="w-fit rounded-full border border-[#d3fb52]/35 bg-[#14151c]/80 px-2.5 py-0.5 text-[10px] font-bold uppercase tracking-wider text-[#d3fb52] backdrop-blur-sm">
                          {displayedOption.highlightTag}
                        </span>
                        <p
                          className="text-xl font-bold leading-tight text-white drop-shadow-sm"
                          style={{ fontFamily: "var(--font-heading)" }}
                        >
                          {displayedOption.label}
                        </p>
                      </div>
                    </div>

                    <div className="mt-3 space-y-2.5 px-1.5 pb-1">
                      <p className="text-xs leading-relaxed text-[#4b5563]">
                        {displayedOption.highlightCopy}
                      </p>
                      <button
                        type="button"
                        onClick={(event) => {
                          event.stopPropagation();
                          forceCloseImmediately();
                          void navigate(displayedOption.to);
                        }}
                        className="group/cta inline-flex w-full cursor-pointer items-center justify-between rounded-xl px-1 py-1 text-left text-xs font-bold text-[#14151c] transition-colors hover:text-[#15733f]"
                      >
                        <span>Open {displayedOption.label}</span>
                        <ArrowRight
                          className="size-4 transition-transform duration-200 group-hover/cta:translate-x-1"
                          aria-hidden="true"
                        />
                      </button>
                    </div>
                  </div>

                  {/* Right Sub-Options Grid */}
                  <div className="col-span-7 flex flex-col justify-between px-2 py-2 lg:col-span-8 lg:px-4">
                    <div>
                      <p className="mb-2.5 px-3 text-[11px] font-bold uppercase tracking-wider text-[#6b7280]">
                        Navigate {displayedOption.label}
                      </p>
                      <ul className="grid grid-cols-1 gap-1 lg:grid-cols-2">
                        {displayedOption.subOptions.map((subItem) => (
                          <li key={`${displayedOption.id}:${subItem.label}`}>
                            <button
                              type="button"
                              role="menuitem"
                              onClick={(event) => {
                                event.stopPropagation();
                                forceCloseImmediately();
                                if (location.pathname === subItem.to) {
                                  window.scrollTo({ top: 0, behavior: "smooth" });
                                } else {
                                  void navigate(subItem.to);
                                }
                              }}
                              className="group/sub flex w-full cursor-pointer items-center justify-between rounded-xl px-3.5 py-2.5 text-left text-[14.5px] font-medium tracking-[-0.01em] text-[#14151c] transition-colors duration-150 hover:bg-[#eef5eb]"
                            >
                              <span>{subItem.label}</span>
                              <ArrowUpRight
                                className="size-4 shrink-0 text-[#15733f] opacity-0 transition-all duration-150 group-hover/sub:translate-x-0.5 group-hover/sub:opacity-100"
                                aria-hidden="true"
                              />
                            </button>
                          </li>
                        ))}
                      </ul>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
        </nav>

        {/* Right Zone: Auth / User Controls */}
        <div className="flex shrink-0 items-center gap-2">
          {isAuthenticated && user ? (
            <div className="flex shrink-0 items-center gap-1.5">
              <div className="shrink-0 whitespace-nowrap">{xpChipSlot ?? <XpLevelChip />}</div>

              {notificationSlot ?? <NotificationBell />}

              <Link
                to="/profile/me"
                title={`${user.firstName} ${user.lastName} · ${user.role}`}
                className={cn(
                  "inline-flex h-9 shrink-0 items-center gap-1.5 whitespace-nowrap rounded-xl border px-1.5 text-xs font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:px-2.5 lg:px-1.5 xl:px-2.5",
                  isScrolled
                    ? "border-black/15 bg-black/5 text-[#14151c] hover:border-black/30"
                    : "border-white/15 bg-[#1b1d26]/90 text-white hover:border-[#d3fb52]/40",
                )}
              >
                <Avatar
                  src={user.photoUrl ?? user.profile?.photoUrl}
                  name={`${user.firstName} ${user.lastName}`}
                  size="xs"
                />
                <span className="hidden sm:inline lg:hidden xl:inline">{user.firstName}</span>
              </Link>

              <button
                type="button"
                disabled={isSigningOut}
                onClick={() => {
                  void handleLogout();
                }}
                aria-label="Sign out"
                className={cn(
                  "inline-flex size-9 shrink-0 cursor-pointer items-center justify-center rounded-xl border transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] disabled:opacity-50",
                  isScrolled
                    ? "border-black/15 bg-black/5 text-[#14151c] hover:border-red-500/40 hover:bg-red-500/10 hover:text-red-600"
                    : "border-white/15 bg-white/10 text-white/80 hover:border-red-500/40 hover:bg-red-500/10 hover:text-red-300",
                )}
              >
                {isSigningOut ? (
                  <Loader2 className="size-4 animate-spin" aria-hidden="true" />
                ) : (
                  <LogOut className="size-4" aria-hidden="true" />
                )}
              </button>
            </div>
          ) : (
            <div className="flex shrink-0 items-center gap-1.5 sm:gap-2">
              <Link
                to="/auth/register"
                className={cn(
                  "hidden h-9 shrink-0 items-center gap-1.5 whitespace-nowrap rounded-[10px] border px-2.5 text-xs font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:inline-flex sm:h-10 xl:px-3.5 xl:text-sm",
                  isScrolled
                    ? "border-[#14151c]/75 bg-white text-[#14151c] hover:bg-black/[0.04]"
                    : "border-white/80 bg-transparent text-white hover:bg-white/10",
                )}
              >
                <UserPlus className="size-3.5 shrink-0" aria-hidden="true" />
                Join
              </Link>
              <Link
                to="/auth/login"
                className={cn(
                  "inline-flex h-9 shrink-0 items-center whitespace-nowrap rounded-[10px] px-3 text-xs font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:h-10 sm:px-3.5 sm:text-sm xl:px-4",
                  isScrolled
                    ? "bg-[#14151c] text-white hover:bg-[#232634]"
                    : "bg-[#d3fb52] text-[#14151c] hover:bg-[#deff6e]",
                )}
              >
                Sign In
              </Link>
            </div>
          )}
        </div>
      </div>

      {/* Mobile Navigation Drawer */}
      {isMobileMenuOpen && (
        <div
          id="mobile-navigation-drawer"
          onClick={(event) => event.stopPropagation()}
          className="pointer-events-auto mt-2 max-h-[calc(100dvh-5.5rem)] overflow-y-auto rounded-2xl border border-white/10 bg-[#14151c]/95 px-4 py-4 shadow-2xl backdrop-blur-md lg:hidden"
        >
          <nav aria-label="Mobile navigation" className="flex flex-col gap-1">
            {MOBILE_NAV_LINKS.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `flex items-center justify-between rounded-xl px-3 py-2 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] ${
                    isActive
                      ? "bg-[#d3fb52] font-semibold text-[#14151c]"
                      : "text-white/80 hover:bg-white/5 hover:text-white"
                  }`
                }
              >
                <span>{item.label}</span>
                <ChevronRight className="size-4 opacity-50" aria-hidden="true" />
              </NavLink>
            ))}

            {isPlatformAdmin && (
              <NavLink
                to="/admin"
                className={({ isActive }) =>
                  `inline-flex items-center gap-2 rounded-xl px-3 py-2 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] ${
                    isActive
                      ? "bg-[#d3fb52] font-semibold text-[#14151c]"
                      : "text-[#d3fb52] hover:bg-white/5"
                  }`
                }
              >
                <Shield className="size-4" aria-hidden="true" />
                Admin Console
              </NavLink>
            )}
          </nav>

          <div className="mt-4 flex flex-wrap items-center justify-end gap-2 border-t border-white/10 pt-3">
            {isAuthenticated && user ? (
              <div className="flex items-center gap-2">
                <Link
                  to="/profile/me"
                  className="text-xs font-medium text-white hover:text-[#d3fb52]"
                >
                  {user.firstName} {user.lastName}
                </Link>
                <button
                  type="button"
                  onClick={() => {
                    void handleLogout();
                  }}
                  className="rounded-lg border border-red-500/30 bg-red-500/10 px-2.5 py-1 text-xs font-medium text-red-200 hover:bg-red-500/20"
                >
                  Sign Out
                </button>
              </div>
            ) : (
              <Link
                to="/auth/register"
                className="rounded-lg bg-[#d3fb52] px-3 py-1 text-xs font-semibold text-[#14151c]"
              >
                Create Account
              </Link>
            )}
          </div>
        </div>
      )}
    </header>
  );
}
