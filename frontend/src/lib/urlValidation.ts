/**
 * @date 2026-11-21
 * @purpose Client-side safe external URL validator mirroring backend SafeUrlValidator (STRIDE THR-T-03).
 */
import { z } from "zod";

const BLOCKED_HOSTNAMES = new Set(["localhost", "127.0.0.1", "0.0.0.0", "::1", "[::1]"]);

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
    const hostname = parsed.hostname.toLowerCase();
    if (!hostname || BLOCKED_HOSTNAMES.has(hostname) || hostname.endsWith(".local")) {
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
  if (trimmed.startsWith("/") && !trimmed.startsWith("//")) {
    return !trimmed.includes("..") && !/[\x00-\x1f\x7f]/.test(trimmed);
  }
  return isSafeHttpUrl(trimmed);
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
  .optional()
  .or(z.literal(""));
