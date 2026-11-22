# TeamUp Hero Section Redesign Plan — Anti-Slop Award Benchmark

> **Inspiration Gallery Directory:** [docs/inspiration/hero-sections/](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/)  
> **Target Implementation Page:** [HomePage.tsx](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/src/pages/HomePage.tsx) (`/`), paired with [index.css](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/src/index.css) Rosetta tokens and live database university catalog (`/api/v1/universities/catalog`).

---

## 1. Visual Teardown of the 10 Captured Hero Reference Images

Each reference screenshot in this folder was captured live at `1440×900` and visually inspected to identify structural patterns worth adopting—and dated patterns to reject:

| # | Reference File | Platform | What Makes It Work (Anti-Slop Strength) | What We Extract / Reject for TeamUp |
| :-: | :--- | :--- | :--- | :--- |
| `01` | [01-handshake-campus-network.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/01-handshake-campus-network.png) | **Handshake** (`joinhandshake.com`) | Condensed `Antonio` all-caps display authority (`LET'S FIND YOUR NEXT JOB`), deep campus-green radial mesh fading cleanly into `#14151c` obsidian, prompt-first white search bar, and sharp rectangular filter chips. | **Extract:** Preserves TeamUp's visual DNA and Rosetta palette (`#14151c`, `#d3fb52`, `#ffffff`). **Upgrade:** Make the bottom half of the viewport an active interactive campus showcase instead of empty dark space. |
| `02` | [02-linear-project-workspace.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/02-linear-project-workspace.png) | **Linear** (`linear.app`) | Anchors the lower half of the hero in a razor-sharp, interactive product canvas frame showing real statuses (`In Progress`), priority, owners, and activity feed—zero purple fog, zero floating bloat. | **Extract:** Crisp dark workspace framing, tabular metadata rows, and clean keyboard-friendly surface contrast. |
| `03` | [03-luma-events-workshops.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/03-luma-events-workshops.png) | **Luma** (`lu.ma`) | High-personality tactile event poster cards orbiting clean editorial copy (`Delightful events start here`). Authentic event imagery immediately tells what happens on the platform. | **Extract:** Tactile event & workshop card previews for campus bootcamps (`+100 XP`, QR check-in badge, seat counter). |
| `04` | [04-cal-interactive-widget.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/04-cal-interactive-widget.png) | **Cal.com** (`cal.com`) | Embeds living status pills (`Intro call 30m · ✓ Booked`) inline cleanly, proving real booking mechanics before scrolling. | **Extract:** Compact verified status pills (`✓ Attendance Verified`, `3/5 Teammates Joined`). **Reject:** Promotional `NEW v7` badge floating above `<h1>`. |
| `05` | [05-superlist-team-tasks.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/05-superlist-team-tasks.png) | **Superlist** (`superlist.com`) | Expressive typography accent + interactive live task-board dock (`Open / Completed` toggle, task items, assignee pills). | **Extract:** Interactive mode switcher inside the hero preview window. |
| `06` | [06-raycast-command-first.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/06-raycast-command-first.png) | **Raycast** (`raycast.com`) | Architectural lighting without cloudy blobs + OS/platform-specific immediate CTA utility. | **Extract:** Single decisive geometric background texture + sharp command bar responsiveness. |
| `07` | [07-peerlist-verified-profiles.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/07-peerlist-verified-profiles.png) | **Peerlist** (`peerlist.io`) | Inline claim input (`peerlist.io/username`) + clean real builder avatar mosaic social proof. | **Extract:** Live student avatar/campus-seal mosaic anchored right below the action area. **Reject:** Floating `"✨ Introducing Ads"` pill above `<h1>`. |
| `08` | [08-figjam-multiplayer-collab.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/08-figjam-multiplayer-collab.png) | **FigJam** (`figma.com/figjam`) | Live multiplayer cursor tags (`Himari`), sprint countdown timer, and collaborator pill strip inside a crisp dotted canvas. | **Extract:** Live collaborator badge (`FCAI · Team Lead`) & tactile squad activity indicators. |
| `09` | [09-devpost-hackathon-teams.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/09-devpost-hackathon-teams.png) | **Devpost** (`devpost.com`) | Dual audience split (`For organizers` / `For participants`) + hackathon search bar + sponsor strip. | **Anti-Pattern Warning:** Shows how side banner ads and busy multi-box borders fragment attention—TeamUp keeps strict whitespace discipline. |
| `10` | [10-contra-team-matchmaking.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/10-contra-team-matchmaking.png) | **Contra** (`contra.com`) | **Closest Structural Fit for TeamUp:** Centered prompt box + filter pills + horizontal **Trending Topics/Squads cards** with collaborator avatars + **`[Projects | People]`** segmented switch + skill discipline filters directly visible at the bottom of the first screen. | **Extract:** Immediate above-the-fold **Live Projects / Clubs / Workshops Explorer Dock** inside the hero, customized cleanly to TeamUp's Handshake Rosetta palette (`#14151c` + `#d3fb52` + white card contrast). |

---

## 2. Strict Anti-AI-Slop Rules Applied to TeamUp's Hero

Every design decision in the proposed hero explicitly avoids modern "vibe-coded" clichés:

1. **Zero Floating Promo Pills Above `<h1>`:** No `✨ v2.0 AI Team Matching Now Live` shimmer badge cluttering the headline anchor.
2. **Zero Random Purple/Pink Neon Blobs:** Stay locked to TeamUp & Handshake's authentic obsidian (`#14151c`), campus forest green gradient, crisp white cards (`#ffffff`), and electric lime (`#d3fb52`) tokens.
3. **Zero Colored Left-Border Accent Cards:** Cards use clean `1px` architectural borders (`rgba(255,255,255,0.09)`) and sharp shadows, never 3px colored left stripes.
4. **Zero Dead Mockup Screenshots:** Every pill, tab, discipline filter chip, and search input inside the hero must execute real interactive state filtering or real route navigation (`/projects?search=...`, `/organizations`, `/events`, `/leaderboard`).
5. **Authentic Official University Logos from DB:** All university seals shown in the hero campus strip and project cards come directly from `/api/v1/universities/catalog` (`Cairo University`, `Ain Shams University`, `Alexandria University`, `Capital University`, `GUC`, `AUC`, etc.) via `<UniversityLogo />`.

---

## 3. Proposed Hero Architecture: *"Command-First Campus Network + Interactive Squad Dock"*

We fuse **Handshake's typographic authority (`01`)** with **Contra's live segmented explorer (`10`)**, **Linear's workspace precision (`02`)**, and **Luma's event cards (`03`)**:

### Zone A — Editorial Headline & Command-First Finder (Top 50% of Hero Viewport)
- **Display Headline (`Antonio`, bold uppercase):**
  `LET'S FIND` / `YOUR NEXT TEAM` *(retains exact test hook compatibility & Handshake kinetic proportion)*.
- **Value Proposition (1 concise line):**
  `"Form cross-faculty project squads, join campus clubs, and verify workshops—starting from Year 1."`
- **Interactive Prompt & Search Bar:**
  - Large high-contrast white rounded search input (`#ffffff` on dark emerald radial mesh) with cycling student use-case prompts + live typing capture.
  - Pressing **Enter** or clicking the lime **Arrow Submit Button** immediately filters the Hero Sandbox cards OR navigates to `/projects?search=...` when a query is submitted.
- **Role / Discipline Filter Chips (`QUICK_FILTER_TAGS`):**
  - Clicking `Team Formation`, `Student Activities`, `Events & Workshops`, or `Skills & Leaderboards` now switches the **Live Hero Sandbox Mode** right below instead of only typing text into an input!

### Zone B — Live Interactive Campus Sandbox Dock (Bottom 50% of Hero Viewport, Above the Fold)
Inspired directly by **Contra (`10`) + Linear (`02`) + Luma (`03`)**:
- **Top Control Bar of Dock:**
  - Left: Segmented Switcher (`Projects & Squads` · `Student Clubs` · `Events & QR Pass` · `Top Campuses`).
  - Right: Live **University Campus Strip** displaying official database emblems from `/api/v1/universities/catalog` (`Cairo`, `Ain Shams`, `Alexandria`, `Capital`, `GUC`, `AUC`) with total count (`"12 Partner Campuses"`).
- **Skill / Track Sub-Filter Row:**
  - Interactive discipline chips (`All`, `AI & Data`, `Full-Stack`, `Embedded & IoT`, `UI/UX`, `Robotics`). Selecting a chip filters the visible preview cards smoothly in zero latency.
- **3–4 Live Interactive Preview Cards (No Page Reload Required):**
  - **In `Projects & Squads` mode:** Shows active faculty project cards (*e.g., "Smart Campus IoT Grid — Engineering · 3/5 Joined · +250 XP"*, *"AI Arabic Tutor — FCAI · 4/6 Joined · React + Python"*, *"FinTech Campus Wallet — Business & CS"*), each carrying a real **official university logo** from DB, collaborator initials stack, needed skill tag (`UI/UX Needed`), and CTA linking directly to `/projects`.
  - **In `Student Clubs` mode:** Shows active organizations (`IEEE Computer Society`, `GDG Campus`, `Enactus`, `Robotics & AI Lab`) with recruitment status badge (`Recruiting Open`) and CTA to `/organizations`.
  - **In `Events & QR Pass` mode:** Shows Luma-style workshop cards (`System Design Bootcamp`, `Cloud & AI Summit`) with real-time seat indicator (`142 / 150 seats`) and `QR Check-In · +100 XP` pill linking to `/events`.
  - **In `Top Campuses` mode:** Highlights real live universities loaded from the database with official seals, faculty codes, and quick `Join with Campus Email →` deep link.

---

## 4. Implementation & Verification Plan (When Approved)

| Step | Scope | Deliverable |
| :-: | :--- | :--- |
| **Step 1** | **New Modular Component** | Create [HeroSandboxDock.tsx](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/src/components/home/HeroSandboxDock.tsx) consuming `universitiesList` (from live `/api/v1/universities/catalog`), typed category tabs (`projects`, `clubs`, `events`, `campuses`), and discipline filter chips. |
| **Step 2** | **Upgrade `HomePage.tsx` Hero** | Wire the Quick Filter chips (`Team Formation`, `Student Activities`, `Events & Workshops`, `Skills & Leaderboards`) and manual search input directly to `<HeroSandboxDock />` state while preserving `#search-bar-container`, `#search-submit-btn`, `#quick-filters-container`, and all existing headings/navbar hooks. |
| **Step 3** | **Responsive & Motion Craft** | Ensure desktop `1440×900` fits Hero Headline + Search + Interactive Sandbox Dock cleanly above/near the fold (`overflow-x: hidden`), mobile `375×812` turns dock cards into a smooth snap-scroll strip, and `prefers-reduced-motion` disables transitions cleanly. |
| **Step 4** | **Automated & Visual QA** | Update `App.test.tsx` + `m1-auth.smoke.spec.ts` to test switching Hero Sandbox tabs (`Projects & Squads` → `Events & QR Pass` → filtering cards) + live official university logos inside the Hero, run `npm run typecheck`, `npm run test`, `npm run build`, `npm run test:e2e`, and capture `1440×900` & `375×812` hero screenshots for visual inspection. |

---

## Part B: Brand-First, Spacious Image-Led Hero Redesign (v2)

Following design review of the v1 prototype (`/demo/hero`), placing a 4-tab switcher, discipline filter chips, and dense multi-column data cards directly inside the hero made the opening viewport feel too busy and dashboard-heavy. A hero section must establish **brand emotion, spaciousness, and unmistakable visual identity** before inviting exploration.

### 5. Visual Teardown of the 8 Brand-First Hero Reference Images (`11`–`18`)

Every screenshot below was captured live at `1440×900` (skipping no obstructions—all 8 loaded cleanly without bot walls) and inspected visually:

| # | Reference File | Brand | Key Brand-First & Image-Led Strengths | Adopted Into TeamUp v2 Hero |
| :-: | :--- | :--- | :--- | :--- |
| `11` | [11-cosmos-editorial-collage.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/11-cosmos-editorial-collage.png) | **Cosmos** (`cosmos.so`) | Over 50% open canvas, expressive centered display headline, floating slightly-tilted editorial photography tiles orbiting the perimeter, and one effortless white pill search input. Zero dashboard clutter. | **Structural archetype for Concept A (`Editorial Photo Orbit`):** 4 slightly-tilted campus photography frames flanking a generous central brand stage + clean white search pill. |
| `12` | [12-patreon-creator-identity.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/12-patreon-creator-identity.png) | **Patreon** (`patreon.com`) | Unapologetic oversized editorial display typography layered against candid, documentary human collaboration photography and a single small creator attribution pill. | Real, unpolished-feeling 35mm documentary university student photography as the emotional core of the brand. |
| `13` | [13-wise-green-editorial.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/13-wise-green-editorial.png) | **Wise** (`wise.com`) | Bold condensed uppercase green/dark brand authority, single focused primary CTA button, and zero micro-widget noise above the main statement. | Tight condensed `Antonio` headline proportion + high-contrast Handshake Rosetta palette (`#14151c`, `#d3fb52`, `#ffffff`). |
| `14` | [14-pitch-visual-storytelling.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/14-pitch-visual-storytelling.png) | **Pitch** (`pitch.com`) | Subtle visual cards recessed peacefully behind giant display copy (`Create slides that win.`) with one clean central prompt box. | Keeping peripheral imagery calm and supportive so the headline and single action command immediate focus. |
| `15` | [15-framer-spatial-brand.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/15-framer-spatial-brand.png) | **Framer** (`framer.com`) | Monumental left-aligned headline, 1-line subtitle, single high-contrast CTA pill (`Get started for free`), and generous dark canvas margin. | Asymmetric left-aligned brand authority spacing in **Concept B (`Magazine Split`)**. |
| `16` | [16-notion-minimal-story.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/16-notion-minimal-story.png) | **Notion** (`notion.com`) | Centered 2-line typographic lockup, 1-line pitch, uncluttered dual pills, and open breathing space framing one hero visual below. | Restraint rule: maximum one subtitle sentence and zero badge soup above or below the action bar. |
| `17` | [17-layers-design-community.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/17-layers-design-community.png) | **Layers** (`layers.to`) | Visual-first mosaic where craft and community photography speak instantly without verbose text summaries. | Short 3-to-4 word micro-captions paired with real database university crests—never multi-line card paragraphs. |
| `18` | [18-apple-education-editorial.png](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/18-apple-education-editorial.png) | **Apple Education** (`apple.com/education/`) | Warm documentary photography of students and mentor collaborating closely over a laptop, paired with razor-sharp stacked typography. | Authentic student collaboration scenes ([hero-squad-lab.jpg](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/public/images/hero/hero-squad-lab.jpg), [hero-workshop-hall.jpg](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/public/images/hero/hero-workshop-hall.jpg)) as visual proof of campus teamwork. |

---

### 6. v2 Brand-First Visual Storytelling Rules

1. **45%–55% Intentional Negative Space:** Remove the 4-tab sandbox bar, 6-pill track filter row, and 3-column data card grid from the hero viewport. Interactive discovery lives below the fold or on `/projects`.
2. **Monumental Headline + 1 Sentence + 1 Focal Action:** Let display type (`Antonio`), concise copy, and photography breathe without promo pills (`NEW v2`) floating above `<h1>`.
3. **Editorial Campus Photography + Official DB Emblems:** Source 4 cohesive 35mm documentary-style campus collaboration images in `frontend/public/images/hero/`, each anchored by a clean 3–4 word micro-caption and a verified official `<UniversityLogo />` loaded from `/api/v1/universities/catalog` (including **Cairo University**, **Ain Shams University**, **Alexandria University**, **Capital University**).
4. **Two Spacious Concepts Previewable at `/demo/hero`:**
   - **Concept A — `Editorial Photo Orbit` (Cosmos + Luma + Handshake):** Non-overlapping 3-column desktop architectural CSS Grid (`[260px wing · minmax(0,660px) center · 260px wing]` at `xl:`), placing two floating tilted editorial photo cards on each side around the spacious centered headline and crisp white rounded search pill (`max-w-[540px]`), plus a clean 2-column photo grid on mobile.
   - **Concept B — `Magazine Split` (Handshake + Wise + Patreon):** Clean `6 / 6` asymmetric split on `lg:` pairing monumental left-aligned typography and `Join Your Campus` / `Explore Squads` pills with a spacious 3-photo asymmetric Bento collage (`Cairo · FCAI`, `Capital · Workshop Pass`, `Ain Shams · Student Clubs`) on the right.

---

## Part C: MindSpring × Editorial Photo Orbit Elevation (v3)

Building on user approval of **Editorial Photo Orbit** plus direct visual direction from [19-mindspring-student-stage.webp](file:///C:/Users/ahmed/Downloads/TeamUp/docs/inspiration/hero-sections/19-mindspring-student-stage.webp), v3 blends Cosmos's spacious orbit composition with MindSpring's dark velvety obsidian craft:

### 7. Design Upgrades Extracted from MindSpring (`19`)

1. **Velvety Obsidian Canvas & Micro-Grid (`.teamup-mindspring-bg`):** Dark `#0f1216` ground lit subtly from left emerald (`rgba(36, 134, 96, 0.24)`) and lower-right warm amber (`rgba(226, 142, 68, 0.14)`), layered over a whisper-quiet `48px` architectural grid plus low-opacity (`8%`) inline academic line sketches (graduation cap, open textbook, rocket, notebook) floating unobtrusively in open background margins.
2. **Luminous Gradient Headline:** `Plus Jakarta Sans` extrabold headline transitioning smoothly across warm cream-gold and Handshake electric lime (`#d3fb52` / `#f4b86b`) with generous line leading (`1.03–1.04`).
3. **MindSpring Circular Emblem Cluster (`<CampusSocialProof />`):** Compact micro-label (`12+ Egyptian Partner Campuses`) above overlapping circular white-ringed official database university seals ending in an electric-lime `+{N}` counter pill.
4. **Vibrant Offset-Wireframe Portrait Framing:** Every tilted photo card (in **Editorial Photo Orbit** and in **MindSpring Campus Stage**) pairs a bold accent rim/studio block (`#ef7336` terracotta, `#3fc970` campus green, `#53d3df` turquoise cyan, `#d3fb52` lime) with a crisp offset white wireframe outline shifted behind the card.
5. **Dual v3 Showcase Concepts at `/demo/hero`:**
   - **Concept A — `Editorial Photo Orbit` (default):** Elevated 3-column Cosmos orbit powered by our 4 documentary campus collaboration photographs framed in offset white wireframes + vibrant pillar accents around the MindSpring luminous headline and white pill search bar.
   - **Concept B — `MindSpring Campus Stage`:** Direct tribute featuring the centered gradient headline, `Get Started` / `Explore Squads` pills flanked cleanly by `<CampusSocialProof />`, and a 3-card lower stage showcasing three bespoke vibrant studio portraits ([hero-student-orange.jpg](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/public/images/hero/hero-student-orange.jpg), [hero-student-green.jpg](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/public/images/hero/hero-student-green.jpg), [hero-student-cyan.jpg](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/public/images/hero/hero-student-cyan.jpg)) paired with Cairo, Capital, and Ain Shams official DB emblems.

---

## Part D: Handshake Rosetta-Locked Elevation (v4)

Per explicit design direction, v4 removes the off-system v3 font/palette changes (`Plus Jakarta Sans` gradient headline, obsidian grid `.teamup-mindspring-bg`, orange/cyan cards) and locks 100% back onto **TeamUp's Handshake Rosetta design system**, keeping **only the joyful student portrait cards and campus hero line-art icons**:

1. **Pure Rosetta Typography & Canvas:** Restores `var(--font-display)` (`'Antonio', 'Barlow Condensed', sans-serif`) bold uppercase pure white display type (`LET'S FIND / YOUR NEXT TEAM`), `var(--font-sans)` (`DM Sans`) body copy, `.teamup-hero-bg` deep campus emerald-to-obsidian radial mesh, and `#d3fb52` electric-lime primary accents.
2. **Rosetta-Harmonized Student Portraits:** Combines emerald ([hero-student-green.jpg](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/public/images/hero/hero-student-green.jpg)), campus teal ([hero-student-teal.jpg](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/public/images/hero/hero-student-teal.jpg)), and lime-sage ([hero-student-lime.jpg](file:///C:/Users/ahmed/Downloads/TeamUp/frontend/public/images/hero/hero-student-lime.jpg)) portraits with crisp offset white wireframe borders and full-bleed edge-to-edge card treatment (eliminating heavy dark card bezels).
3. **Hero Campus Iconography (`HeroCampusIcons` + Micro-Icons):** Places 6 subtle thin-stroke academic SVG line icons across background negative space plus crisp `#d3fb52` Lucide icons (`Users`, `Sparkles`, `CalendarCheck`, `Award`) inside every floating glass caption pill and circular DB university seal cluster (`+6`).



