/**
 * @date 2026-11-25
 * @purpose Shared card cover band: one consistent aspect ratio, crop and corner treatment for organization,
 * event and project covers across every directory. Hides itself cleanly if the URL is missing or fails to load.
 */
import { useState } from "react";
import { cn } from "@/lib/cn";
import { resolveMediaUrl } from "@/lib/urlValidation";

export interface CoverImageProps {
  url: string | null | undefined;
  alt?: string;
  className?: string;
}

export function CoverImage({ url, alt = "", className }: CoverImageProps) {
  const [failedUrl, setFailedUrl] = useState<string | null>(null);
  const src = resolveMediaUrl(url);
  if (!src || failedUrl === src) return null;

  return (
    <div className={cn("aspect-[2/1] overflow-hidden", className)}>
      <img
        src={src}
        alt={alt}
        loading="lazy"
        decoding="async"
        referrerPolicy="no-referrer"
        onError={() => setFailedUrl(src)}
        className="h-full w-full object-cover"
      />
    </div>
  );
}
