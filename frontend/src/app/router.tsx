/**
 * @date 2026-11-21
 * @purpose Modular root client-side router composing all 4 zero-conflict developer route fragments with Suspense fallback.
 */
import { Loader2 } from "lucide-react";
import { Suspense } from "react";
import { useRoutes, type RouteObject } from "react-router-dom";
import { AppShell } from "@/components/layout/AppShell";
import { NotFoundPage } from "@/pages/NotFoundPage";
import { authRoutes } from "./routes/authRoutes";
import { communityRoutes } from "./routes/communityRoutes";
import { orgEventRoutes } from "./routes/orgEventRoutes";
import { projectRoutes } from "./routes/projectRoutes";

const appRouteObjects: RouteObject[] = [
  {
    element: <AppShell />,
    children: [
      ...communityRoutes,
      ...authRoutes,
      ...projectRoutes,
      ...orgEventRoutes,
      { path: "*", element: <NotFoundPage /> },
    ],
  },
];

function RouteLoadingFallback() {
  return (
    <div
      role="status"
      aria-live="polite"
      className="flex min-h-[40vh] flex-col items-center justify-center gap-3 text-sm text-[#9ca3af]"
    >
      <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
      <span>Loading workspace…</span>
    </div>
  );
}

export function AppRouter() {
  const element = useRoutes(appRouteObjects);
  return <Suspense fallback={<RouteLoadingFallback />}>{element}</Suspense>;
}
