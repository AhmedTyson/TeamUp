/**
 * @date 2026-11-22
 * @purpose Personal Level card displaying Level formula FLOOR(SQRT(pts/100))+1, animated progress bar, and points to next level.
 */
import { useQuery } from "@tanstack/react-query";
import { ArrowUpRight, Flame, Loader2, Sparkles, Trophy } from "lucide-react";
import { Link } from "react-router-dom";
import { gamificationApi, type StudentScoreDto } from "@/api";
import { ErrorBanner } from "@/components/ui";
import { queryKeys } from "@/lib/queryKeys";

export interface XpLevelProgressCardProps {
  score?: StudentScoreDto;
  studentId?: number;
}

function getLevelTitle(level: number): string {
  if (level <= 1) return "Novice";
  if (level === 2) return "Apprentice";
  if (level === 3) return "Builder";
  if (level === 4) return "Explorer";
  if (level === 5) return "Architect";
  return "Master";
}

export function XpLevelProgressCard({ score: scoreProp, studentId }: XpLevelProgressCardProps) {
  const scoreQuery = useQuery({
    queryKey: queryKeys.gamification.score(studentId ?? 0),
    queryFn: ({ signal }) => gamificationApi.getStudentScore(studentId ?? 0, signal),
    enabled: !scoreProp && Boolean(studentId && studentId > 0),
  });

  const score = scoreProp ?? scoreQuery.data;

  if (!score && scoreQuery.isLoading) {
    return (
      <div className="flex min-h-[160px] items-center justify-center rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-6 text-sm text-[#9ca3af]">
        <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
      </div>
    );
  }

  if (!score && scoreQuery.isError) {
    return (
      <ErrorBanner
        message={
          scoreQuery.error instanceof Error
            ? scoreQuery.error.message
            : "Failed to load XP standings."
        }
        onRetry={() => void scoreQuery.refetch()}
      />
    );
  }

  if (!score) return null;

  const currentLevel = score.level;
  const currentPts = score.totalPoints;
  const ptsToNext = score.pointsToNext;
  const progressPct = Math.min(100, Math.max(0, Math.round(score.progress * 100)));

  // Bounds for Level formula: Level = FLOOR(SQRT(pts / 100)) + 1
  const currentLevelMinPts = Math.pow(currentLevel - 1, 2) * 100;
  const nextLevelTargetPts = Math.pow(currentLevel, 2) * 100;

  return (
    <div className="relative overflow-hidden rounded-3xl border border-white/[0.085] bg-gradient-to-br from-[#1b1d26] via-[#171922] to-[#12131a] p-6 sm:p-8">
      {/* Decorative background glow */}
      <div
        className="pointer-events-none absolute -right-20 -top-20 size-72 rounded-full bg-[#d3fb52]/10 blur-3xl"
        aria-hidden="true"
      />

      <div className="relative z-10 flex flex-col justify-between gap-6 md:flex-row md:items-center">
        {/* Left Column: Level pill, Total XP & Formula */}
        <div className="space-y-4">
          <div className="flex flex-wrap items-center gap-3">
            <span className="inline-flex items-center gap-1.5 rounded-2xl bg-[#d3fb52] px-3.5 py-1.5 text-sm font-black text-[#14151c] shadow-[0_0_20px_rgba(211,251,82,0.3)]">
              <Sparkles className="size-4" aria-hidden="true" />
              Level {currentLevel} {getLevelTitle(currentLevel)}
            </span>
            <span className="inline-flex items-center gap-1.5 rounded-full border border-white/10 bg-white/5 px-3 py-1 font-mono text-xs text-[#9ca3af]">
              <Flame className="size-3.5 text-amber-400" aria-hidden="true" />
              Formula: <code className="text-white">FLOOR(SQRT(pts / 100)) + 1</code>
            </span>
          </div>

          <div>
            <div className="flex items-baseline gap-2">
              <span
                className="font-mono text-4xl sm:text-5xl font-black text-white tracking-tight"
                style={{ fontFamily: "'Antonio', monospace" }}
              >
                {currentPts.toLocaleString()}
              </span>
              <span className="font-mono text-lg font-bold text-[#d3fb52]">XP</span>
            </div>
            <p className="mt-1 text-xs text-[#9ca3af]">
              Earned across verified attendance, project deliveries, and completed sprint tasks.
            </p>
          </div>
        </div>

        {/* Right Column: CTA link to Leaderboard */}
        <div className="flex shrink-0 items-center">
          <Link
            to="/leaderboard"
            className="inline-flex items-center gap-2 rounded-xl border border-[#d3fb52]/30 bg-[#d3fb52]/10 px-4 py-2.5 text-xs font-bold text-[#d3fb52] transition-colors hover:border-[#d3fb52] hover:bg-[#d3fb52] hover:text-[#14151c]"
          >
            <Trophy className="size-4" aria-hidden="true" />
            <span>Campus Leaderboard</span>
            <ArrowUpRight className="size-3.5" aria-hidden="true" />
          </Link>
        </div>
      </div>

      {/* Progress Bar Section */}
      <div className="relative z-10 mt-6 space-y-2 border-t border-white/[0.08] pt-5">
        <div className="flex items-center justify-between text-xs">
          <span className="font-mono text-[#9ca3af]">
            Level {currentLevel} ({currentLevelMinPts.toLocaleString()} XP)
          </span>
          <span className="font-mono font-bold text-[#d3fb52]">
            {progressPct}% ({ptsToNext > 0 ? `${ptsToNext.toLocaleString()} XP needed to level up` : "Max Level Reached"})
          </span>
          <span className="font-mono text-[#9ca3af]">
            Level {currentLevel + 1} ({nextLevelTargetPts.toLocaleString()} XP)
          </span>
        </div>

        <div className="h-3 w-full overflow-hidden rounded-full bg-white/10 p-0.5">
          <div
            className="h-full rounded-full bg-gradient-to-r from-[#10b981] via-[#d3fb52] to-[#deff6e] transition-all duration-700 ease-out shadow-[0_0_12px_rgba(211,251,82,0.5)]"
            style={{ width: `${progressPct}%` }}
          />
        </div>
      </div>
    </div>
  );
}
