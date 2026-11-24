/**
 * @date 2026-11-24
 * @purpose Role-based dashboard entrypoint routing PlatformAdmin to /admin, OrgAdmin/Org Leaders to OrgAdminDashboardPage, and Students to StudentDashboardPage.
 */
import { Navigate } from "react-router-dom";
import { useAuth } from "@/features/auth";
import { OrgAdminDashboardPage } from "./OrgAdminDashboardPage";
import { StudentDashboardPage } from "./StudentDashboardPage";

export function RoleDashboardRouterPage() {
  const { isPlatformAdmin, isOrgAdmin, orgAdminIds } = useAuth();

  if (isPlatformAdmin) {
    return <Navigate to="/admin" replace />;
  }

  if (isOrgAdmin || orgAdminIds.length > 0) {
    return <OrgAdminDashboardPage />;
  }

  return <StudentDashboardPage />;
}
