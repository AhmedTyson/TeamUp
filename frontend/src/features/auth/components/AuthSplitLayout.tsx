/**
 * @date 2026-11-22
 * @purpose Handshake-inspired split layout framing authentication and onboarding flows.
 */
import { Award, QrCode, ShieldCheck, Users } from "lucide-react";
import type { ReactNode } from "react";
import { Badge } from "@/components/ui";

export interface AuthSplitLayoutProps {
  badge: string;
  headline: string;
  showcaseSubtitle: string;
  title: string;
  description: string;
  footer?: ReactNode;
  children: ReactNode;
}

const HIGHLIGHTS = [
  {
    icon: Users,
    title: "Cross-Faculty Squads",
    detail: "Join skill-matched student teams and Kanban sprints.",
  },
  {
    icon: QrCode,
    title: "60s Live QR Attendance",
    detail: "Verify campus sessions and earn instant activity XP.",
  },
  {
    icon: Award,
    title: "Verifiable XP Ledger",
    detail: "Build a portfolio backed by immutable milestones & badges.",
  },
] as const;

export function AuthSplitLayout({
  badge,
  headline,
  showcaseSubtitle,
  title,
  description,
  footer,
  children,
}: AuthSplitLayoutProps) {
  return (
    <section className="mx-auto grid w-full max-w-5xl items-stretch gap-8 py-2 lg:grid-cols-12">
      <aside className="relative flex flex-col justify-between overflow-hidden rounded-3xl border border-white/10 bg-[#1b1d26] p-7 sm:p-8 lg:col-span-5">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -right-20 -top-24 size-64 rounded-full bg-[#d3fb52]/12 blur-3xl"
        />
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -bottom-24 -left-16 size-64 rounded-full bg-[#10b981]/12 blur-3xl"
        />

        <div className="relative space-y-5">
          <Badge variant="accent" size="md">
            <ShieldCheck className="size-3.5" aria-hidden="true" />
            {badge}
          </Badge>

          <div className="space-y-3">
            <h1
              className="text-4xl font-bold uppercase leading-[0.95] tracking-tight text-white sm:text-5xl"
              style={{ fontFamily: "var(--font-display)" }}
            >
              {headline}
            </h1>
            <p className="text-sm leading-relaxed text-[#9ca3af]">{showcaseSubtitle}</p>
          </div>
        </div>

        <ul className="relative mt-8 space-y-3">
          {HIGHLIGHTS.map((item) => {
            const Icon = item.icon;
            return (
              <li
                key={item.title}
                className="flex items-start gap-3 rounded-2xl border border-white/8 bg-[#14151c]/75 p-3.5"
              >
                <span className="mt-0.5 inline-flex size-8 shrink-0 items-center justify-center rounded-xl bg-[#d3fb52]/12 text-[#d3fb52]">
                  <Icon className="size-4" aria-hidden="true" />
                </span>
                <div>
                  <p className="text-sm font-semibold text-white">{item.title}</p>
                  <p className="text-xs text-[#9ca3af]">{item.detail}</p>
                </div>
              </li>
            );
          })}
        </ul>
      </aside>

      <div className="flex flex-col justify-center rounded-3xl border border-white/10 bg-[#1b1d26] p-6 sm:p-8 lg:col-span-7">
        <div className="mb-6 space-y-1.5">
          <h2 className="text-2xl font-bold tracking-tight text-white sm:text-3xl">{title}</h2>
          <p className="text-sm text-[#9ca3af]">{description}</p>
        </div>

        <div className="space-y-5">{children}</div>

        {footer && (
          <div className="mt-6 border-t border-white/10 pt-5 text-sm text-[#9ca3af]">
            {footer}
          </div>
        )}
      </div>
    </section>
  );
}
