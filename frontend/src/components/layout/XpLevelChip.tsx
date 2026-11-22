/**
 * @date 2026-11-22
 * @purpose Compact Navbar XP level chip displaying live student level, total points, and mini progress bar synced with queryKeys.gamification.score.
 */
import { useQuery } from "@tanstack/react-query";
import { Sparkles } from "lucide-react";
import { Link } from "react-router-dom";
import {
  API_ENDPOINTS,
  apiClient,
  unwrapApiResponse,
  type ApiResponse,
} from "@/api";
import { useAuth } from "@/features/auth";
import { cn } from "@/lib/cn";
import { queryKeys } from "@/lib/queryKeys";

export interface XpLevelChipProps {
  isScrolled?: boolean;
}

export interface NavbarStudentScore {
  studentId: number;
  totalPoints: number;
  level: number;
  currentLevelStart: number;
  nextLevelThreshold: number;
  pointsToNext: number;
  progress: number;
}

export function XpLevelChip({ isScrolled = false }: XpLevelChipProps) {
  const { user, isAuthenticated } = useAuth();
  const studentId = user?.studentId ?? 0;

  const scoreQuery = useQuery({
    queryKey: queryKeys.gamification.score(studentId),
    queryFn: async ({ signal }) => {
      const res = await apiClient.get<ApiResponse<NavbarStudentScore>>(
        API_ENDPOINTS.students.score(studentId),
        { signal },
      );
      return unwrapApiResponse(res);
    },
    enabled: isAuthenticated && studentId > 0,
    staleTime: 60_000,
    retry: false,
  });

  if (!isAuthenticated || studentId <= 0) {
    return null;
  }

  const score = scoreQuery.data;
  const pct = score ? Math.min(100, Math.max(0, Math.round(score.progress * 100))) : 0;

  return (
    <Link
      to="/gamification"
      aria-label={
        score
          ? `Level ${score.level} with ${score.totalPoints} XP`
          : "Open XP ledger and badges"
      }
      title={
        score
          ? `${score.pointsToNext} XP to Level ${score.level + 1} (${pct}%)`
          : "View XP & Badges"
      }
      className={cn(
        "hidden items-center gap-1.5 rounded-full border px-2 py-1 text-[11px] font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52] sm:inline-flex xl:gap-2 xl:px-2.5 xl:text-xs",
        isScrolled
          ? "border-[#14151c] bg-[#14151c] text-white shadow-sm hover:bg-[#222532]"
          : "border-[#d3fb52]/30 bg-[#d3fb52]/10 text-[#d3fb52] hover:bg-[#d3fb52]/20",
      )}
    >
      <Sparkles
        className={cn("size-3.5 shrink-0", isScrolled && "text-[#d3fb52]")}
        aria-hidden="true"
      />
      {score ? (
        <span className="inline-flex items-center gap-1.5 tabular-nums">
          <span>Lvl {score.level}</span>
          <span className="text-white/40" aria-hidden="true">
            ·
          </span>
          <span className={cn("font-mono", isScrolled && "text-[#d3fb52]")}>
            {score.totalPoints} XP
          </span>
        </span>
      ) : (
        <span className={isScrolled ? "text-[#d3fb52]" : undefined}>XP &amp; Badges</span>
      )}
    </Link>
  );
}
