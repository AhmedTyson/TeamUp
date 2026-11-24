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
export { gamificationApi } from "./gamification.api";
export { badgesApi } from "./badges.api";
export { notificationsApi } from "./notifications.api";
export { adminApi } from "./admin.api";
export { invalidateGamification } from "@/lib/queryKeys";
export { skillsApi } from "./skills.api";
export type {
  SkillDto,
  StudentSkillDto,
  SkillQueryParams,
  SkillPayload,
  StudentSkillPayload,
} from "./skills.api";
export { projectsApi } from "./projects.api";
export type {
  ProjectListItemDto,
  ProjectMemberDto,
  ProjectSkillDto,
  ProjectDetailDto,
  ProjectApplicationDto,
  ProjectListParams,
  CreateProjectPayload,
  UpdateProjectPayload,
  RequiredSkillPayload,
} from "./projects.api";
export { tasksApi } from "./tasks.api";
export type {
  ProjectTaskDto,
  CreateProjectTaskPayload,
  UpdateProjectTaskPayload,
} from "./tasks.api";
export type {
  PublicStudentShowcaseItem,
  StudentProfileDto,
  StudentProfileDto as StudentProfile,
  UpsertProfileRequest,
  UpdateStudentRequest,
  StudentDetailDto,
  StudentCardDto,
  StudentPublicProfileDto,
  StudentDirectoryQueryParams,
} from "./students.api";
export type {
  StudentScoreDto,
  ScoreTransactionDto,
  CreateScoreTransactionRequest,
  LeaderboardEntryDto,
  LeaderboardQueryParams,
} from "./gamification.api";
export type {
  BadgeDto,
  StudentBadgeDto,
  CreateBadgeRequest,
  UpdateBadgeRequest,
} from "./badges.api";
export type { NotificationDto } from "./notifications.api";
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
  CampusNewsItem,
  AdminReportSummaryDto,
  AdminReportEmailResultDto,
  SendAdminReportPayload,
  DigestDispatchMode,
  AdminLeaderboardDigestPayload,
  AdminProjectMatchDigestPayload,
  DigestDispatchResultDto,
} from "./admin.api";
