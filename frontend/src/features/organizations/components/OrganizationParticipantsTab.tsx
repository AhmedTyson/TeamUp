/**
 * @date 2026-11-24
 * @purpose Event Participants & Trainees roster tab cleanly separating event attendees from permanent organizing crew, with verification certificate preview.
 */
import { Award, CalendarCheck2, GraduationCap } from "lucide-react";
import { useState } from "react";
import type { OrganizationParticipant } from "@/api";
import { Avatar, Badge, Button, Card, EmptyState, Modal, Skeleton } from "@/components/ui";

export interface OrganizationParticipantsTabProps {
  organizationName: string;
  participants: readonly OrganizationParticipant[];
  isLoading: boolean;
  isAuthenticated: boolean;
}

export function OrganizationParticipantsTab({
  organizationName,
  participants,
  isLoading,
  isAuthenticated,
}: OrganizationParticipantsTabProps) {
  const [certTarget, setCertTarget] = useState<OrganizationParticipant | null>(null);

  if (!isAuthenticated) {
    return (
      <EmptyState
        icon={<GraduationCap className="size-6" />}
        title="Sign in to inspect participant engagement"
        description="Participant attendance rosters are protected for registered members and organizers."
      />
    );
  }

  if (isLoading) {
    return <Skeleton className="h-44 rounded-2xl" />;
  }

  if (participants.length === 0) {
    return (
      <EmptyState
        icon={<CalendarCheck2 className="size-6" />}
        title="No confirmed event participants yet"
        description="Students who register for workshops and bootcamps hosted by this organization appear here — separate from organizing crew."
      />
    );
  }

  return (
    <div className="space-y-4 pt-2">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <h2 className="text-base font-semibold text-white">Event Participants &amp; Trainees</h2>
          <p className="text-xs text-white/60">
            Attendees joining workshops, bootcamps, and hackathons without permanent staff duties.
          </p>
        </div>
        <Badge variant="accent">{participants.length} participants</Badge>
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        {participants.map((p) => {
          const fullName = `${p.firstName} ${p.lastName}`;
          return (
            <Card key={p.studentId} className="flex flex-col justify-between gap-3 bg-[#14151c] p-4">
              <div className="flex items-start justify-between gap-3">
                <div className="flex min-w-0 items-center gap-3">
                  <Avatar name={fullName} size="sm" />
                  <div className="min-w-0">
                    <p className="truncate font-semibold text-white">{fullName}</p>
                    <p className="truncate text-xs text-white/55">{p.affiliationLabel}</p>
                  </div>
                </div>
                <Badge variant={p.sessionsAttendedCount > 0 ? "success" : "default"} size="sm">
                  {p.sessionsAttendedCount} checked in
                </Badge>
              </div>

              <div className="flex flex-wrap items-center justify-between gap-2 border-t border-white/10 pt-3 text-xs text-white/60">
                <span className="truncate">
                  {p.eventsRegisteredCount} event(s)
                  {p.lastEventTitle ? ` · ${p.lastEventTitle}` : ""}
                </span>
                <Button
                  size="sm"
                  variant="ghost"
                  leftIcon={<Award className="size-3.5 text-[#d3fb52]" />}
                  onClick={() => setCertTarget(p)}
                >
                  Certificate
                </Button>
              </div>
            </Card>
          );
        })}
      </div>

      <Modal
        open={certTarget !== null}
        onClose={() => setCertTarget(null)}
        title="Verified Participation Certificate"
      >
        {certTarget && (
          <div className="space-y-4">
            <div className="rounded-2xl border-2 border-[#d3fb52]/50 bg-[#14151c] p-6 text-center">
              <p className="font-mono text-[10px] uppercase tracking-[0.2em] text-[#d3fb52]">
                TEAMUP VERIFIED ACTIVITY RECORD
              </p>
              <h3 className="mt-3 text-2xl font-bold text-white">
                {certTarget.firstName} {certTarget.lastName}
              </h3>
              <p className="mt-1 text-xs text-white/60">{certTarget.affiliationLabel}</p>
              <p className="mt-4 text-sm leading-relaxed text-white/80">
                Actively participated in{" "}
                <strong className="text-white">{certTarget.eventsRegisteredCount}</strong>{" "}
                event(s) and completed{" "}
                <strong className="text-[#d3fb52]">{certTarget.sessionsAttendedCount}</strong>{" "}
                verified check-in session(s) hosted by{" "}
                <strong className="text-white">{organizationName}</strong>.
              </p>
              {certTarget.lastEventTitle && (
                <p className="mt-3 text-xs font-mono text-[#539886]">
                  Latest highlight: {certTarget.lastEventTitle}
                </p>
              )}
            </div>
            <div className="flex justify-end">
              <Button variant="outline" onClick={() => window.print()}>
                Print certificate
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
