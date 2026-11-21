/**
 * @date 2026-11-21
 * @purpose Client-side 404 fallback route page.
 */
import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <section className="flex flex-col items-center justify-center rounded-2xl border border-white/10 bg-[#1b1d26] px-6 py-16 text-center">
      <p className="text-sm font-semibold uppercase tracking-widest text-[#d3fb52]">404</p>
      <h1 className="mt-2 text-3xl font-bold text-white">Page not found</h1>
      <p className="mt-2 text-sm text-white/65">
        The route you requested does not exist in this TeamUp workspace.
      </p>
      <Link
        to="/"
        className="mt-6 rounded-xl bg-[#d3fb52] px-4 py-2 text-sm font-semibold text-[#14151c]"
      >
        Return Home
      </Link>
    </section>
  );
}
