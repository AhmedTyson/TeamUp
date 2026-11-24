/**
 * @date 2026-11-24
 * @purpose Role-aware dashboard switcher header showing the signed-in user's assigned role badge and quick navigation between authorized role workspaces.
 */
import { Building2, GraduationCap, Shield } from "lucide-react";
import { NavLink } from "react-router-dom";
import { USER_ROLES } from "@/api";
import { Badge } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { cn } from "@/lib/cn";

export function DashboardRoleSwitcher() {
  const { user, isPlatformAdmin, isOrgAdmin, orgAdminIds } = useAuth();
  if (!user) {
    return null;
  }

  const canLeadOrg = isPlatformAdmin || isOrgAdmin || orgAdminIds.length > 0;
  const roleLabel =
    user.role === USER_ROLES.PlatformAdmin
      ? "Platform Admin"
      : canLeadOrg
        ? "Org Admin · Student Leader"
        : "Student Member";

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-white/[0.085] bg-[#1b1d26]/90 px-4 py-3">
      <div className="flex flex-wrap items-center gap-2.5">
        <span className="text-[11px] font-semibold uppercase tracking-wider text-white/55">
          Assigned Workspace
        </span>
        <Badge variant="accent" size="sm">
          {roleLabel}
        </Badge>
        {orgAdminIds.length > 0 && (
          <span className="text-xs text-[#9ca3af]">
            Scoped coordinator for {orgAdminIds.length}{" "}
            {orgAdminIds.length === 1 ? "organization" : "organizations"}
          </span>
        )}
      </div>

      <nav aria-label="Role dashboards" className="flex flex-wrap items-center gap-2">
        <NavLink
          to="/dashboard/student"
          className={({ isActive }) =>
            cn(
              "inline-flex items-center gap-1.5 rounded-xl px-3 py-1.5 text-xs font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
              isActive
                ? "bg-[#d3fb52] text-[#14151c]"
                : "border border-white/10 bg-[#14151c] text-white/80 hover:border-[#d3fb52]/40 hover:text-white",
            )
          }
        >
          <GraduationCap className="size-3.5" aria-hidden="true" />
          Student Dashboard
        </NavLink>

        {canLeadOrg && (
          <NavLink
            to="/dashboard/org"
            className={({ isActive }) =>
              cn(
                "inline-flex items-center gap-1.5 rounded-xl px-3 py-1.5 text-xs font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                isActive
                  ? "bg-[#d3fb52] text-[#14151c]"
                  : "border border-white/10 bg-[#14151c] text-white/80 hover:border-[#d3fb52]/40 hover:text-white",
              )
            }
          >
            <Building2 className="size-3.5" aria-hidden="true" />
            Org Leader Dashboard
          </NavLink>
        )}

        {isPlatformAdmin && (
          <NavLink
            to="/admin"
            className={({ isActive }) =>
              cn(
                "inline-flex items-center gap-1.5 rounded-xl px-3 py-1.5 text-xs font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                isActive
                  ? "bg-[#d3fb52] text-[#14151c]"
                  : "border border-white/10 bg-[#14151c] text-white/80 hover:border-[#d3fb52]/40 hover:text-white",
              )
            }
          >
            <Shield className="size-3.5" aria-hidden="true" />
            Platform Admin Console
          </NavLink>
        )}
      </nav>
    </div>
  );
}
