/**
 * @date 2026-11-24
 * @purpose Editorial Privacy & Platform Policy page (/privacy, /policy) detailing THR-I-01 directory privacy, email controls, security architecture, campus conduct, and student rights.
 */
import {
  ArrowRight,
  Award,
  Bell,
  Database,
  EyeOff,
  FileCheck2,
  Lock,
  Mail,
  ShieldCheck,
  UserCheck,
  UserX,
} from "lucide-react";
import { Link } from "react-router-dom";
import { Badge, Button } from "@/components/ui";

interface PolicySectionNavItem {
  id: string;
  label: string;
  code: string;
}

const POLICY_SECTIONS: readonly PolicySectionNavItem[] = [
  { id: "data-collected", label: "Data & Verification", code: "01" },
  { id: "directory-privacy", label: "Directory Privacy", code: "02" },
  { id: "email-preferences", label: "Email & Digests", code: "03" },
  { id: "security-controls", label: "Security Architecture", code: "04" },
  { id: "campus-conduct", label: "Campus Code & Fair Play", code: "05" },
  { id: "rights-retention", label: "Rights & Deletion", code: "06" },
  { id: "governance-contact", label: "Governance & Contact", code: "07" },
];

export function PrivacyPolicyPage() {
  return (
    <div className="space-y-8">
      {/* Editorial Hero */}
      <section className="teamup-hero-bg overflow-hidden rounded-[28px] border border-white/12 p-6 shadow-2xl sm:p-10">
        <div className="max-w-3xl space-y-4">
          <div className="flex flex-wrap items-center gap-2.5">
            <Badge variant="accent" size="sm">
              <ShieldCheck className="mr-1 inline size-3.5" aria-hidden="true" />
              Trust, Safety &amp; Governance
            </Badge>
            <span className="font-mono text-[11px] uppercase tracking-wider text-white/65">
              Effective · November 2026
            </span>
          </div>

          <h1
            className="font-black uppercase tracking-tight text-white"
            style={{
              fontFamily: "var(--font-display)",
              fontSize: "clamp(2.3rem, 5.5vw, 4.25rem)",
              lineHeight: 0.92,
              letterSpacing: "-0.025em",
            }}
          >
            PRIVACY &amp; PLATFORM POLICY
          </h1>

          <p className="text-sm leading-relaxed text-white/80 sm:text-base">
            TeamUp is built for verified Egyptian university students and faculty communities. We
            collect only what powers academic team formation, verified event participation, and
            merit scoring—with strict privacy-by-default controls.
          </p>

          <div className="flex flex-wrap gap-2 pt-2">
            <span className="inline-flex items-center gap-1.5 rounded-full border border-[#d3fb52]/30 bg-[#d3fb52]/10 px-3 py-1 text-xs font-medium text-[#d3fb52]">
              <EyeOff className="size-3.5" aria-hidden="true" />
              Default-Private Emails
            </span>
            <span className="inline-flex items-center gap-1.5 rounded-full border border-emerald-400/30 bg-emerald-500/10 px-3 py-1 text-xs font-medium text-emerald-300">
              <Bell className="size-3.5" aria-hidden="true" />
              Granular Digest Controls
            </span>
            <span className="inline-flex items-center gap-1.5 rounded-full border border-white/15 bg-white/[0.06] px-3 py-1 text-xs font-medium text-white/85">
              <Lock className="size-3.5" aria-hidden="true" />
              Zero Ad Tracking
            </span>
            <span className="inline-flex items-center gap-1.5 rounded-full border border-white/15 bg-white/[0.06] px-3 py-1 text-xs font-medium text-white/85">
              <UserX className="size-3.5" aria-hidden="true" />
              Instant Account Soft-Delete
            </span>
          </div>
        </div>
      </section>

      {/* Main Content + Sticky Quick-Jump Sidebar */}
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-12">
        <aside className="lg:col-span-4">
          <nav
            aria-label="Privacy and policy sections"
            className="rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-4 lg:sticky lg:top-28"
          >
            <p className="mb-3 px-2 font-mono text-[11px] font-semibold uppercase tracking-wider text-[#9ca3af]">
              Policy Contents
            </p>
            <ul className="flex flex-wrap gap-2 lg:flex-col lg:gap-1">
              {POLICY_SECTIONS.map((section) => (
                <li key={section.id}>
                  <a
                    href={`#${section.id}`}
                    className="group flex items-center gap-2.5 rounded-xl px-2.5 py-2 text-xs font-medium text-white/80 transition-colors hover:bg-white/[0.05] hover:text-[#d3fb52] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
                  >
                    <span className="font-mono text-[11px] text-[#9ca3af] group-hover:text-[#d3fb52]">
                      {section.code}
                    </span>
                    <span>{section.label}</span>
                  </a>
                </li>
              ))}
            </ul>

            <div className="mt-4 hidden border-t border-white/[0.08] pt-4 lg:block">
              <p className="text-[11px] leading-relaxed text-[#9ca3af]">
                Signed-in students can update directory email visibility and digest subscriptions at
                any time.
              </p>
              <Link
                to="/profile/me"
                className="mt-2.5 inline-flex items-center gap-1 text-xs font-semibold text-[#d3fb52] hover:underline"
              >
                Open Profile Studio
                <ArrowRight className="size-3.5" aria-hidden="true" />
              </Link>
            </div>
          </nav>
        </aside>

        <div className="space-y-6 lg:col-span-8">
          {/* 01 Data Collected */}
          <section
            id="data-collected"
            className="scroll-mt-28 space-y-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6"
          >
            <div className="flex items-center gap-2.5">
              <Database className="size-5 text-[#d3fb52]" aria-hidden="true" />
              <h2
                className="text-lg font-bold tracking-tight text-white"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                01. Data We Collect &amp; Academic Verification
              </h2>
            </div>
            <p className="text-sm leading-relaxed text-white/80">
              TeamUp processes only first-party academic and collaboration records necessary to match
              students into multidisciplinary squads and verify campus merit:
            </p>
            <ul className="space-y-2 text-xs leading-relaxed text-[#9ca3af] list-disc pl-5">
              <li>
                <strong className="text-white/90">Account Identity:</strong> First name, last name,
                university email, selected university, faculty, and academic year provided at
                registration.
              </li>
              <li>
                <strong className="text-white/90">Portfolio &amp; Skills:</strong> Optional bio,
                profile photo, validated external GitHub/LinkedIn links, specialization tags, and
                declared technical skills.
              </li>
              <li>
                <strong className="text-white/90">Campus Participation &amp; Merit Ledger:</strong>{" "}
                Project applications, task assignments, organization memberships, event
                registrations, attendance check-ins, and immutable XP/badge audit ledger entries.
              </li>
              <li>
                <strong className="text-white/90">No Behavioral Ad Profiling:</strong> We never sell
                student data, embed third-party ad pixels, or share rosters with external brokers.
              </li>
            </ul>
          </section>

          {/* 02 Directory Privacy */}
          <section
            id="directory-privacy"
            className="scroll-mt-28 space-y-4 rounded-2xl border border-[#d3fb52]/25 bg-[#d3fb52]/[0.05] p-6"
          >
            <div className="flex items-center justify-between gap-2 flex-wrap">
              <div className="flex items-center gap-2.5">
                <UserCheck className="size-5 text-[#d3fb52]" aria-hidden="true" />
                <h2
                  className="text-lg font-bold tracking-tight text-white"
                  style={{ fontFamily: "var(--font-heading)" }}
                >
                  02. Student Directory Email Privacy
                </h2>
              </div>
              <Badge variant="accent" size="sm">
                Private by Default
              </Badge>
            </div>
            <p className="text-sm leading-relaxed text-white/85">
              Every newly registered account starts with{" "}
              <code className="rounded bg-[#14151c] px-1.5 py-0.5 font-mono text-xs text-[#d3fb52]">
                showEmailInDirectory = false
              </code>
              . Unless you explicitly opt in:
            </p>
            <ul className="space-y-2 text-xs leading-relaxed text-white/75 list-disc pl-5">
              <li>
                Fellow students browsing the Student Directory or viewing your Public Portfolio see{" "}
                <span className="italic text-white">&quot;Email private&quot;</span> instead of your
                email address.
              </li>
              <li>
                Directory keyword searches by email address exclude hidden accounts so external
                peers cannot enumerate emails via search probing.
              </li>
              <li>
                Checking <strong className="text-white">Show my email in student directory</strong>{" "}
                in Profile Studio immediately lets authenticated students and faculty contact you via
                your listed academic email—and unchecking it immediately re-masks your email.
              </li>
            </ul>
            <div>
              <Link
                to="/profile/me"
                className="inline-flex items-center gap-1.5 rounded-xl border border-[#d3fb52]/40 bg-[#14151c] px-3.5 py-2 text-xs font-semibold text-[#d3fb52] hover:bg-[#d3fb52] hover:text-[#14151c] transition-colors"
              >
                Manage Directory Email Opt-In
                <ArrowRight className="size-3.5" aria-hidden="true" />
              </Link>
            </div>
          </section>

          {/* 03 Email Preferences */}
          <section
            id="email-preferences"
            className="scroll-mt-28 space-y-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6"
          >
            <div className="flex items-center gap-2.5">
              <Mail className="size-5 text-[#10b981]" aria-hidden="true" />
              <h2
                className="text-lg font-bold tracking-tight text-white"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                03. Email Notifications &amp; Digest Controls
              </h2>
            </div>
            <p className="text-sm leading-relaxed text-white/80">
              During onboarding and inside Profile Studio, you control three independent account-level
              communication flags:
            </p>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3 pt-1">
              <div className="rounded-xl border border-white/[0.08] bg-[#14151c] p-3.5 space-y-1">
                <p className="text-xs font-bold text-white">Master Email Switch</p>
                <p className="text-[11px] leading-relaxed text-[#9ca3af]">
                  Controls general account &amp; campus notices. Disabling this also mutes all
                  optional digests.
                </p>
              </div>
              <div className="rounded-xl border border-white/[0.08] bg-[#14151c] p-3.5 space-y-1">
                <p className="text-xs font-bold text-white">Leaderboard Digests</p>
                <p className="text-[11px] leading-relaxed text-[#9ca3af]">
                  Optional campus standings summaries, milestone recaps, and curated student news.
                </p>
              </div>
              <div className="rounded-xl border border-white/[0.08] bg-[#14151c] p-3.5 space-y-1">
                <p className="text-xs font-bold text-white">Project Match Alerts</p>
                <p className="text-[11px] leading-relaxed text-[#9ca3af]">
                  Curated recruiting projects matched to your verified skills and interests.
                </p>
              </div>
            </div>
            <p className="text-xs leading-relaxed text-[#9ca3af]">
              Transactional security messages—such as time-limited password reset tokens—are
              always sent when requested to protect account integrity.
            </p>
          </section>

          {/* 04 Security Architecture */}
          <section
            id="security-controls"
            className="scroll-mt-28 space-y-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6"
          >
            <div className="flex items-center gap-2.5">
              <Lock className="size-5 text-[#d3fb52]" aria-hidden="true" />
              <h2
                className="text-lg font-bold tracking-tight text-white"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                04. Security Architecture &amp; Technical Safeguards
              </h2>
            </div>
            <ul className="space-y-2 text-xs leading-relaxed text-[#9ca3af] list-disc pl-5">
              <li>
                <strong className="text-white/90">Credential Protection:</strong> Passwords are
                salted and hashed with ASP.NET Core Identity&apos;s PBKDF2{" "}
                <code className="text-white/80">PasswordHasher</code> and protected by per-account
                escalating login backoff and rate-limited auth endpoints.
              </li>
              <li>
                <strong className="text-white/90">Short-Lived Tokens &amp; Rotation:</strong>{" "}
                Stateless access tokens expire every 15 minutes and validate live security stamps;
                refresh tokens rotate on every use and revoke the entire session chain on reuse
                detection.
              </li>
              <li>
                <strong className="text-white/90">Anti-Scraping Controls:</strong> Directory pages
                are strictly capped at 20 students per request behind authenticated{" "}
                <code className="text-white/80">directory-policy</code> rate limits.
              </li>
              <li>
                <strong className="text-white/90">Transport &amp; Browser Hardening:</strong>{" "}
                HTTPS/HSTS transport, verified TLS database connections, strict{" "}
                <code className="text-white/80">Content-Security-Policy</code> headers (zero{" "}
                <code className="text-white/80">&apos;unsafe-eval&apos;</code>), and server-side
                safe-URL sanitization on profile assets and external links.
              </li>
            </ul>
          </section>

          {/* 05 Campus Code of Conduct */}
          <section
            id="campus-conduct"
            className="scroll-mt-28 space-y-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6"
          >
            <div className="flex items-center gap-2.5">
              <Award className="size-5 text-[#10b981]" aria-hidden="true" />
              <h2
                className="text-lg font-bold tracking-tight text-white"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                05. Campus Code of Conduct &amp; Fair Merit Policy
              </h2>
            </div>
            <p className="text-sm leading-relaxed text-white/80">
              All participants—Students, Organization Admins, and Platform Admins—agree to uphold
              academic honesty across squads, events, and campus leaderboards:
            </p>
            <ul className="space-y-2 text-xs leading-relaxed text-[#9ca3af] list-disc pl-5">
              <li>
                Do not automate directory scraping, share QR/RFID attendance check-in codes outside
                physical/live session attendees, or fabricate completed project deliverables for XP.
              </li>
              <li>
                Organization leads and event organizers must handle attendee rosters solely for
                legitimate campus club operations.
              </li>
              <li>
                Repeated check-in abuse triggers per-session burst limits, and fraudulent score
                activity can be audited or reverted by Platform Administrators via immutable ledger
                adjustments.
              </li>
            </ul>
          </section>

          {/* 06 Rights & Account Deletion */}
          <section
            id="rights-retention"
            className="scroll-mt-28 space-y-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6"
          >
            <div className="flex items-center gap-2.5">
              <UserX className="size-5 text-red-400" aria-hidden="true" />
              <h2
                className="text-lg font-bold tracking-tight text-white"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                06. Student Rights, Retention &amp; Instant Account Deletion
              </h2>
            </div>
            <p className="text-sm leading-relaxed text-white/80">
              You may review, edit, export your personal portfolio view, or delete your account at
              any time directly inside Profile Studio:
            </p>
            <ul className="space-y-2 text-xs leading-relaxed text-[#9ca3af] list-disc pl-5">
              <li>
                Using <strong className="text-white/90">Delete Account</strong> in Profile Studio
                immediately rotates your security stamp, invalidates active access and refresh
                tokens, unassigns open project tasks, removes your card from active directory
                searches, and writes an audit event.
              </li>
              <li>
                Non-identifying organizational governance logs and historical event/project records
                retain referential integrity (with zero destructive database cascades) for university
                oversight.
              </li>
            </ul>
          </section>

          {/* 07 Governance & Contact */}
          <section
            id="governance-contact"
            className="scroll-mt-28 space-y-4 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6"
          >
            <div className="flex items-center gap-2.5">
              <FileCheck2 className="size-5 text-[#d3fb52]" aria-hidden="true" />
              <h2
                className="text-lg font-bold tracking-tight text-white"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                07. Platform Governance &amp; Privacy Contact
              </h2>
            </div>
            <p className="text-sm leading-relaxed text-white/80">
              Administrative actions—including soft-deletes, role changes, score adjustments, and
              platform digest dispatches—are recorded in the platform audit trail. For privacy
              inquiries, university data corrections, or abuse reports, contact your campus Student
              Activities lead or Platform Administration team.
            </p>
            <div className="flex flex-wrap items-center gap-3 pt-1">
              <Link to="/profile/me">
                <Button variant="primary" size="md">
                  Manage Privacy in Profile Studio →
                </Button>
              </Link>
              <Link to="/projects">
                <Button variant="outline" size="md">
                  Explore Projects
                </Button>
              </Link>
            </div>
          </section>
        </div>
      </div>
    </div>
  );
}
