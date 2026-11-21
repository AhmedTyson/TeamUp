/**
 * @date 2026-11-21
 * @purpose Route guard ensuring only authenticated students can access protected routes.
 */
import { Loader2 } from "lucide-react";
import type { ReactNode } from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

export interface ProtectedRouteProps {
  children?: ReactNode;
  redirectTo?: string;
}

export function ProtectedRoute({
  children,
  redirectTo = "/auth/login",
}: ProtectedRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <div
        role="status"
        aria-live="polite"
        className="flex min-h-[40vh] flex-col items-center justify-center gap-3 text-sm text-[#9ca3af]"
      >
        <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
        <span>Verifying session…</span>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to={redirectTo} replace state={{ from: location }} />;
  }

  return children ? <>{children}</> : <Outlet />;
}
