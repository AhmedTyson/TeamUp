/**
 * @date 2026-11-24
 * @purpose Safe organization avatar/emblem displaying validated external logoUrl with clean initial fallback.
 */
import { useState } from "react";
import { cn } from "@/lib/cn";
import { isSafeHttpUrl } from "@/lib/urlValidation";

export interface OrganizationMarkProps {
  name: string;
  logoUrl: string | null;
  className?: string;
}

export function OrganizationMark({
  name,
  logoUrl,
  className = "size-12 rounded-2xl text-lg",
}: OrganizationMarkProps) {
  const [imageError, setImageError] = useState(false);
  const canRenderLogo = Boolean(logoUrl && isSafeHttpUrl(logoUrl) && !imageError);

  return (
    <div
      className={cn(
        "flex shrink-0 items-center justify-center overflow-hidden border border-white/10 bg-[#d3fb52]/10 font-bold text-[#d3fb52]",
        className,
      )}
    >
      {canRenderLogo && logoUrl ? (
        <img
          src={logoUrl}
          alt={`${name} logo`}
          className="size-full object-cover"
          onError={() => setImageError(true)}
        />
      ) : (
        name.trim().slice(0, 1).toUpperCase() || "O"
      )}
    </div>
  );
}
