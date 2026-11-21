/**
 * @date 2026-11-21
 * @purpose Domain enum constants and union types matching TeamUp.Domain.Enums and OrganizationRoles (erasableSyntaxOnly safe).
 */

export const USER_ROLES = {
  Student: "Student",
  OrgAdmin: "OrgAdmin",
  PlatformAdmin: "PlatformAdmin",
} as const;
export type UserRole = (typeof USER_ROLES)[keyof typeof USER_ROLES];

export const ORGANIZATION_ROLES = {
  Admin: "Admin",
  OrgAdmin: "OrgAdmin",
  President: "President",
  VicePresident: "Vice President",
  Executive: "Executive",
  Member: "Member",
} as const;
export type OrganizationRole = (typeof ORGANIZATION_ROLES)[keyof typeof ORGANIZATION_ROLES];

export const ORGANIZATION_ADMIN_ROLES: readonly OrganizationRole[] = [
  ORGANIZATION_ROLES.Admin,
  ORGANIZATION_ROLES.OrgAdmin,
  ORGANIZATION_ROLES.President,
  ORGANIZATION_ROLES.VicePresident,
  ORGANIZATION_ROLES.Executive,
] as const;

export function isOrganizationAdminRole(role?: string | null): boolean {
  if (!role) {
    return false;
  }
  const normalized = role.trim().toLowerCase();
  return ORGANIZATION_ADMIN_ROLES.some((r) => r.toLowerCase() === normalized);
}

export const PROFICIENCY_LEVELS = {
  Beginner: "Beginner",
  Intermediate: "Intermediate",
  Advanced: "Advanced",
} as const;
export type ProficiencyLevel = (typeof PROFICIENCY_LEVELS)[keyof typeof PROFICIENCY_LEVELS];

export const PROJECT_STATUSES = {
  Recruiting: "Recruiting",
  InProgress: "InProgress",
  Completed: "Completed",
  Archived: "Archived",
} as const;
export type ProjectStatus = (typeof PROJECT_STATUSES)[keyof typeof PROJECT_STATUSES];

export const APPLICATION_STATUSES = {
  Pending: "Pending",
  Accepted: "Accepted",
  Rejected: "Rejected",
  Withdrawn: "Withdrawn",
} as const;
export type ApplicationStatus = (typeof APPLICATION_STATUSES)[keyof typeof APPLICATION_STATUSES];

export const PROJECT_TASK_STATUSES = {
  Todo: "Todo",
  InProgress: "InProgress",
  InReview: "InReview",
  Done: "Done",
} as const;
export type ProjectTaskStatus = (typeof PROJECT_TASK_STATUSES)[keyof typeof PROJECT_TASK_STATUSES];

export const TASK_PRIORITIES = {
  Low: "Low",
  Medium: "Medium",
  High: "High",
  Urgent: "Urgent",
} as const;
export type TaskPriority = (typeof TASK_PRIORITIES)[keyof typeof TASK_PRIORITIES];

export const EVENT_TYPES = {
  Workshop: "Workshop",
  Hackathon: "Hackathon",
  Session: "Session",
  Competition: "Competition",
} as const;
export type EventType = (typeof EVENT_TYPES)[keyof typeof EVENT_TYPES];

export const REGISTRATION_STATUSES = {
  Confirmed: "Confirmed",
  Waitlisted: "Waitlisted",
  Cancelled: "Cancelled",
} as const;
export type RegistrationStatus =
  (typeof REGISTRATION_STATUSES)[keyof typeof REGISTRATION_STATUSES];

export const VERIFICATION_METHODS = {
  QRCode: "QRCode",
  RFID: "RFID",
  ManualByOrganizer: "ManualByOrganizer",
} as const;
export type VerificationMethod =
  (typeof VERIFICATION_METHODS)[keyof typeof VERIFICATION_METHODS];

export const SCORE_SOURCE_TYPES = {
  EventAttendance: "EventAttendance",
  TaskCompleted: "TaskCompleted",
  ProjectDelivered: "ProjectDelivered",
  Manual: "Manual",
} as const;
export type ScoreSourceType = (typeof SCORE_SOURCE_TYPES)[keyof typeof SCORE_SOURCE_TYPES];

export const BADGE_CRITERIA_TYPES = {
  PointsThreshold: "PointsThreshold",
  EventCount: "EventCount",
  TaskCount: "TaskCount",
} as const;
export type BadgeCriteriaType = (typeof BADGE_CRITERIA_TYPES)[keyof typeof BADGE_CRITERIA_TYPES];

export const NOTIFICATION_TYPES = {
  ApplicationStatus: "ApplicationStatus",
  TaskAssigned: "TaskAssigned",
  BadgeEarned: "BadgeEarned",
  EventReminder: "EventReminder",
} as const;
export type NotificationType = (typeof NOTIFICATION_TYPES)[keyof typeof NOTIFICATION_TYPES];

export const RESTORABLE_ENTITY_TYPES = {
  student: "student",
  university: "university",
  faculty: "faculty",
  skill: "skill",
  project: "project",
  organization: "organization",
  event: "event",
  badge: "badge",
} as const;
export type RestorableEntityType =
  (typeof RESTORABLE_ENTITY_TYPES)[keyof typeof RESTORABLE_ENTITY_TYPES];
