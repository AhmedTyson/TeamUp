/**
 * @date 2026-11-22
 * @purpose M1 (Tyson) route fragment owning authentication, onboarding, and lazy-loaded PlatformAdmin governance routes.
 */
import { lazy } from "react";
import { Navigate, type RouteObject } from "react-router-dom";
import { USER_ROLES } from "@/api";
import {
  ForgotPasswordPage,
  GuestOnlyRoute,
  LoginPage,
  ProtectedRoute,
  RegisterPage,
  ResetPasswordPage,
  RoleGuard,
} from "@/features/auth";

const AdminDashboardPage = lazy(() =>
  import("@/features/admin").then((m) => ({ default: m.AdminDashboardPage })),
);
const AdminCatalogPage = lazy(() =>
  import("@/features/admin").then((m) => ({ default: m.AdminCatalogPage })),
);
const AdminRolesPage = lazy(() =>
  import("@/features/admin").then((m) => ({ default: m.AdminRolesPage })),
);
const AdminRestorePage = lazy(() =>
  import("@/features/admin").then((m) => ({ default: m.AdminRestorePage })),
);
const AdminAuditLogsPage = lazy(() =>
  import("@/features/admin").then((m) => ({ default: m.AdminAuditLogsPage })),
);

export const authRoutes: RouteObject[] = [
  {
    path: "auth",
    element: <Navigate to="/auth/login" replace />,
  },
  {
    element: <GuestOnlyRoute />,
    children: [
      {
        path: "auth/login",
        element: <LoginPage />,
      },
      {
        path: "auth/register",
        element: <RegisterPage />,
      },
      {
        path: "auth/forgot-password",
        element: <ForgotPasswordPage />,
      },
      {
        path: "auth/reset-password",
        element: <ResetPasswordPage />,
      },
    ],
  },
  {
    path: "admin",
    element: <ProtectedRoute />,
    children: [
      {
        element: <RoleGuard roles={[USER_ROLES.PlatformAdmin]} />,
        children: [
          {
            index: true,
            element: <AdminDashboardPage />,
          },
          {
            path: "catalog",
            element: <AdminCatalogPage />,
          },
          {
            path: "roles",
            element: <AdminRolesPage />,
          },
          {
            path: "restore",
            element: <AdminRestorePage />,
          },
          {
            path: "audit-logs",
            element: <AdminAuditLogsPage />,
          },
        ],
      },
    ],
  },
];
