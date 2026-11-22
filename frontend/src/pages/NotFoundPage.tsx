/**
 * @date 2026-11-22
 * @purpose Handshake Rosetta 404 fallback stage with clear navigation back to active student hubs.
 */
import { ArrowRight, Compass } from "lucide-react";
import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <section className="teamup-hero-bg mx-auto flex max-w-3xl flex-col items-center justify-center rounded-[28px] border border-white/12 px-6 py-16 text-center shadow-2xl sm:py-20">
      <p className="text-xs font-bold uppercase tracking-[0.22em] text-[#d3fb52]">Route 404</p>
      <h1
        className="mt-2 font-bold uppercase text-white"
        style={{
          fontFamily: "var(--font-display)",
          fontSize: "clamp(3.2rem, 8vw, 5.5rem)",
          lineHeight: 0.9,
        }}
      >
        PAGE NOT FOUND
      </h1>
      <p className="mt-3 max-w-md text-sm leading-relaxed text-white/80 sm:text-base">
        The campus page you requested doesn&apos;t exist or may have moved to another hub.
      </p>
      <div className="mt-7 flex flex-wrap items-center justify-center gap-3">
        <Link
          to="/"
          className="inline-flex h-11 items-center gap-2 rounded-full bg-[#d3fb52] px-6 text-sm font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
        >
          <span>Return Home</span>
          <ArrowRight className="size-4" aria-hidden="true" />
        </Link>
        <Link
          to="/projects"
          className="inline-flex h-11 items-center gap-2 rounded-full border border-white/25 bg-black/25 px-6 text-sm font-semibold text-white transition-colors hover:border-white/55"
        >
          <Compass className="size-4 text-[#d3fb52]" aria-hidden="true" />
          <span>Explore Squads</span>
        </Link>
      </div>
    </section>
  );
}
