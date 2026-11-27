/**
 * @date 2026-11-22
 * @purpose Badges Gallery component merging master catalog with student earned badges to visualize Unlocked vs. Locked milestones.
 */
import { useQuery } from "@tanstack/react-query";
import { Award, CheckCircle2, Lock } from "lucide-react";
import { useState } from "react";
import { badgesApi, type BadgeDto, type StudentBadgeDto } from "@/api";
import { Badge } from "@/components/ui";
import { formatDateTime } from "@/lib/formatters";
import { queryKeys } from "@/lib/queryKeys";
import { resolveMediaUrl } from "@/lib/urlValidation";

export interface BadgesGalleryProps {
  studentId: number;
  catalog?: BadgeDto[];
  earnedBadges?: StudentBadgeDto[];
}

function BadgeIcon({
  src,
  alt,
  isUnlocked,
}: {
  src: string | null;
  alt: string;
  isUnlocked: boolean;
}) {
  const [failedUrl, setFailedUrl] = useState<string | null>(null);
  const canRenderImage = Boolean(src && failedUrl !== src);

  if (canRenderImage && src) {
    return (
      <img
        src={src}
        alt={alt}
        loading="lazy"
        decoding="async"
        referrerPolicy="no-referrer"
        onError={() => setFailedUrl(src)}
        className="size-7 rounded-lg object-contain"
      />
    );
  }

  return isUnlocked ? (
    <Award className="size-6" aria-hidden="true" />
  ) : (
    <Lock className="size-5" aria-hidden="true" />
  );
}

export function BadgesGallery({
  studentId,
  catalog: catalogProp,
  earnedBadges: earnedProp,
}: BadgesGalleryProps) {
  const [filter, setFilter] = useState<"all" | "unlocked" | "locked">("all");

  const catalogQuery = useQuery({
    queryKey: queryKeys.badges.catalog,
    queryFn: ({ signal }) => badgesApi.getBadgeCatalog(signal),
    enabled: catalogProp === undefined,
  });

  const studentBadgesQuery = useQuery({
    queryKey: queryKeys.badges.student(studentId),
    queryFn: ({ signal }) => badgesApi.getStudentBadges(studentId, signal),
    enabled: studentId > 0 && earnedProp === undefined,
  });

  const catalog = catalogProp ?? catalogQuery.data ?? [];
  const earnedBadges = earnedProp ?? studentBadgesQuery.data ?? [];

  const earnedMap = new Map(earnedBadges.map((b) => [b.badgeId, b]));

  const mergedBadges = catalog.map((badge) => {
    const earned = earnedMap.get(badge.badgeId);
    return {
      ...badge,
      isUnlocked: Boolean(earned),
      awardedAt: earned?.awardedAt ?? null,
    };
  });

  const filtered = mergedBadges.filter((b) => {
    if (filter === "unlocked") return b.isUnlocked;
    if (filter === "locked") return !b.isUnlocked;
    return true;
  });

  const unlockedCount = mergedBadges.filter((b) => b.isUnlocked).length;

  return (
    <section className="space-y-6 rounded-3xl border border-white/[0.085] bg-[#1b1d26] p-5 sm:p-7">
      <div className="flex flex-col justify-between gap-4 border-b border-white/[0.08] pb-4 sm:flex-row sm:items-center">
        <div>
          <div className="flex items-center gap-2">
            <Award className="size-5 text-[#d3fb52]" aria-hidden="true" />
            <h2
              className="text-lg font-bold text-white uppercase tracking-wider"
              style={{ fontFamily: "var(--font-heading)" }}
            >
              Achievement Badges &amp; Milestone Shelf
            </h2>
          </div>
          <p className="text-xs text-[#9ca3af]">
            Permanent badges unlocked through points, workshops, and verified sprint contributions.
          </p>
        </div>

        {/* Filter Pills */}
        <div className="flex items-center gap-1.5 rounded-xl border border-white/10 bg-[#14151c] p-1">
          <button
            type="button"
            onClick={() => setFilter("all")}
            className={`rounded-lg px-3 py-1 text-xs font-semibold transition-colors cursor-pointer ${
              filter === "all" ? "bg-[#d3fb52] text-[#14151c]" : "text-white/70 hover:text-white"
            }`}
          >
            All ({mergedBadges.length})
          </button>
          <button
            type="button"
            onClick={() => setFilter("unlocked")}
            className={`rounded-lg px-3 py-1 text-xs font-semibold transition-colors cursor-pointer ${
              filter === "unlocked" ? "bg-[#10b981] text-[#14151c]" : "text-white/70 hover:text-white"
            }`}
          >
            Unlocked ({unlockedCount})
          </button>
          <button
            type="button"
            onClick={() => setFilter("locked")}
            className={`rounded-lg px-3 py-1 text-xs font-semibold transition-colors cursor-pointer ${
              filter === "locked" ? "bg-white/20 text-white" : "text-white/70 hover:text-white"
            }`}
          >
            Locked ({mergedBadges.length - unlockedCount})
          </button>
        </div>
      </div>

      {/* Badges Grid */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {filtered.map((badge) => {
          const resolvedIconUrl = resolveMediaUrl(badge.iconUrl);
          return (
            <div
              key={badge.badgeId}
              className={`relative flex flex-col justify-between rounded-2xl border p-5 transition-all ${
                badge.isUnlocked
                  ? "border-[#d3fb52]/40 bg-[#14151c] shadow-[0_0_20px_rgba(211,251,82,0.08)]"
                  : "border-white/[0.06] bg-[#14151c]/60 opacity-80"
              }`}
            >
              <div className="space-y-3">
                <div className="flex items-start justify-between gap-3">
                  <div
                    className={`flex size-11 items-center justify-center rounded-xl ${
                      badge.isUnlocked
                        ? "bg-[#d3fb52] text-[#14151c]"
                        : "bg-white/10 text-white/40"
                    }`}
                  >
                    <BadgeIcon
                      src={resolvedIconUrl}
                      alt={badge.name}
                      isUnlocked={badge.isUnlocked}
                    />
                  </div>
                  {badge.isUnlocked ? (
                    <Badge variant="accent" size="sm" className="gap-1">
                      <CheckCircle2 className="size-3" aria-hidden="true" />
                      Unlocked
                    </Badge>
                  ) : (
                    <Badge variant="default" size="sm" className="gap-1">
                      <Lock className="size-3" aria-hidden="true" />
                      Locked
                    </Badge>
                  )}
                </div>

                <div>
                  <h3 className="text-base font-bold text-white">{badge.name}</h3>
                  <p className="mt-1 text-xs text-[#9ca3af] leading-relaxed">
                    {badge.description}
                  </p>
                </div>
              </div>

              {/* Criteria & Awarded Footer */}
              <div className="mt-4 border-t border-white/[0.06] pt-3 text-[11px]">
                {badge.isUnlocked && badge.awardedAt ? (
                  <div className="flex items-center justify-between text-white/70">
                    <span>Awarded:</span>
                    <span className="font-mono text-[#d3fb52]">
                      {formatDateTime(badge.awardedAt)}
                    </span>
                  </div>
                ) : (
                  <div className="flex items-center justify-between text-[#9ca3af]">
                    <span>Criterion:</span>
                    <span className="font-mono font-semibold text-white/90">
                      {badge.criteriaType}: {badge.criteriaValue}
                    </span>
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </section>
  );
}
