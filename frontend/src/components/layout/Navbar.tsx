/**
 * @date 2026-11-22
 * @purpose Sticky top navigation bar with TeamUp brand mark, module links, responsive mobile drawer, slots for NotificationBell & XpLevelChip, and auth state controls.
 */
import {
  Activity,
  Bell,
  Loader2,
  LogOut,
  Menu,
  Shield,
  Sparkles,
  UserPlus,
  X,
} from "lucide-react";
import { useEffect, useState, type ReactNode } from "react";
import { Link, NavLink, useLocation, useNavigate } from "react-router-dom";
import { USER_ROLES } from "@/api";
import { Avatar, Badge } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { useApiHealth } from "@/hooks/useApiHealth";

export interface NavbarProps {
  /** Cross-module slot for M4's <XpLevelChip /> widget. */
  xpChipSlot?: ReactNode;
  /** Cross-module slot for M4's <NotificationBell /> widget. */
  notificationSlot?: ReactNode;
}

const NAV_LINKS = [
  { to: "/projects", label: "Projects" },
  { to: "/skills", label: "Skills" },
  { to: "/organizations", label: "Organizations" },
  { to: "/events", label: "Events" },
  { to: "/students", label: "Students" },
  { to: "/leaderboard", label: "Leaderboard" },
  { to: "/gamification", label: "Gamification" },
] as const;

export function Navbar({ xpChipSlot, notificationSlot }: NavbarProps) {
  const apiStatus = useApiHealth();
  const { user, isAuthenticated, isPlatformAdmin, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isSigningOut, setIsSigningOut] = useState(false);

  useEffect(() => {
    setIsMobileMenuOpen(false);
  }, [location.pathname, location.search]);

  useEffect(() => {
    if (!isMobileMenuOpen) {
      return;
    }
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsMobileMenuOpen(false);
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isMobileMenuOpen]);

  const statusStyles =
    apiStatus === "online"
      ? "border-emerald-500/40 bg-emerald-500/10 text-emerald-300"
      : apiStatus === "offline"
        ? "border-white/15 bg-white/5 text-white/60"
        : "border-[#d3fb52]/30 bg-[#d3fb52]/10 text-[#d3fb52]";

  const statusLabel =
    apiStatus === "online" ? "API Online" : apiStatus === "offline" ? "API Offline" : "Checking API";

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
    <header className="sticky top-0 z-40 border-b border-white/10 bg-[#14151c]/90 backdrop-blur-md">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-3 px-4 sm:px-6">
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={() => setIsMobileMenuOpen((prev) => !prev)}
            aria-label={isMobileMenuOpen ? "Close navigation menu" : "Toggle navigation menu"}
            aria-expanded={isMobileMenuOpen}
            aria-controls="mobile-navigation-drawer"
            className="inline-flex size-9 cursor-pointer items-center justify-center rounded-xl border border-white/10 bg-[#1b1d26] text-white/80 transition-colors hover:border-[#d3fb52]/40 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] md:hidden"
          >
            {isMobileMenuOpen ? (
              <X className="size-4" aria-hidden="true" />
            ) : (
              <Menu className="size-4" aria-hidden="true" />
            )}
          </button>

          <Link
            to="/"
            className="text-3xl font-black italic tracking-tight text-[#d3fb52] transition-opacity hover:opacity-90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] focus-visible:ring-offset-2 focus-visible:ring-offset-[#14151c]"
            style={{ fontFamily: "var(--font-brand)" }}
          >
            TeamUp
          </Link>
        </div>

        <nav aria-label="Primary navigation" className="hidden items-center gap-1 md:flex">
          {NAV_LINKS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `rounded-lg px-2.5 py-1.5 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] ${
                  isActive
                    ? "bg-[#d3fb52] font-semibold text-[#14151c]"
                    : "text-white/75 hover:bg-white/5 hover:text-white"
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
          {isPlatformAdmin && (
            <NavLink
              to="/admin"
              className={({ isActive }) =>
                `inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] ${
                  isActive
                    ? "bg-[#d3fb52] font-semibold text-[#14151c]"
                    : "text-[#d3fb52]/85 hover:bg-white/5 hover:text-[#d3fb52]"
                }`
              }
            >
              <Shield className="size-3.5" aria-hidden="true" />
              Admin
            </NavLink>
          )}
        </nav>

        <div className="flex items-center gap-2 sm:gap-2.5">
          <span
            className={`hidden items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-medium lg:inline-flex ${statusStyles}`}
          >
            <Activity className="size-3.5" aria-hidden="true" />
            {statusLabel}
          </span>

          {isAuthenticated && user ? (
            <div className="flex items-center gap-2">
              {xpChipSlot ?? (
                <Link
                  to="/gamification"
                  aria-label="Open XP ledger and badges"
                  className="hidden items-center gap-1.5 rounded-full border border-[#d3fb52]/30 bg-[#d3fb52]/10 px-2.5 py-1 text-xs font-semibold text-[#d3fb52] transition-colors hover:bg-[#d3fb52]/20 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:inline-flex"
                >
                  <Sparkles className="size-3.5" aria-hidden="true" />
                  <span>XP &amp; Badges</span>
                </Link>
              )}

              {notificationSlot ?? (
                <Link
                  to="/notifications"
                  aria-label="Open notifications"
                  className="inline-flex size-8 items-center justify-center rounded-xl border border-white/10 bg-[#1b1d26] text-white/75 transition-colors hover:border-[#d3fb52]/40 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
                >
                  <Bell className="size-4" aria-hidden="true" />
                </Link>
              )}

              <Link
                to="/profile/me"
                className="inline-flex items-center gap-2 rounded-xl border border-white/10 bg-[#1b1d26] px-2.5 py-1 text-xs font-medium text-white transition-colors hover:border-[#d3fb52]/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
              >
                <Avatar name={`${user.firstName} ${user.lastName}`} size="xs" />
                <span className="hidden sm:inline">{user.firstName}</span>
                {user.role !== USER_ROLES.Student && (
                  <Badge variant="accent" size="sm" className="hidden xl:inline-flex">
                    {user.role}
                  </Badge>
                )}
              </Link>

              <button
                type="button"
                disabled={isSigningOut}
                onClick={() => {
                  void handleLogout();
                }}
                aria-label="Sign out"
                className="inline-flex size-8 cursor-pointer items-center justify-center rounded-xl border border-white/10 bg-white/5 text-white/75 transition-colors hover:border-red-500/40 hover:bg-red-500/10 hover:text-red-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] disabled:opacity-50"
              >
                {isSigningOut ? (
                  <Loader2 className="size-4 animate-spin" aria-hidden="true" />
                ) : (
                  <LogOut className="size-4" aria-hidden="true" />
                )}
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <Link
                to="/auth/register"
                className="hidden items-center gap-1.5 rounded-xl border border-white/15 bg-white/5 px-3 py-1.5 text-sm font-medium text-white transition-colors hover:border-[#d3fb52]/45 hover:text-[#d3fb52] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:inline-flex"
              >
                <UserPlus className="size-3.5" aria-hidden="true" />
                Join
              </Link>
              <Link
                to="/auth/login"
                className="rounded-xl bg-[#d3fb52] px-3.5 py-1.5 text-sm font-semibold text-[#14151c] transition-transform hover:scale-[1.02] hover:bg-[#deff6e] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] focus-visible:ring-offset-2 focus-visible:ring-offset-[#14151c]"
              >
                Sign In
              </Link>
            </div>
          )}
        </div>
      </div>

      {isMobileMenuOpen && (
        <div
          id="mobile-navigation-drawer"
          className="border-t border-white/10 bg-[#14151c] px-4 py-4 md:hidden"
        >
          <nav aria-label="Mobile navigation" className="flex flex-col gap-1">
            {NAV_LINKS.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `rounded-xl px-3 py-2 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] ${
                    isActive
                      ? "bg-[#d3fb52] font-semibold text-[#14151c]"
                      : "text-white/80 hover:bg-white/5 hover:text-white"
                  }`
                }
              >
                {item.label}
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

          <div className="mt-4 flex flex-wrap items-center justify-between gap-2 border-t border-white/10 pt-3">
            <span
              className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-medium ${statusStyles}`}
            >
              <Activity className="size-3.5" aria-hidden="true" />
              {statusLabel}
            </span>

            {isAuthenticated && user ? (
              <div className="flex items-center gap-2">
                <Link
                  to="/profile/me"
                  className="rounded-xl border border-white/12 bg-[#1b1d26] px-3 py-1.5 text-xs font-medium text-white hover:border-[#d3fb52]/40"
                >
                  My Profile
                </Link>
                <button
                  type="button"
                  disabled={isSigningOut}
                  onClick={() => {
                    void handleLogout();
                  }}
                  className="rounded-xl border border-red-500/35 bg-red-500/15 px-3 py-1.5 text-xs font-semibold text-red-200 hover:bg-red-500/25 disabled:opacity-50"
                >
                  Sign Out
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-2">
                <Link
                  to="/auth/register"
                  className="rounded-xl border border-white/15 bg-white/5 px-3 py-1.5 text-xs font-medium text-white"
                >
                  Create Account
                </Link>
                <Link
                  to="/auth/login"
                  className="rounded-xl bg-[#d3fb52] px-3 py-1.5 text-xs font-semibold text-[#14151c]"
                >
                  Sign In
                </Link>
              </div>
            )}
          </div>
        </div>
      )}
    </header>
  );
}
