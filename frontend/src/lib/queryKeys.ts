/**
 * @date 2026-11-21
 * @purpose Standardized TanStack Query keys across all 4 feature tracks (M1–M4) and shared gamification invalidation helper.
 */
import type { QueryClient } from "@tanstack/react-query";

export type QueryParams = Record<string, unknown>;

export const queryKeys = {
  health: ["health"] as const,

  auth: {
    me: ["auth", "me"] as const,
  },

  universities: {
    all: ["universities"] as const,
    catalog: ["universities", "catalog"] as const,
    list: (params?: QueryParams) => ["universities", "list", params ?? {}] as const,
    detail: (id: number) => ["universities", "detail", id] as const,
    faculties: (universityId: number) => ["universities", universityId, "faculties"] as const,
  },

  admin: {
    all: ["admin"] as const,
    dashboard: ["admin", "dashboard"] as const,
    softDeleted: (entityType?: string) => ["admin", "soft-deleted", entityType ?? "all"] as const,
    auditLogs: (params?: QueryParams) => ["admin", "audit-logs", params ?? {}] as const,
    templates: ["admin", "templates"] as const,
    templatePreview: (name: string) => ["admin", "templates", "preview", name] as const,
    organizationRequests: (params?: QueryParams) =>
      ["admin", "organization-requests", params ?? {}] as const,
  },

  skills: {
    all: ["skills"] as const,
    list: (params?: QueryParams) => ["skills", "list", params ?? {}] as const,
    detail: (id: number) => ["skills", "detail", id] as const,
  },

  students: {
    all: ["students"] as const,
    showcase: (limit = 6) => ["students", "showcase", { limit }] as const,
    directory: (params?: QueryParams) => ["students", "directory", params ?? {}] as const,
    detail: (id: number) => ["students", "detail", id] as const,
    profile: (id: number) => ["students", "profile", id] as const,
    interests: (id: number) => ["students", "interests", id] as const,
    skills: (id: number) => ["students", "skills", id] as const,
  },

  projects: {
    all: ["projects"] as const,
    list: (params?: QueryParams) => ["projects", "list", params ?? {}] as const,
    detail: (id: number) => ["projects", "detail", id] as const,
    requiredSkills: (id: number) => ["projects", id, "required-skills"] as const,
    members: (id: number) => ["projects", id, "members"] as const,
    applications: (id: number) => ["projects", id, "applications"] as const,
    tasks: (id: number) => ["projects", id, "tasks"] as const,
    taskDetail: (taskId: number) => ["tasks", "detail", taskId] as const,
  },

  organizations: {
    all: ["organizations"] as const,
    list: (params?: QueryParams) => ["organizations", "list", params ?? {}] as const,
    myApplications: ["organizations", "my-applications"] as const,
    myCreationRequests: ["organizations", "my-creation-requests"] as const,
    detail: (id: number) => ["organizations", "detail", id] as const,
    members: (id: number) => ["organizations", id, "members"] as const,
    units: (id: number) => ["organizations", id, "units"] as const,
    hierarchy: (id: number) => ["organizations", id, "hierarchy"] as const,
    participants: (id: number) => ["organizations", id, "participants"] as const,
    applications: (id: number) => ["organizations", id, "applications"] as const,
  },

  events: {
    all: ["events"] as const,
    list: (params?: QueryParams) => ["events", "list", params ?? {}] as const,
    myRegistrations: ["events", "my-registrations"] as const,
    detail: (id: number) => ["events", "detail", id] as const,
    sessions: (id: number) => ["events", id, "sessions"] as const,
    registrations: (id: number) => ["events", id, "registrations"] as const,
  },

  sessions: {
    all: ["sessions"] as const,
    attendance: (sessionId: number) => ["sessions", sessionId, "attendance"] as const,
    attendanceReport: (sessionId: number) => ["sessions", sessionId, "attendance", "report"] as const,
  },

  gamification: {
    all: ["gamification"] as const,
    score: (studentId: number) => ["gamification", "score", studentId] as const,
    ledger: (studentId: number) => ["gamification", "ledger", studentId] as const,
    leaderboard: (params?: QueryParams) => ["gamification", "leaderboard", params ?? {}] as const,
  },

  badges: {
    all: ["badges"] as const,
    catalog: ["badges", "catalog"] as const,
    student: (studentId: number) => ["badges", "student", studentId] as const,
  },

  notifications: {
    all: ["notifications"] as const,
    inbox: (unreadOnly = false) => ["notifications", "inbox", { unreadOnly }] as const,
  },
} as const;

/**
 * Shared cross-module helper called after XP-earning actions (task Done +50 XP, session check-in +25/+100 XP,
 * project Completed, manual adjustments) so the XP chip, leaderboard, badges, and notification bell stay in sync.
 */
export async function invalidateGamification(
  queryClient: QueryClient,
  studentId?: number,
): Promise<void> {
  const tasks: Promise<void>[] = [
    queryClient.invalidateQueries({ queryKey: queryKeys.gamification.all }),
    queryClient.invalidateQueries({ queryKey: queryKeys.badges.all }),
    queryClient.invalidateQueries({ queryKey: queryKeys.notifications.all }),
  ];

  if (studentId !== undefined && studentId > 0) {
    tasks.push(
      queryClient.invalidateQueries({ queryKey: queryKeys.gamification.score(studentId) }),
      queryClient.invalidateQueries({ queryKey: queryKeys.gamification.ledger(studentId) }),
      queryClient.invalidateQueries({ queryKey: queryKeys.badges.student(studentId) }),
    );
  }

  await Promise.all(tasks);
}
