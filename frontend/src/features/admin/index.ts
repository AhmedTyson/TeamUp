/**
 * @date 2026-11-22
 * @purpose Public PlatformAdmin governance module barrel exports.
 */
export { AdminConsoleLayout } from "./components/AdminConsoleLayout";
export type { AdminConsoleLayoutProps } from "./components/AdminConsoleLayout";
export { FacultyManagerPanel } from "./components/FacultyManagerPanel";
export type { FacultyManagerPanelProps } from "./components/FacultyManagerPanel";
export {
  useAdminDashboardQuery,
  useUniversitiesListQuery,
  useUniversityFacultiesQuery,
  useCreateUniversityMutation,
  useUpdateUniversityMutation,
  useDeleteUniversityMutation,
  useCreateFacultyMutation,
  useUpdateFacultyMutation,
  useDeleteFacultyMutation,
  useAdminStudentSearchQuery,
  useUpdateStudentRoleMutation,
  useSoftDeletedQuery,
  useRestoreEntityMutation,
  useAuditLogsQuery,
  useAdminEmailTemplatesQuery,
  useAdminTemplatePreviewQuery,
  useSendPlatformReportMutation,
  useDispatchLeaderboardDigestMutation,
  useDispatchProjectMatchDigestMutation,
  useAdminOrganizationRequestsQuery,
  useReviewOrganizationRequestMutation,
} from "./hooks/useAdminHooks";
export {
  universityFormSchema,
  facultyFormSchema,
  roleUpdateSchema,
} from "./schemas/adminSchemas";
export type {
  UniversityFormValues,
  FacultyFormValues,
  RoleUpdateFormValues,
} from "./schemas/adminSchemas";
export { AdminDashboardPage } from "./pages/AdminDashboardPage";
export { AdminCatalogPage } from "./pages/AdminCatalogPage";
export { AdminRolesPage } from "./pages/AdminRolesPage";
export { AdminRestorePage } from "./pages/AdminRestorePage";
export { AdminAuditLogsPage } from "./pages/AdminAuditLogsPage";
export { AdminCommunicationsPage } from "./pages/AdminCommunicationsPage";
export { AdminOrganizationRequestsPage } from "./pages/AdminOrganizationRequestsPage";
