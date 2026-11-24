/**
 * @date 2026-11-21
 * @purpose Unit tests for JWT claim decoding (.NET WS-Fed + standard claims), safe URL validation, form error mapping, and query invalidation.
 */
import fs from "node:fs";
import path from "node:path";
import { describe, expect, it, vi } from "vitest";
import { ApiClientError } from "@/api";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import { computeJsonDiffEntries, formatDateTime } from "@/lib/formatters";
import { decodeJwtPayload, isTokenExpired } from "@/lib/jwt";
import { createAppQueryClient, shouldRetryApiError } from "@/lib/queryClient";
import { invalidateGamification, queryKeys } from "@/lib/queryKeys";
import {
  isSafeHttpUrl,
  optionalSafeUrlSchema,
  resolveMediaUrl,
  safeUrlSchema,
} from "@/lib/urlValidation";

function createFakeJwt(payload: Record<string, unknown>): string {
  const encode = (obj: object) =>
    btoa(JSON.stringify(obj)).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "");
  return `${encode({ alg: "HS256", typ: "JWT" })}.${encode(payload)}.signature`;
}

describe("JWT claim decoding", () => {
  it("decodes standard and .NET WS-Federation claims with multiple org_admin IDs", () => {
    const futureExp = Math.floor(Date.now() / 1000) + 900;
    const token = createFakeJwt({
      sub: "42",
      email: "student@teamup.edu",
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "Ahmed Elsayed",
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "OrgAdmin",
      org_admin: ["10", "25"],
      security_stamp: "11111111-1111-1111-1111-111111111111",
      exp: futureExp,
    });

    const claims = decodeJwtPayload(token);
    expect(claims).toEqual({
      studentId: 42,
      email: "student@teamup.edu",
      fullName: "Ahmed Elsayed",
      role: "OrgAdmin",
      orgAdminIds: [10, 25],
      securityStamp: "11111111-1111-1111-1111-111111111111",
      exp: futureExp,
    });
    expect(isTokenExpired(token)).toBe(false);
  });

  it("normalizes single string org_admin claim and flags expired/malformed tokens", () => {
    const pastExp = Math.floor(Date.now() / 1000) - 60;
    const token = createFakeJwt({
      sub: "7",
      role: "Student",
      org_admin: "3",
      exp: pastExp,
    });

    const claims = decodeJwtPayload(token);
    expect(claims?.orgAdminIds).toEqual([3]);
    expect(isTokenExpired(token)).toBe(true);
    expect(decodeJwtPayload("not-a-jwt")).toBeNull();
    expect(isTokenExpired(null)).toBe(true);
  });
});

describe("Safe URL validation (STRIDE THR-T-03)", () => {
  it("accepts public http/https URLs and rejects unsafe schemes or loopback hosts", () => {
    expect(isSafeHttpUrl("https://github.com/AhmedTyson")).toBe(true);
    expect(isSafeHttpUrl("http://example.edu/club")).toBe(true);

    expect(isSafeHttpUrl("javascript:alert(1)")).toBe(false);
    expect(isSafeHttpUrl("data:text/html,<script>alert(1)</script>")).toBe(false);
    expect(isSafeHttpUrl("http://localhost:5186/api")).toBe(false);
    expect(isSafeHttpUrl("http://127.0.0.1/internal")).toBe(false);
    expect(isSafeHttpUrl("ftp://files.example.com")).toBe(false);

    expect(safeUrlSchema.safeParse("https://linkedin.com/in/student").success).toBe(true);
    expect(safeUrlSchema.safeParse("javascript:void(0)").success).toBe(false);
    expect(optionalSafeUrlSchema.safeParse("").success).toBe(true);
  });

  it("resolves uploaded backend portrait paths against VITE_API_BASE_URL while keeping seeded paths local", () => {
    expect(resolveMediaUrl("/images/people/salma-khaled.jpg", "https://teamup-api.runasp.net")).toBe(
      "/images/people/salma-khaled.jpg",
    );
    expect(
      resolveMediaUrl("/images/people/student-10-abc.jpg", "https://teamup-api.runasp.net/api"),
    ).toBe("https://teamup-api.runasp.net/images/people/student-10-abc.jpg");
    expect(resolveMediaUrl("/images/people/student-10-abc.jpg", "")).toBe(
      "/images/people/student-10-abc.jpg",
    );
    expect(resolveMediaUrl("//evil.example.com/a.jpg", "https://teamup-api.runasp.net")).toBeNull();
    expect(resolveMediaUrl("/images/../secret.jpg", "https://teamup-api.runasp.net")).toBeNull();
  });
});

describe("Form error mapping & query helpers", () => {
  it("maps PascalCase backend FluentValidation errors to camelCase react-hook-form fields", () => {
    const setError = vi.fn();
    const error = new ApiClientError("Validation failed", 400, [
      { field: "Email", message: "Email is already registered." },
      { field: "AcademicYear", message: "Academic year must be between 1 and 7." },
    ]);

    const mapped = mapApiErrorsToForm<{ email: string; academicYear: number }>(
      error,
      setError,
      ["email", "academicYear"],
    );

    expect(mapped).toBe(2);
    expect(setError).toHaveBeenCalledWith("email", {
      type: "server",
      message: "Email is already registered.",
    });
    expect(setError).toHaveBeenCalledWith("academicYear", {
      type: "server",
      message: "Academic year must be between 1 and 7.",
    });
  });

  it("avoids retrying client 4xx/429 errors and invalidates all gamification caches", async () => {
    expect(shouldRetryApiError(0, new ApiClientError("Forbidden", 403))).toBe(false);
    expect(shouldRetryApiError(0, new ApiClientError("Rate limited", 429))).toBe(false);
    expect(shouldRetryApiError(0, new ApiClientError("Server error", 500))).toBe(true);
    expect(shouldRetryApiError(1, new ApiClientError("Server error", 500))).toBe(false);

    const queryClient = createAppQueryClient();
    const spy = vi.spyOn(queryClient, "invalidateQueries");
    await invalidateGamification(queryClient, 42);

    expect(spy).toHaveBeenCalledWith({ queryKey: queryKeys.gamification.all });
    expect(spy).toHaveBeenCalledWith({ queryKey: queryKeys.badges.all });
    expect(spy).toHaveBeenCalledWith({ queryKey: queryKeys.notifications.all });
    expect(spy).toHaveBeenCalledWith({ queryKey: queryKeys.gamification.score(42) });
  });

  it("formats timestamps consistently and computes structured JSON audit diffs", () => {
    expect(formatDateTime(null)).toBe("—");
    expect(formatDateTime("2026-11-22T01:05:00Z")).toMatch(/Nov \d{2}, 2026 · \d{2}:\d{2}/);

    const diffs = computeJsonDiffEntries(
      JSON.stringify({ Role: "Student", Email: "a@cu.edu.eg" }),
      JSON.stringify({ Role: "OrgAdmin", Email: "a@cu.edu.eg" }),
    );
    expect(diffs).toEqual([
      { field: "Role", before: "Student", after: "OrgAdmin", changed: true },
      { field: "Email", before: "a@cu.edu.eg", after: "a@cu.edu.eg", changed: false },
    ]);
  });
});

describe("Deployment security headers & CSP hygiene", () => {
  it("enforces matching strict CSP and security headers across Vercel and Netlify without inline scripts in index.html", () => {
    const frontendRoot = path.resolve(import.meta.dirname, "../..");
    const repoRoot = path.resolve(frontendRoot, "..");

    const vercelRaw = fs.readFileSync(path.join(frontendRoot, "vercel.json"), "utf-8");
    const vercelJson = JSON.parse(vercelRaw) as {
      headers?: Array<{
        source: string;
        headers: Array<{ key: string; value: string }>;
      }>;
    };

    const catchAllRule = vercelJson.headers?.find((entry) => entry.source === "/(.*)");
    expect(catchAllRule).toBeDefined();

    const headerMap = new Map(catchAllRule?.headers.map((h) => [h.key, h.value]));
    const requiredHeaders = [
      "Content-Security-Policy",
      "X-Content-Type-Options",
      "X-Frame-Options",
      "Referrer-Policy",
      "Permissions-Policy",
      "Strict-Transport-Security",
    ];
    for (const key of requiredHeaders) {
      expect(headerMap.get(key)).toBeTruthy();
    }

    const csp = headerMap.get("Content-Security-Policy")!;
    expect(csp).toContain("default-src 'self'");
    expect(csp).toContain("script-src 'self'");
    expect(csp).not.toMatch(/script-src[^;]*'unsafe-inline'/);
    expect(csp).not.toMatch(/script-src[^;]*'unsafe-eval'/);
    expect(csp).toContain("object-src 'none'");
    expect(csp).toContain("frame-ancestors 'none'");
    expect(csp).toContain("frame-src 'self' blob:");
    expect(csp).toContain("https://teamup-api.runasp.net");

    const netlifyToml = fs.readFileSync(path.join(repoRoot, "netlify.toml"), "utf-8");
    expect(netlifyToml).toContain(`Content-Security-Policy = "${csp}"`);
    expect(netlifyToml).toContain('Permissions-Policy = "camera=(self), microphone=(), geolocation=(), payment=()"');
    expect(netlifyToml).toContain('Strict-Transport-Security = "max-age=31536000; includeSubDomains"');

    const indexHtml = fs.readFileSync(path.join(frontendRoot, "index.html"), "utf-8");
    const scriptTags = [...indexHtml.matchAll(/<script\b([^>]*)>([\s\S]*?)<\/script>/gi)];
    expect(scriptTags.length).toBeGreaterThan(0);
    for (const [, attrs, body] of scriptTags) {
      expect(attrs).toMatch(/\bsrc\s*=/i);
      expect(body.trim()).toBe("");
    }
  });
});
