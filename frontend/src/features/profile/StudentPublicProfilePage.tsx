/**
 * @date 2026-11-22
 * @purpose Public Student Portfolio (/students/:id) with bio, safe external links, skills, gamification summary, and admin soft-delete.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ExternalLink,
  EyeOff,
  GraduationCap,
  Loader2,
  Mail,
  Trash2,
  User,
} from "lucide-react";
import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import { studentsApi } from "@/api";
import {
  Avatar,
  Badge,
  Button,
  ConfirmDialog,
  EmptyState,
  ErrorBanner,
  VerifiedStudentBadge,
} from "@/components/ui";
import { useAuth } from "@/features/auth";
import { StudentGamificationSummary } from "@/features/gamification/StudentGamificationSummary";
import { StudentSkillsPanel } from "@/features/skills/StudentSkillsPanel";
import { isSafeHttpUrl } from "@/lib/urlValidation";
import { queryKeys } from "@/lib/queryKeys";

export function StudentPublicProfilePage() {
  const { id } = useParams<{ id: string }>();
  const studentId = Number(id);
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user, isPlatformAdmin } = useAuth();
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  const isMe = user?.studentId === studentId;

  const profileQuery = useQuery({
    queryKey: queryKeys.students.detail(studentId),
    queryFn: ({ signal }) => studentsApi.getById(studentId, signal),
    enabled: !Number.isNaN(studentId) && studentId > 0,
  });

  const deleteStudentMutation = useMutation({
    mutationFn: () => studentsApi.deleteStudent(studentId),
    onSuccess: async () => {
      toast.success("Student account has been soft-deleted.");
      await queryClient.invalidateQueries({ queryKey: queryKeys.students.all });
      void navigate("/students");
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : "Failed to delete student account.";
      toast.error(msg);
    },
  });

  if (Number.isNaN(studentId) || studentId <= 0) {
    return (
      <EmptyState
        icon={<User className="size-6" />}
        title="Invalid student ID"
        description="The student requested does not exist or has an invalid URL parameter."
        action={
          <Button variant="outline" size="sm" onClick={() => void navigate("/students")}>
            Back to Directory
          </Button>
        }
      />
    );
  }

  if (profileQuery.isLoading) {
    return (
      <div className="flex min-h-[40vh] flex-col items-center justify-center gap-3 text-sm text-[#9ca3af]">
        <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
        <span>Loading student portfolio…</span>
      </div>
    );
  }

  if (profileQuery.isError || !profileQuery.data) {
    return (
      <ErrorBanner
        message={
          profileQuery.error instanceof Error
            ? profileQuery.error.message
            : "Student profile could not be found."
        }
        onRetry={() => void profileQuery.refetch()}
      />
    );
  }

  const student = profileQuery.data;
  const fullName = `${student.firstName} ${student.lastName}`;
  const profile = student.profile;

  return (
    <div className="space-y-8">
      {/* Top Hero Banner */}
      <section className="teamup-hero-bg overflow-hidden rounded-[28px] border border-white/12 p-6 shadow-2xl sm:p-10">
        <div className="flex flex-col justify-between gap-6 md:flex-row md:items-center">
          <div className="flex flex-col sm:flex-row items-start sm:items-center gap-5">
            <Avatar
              src={profile?.photoUrl ?? undefined}
              name={fullName}
              size="lg"
              className="ring-4 ring-white/15 shrink-0"
            />
            <div className="space-y-1.5">
              <div className="flex flex-wrap items-center gap-2">
                <span className="inline-flex items-center gap-1 rounded-full border border-[#d3fb52]/35 bg-[#d3fb52]/10 px-2.5 py-0.5 text-xs font-bold text-[#d3fb52]">
                  <GraduationCap className="size-3" aria-hidden="true" />
                  Year {student.academicYear} Student
                </span>
                <VerifiedStudentBadge
                  verified={Boolean(student.isEmailVerified)}
                  showUnverified
                  size="sm"
                />
                {isMe && (
                  <Badge variant="accent" size="sm">
                    You
                  </Badge>
                )}
              </div>
              <h1
                className="text-2xl font-bold tracking-tight text-white sm:text-4xl"
                style={{ fontFamily: "var(--font-heading)" }}
              >
                {fullName}
              </h1>
              <p className="text-sm text-white/80">
                {student.facultyName} · {student.universityName}
              </p>
            </div>
          </div>

          {/* Action CTAs */}
          <div className="flex flex-wrap items-center gap-3">
            {isMe && (
              <Button
                variant="primary"
                size="md"
                onClick={() => void navigate("/profile/me")}
              >
                Edit My Portfolio
              </Button>
            )}

            {isPlatformAdmin && !isMe && (
              <Button
                variant="danger"
                size="md"
                onClick={() => setIsDeleteModalOpen(true)}
                className="gap-1.5"
              >
                <Trash2 className="size-4" aria-hidden="true" />
                Soft-Delete Student
              </Button>
            )}
          </div>
        </div>
      </section>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-12">
        {/* Left Column: Bio & External Links (7 cols) */}
        <div className="space-y-6 lg:col-span-7">
          {/* Biography Card */}
          <div className="space-y-4 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6">
            <h2
              className="text-base font-bold text-white uppercase tracking-wider"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              About &amp; Biography
            </h2>
            {profile?.bio ? (
              <p className="whitespace-pre-wrap text-sm leading-relaxed text-[#9ca3af]">
                {profile.bio}
              </p>
            ) : (
              <p className="italic text-xs text-[#9ca3af]/70">No biography provided yet.</p>
            )}

            {/* External Links & Contact Privacy Status (THR-I-01) */}
            <div className="flex flex-wrap items-center gap-3 border-t border-white/[0.08] pt-4">
              {student.email ? (
                <a
                  href={`mailto:${student.email}`}
                  className="inline-flex items-center gap-1.5 rounded-xl border border-[#d3fb52]/30 bg-[#d3fb52]/[0.08] px-3 py-1.5 text-xs font-semibold text-white hover:border-[#d3fb52]/60 hover:text-[#d3fb52] transition-colors break-all"
                >
                  <Mail className="size-3.5 shrink-0 text-[#d3fb52]" aria-hidden="true" />
                  <span>{student.email}</span>
                  {isMe && user?.showEmailInDirectory === false && (
                    <span className="text-[10px] font-normal text-white/65">(Private to peers)</span>
                  )}
                </a>
              ) : (
                <span className="inline-flex items-center gap-1.5 rounded-xl border border-white/10 bg-[#14151c] px-3 py-1.5 text-xs italic text-[#9ca3af]">
                  <EyeOff className="size-3.5 shrink-0 text-[#9ca3af]" aria-hidden="true" />
                  Email private
                </span>
              )}

              {profile?.gitHubUrl && isSafeHttpUrl(profile.gitHubUrl) ? (
                <a
                  href={profile.gitHubUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-1.5 rounded-xl border border-white/10 bg-[#14151c] px-3 py-1.5 text-xs font-semibold text-white/90 hover:border-[#d3fb52]/40 hover:text-white transition-colors"
                >
                  GitHub
                  <ExternalLink className="size-3 text-[#d3fb52]" aria-hidden="true" />
                </a>
              ) : null}

              {profile?.linkedInUrl && isSafeHttpUrl(profile.linkedInUrl) ? (
                <a
                  href={profile.linkedInUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-1.5 rounded-xl border border-white/10 bg-[#14151c] px-3 py-1.5 text-xs font-semibold text-white/90 hover:border-[#d3fb52]/40 hover:text-white transition-colors"
                >
                  LinkedIn
                  <ExternalLink className="size-3 text-[#d3fb52]" aria-hidden="true" />
                </a>
              ) : null}

              {!profile?.gitHubUrl && !profile?.linkedInUrl && (
                <span className="text-xs text-[#9ca3af]">No external profiles linked.</span>
              )}
            </div>
          </div>

          {/* Interests Tags */}
          <div className="space-y-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26] p-6">
            <h2
              className="text-base font-bold text-white uppercase tracking-wider"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              Interests &amp; Specializations
            </h2>
            {student.interests && student.interests.length > 0 ? (
              <div className="flex flex-wrap gap-2">
                {student.interests.map((interest) => (
                  <Badge key={interest} variant="default" size="md">
                    {interest}
                  </Badge>
                ))}
              </div>
            ) : (
              <p className="italic text-xs text-[#9ca3af]/70">No interests tagged yet.</p>
            )}
          </div>

          {/* M2's Skills Panel */}
          <StudentSkillsPanel studentId={studentId} editable={isMe} />
        </div>

        {/* Right Column: Gamification & Badges Summary (5 cols) */}
        <div className="space-y-6 lg:col-span-5">
          <StudentGamificationSummary studentId={studentId} />
        </div>
      </div>

      {/* Admin Delete Confirmation Modal */}
      <ConfirmDialog
        open={isDeleteModalOpen}
        onClose={() => setIsDeleteModalOpen(false)}
        title="Soft-Delete Student Account"
        description={`Are you sure you want to soft-delete the student account for ${fullName}? This removes them from the active campus directory.`}
        confirmLabel="Yes, Soft-Delete"
        variant="danger"
        isLoading={deleteStudentMutation.isPending}
        onConfirm={() => deleteStudentMutation.mutate()}
      />
    </div>
  );
}
