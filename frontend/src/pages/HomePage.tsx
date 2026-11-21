/**
 * @date 2026-11-21
 * @purpose TeamUp full-stack React landing page introducing core platform modules.
 */
import { ArrowRight, Award, Calendar, FolderKanban, Users } from "lucide-react";
import { Link } from "react-router-dom";

const PLATFORM_MODULES = [
  {
    title: "Projects & Teams",
    description: "Find cross-faculty teammates, match required skills, and ship tasks together.",
    to: "/projects",
    endpoint: "/api/v1/projects",
    icon: FolderKanban,
  },
  {
    title: "Student Organizations",
    description: "Explore campus chapters, executive committees, and club recruitment.",
    to: "/organizations",
    endpoint: "/api/v1/organizations",
    icon: Users,
  },
  {
    title: "Campus Events",
    description: "Register for workshops, track multi-session schedules, and check in with QR/PIN.",
    to: "/events",
    endpoint: "/api/v1/events",
    icon: Calendar,
  },
  {
    title: "Gamification & Badges",
    description: "Earn verifiable XP through an append-only score ledger and unlock milestones.",
    to: "/gamification",
    endpoint: "/api/v1/badges",
    icon: Award,
  },
] as const;

export function HomePage() {
  return (
    <div className="space-y-12">
      <section className="rounded-3xl border border-white/10 bg-gradient-to-br from-[#1e3338] via-[#22352c] to-[#14151c] p-8 sm:p-12">
        <p className="inline-flex items-center rounded-full border border-[#d3fb52]/40 bg-[#d3fb52]/10 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-[#d3fb52]">
          Student Collaboration Platform
        </p>
        <h1
          className="mt-4 text-5xl font-bold uppercase leading-[0.95] tracking-tight text-white sm:text-7xl"
          style={{ fontFamily: "var(--font-display)" }}
        >
          Let&apos;s Find Your Next Team
        </h1>
        <p className="mt-4 max-w-2xl text-base text-white/80 sm:text-lg">
          Build your skills, form cross-functional student teams, join campus clubs, and level up
          your university journey — powered by React, Tailwind CSS v4, and the TeamUp .NET 10 Web API.
        </p>
        <div className="mt-8 flex flex-wrap gap-3">
          <Link
            to="/projects"
            className="inline-flex items-center gap-2 rounded-xl bg-[#d3fb52] px-5 py-2.5 text-sm font-semibold text-[#14151c] transition-transform hover:scale-[1.02]"
          >
            Explore Projects
            <ArrowRight className="size-4" aria-hidden="true" />
          </Link>
          <Link
            to="/auth/login"
            className="inline-flex items-center gap-2 rounded-xl border border-white/20 bg-white/5 px-5 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-white/10"
          >
            Student Sign In
          </Link>
        </div>
      </section>

      <section aria-labelledby="modules-heading" className="space-y-4">
        <h2
          id="modules-heading"
          className="text-2xl font-semibold tracking-tight text-white"
          style={{ fontFamily: "var(--font-heading)" }}
        >
          Platform Modules
        </h2>
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {PLATFORM_MODULES.map((mod) => {
            const Icon = mod.icon;
            return (
              <Link
                key={mod.to}
                to={mod.to}
                className="group rounded-2xl border border-white/10 bg-[#1b1d26] p-6 transition-all hover:-translate-y-0.5 hover:border-[#d3fb52]/40"
              >
                <div className="flex items-center justify-between">
                  <div className="flex size-10 items-center justify-center rounded-xl bg-[#d3fb52]/15 text-[#d3fb52]">
                    <Icon className="size-5" aria-hidden="true" />
                  </div>
                  <code className="rounded-md bg-white/5 px-2 py-0.5 text-xs text-emerald-300">
                    {mod.endpoint}
                  </code>
                </div>
                <h3 className="mt-4 text-lg font-semibold text-white group-hover:text-[#d3fb52]">
                  {mod.title}
                </h3>
                <p className="mt-1.5 text-sm text-white/70">{mod.description}</p>
              </Link>
            );
          })}
        </div>
      </section>
    </div>
  );
}
