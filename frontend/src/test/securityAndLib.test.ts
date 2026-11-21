/**
 * @date 2026-11-21
 * @purpose Unit tests for JWT claim decoding (.NET WS-Fed + standard claims), safe URL validation, form error mapping, and query invalidation.
 */
import { describe, expect, it, vi } from "vitest";
import { ApiClientError } from "@/api";
import { mapApiErrorsToForm } from "@/lib/formErrors";
import { decodeJwtPayload, isTokenExpired } from "@/lib/jwt";
import { createAppQueryClient, shouldRetryApiError } from "@/lib/queryClient";
import { invalidateGamification, queryKeys } from "@/lib/queryKeys";
import { isSafeHttpUrl, optionalSafeUrlSchema, safeUrlSchema } from "@/lib/urlValidation";

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
});
