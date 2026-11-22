/**
 * @date 2026-11-22
 * @purpose Handshake editorial full-bleed photographic split layout for Student Login, Register, and Password Recovery (mobile form-first).
 */
import type { ReactNode } from "react";
import { UniversityLogo } from "@/components/ui";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { cn } from "@/lib/cn";

export interface AuthSplitLayoutProps {
  eyebrow?: string;
  headline: string;
  showcaseSubtitle: string;
  heroImageSrc?: string;
  heroImageAlt?: string;
  heroImagePosition?: string;
  title: string;
  description: string;
  previewSlot?: ReactNode;
  footer?: ReactNode;
  children: ReactNode;
}

export function AuthSplitLayout({
  eyebrow,
  headline,
  showcaseSubtitle,
  heroImageSrc = "/images/hero/hero-squad-lab.jpg",
  heroImageAlt = "Egyptian university students collaborating together around laptops on campus",
  heroImagePosition = "object-center",
  title,
  description,
  previewSlot,
  footer,
  children,
}: AuthSplitLayoutProps) {
  const { universities } = useUniversityCatalog(!previewSlot);
  const visibleUniversities = universities.slice(0, 6);
  const extraCount = Math.max(0, universities.length - visibleUniversities.length);

  return (
    <section className="mx-auto grid w-full max-w-[1140px] grid-cols-1 items-stretch gap-6 py-2 lg:grid-cols-12 lg:gap-8">
      {/* Elevated Form Panel (order-1 on mobile so inputs sit immediately above the fold) */}
      <div className="order-1 flex min-w-0 flex-col justify-center rounded-[28px] border border-white/[0.09] bg-[#181a23] p-5 shadow-[0_32px_80px_-16px_rgba(0,0,0,0.6)] sm:p-9 lg:order-2 lg:col-span-7 lg:p-10">
        <div className="mb-6 space-y-1.5">
          {eyebrow && (
            <p className="text-[11px] font-bold uppercase tracking-[0.18em] text-[#d3fb52]">
              {eyebrow}
            </p>
          )}
          <h2
            className="text-2xl font-bold tracking-tight text-white sm:text-3xl"
            style={{ fontFamily: "var(--font-heading)" }}
          >
            {title}
          </h2>
          <p className="text-sm leading-relaxed text-[#9ca3af]">{description}</p>
        </div>

        <div className="space-y-5">{children}</div>

        {footer && (
          <div className="mt-7 border-t border-white/[0.085] pt-5 text-sm text-[#9ca3af]">
            {footer}
          </div>
        )}
      </div>

      {/* Editorial Full-Bleed Campus Photography Showcase Panel */}
      <aside className="order-2 relative flex min-h-[360px] min-w-0 flex-col justify-between overflow-hidden rounded-[28px] border border-white/12 bg-[#14151c] p-5 shadow-[0_32px_70px_-18px_rgba(0,0,0,0.65)] sm:p-8 lg:order-1 lg:col-span-5 lg:min-h-[600px]">
        <img
          src={heroImageSrc}
          alt={heroImageAlt}
          loading="eager"
          decoding="async"
          className={cn("absolute inset-0 h-full w-full object-cover", heroImagePosition)}
        />
        <div
          aria-hidden="true"
          className="absolute inset-0 bg-gradient-to-b from-[#0d2722]/90 via-[#14151c]/55 to-[#14151c]/95"
        />

        {/* Top Brand + Display Anchor */}
        <div className="relative z-10 space-y-4">
          <span
            style={{ fontFamily: "'Barlow Condensed', sans-serif" }}
            className="inline-block text-2xl font-black italic leading-none tracking-[-0.03em] text-[#d3fb52]"
          >
            TeamUp
          </span>

          <h1
            className="text-4xl font-bold uppercase leading-[0.92] tracking-[-0.02em] text-white sm:text-5xl"
            style={{ fontFamily: "var(--font-display)" }}
          >
            {headline}
          </h1>

          <p className="text-sm leading-relaxed text-white/85">{showcaseSubtitle}</p>
        </div>

        {/* Bottom Contextual Slot or Circular Official University Emblem Cluster */}
        <div className="relative z-10 mt-6 space-y-4">
          {previewSlot ? (
            previewSlot
          ) : (
            <div className="rounded-2xl border border-white/15 bg-[#14151c]/82 p-4 backdrop-blur-xl shadow-2xl">
              <p className="text-xs font-semibold text-white">Verified Student Network</p>
              <p className="mt-0.5 text-[11px] text-white/70">
                Built for projects, clubs, and workshops across Egypt&apos;s partner universities
              </p>
              <div className="mt-3 flex items-center -space-x-2">
                {visibleUniversities.map((uni) => (
                  <div
                    key={uni.universityId}
                    className="size-8 overflow-hidden rounded-full bg-white p-1 ring-2 ring-[#14151c] shadow-sm"
                  >
                    <UniversityLogo
                      name={uni.name}
                      logoUrl={uni.logoUrl}
                      size="sm"
                      className="size-full rounded-full border-0 bg-transparent p-0 shadow-none"
                    />
                  </div>
                ))}
                {extraCount > 0 && (
                  <span className="inline-flex size-8 items-center justify-center rounded-full bg-[#d3fb52] text-[11px] font-extrabold text-[#14151c] ring-2 ring-[#14151c]">
                    +{extraCount}
                  </span>
                )}
              </div>
            </div>
          )}
        </div>
      </aside>
    </section>
  );
}
