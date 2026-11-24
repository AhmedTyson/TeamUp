/**
 * @date 2026-11-21
 * @purpose Route guard redirecting authenticated users away from guest-only screens (login, register, password reset).
 */
import type { ReactNode } from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

interface RedirectLocationState {
  from?: {
    pathname?: string;
    search?: string;
  };
}

export interface GuestOnlyRouteProps {
  redirectTo?: string;
  children?: ReactNode;
}

export function GuestOnlyRoute({ redirectTo = "/projects", children }: GuestOnlyRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return null;
  }

  if (isAuthenticated) {
    const state = location.state as RedirectLocationState | null;
    const fromPath = state?.from?.pathname;
    const fromSearch = state?.from?.search ?? "";
    const safeRedirect =
      fromPath &&
      fromPath.startsWith("/") &&
      !fromPath.startsWith("//") &&
      !fromPath.startsWith("/auth")
        ? `${fromPath}${fromSearch}`
        : redirectTo;

    return <Navigate to={safeRedirect} replace />;
  }

  return children ? <>{children}</> : <Outlet />;
}
