/**
 * @date 2026-11-22
 * @purpose Student Directory Card component enforcing THR-I-01 email privacy masking and safe routing.
 */
import { Link } from "react-router-dom";
import type { StudentCardDto } from "@/api/students.api";
import { Avatar, Badge } from "@/components/ui";

export interface StudentCardProps {
  student: StudentCardDto;
}

export function StudentCard({ student }: StudentCardProps) {
  const fullName = `${student.firstName} ${student.lastName}`;

  return (
    <article className="flex min-w-0 flex-col justify-between gap-4 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-5 transition-all duration-200 hover:-translate-y-0.5 hover:border-[#d3fb52]/40 shadow-sm">
      <div className="space-y-3">
        {/* Top Header */}
        <div className="flex items-start gap-3.5">
          <Avatar
            src={student.photoUrl ?? undefined}
            name={fullName}
            size="md"
            className="ring-2 ring-white/10 shrink-0"
          />
          <div className="min-w-0 flex-1">
            <h3
              className="truncate text-base font-bold text-white"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              {fullName}
            </h3>
            <p className="truncate text-xs text-[#9ca3af]">
              {student.facultyName} · Year {student.academicYear}
            </p>
            <p className="truncate text-[11px] text-white/50">{student.universityName}</p>
          </div>
        </div>

        {/* Email Privacy Mask (STRIDE THR-I-01) */}
        <div className="rounded-xl border border-white/[0.06] bg-[#14151c] px-3 py-1.5 text-xs">
          <span className="text-[#9ca3af]">Directory Email: </span>
          {student.email ? (
            <span className="font-mono text-white/90">{student.email}</span>
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
              <span className="text-[10px] text-[#9ca3af] self-center">
                +{student.interests.length - 4} more
              </span>
            )}
          </div>
        )}
      </div>

      {/* Bottom CTA */}
      <div className="flex items-center justify-between border-t border-white/[0.07] pt-3 text-xs">
        <span className="font-mono text-[11px] text-[#d3fb52]">Verified Student</span>
        <Link
          to={`/students/${student.studentId}`}
          className="font-semibold text-white/90 hover:text-[#d3fb52] focus-visible:outline-none focus-visible:underline"
        >
          View Portfolio →
        </Link>
      </div>
    </article>
  );
}
