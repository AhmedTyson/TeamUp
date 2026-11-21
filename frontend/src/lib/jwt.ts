/**
 * @date 2026-11-21
 * @purpose Safe client-side JWT payload decoder for TeamUp UI role hints (sub, role, org_admin[], exp).
 *          Never relied on for authorization enforcement; backend validates signature & SecurityStamp on every request.
 */
import { USER_ROLES, type UserRole } from "@/api/enums";

const WS_CLAIM_NAME_ID = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
const WS_CLAIM_EMAIL = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
const WS_CLAIM_NAME = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
const WS_CLAIM_ROLE = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export interface DecodedJwtClaims {
  studentId: number | null;
  email: string | null;
  fullName: string | null;
  role: UserRole | null;
  orgAdminIds: number[];
  securityStamp: string | null;
  exp: number | null;
}

function decodeBase64Url(segment: string): string | null {
  try {
    const base64 = segment.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
    const binary = atob(padded);
    const bytes = Uint8Array.from(binary, (char) => char.charCodeAt(0));
    return new TextDecoder().decode(bytes);
  } catch {
    return null;
  }
}

function parsePositiveInt(value: unknown): number | null {
  if (typeof value === "number" && Number.isInteger(value) && value > 0) {
    return value;
  }
  if (typeof value === "string" && /^\d+$/.test(value.trim())) {
    const parsed = Number.parseInt(value.trim(), 10);
    return parsed > 0 ? parsed : null;
  }
  return null;
}

function normalizeRole(value: unknown): UserRole | null {
  if (typeof value !== "string") {
    return null;
  }
  const normalized = value.trim();
  if (
    normalized === USER_ROLES.Student ||
    normalized === USER_ROLES.OrgAdmin ||
    normalized === USER_ROLES.PlatformAdmin
  ) {
    return normalized;
  }
  return null;
}

function normalizeOrgAdminIds(raw: unknown): number[] {
  const items = Array.isArray(raw) ? raw : raw !== undefined && raw !== null ? [raw] : [];
  const ids = new Set<number>();

  for (const item of items) {
    const parsed = parsePositiveInt(item);
    if (parsed !== null) {
      ids.add(parsed);
    }
  }

  return Array.from(ids);
}

export function decodeJwtPayload(token: string | null | undefined): DecodedJwtClaims | null {
  if (!token || typeof token !== "string") {
    return null;
  }

  const segments = token.split(".");
  if (segments.length !== 3 || !segments[1]) {
    return null;
  }

  const json = decodeBase64Url(segments[1]);
  if (!json) {
    return null;
  }

  try {
    const raw = JSON.parse(json) as Record<string, unknown>;
    const rawSub = raw.sub ?? raw[WS_CLAIM_NAME_ID];
    const rawEmail = raw.email ?? raw[WS_CLAIM_EMAIL];
    const rawName = raw.name ?? raw[WS_CLAIM_NAME];
    const rawRole = raw.role ?? raw[WS_CLAIM_ROLE];
    const rawExp = raw.exp;

    return {
      studentId: parsePositiveInt(rawSub),
      email: typeof rawEmail === "string" ? rawEmail : null,
      fullName: typeof rawName === "string" ? rawName : null,
      role: normalizeRole(rawRole),
      orgAdminIds: normalizeOrgAdminIds(raw.org_admin),
      securityStamp: typeof raw.security_stamp === "string" ? raw.security_stamp : null,
      exp: typeof rawExp === "number" && Number.isFinite(rawExp) ? rawExp : null,
    };
  } catch {
    return null;
  }
}

export function isTokenExpired(token: string | null | undefined, skewSeconds = 15): boolean {
  const claims = decodeJwtPayload(token);
  if (!claims || claims.exp === null) {
    return true;
  }
  const nowSeconds = Math.floor(Date.now() / 1000);
  return claims.exp <= nowSeconds + skewSeconds;
}
