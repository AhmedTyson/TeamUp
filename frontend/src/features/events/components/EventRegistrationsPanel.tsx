/**
 * @date 2026-11-24
 * @purpose Organizer registrations & FIFO waitlist panel for EventDetailPage.
 */
import { Users } from "lucide-react";
import { Link } from "react-router-dom";
import type { EventRegistration } from "@/api";
import { Avatar, Button, Card, EmptyState, ErrorBanner, Skeleton, StatusPill } from "@/components/ui";

export interface EventRegistrationsPanelProps {
  registrations: readonly EventRegistration[];
  isLoading: boolean;
  isError: boolean;
  error: unknown;
  onCancelRegistration: (studentId: number) => void;
}

export function EventRegistrationsPanel({
  registrations,
  isLoading,
  isError,
  error,
  onCancelRegistration,
}: EventRegistrationsPanelProps) {
  return (
    <Card className="p-5 sm:p-6">
      <h2 className="text-xl font-bold text-white">Event registrations</h2>
      <p className="mt-1 text-xs text-white/55">
        Confirmed attendees and automated FIFO waitlist queue.
      </p>

      {isLoading ? (
        <Skeleton className="mt-4 h-40 rounded-2xl" />
      ) : isError ? (
        <div className="mt-4">
          <ErrorBanner error={error} />
        </div>
      ) : registrations.length > 0 ? (
        <div className="mt-4 space-y-2.5">
          {registrations.map((reg) => (
            <div
              key={reg.studentId}
              className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-white/10 bg-[#14151c] px-4 py-3 text-sm"
            >
              <Link
                to={`/students/${reg.studentId}`}
                className="group flex min-w-0 items-center gap-2.5"
              >
                <Avatar
                  name={`${reg.firstName} ${reg.lastName}`}
                  size="xs"
                  className="shrink-0 ring-1 ring-white/15 group-hover:ring-[#d3fb52]"
                />
                <div className="min-w-0">
                  <p className="truncate font-medium text-white transition-colors group-hover:text-[#d3fb52]">
                    {reg.firstName} {reg.lastName}
                  </p>
                  <p className="truncate text-xs text-white/50">{reg.email}</p>
                </div>
              </Link>
              <div className="flex items-center gap-2">
                <StatusPill status={reg.status} />
                <Button
                  size="sm"
                  variant="danger"
                  onClick={() => onCancelRegistration(reg.studentId)}
                >
                  Cancel
                </Button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="mt-4">
          <EmptyState
            icon={<Users className="size-6" />}
            title="No registrations yet"
            description="Student registrations will populate here once attendees sign up."
          />
        </div>
      )}
    </Card>
  );
}
