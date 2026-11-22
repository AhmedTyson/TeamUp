/**
 * @date 2026-11-21
 * @purpose Barrel exports for the TeamUp frontend API layer.
 */
export {
  apiClient,
  authClientEvents,
  createApiClient,
  ApiClientError,
  unwrapApiResponse,
  unwrapVoidResponse,
  fetchApiHtml,
  parseRetryAfterSeconds,
} from "./client";
export type { CreateApiClientOptions, TeamUpRequestConfig } from "./client";
export { API_ENDPOINTS } from "./endpoints";
export {
  USER_ROLES,
  ORGANIZATION_ROLES,
  ORGANIZATION_ADMIN_ROLES,
  isOrganizationAdminRole,
  PROFICIENCY_LEVELS,
  PROJECT_STATUSES,
  APPLICATION_STATUSES,
  PROJECT_TASK_STATUSES,
  TASK_PRIORITIES,
  EVENT_TYPES,
  REGISTRATION_STATUSES,
  VERIFICATION_METHODS,
  SCORE_SOURCE_TYPES,
  BADGE_CRITERIA_TYPES,
  NOTIFICATION_TYPES,
  RESTORABLE_ENTITY_TYPES,
} from "./enums";
export type {
  UserRole,
  OrganizationRole,
  ProficiencyLevel,
  ProjectStatus,
  ApplicationStatus,
  ProjectTaskStatus,
  TaskPriority,
  EventType,
  RegistrationStatus,
  VerificationMethod,
  ScoreSourceType,
  BadgeCriteriaType,
  NotificationType,
  RestorableEntityType,
} from "./enums";
export { healthApi } from "./health.api";
export { authApi } from "./auth.api";
export { universitiesApi } from "./universities.api";
export { studentsApi } from "./students.api";
export { adminApi } from "./admin.api";
export type { PublicStudentShowcaseItem, StudentProfile } from "./students.api";
export type { ApiResponse, ErrorDetail, PagedResult, HealthStatus } from "./types";
export type {
  LoginPayload,
  RegisterPayload,
  ForgotPasswordPayload,
  ForgotPasswordResult,
  ResetPasswordPayload,
  AuthResult,
  AuthStudentSummary,
} from "./auth.api";
export type {
  UniversityCatalogItem,
  FacultyCatalogItem,
  UniversitySummary,
  UniversityDetail,
  UniversityListParams,
  CreateUniversityPayload,
  UpdateUniversityPayload,
  FacultyDetail,
  CreateFacultyPayload,
  UpdateFacultyPayload,
} from "./universities.api";
export type {
  AdminDashboardStats,
  UpdateUserRolePayload,
  SoftDeletedItem,
  AuditLogItem,
  AuditLogQueryParams,
  AdminStudentDirectoryItem,
  AdminStudentSearchParams,
} from "./admin.api";
