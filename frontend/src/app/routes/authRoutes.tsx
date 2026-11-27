/**
 * @date 2026-11-22
 * @purpose M1 (Tyson) route fragment owning authentication, onboarding, and lazy-loaded PlatformAdmin governance routes.
 */
import { lazy } from "react";
import { Navigate, type RouteObject } from "react-router-dom";
import { USER_ROLES } from "@/api";
import {
  GuestOnlyRoute,
  ProtectedRoute,
  RoleGuard,
} from "@/features/auth";

const LoginPage = lazy(() =>
  import("@/features/auth/pages/LoginPage").then((m) => ({ default: m.LoginPage })),
);
const RegisterPage = lazy(() =>
  import("@/features/auth/pages/RegisterPage").then((m) => ({ default: m.RegisterPage })),
);
const ForgotPasswordPage = lazy(() =>
  import("@/features/auth/pages/ForgotPasswordPage").then((m) => ({
    default: m.ForgotPasswordPage,
  })),
);
const ResetPasswordPage = lazy(() =>
  import("@/features/auth/pages/ResetPasswordPage").then((m) => ({
    default: m.ResetPasswordPage,
  })),
);
const VerifyEmailPage = lazy(() =>
  import("@/features/auth/pages/VerifyEmailPage").then((m) => ({
    default: m.VerifyEmailPage,
  })),
);

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
const AdminCommunicationsPage = lazy(() =>
  import("@/features/admin").then((m) => ({ default: m.AdminCommunicationsPage })),
);
const AdminOrganizationRequestsPage = lazy(() =>
  import("@/features/admin").then((m) => ({ default: m.AdminOrganizationRequestsPage })),
);

export const authRoutes: RouteObject[] = [
  {
    path: "auth",
    element: <Navigate to="/auth/login" replace />,
  },
  {
    path: "auth/verify-email",
    element: <VerifyEmailPage />,
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
          {
            path: "communications",
            element: <AdminCommunicationsPage />,
          },
          {
            path: "organization-requests",
            element: <AdminOrganizationRequestsPage />,
          },
        ],
      },
    ],
  },
];
