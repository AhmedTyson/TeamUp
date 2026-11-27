/**
 * @date 2026-11-27
 * @purpose Shared helper to compute clamped integer level progress percentage across 0..100 backend and 0..1 mock DTOs.
 */
import type { StudentScoreDto } from "@/api";

export function computeLevelProgressPercent(
  score: Pick<StudentScoreDto, "totalPoints" | "currentLevelStart" | "nextLevelThreshold" | "progress">,
): number {
  const span = score.nextLevelThreshold - score.currentLevelStart;
  if (span > 0) {
    const raw = ((Math.max(score.totalPoints, 0) - score.currentLevelStart) / span) * 100;
    return Math.min(100, Math.max(0, Math.round(raw)));
  }

  const normalized = score.progress > 1 ? score.progress : score.progress * 100;
  return Math.min(100, Math.max(0, Math.round(normalized)));
}
