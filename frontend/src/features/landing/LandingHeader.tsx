/**
 * @date 2026-11-22
 * @purpose Standalone morphing Liquid-Pill navigation header for Flagship Landing page faithfully ported from landing-static.html.
 */
import { ArrowRight, ArrowUpRight, LogOut, Menu, UserPlus, X } from "lucide-react";
import { useCallback, useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Avatar } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { cn } from "@/lib/cn";
import { NotificationBell } from "@/components/layout/NotificationBell";
import { XpLevelChip } from "@/components/layout/XpLevelChip";

interface NavOptionGroup {
  id: string;
  label: string;
  to: string;
  highlightTag: string;
  highlightCopy: string;
  subOptions: readonly { label: string; to: string }[];
}

const NAV_OPTIONS: readonly NavOptionGroup[] = [
  {
    id: "team-formation",
    label: "Team Formation",
    to: "/projects",
    highlightTag: "Cross-Faculty Squads",
    highlightCopy: "Match with React, AI, and Engineering builders across campus.",
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
    subOptions: [
      { label: "Points & Level System", to: "/gamification" },
      { label: "Badges & Achievements", to: "/gamification" },
      { label: "Campus Leaderboards", to: "/leaderboard" },
      { label: "Badge Unlock Criteria", to: "/gamification" },
      { label: "Notifications Inbox", to: "/notifications" },
    ],
  },
];

export function LandingHeader() {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();
  const [isScrolled, setIsScrolled] = useState(false);
  const [activeMenuId, setActiveMenuId] = useState<string | null>(null);
  const [isMobileOpen, setIsMobileOpen] = useState(false);
  const closeTimerRef = useRef<number | null>(null);

  const clearTimer = useCallback(() => {
    if (closeTimerRef.current !== null) {
      window.clearTimeout(closeTimerRef.current);
      closeTimerRef.current = null;
    }
  }, []);

  useEffect(() => {
    const handleScroll = () => {
      const scrollY = window.scrollY;
      setIsScrolled(scrollY > 90);
    };

    window.addEventListener("scroll", handleScroll, { passive: true });
    handleScroll();
    return () => {
      window.removeEventListener("scroll", handleScroll);
      clearTimer();
    };
  }, [clearTimer]);

  const activeOption = NAV_OPTIONS.find((opt) => opt.id === activeMenuId);

  return (
    <header
      id="main-header"
      className={cn(
        "fixed top-0 left-0 right-0 z-50 transition-all duration-300 ease-out pointer-events-none px-3 sm:px-5 md:px-10",
        isScrolled ? "pt-2 sm:pt-3" : "pt-2.5 sm:pt-4",
      )}
    >
      <div
        id="header-inner"
        className={cn(
          "mx-auto pointer-events-auto transition-all duration-300 ease-out flex items-center justify-between",
          isScrolled
            ? "max-w-[1120px] bg-white rounded-[20px] px-4 py-2 text-[#14151c] shadow-[0_18px_46px_-8px_rgba(8,18,20,0.42)] border border-black/10"
            : "max-w-[1440px] px-1 sm:px-2 bg-transparent text-white",
        )}
      >
        {/* Left Zone: Brand Wordmark / Lime Square Icon */}
        <div className="flex items-center gap-2 shrink-0">
          <Link
            to="/"
            title="TeamUp"
            aria-label="TeamUp Home"
            className="inline-flex items-center focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
          >
            {isScrolled ? (
              <span
                style={{ fontFamily: "'Barlow Condensed', sans-serif" }}
                className="flex size-9 sm:size-10 items-center justify-center rounded-[10px] sm:rounded-[12px] bg-[#d3fb52] text-[22px] sm:text-[26px] font-black italic text-[#14151c] transition-transform hover:scale-105"
              >
                T
              </span>
            ) : (
              <span
                style={{ fontFamily: "'Barlow Condensed', sans-serif" }}
                className="text-[#d3fb52] text-[30px] sm:text-[36px] md:text-[40px] font-black italic tracking-[-0.03em] hover:scale-[1.03] transition-transform"
              >
                TeamUp
              </span>
            )}
          </Link>
        </div>

        {/* Center Zone: 5 Module Dropdowns */}
        <nav
          aria-label="Landing Navigation"
          className="relative hidden lg:flex items-center gap-1 sm:gap-1.5"
          onMouseLeave={() => {
            clearTimer();
            closeTimerRef.current = window.setTimeout(() => {
              setActiveMenuId(null);
            }, 180);
          }}
        >
          {NAV_OPTIONS.map((option) => {
            const isOpen = activeMenuId === option.id;
            return (
              <div
                key={option.id}
                onMouseEnter={() => {
                  clearTimer();
                  setActiveMenuId(option.id);
                }}
                className="relative"
              >
                <button
                  type="button"
                  aria-expanded={isOpen}
                  onClick={() => setActiveMenuId((prev) => (prev === option.id ? null : option.id))}
                  className={cn(
                    "cursor-pointer px-2.5 py-1.5 rounded-[10px] text-[13px] font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                    isScrolled
                      ? isOpen
                        ? "bg-[#d3fb52] text-[#14151c] font-semibold"
                        : "text-[#14151c]/80 hover:text-[#14151c] hover:bg-black/5"
                      : isOpen
                        ? "bg-white/15 text-white font-semibold"
                        : "text-white/85 hover:text-white hover:bg-white/10",
                  )}
                >
                  {option.label}
                </button>
              </div>
            );
          })}

          {/* Active Dropdown Mega-Panel */}
          {activeOption && (
            <div
              onMouseEnter={clearTimer}
              onMouseLeave={() => {
                clearTimer();
                closeTimerRef.current = window.setTimeout(() => {
                  setActiveMenuId(null);
                }, 180);
              }}
              role="menu"
              aria-label={`${activeOption.label} menu`}
              className="absolute left-1/2 top-full mt-2 w-[480px] -translate-x-1/2 rounded-2xl border border-black/10 bg-white p-4 text-[#14151c] shadow-[0_24px_54px_-10px_rgba(0,0,0,0.35)] animate-nav-fold-in z-50"
            >
              <div className="flex items-center justify-between border-b border-black/10 pb-2 mb-2">
                <div>
                  <span className="text-[10px] font-bold uppercase tracking-wider text-[#15733f]">
                    {activeOption.highlightTag}
                  </span>
                  <p className="text-xs text-[#4b5563]">{activeOption.highlightCopy}</p>
                </div>
                <Link
                  to={activeOption.to}
                  onClick={() => setActiveMenuId(null)}
                  className="inline-flex items-center gap-1 text-xs font-bold text-[#15733f] hover:underline"
                >
                  Explore
                  <ArrowRight className="size-3" aria-hidden="true" />
                </Link>
              </div>

              <ul className="grid grid-cols-2 gap-1 pt-1">
                {activeOption.subOptions.map((sub) => (
                  <li key={sub.label}>
                    <Link
                      to={sub.to}
                      onClick={() => setActiveMenuId(null)}
                      className="group flex items-center justify-between rounded-xl px-2.5 py-2 text-xs font-medium text-[#14151c] hover:bg-[#eef5eb] transition-colors"
                    >
                      <span>{sub.label}</span>
                      <ArrowUpRight className="size-3 text-[#15733f] opacity-0 group-hover:opacity-100 transition-opacity" />
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </nav>

        {/* Right Zone: Guest CTAs or Authenticated User Controls */}
        <div className="flex items-center gap-2">
          {isAuthenticated && user ? (
            <div className="flex items-center gap-2">
              <XpLevelChip isScrolled={isScrolled} />
              <NotificationBell />
              <Link
                to="/profile/me"
                title={`${user.firstName} ${user.lastName}`}
                className={cn(
                  "inline-flex items-center gap-1.5 rounded-xl border px-2 py-1 text-xs font-medium transition-colors focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                  isScrolled
                    ? "border-black/15 bg-black/5 text-[#14151c] hover:bg-black/10"
                    : "border-white/20 bg-white/10 text-white hover:bg-white/20",
                )}
              >
                <Avatar src={user.photoUrl ?? user.profile?.photoUrl} name={`${user.firstName} ${user.lastName}`} size="xs" />
                <span className="hidden sm:inline font-semibold">{user.firstName}</span>
              </Link>
              <button
                type="button"
                aria-label="Sign out"
                onClick={() => {
                  void logout();
                  void navigate("/auth/login");
                }}
                className={cn(
                  "cursor-pointer inline-flex size-8 items-center justify-center rounded-xl border transition-colors focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                  isScrolled
                    ? "border-black/15 bg-black/5 text-[#14151c] hover:text-red-600 hover:bg-red-500/10"
                    : "border-white/20 bg-white/10 text-white hover:text-red-300 hover:bg-red-500/20",
                )}
              >
                <LogOut className="size-3.5" aria-hidden="true" />
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <Link
                to="/auth/register"
                className={cn(
                  "hidden sm:inline-flex items-center gap-1 rounded-[10px] border px-3 py-1.5 text-xs font-medium transition-colors focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                  isScrolled
                    ? "border-[#14151c]/70 text-[#14151c] hover:bg-black/5"
                    : "border-white/80 text-white hover:bg-white/10",
                )}
              >
                <UserPlus className="size-3.5" aria-hidden="true" />
                Join Free
              </Link>
              <Link
                to="/auth/login"
                className={cn(
                  "inline-flex items-center rounded-[10px] px-3.5 py-1.5 text-xs font-bold transition-colors focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                  isScrolled
                    ? "bg-[#14151c] text-white hover:bg-[#232634]"
                    : "bg-[#d3fb52] text-[#14151c] hover:bg-[#deff6e]",
                )}
              >
                Sign In
              </Link>
            </div>
          )}

          {/* Mobile hamburger button */}
          <button
            type="button"
            onClick={() => setIsMobileOpen((prev) => !prev)}
            aria-label={isMobileOpen ? "Close menu" : "Open menu"}
            className={cn(
              "lg:hidden inline-flex size-8 items-center justify-center rounded-xl border cursor-pointer focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
              isScrolled ? "border-black/15 text-[#14151c]" : "border-white/20 text-white",
            )}
          >
            {isMobileOpen ? <X className="size-4" /> : <Menu className="size-4" />}
          </button>
        </div>
      </div>

      {/* Mobile Drawer */}
      {isMobileOpen && (
        <div className="pointer-events-auto mx-auto mt-2 max-w-[1120px] rounded-2xl border border-white/10 bg-[#14151c]/95 p-4 text-white shadow-2xl backdrop-blur-md lg:hidden">
          <ul className="space-y-2">
            {NAV_OPTIONS.map((opt) => (
              <li key={opt.id}>
                <Link
                  to={opt.to}
                  onClick={() => setIsMobileOpen(false)}
                  className="flex items-center justify-between rounded-xl px-3 py-2 text-sm font-semibold hover:bg-white/10"
                >
                  <span>{opt.label}</span>
                  <ArrowRight className="size-4 text-[#d3fb52]" />
                </Link>
              </li>
            ))}
          </ul>
        </div>
      )}
    </header>
  );
}
