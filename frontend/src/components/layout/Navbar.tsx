/**
 * @date 2026-11-22
 * @purpose Handshake Rosetta scroll-folding navigation header with rAF-coalesced width morph, photo-backed 3D fold-in/out mega-panel, and clean route transitions.
 */
import {
  ArrowRight,
  ArrowUpRight,
  ChevronDown,
  LayoutDashboard,
  Loader2,
  LogOut,
  Menu,
  Search,
  Shield,
  Sparkles,
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
import { dispatchOpenCommandPalette } from "./CommandPalette";
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
      { label: "Discover Feed", to: "/explore" },
      { label: "Explore Projects", to: "/projects" },
      { label: "Create a Project", to: "/projects/new" },
      { label: "Publish Required Skills", to: "/skills" },
      { label: "Search for Teammates", to: "/students" },
      { label: "Project Tasks Board", to: "/projects" },
      { label: "My Role Dashboard", to: "/dashboard" },
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
      { label: "Student Directory", to: "/students" },
      { label: "Student Profile & Bio", to: "/profile/me" },
      { label: "Skills & Proficiency", to: "/skills" },
      { label: "Score Transactions Ledger", to: "/gamification" },
      { label: "Campus Leaderboards", to: "/leaderboard" },
      { label: "Privacy & Policy", to: "/privacy" },
    ],
  },
  {
    id: "student-activities",
    label: "Student Activities",
    to: "/organizations",
    highlightTag: "50+ Campus Chapters",
    highlightCopy: "Join IEEE, GDG, Enactus, and open collectives across Egypt.",
    imageSrc: "/images/hero/hero-club-meetup.jpg",
    imageAlt: "Student club members brainstorming hardware and software ideas on campus",
    imagePosition: "object-center",
    subOptions: [
      { label: "Organizations Directory", to: "/organizations" },
      { label: "University Branches (IEEE, Enactus)", to: "/organizations?scope=UniversityBranch" },
      { label: "Open & Independent Communities", to: "/organizations?scope=Independent" },
      { label: "Cross-University Initiatives", to: "/organizations?scope=CrossUniversity" },
      { label: "Recruiting Open Now", to: "/organizations?scope=recruiting" },
      { label: "Request New Organization", to: "/organizations?request=1" },
      { label: "Org Leader Dashboard", to: "/dashboard/org" },
    ],
  },
  {
    id: "events-workshops",
    label: "Events & Workshops",
    to: "/events",
    highlightTag: "Instant Session Check-In",
    highlightCopy: "Reserve workshop seats and check in via QR or camera scanner.",
    imageSrc: "/images/hero/hero-workshop-hall.jpg",
    imageAlt: "Students attending a hands-on technical workshop in a university auditorium",
    imagePosition: "object-center",
    subOptions: [
      { label: "Discover University Events", to: "/events" },
      { label: "Register for Workshops", to: "/events?type=Workshop" },
      { label: "Participate in Competitions", to: "/events?type=Competition" },
      { label: "Hybrid & Online Sessions", to: "/events?deliveryMode=Hybrid" },
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
      { label: "Notifications Inbox", to: "/notifications" },
      { label: "My Activity Dashboard", to: "/dashboard/student" },
    ],
  },
];

interface MobileNavModuleGroup {
  id: string;
  label: string;
  highlightTag: string;
  items: readonly NavSubOption[];
}

const MOBILE_NAV_MODULES: readonly MobileNavModuleGroup[] = [
  {
    id: "team-formation",
    label: "Team Formation",
    highlightTag: "Cross-Faculty Squads",
    items: [
      { label: "Discover Feed", to: "/explore" },
      { label: "Explore Projects", to: "/projects" },
      { label: "Create a Project", to: "/projects/new" },
      { label: "Skills Catalog", to: "/skills" },
      { label: "Search Teammates", to: "/students" },
      { label: "My Role Dashboard", to: "/dashboard" },
    ],
  },
  {
    id: "profiles-scoring",
    label: "Profiles & Scoring",
    highlightTag: "Verified Identity",
    items: [
      { label: "Student Directory", to: "/students" },
      { label: "Profile Studio", to: "/profile/me" },
      { label: "Skills & Proficiency", to: "/skills" },
      { label: "XP & Score Ledger", to: "/gamification" },
      { label: "Privacy & Policy", to: "/privacy" },
    ],
  },
  {
    id: "student-activities",
    label: "Student Activities",
    highlightTag: "50+ Campus Chapters",
    items: [
      { label: "Organizations Directory", to: "/organizations" },
      { label: "Open & Independent Communities", to: "/organizations?scope=Independent" },
      { label: "Recruiting Now", to: "/organizations?scope=recruiting" },
      { label: "Request New Organization", to: "/organizations?request=1" },
      { label: "Org Leader Dashboard", to: "/dashboard/org" },
    ],
  },
  {
    id: "events-workshops",
    label: "Events & Workshops",
    highlightTag: "Session Check-In",
    items: [
      { label: "Discover Events", to: "/events" },
      { label: "Workshops", to: "/events?type=Workshop" },
      { label: "Verify Check-In", to: "/check-in" },
    ],
  },
  {
    id: "gamification",
    label: "Gamification",
    highlightTag: "XP, Levels & Badges",
    items: [
      { label: "Points & Badges", to: "/gamification" },
      { label: "Campus Leaderboard", to: "/leaderboard" },
      { label: "Notifications Inbox", to: "/notifications" },
    ],
  },
];

function matchesMobilePath(pathname: string, target: string): boolean {
  return pathname === target || pathname.startsWith(`${target}/`);
}

interface GlidingPillRect {
  left: number;
  top: number;
  width: number;
  height: number;
  opacity: number;
}

export function Navbar({ xpChipSlot, notificationSlot }: NavbarProps) {
  const { user, isAuthenticated, isPlatformAdmin, isOrgAdmin, orgAdminIds, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [expandedMobileGroupId, setExpandedMobileGroupId] = useState<string | null>(null);
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
  const menuPinnedByClickRef = useRef<string | null>(null);

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
    menuPinnedByClickRef.current = null;
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
    setExpandedMobileGroupId(null);
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
      if (window.innerWidth >= 1024) {
        setIsMobileMenuOpen(false);
        setExpandedMobileGroupId(null);
      } else {
        forceCloseImmediately();
      }
      syncGlidingPill(openMenuIdRef.current);
    };

    const handleDocumentClick = () => {
      clearCloseTimer();
      menuPinnedByClickRef.current = null;
      setPinnedMenuId(null);
      setActiveMenuId(null);
      setIsMobileMenuOpen(false);
      setExpandedMobileGroupId(null);
    };

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsMobileMenuOpen(false);
        setExpandedMobileGroupId(null);
        clearCloseTimer();
        menuPinnedByClickRef.current = null;
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
  }, [clearCloseTimer, clearExitTimer, forceCloseImmediately, syncGlidingPill]);

  const handleLogout = async () => {
    setIsSigningOut(true);
    try {
      await logout();
      setIsMobileMenuOpen(false);
      setExpandedMobileGroupId(null);
      void navigate("/auth/login");
    } finally {
      setIsSigningOut(false);
    }
  };

  return (
    <header
      id="main-header"
      className="pointer-events-none fixed inset-x-0 top-[max(0.75rem,env(safe-area-inset-top))] z-50 px-[max(0.625rem,env(safe-area-inset-left))] sm:top-[max(1.25rem,env(safe-area-inset-top))] sm:px-4 xl:px-6"
    >
      <div
        id="header-inner"
        className={cn(
          "pointer-events-auto mx-auto flex min-h-[56px] w-full items-center justify-between gap-1.5 rounded-[16px] border px-2 py-1.5 transition-[max-width,background-color,box-shadow,border-color] duration-[380ms] ease-[cubic-bezier(0.22,1,0.36,1)] sm:gap-2 sm:px-3 sm:py-2 xl:px-5",
          isScrolled
            ? cn(
                isAuthenticated ? "max-w-[1240px]" : "max-w-[1040px]",
                "border-black/[0.06] bg-white/[0.985] shadow-[0_18px_46px_-8px_rgba(8,18,20,0.42)]",
              )
            : "max-w-[1392px] border-transparent bg-white/0 shadow-none",
        )}
      >
        {/* Left Zone: Smooth Folding Brand Wordmark <-> Lime Square Icon */}
        <div className="flex shrink-0 items-center gap-1.5 sm:gap-2.5">
          <button
            type="button"
            onClick={(event) => {
              event.stopPropagation();
              setIsMobileMenuOpen((prev) => {
                const next = !prev;
                if (next) {
                  const matchedGroup = MOBILE_NAV_MODULES.find((group) =>
                    group.items.some((item) => matchesMobilePath(location.pathname, item.to)),
                  );
                  setExpandedMobileGroupId(matchedGroup?.id ?? null);
                } else {
                  setExpandedMobileGroupId(null);
                }
                return next;
              });
            }}
            aria-label={isMobileMenuOpen ? "Close navigation menu" : "Toggle navigation menu"}
            aria-expanded={isMobileMenuOpen}
            aria-controls="mobile-navigation-drawer"
            className={cn(
              "inline-flex min-h-[36px] min-w-[36px] shrink-0 cursor-pointer items-center justify-center rounded-xl border transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:min-h-[40px] sm:min-w-[40px] lg:hidden",
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
              "inline-flex min-h-[40px] shrink-0 items-center overflow-hidden transition-[width] duration-[360ms] ease-[cubic-bezier(0.22,1,0.36,1)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
              isScrolled ? "w-9 sm:w-10" : "w-[74px] sm:w-[92px] lg:w-[86px] xl:w-[126px]",
            )}
          >
            {isScrolled ? (
              <span
                id="brand-icon-scrolled"
                style={{ fontFamily: "'Barlow Condensed', sans-serif" }}
                className="animate-brand-morph flex h-9 w-9 shrink-0 cursor-pointer items-center justify-center rounded-[11px] bg-[#d3fb52] text-[22px] font-black italic leading-none tracking-tighter text-[#14151c] transition-transform duration-200 hover:scale-105 sm:h-10 sm:w-10 sm:text-[24px]"
              >
                T
              </span>
            ) : (
              <span
                id="brand-wordmark"
                style={{ fontFamily: "'Barlow Condensed', sans-serif" }}
                className="animate-brand-morph inline-block shrink-0 cursor-pointer whitespace-nowrap text-[22px] font-black italic leading-none tracking-[-0.03em] text-[#d3fb52] transition-transform duration-200 hover:scale-[1.03] sm:text-[26px] lg:text-[24px] xl:text-[34px]"
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
          className="relative hidden min-w-0 items-center justify-center gap-0.5 lg:flex xl:gap-1"
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
                  if (menuPinnedByClickRef.current !== option.id) {
                    menuPinnedByClickRef.current = null;
                  }
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
                    if (menuPinnedByClickRef.current === option.id && pinnedMenuId === option.id) {
                      menuPinnedByClickRef.current = null;
                      setPinnedMenuId(null);
                      setActiveMenuId(null);
                    } else {
                      menuPinnedByClickRef.current = option.id;
                      activateMenu(option.id, true);
                    }
                  }}
                  className={cn(
                    "nav-option-btn inline-flex min-h-[38px] cursor-pointer items-center whitespace-nowrap rounded-[10px] px-1.5 py-2 text-[12px] font-medium tracking-[-0.015em] transition-colors duration-150 xl:px-2.5 xl:text-[13.5px]",
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

          {isPlatformAdmin ? (
            <NavLink
              to="/admin"
              className={({ isActive }) =>
                cn(
                  "relative z-10 inline-flex min-h-[38px] shrink-0 items-center gap-1 whitespace-nowrap rounded-[10px] px-1.5 py-1.5 text-[12px] font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] xl:px-2.5 xl:text-[13.5px]",
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
          ) : (
            isAuthenticated && (
              <NavLink
                to={isOrgAdmin || orgAdminIds.length > 0 ? "/dashboard/org" : "/dashboard"}
                className={({ isActive }) =>
                  cn(
                    "relative z-10 inline-flex min-h-[38px] shrink-0 items-center gap-1 whitespace-nowrap rounded-[10px] px-1.5 py-1.5 text-[12px] font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] xl:px-2.5 xl:text-[13.5px]",
                    isActive
                      ? "bg-[#d3fb52] font-semibold text-[#14151c]"
                      : isScrolled
                        ? "text-[#14151c] hover:bg-black/5"
                        : "text-[#d3fb52]/90 hover:bg-white/10 hover:text-[#d3fb52]",
                  )
                }
              >
                <LayoutDashboard className="size-3.5 shrink-0" aria-hidden="true" />
                Dashboard
              </NavLink>
            )
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

        {/* Right Zone: Icon-Only Command Palette, Auth / User Controls */}
        <div className="flex shrink-0 items-center gap-1 sm:gap-1.5">
          <button
            type="button"
            onClick={() => dispatchOpenCommandPalette()}
            aria-label="Quick search command palette"
            title="Quick search (Ctrl+K)"
            className={cn(
              "inline-flex min-h-[36px] min-w-[36px] shrink-0 cursor-pointer items-center justify-center rounded-xl border transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:min-h-[40px] sm:min-w-[40px]",
              isScrolled
                ? "border-black/15 bg-black/5 text-[#14151c] hover:border-black/30 hover:bg-black/10"
                : "border-white/15 bg-[#1b1d26]/90 text-white/85 hover:border-[#d3fb52]/40 hover:text-white",
            )}
          >
            <Search className="size-4 shrink-0" aria-hidden="true" />
          </button>

          {isAuthenticated && user ? (
            <div className="flex shrink-0 items-center gap-1 sm:gap-1.5">
              <div className="shrink-0 whitespace-nowrap">
                {xpChipSlot ?? <XpLevelChip isScrolled={isScrolled} />}
              </div>

              {notificationSlot ?? (
                <NotificationBell
                  isScrolled={isScrolled}
                  onOpenChange={(open) => {
                    if (open) {
                      setIsMobileMenuOpen(false);
                      setExpandedMobileGroupId(null);
                    }
                  }}
                />
              )}

              <Link
                to="/profile/me"
                title={`${user.firstName} ${user.lastName} · ${user.role}`}
                className={cn(
                  "inline-flex min-h-[36px] min-w-[36px] shrink-0 items-center justify-center gap-1.5 whitespace-nowrap rounded-xl border px-1.5 text-xs font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:min-h-[40px] sm:px-2.5 lg:size-10 lg:px-0 xl:w-auto xl:px-2.5",
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
                <span className="hidden max-w-[92px] truncate sm:inline lg:hidden xl:inline">
                  {user.firstName}
                </span>
              </Link>

              <button
                type="button"
                disabled={isSigningOut}
                onClick={() => {
                  void handleLogout();
                }}
                aria-label="Sign out"
                className={cn(
                  "inline-flex min-h-[36px] min-w-[36px] shrink-0 cursor-pointer items-center justify-center rounded-xl border transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] disabled:opacity-50 sm:min-h-[40px] sm:min-w-[40px]",
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
            <div className="flex shrink-0 items-center gap-1 sm:gap-1.5">
              <Link
                to="/auth/register"
                className={cn(
                  "hidden min-h-[38px] shrink-0 items-center gap-1.5 whitespace-nowrap rounded-[10px] border px-2.5 text-xs font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:inline-flex sm:min-h-[40px] xl:px-3.5 xl:text-sm",
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
                  "inline-flex min-h-[36px] shrink-0 items-center whitespace-nowrap rounded-[10px] px-2.5 text-xs font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:min-h-[40px] sm:px-3.5 sm:text-sm xl:px-4",
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

      {/* Mobile Navigation Drawer & Click-to-Dismiss Backdrop */}
      {isMobileMenuOpen && (
        <>
          <div
            aria-hidden="true"
            onClick={() => {
              setIsMobileMenuOpen(false);
              setExpandedMobileGroupId(null);
            }}
            className="pointer-events-auto fixed inset-0 -z-10 bg-black/55 backdrop-blur-[2px] lg:hidden"
          />

          <div
            id="mobile-navigation-drawer"
            onClick={(event) => event.stopPropagation()}
            className={cn(
              "animate-mobile-drawer-in pointer-events-auto mx-auto mt-2 w-full max-h-[calc(100dvh-5.5rem)] overflow-y-auto overscroll-contain rounded-2xl border border-white/12 bg-[#14151c]/[0.98] p-3 pb-[max(0.75rem,env(safe-area-inset-bottom))] shadow-[0_24px_64px_-12px_rgba(0,0,0,0.75)] backdrop-blur-xl sm:p-4 lg:hidden",
              isScrolled
                ? isAuthenticated
                  ? "max-w-[1240px]"
                  : "max-w-[1040px]"
                : "max-w-[1392px]",
            )}
          >
            <button
              type="button"
              onClick={() => {
                setIsMobileMenuOpen(false);
                dispatchOpenCommandPalette();
              }}
              className="mb-3 flex min-h-[42px] w-full cursor-pointer items-center justify-between gap-2 rounded-xl border border-white/12 bg-[#1b1d26] px-3.5 py-2 text-left text-xs font-medium text-white/80 transition-colors hover:border-[#d3fb52]/40 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              <span className="inline-flex items-center gap-2">
                <Search className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
                <span>Quick Search…</span>
              </span>
              <span className="text-[11px] text-white/45">Pages · Clubs · Events</span>
            </button>
            {isAuthenticated && user && (
              <div className="mb-3 flex items-center justify-between gap-2 rounded-xl border border-white/10 bg-[#1b1d26] p-2.5 sm:hidden">
                <Link
                  to="/profile/me"
                  onClick={() => setIsMobileMenuOpen(false)}
                  className="flex min-w-0 items-center gap-2.5"
                >
                  <Avatar
                    src={user.photoUrl ?? user.profile?.photoUrl}
                    name={`${user.firstName} ${user.lastName}`}
                    size="xs"
                  />
                  <div className="min-w-0">
                    <p className="truncate text-xs font-semibold text-white">
                      {user.firstName} {user.lastName}
                    </p>
                    <p className="truncate text-[10px] text-white/60">Profile Studio</p>
                  </div>
                </Link>
                <Link
                  to="/gamification"
                  onClick={() => setIsMobileMenuOpen(false)}
                  className="inline-flex shrink-0 items-center gap-1 rounded-full border border-[#d3fb52]/35 bg-[#d3fb52]/10 px-2.5 py-1 text-[11px] font-semibold text-[#d3fb52]"
                >
                  <Sparkles className="size-3" aria-hidden="true" />
                  <span>XP &amp; Badges</span>
                </Link>
              </div>
            )}

            <nav aria-label="Mobile navigation" className="flex flex-col gap-2">
              {MOBILE_NAV_MODULES.map((group) => {
                const isExpanded = expandedMobileGroupId === group.id;
                const hasActiveRoute = group.items.some((item) =>
                  matchesMobilePath(location.pathname, item.to),
                );
                const panelId = `mobile-nav-panel-${group.id}`;

                return (
                  <div
                    key={group.id}
                    className={cn(
                      "overflow-hidden rounded-xl border transition-colors",
                      isExpanded || hasActiveRoute
                        ? "border-[#d3fb52]/30 bg-white/[0.04]"
                        : "border-white/[0.08] bg-white/[0.02]",
                    )}
                  >
                    <button
                      type="button"
                      aria-expanded={isExpanded}
                      aria-controls={panelId}
                      onClick={() => {
                        setExpandedMobileGroupId((prev) => (prev === group.id ? null : group.id));
                      }}
                      className="flex min-h-[46px] w-full cursor-pointer items-center justify-between gap-2 px-3.5 py-2.5 text-left transition-colors hover:bg-white/[0.04] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-[#d3fb52]"
                    >
                      <div className="flex min-w-0 items-center gap-2">
                        {hasActiveRoute && (
                          <span
                            aria-hidden="true"
                            className="size-1.5 shrink-0 rounded-full bg-[#d3fb52]"
                          />
                        )}
                        <span className="truncate text-sm font-semibold text-white">
                          {group.label}
                        </span>
                      </div>

                      <div className="flex shrink-0 items-center gap-2">
                        <span className="hidden font-mono text-[10px] uppercase tracking-wider text-white/45 sm:inline">
                          {group.highlightTag}
                        </span>
                        <ChevronDown
                          className={cn(
                            "size-4 text-white/65 transition-transform duration-200",
                            isExpanded && "rotate-180 text-[#d3fb52]",
                          )}
                          aria-hidden="true"
                        />
                      </div>
                    </button>

                    {isExpanded && (
                      <div
                        id={panelId}
                        className="grid grid-cols-1 gap-1 border-t border-white/[0.07] bg-[#101117]/60 p-2 sm:grid-cols-2"
                      >
                        {group.items.map((item) => (
                          <NavLink
                            key={`${group.id}:${item.to}:${item.label}`}
                            to={item.to}
                            end={item.to === "/projects" || item.to === "/events"}
                            onClick={() => setIsMobileMenuOpen(false)}
                            className={({ isActive }) =>
                              cn(
                                "flex min-h-[40px] items-center justify-between gap-2 rounded-lg px-3 py-2 text-xs font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                                isActive
                                  ? "bg-[#d3fb52] font-semibold text-[#14151c]"
                                  : "text-white/85 hover:bg-white/[0.07] hover:text-white",
                              )
                            }
                          >
                            <span className="truncate">{item.label}</span>
                            <ArrowUpRight className="size-3.5 shrink-0 opacity-65" aria-hidden="true" />
                          </NavLink>
                        ))}
                      </div>
                    )}
                  </div>
                );
              })}

              {isPlatformAdmin ? (
                <div className="space-y-1.5 rounded-xl border border-[#d3fb52]/30 bg-[#d3fb52]/[0.06] p-2.5">
                  <NavLink
                    to="/admin"
                    onClick={() => setIsMobileMenuOpen(false)}
                    className={({ isActive }) =>
                      cn(
                        "flex min-h-[42px] items-center justify-between gap-2 rounded-lg px-3 py-2 text-sm font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                        isActive
                          ? "bg-[#d3fb52] text-[#14151c]"
                          : "text-[#d3fb52] hover:bg-[#d3fb52]/15",
                      )
                    }
                  >
                    <span className="inline-flex items-center gap-2">
                      <Shield className="size-4" aria-hidden="true" />
                      Admin Console
                    </span>
                    <ArrowUpRight className="size-4 opacity-70" aria-hidden="true" />
                  </NavLink>
                  <div className="grid grid-cols-2 gap-1">
                    {[
                      { label: "Org Requests", to: "/admin/organization-requests" },
                      { label: "Catalog", to: "/admin/catalog" },
                      { label: "Roles", to: "/admin/roles" },
                      { label: "Archive", to: "/admin/restore" },
                      { label: "Audit Logs", to: "/admin/audit-logs" },
                      { label: "Comms & Reports", to: "/admin/communications" },
                      { label: "Org Command", to: "/dashboard/org" },
                    ].map((adminLink) => (
                      <NavLink
                        key={adminLink.to}
                        to={adminLink.to}
                        onClick={() => setIsMobileMenuOpen(false)}
                        className="inline-flex min-h-[38px] items-center rounded-lg px-2.5 py-1.5 text-xs font-medium text-white/80 hover:bg-white/[0.07] hover:text-white"
                      >
                        {adminLink.label}
                      </NavLink>
                    ))}
                  </div>
                </div>
              ) : (
                isAuthenticated && (
                  <NavLink
                    to={isOrgAdmin || orgAdminIds.length > 0 ? "/dashboard/org" : "/dashboard"}
                    onClick={() => setIsMobileMenuOpen(false)}
                    className={({ isActive }) =>
                      cn(
                        "flex min-h-[44px] items-center justify-between gap-2 rounded-xl border px-3.5 py-2 text-sm font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                        isActive
                          ? "border-[#d3fb52] bg-[#d3fb52] text-[#14151c]"
                          : "border-[#d3fb52]/30 bg-[#d3fb52]/[0.08] text-[#d3fb52] hover:bg-[#d3fb52]/15",
                      )
                    }
                  >
                    <span className="inline-flex items-center gap-2">
                      <LayoutDashboard className="size-4" aria-hidden="true" />
                      My Role Dashboard
                    </span>
                    <ArrowUpRight className="size-4 opacity-70" aria-hidden="true" />
                  </NavLink>
                )
              )}
            </nav>

            <div className="mt-3 space-y-2.5 border-t border-white/10 pt-3">
              {isAuthenticated && user ? (
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <Link
                    to="/profile/me"
                    onClick={() => setIsMobileMenuOpen(false)}
                    className="inline-flex min-h-[40px] items-center gap-2 rounded-xl px-1.5 text-xs font-medium text-white hover:text-[#d3fb52]"
                  >
                    <Avatar
                      src={user.photoUrl ?? user.profile?.photoUrl}
                      name={`${user.firstName} ${user.lastName}`}
                      size="xs"
                    />
                    <span>
                      {user.firstName} {user.lastName}
                    </span>
                  </Link>

                  <div className="flex items-center gap-2">
                    <Link
                      to="/privacy"
                      onClick={() => setIsMobileMenuOpen(false)}
                      className="rounded-lg px-2 py-1.5 text-[11px] text-white/60 hover:text-[#d3fb52]"
                    >
                      Privacy &amp; Policy
                    </Link>
                    <button
                      type="button"
                      disabled={isSigningOut}
                      onClick={() => {
                        void handleLogout();
                      }}
                      className="min-h-[40px] cursor-pointer rounded-xl border border-red-500/30 bg-red-500/10 px-3 py-1.5 text-xs font-semibold text-red-200 hover:bg-red-500/20 disabled:opacity-50"
                    >
                      Sign Out
                    </button>
                  </div>
                </div>
              ) : (
                <>
                  <div className="grid grid-cols-2 gap-2">
                    <Link
                      to="/auth/login"
                      onClick={() => setIsMobileMenuOpen(false)}
                      className="inline-flex min-h-[42px] items-center justify-center rounded-xl border border-white/20 bg-white/[0.04] px-3 py-2 text-xs font-semibold text-white transition-colors hover:bg-white/10"
                    >
                      Sign In
                    </Link>
                    <Link
                      to="/auth/register"
                      onClick={() => setIsMobileMenuOpen(false)}
                      className="inline-flex min-h-[42px] items-center justify-center rounded-xl bg-[#d3fb52] px-3 py-2 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
                    >
                      Create Account
                    </Link>
                  </div>
                  <div className="flex justify-center">
                    <Link
                      to="/privacy"
                      onClick={() => setIsMobileMenuOpen(false)}
                      className="text-[11px] text-white/55 hover:text-[#d3fb52]"
                    >
                      Privacy &amp; Platform Policy
                    </Link>
                  </div>
                </>
              )}
            </div>
          </div>
        </>
      )}
    </header>
  );
}
