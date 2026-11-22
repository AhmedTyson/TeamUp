/**
 * @date 2026-11-22
 * @purpose Handshake Employers-inspired Projects Squads & Skills Matrix hub featuring two dark architectural action cards, live search, track filters, pagination, and DB university emblems.
 */
import { ArrowRight, Code2, Cpu, Layers, PlusCircle, Rocket, Sparkles, Users } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { Badge, Pagination, SearchInput, UniversityLogo } from "@/components/ui";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";
import { cn } from "@/lib/cn";

export interface ProjectsHubPageProps {
  mode?: "projects" | "create" | "detail" | "skills";
}

type TrackFilter = "All Tracks" | "AI & Data" | "Full-Stack" | "Embedded & IoT" | "Robotics";

interface CampusSquadItem {
  id: string;
  title: string;
  summary: string;
  imageSrc: string;
  university: string;
  facultyCode: string;
  track: Exclude<TrackFilter, "All Tracks">;
  neededRole: string;
  skills: readonly string[];
  seatsJoined: number;
  seatsTotal: number;
  xpReward: number;
  sprintStage: string;
}

interface SkillCatalogEntry {
  id: string;
  name: string;
  track: Exclude<TrackFilter, "All Tracks">;
  demandLabel: string;
  openSquads: number;
  verifiedBuilders: number;
}

const TRACK_FILTERS: readonly TrackFilter[] = [
  "All Tracks",
  "AI & Data",
  "Full-Stack",
  "Embedded & IoT",
  "Robotics",
];

const CAMPUS_SQUADS: readonly CampusSquadItem[] = [
  {
    id: "ai-study-companion",
    title: "Bilingual Arabic & STEM RAG Study Assistant",
    summary: "Building retrieval-augmented lecture search and summarization tailored for FCAI & Engineering courses.",
    imageSrc: "/images/hero/hero-squad-lab.jpg",
    university: "Cairo University",
    facultyCode: "FCAI",
    track: "AI & Data",
    neededRole: "UI/UX Lead",
    skills: ["Python", "React", "TypeScript", "FastAPI"],
    seatsJoined: 4,
    seatsTotal: 6,
    xpReward: 250,
    sprintStage: "Week 3 · Retrieval Eval",
  },
  {
    id: "smart-campus-microgrid",
    title: "Campus Microgrid Energy Monitor & Telemetry",
    summary: "Real-time ESP32 & STM32 substation load monitoring with clean WebSocket telemetry dashboard.",
    imageSrc: "/images/hero/hero-demo-day.jpg",
    university: "Capital University",
    facultyCode: "ENG_CAPU",
    track: "Embedded & IoT",
    neededRole: "Firmware Engineer",
    skills: ["C++", "STM32", "MQTT", "React"],
    seatsJoined: 3,
    seatsTotal: 5,
    xpReward: 250,
    sprintStage: "Week 2 · PCB Integration",
  },
  {
    id: "cross-faculty-portfolio",
    title: "Verified Cross-Faculty Team Portfolio Hub",
    summary: "Full-stack portal connecting CS, Business, and Architecture students for graduation sprints.",
    imageSrc: "/images/hero/hero-club-meetup.jpg",
    university: "Ain Shams University",
    facultyCode: "CIS",
    track: "Full-Stack",
    neededRole: "Backend & API",
    skills: ["ASP.NET Core", "React", "PostgreSQL"],
    seatsJoined: 4,
    seatsTotal: 5,
    xpReward: 250,
    sprintStage: "Sprint Review Ready",
  },
  {
    id: "autonomous-rover",
    title: "Autonomous Competition Rover Vision Pipeline",
    summary: "Stereo depth obstacle avoidance and path planning stack for the national robotics challenge.",
    imageSrc: "/images/hero/hero-workshop-hall.jpg",
    university: "Alexandria University",
    facultyCode: "ENG_ALEX",
    track: "Robotics",
    neededRole: "ROS2 Specialist",
    skills: ["ROS2", "OpenCV", "Python", "C++"],
    seatsJoined: 5,
    seatsTotal: 7,
    xpReward: 300,
    sprintStage: "SLAM Field Calibration",
  },
  {
    id: "transit-route-optimizer",
    title: "Greater Cairo Student Commute Carpool Planner",
    summary: "Graph routing engine matching campus shuttle times and peer carpool slots cleanly.",
    imageSrc: "/images/hero/hero-club-meetup.jpg",
    university: "German University in Cairo (GUC)",
    facultyCode: "MET_GUC",
    track: "Full-Stack",
    neededRole: "Mobile Frontend",
    skills: ["TypeScript", "React", "Go", "PostGIS"],
    seatsJoined: 3,
    seatsTotal: 5,
    xpReward: 200,
    sprintStage: "Week 1 · API Contracts",
  },
  {
    id: "medical-triage-nlp",
    title: "Clinical Notes Extraction & Triage Assistant",
    summary: "Structured bilingual medical entity extraction benchmarked with university hospital datasets.",
    imageSrc: "/images/hero/hero-squad-lab.jpg",
    university: "Mansoura University",
    facultyCode: "FCI_MANS",
    track: "AI & Data",
    neededRole: "ML Pipeline",
    skills: ["PyTorch", "Transformers", "Python"],
    seatsJoined: 2,
    seatsTotal: 4,
    xpReward: 250,
    sprintStage: "Dataset Annotation",
  },
  {
    id: "drone-swarm-relay",
    title: "Search & Rescue Drone Mesh Telemetry Unit",
    summary: "Low-latency LoRa relay packets with automated waypoint handoff for field competitions.",
    imageSrc: "/images/hero/hero-demo-day.jpg",
    university: "Assiut University",
    facultyCode: "FCI_AUN",
    track: "Robotics",
    neededRole: "RF & Embedded",
    skills: ["C", "FreeRTOS", "LoRaWAN"],
    seatsJoined: 4,
    seatsTotal: 6,
    xpReward: 280,
    sprintStage: "Flight Test Prep",
  },
];

const SKILLS_CATALOG: readonly SkillCatalogEntry[] = [
  { id: "react-ts", name: "React & TypeScript", track: "Full-Stack", demandLabel: "High Demand", openSquads: 18, verifiedBuilders: 340 },
  { id: "dotnet-api", name: "ASP.NET Core & SQL", track: "Full-Stack", demandLabel: "High Demand", openSquads: 14, verifiedBuilders: 215 },
  { id: "pytorch-rag", name: "PyTorch & RAG Pipelines", track: "AI & Data", demandLabel: "Trending", openSquads: 16, verifiedBuilders: 260 },
  { id: "stm32-iot", name: "STM32, ESP32 & MQTT", track: "Embedded & IoT", demandLabel: "Active", openSquads: 11, verifiedBuilders: 148 },
  { id: "ros2-cv", name: "ROS2 & Computer Vision", track: "Robotics", demandLabel: "High Demand", openSquads: 9, verifiedBuilders: 122 },
  { id: "uiux-figma", name: "Figma & Design Systems", track: "Full-Stack", demandLabel: "Needed Across Teams", openSquads: 21, verifiedBuilders: 195 },
  { id: "data-eng", name: "Data Modeling & BigQuery", track: "AI & Data", demandLabel: "Active", openSquads: 10, verifiedBuilders: 164 },
  { id: "pcb-cad", name: "PCB Design & SolidWorks", track: "Embedded & IoT", demandLabel: "Hardware Core", openSquads: 8, verifiedBuilders: 112 },
];

export function ProjectsHubPage({ mode = "projects" }: ProjectsHubPageProps) {
  const { universities, resolveLogo } = useUniversityCatalog();
  const [search, setSearch] = useState("");
  const [track, setTrack] = useState<TrackFilter>("All Tracks");
  const [page, setPage] = useState(1);
  const pageSize = 6;

  const isSkillsMode = mode === "skills";
  const q = search.trim().toLowerCase();

  const filteredSquads = CAMPUS_SQUADS.filter((item) => {
    const matchesTrack = track === "All Tracks" || item.track === track;
    const matchesQ =
      !q ||
      item.title.toLowerCase().includes(q) ||
      item.university.toLowerCase().includes(q) ||
      item.neededRole.toLowerCase().includes(q) ||
      item.skills.some((s) => s.toLowerCase().includes(q));
    return matchesTrack && matchesQ;
  });

  const filteredSkills = SKILLS_CATALOG.filter((item) => {
    const matchesTrack = track === "All Tracks" || item.track === track;
    const matchesQ = !q || item.name.toLowerCase().includes(q) || item.track.toLowerCase().includes(q);
    return matchesTrack && matchesQ;
  });

  const totalCount = isSkillsMode ? filteredSkills.length : filteredSquads.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const safePage = Math.min(page, totalPages);
  const pagedSquads = filteredSquads.slice((safePage - 1) * pageSize, safePage * pageSize);

  return (
    <div className="space-y-8">
      {/* Handshake Employers-Archetype Radial Stage Banner */}
      <section className="teamup-hero-bg overflow-hidden rounded-[28px] border border-white/12 px-6 py-10 text-center shadow-2xl sm:px-10 sm:py-14">
        <div className="mx-auto max-w-[820px]">
          <p className="inline-flex items-center gap-1.5 rounded-full border border-white/20 bg-black/25 px-3.5 py-1 text-xs font-semibold text-white/90">
            <Rocket className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
            <span>Across {universities.length} Egyptian partner campuses</span>
          </p>

          <h1
            className="mt-4 font-bold uppercase text-white"
            style={{
              fontFamily: "var(--font-display)",
              fontSize: "clamp(2.5rem, 6vw, 4.75rem)",
              lineHeight: 0.92,
              letterSpacing: "-0.025em",
            }}
          >
            {isSkillsMode
              ? "SKILLS MATRIX & TRACKS"
              : mode === "create"
                ? "LAUNCH A CAMPUS SQUAD"
                : "BUILD SQUADS THAT SHIP"}
          </h1>

          <p className="mx-auto mt-3 max-w-[600px] text-sm leading-relaxed text-white/85 sm:text-base">
            {isSkillsMode
              ? "Verify your technical stack, browse high-demand roles across faculties, and match straight into active capstone projects."
              : "Publish your project idea, define needed engineering or design skills, and assemble cross-faculty teammates from Year One."}
          </p>

          {/* Dual Architectural Action Cards (Handshake /employers/ rhythm) */}
          <div className="mx-auto mt-8 grid max-w-[780px] gap-4 text-left sm:grid-cols-2">
            <div className="flex flex-col justify-between rounded-2xl border border-white/12 bg-[#14151c]/82 p-5 backdrop-blur-md">
              <div>
                <div className="flex items-center justify-between gap-2">
                  <h2
                    className="text-lg font-bold text-white"
                    style={{ fontFamily: "var(--font-heading)" }}
                  >
                    {isSkillsMode ? "Find Open Squads" : "Post a New Squad"}
                  </h2>
                  <PlusCircle className="size-5 text-[#d3fb52]" aria-hidden="true" />
                </div>
                <p className="mt-1.5 text-xs leading-relaxed text-white/70">
                  {isSkillsMode
                    ? "Apply your verified skills directly to live faculty projects and graduation squads."
                    : "Create a project brief, attach required skills, and start reviewing applications right away."}
                </p>
              </div>
              <Link
                to={isSkillsMode ? "/projects" : "/projects/new"}
                className="mt-4 inline-flex w-fit items-center gap-1.5 rounded-xl border border-white/25 bg-white/10 px-4 py-2 text-xs font-semibold text-white transition-colors hover:border-[#d3fb52] hover:text-[#d3fb52]"
              >
                <span>{isSkillsMode ? "Browse Projects" : "Start Squad Draft"}</span>
                <ArrowRight className="size-3.5" aria-hidden="true" />
              </Link>
            </div>

            <div className="flex flex-col justify-between rounded-2xl border border-white/12 bg-[#14151c]/82 p-5 backdrop-blur-md">
              <div>
                <div className="flex items-center justify-between gap-2">
                  <h2
                    className="text-lg font-bold text-white"
                    style={{ fontFamily: "var(--font-heading)" }}
                  >
                    {isSkillsMode ? "Claim Profile Skills" : "Match by Verified Skill"}
                  </h2>
                  <Code2 className="size-5 text-[#d3fb52]" aria-hidden="true" />
                </div>
                <p className="mt-1.5 text-xs leading-relaxed text-white/70">
                  Discover which React, AI, Embedded, or UI/UX roles campuses need most this semester.
                </p>
              </div>
              <Link
                to={isSkillsMode ? "/students" : "/skills"}
                className="mt-4 inline-flex w-fit items-center gap-1.5 rounded-xl bg-[#d3fb52] px-4 py-2 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
              >
                <span>{isSkillsMode ? "Explore Builders" : "Explore Skills Matrix"}</span>
                <ArrowRight className="size-3.5" aria-hidden="true" />
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Filter & Search Bar */}
      <section className="space-y-6 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-5 sm:p-7">
        <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-center">
          <div className="flex flex-wrap items-center gap-2">
            {TRACK_FILTERS.map((item) => {
              const active = track === item;
              return (
                <button
                  key={item}
                  type="button"
                  onClick={() => {
                    setTrack(item);
                    setPage(1);
                  }}
                  className={cn(
                    "cursor-pointer rounded-full px-3.5 py-1.5 text-xs font-semibold transition-colors",
                    active
                      ? "bg-[#d3fb52] text-[#14151c]"
                      : "border border-white/12 bg-[#14151c] text-white/75 hover:border-white/30 hover:text-white",
                  )}
                >
                  {item}
                </button>
              );
            })}
          </div>

          <SearchInput
            value={search}
            onChange={(val) => {
              setSearch(val);
              setPage(1);
            }}
            placeholder={
              isSkillsMode
                ? "Search React, Python, ROS2, UI/UX..."
                : "Search projects, skills, or university..."
            }
            containerClassName="w-full lg:w-80"
          />
        </div>

        {isSkillsMode ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {filteredSkills.map((skill) => (
              <div
                key={skill.id}
                className="flex flex-col justify-between gap-4 rounded-2xl border border-white/[0.085] bg-[#14151c] p-5 transition-colors hover:border-[#d3fb52]/40"
              >
                <div className="space-y-2.5">
                  <div className="flex items-center justify-between gap-2">
                    <span className="inline-flex size-9 items-center justify-center rounded-xl bg-[#d3fb52]/15 text-[#d3fb52]">
                      <Cpu className="size-4" aria-hidden="true" />
                    </span>
                    <Badge variant="success" size="sm">
                      {skill.demandLabel}
                    </Badge>
                  </div>
                  <h3
                    className="text-base font-bold text-white"
                    style={{ fontFamily: "var(--font-heading)" }}
                  >
                    {skill.name}
                  </h3>
                  <p className="text-xs text-[#9ca3af]">{skill.track}</p>
                </div>

                <div className="flex items-center justify-between border-t border-white/[0.08] pt-3 text-xs text-white/75">
                  <span>{skill.openSquads} open squads</span>
                  <span className="font-semibold text-[#d3fb52]">{skill.verifiedBuilders} students</span>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
            {pagedSquads.map((squad) => (
              <article
                key={squad.id}
                className="flex flex-col justify-between overflow-hidden rounded-2xl border border-white/[0.085] bg-[#14151c] transition-all duration-200 hover:-translate-y-0.5 hover:border-[#d3fb52]/45"
              >
                <div className="relative h-32 overflow-hidden bg-[#181a23]">
                  <img
                    src={squad.imageSrc}
                    alt={`${squad.title} campus team photo`}
                    loading="lazy"
                    className="h-full w-full object-cover object-center opacity-85"
                  />
                  <div
                    aria-hidden="true"
                    className="absolute inset-0 bg-gradient-to-t from-[#14151c] via-[#14151c]/35 to-transparent"
                  />
                  <div className="absolute inset-x-3.5 bottom-2.5 flex items-center justify-between gap-2">
                    <span className="rounded-full bg-[#d3fb52] px-2.5 py-0.5 text-[11px] font-bold text-[#14151c]">
                      {squad.neededRole} Needed
                    </span>
                    <span className="font-mono text-[11px] font-semibold text-white/90">
                      +{squad.xpReward} XP
                    </span>
                  </div>
                </div>

                <div className="flex flex-1 flex-col justify-between space-y-4 p-5">
                  <div className="space-y-2.5">
                    <div className="flex items-center gap-2.5">
                      <UniversityLogo
                        name={squad.university}
                        logoUrl={resolveLogo(squad.university)}
                        size="sm"
                      />
                      <div className="min-w-0">
                        <p className="truncate text-xs font-semibold text-white">
                          {squad.university}
                        </p>
                        <p className="font-mono text-[11px] text-[#9ca3af]">
                          {squad.facultyCode} · {squad.sprintStage}
                        </p>
                      </div>
                    </div>

                    <h3
                      className="text-base font-bold leading-snug text-white"
                      style={{ fontFamily: "var(--font-heading)" }}
                    >
                      {squad.title}
                    </h3>
                    <p className="text-xs leading-relaxed text-[#9ca3af]">{squad.summary}</p>

                    <div className="flex flex-wrap gap-1.5 pt-1">
                      {squad.skills.map((sk) => (
                        <Badge key={sk} variant="default" size="sm">
                          {sk}
                        </Badge>
                      ))}
                    </div>
                  </div>

                  <div className="flex items-center justify-between border-t border-white/[0.08] pt-3">
                    <span className="inline-flex items-center gap-1.5 text-xs text-white/75">
                      <Users className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
                      {squad.seatsJoined}/{squad.seatsTotal} seats
                    </span>
                    <Link
                      to={`/projects/${squad.id}`}
                      className="inline-flex items-center gap-1 text-xs font-semibold text-[#d3fb52] hover:underline"
                    >
                      <span>View Squad</span>
                      <Sparkles className="size-3.5" aria-hidden="true" />
                    </Link>
                  </div>
                </div>
              </article>
            ))}
          </div>
        )}

        {totalCount === 0 ? (
          <div className="rounded-2xl border border-white/10 bg-[#14151c] p-8 text-center text-sm text-[#9ca3af]">
            <Layers className="mx-auto mb-2 size-6 text-white/40" aria-hidden="true" />
            No matching squads or skills found for &ldquo;{search}&rdquo;.
          </div>
        ) : (
          totalPages > 1 && (
            <Pagination
              page={safePage}
              totalPages={totalPages}
              totalCount={totalCount}
              pageSize={pageSize}
              onPageChange={setPage}
            />
          )
        )}
      </section>
    </div>
  );
}
