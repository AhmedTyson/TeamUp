/**
 * @date 2026-11-21
 * @purpose Unit tests for AuthProvider JWT claim hints (isPlatformAdmin, isOrgAdminOf) and ProtectedRoute/RoleGuard/GuestOnlyRoute.
 */
import { act, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes, useLocation } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { authApi, authClientEvents, USER_ROLES, type AuthStudentSummary } from "@/api";
import { AppCoreProviders } from "@/app/providers";
import { GuestOnlyRoute, ProtectedRoute, RoleGuard, useAuth } from "@/features/auth";
import { tokenStorage } from "@/lib/tokenStorage";

function createFakeJwt(payload: Record<string, unknown>): string {
  const encode = (obj: object) =>
    btoa(JSON.stringify(obj)).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "");
  return `${encode({ alg: "HS256", typ: "JWT" })}.${encode(payload)}.sig`;
}

const STUDENT_USER: AuthStudentSummary = {
  studentId: 10,
  firstName: "Nour",
  lastName: "Hassan",
  email: "nour@teamup.edu",
  academicYear: 3,
  role: USER_ROLES.Student,
  facultyId: 1,
  facultyName: "Computer Science",
  universityName: "Cairo University",
};

const ORG_ADMIN_USER: AuthStudentSummary = {
  ...STUDENT_USER,
  studentId: 11,
  role: USER_ROLES.OrgAdmin,
};

const PLATFORM_ADMIN_USER: AuthStudentSummary = {
  ...STUDENT_USER,
  studentId: 1,
  role: USER_ROLES.PlatformAdmin,
};

function AuthProbe() {
  const { isAuthenticated, isPlatformAdmin, isOrgAdminOf } = useAuth();
  return (
    <div>
      <span data-testid="auth">{String(isAuthenticated)}</span>
      <span data-testid="platform">{String(isPlatformAdmin)}</span>
      <span data-testid="org-5">{String(isOrgAdminOf(5))}</span>
      <span data-testid="org-9">{String(isOrgAdminOf(9))}</span>
    </div>
  );
}

describe("AuthContext and route guards", () => {
  beforeEach(() => {
    tokenStorage.clear();
  });

  it("enforces scoped org_admin UI hints for OrgAdmin vs PlatformAdmin", () => {
    const token = createFakeJwt({
      sub: "11",
      role: "OrgAdmin",
      org_admin: ["5"],
      exp: Math.floor(Date.now() / 1000) + 900,
    });
    tokenStorage.setTokens(token, "refresh-token");

    const { unmount } = render(
      <MemoryRouter>
        <AppCoreProviders initialUser={ORG_ADMIN_USER}>
          <AuthProbe />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByTestId("auth")).toHaveTextContent("true");
    expect(screen.getByTestId("platform")).toHaveTextContent("false");
    expect(screen.getByTestId("org-5")).toHaveTextContent("true");
    expect(screen.getByTestId("org-9")).toHaveTextContent("false");

    unmount();

    render(
      <MemoryRouter>
        <AppCoreProviders initialUser={PLATFORM_ADMIN_USER}>
          <AuthProbe />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByTestId("platform")).toHaveTextContent("true");
    expect(screen.getByTestId("org-9")).toHaveTextContent("true");
  });

  it("handles login, logout, and session-expired redirect cleanly", async () => {
    const loginToken = createFakeJwt({
      sub: "11",
      role: "OrgAdmin",
      org_admin: "7",
      exp: Math.floor(Date.now() / 1000) + 900,
    });
    vi.spyOn(authApi, "login").mockResolvedValueOnce({
      accessToken: loginToken,
      refreshToken: "refresh-7",
      expiresAt: new Date(Date.now() + 900_000).toISOString(),
      student: ORG_ADMIN_USER,
    });
    vi.spyOn(authApi, "logout").mockResolvedValueOnce(undefined);

    function LocationEcho() {
      const loc = useLocation();
      return <span data-testid="loc">{`${loc.pathname}${loc.search}`}</span>;
    }

    function LoginDriver() {
      const { login, logout, isAuthenticated, isOrgAdminOf } = useAuth();
      return (
        <div>
          <span data-testid="logged-in">{String(isAuthenticated)}</span>
          <span data-testid="org-7">{String(isOrgAdminOf(7))}</span>
          <button
            type="button"
            onClick={() => {
              void login({ email: "nour@teamup.edu", password: "Password1" });
            }}
          >
            Do Login
          </button>
          <button
            type="button"
            onClick={() => {
              void logout();
            }}
          >
            Do Logout
          </button>
        </div>
      );
    }

    render(
      <MemoryRouter initialEntries={["/projects"]}>
        <AppCoreProviders initialUser={null}>
          <LocationEcho />
          <Routes>
            <Route path="/projects" element={<LoginDriver />} />
            <Route path="/auth/login" element={<h1>Session Expired Login</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByTestId("logged-in")).toHaveTextContent("false");

    await act(async () => {
      screen.getByRole("button", { name: "Do Login" }).click();
    });

    expect(screen.getByTestId("logged-in")).toHaveTextContent("true");
    expect(screen.getByTestId("org-7")).toHaveTextContent("true");
    expect(tokenStorage.getAccessToken()).toBe(loginToken);

    await act(async () => {
      authClientEvents.emitSessionExpired();
    });

    expect(screen.getByText("Session Expired Login")).toBeInTheDocument();
    expect(screen.getByTestId("loc")).toHaveTextContent("/auth/login?reason=session_expired");
    expect(tokenStorage.getAccessToken()).toBeNull();
  });

  it("redirects unauthenticated users from ProtectedRoute to /auth/login", () => {
    render(
      <MemoryRouter initialEntries={["/projects"]}>
        <AppCoreProviders initialUser={null}>
          <Routes>
            <Route path="/auth/login" element={<h1>Login Screen</h1>} />
            <Route element={<ProtectedRoute />}>
              <Route path="/projects" element={<h1>Projects Workspace</h1>} />
            </Route>
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByText("Login Screen")).toBeInTheDocument();
  });

  it("blocks Student from PlatformAdmin RoleGuard with ForbiddenBanner and allows PlatformAdmin", () => {
    const { unmount } = render(
      <MemoryRouter initialEntries={["/admin"]}>
        <AppCoreProviders initialUser={STUDENT_USER}>
          <Routes>
            <Route element={<RoleGuard roles={[USER_ROLES.PlatformAdmin]} />}>
              <Route path="/admin" element={<h1>Admin Console</h1>} />
            </Route>
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByRole("alert")).toHaveTextContent(/access restricted/i);
    expect(screen.queryByText("Admin Console")).not.toBeInTheDocument();
    unmount();

    render(
      <MemoryRouter initialEntries={["/admin"]}>
        <AppCoreProviders initialUser={PLATFORM_ADMIN_USER}>
          <Routes>
            <Route element={<RoleGuard roles={[USER_ROLES.PlatformAdmin]} />}>
              <Route path="/admin" element={<h1>Admin Console</h1>} />
            </Route>
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByText("Admin Console")).toBeInTheDocument();
  });

  it("redirects authenticated users away from GuestOnlyRoute", () => {
    render(
      <MemoryRouter initialEntries={["/auth/login"]}>
        <AppCoreProviders initialUser={STUDENT_USER}>
          <Routes>
            <Route element={<GuestOnlyRoute redirectTo="/projects" />}>
              <Route path="/auth/login" element={<h1>Login Screen</h1>} />
            </Route>
            <Route path="/projects" element={<h1>Projects Hub</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByText("Projects Hub")).toBeInTheDocument();
  });

  it("preserves safe location.state.from on GuestOnlyRoute and rejects unsafe return paths", () => {
    function CurrentLocation() {
      const loc = useLocation();
      return <span data-testid="dest">{`${loc.pathname}${loc.search}`}</span>;
    }

    const { unmount } = render(
      <MemoryRouter
        initialEntries={[
          { pathname: "/auth/login", state: { from: { pathname: "/leaderboard", search: "?tab=top" } } },
        ]}
      >
        <AppCoreProviders initialUser={STUDENT_USER}>
          <CurrentLocation />
          <Routes>
            <Route element={<GuestOnlyRoute redirectTo="/projects" />}>
              <Route path="/auth/login" element={<h1>Login Screen</h1>} />
            </Route>
            <Route path="/leaderboard" element={<h1>Leaderboard View</h1>} />
            <Route path="/projects" element={<h1>Projects Hub</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByText("Leaderboard View")).toBeInTheDocument();
    expect(screen.getByTestId("dest")).toHaveTextContent("/leaderboard?tab=top");
    unmount();

    render(
      <MemoryRouter
        initialEntries={[
          { pathname: "/auth/login", state: { from: { pathname: "//evil.example", search: "" } } },
        ]}
      >
        <AppCoreProviders initialUser={STUDENT_USER}>
          <CurrentLocation />
          <Routes>
            <Route element={<GuestOnlyRoute redirectTo="/projects" />}>
              <Route path="/auth/login" element={<h1>Login Screen</h1>} />
            </Route>
            <Route path="/projects" element={<h1>Projects Hub</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByText("Projects Hub")).toBeInTheDocument();
    expect(screen.getByTestId("dest")).toHaveTextContent("/projects");
  });
});
