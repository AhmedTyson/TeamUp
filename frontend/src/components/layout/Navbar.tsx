/**
 * @date 2026-11-21
 * @purpose Sticky top navigation bar with TeamUp brand mark, module links, and live API status badge.
 */
import { Activity } from "lucide-react";
import { Link, NavLink } from "react-router-dom";
import { useApiHealth } from "@/hooks/useApiHealth";

const NAV_LINKS = [
  { to: "/projects", label: "Projects" },
  { to: "/organizations", label: "Organizations" },
  { to: "/events", label: "Events" },
  { to: "/gamification", label: "Gamification" },
] as const;

export function Navbar() {
  const apiStatus = useApiHealth();

  const statusStyles =
    apiStatus === "online"
      ? "border-emerald-500/40 bg-emerald-500/10 text-emerald-300"
      : apiStatus === "offline"
        ? "border-white/15 bg-white/5 text-white/60"
        : "border-[#d3fb52]/30 bg-[#d3fb52]/10 text-[#d3fb52]";

  const statusLabel =
    apiStatus === "online" ? "API Online" : apiStatus === "offline" ? "API Offline" : "Checking API";

  return (
    <header className="sticky top-0 z-40 border-b border-white/10 bg-[#14151c]/85 backdrop-blur-md">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6">
        <Link
          to="/"
          className="text-3xl font-black italic tracking-tight text-[#d3fb52] transition-opacity hover:opacity-90"
          style={{ fontFamily: "var(--font-brand)" }}
        >
          TeamUp
        </Link>

        <nav aria-label="Primary navigation" className="hidden items-center gap-1 md:flex">
          {NAV_LINKS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                  isActive
                    ? "bg-[#d3fb52]/15 text-[#d3fb52]"
                    : "text-white/75 hover:bg-white/5 hover:text-white"
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="flex items-center gap-3">
          <span
            className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-medium ${statusStyles}`}
          >
            <Activity className="size-3.5" aria-hidden="true" />
            {statusLabel}
          </span>

          <Link
            to="/auth"
            className="rounded-xl bg-[#d3fb52] px-3.5 py-1.5 text-sm font-semibold text-[#14151c] transition-transform hover:scale-[1.02]"
          >
            Sign In
          </Link>
        </div>
      </div>
    </header>
  );
}
