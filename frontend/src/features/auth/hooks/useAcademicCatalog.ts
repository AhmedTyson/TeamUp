/**
 * @date 2026-11-22
 * @purpose TanStack Query hooks for cached academic catalog lookup and password recovery mutations.
 */
import { useMutation, useQuery } from "@tanstack/react-query";
import {
  authApi,
  universitiesApi,
  type ForgotPasswordPayload,
  type ResetPasswordPayload,
} from "@/api";
import { queryKeys } from "@/lib/queryKeys";

export function useAcademicCatalogQuery() {
  return useQuery({
    queryKey: queryKeys.universities.catalog,
    queryFn: ({ signal }) => universitiesApi.getCatalog(signal),
    staleTime: 5 * 60 * 1000,
  });
}

export function useForgotPasswordMutation() {
  return useMutation({
    mutationFn: (payload: ForgotPasswordPayload) => authApi.forgotPassword(payload),
  });
}

export function useResetPasswordMutation() {
  return useMutation({
    mutationFn: (payload: ResetPasswordPayload) => authApi.resetPassword(payload),
  });
}
