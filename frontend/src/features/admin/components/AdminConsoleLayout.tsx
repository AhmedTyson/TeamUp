/**
 * @date 2026-11-22
 * @purpose Shared PlatformAdmin console header and sub-navigation framing all /admin/* governance routes.
 */
import {
  Building2,
  ClipboardList,
  LayoutDashboard,
  RotateCcw,
  UserCog,
} from "lucide-react";
import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";
import { cn } from "@/lib/cn";

export interface AdminConsoleLayoutProps {
  title: string;
  description: string;
  actions?: ReactNode;
  children: ReactNode;
}

const ADMIN_TABS = [
  { to: "/admin", label: "Overview", icon: LayoutDashboard, end: true },
  { to: "/admin/catalog", label: "Academic Catalog", icon: Building2, end: false },
  { to: "/admin/roles", label: "Role Governance", icon: UserCog, end: false },
  { to: "/admin/restore", label: "Archive", icon: RotateCcw, end: false },
  { to: "/admin/audit-logs", label: "Activity Log", icon: ClipboardList, end: false },
] as const;

export function AdminConsoleLayout({
  title,
  description,
  actions,
  children,
}: AdminConsoleLayoutProps) {
  return (
    <div className="space-y-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div className="space-y-1.5">
          <h1
            className="text-3xl font-bold tracking-tight text-white sm:text-4xl"
            style={{ fontFamily: "var(--font-heading)" }}
          >
            {title}
          </h1>
          <p className="max-w-2xl text-sm text-[#9ca3af]">{description}</p>
        </div>

        {actions && <div className="flex shrink-0 items-center gap-2.5">{actions}</div>}
      </div>

      <nav
        aria-label="Platform admin sections"
        className="no-scrollbar flex items-center gap-1.5 overflow-x-auto border-b border-white/[0.085] pb-3"
      >
        {ADMIN_TABS.map((tab) => {
          const Icon = tab.icon;
          return (
            <NavLink
              key={tab.to}
              to={tab.to}
              end={tab.end}
              className={({ isActive }) =>
                cn(
                  "inline-flex shrink-0 items-center gap-2 rounded-xl px-3.5 py-2 text-xs font-semibold transition-colors sm:text-sm",
                  "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
                  isActive
                    ? "bg-[#d3fb52] text-[#14151c]"
                    : "border border-white/10 bg-[#1b1d26] text-white/75 hover:border-[#d3fb52]/35 hover:text-white",
                )
              }
            >
              <Icon className="size-4" aria-hidden="true" />
              <span>{tab.label}</span>
            </NavLink>
          );
        })}
      </nav>

      <div className="space-y-6">{children}</div>
    </div>
  );
}
