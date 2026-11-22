/**
 * @date 2026-11-22
 * @purpose Renders official database-driven university logo images with verified official raster backup and initials fallback.
 */
import { useEffect, useState } from "react";
import { cn } from "@/lib/cn";
import { isSafeAssetOrHttpUrl } from "@/lib/urlValidation";

export interface UniversityLogoProps {
  name: string;
  logoUrl?: string | null;
  size?: "sm" | "md" | "lg";
  className?: string;
}

const SIZE_CLASSES: Record<NonNullable<UniversityLogoProps["size"]>, string> = {
  sm: "size-8 rounded-lg",
  md: "size-11 rounded-xl",
  lg: "size-14 rounded-2xl",
};

const OFFICIAL_BACKUP_ASSETS: ReadonlyArray<{ match: string; path: string }> = [
  { match: "cairo", path: "/logos/universities/official/cairo-university.png" },
  { match: "ain shams", path: "/logos/universities/official/ain-shams-university.png" },
  { match: "alexandria", path: "/logos/universities/official/alexandria-university.png" },
  { match: "mansoura", path: "/logos/universities/official/mansoura-university.png" },
  { match: "assiut", path: "/logos/universities/official/assiut-university.png" },
  { match: "capital", path: "/logos/universities/official/capital-university.png" },
  { match: "helwan", path: "/logos/universities/official/capital-university.png" },
  { match: "zagazig", path: "/logos/universities/official/zagazig-university.png" },
  { match: "tanta", path: "/logos/universities/official/tanta-university.png" },
  { match: "benha", path: "/logos/universities/official/benha-university.png" },
  { match: "suez", path: "/logos/universities/official/suez-canal-university.png" },
  { match: "guc", path: "/logos/universities/official/guc.png" },
  { match: "german university", path: "/logos/universities/official/guc.png" },
  { match: "auc", path: "/logos/universities/official/auc.png" },
  { match: "american university", path: "/logos/universities/official/auc.png" },
];

function getInitials(name: string): string {
  const words = name
    .trim()
    .split(/\s+/)
    .filter((w) => w.length > 0 && !/^(of|and|for|the|in)$/i.test(w));
  if (words.length === 0) {
    return "TU";
  }
  if (words.length === 1) {
    return words[0].slice(0, 2).toUpperCase();
  }
  return `${words[0][0]}${words[1][0]}`.toUpperCase();
}

function getOfficialBackupPath(name: string): string | null {
  const lower = name.toLowerCase();
  return OFFICIAL_BACKUP_ASSETS.find((entry) => lower.includes(entry.match))?.path ?? null;
}

export function UniversityLogo({ name, logoUrl, size = "md", className }: UniversityLogoProps) {
  const [primaryFailed, setPrimaryFailed] = useState(false);
  const [backupFailed, setBackupFailed] = useState(false);

  useEffect(() => {
    setPrimaryFailed(false);
    setBackupFailed(false);
  }, [logoUrl, name]);

  const boxClass = SIZE_CLASSES[size];
  const validPrimaryUrl = logoUrl && isSafeAssetOrHttpUrl(logoUrl) ? logoUrl.trim() : null;
  const backupPath = getOfficialBackupPath(name);

  if (validPrimaryUrl && !primaryFailed) {
    return (
      <div
        title={name}
        className={cn(
          "inline-flex shrink-0 items-center justify-center overflow-hidden border border-black/10 bg-white p-1 shadow-sm",
          boxClass,
          className,
        )}
      >
        <img
          src={validPrimaryUrl}
          alt={`${name} official logo`}
          decoding="async"
          referrerPolicy="no-referrer"
          onError={() => setPrimaryFailed(true)}
          className="h-full w-full object-contain"
        />
      </div>
    );
  }

  // Only attempt cached official raster image when primary was missing cleanly or remote failed transiently,
  // unless an explicit unsafe URL was passed (in which case go straight to initials shield).
  const isUnsafeExplicitUrl = Boolean(logoUrl && !validPrimaryUrl);
  if (!isUnsafeExplicitUrl && backupPath && !backupFailed) {
    return (
      <div
        title={name}
        className={cn(
          "inline-flex shrink-0 items-center justify-center overflow-hidden border border-black/10 bg-white p-1 shadow-sm",
          boxClass,
          className,
        )}
      >
        <img
          src={backupPath}
          alt={`${name} official logo`}
          decoding="async"
          onError={() => setBackupFailed(true)}
          className="h-full w-full object-contain"
        />
      </div>
    );
  }

  const initials = getInitials(name);
  return (
    <div
      role="img"
      aria-label={`${name} crest`}
      title={name}
      className={cn(
        "inline-flex shrink-0 items-center justify-center border border-white/15 bg-gradient-to-br from-[#272a3a] via-[#1c1e2a] to-[#14151c] text-[#d3fb52] shadow-sm",
        boxClass,
        className,
      )}
    >
      <span className="font-mono text-[11px] font-bold tracking-tight">{initials}</span>
    </div>
  );
}
