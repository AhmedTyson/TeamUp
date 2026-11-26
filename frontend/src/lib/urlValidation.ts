/**
 * @date 2026-11-21
 * @purpose Client-side safe external URL validator mirroring backend SafeUrlValidator (STRIDE THR-T-03).
 */
import { z } from "@/lib/zodCspConfig";
import { env } from "@/lib/env";

const BLOCKED_HOSTNAMES = new Set(["localhost", "127.0.0.1", "0.0.0.0", "::1", "[::1]"]);
const STORED_IMAGE_PATH_RE =
  /^\/api\/v1\/images\/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export function isSafeHttpUrl(value: string | null | undefined): boolean {
  if (!value) {
    return false;
  }
  const trimmed = value.trim();
  if (!/^https?:\/\//i.test(trimmed)) {
    return false;
  }

  try {
    const parsed = new URL(trimmed);
    if (parsed.protocol !== "http:" && parsed.protocol !== "https:") {
      return false;
    }
    const hostname = parsed.hostname.toLowerCase().replace(/\.+$/, "");
    if (
      !hostname ||
      BLOCKED_HOSTNAMES.has(hostname) ||
      hostname.endsWith(".local") ||
      hostname.endsWith(".localhost")
    ) {
      return false;
    }
    return true;
  } catch {
    return false;
  }
}

export function isSafeAssetOrHttpUrl(value: string | null | undefined): boolean {
  if (!value) {
    return false;
  }
  const trimmed = value.trim();
  if (trimmed.startsWith("/")) {
    if (
      trimmed.startsWith("//") ||
      trimmed.includes("\\") ||
      trimmed.includes("..") ||
      /[\x00-\x1f\x7f]/.test(trimmed)
    ) {
      return false;
    }
    return (
      STORED_IMAGE_PATH_RE.test(trimmed) ||
      trimmed.startsWith("/images/") ||
      trimmed.startsWith("/logos/") ||
      trimmed.startsWith("/badges/")
    );
  }
  return isSafeHttpUrl(trimmed);
}

/**
 * Resolves uploaded backend media paths (/images/people/student-* and /api/v1/images/*) against
 * VITE_API_BASE_URL when the SPA runs on a separate origin, while keeping seeded static assets local.
 */
export function resolveMediaUrl(
  value: string | null | undefined,
  apiBaseUrl: string = env.apiBaseUrl,
): string | null {
  if (!value) {
    return null;
  }
  const trimmed = value.trim();
  if (!isSafeAssetOrHttpUrl(trimmed)) {
    return null;
  }
  const trimmedBase = apiBaseUrl.trim();
  if (trimmedBase.length > 0 && (trimmed.startsWith("/images/people/student-") || trimmed.startsWith("/api/"))) {
    try {
      const parsedBase = new URL(trimmedBase);
      if (parsedBase.protocol === "http:" || parsedBase.protocol === "https:") {
        return `${parsedBase.origin}${trimmed}`;
      }
      return trimmed;
    } catch {
      const normalizedBase = trimmedBase.replace(/\/+$/, "");
      if (!trimmed.startsWith("/api/") || normalizedBase.length === 0) {
        return trimmed;
      }
      if (normalizedBase === "/api" || normalizedBase.endsWith("/api")) {
        return trimmed;
      }
      return `${normalizedBase}${trimmed}`;
    }
  }
  return trimmed;
}

export const safeUrlSchema = z
  .string()
  .trim()
  .refine((val) => isSafeHttpUrl(val), {
    message: "Must be a valid public http:// or https:// URL.",
  });

export const optionalSafeUrlSchema = z
  .string()
  .trim()
  .refine((val) => val.length === 0 || isSafeHttpUrl(val), {
    message: "Must be a valid public http:// or https:// URL.",
  })
  .optional();

export const optionalSafeAssetOrUrlSchema = z
  .string()
  .trim()
  .refine((val) => val.length === 0 || isSafeAssetOrHttpUrl(val), {
    message: "Must be a valid asset path or public http:// / https:// URL.",
  })
  .optional();

