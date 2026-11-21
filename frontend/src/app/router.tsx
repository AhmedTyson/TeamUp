/**
 * @date 2026-11-21
 * @purpose Client-side route table mapped to TeamUp platform feature modules.
 */
import { Route, Routes } from "react-router-dom";
import { API_ENDPOINTS } from "@/api";
import { AppShell } from "@/components/layout/AppShell";
import { HomePage } from "@/pages/HomePage";
import { ModulePlaceholderPage } from "@/pages/ModulePlaceholderPage";
import { NotFoundPage } from "@/pages/NotFoundPage";

export function AppRouter() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route index element={<HomePage />} />
        <Route
          path="projects"
          element={
            <ModulePlaceholderPage
              title="Projects & Teams"
              description="Cross-faculty project discovery, required skills matching, applications, and Kanban task boards."
              endpoint={API_ENDPOINTS.projects.list}
            />
          }
        />
        <Route
          path="organizations"
          element={
            <ModulePlaceholderPage
              title="Student Organizations"
              description="University clubs, student chapters, leadership rosters, and membership applications."
              endpoint={API_ENDPOINTS.organizations.list}
            />
          }
        />
        <Route
          path="events"
          element={
            <ModulePlaceholderPage
              title="Campus Events & Sessions"
              description="Multi-session workshops, seat capacity registration, and QR/PIN attendance verification."
              endpoint={API_ENDPOINTS.events.list}
            />
          }
        />
        <Route
          path="gamification"
          element={
            <ModulePlaceholderPage
              title="Gamification & Badges"
              description="Immutable score ledger, progressive level ranks, achievement badges, and notifications."
              endpoint={API_ENDPOINTS.gamification.badges}
            />
          }
        />
        <Route
          path="auth"
          element={
            <ModulePlaceholderPage
              title="Student Sign In & Onboarding"
              description="JWT authentication, refresh token rotation, and university/faculty catalog registration."
              endpoint={API_ENDPOINTS.auth.login}
            />
          }
        />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
