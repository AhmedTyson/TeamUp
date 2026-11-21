/**
 * @date 2026-11-21
 * @purpose Clean placeholder route view wiring module navigation to backend API endpoints.
 */
import { ArrowLeft } from "lucide-react";
import { Link } from "react-router-dom";

interface ModulePlaceholderPageProps {
  title: string;
  description: string;
  endpoint: string;
}

export function ModulePlaceholderPage({
  title,
  description,
  endpoint,
}: ModulePlaceholderPageProps) {
  return (
    <section className="rounded-2xl border border-white/10 bg-[#1b1d26] p-8">
      <Link
        to="/"
        className="inline-flex items-center gap-1.5 text-xs font-medium text-white/60 transition-colors hover:text-[#d3fb52]"
      >
        <ArrowLeft className="size-3.5" aria-hidden="true" />
        Back to overview
      </Link>
      <h1
        className="mt-4 text-3xl font-bold tracking-tight text-white"
        style={{ fontFamily: "var(--font-heading)" }}
      >
        {title}
      </h1>
      <p className="mt-2 max-w-2xl text-sm text-white/75">{description}</p>
      <div className="mt-6 inline-flex items-center gap-2 rounded-xl border border-emerald-500/30 bg-emerald-500/10 px-3.5 py-2 text-xs text-emerald-300">
        <span>Backend API Endpoint:</span>
        <code className="font-semibold">{endpoint}</code>
      </div>
    </section>
  );
}
