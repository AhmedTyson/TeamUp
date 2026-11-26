/**
 * @date 2026-11-22
 * @purpose Safe Avatar primitive resolving relative asset paths, stored image URLs (/api/v1/images/{id}), and public HTTP URLs via resolveMediaUrl with monogram fallback.
 */
import { useEffect, useState } from "react";
import { cn } from "@/lib/cn";
import { resolveMediaUrl } from "@/lib/urlValidation";

export type AvatarSize = "xs" | "sm" | "md" | "lg" | "xl";

export interface AvatarProps {
  src?: string | null;
  name: string;
  size?: AvatarSize;
  className?: string;
}

const SIZE_CLASSES: Record<AvatarSize, string> = {
  xs: "size-6 text-[10px]",
  sm: "size-8 text-xs",
  md: "size-10 text-sm",
  lg: "size-14 text-base",
  xl: "size-20 text-xl",
};

function getInitials(name: string): string {
  const parts = name
    .trim()
    .split(/\s+/)
    .filter(Boolean);
  if (parts.length === 0) {
    return "?";
  }
  if (parts.length === 1) {
    return parts[0].slice(0, 2).toUpperCase();
  }
  return `${parts[0][0] ?? ""}${parts[parts.length - 1][0] ?? ""}`.toUpperCase();
}

export function Avatar({ src, name, size = "md", className }: AvatarProps) {
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    setFailed(false);
  }, [src]);

  const resolvedSrc = resolveMediaUrl(src);
  const canRenderImage = Boolean(resolvedSrc && !failed);

  return (
    <span
      role="img"
      aria-label={name}
      className={cn(
        "relative inline-flex shrink-0 items-center justify-center overflow-hidden rounded-full border border-white/12 bg-[#222532] font-semibold text-[#d3fb52] select-none",
        SIZE_CLASSES[size],
        className,
      )}
    >
      {canRenderImage && resolvedSrc ? (
        <img
          src={resolvedSrc}
          alt={name}
          loading="lazy"
          decoding="async"
          onError={() => setFailed(true)}
          className="size-full object-cover"
          referrerPolicy="no-referrer"
        />
      ) : (
        <span aria-hidden="true">{getInitials(name)}</span>
      )}
    </span>
  );
}
