/**
 * @date 2026-11-22
 * @purpose Main application shell framing full-bleed landing route and padded inner routes with fixed morphing Navbar.
 */
import type { ReactNode } from "react";
import { Link, Outlet, useLocation } from "react-router-dom";
import { CommandPalette } from "./CommandPalette";
import { Navbar, type NavbarProps } from "./Navbar";

export type AppShellProps = NavbarProps & {
  children?: ReactNode;
};

export function AppShell({ xpChipSlot, notificationSlot, children }: AppShellProps) {
  const location = useLocation();
  const isLandingPage = location.pathname === "/" || location.pathname === "/demo/hero";

  return (
    <div className="flex min-h-screen flex-col overflow-x-clip bg-[#14151c] text-white">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-[60] focus:rounded-xl focus:bg-[#d3fb52] focus:px-4 focus:py-2 focus:text-sm focus:font-semibold focus:text-[#14151c]"
      >
        Skip to main content
      </a>

      <Navbar xpChipSlot={xpChipSlot} notificationSlot={notificationSlot} />
      <CommandPalette />

      {isLandingPage ? (
        <main id="main-content" className="w-full flex-1">
          {children ?? <Outlet />}
        </main>
      ) : (
        <>
          <main
            id="main-content"
            className="mx-auto w-full max-w-6xl flex-1 px-3.5 pb-[calc(2.5rem+env(safe-area-inset-bottom))] pt-[calc(5.5rem+env(safe-area-inset-top))] sm:px-6 sm:pt-[calc(6.75rem+env(safe-area-inset-top))]"
          >
            {children ?? <Outlet />}
          </main>

          <footer className="border-t border-white/10 pb-[calc(1.5rem+env(safe-area-inset-bottom))] pt-6 text-xs text-white/55">
            <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-3 px-4 sm:flex-row sm:px-6">
              <div className="flex flex-wrap items-center justify-center gap-3 sm:justify-start">
                <span className="text-center sm:text-left">
                  © 2026 TeamUp · The Collaboration Network for Egyptian Youth
                </span>
              </div>
              <div className="flex flex-wrap items-center justify-center gap-x-4 gap-y-2">
                <Link to="/projects" className="hover:text-[#d3fb52]">
                  Projects
                </Link>
                <Link to="/organizations" className="hover:text-[#d3fb52]">
                  Organizations
                </Link>
                <Link to="/events" className="hover:text-[#d3fb52]">
                  Events
                </Link>
                <Link to="/leaderboard" className="hover:text-[#d3fb52]">
                  Leaderboard
                </Link>
                <Link to="/privacy" className="hover:text-[#d3fb52]">
                  Privacy &amp; Policy
                </Link>
              </div>
            </div>
          </footer>
        </>
      )}
    </div>
  );
}
