/**
 * @date 2026-11-22
 * @purpose M1 (Tyson) route fragment owning authentication, onboarding, and PlatformAdmin governance routes.
 */
import { Navigate, type RouteObject } from "react-router-dom";
import { API_ENDPOINTS, USER_ROLES } from "@/api";
import {
  ForgotPasswordPage,
  GuestOnlyRoute,
  LoginPage,
  ProtectedRoute,
  RegisterPage,
  ResetPasswordPage,
  RoleGuard,
} from "@/features/auth";
import { ModulePlaceholderPage } from "@/pages/ModulePlaceholderPage";

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
            element: (
              <ModulePlaceholderPage
                title="Platform Admin Dashboard"
                description="Overview KPIs, active and soft-deleted entity metrics, and role distribution."
                endpoint={API_ENDPOINTS.admin.dashboard}
              />
            ),
          },
          {
            path: "catalog",
            element: (
              <ModulePlaceholderPage
                title="Academic Catalog Governance"
                description="Manage universities and faculties across the platform."
                endpoint={API_ENDPOINTS.universities.list}
              />
            ),
          },
          {
            path: "roles",
            element: (
              <ModulePlaceholderPage
                title="Student Role Governance"
                description="Manage Student and OrgAdmin role assignments with SecurityStamp revocation."
                endpoint={API_ENDPOINTS.admin.studentRole(0)}
              />
            ),
          },
          {
            path: "restore",
            element: (
              <ModulePlaceholderPage
                title="Soft-Delete Recycle Bin"
                description="Restore soft-deleted records across all 8 supported entity types."
                endpoint={API_ENDPOINTS.admin.softDeleted}
              />
            ),
          },
          {
            path: "audit-logs",
            element: (
              <ModulePlaceholderPage
                title="Immutable Audit Logs"
                description="Inspect administrative actions, actor IDs, client IPs, and old/new state diffs."
                endpoint={API_ENDPOINTS.admin.auditLogs}
              />
            ),
          },
        ],
      },
    ],
  },
];
