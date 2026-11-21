/**
 * @date 2026-11-21
 * @purpose Route guard redirecting authenticated users away from guest-only screens (login, register, password reset).
 */
import type { ReactNode } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./AuthContext";

export interface GuestOnlyRouteProps {
  redirectTo?: string;
  children?: ReactNode;
}

export function GuestOnlyRoute({ redirectTo = "/projects", children }: GuestOnlyRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return null;
  }

  if (isAuthenticated) {
    return <Navigate to={redirectTo} replace />;
  }

  return children ? <>{children}</> : <Outlet />;
}
