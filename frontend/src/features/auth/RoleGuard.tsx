/**
 * @date 2026-11-21
 * @purpose Role and scoped-organization authorization route guard rendering ForbiddenBanner when unauthorized.
 */
import { Loader2 } from "lucide-react";
import type { ReactNode } from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import type { UserRole } from "@/api/enums";
import { ForbiddenBanner } from "@/components/ui";
import { useAuth } from "./AuthContext";

export interface RoleGuardProps {
  roles: readonly UserRole[];
  /** Optional scoped organization ID; allows PlatformAdmin or scoped org_admin of this club. */
  requireOrgAdminOf?: number;
  /** When true, also grants access to students holding at least one scoped organization admin assignment. */
  allowScopedOrgAdmin?: boolean;
  fallback?: ReactNode;
  children?: ReactNode;
}

export function RoleGuard({
  roles,
  requireOrgAdminOf,
  allowScopedOrgAdmin = false,
  fallback,
  children,
}: RoleGuardProps) {
  const { user, isAuthenticated, isLoading, isOrgAdminOf, orgAdminIds } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <div
        role="status"
        aria-live="polite"
        className="flex min-h-[40vh] flex-col items-center justify-center gap-3 text-sm text-[#9ca3af]"
      >
        <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
        <span>Checking permissions…</span>
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return <Navigate to="/auth/login" replace state={{ from: location }} />;
  }

  const hasRequiredRole =
    roles.includes(user.role) || (allowScopedOrgAdmin && orgAdminIds.length > 0);
  const hasScopedOrgAccess =
    requireOrgAdminOf === undefined || isOrgAdminOf(requireOrgAdminOf);

  if (!hasRequiredRole || !hasScopedOrgAccess) {
    return (
      <>
        {fallback ?? (
          <ForbiddenBanner message="Your account role does not have permission to access this area." />
        )}
      </>
    );
  }

  return children ? <>{children}</> : <Outlet />;
}
