/**
 * @date 2026-11-24
 * @purpose Scoped per-organization authorization helpers and student self-application state storage.
 */
import { isCommitteeLeaderRole, isOrganizationAdminRole, type OrganizationMember } from "@/api";

export function isOrganizationMember(
  studentId: number | undefined,
  members: readonly OrganizationMember[] = [],
): boolean {
  if (!studentId) {
    return false;
  }
  return members.some((member) => member.studentId === studentId);
}

export function canManageOrganization(
  organizationId: number,
  studentId: number | undefined,
  isPlatformAdmin: boolean,
  isOrgAdminOf: (id: number) => boolean,
  members: readonly OrganizationMember[] = [],
): boolean {
  if (isPlatformAdmin) {
    return true;
  }
  if (!studentId) {
    return false;
  }
  if (isOrgAdminOf(organizationId)) {
    return true;
  }
  const membership = members.find((member) => member.studentId === studentId);
  return Boolean(
    membership &&
      (membership.hierarchyTier === "HighBoard" || isOrganizationAdminRole(membership.role)),
  );
}

export function canReviewOrgRecruitment(
  organizationId: number,
  studentId: number | undefined,
  isPlatformAdmin: boolean,
  isOrgAdminOf: (id: number) => boolean,
  members: readonly OrganizationMember[] = [],
): boolean {
  if (canManageOrganization(organizationId, studentId, isPlatformAdmin, isOrgAdminOf, members)) {
    return true;
  }
  if (!studentId) {
    return false;
  }
  const membership = members.find((member) => member.studentId === studentId);
  if (!membership) {
    return false;
  }
  return (
    membership.hierarchyTier === "CommitteeLeadership" || isCommitteeLeaderRole(membership.role)
  );
}

export interface SavedOrgApplication {
  applicationId: number;
  message: string | null;
  appliedAt: string;
}

function appStorageKey(studentId: number, organizationId: number): string {
  return `teamup.orgApp.${studentId}.${organizationId}`;
}

export const myOrgApplicationStorage = {
  get(studentId: number | undefined, organizationId: number): SavedOrgApplication | null {
    if (!studentId || organizationId <= 0 || typeof window === "undefined") {
      return null;
    }
    try {
      const raw = window.localStorage.getItem(appStorageKey(studentId, organizationId));
      if (!raw) {
        return null;
      }
      const parsed = JSON.parse(raw) as Partial<SavedOrgApplication>;
      if (typeof parsed.applicationId !== "number" || !Number.isFinite(parsed.applicationId)) {
        return null;
      }
      return {
        applicationId: parsed.applicationId,
        message: typeof parsed.message === "string" ? parsed.message : null,
        appliedAt:
          typeof parsed.appliedAt === "string" ? parsed.appliedAt : new Date().toISOString(),
      };
    } catch {
      return null;
    }
  },

  set(
    studentId: number | undefined,
    organizationId: number,
    value: SavedOrgApplication,
  ): void {
    if (!studentId || organizationId <= 0 || typeof window === "undefined") {
      return;
    }
    try {
      window.localStorage.setItem(appStorageKey(studentId, organizationId), JSON.stringify(value));
    } catch {
      // Ignore storage quota failures
    }
  },

  clear(studentId: number | undefined, organizationId: number): void {
    if (!studentId || organizationId <= 0 || typeof window === "undefined") {
      return;
    }
    try {
      window.localStorage.removeItem(appStorageKey(studentId, organizationId));
    } catch {
      // Ignore storage failures
    }
  },
};
