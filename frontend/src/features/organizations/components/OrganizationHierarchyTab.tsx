/**
 * @date 2026-11-24
 * @purpose Interactive 4-tier Egyptian student-activity Org Chart tree (High Board -> Committee Leadership -> Organizing Crew -> Event Participants) with zoom controls, unit filter, and role inspector drawer.
 */
import { Crown, Network, ShieldCheck, Users, Wrench, ZoomIn, ZoomOut } from "lucide-react";
import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import type { OrganizationHierarchy, OrganizationMember } from "@/api";
import { Avatar, Badge, Card, EmptyState, Select } from "@/components/ui";
import { cn } from "@/lib/cn";

export interface OrganizationHierarchyTabProps {
  hierarchy?: OrganizationHierarchy;
  isLoading: boolean;
}

const ZOOM_LEVELS = [0.85, 1, 1.15] as const;

function permissionSummaryForMember(member: OrganizationMember): readonly string[] {
  if (member.hierarchyTier === "HighBoard") {
    return [
      "Full organization governance & High Board elections",
      "Create / edit / archive chapters, events & sessions",
      "Approve & promote any committee member or chair",
      "Export attendance & recruitment CSV records",
    ];
  }
  if (member.hierarchyTier === "CommitteeLeadership") {
    const isHr =
      member.unitCode?.toUpperCase() === "HR" ||
      member.role.toLowerCase().includes("hr");
    return isHr
      ? [
          "Screen incoming recruitment applicants across units",
          "Record evaluation scores & HR interview notes",
          "Coordinate committee onboarding seasons",
        ]
      : [
          `Review & score applicants targeting ${member.unitName ?? "this unit"}`,
          "Lead technical workshops and mentor crew members",
          "Coordinate committee deliverables with High Board",
        ];
  }
  return [
    "Participate in assigned chapter/committee tasks",
    "Assist event logistics, workshops, and attendee onboarding",
    "Eligible for instructor & vice head nominations",
  ];
}

export function OrganizationHierarchyTab({
  hierarchy,
  isLoading,
}: OrganizationHierarchyTabProps) {
  const [zoomIdx, setZoomIdx] = useState(1);
  const [unitFilter, setUnitFilter] = useState("");
  const [selectedMember, setSelectedMember] = useState<OrganizationMember | null>(null);

  const unitOptions = useMemo(
    () =>
      (hierarchy?.units ?? []).map((u) => ({
        value: String(u.organizationUnitId),
        label: `${u.code} · ${u.name}`,
      })),
    [hierarchy?.units],
  );

  const filterByUnit = (members: readonly OrganizationMember[]) => {
    if (!unitFilter) {
      return members;
    }
    const targetId = Number(unitFilter);
    return members.filter((m) => m.organizationUnitId === targetId);
  };

  if (isLoading) {
    return (
      <div className="py-10 text-center text-xs text-white/60">
        Loading interactive hierarchy tree…
      </div>
    );
  }

  if (!hierarchy) {
    return (
      <EmptyState
        icon={<Network className="size-6" />}
        title="Hierarchy unavailable"
        description="Organization governance data could not be loaded."
      />
    );
  }

  const highBoard = hierarchy.highBoard;
  const leadership = filterByUnit(hierarchy.committeeLeadership);
  const crew = filterByUnit(hierarchy.organizingCrew);
  const scale = ZOOM_LEVELS[zoomIdx];

  return (
    <div className="space-y-5 pt-2">
      <div className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-white/10 bg-[#14151c] px-4 py-3">
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-xs font-semibold text-white/80">Egyptian Governance Tree</span>
          <Badge variant="accent" size="sm">
            {hierarchy.scope}
          </Badge>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <Select
            aria-label="Filter hierarchy by chapter or committee"
            placeholder="All chapters & committees"
            options={unitOptions}
            value={unitFilter}
            onChange={(e) => setUnitFilter(e.target.value)}
            className="h-9 min-w-[200px] bg-[#1b1d26] text-xs"
          />
          <div className="inline-flex items-center rounded-xl border border-white/15 bg-[#1b1d26] p-0.5">
            <button
              type="button"
              onClick={() => setZoomIdx((i) => Math.max(0, i - 1))}
              disabled={zoomIdx === 0}
              aria-label="Zoom out hierarchy tree"
              className="inline-flex size-8 cursor-pointer items-center justify-center rounded-lg text-white/75 hover:bg-white/10 disabled:opacity-40"
            >
              <ZoomOut className="size-3.5" aria-hidden="true" />
            </button>
            <span className="px-2 font-mono text-[11px] text-white/75">
              {Math.round(scale * 100)}%
            </span>
            <button
              type="button"
              onClick={() => setZoomIdx((i) => Math.min(ZOOM_LEVELS.length - 1, i + 1))}
              disabled={zoomIdx === ZOOM_LEVELS.length - 1}
              aria-label="Zoom in hierarchy tree"
              className="inline-flex size-8 cursor-pointer items-center justify-center rounded-lg text-white/75 hover:bg-white/10 disabled:opacity-40"
            >
              <ZoomIn className="size-3.5" aria-hidden="true" />
            </button>
          </div>
        </div>
      </div>

      <div className="grid gap-5 lg:grid-cols-[1fr_320px]">
        <div className="overflow-x-auto rounded-2xl border border-white/10 bg-[#14151c] p-5">
          <div
            style={{ transform: `scale(${scale})`, transformOrigin: "top center" }}
            className="min-w-[540px] space-y-5 transition-transform duration-200"
          >
            {/* Tier 1: High Board */}
            <div className="space-y-2.5 text-center">
              <div className="inline-flex items-center gap-1.5 rounded-full border border-[#d3fb52]/35 bg-[#d3fb52]/10 px-3 py-1 text-[11px] font-bold uppercase tracking-wider text-[#d3fb52]">
                <Crown className="size-3.5" aria-hidden="true" />
                Tier 1 · High Board (Chair · Vice · Secretary · Treasurer)
              </div>
              <div className="flex flex-wrap justify-center gap-3">
                {highBoard.length > 0 ? (
                  highBoard.map((member) => (
                    <HierarchyNodeCard
                      key={member.studentId}
                      member={member}
                      tone="lime"
                      selected={selectedMember?.studentId === member.studentId}
                      onSelect={() => setSelectedMember(member)}
                    />
                  ))
                ) : (
                  <p className="text-xs text-white/50">No High Board members assigned yet.</p>
                )}
              </div>
            </div>

            {/* Orthogonal Connector */}
            <div className="mx-auto flex w-4/5 flex-col items-center" aria-hidden="true">
              <div className="h-5 w-px bg-white/20" />
              <div className="h-px w-full bg-white/20" />
              <div className="flex w-full justify-between">
                <div className="h-3 w-px bg-white/20" />
                <div className="h-3 w-px bg-white/20" />
                <div className="h-3 w-px bg-white/20" />
              </div>
            </div>

            {/* Tier 2: Committee Heads & Vice Heads */}
            <div className="space-y-2.5 text-center">
              <div className="inline-flex items-center gap-1.5 rounded-full border border-emerald-400/35 bg-emerald-400/10 px-3 py-1 text-[11px] font-bold uppercase tracking-wider text-emerald-300">
                <ShieldCheck className="size-3.5" aria-hidden="true" />
                Tier 2 · Chapter Chairs &amp; Committee Heads
              </div>
              <div className="flex flex-wrap justify-center gap-3">
                {leadership.length > 0 ? (
                  leadership.map((member) => (
                    <HierarchyNodeCard
                      key={member.studentId}
                      member={member}
                      tone="emerald"
                      selected={selectedMember?.studentId === member.studentId}
                      onSelect={() => setSelectedMember(member)}
                    />
                  ))
                ) : (
                  <p className="text-xs text-white/50">No committee heads match the filter.</p>
                )}
              </div>
            </div>

            <div className="mx-auto h-5 w-px bg-white/15" aria-hidden="true" />

            {/* Tier 3: Organizing Crew & Instructors */}
            <div className="space-y-2.5 text-center">
              <div className="inline-flex items-center gap-1.5 rounded-full border border-white/15 bg-white/[0.06] px-3 py-1 text-[11px] font-bold uppercase tracking-wider text-white/80">
                <Wrench className="size-3.5" aria-hidden="true" />
                Tier 3 · Instructors &amp; Organizing Crew ({crew.length})
              </div>
              <div className="flex flex-wrap justify-center gap-3">
                {crew.length > 0 ? (
                  crew.map((member) => (
                    <HierarchyNodeCard
                      key={member.studentId}
                      member={member}
                      tone="neutral"
                      selected={selectedMember?.studentId === member.studentId}
                      onSelect={() => setSelectedMember(member)}
                    />
                  ))
                ) : (
                  <p className="text-xs text-white/50">No active crew in this filter.</p>
                )}
              </div>
            </div>

            {/* Tier 4: Event Participants Band */}
            <div className="rounded-xl border border-dashed border-sky-400/35 bg-sky-400/[0.07] p-3.5 text-center">
              <div className="flex flex-wrap items-center justify-center gap-2 text-xs font-semibold text-sky-200">
                <Users className="size-4 text-sky-300" aria-hidden="true" />
                <span>
                  Tier 4 · Event Participants &amp; Bootcamp Trainees:{" "}
                  <strong className="font-mono text-white">{hierarchy.participantsCount}</strong> confirmed
                </span>
              </div>
              <p className="mt-1 text-[11px] text-white/60">
                Tracked separately from organizing members in the Participants tab.
              </p>
            </div>
          </div>
        </div>

        {/* Right Role & Permissions Inspector */}
        <Card className="h-fit space-y-4 bg-[#14151c] p-5">
          <div className="border-b border-white/10 pb-3">
            <p className="font-mono text-[10px] uppercase tracking-widest text-[#d3fb52]">
              Role &amp; Authority Inspector
            </p>
            <h3 className="mt-1 text-base font-bold text-white">
              {selectedMember
                ? `${selectedMember.firstName} ${selectedMember.lastName}`
                : "Select any member node"}
            </h3>
          </div>

          {selectedMember ? (
            <div className="space-y-4">
              <div className="flex items-center gap-3">
                <Avatar name={`${selectedMember.firstName} ${selectedMember.lastName}`} size="md" />
                <div className="min-w-0">
                  <p className="text-sm font-semibold text-[#d3fb52]">{selectedMember.role}</p>
                  <p className="text-xs text-white/60">
                    {selectedMember.unitName
                      ? `${selectedMember.unitCode} · ${selectedMember.unitName}`
                      : "Organization-Wide"}
                  </p>
                </div>
              </div>

              <div className="space-y-1.5">
                <p className="text-xs font-semibold uppercase tracking-wider text-white/55">
                  Permissions &amp; Responsibilities
                </p>
                <ul className="space-y-1.5 text-xs text-white/80">
                  {permissionSummaryForMember(selectedMember).map((item) => (
                    <li key={item} className="flex items-start gap-2">
                      <span className="mt-1.5 size-1.5 shrink-0 rounded-full bg-[#d3fb52]" />
                      <span>{item}</span>
                    </li>
                  ))}
                </ul>
              </div>

              <Link
                to={`/students/${selectedMember.studentId}`}
                className="inline-flex w-full items-center justify-center rounded-xl border border-[#d3fb52]/35 bg-[#d3fb52]/10 px-3 py-2 text-xs font-semibold text-[#d3fb52] hover:bg-[#d3fb52]/20"
              >
                View student portfolio →
              </Link>
            </div>
          ) : (
            <p className="text-xs leading-relaxed text-white/60">
              Click any member card on the Egyptian hierarchy tree to inspect their unit scope,
              governance tier, and delegated review permissions.
            </p>
          )}
        </Card>
      </div>
    </div>
  );
}

function HierarchyNodeCard({
  member,
  tone,
  selected,
  onSelect,
}: {
  member: OrganizationMember;
  tone: "lime" | "emerald" | "neutral";
  selected: boolean;
  onSelect: () => void;
}) {
  return (
    <button
      type="button"
      onClick={onSelect}
      className={cn(
        "flex min-w-[180px] max-w-[230px] cursor-pointer items-center gap-2.5 rounded-xl border px-3 py-2.5 text-start transition-all",
        selected
          ? "border-[#d3fb52] bg-[#d3fb52]/15 ring-2 ring-[#d3fb52]/35"
          : tone === "lime"
            ? "border-[#d3fb52]/35 bg-[#1b1d26] hover:border-[#d3fb52]/70"
            : tone === "emerald"
              ? "border-emerald-400/30 bg-[#1b1d26] hover:border-emerald-400/70"
              : "border-white/12 bg-[#1b1d26] hover:border-white/30",
      )}
    >
      <Avatar name={`${member.firstName} ${member.lastName}`} size="xs" />
      <div className="min-w-0 flex-1">
        <p className="truncate text-xs font-bold text-white">
          {member.firstName} {member.lastName}
        </p>
        <p className="truncate text-[11px] text-white/65">{member.role}</p>
        {member.unitCode && (
          <span className="mt-0.5 inline-block rounded bg-white/[0.07] px-1.5 py-0.5 font-mono text-[10px] text-[#d3fb52]">
            {member.unitCode}
          </span>
        )}
      </div>
    </button>
  );
}
