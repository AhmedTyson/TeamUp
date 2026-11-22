/**
 * @date 2026-11-22
 * @purpose Editorial split Events, Workshops & QR Attendance verification page featuring crisp white Handshake event cards and live QRCodeSVG session passes.
 */
import { Calendar, CheckCircle2, MapPin, QrCode } from "lucide-react";
import { QRCodeSVG } from "qrcode.react";
import { useState } from "react";
import { Badge, SearchInput, UniversityLogo } from "@/components/ui";
import { useUniversityCatalog } from "@/features/catalog/useUniversityCatalogFallback";

export interface EventsHubPageProps {
  mode?: "events" | "detail" | "attendance" | "check-in";
}

interface CampusEventItem {
  id: string;
  title: string;
  organizer: string;
  university: string;
  venue: string;
  dateLabel: string;
  registered: number;
  capacity: number;
  xpReward: number;
  sessionCode: string;
}

const CAMPUS_EVENTS: readonly CampusEventItem[] = [
  {
    id: "sys-design-bootcamp",
    title: "System Design & Distributed Databases Bootcamp",
    organizer: "IEEE Cairo Student Branch",
    university: "Cairo University",
    venue: "FCAI Main Auditorium · Hall B-204",
    dateLabel: "Thu · 2:00 PM · 2 Sessions",
    registered: 142,
    capacity: 150,
    xpReward: 100,
    sessionCode: "CU-SYS-104",
  },
  {
    id: "llm-rag-workshop",
    title: "Applied LLMs & Bilingual RAG Architecture Lab",
    organizer: "GDG on Campus Ain Shams",
    university: "Ain Shams University",
    venue: "CIS Innovation Lab 3",
    dateLabel: "Sat · 11:00 AM · Hands-On",
    registered: 94,
    capacity: 100,
    xpReward: 100,
    sessionCode: "ASU-AI-208",
  },
  {
    id: "embedded-iot-day",
    title: "Embedded IoT & Real-Time Sensor Telemetry Day",
    organizer: "Capital Robotics Lab",
    university: "Capital University",
    venue: "Engineering Hall 4",
    dateLabel: "Mon · 1:30 PM · Hardware Kit",
    registered: 58,
    capacity: 75,
    xpReward: 120,
    sessionCode: "CAPU-IOT-312",
  },
  {
    id: "icpc-warmups",
    title: "Algorithmic Graph Theory & Contest Mock",
    organizer: "ACM Alexandria Chapter",
    university: "Alexandria University",
    venue: "Computing Lab A · Ground Floor",
    dateLabel: "Wed · 4:00 PM · Timed Contest",
    registered: 112,
    capacity: 120,
    xpReward: 110,
    sessionCode: "ALEX-ACM-401",
  },
];

export function EventsHubPage({ mode = "events" }: EventsHubPageProps) {
  const { resolveLogo } = useUniversityCatalog();
  const [search, setSearch] = useState("");
  const [activeQrId, setActiveQrId] = useState<string | null>(
    mode === "attendance" || mode === "check-in" ? CAMPUS_EVENTS[0].id : null,
  );
  const [checkedInPin, setCheckedInPin] = useState("");
  const [pinConfirmed, setPinConfirmed] = useState(false);

  const q = search.trim().toLowerCase();
  const filtered = CAMPUS_EVENTS.filter(
    (ev) =>
      !q ||
      ev.title.toLowerCase().includes(q) ||
      ev.university.toLowerCase().includes(q) ||
      ev.organizer.toLowerCase().includes(q),
  );

  return (
    <div className="space-y-8">
      {/* Magazine Split Editorial Hero with Auditorium Photography */}
      <section className="grid grid-cols-1 items-center gap-8 overflow-hidden rounded-[28px] border border-white/12 bg-[#1b1d26] p-5 shadow-2xl sm:p-9 lg:grid-cols-12">
        <div className="min-w-0 space-y-4 lg:col-span-6">
          <Badge variant="accent" size="sm">
            {mode === "check-in" || mode === "attendance"
              ? "Verified Session Check-In"
              : "Workshops & Campus Summits"}
          </Badge>

          <h1
            className="font-bold uppercase text-white"
            style={{
              fontFamily: "var(--font-display)",
              fontSize: "clamp(2.25rem, 5.5vw, 4.4rem)",
              lineHeight: 0.9,
              letterSpacing: "-0.025em",
            }}
          >
            WORKSHOPS, HACKATHONS
            <br />
            &amp; LIVE QR PASS
          </h1>

          <p className="max-w-lg text-sm leading-relaxed text-white/80 sm:text-base">
            Reserve technical sessions across partner universities, scan in via verified QR or
            60-second PIN, and earn participation XP straight to your student portfolio.
          </p>

          {(mode === "check-in" || mode === "attendance") && (
            <form
              onSubmit={(e) => {
                e.preventDefault();
                if (checkedInPin.trim().length >= 4) {
                  setPinConfirmed(true);
                }
              }}
              className="flex flex-wrap items-center gap-2 pt-2"
            >
              <input
                type="text"
                aria-label="Session PIN code"
                value={checkedInPin}
                onChange={(e) => {
                  setCheckedInPin(e.target.value);
                  setPinConfirmed(false);
                }}
                placeholder="Enter session code (e.g. CU-SYS-104)"
                className="h-10 w-full min-w-0 flex-1 rounded-xl border border-white/15 bg-[#14151c] px-3.5 text-sm text-white placeholder:text-white/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:w-auto sm:min-w-[220px]"
              />
              <button
                type="submit"
                className="inline-flex h-10 cursor-pointer items-center gap-1.5 rounded-xl bg-[#d3fb52] px-4 text-xs font-bold text-[#14151c] transition-colors hover:bg-[#deff6e]"
              >
                <QrCode className="size-4" aria-hidden="true" />
                Verify Check-In
              </button>
              {pinConfirmed && (
                <p className="inline-flex w-full items-center gap-1.5 text-xs font-semibold text-emerald-300">
                  <CheckCircle2 className="size-4" aria-hidden="true" />
                  Attendance code validated (+100 XP ready on active enrollment).
                </p>
              )}
            </form>
          )}
        </div>

        <div className="relative min-w-0 lg:col-span-6">
          <div
            aria-hidden="true"
            className="pointer-events-none absolute -inset-1 translate-x-1.5 -translate-y-1.5 rounded-[24px] border-[1.5px] border-white/55 sm:translate-x-2 sm:-translate-y-2"
          />
          <div className="relative overflow-hidden rounded-[22px] border border-white/20 bg-[#14151c] shadow-2xl">
            <img
              src="/images/hero/hero-workshop-hall.jpg"
              alt="Egyptian university students seated with laptops in a technical workshop hall"
              loading="eager"
              decoding="async"
              className="aspect-[16/10] w-full object-cover object-center"
            />
          </div>
        </div>
      </section>

      {/* Crisp High-Contrast White Handshake Event Cards */}
      <section className="space-y-6 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-4 sm:p-7">
        <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
          <div>
            <h2
              className="text-xl font-bold text-white"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              Upcoming Campus Sessions
            </h2>
            <p className="text-xs text-[#9ca3af]">
              Inspect real workshop seat availability and toggle an instant sample QR pass.
            </p>
          </div>
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder="Filter by event, organizer, or university..."
            containerClassName="w-full sm:w-80"
          />
        </div>

        <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
          {filtered.map((ev) => {
            const pct = Math.min(100, Math.round((ev.registered / ev.capacity) * 100));
            const qrOpen = activeQrId === ev.id;

            return (
              <article
                key={ev.id}
                className="hs-white-card flex min-w-0 flex-col justify-between gap-4 p-4 transition-transform duration-200 hover:-translate-y-0.5 sm:p-6"
              >
                <div className="space-y-3">
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex min-w-0 items-center gap-2.5">
                      <UniversityLogo
                        name={ev.university}
                        logoUrl={resolveLogo(ev.university)}
                        size="md"
                      />
                      <div className="min-w-0">
                        <p className="truncate text-xs font-bold text-[#14151c]">{ev.organizer}</p>
                        <p className="truncate text-[11px] text-[#4b5563]">{ev.university}</p>
                      </div>
                    </div>
                    <span className="shrink-0 rounded-full bg-[#14151c] px-3 py-1 text-[11px] font-bold text-[#d3fb52]">
                      +{ev.xpReward} XP
                    </span>
                  </div>

                  <h3
                    className="text-lg font-bold leading-snug text-[#14151c]"
                    style={{ fontFamily: "var(--font-heading)" }}
                  >
                    {ev.title}
                  </h3>

                  <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-[#4b5563]">
                    <span className="inline-flex items-center gap-1">
                      <Calendar className="size-3.5 text-[#15733f]" aria-hidden="true" />
                      {ev.dateLabel}
                    </span>
                    <span className="inline-flex items-center gap-1">
                      <MapPin className="size-3.5 text-[#15733f]" aria-hidden="true" />
                      {ev.venue}
                    </span>
                  </div>

                  <div className="space-y-1 pt-1">
                    <div className="flex items-center justify-between text-[11px] font-semibold text-[#14151c]">
                      <span>Seat capacity</span>
                      <span className="tabular-nums">
                        {ev.registered}/{ev.capacity} registered
                      </span>
                    </div>
                    <div className="h-2 overflow-hidden rounded-full bg-black/10">
                      <div
                        className="h-full rounded-full bg-[#15733f]"
                        style={{ width: `${pct}%` }}
                      />
                    </div>
                  </div>
                </div>

                {qrOpen && (
                  <div className="flex flex-col items-start gap-3 rounded-2xl border border-black/10 bg-[#f3f6f4] p-3.5 sm:flex-row sm:items-center sm:gap-4">
                    <div className="shrink-0 rounded-xl bg-white p-2 shadow-sm">
                      <QRCodeSVG value={`teamup:checkin:${ev.sessionCode}`} size={72} />
                    </div>
                    <div className="min-w-0 space-y-1">
                      <p className="text-xs font-bold text-[#14151c]">Live Attendance Pass</p>
                      <p className="font-mono text-xs font-semibold text-[#15733f]">
                        {ev.sessionCode}
                      </p>
                      <p className="text-[11px] text-[#4b5563]">
                        Show this QR at venue entrance to record check-in automatically.
                      </p>
                    </div>
                  </div>
                )}

                <div className="flex flex-wrap items-center justify-between gap-2 border-t border-black/10 pt-3 sm:gap-3">
                  <span className="font-mono text-xs font-semibold text-[#4b5563]">
                    Session {ev.sessionCode}
                  </span>
                  <button
                    type="button"
                    onClick={() => setActiveQrId((prev) => (prev === ev.id ? null : ev.id))}
                    className="inline-flex cursor-pointer items-center gap-1.5 rounded-xl bg-[#14151c] px-4 py-2 text-xs font-bold text-[#d3fb52] transition-colors hover:bg-[#222532]"
                  >
                    <QrCode className="size-3.5" aria-hidden="true" />
                    <span>{qrOpen ? "Hide QR Pass" : "Show QR Pass"}</span>
                  </button>
                </div>
              </article>
            );
          })}
        </div>
      </section>
    </div>
  );
}
