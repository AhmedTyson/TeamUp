/**
 * @date 2026-11-26
 * @purpose Centralized, strongly-typed route definitions matching TeamUp.Api OpenAPI v1 spec (.NET 11).
 */
import type { RestorableEntityType } from "./enums";

const V1 = "/api/v1";

export const API_ENDPOINTS = {
  health: "/health",
  openApi: "/openapi/v1.json",

  auth: {
    register: `${V1}/auth/register`,
    login: `${V1}/auth/login`,
    refresh: `${V1}/auth/refresh`,
    logout: `${V1}/auth/logout`,
    forgotPassword: `${V1}/auth/forgot-password`,
    resetPassword: `${V1}/auth/reset-password`,
    sendVerificationEmail: `${V1}/auth/send-verification-email`,
    verifyEmail: `${V1}/auth/verify-email`,
  },

  universities: {
    list: `${V1}/universities`,
    catalog: `${V1}/universities/catalog`,
    byId: (id: number) => `${V1}/universities/${id}`,
    faculties: (universityId: number) => `${V1}/universities/${universityId}/faculties`,
  },

  faculties: {
    byId: (id: number) => `${V1}/faculties/${id}`,
  },

  students: {
    list: `${V1}/students`,
    showcase: `${V1}/students/showcase`,
    me: `${V1}/students/me`,
    byId: (id: number) => `${V1}/students/${id}`,
    profile: (id: number) => `${V1}/students/${id}/profile`,
    myProfile: `${V1}/students/me/profile`,
    myProfilePhoto: `${V1}/students/me/profile/photo`,
    interests: (id: number) => `${V1}/students/${id}/interests`,
    myInterests: `${V1}/students/me/interests`,
    myInterest: (interest: string) => `${V1}/students/me/interests/${encodeURIComponent(interest)}`,
    skills: (id: number) => `${V1}/students/${id}/skills`,
    mySkill: (skillId: number) => `${V1}/students/me/skills/${skillId}`,
    score: (id: number) => `${V1}/students/${id}/score`,
    scoreTransactions: (id: number) => `${V1}/students/${id}/score-transactions`,
    badges: (id: number) => `${V1}/students/${id}/badges`,
    manualAwardBadge: (id: number, badgeId: number) => `${V1}/students/${id}/badges/${badgeId}`,
  },

  skills: {
    list: `${V1}/skills`,
    byId: (id: number) => `${V1}/skills/${id}`,
    create: `${V1}/skills`,
  },

  projects: {
    list: `${V1}/projects`,
    byId: (id: number) => `${V1}/projects/${id}`,
    requiredSkills: (id: number) => `${V1}/projects/${id}/required-skills`,
    requiredSkill: (id: number, skillId: number) => `${V1}/projects/${id}/required-skills/${skillId}`,
    members: (id: number) => `${V1}/projects/${id}/members`,
    member: (id: number, studentId: number) => `${V1}/projects/${id}/members/${studentId}`,
    applications: (id: number) => `${V1}/projects/${id}/applications`,
    myApplication: (id: number) => `${V1}/projects/${id}/applications/mine`,
    application: (id: number, appId: number) => `${V1}/projects/${id}/applications/${appId}`,
    tasks: (id: number) => `${V1}/projects/${id}/tasks`,
  },

  tasks: {
    byId: (id: number) => `${V1}/tasks/${id}`,
    assignee: (id: number) => `${V1}/tasks/${id}/assignee`,
  },

  organizations: {
    list: `${V1}/organizations`,
    publicList: `${V1}/organizations/public`,
    publicCategories: `${V1}/organizations/public/categories`,
    publicById: (id: number) => `${V1}/organizations/public/${id}`,
    publicHierarchy: (id: number) => `${V1}/organizations/public/${id}/hierarchy`,
    myApplications: `${V1}/organizations/my-applications`,
    requests: `${V1}/organizations/requests`,
    myRequests: `${V1}/organizations/requests/my`,
    requestById: (id: number) => `${V1}/organizations/requests/${id}`,
    byId: (id: number) => `${V1}/organizations/${id}`,
    members: (id: number) => `${V1}/organizations/${id}/members`,
    member: (id: number, studentId: number) => `${V1}/organizations/${id}/members/${studentId}`,
    units: (id: number) => `${V1}/organizations/${id}/units`,
    unitById: (id: number, unitId: number) => `${V1}/organizations/${id}/units/${unitId}`,
    hierarchy: (id: number) => `${V1}/organizations/${id}/hierarchy`,
    participants: (id: number) => `${V1}/organizations/${id}/participants`,
    applications: (id: number) => `${V1}/organizations/${id}/applications`,
    application: (id: number, appId: number) => `${V1}/organizations/${id}/applications/${appId}`,
    applicationStage: (id: number, appId: number) =>
      `${V1}/organizations/${id}/applications/${appId}/stage`,
    applicationsCsv: (id: number) => `${V1}/organizations/${id}/applications/export.csv`,
  },

  events: {
    list: `${V1}/events`,
    publicList: `${V1}/events/public`,
    publicById: (id: number) => `${V1}/events/public/${id}`,
    myRegistrations: `${V1}/events/my-registrations`,
    byId: (id: number) => `${V1}/events/${id}`,
    sessions: (id: number) => `${V1}/events/${id}/sessions`,
    registrations: (id: number) => `${V1}/events/${id}/registrations`,
    myRegistration: (id: number) => `${V1}/events/${id}/registrations/mine`,
  },

  sessions: {
    byId: (id: number) => `${V1}/sessions/${id}`,
    attendance: (id: number) => `${V1}/sessions/${id}/attendance`,
    checkInCode: (id: number) => `${V1}/sessions/${id}/attendance/check-in-code`,
    selfCheckIn: (id: number) => `${V1}/sessions/${id}/attendance/self-check-in`,
    attendanceReport: (id: number) => `${V1}/sessions/${id}/attendance/report`,
    attendanceCsv: (id: number) => `${V1}/sessions/${id}/attendance/export.csv`,
    studentAttendance: (id: number, studentId: number) => `${V1}/sessions/${id}/attendance/${studentId}`,
  },

  gamification: {
    leaderboard: `${V1}/gamification/leaderboard`,
    badges: `${V1}/badges`,
    badgeById: (id: number) => `${V1}/badges/${id}`,
    scoreTransactions: `${V1}/score-transactions`,
    notifications: `${V1}/notifications`,
    notificationById: (id: number) => `${V1}/notifications/${id}`,
    markAllNotificationsRead: `${V1}/notifications/mark-all-read`,
    notificationsUnreadCount: `${V1}/notifications/unread-count`,
  },

  admin: {
    dashboard: `${V1}/admin/dashboard`,
    studentRole: (id: number) => `${V1}/admin/students/${id}/role`,
    softDeleted: `${V1}/admin/soft-deleted`,
    restore: (entityType: RestorableEntityType | string, id: number) =>
      `${V1}/admin/restore/${entityType}/${id}`,
    auditLogs: `${V1}/admin/audit-logs`,
    platformSummaryPdf: `${V1}/admin/reports/platform-summary.pdf`,
    sendSummaryReport: `${V1}/admin/reports/send-summary`,
    emailTemplates: `${V1}/admin/templates`,
    emailTemplatePreview: (templateName: string) =>
      `${V1}/admin/templates/preview/${encodeURIComponent(templateName)}`,
    leaderboardDigest: `${V1}/admin/communications/leaderboard-digest`,
    projectMatchDigest: `${V1}/admin/communications/project-matches`,
    organizationRequests: `${V1}/admin/organization-requests`,
    reviewOrganizationRequest: (requestId: number) =>
      `${V1}/admin/organization-requests/${requestId}/review`,
  },
} as const;
