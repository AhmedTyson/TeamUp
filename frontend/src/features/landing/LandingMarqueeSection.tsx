/**
 * @date 2026-11-24
 * @purpose Seamless RTL marquee strip of interactive student activity & community chapter tiles linking to filtered views.
 */
import { ArrowUpRight } from "lucide-react";
import type { RefObject } from "react";
import { ACM_ROTATIONS, MARQUEE_TILES, updateSpotlightCoords } from "./LandingConstants";

function renderMarqueeArtwork(tileId: (typeof MARQUEE_TILES)[number]["id"]) {
  switch (tileId) {
    case "campus-guilds":
      return (
        <div className="relative z-10 flex items-center gap-2 text-[#9d9fa7] transition-colors duration-300 group-hover:text-[#d3fb52]">
          <svg className="h-14 w-12 shrink-0" viewBox="0 0 64 72" fill="currentColor" aria-hidden="true">
            <path d="M36 6c3.5-4.2 9.5-3.2 11 1.5 1.2 3.8-2.8 8.2-11 12.5-8.2-4.3-12.2-8.7-11-12.5 1.5-4.7 7.5-5.7 11-1.5z" />
            <path d="M28 22c-11 0-19 7.5-19 17.5 0 7.2 4.6 13.1 11.4 15.2-5.8 1.8-9.4 5.6-9.4 10.3 0 6.8 7.6 11.5 18.5 11.5s18.5-4.7 18.5-11.5c0-6.2-5.4-10.2-14.5-11.2v-6.3h8.5v-8h-16v12.8c-4.2-1.2-7-4.8-7-9.8 0-5.8 4.4-10 10.5-10 4.5 0 8.2 2.4 9.6 6.2l8.2-3.4C44.8 27 37.4 22 28 22zm1.5 34.5c5.2 0 8.8 2 8.8 5s-3.6 5-8.8 5-8.8-2-8.8-5 3.6-5 8.8-5z" />
          </svg>
          <div className="flex flex-col text-left text-[15px] font-bold leading-[1.08] tracking-[-0.02em]">
            <span>Campus</span>
            <span>Guilds</span>
          </div>
        </div>
      );
    case "ieee-cs":
      return (
        <div className="relative z-10 flex items-center justify-center text-[#9d9fa7] transition-colors duration-300 group-hover:text-[#d3fb52]">
          <svg className="h-16 w-36 sm:w-40" viewBox="0 0 220 84" fill="currentColor" aria-hidden="true">
            <path d="M110 10L210 42L110 74L10 42L110 10Z" />
            <text
              x="110"
              y="51"
              textAnchor="middle"
              fill="#1c1e26"
              fontFamily="'Plus Jakarta Sans', sans-serif"
              fontWeight="900"
              fontSize="27"
              letterSpacing="-1"
            >
              IEEE · CS
            </text>
          </svg>
        </div>
      );
    case "robotics-ai":
      return (
        <div className="relative z-10 flex items-center justify-center text-[#9d9fa7] transition-colors duration-300 group-hover:text-[#d3fb52]">
          <svg className="h-20 w-32 sm:w-36" viewBox="0 0 200 100" fill="currentColor" aria-hidden="true">
            <polygon points="16,12 184,12 146,88 16,88" />
            <text
              x="32"
              y="39"
              fill="#1c1e26"
              fontFamily="'Plus Jakarta Sans', sans-serif"
              fontWeight="800"
              fontStyle="italic"
              fontSize="19"
            >
              Robotics
            </text>
            <text
              x="32"
              y="59"
              fill="#1c1e26"
              fontFamily="'Plus Jakarta Sans', sans-serif"
              fontWeight="800"
              fontStyle="italic"
              fontSize="19"
            >
              &amp; AI Lab
            </text>
            <text
              x="32"
              y="79"
              fill="#1c1e26"
              fontFamily="'Plus Jakarta Sans', sans-serif"
              fontWeight="800"
              fontStyle="italic"
              fontSize="19"
            >
              Chapter
            </text>
          </svg>
        </div>
      );
    case "eng-cs":
      return (
        <div className="relative z-10 flex items-center gap-2.5 px-4 text-[#9d9fa7] transition-colors duration-300 group-hover:text-[#d3fb52]">
          <svg
            className="h-10 w-8 shrink-0"
            viewBox="0 0 44 52"
            fill="none"
            stroke="currentColor"
            strokeWidth="2.6"
            aria-hidden="true"
          >
            <path d="M4 6h36v20c0 13-18 22-18 22S4 39 4 26V6z" />
            <path d="M4 16h36M14 6v10M30 6v10" />
            <path d="M12 24l10 10 10-10" strokeWidth="3" />
          </svg>
          <span
            className="whitespace-nowrap text-[20px] font-normal tracking-[-0.02em]"
            style={{ fontFamily: "Georgia, serif" }}
          >
            Eng &amp; CS
          </span>
        </div>
      );
    case "enactus":
      return (
        <div className="relative z-10 flex flex-col items-center justify-center leading-none text-[#9d9fa7] transition-colors duration-300 group-hover:text-[#d3fb52]">
          <span className="text-[22px] font-extrabold uppercase tracking-[0.06em]">ENACTUS</span>
          <span className="-mt-0.5 text-[24px] font-light lowercase tracking-[0.04em]">campus</span>
        </div>
      );
    case "acm":
      return (
        <div className="relative z-10 flex flex-col items-center justify-center text-[#9d9fa7] transition-colors duration-300 group-hover:text-[#d3fb52]">
          <svg className="mb-2 h-12 w-12" viewBox="0 0 64 64" fill="currentColor" aria-hidden="true">
            {ACM_ROTATIONS.map((angle) => (
              <rect
                key={angle}
                x="30.2"
                y="4"
                width="3.6"
                height="19"
                rx="1.8"
                transform={`rotate(${angle} 32 32)`}
              />
            ))}
          </svg>
          <span className="text-[14px] font-bold leading-tight tracking-[-0.01em]">
            STP &amp; ACM
          </span>
          <span className="text-[14px] font-bold leading-tight tracking-[-0.01em]">
            Egypt Chapters
          </span>
        </div>
      );
    case "gdg-cloud":
      return (
        <div className="relative z-10 flex flex-col items-center justify-center leading-none text-[#9d9fa7] transition-colors duration-300 group-hover:text-[#d3fb52]">
          <span className="text-[21px] font-black uppercase tracking-[0.05em]">GDG CLOUD</span>
          <span className="mt-1 text-[18px] font-normal uppercase tracking-[0.08em] opacity-80">
            HELWAN &amp; CAIRO
          </span>
        </div>
      );
  }
}

export interface LandingMarqueeSectionProps {
  sectionRef: RefObject<HTMLDivElement | null>;
  marqueeVisible: boolean;
  onNavigate: (to: string) => void;
}

export function LandingMarqueeSection({
  sectionRef,
  marqueeVisible,
  onNavigate,
}: LandingMarqueeSectionProps) {
  return (
    <div
      ref={sectionRef}
      className={`no-scrollbar section-reveal-up marquee-edge-mask -mx-4 mt-16 overflow-x-hidden overflow-y-hidden py-4 sm:-mx-6 md:-mx-12 ${
        marqueeVisible ? "is-visible" : ""
      }`}
    >
      <div id="marquee-track" className="marquee-track-rtl">
        {[0, 1].map((copy) => (
          <div
            key={copy}
            aria-hidden={copy === 1 ? "true" : undefined}
            className={
              copy === 1
                ? "marquee-duplicate-copy flex shrink-0 items-center"
                : "flex shrink-0 items-center"
            }
          >
            {MARQUEE_TILES.map((tile) => (
              <div
                key={`${copy}-${tile.id}`}
                onClick={() => onNavigate(tile.to)}
                onMouseMove={updateSpotlightCoords}
                className="marquee-tile group"
              >
                <span className="absolute right-4 top-4 z-10 flex h-8 w-8 scale-90 items-center justify-center rounded-full border border-white/[0.08] bg-white/[0.04] text-white/35 opacity-0 transition-all duration-300 group-hover:scale-100 group-hover:border-[#d3fb52] group-hover:bg-[#d3fb52] group-hover:text-[#14151c] group-hover:opacity-100">
                  <ArrowUpRight className="size-4 stroke-[2.2]" aria-hidden="true" />
                </span>
                {renderMarqueeArtwork(tile.id)}
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
