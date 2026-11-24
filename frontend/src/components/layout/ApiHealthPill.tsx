/**
 * @date 2026-11-24
 * @purpose Live backend API & database connectivity status pill powered by GET /health.
 */
import { RefreshCw } from "lucide-react";
import { useApiHealthDetails } from "@/hooks/useApiHealth";
import { cn } from "@/lib/cn";
import { useI18n } from "@/lib/i18n";

export interface ApiHealthPillProps {
  className?: string;
}

export function ApiHealthPill({ className }: ApiHealthPillProps) {
  const { state, database, refetch } = useApiHealthDetails();
  const { t } = useI18n();

  const dotClass =
    state === "online"
      ? "bg-emerald-400 shadow-[0_0_8px_rgba(52,211,153,0.75)]"
      : state === "checking"
        ? "bg-amber-400 animate-pulse"
        : state === "degraded"
          ? "bg-amber-400"
          : "bg-rose-400";

  const label =
    state === "online"
      ? t("health.online")
      : state === "checking"
        ? t("health.checking")
        : state === "degraded"
          ? t("health.degraded")
          : t("health.offline");

  return (
    <button
      type="button"
      onClick={() => refetch()}
      title={database ? `API ${state} · DB: ${database}` : `API ${state}`}
      aria-label={`API status: ${label}. Click to re-check.`}
      className={cn(
        "group inline-flex cursor-pointer items-center gap-1.5 rounded-full border border-white/12 bg-white/[0.04] px-2.5 py-1 text-[11px] font-medium text-white/75 transition-colors hover:border-[#d3fb52]/35 hover:text-white",
        className,
      )}
    >
      <span className={cn("size-2 rounded-full", dotClass)} aria-hidden="true" />
      <span>{label}</span>
      <RefreshCw
        className="size-3 opacity-0 transition-opacity group-hover:opacity-80"
        aria-hidden="true"
      />
    </button>
  );
}
