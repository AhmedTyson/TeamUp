/**
 * @date 2026-11-22
 * @purpose Gamification Hub (/gamification) composing Level Progress Card, Badges Gallery, Score Ledger, and Admin Governance.
 */
import { useQuery } from "@tanstack/react-query";
import { Award, Loader2 } from "lucide-react";
import { gamificationApi } from "@/api";
import { Badge, ErrorBanner } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { queryKeys } from "@/lib/queryKeys";
import { AdminGamificationGovernance } from "./AdminGamificationGovernance";
import { BadgesGallery } from "./BadgesGallery";
import { ScoreLedgerTable } from "./ScoreLedgerTable";
import { XpLevelProgressCard } from "./XpLevelProgressCard";

export function GamificationHubPage() {
  const { user, isPlatformAdmin } = useAuth();
  const studentId = user?.studentId ?? 0;

  const scoreQuery = useQuery({
    queryKey: queryKeys.gamification.score(studentId),
    queryFn: ({ signal }) => gamificationApi.getStudentScore(studentId, signal),
    enabled: studentId > 0,
  });

  if (studentId <= 0) {
    return (
      <div className="py-12 text-center text-sm text-[#9ca3af]">
        Please sign in to access your gamification hub and XP progress.
      </div>
    );
  }

  if (scoreQuery.isLoading) {
    return (
      <div className="flex min-h-[40vh] flex-col items-center justify-center gap-3 text-sm text-[#9ca3af]">
        <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
        <span>Loading gamification score ledger…</span>
      </div>
    );
  }

  if (scoreQuery.isError || !scoreQuery.data) {
    return (
      <ErrorBanner
        message={
          scoreQuery.error instanceof Error
            ? scoreQuery.error.message
            : "Unable to load student score data."
        }
        onRetry={() => void scoreQuery.refetch()}
      />
    );
  }

  const score = scoreQuery.data;

  return (
    <div className="space-y-8">
      {/* Page Title */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <div className="inline-flex items-center gap-1.5 rounded-full border border-[#d3fb52]/30 bg-[#d3fb52]/10 px-2.5 py-0.5 text-xs font-bold text-[#d3fb52]">
            <Award className="size-3.5" aria-hidden="true" />
            Verified Participation Engine
          </div>
          <h1
            className="mt-2 text-2xl font-bold tracking-tight text-white sm:text-3xl"
            style={{ fontFamily: "var(--font-heading)" }}
          >
            Gamification &amp; XP Hub
          </h1>
          <p className="mt-1 text-sm text-[#9ca3af]">
            Track your verified level progress, immutable score transactions ledger, and earned achievement badges.
          </p>
        </div>

        <Badge variant="accent" size="md" className="font-mono self-start sm:self-auto">
          {score.totalPoints.toLocaleString()} Total XP
        </Badge>
      </div>

      {/* 1. Personal Level & Progress Card */}
      <XpLevelProgressCard score={score} />

      {/* 2. Badges Showcase (Unlocked vs Locked) */}
      <BadgesGallery studentId={studentId} />

      {/* 3. Read-Only Append-Only Score Transactions Ledger */}
      <ScoreLedgerTable studentId={studentId} />

      {/* 4. PlatformAdmin Governance Console (CRUD Badges, Manual Award, Manual Score Adjust) */}
      {isPlatformAdmin && <AdminGamificationGovernance />}
    </div>
  );
}

