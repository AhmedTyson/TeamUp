/**
 * @date 2026-11-22
 * @purpose Flagship Landing Page porting 100% of landing-static.html with live SPA routing and micro-interactions.
 */
import { CampusMarquee } from "./CampusMarquee";
import { FeatureShowcaseSection } from "./FeatureShowcaseSection";
import { HeroSection } from "./HeroSection";
import { LandingFooter } from "./LandingFooter";
import { ServicesGridSection } from "./ServicesGridSection";

export function LandingPage() {
  return (
    <div id="top" className="min-h-screen w-full select-none overflow-x-clip bg-[#14151c]">
      {/* SECTION 1: HERO VIEWPORT WITH TYPEWRITER PILL SEARCH */}
      <HeroSection />

      {/* SECTION 2: 3x2 SERVICES SPOTLIGHT GRID & SEAMLESS RTL MARQUEE */}
      <ServicesGridSection />
      <div className="relative z-10 w-full bg-[#14151c] pb-10">
        <CampusMarquee />
      </div>

      {/* SECTION 3: 4 ALTERNATING GRADIENT FEATURE SHOWCASE ROWS */}
      <FeatureShowcaseSection />

      {/* SECTION 4: FOOTER SITEMAP & BURST WORDMARK */}
      <LandingFooter />
    </div>
  );
}

