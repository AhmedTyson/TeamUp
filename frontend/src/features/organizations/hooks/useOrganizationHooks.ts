/**
 * @date 2026-11-24
 * @purpose TanStack Query hooks and mutations for organizations, public previews, units, hierarchy, participants, and Kanban recruitment.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  organizationsApi,
  workspacesApi,
  type CreateOrganizationProposalPayload,
  type OrganizationApplicationPayload,
  type OrganizationListParams,
  type OrganizationPayload,
  type OrganizationUnitPayload,
  type UpdateApplicationStagePayload,
  type UpdateMemberRolePayload,
} from "@/api";
import { useAuth } from "@/features/auth/AuthContext";
import { queryKeys } from "@/lib/queryKeys";

export function useOrganizations(params: OrganizationListParams, enabled = true) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: [...queryKeys.organizations.list(params), isAuthenticated],
    queryFn: ({ signal }) =>
      isAuthenticated
        ? organizationsApi.list(params, signal)
        : organizationsApi.publicList(params, signal),
    enabled,
  });
}

export function useOrganization(id: number, enabled = true) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: [...queryKeys.organizations.detail(id), isAuthenticated],
    queryFn: ({ signal }) =>
      isAuthenticated
        ? organizationsApi.detail(id, signal)
        : organizationsApi.publicDetail(id, signal),
    enabled: enabled && Number.isInteger(id) && id > 0,
  });
}

/**
 * Single-request organization workspace: loads org detail, authorization flags,
 * and role-appropriate applications in one HTTP round-trip (anonymous falls back to public detail).
 */
export function useOrganizationWorkspace(id: number) {
  const { isAuthenticated } = useAuth();
  const isValid = Number.isInteger(id) && id > 0;

  const workspaceQuery = useQuery({
    queryKey: queryKeys.organizations.workspace(id),
    queryFn: ({ signal }) => workspacesApi.organizationWorkspace(id, signal),
    enabled: isValid && isAuthenticated,
  });
  const publicQuery = useOrganization(id, !isAuthenticated);
  const active = isAuthenticated ? workspaceQuery : publicQuery;

  return {
    organization: isAuthenticated ? workspaceQuery.data?.organization : publicQuery.data,
    canManage: isAuthenticated ? Boolean(workspaceQuery.data?.canManage) : false,
    canReviewRecruitment: isAuthenticated
      ? Boolean(workspaceQuery.data?.canReviewRecruitment)
      : false,
    isMember: isAuthenticated ? Boolean(workspaceQuery.data?.isMember) : false,
    adminApplications: isAuthenticated ? (workspaceQuery.data?.adminApplications ?? []) : [],
    myApplications: isAuthenticated ? workspaceQuery.data?.myApplications : undefined,
    isLoading: active.isLoading,
    isError: active.isError,
    error: active.error,
    refetch: active.refetch,
  };
}

export function useOrganizationHierarchy(id: number, enabled = true) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: [...queryKeys.organizations.hierarchy(id), isAuthenticated],
    queryFn: ({ signal }) =>
      isAuthenticated
        ? organizationsApi.hierarchy(id, signal)
        : organizationsApi.publicHierarchy(id, signal),
    enabled: Number.isInteger(id) && id > 0 && enabled,
  });
}

export function useOrganizationUnits(id: number, enabled = true) {
  return useQuery({
    queryKey: queryKeys.organizations.units(id),
    queryFn: ({ signal }) => organizationsApi.units(id, signal),
    enabled: Number.isInteger(id) && id > 0 && enabled,
  });
}

export function useOrganizationParticipants(id: number, enabled = true) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: queryKeys.organizations.participants(id),
    queryFn: ({ signal }) => organizationsApi.participants(id, signal),
    enabled: Number.isInteger(id) && id > 0 && isAuthenticated && enabled,
  });
}

export function useMyOrganizationApplications(enabled = true) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: queryKeys.organizations.myApplications,
    queryFn: ({ signal }) => organizationsApi.myApplications(signal),
    enabled: isAuthenticated && enabled,
  });
}

export function useMyOrganizationCreationRequests(enabled = true) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: queryKeys.organizations.myCreationRequests,
    queryFn: ({ signal }) => organizationsApi.myCreationRequests(signal),
    enabled: isAuthenticated && enabled,
  });
}

export function useOrganizationApplications(id: number, enabled: boolean) {
  return useQuery({
    queryKey: queryKeys.organizations.applications(id),
    queryFn: ({ signal }) => organizationsApi.applications(id, signal),
    enabled: Number.isInteger(id) && id > 0 && enabled,
  });
}

export function useOrganizationMutations(organizationId?: number) {
  const queryClient = useQueryClient();

  const invalidateOrg = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: queryKeys.organizations.all }),
      queryClient.invalidateQueries({ queryKey: queryKeys.dashboards.all }),
    ]);
    if (organizationId && organizationId > 0) {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.organizations.detail(organizationId) }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.organizations.workspace(organizationId),
        }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.organizations.applications(organizationId),
        }),
        queryClient.invalidateQueries({ queryKey: queryKeys.organizations.units(organizationId) }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.organizations.hierarchy(organizationId),
        }),
        queryClient.invalidateQueries({
          queryKey: queryKeys.organizations.participants(organizationId),
        }),
      ]);
    }
  };

  const create = useMutation({
    mutationFn: (payload: OrganizationPayload) => organizationsApi.create(payload),
    onSuccess: invalidateOrg,
  });

  const update = useMutation({
    mutationFn: (payload: OrganizationPayload) =>
      organizationsApi.update(organizationId ?? 0, payload),
    onSuccess: invalidateOrg,
  });

  const remove = useMutation({
    mutationFn: () => organizationsApi.remove(organizationId ?? 0),
    onSuccess: invalidateOrg,
  });

  const apply = useMutation({
    mutationFn: (payload: string | null | OrganizationApplicationPayload) => {
      const body: OrganizationApplicationPayload =
        typeof payload === "string" || payload === null ? { message: payload } : payload;
      return organizationsApi.apply(organizationId ?? 0, body);
    },
    onSuccess: invalidateOrg,
  });

  const withdraw = useMutation({
    mutationFn: (applicationId: number) =>
      organizationsApi.withdrawApplication(organizationId ?? 0, applicationId),
    onSuccess: invalidateOrg,
  });

  const review = useMutation({
    mutationFn: ({
      applicationId,
      status,
    }: {
      applicationId: number;
      status: "Accepted" | "Rejected";
    }) => organizationsApi.reviewApplication(organizationId ?? 0, applicationId, status),
    onSuccess: invalidateOrg,
  });

  const updateStage = useMutation({
    mutationFn: ({
      applicationId,
      payload,
    }: {
      applicationId: number;
      payload: UpdateApplicationStagePayload;
    }) => organizationsApi.updateApplicationStage(organizationId ?? 0, applicationId, payload),
    onSuccess: invalidateOrg,
  });

  const updateRole = useMutation({
    mutationFn: ({
      studentId,
      role,
      organizationUnitId,
      hierarchyTier,
    }: { studentId: number } & UpdateMemberRolePayload) =>
      organizationsApi.updateMemberRole(organizationId ?? 0, studentId, {
        role,
        organizationUnitId,
        hierarchyTier,
      }),
    onSuccess: invalidateOrg,
  });

  const removeMember = useMutation({
    mutationFn: (studentId: number) =>
      organizationsApi.removeMember(organizationId ?? 0, studentId),
    onSuccess: invalidateOrg,
  });

  const createUnit = useMutation({
    mutationFn: (payload: OrganizationUnitPayload) =>
      organizationsApi.createUnit(organizationId ?? 0, payload),
    onSuccess: invalidateOrg,
  });

  const updateUnit = useMutation({
    mutationFn: ({ unitId, payload }: { unitId: number; payload: OrganizationUnitPayload }) =>
      organizationsApi.updateUnit(organizationId ?? 0, unitId, payload),
    onSuccess: invalidateOrg,
  });

  const deleteUnit = useMutation({
    mutationFn: (unitId: number) =>
      organizationsApi.removeUnit(organizationId ?? 0, unitId),
    onSuccess: invalidateOrg,
  });

  const submitCreationRequest = useMutation({
    mutationFn: (payload: CreateOrganizationProposalPayload) =>
      organizationsApi.submitCreationRequest(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.organizations.myCreationRequests });
      void queryClient.invalidateQueries({ queryKey: queryKeys.admin.all });
    },
  });

  const withdrawCreationRequest = useMutation({
    mutationFn: (requestId: number) => organizationsApi.withdrawCreationRequest(requestId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.organizations.myCreationRequests });
      void queryClient.invalidateQueries({ queryKey: queryKeys.admin.all });
    },
  });

  return {
    create,
    update,
    remove,
    apply,
    withdraw,
    review,
    updateStage,
    updateRole,
    removeMember,
    createUnit,
    updateUnit,
    deleteUnit,
    submitCreationRequest,
    withdrawCreationRequest,
  };
}

/**
 * Distinct organization categories in use, most used first. Feeds the directory filter chips so a new
 * category appears automatically instead of waiting for a frontend constant to be updated.
 */
const CATEGORIES_STALE_TIME_MS = 5 * 60_000;

export function useOrganizationCategories() {
  return useQuery({
    queryKey: queryKeys.organizations.categories,
    queryFn: ({ signal }) => organizationsApi.categories(signal),
    staleTime: CATEGORIES_STALE_TIME_MS,
  });
}
