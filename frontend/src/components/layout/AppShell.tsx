/**
 * @date 2026-11-21
 * @purpose Main application shell framing all client routes with Navbar and Footer.
 */
import { Outlet } from "react-router-dom";
import { Navbar } from "./Navbar";

export function AppShell() {
  return (
    <div className="flex min-h-screen flex-col bg-[#14151c] text-white">
      <Navbar />
      <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-10 sm:px-6">
        <Outlet />
      </main>
      <footer className="border-t border-white/10 py-6 text-center text-xs text-white/50">
        TeamUp Full-Stack Platform · React + Vite + Tailwind CSS v4 · ASP.NET Core 10 Web API
      </footer>
    </div>
  );
}
