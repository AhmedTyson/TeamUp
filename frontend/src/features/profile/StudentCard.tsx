/**
 * @date 2026-11-22
 * @purpose Student Directory Card component enforcing THR-I-01 email privacy masking, Verified Student badge, and full-card clickability.
 */
import { ArrowRight } from "lucide-react";
import { Link } from "react-router-dom";
import type { StudentCardDto } from "@/api/students.api";
import { Avatar, Badge, VerifiedStudentBadge } from "@/components/ui";
import { useAuth } from "@/features/auth";

export interface StudentCardProps {
  student: StudentCardDto;
}

export function StudentCard({ student }: StudentCardProps) {
  const { user } = useAuth();
  const fullName = `${student.firstName} ${student.lastName}`;
  const isSelfPrivate =
    user?.studentId === student.studentId && user.showEmailInDirectory === false;
  const profileUrl = `/students/${student.studentId}`;

  return (
    <article className="group relative flex min-w-0 cursor-pointer flex-col justify-between gap-4 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-5 shadow-sm transition-all duration-200 hover:-translate-y-0.5 hover:border-[#d3fb52]/40">
      <div className="space-y-3">
        {/* Top Header */}
        <div className="flex items-start gap-3.5">
          <Avatar
            src={student.photoUrl ?? undefined}
            name={fullName}
            size="md"
            className="shrink-0 ring-2 ring-white/10 transition-all group-hover:ring-[#d3fb52]/50"
          />
          <div className="min-w-0 flex-1">
            <h3
              className="truncate text-base font-bold text-white"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              <Link
                to={profileUrl}
                className="transition-colors group-hover:text-[#d3fb52] after:absolute after:inset-0 after:content-[''] focus-visible:outline-none"
              >
                {fullName}
              </Link>
            </h3>
            <p className="truncate text-xs text-[#9ca3af]">
              {student.facultyName} · Year {student.academicYear}
            </p>
            <p className="truncate text-[11px] text-white/50">{student.universityName}</p>
          </div>
        </div>

        {/* Email Privacy Mask (STRIDE THR-I-01) */}
        <div className="flex flex-wrap items-center gap-x-1.5 gap-y-1 rounded-xl border border-white/[0.06] bg-[#14151c] px-3 py-1.5 text-xs">
          <span className="text-[#9ca3af]">Directory Email:</span>
          {student.email ? (
            <>
              <a
                href={`mailto:${student.email}`}
                className="relative z-10 break-all font-mono text-white/90 transition-colors hover:text-[#d3fb52]"
              >
                {student.email}
              </a>
              {isSelfPrivate && (
                <span className="rounded-full bg-white/[0.08] px-2 py-0.5 text-[10px] text-[#9ca3af]">
                  Only visible to you · Private
                </span>
              )}
            </>
          ) : (
            <span className="italic text-[#9ca3af]/80">Email private</span>
          )}
        </div>

        {/* Interests Chips */}
        {student.interests && student.interests.length > 0 && (
          <div className="flex flex-wrap gap-1.5 pt-1">
            {student.interests.slice(0, 4).map((interest) => (
              <Badge key={interest} variant="default" size="sm">
                {interest}
              </Badge>
            ))}
            {student.interests.length > 4 && (
              <span className="self-center text-[10px] text-[#9ca3af]">
                +{student.interests.length - 4} more
              </span>
            )}
          </div>
        )}
      </div>

      {/* Bottom CTA */}
      <div className="flex items-center justify-between gap-2 border-t border-white/[0.07] pt-3 text-xs">
        {student.isEmailVerified ? (
          <VerifiedStudentBadge verified size="sm" />
        ) : (
          <span className="font-mono text-[11px] text-white/45">Unverified Account</span>
        )}
        <span className="inline-flex items-center gap-1 rounded-lg px-2 py-1 font-semibold text-white/90 transition-all duration-200 group-hover:bg-[#d3fb52]/12 group-hover:text-[#d3fb52]">
          <span>View Portfolio</span>
          <ArrowRight
            className="size-3.5 transition-transform duration-200 group-hover:translate-x-0.5"
            aria-hidden="true"
          />
        </span>
      </div>
    </article>
  );
}
