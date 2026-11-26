/**
 * @date 2026-11-24
 * @purpose Safe organization emblem displaying logo URLs resolved via resolveMediaUrl (including /api/v1/images/{id}) with clean initial fallback.
 */
import { useState } from "react";
import { cn } from "@/lib/cn";
import { resolveMediaUrl } from "@/lib/urlValidation";

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
  const [failedUrl, setFailedUrl] = useState<string | null>(null);
  const resolvedLogoUrl = resolveMediaUrl(logoUrl);
  const canRenderLogo = Boolean(resolvedLogoUrl && failedUrl !== resolvedLogoUrl);

  return (
    <div
      className={cn(
        "flex shrink-0 items-center justify-center overflow-hidden border",
        canRenderLogo
          ? "border-black/10 bg-white p-1.5 shadow-sm"
          : "border-white/10 bg-[#d3fb52]/10 font-bold text-[#d3fb52]",
        className,
      )}
    >
      {canRenderLogo && resolvedLogoUrl ? (
        <img
          src={resolvedLogoUrl}
          alt={`${name} logo`}
          loading="lazy"
          decoding="async"
          referrerPolicy="no-referrer"
          className="size-full object-contain"
          onError={() => setFailedUrl(resolvedLogoUrl)}
        />
      ) : (
        name.trim().slice(0, 1).toUpperCase() || "O"
      )}
    </div>
  );
}
