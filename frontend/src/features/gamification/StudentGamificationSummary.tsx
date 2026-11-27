/**
 * @date 2026-11-22
 * @purpose Cross-module StudentGamificationSummary widget displaying Level, XP points, progress bar, and earned badges shelf.
 */
import { useQuery } from "@tanstack/react-query";
import { Award, Sparkles } from "lucide-react";
import { Link } from "react-router-dom";
import {
  badgesApi,
  gamificationApi,
  type StudentBadgeDto,
  type StudentScoreDto,
} from "@/api";
import { Badge } from "@/components/ui";
import { queryKeys } from "@/lib/queryKeys";
import { computeLevelProgressPercent } from "./levelProgress";

export interface StudentGamificationSummaryProps {
  studentId: number;
  score?: StudentScoreDto;
  badges?: StudentBadgeDto[];
}

export function StudentGamificationSummary({
  studentId,
  score: scoreProp,
  badges: badgesProp,
}: StudentGamificationSummaryProps) {
  const scoreQuery = useQuery({
    queryKey: queryKeys.gamification.score(studentId),
    queryFn: ({ signal }) => gamificationApi.getStudentScore(studentId, signal),
    enabled: studentId > 0 && scoreProp === undefined,
  });

  const badgesQuery = useQuery({
    queryKey: queryKeys.badges.student(studentId),
    queryFn: ({ signal }) => badgesApi.getStudentBadges(studentId, signal),
    enabled: studentId > 0 && badgesProp === undefined,
  });

  const score = scoreProp ?? scoreQuery.data;
  const badges = badgesProp ?? badgesQuery.data ?? [];
  const pct = score ? computeLevelProgressPercent(score) : 0;

  return (
    <div className="space-y-4 rounded-2xl border border-white/[0.085] bg-[#14151c] p-5">
      <div className="flex items-center justify-between border-b border-white/[0.08] pb-3">
        <div className="flex items-center gap-2">
          <Sparkles className="size-4 text-[#d3fb52]" aria-hidden="true" />
          <h3 className="text-sm font-bold uppercase tracking-wider text-white">
            Campus Standing &amp; Badges
          </h3>
        </div>
        <Link
          to="/leaderboard"
          className="text-xs font-semibold text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]"
        >
          View Leaderboard →
        </Link>
      </div>

      {score ? (
        <div className="space-y-3">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-2">
              <span className="rounded-xl bg-[#d3fb52] px-2.5 py-1 text-xs font-black text-[#14151c]">
                Level {score.level}
              </span>
              <span className="font-mono text-sm font-bold text-white">
                {score.totalPoints.toLocaleString()} XP
              </span>
            </div>
            <span className="text-xs text-[#9ca3af]">
              {score.pointsToNext > 0
                ? `${score.pointsToNext} XP to Level ${score.level + 1}`
                : "Max Level"}
            </span>
          </div>

          {/* Progress Bar */}
          <div className="space-y-1">
            <div className="h-2 w-full overflow-hidden rounded-full bg-white/10">
              <div
                className="h-full rounded-full bg-[#d3fb52] transition-all duration-500"
                style={{ width: `${pct}%` }}
              />
            </div>
            <div className="flex justify-between text-[11px] text-white/50">
              <span>{score.currentLevelStart} pts</span>
              <span>{pct}%</span>
              <span>{score.nextLevelThreshold} pts</span>
            </div>
          </div>
        </div>
      ) : (
        <p className="text-xs text-[#9ca3af]">XP and score data loading…</p>
      )}

      {/* Earned Badges Shelf */}
      <div className="pt-2">
        <div className="mb-2 flex items-center justify-between">
          <span className="text-xs font-semibold text-[#9ca3af]">
            Earned Badges ({badges.length})
          </span>
          <Link to="/gamification" className="text-[11px] text-[#d3fb52] no-underline transition-colors hover:text-[#e6ff8a]">
            All Badges
          </Link>
        </div>

        {badges.length === 0 ? (
          <p className="py-2 text-xs text-[#9ca3af]/80">No badges earned yet.</p>
        ) : (
          <div className="flex flex-wrap gap-2">
            {badges.map((b) => (
              <div
                key={b.badgeId}
                title={`${b.badgeName}: ${b.description}`}
                className="inline-flex items-center gap-1.5 rounded-xl border border-white/10 bg-[#1b1d26] px-2.5 py-1 text-xs text-white"
              >
                <Award className="size-3.5 text-[#d3fb52]" aria-hidden="true" />
                <span className="font-medium">{b.badgeName}</span>
                <Badge variant="accent" size="sm">
                  Verified
                </Badge>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

