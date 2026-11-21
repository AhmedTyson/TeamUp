/**
 * @date 2026-11-22
 * @purpose TanStack Query hooks for PlatformAdmin dashboard KPIs, academic catalog CRUD, student roles, soft-delete recovery, and audit logs.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  adminApi,
  universitiesApi,
  type AdminStudentSearchParams,
  type AuditLogQueryParams,
  type CreateFacultyPayload,
  type CreateUniversityPayload,
  type RestorableEntityType,
  type UniversityListParams,
  type UpdateFacultyPayload,
  type UpdateUniversityPayload,
  type UserRole,
} from "@/api";
import { queryKeys } from "@/lib/queryKeys";

export function useAdminDashboardQuery() {
  return useQuery({
    queryKey: queryKeys.admin.dashboard,
    queryFn: ({ signal }) => adminApi.getDashboard(signal),
  });
}

export function useUniversitiesListQuery(params: UniversityListParams) {
  return useQuery({
    queryKey: queryKeys.universities.list(params as Record<string, unknown>),
    queryFn: ({ signal }) => universitiesApi.list(params, signal),
  });
}

export function useUniversityFacultiesQuery(universityId: number | null) {
  return useQuery({
    queryKey: queryKeys.universities.faculties(universityId ?? 0),
    queryFn: ({ signal }) => universitiesApi.getFaculties(universityId!, signal),
    enabled: universityId !== null && universityId > 0,
  });
}

export function useCreateUniversityMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateUniversityPayload) => universitiesApi.create(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.universities.all });
      void queryClient.invalidateQueries({ queryKey: queryKeys.admin.all });
    },
  });
}

export function useUpdateUniversityMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: UpdateUniversityPayload }) =>
      universitiesApi.update(id, payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.universities.all });
      void queryClient.invalidateQueries({ queryKey: queryKeys.admin.all });
    },
  });
}

export function useDeleteUniversityMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => universitiesApi.remove(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.universities.all });
      void queryClient.invalidateQueries({ queryKey: queryKeys.admin.all });
    },
  });
}

export function useCreateFacultyMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      universityId,
      payload,
    }: {
      universityId: number;
      payload: CreateFacultyPayload;
    }) => universitiesApi.createFaculty(universityId, payload),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.universities.all });
      void queryClient.invalidateQueries({
        queryKey: queryKeys.universities.faculties(variables.universityId),
      });
      void queryClient.invalidateQueries({ queryKey: queryKeys.admin.all });
    },
  });
}

export function useUpdateFacultyMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      facultyId,
      payload,
    }: {
      facultyId: number;
      universityId: number;
      payload: UpdateFacultyPayload;
    }) => universitiesApi.updateFaculty(facultyId, payload),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.universities.all });
      void queryClient.invalidateQueries({
        queryKey: queryKeys.universities.faculties(variables.universityId),
      });
      void queryClient.invalidateQueries({ queryKey: queryKeys.admin.all });
    },
  });
}

export function useDeleteFacultyMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ facultyId }: { facultyId: number; universityId: number }) =>
      universitiesApi.removeFaculty(facultyId),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.universities.all });
      void queryClient.invalidateQueries({
        queryKey: queryKeys.universities.faculties(variables.universityId),
      });
      void queryClient.invalidateQueries({ queryKey: queryKeys.admin.all });
    },
  });
}

export function useAdminStudentSearchQuery(params: AdminStudentSearchParams) {
  return useQuery({
    queryKey: queryKeys.students.directory({ scope: "admin", ...params }),
    queryFn: ({ signal }) => adminApi.searchStudents(params, signal),
  });
}

export function useUpdateStudentRoleMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ studentId, role }: { studentId: number; role: UserRole }) =>
      adminApi.updateStudentRole(studentId, { role }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.students.all });
      void queryClient.invalidateQueries({ queryKey: queryKeys.admin.all });
    },
  });
}

export function useSoftDeletedQuery(entityType?: RestorableEntityType | "all") {
  const filterParam = entityType && entityType !== "all" ? entityType : undefined;
  return useQuery({
    queryKey: queryKeys.admin.softDeleted(filterParam),
    queryFn: ({ signal }) => adminApi.getSoftDeleted(filterParam, signal),
  });
}

export function useRestoreEntityMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ entityType, id }: { entityType: string; id: number }) =>
      adminApi.restoreEntity(entityType.toLowerCase(), id),
    onSuccess: () => {
      void Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.admin.all }),
        queryClient.invalidateQueries({ queryKey: queryKeys.universities.all }),
        queryClient.invalidateQueries({ queryKey: queryKeys.skills.all }),
        queryClient.invalidateQueries({ queryKey: queryKeys.projects.all }),
        queryClient.invalidateQueries({ queryKey: queryKeys.organizations.all }),
        queryClient.invalidateQueries({ queryKey: queryKeys.events.all }),
        queryClient.invalidateQueries({ queryKey: queryKeys.badges.all }),
        queryClient.invalidateQueries({ queryKey: queryKeys.students.all }),
      ]);
    },
  });
}

export function useAuditLogsQuery(params: AuditLogQueryParams) {
  return useQuery({
    queryKey: queryKeys.admin.auditLogs(params as Record<string, unknown>),
    queryFn: ({ signal }) => adminApi.getAuditLogs(params, signal),
  });
}
