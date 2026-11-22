/**
 * @date 2026-11-22
 * @purpose Mesh gradient hero with giant headline, live typewriter search bar transitioning into manual input, and chip routing.
 */
import { ArrowUp, Search } from "lucide-react";
import { useEffect, useRef, useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { cn } from "@/lib/cn";

interface QuickFilterTag {
  id: string;
  label: string;
  routePrefix: string;
  queryParam: string;
  sampleQuery: string;
}

const QUICK_FILTERS: readonly QuickFilterTag[] = [
  {
    id: "team-formation",
    label: "Team Formation",
    routePrefix: "/projects",
    queryParam: "keyword",
    sampleQuery: "Find teammates with Frontend, AI, or UI/UX skills for your project",
  },
  {
    id: "student-activities",
    label: "Student Activities",
    routePrefix: "/organizations",
    queryParam: "search",
    sampleQuery: "Discover student organizations and apply for recruitment opportunities",
  },
  {
    id: "events-workshops",
    label: "Events & Workshops",
    routePrefix: "/events",
    queryParam: "search",
    sampleQuery: "Register for university workshops, competitions, and event sessions",
  },
  {
    id: "skills-leaderboards",
    label: "Skills & Leaderboards",
    routePrefix: "/students",
    queryParam: "search",
    sampleQuery: "Build your student profile, verify participation, and unlock badges",
  },
];

export function HeroSection() {
  const navigate = useNavigate();
  const [activeChipId, setActiveChipId] = useState<string>("team-formation");
  const [isManualMode, setIsManualMode] = useState<boolean>(false);
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [typewriterText, setTypewriterText] = useState<string>("");
  const inputRef = useRef<HTMLInputElement | null>(null);

  const activeChip = QUICK_FILTERS.find((chip) => chip.id === activeChipId) ?? QUICK_FILTERS[0];

  // Typewriter effect rotating through active or cycle queries
  useEffect(() => {
    if (isManualMode) {
      return;
    }

    let isCancelled = false;
    let charIndex = 0;
    let isDeleting = false;
    let timerId: number | null = null;
    let currentSuggestionIdx = QUICK_FILTERS.findIndex((chip) => chip.id === activeChipId);
    if (currentSuggestionIdx < 0) currentSuggestionIdx = 0;

    const tick = () => {
      if (isCancelled) return;

      const currentSuggestion = QUICK_FILTERS[currentSuggestionIdx]?.sampleQuery ?? "";

      if (!isDeleting) {
        charIndex++;
        setTypewriterText(currentSuggestion.slice(0, charIndex));

        if (charIndex >= currentSuggestion.length) {
          isDeleting = true;
          timerId = window.setTimeout(tick, 2200);
          return;
        }
        timerId = window.setTimeout(tick, 38);
      } else {
        charIndex -= 2;
        if (charIndex < 0) charIndex = 0;
        setTypewriterText(currentSuggestion.slice(0, charIndex));

        if (charIndex === 0) {
          isDeleting = false;
          currentSuggestionIdx = (currentSuggestionIdx + 1) % QUICK_FILTERS.length;
          timerId = window.setTimeout(tick, 500);
          return;
        }
        timerId = window.setTimeout(tick, 22);
      }
    };

    timerId = window.setTimeout(tick, 600);

    return () => {
      isCancelled = true;
      if (timerId !== null) window.clearTimeout(timerId);
    };
  }, [isManualMode, activeChipId]);

  const handleChipClick = (chip: QuickFilterTag) => {
    setActiveChipId(chip.id);
    if (!isManualMode) {
      setTypewriterText(chip.sampleQuery);
    }
  };

  const handleContainerClick = () => {
    setIsManualMode(true);
    setTimeout(() => {
      inputRef.current?.focus();
    }, 50);
  };

  const handleSubmit = (event?: FormEvent) => {
    if (event) {
      event.preventDefault();
    }
    const query = isManualMode ? searchQuery.trim() : "";
    const route = activeChip.routePrefix;
    const param = activeChip.queryParam;

    if (query) {
      void navigate(`${route}?${param}=${encodeURIComponent(query)}`);
    } else {
      void navigate(route);
    }
  };

  return (
    <section className="teamup-hero-bg relative z-10 min-h-screen w-full flex flex-col justify-between pt-36 sm:pt-32 lg:pt-28 pb-10 px-4 sm:px-6">
      <div className="w-full max-w-[1060px] mx-auto flex flex-col items-center text-center my-auto">
        {/* Giant Condensed Display Headline */}
        <h1
          className="uppercase text-white font-bold select-none flex flex-col items-center"
          style={{
            fontFamily: "'Antonio', 'Barlow Condensed', sans-serif",
            fontSize: "clamp(2.9rem, 11.2vw, 9.5rem)",
            lineHeight: 0.9,
            letterSpacing: "-0.025em",
          }}
        >
          <span className="animate-hero-line-1 block">LET'S FIND</span>
          <span className="animate-hero-line-2 block mt-1">YOUR NEXT TEAM</span>
        </h1>

        {/* Subtitle grounded in the specification */}
        <p className="animate-hero-sub mt-5 sm:mt-7 text-white text-[15.5px] sm:text-[17px] md:text-[20px] font-normal tracking-[-0.01em] max-w-[760px] px-1">
          Build your skills, find your team, join student activities, and grow together from your
          first year at university.
        </p>

        {/* Large White Rounded Pill Search Bar with Typewriter & Live Input */}
        <form
          onSubmit={handleSubmit}
          className="animate-search-bar w-full max-w-[970px] mt-7 sm:mt-10"
        >
          <div
            id="search-bar-container"
            onClick={handleContainerClick}
            className="w-full h-[64px] sm:h-[76px] md:h-[86px] bg-white rounded-[20px] sm:rounded-[26px] pl-3 sm:pl-6 pr-2.5 sm:pr-4 flex items-center justify-between transition-all duration-200 shadow-[0_20px_50px_rgba(12,24,22,0.24)] hover:shadow-[0_24px_56px_rgba(12,24,22,0.3)] cursor-text"
          >
            {/* Left Search Icon */}
            <button
              type="submit"
              aria-label="Search"
              className="relative size-9 sm:size-11 rounded-full flex items-center justify-center shrink-0 transition-all duration-200 cursor-pointer text-[#12181c] hover:bg-black/[0.06] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              <Search className="size-5 sm:size-6" aria-hidden="true" />
            </button>

            {/* Center Area: Typewriter / Manual Input */}
            <div className="flex-1 px-2 sm:px-3 text-left overflow-hidden">
              {!isManualMode ? (
                <div
                  id="typewriter-display"
                  className="w-full truncate text-[#5f666e] text-[15px] sm:text-[18px] md:text-[21px] font-normal tracking-[-0.01em] flex items-center"
                >
                  <span id="typewriter-text" className="truncate">
                    {typewriterText || "Search projects, teammates, activities..."}
                  </span>
                  <span className="typewriter-cursor shrink-0" aria-hidden="true" />
                </div>
              ) : (
                <input
                  ref={inputRef}
                  id="manual-search-input"
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onBlur={() => {
                    if (!searchQuery) setIsManualMode(false);
                  }}
                  placeholder={`Search ${activeChip.label.toLowerCase()}...`}
                  aria-label={`Search ${activeChip.label}`}
                  className="w-full bg-transparent text-[#141a1f] text-[15px] sm:text-[18px] md:text-[21px] font-normal outline-none placeholder:text-[#6c737a]"
                />
              )}
            </div>

            {/* Right Submit Button */}
            <button
              id="search-submit-btn"
              type="submit"
              aria-label="Submit search"
              className="relative size-[42px] sm:size-[52px] rounded-[13px] sm:rounded-[15px] flex items-center justify-center shrink-0 transition-all duration-200 cursor-pointer bg-[#e7e8eb] text-[#686f77] hover:bg-[#d3fb52] hover:text-[#14151c] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
            >
              <ArrowUp className="size-5 sm:size-6 rotate-90" aria-hidden="true" />
            </button>
          </div>
        </form>

        {/* Quick Filter Options Row */}
        <div
          id="quick-filters-container"
          className="mt-7 sm:mt-10 flex flex-wrap items-center justify-center gap-2 sm:gap-2.5"
        >
          {QUICK_FILTERS.map((chip) => {
            const isSelected = activeChipId === chip.id;
            return (
              <button
                key={chip.id}
                type="button"
                onClick={() => handleChipClick(chip)}
                className={cn(
                  "cursor-pointer rounded-full px-4 py-2 text-xs sm:text-sm font-semibold transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                  isSelected
                    ? "bg-[#d3fb52] text-[#14151c] shadow-[0_4px_14px_rgba(211,251,82,0.35)] scale-105"
                    : "bg-white/10 text-white/85 hover:bg-white/20 hover:text-white border border-white/15",
                )}
              >
                {chip.label}
              </button>
            );
          })}
        </div>
      </div>

      {/* Smooth Scroll Cue */}
      <div className="w-full flex justify-center pt-6">
        <a
          href="#services"
          aria-label="Scroll to platform services"
          className="group flex flex-col items-center gap-1.5 text-white/60 hover:text-white transition-colors duration-200 cursor-pointer focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
        >
          <span className="text-[13px] font-medium tracking-wide">Explore platform modules</span>
          <span className="hero-scroll-cue text-sm">↓</span>
        </a>
      </div>
    </section>
  );
}

