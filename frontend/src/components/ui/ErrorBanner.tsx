/**
 * @date 2026-11-21
 * @purpose Accessible ErrorBanner and ForbiddenBanner primitives handling ApiClientError, 403 permissions, and 429 Retry-After hints.
 */
import { AlertTriangle, Clock, RefreshCw, ShieldAlert } from "lucide-react";
import { ApiClientError } from "@/api/client";
import { cn } from "@/lib/cn";
import { Button } from "./Button";

export type ErrorBannerVariant = "error" | "forbidden" | "rateLimit" | "warning";

export interface ErrorBannerProps {
  error?: unknown;
  title?: string;
  message?: string;
  variant?: ErrorBannerVariant;
  retryAfterSeconds?: number;
  onRetry?: () => void;
  className?: string;
}

const VARIANT_STYLES: Record<
  ErrorBannerVariant,
  { border: string; bg: string; iconColor: string; titleColor: string }
> = {
  error: {
    border: "border-red-500/35",
    bg: "bg-red-500/10",
    iconColor: "text-red-400",
    titleColor: "text-red-200",
  },
  forbidden: {
    border: "border-amber-500/35",
    bg: "bg-amber-500/10",
    iconColor: "text-amber-300",
    titleColor: "text-amber-200",
  },
  rateLimit: {
    border: "border-sky-400/35",
    bg: "bg-sky-400/10",
    iconColor: "text-sky-300",
    titleColor: "text-sky-200",
  },
  warning: {
    border: "border-amber-500/35",
    bg: "bg-amber-500/10",
    iconColor: "text-amber-300",
    titleColor: "text-amber-200",
  },
};

export function ErrorBanner({
  error,
  title,
  message,
  variant,
  retryAfterSeconds,
  onRetry,
  className,
}: ErrorBannerProps) {
  const apiError = error instanceof ApiClientError ? error : null;

  const resolvedVariant: ErrorBannerVariant =
    variant ??
    (apiError?.isForbidden
      ? "forbidden"
      : apiError?.isRateLimited
        ? "rateLimit"
        : "error");

  const resolvedRetryAfter = retryAfterSeconds ?? apiError?.retryAfterSeconds;

  const resolvedTitle =
    title ??
    (resolvedVariant === "forbidden"
      ? "Access denied"
      : resolvedVariant === "rateLimit"
        ? "Too many requests"
        : "Something went wrong");

  const resolvedMessage =
    message ??
    (apiError?.message ||
      (error instanceof Error ? error.message : typeof error === "string" ? error : undefined)) ??
    "Unable to complete the request. Please try again.";

  const styles = VARIANT_STYLES[resolvedVariant];
  const Icon =
    resolvedVariant === "forbidden"
      ? ShieldAlert
      : resolvedVariant === "rateLimit"
        ? Clock
        : AlertTriangle;

  return (
    <div
      role="alert"
      className={cn(
        "flex flex-col justify-between gap-4 rounded-2xl border p-4 sm:flex-row sm:items-center",
        styles.border,
        styles.bg,
        className,
      )}
    >
      <div className="flex items-start gap-3">
        <Icon className={cn("mt-0.5 size-5 shrink-0", styles.iconColor)} aria-hidden="true" />
        <div className="space-y-1">
          <p className={cn("text-sm font-semibold", styles.titleColor)}>{resolvedTitle}</p>
          <p className="text-sm text-white/80">{resolvedMessage}</p>
          {resolvedRetryAfter !== undefined && resolvedRetryAfter > 0 && (
            <p className="text-xs font-medium text-sky-300 tabular-nums">
              Please wait approximately {resolvedRetryAfter}s before trying again.
            </p>
          )}
        </div>
      </div>

      {onRetry && (
        <Button
          variant="secondary"
          size="sm"
          onClick={onRetry}
          leftIcon={<RefreshCw className="size-3.5" aria-hidden="true" />}
        >
          Retry
        </Button>
      )}
    </div>
  );
}

export function ForbiddenBanner({
  message = "You do not have permission to view or modify this resource.",
  className,
}: {
  message?: string;
  className?: string;
}) {
  return (
    <ErrorBanner
      variant="forbidden"
      title="Access restricted"
      message={message}
      className={className}
    />
  );
}
