/**
 * @date 2026-11-22
 * @purpose Unit and integration tests for M1 Phase 1 (universitiesApi, Login, Register cascade, Forgot/Reset Password, Navbar & AppShell).
 */
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  apiClient,
  ApiClientError,
  authApi,
  universitiesApi,
  USER_ROLES,
  type AuthStudentSummary,
  type UniversityCatalogItem,
} from "@/api";
import { AppCoreProviders } from "@/app/providers";
import { AppShell } from "@/components/layout/AppShell";
import {
  ForgotPasswordPage,
  LoginPage,
  RegisterPage,
  ResetPasswordPage,
} from "@/features/auth";
import { tokenStorage } from "@/lib/tokenStorage";

const MOCK_CATALOG: UniversityCatalogItem[] = [
  {
    universityId: 1,
    name: "Cairo University",
    city: "Giza",
    country: "Egypt",
    website: "https://cu.edu.eg",
    faculties: [
      { facultyId: 11, name: "Computer Science", code: "CS" },
      { facultyId: 12, name: "Engineering", code: "ENG" },
    ],
  },
  {
    universityId: 2,
    name: "Ain Shams University",
    city: "Cairo",
    country: "Egypt",
    website: "https://asu.edu.eg",
    faculties: [{ facultyId: 21, name: "Computer & Information Sciences", code: "CIS" }],
  },
];

const STUDENT_USER: AuthStudentSummary = {
  studentId: 10,
  firstName: "Nour",
  lastName: "Hassan",
  email: "nour@cu.edu.eg",
  academicYear: 3,
  role: USER_ROLES.Student,
  facultyId: 11,
  facultyName: "Computer Science",
  universityName: "Cairo University",
};

const ADMIN_USER: AuthStudentSummary = {
  ...STUDENT_USER,
  studentId: 1,
  firstName: "Tyson",
  role: USER_ROLES.PlatformAdmin,
};

describe("M1 Phase 1 — Auth Pages, Academic Catalog API & AppShell", () => {
  beforeEach(() => {
    tokenStorage.clear();
    vi.restoreAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("exposes full universities and faculties CRUD wrappers matching OpenAPI routes", async () => {
    const getSpy = vi.spyOn(apiClient, "get").mockResolvedValue({
      data: {
        success: true,
        message: "OK",
        statusCode: 200,
        data: { items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
        timestamp: new Date().toISOString(),
      },
      status: 200,
    } as never);
    const postSpy = vi.spyOn(apiClient, "post").mockResolvedValue({
      data: {
        success: true,
        message: "Created",
        statusCode: 201,
        data: { facultyId: 31, name: "Medicine", code: "MED", universityId: 1, universityName: "Cairo", studentCount: 0 },
        timestamp: new Date().toISOString(),
      },
      status: 201,
    } as never);
    const putSpy = vi.spyOn(apiClient, "put").mockResolvedValue({
      data: {
        success: true,
        message: "Updated",
        statusCode: 200,
        data: { facultyId: 31, name: "Medicine", code: "MED", universityId: 1, universityName: "Cairo", studentCount: 0 },
        timestamp: new Date().toISOString(),
      },
      status: 200,
    } as never);
    const deleteSpy = vi.spyOn(apiClient, "delete").mockResolvedValue({
      data: "",
      status: 204,
    } as never);

    await universitiesApi.list({ search: "Cairo", page: 1, pageSize: 10 });
    expect(getSpy).toHaveBeenCalledWith("/api/v1/universities", {
      params: { search: "Cairo", page: 1, pageSize: 10 },
      signal: undefined,
    });

    await universitiesApi.createFaculty(1, { name: "Medicine", code: "MED" });
    expect(postSpy).toHaveBeenCalledWith("/api/v1/universities/1/faculties", {
      name: "Medicine",
      code: "MED",
    });

    await universitiesApi.updateFaculty(31, { name: "Medicine", code: "MED" });
    expect(putSpy).toHaveBeenCalledWith("/api/v1/faculties/31", {
      name: "Medicine",
      code: "MED",
    });

    await universitiesApi.remove(1);
    expect(deleteSpy).toHaveBeenCalledWith("/api/v1/universities/1");
    await universitiesApi.removeFaculty(31);
    expect(deleteSpy).toHaveBeenCalledWith("/api/v1/faculties/31");
  });

  it("renders session_expired banner, maps 400 field errors & 429 cooldown, and logs in on LoginPage", async () => {
    const loginSpy = vi
      .spyOn(authApi, "login")
      .mockRejectedValueOnce(
        new ApiClientError("Validation failed", 400, [
          { field: "Email", message: "Institutional email is required." },
        ]),
      )
      .mockRejectedValueOnce(new ApiClientError("Rate limit exceeded.", 429, [], 15))
      .mockResolvedValueOnce({
        accessToken: "access-token",
        refreshToken: "refresh-token",
        expiresAt: new Date(Date.now() + 900_000).toISOString(),
        student: STUDENT_USER,
      });

    const { unmount } = render(
      <MemoryRouter initialEntries={["/auth/login?reason=session_expired"]}>
        <AppCoreProviders initialUser={null}>
          <Routes>
            <Route path="/auth/login" element={<LoginPage />} />
            <Route path="/projects" element={<h1>Projects Workspace</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByText("Session expired")).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/university email/i), {
      target: { value: "nour@cu.edu.eg" },
    });
    fireEvent.change(screen.getByLabelText(/^password/i), {
      target: { value: "Password123" },
    });

    // Attempt 1 -> 400 field mapping
    fireEvent.click(screen.getByRole("button", { name: /^sign in$/i }));
    await waitFor(() => {
      expect(screen.getByText("Institutional email is required.")).toBeInTheDocument();
    });

    // Attempt 2 -> 429 cooldown UI
    fireEvent.click(screen.getByRole("button", { name: /^sign in$/i }));
    await waitFor(() => {
      expect(screen.getByRole("button", { name: /wait 15s to retry/i })).toBeDisabled();
    });

    unmount();

    // Render fresh page without active cooldown for Attempt 3 -> 200 success redirect
    render(
      <MemoryRouter initialEntries={["/auth/login"]}>
        <AppCoreProviders initialUser={null}>
          <Routes>
            <Route path="/auth/login" element={<LoginPage />} />
            <Route path="/projects" element={<h1>Projects Workspace</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    fireEvent.change(screen.getByLabelText(/university email/i), {
      target: { value: "nour@cu.edu.eg" },
    });
    fireEvent.change(screen.getByLabelText(/^password/i), {
      target: { value: "Password123" },
    });
    fireEvent.click(screen.getByRole("button", { name: /^sign in$/i }));

    await waitFor(() => {
      expect(screen.getByText("Projects Workspace")).toBeInTheDocument();
    });
    expect(loginSpy).toHaveBeenCalledTimes(3);
  });

  it("loads /universities/catalog, cascades University -> Faculty, resets faculty on university change, and registers student", async () => {
    vi.spyOn(universitiesApi, "getCatalog").mockResolvedValueOnce(MOCK_CATALOG);
    const registerSpy = vi.spyOn(authApi, "register").mockResolvedValueOnce({
      accessToken: "reg-access",
      refreshToken: "reg-refresh",
      expiresAt: new Date(Date.now() + 900_000).toISOString(),
      student: STUDENT_USER,
    });

    render(
      <MemoryRouter initialEntries={["/auth/register"]}>
        <AppCoreProviders initialUser={null}>
          <Routes>
            <Route path="/auth/register" element={<RegisterPage />} />
            <Route path="/projects" element={<h1>Projects Hub</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    const universitySelect = await screen.findByLabelText(/^university\*?$/i);
    await waitFor(() => {
      expect(universitySelect).not.toBeDisabled();
    });

    const facultySelect = screen.getByLabelText(/^faculty\*?$/i) as HTMLSelectElement;
    expect(facultySelect).toBeDisabled();

    // Select Cairo University -> unlocks CS & Engineering
    fireEvent.change(universitySelect, { target: { value: "1" } });
    expect(facultySelect).not.toBeDisabled();
    expect(screen.getByRole("option", { name: /computer science \(cs\)/i })).toBeInTheDocument();

    fireEvent.change(facultySelect, { target: { value: "11" } });
    expect(facultySelect.value).toBe("11");

    // Switch to Ain Shams University -> resets facultyId cleanly
    fireEvent.change(universitySelect, { target: { value: "2" } });
    expect(facultySelect.value).toBe("");
    expect(
      screen.getByRole("option", { name: /computer & information sciences \(cis\)/i }),
    ).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/first name/i), { target: { value: "Nour" } });
    fireEvent.change(screen.getByLabelText(/last name/i), { target: { value: "Hassan" } });
    fireEvent.change(screen.getByLabelText(/university email/i), {
      target: { value: "nour@asu.edu.eg" },
    });
    fireEvent.change(facultySelect, { target: { value: "21" } });
    fireEvent.change(screen.getByLabelText(/academic year/i), { target: { value: "3" } });
    fireEvent.change(screen.getByLabelText(/^password\*?$/i), {
      target: { value: "CampusPass1" },
    });

    fireEvent.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(registerSpy).toHaveBeenCalledWith({
        firstName: "Nour",
        lastName: "Hassan",
        email: "nour@asu.edu.eg",
        password: "CampusPass1",
        academicYear: 3,
        facultyId: 21,
      });
    });
    expect(await screen.findByText("Projects Hub")).toBeInTheDocument();
  });

  it("handles ForgotPasswordPage opaque confirmation and ResetPasswordPage query prefill", async () => {
    const forgotSpy = vi.spyOn(authApi, "forgotPassword").mockResolvedValueOnce({
      message: "If an account exists, reset instructions were dispatched.",
    });
    const resetSpy = vi
      .spyOn(authApi, "resetPassword")
      .mockResolvedValueOnce("Password reset successfully.");

    const { unmount } = render(
      <MemoryRouter initialEntries={["/auth/forgot-password"]}>
        <AppCoreProviders initialUser={null}>
          <Routes>
            <Route path="/auth/forgot-password" element={<ForgotPasswordPage />} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    fireEvent.change(screen.getByLabelText(/university email/i), {
      target: { value: "nour@cu.edu.eg" },
    });
    fireEvent.click(screen.getByRole("button", { name: /send recovery instructions/i }));

    await waitFor(() => {
      expect(screen.getByText(/recovery request received/i)).toBeInTheDocument();
    });
    expect(forgotSpy).toHaveBeenCalledWith({ email: "nour@cu.edu.eg" });
    expect(screen.getByRole("link", { name: /continue to reset password/i })).toHaveAttribute(
      "href",
      "/auth/reset-password?email=nour%40cu.edu.eg",
    );

    unmount();

    render(
      <MemoryRouter
        initialEntries={["/auth/reset-password?email=nour%40cu.edu.eg&token=RESET-XYZ"]}
      >
        <AppCoreProviders initialUser={null}>
          <Routes>
            <Route path="/auth/reset-password" element={<ResetPasswordPage />} />
            <Route path="/auth/login" element={<h1>Login After Reset</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByLabelText(/university email/i)).toHaveValue("nour@cu.edu.eg");
    expect(screen.getByLabelText(/recovery token/i)).toHaveValue("RESET-XYZ");

    fireEvent.change(screen.getByLabelText(/^new password\*?$/i), {
      target: { value: "NewSecure9" },
    });
    fireEvent.change(screen.getByLabelText(/^confirm new password\*?$/i), {
      target: { value: "NewSecure9" },
    });

    fireEvent.click(screen.getByRole("button", { name: /update password/i }));

    await waitFor(() => {
      expect(resetSpy).toHaveBeenCalledWith({
        email: "nour@cu.edu.eg",
        token: "RESET-XYZ",
        newPassword: "NewSecure9",
      });
    });
    expect(await screen.findByText("Login After Reset")).toBeInTheDocument();
  });

  it("renders upgraded AppShell with PlatformAdmin link, custom M4 slots, mobile drawer Escape dismissal, and Sign Out", async () => {
    const logoutSpy = vi.spyOn(authApi, "logout").mockResolvedValueOnce(undefined);
    tokenStorage.setTokens("jwt-token", "refresh-token");

    render(
      <MemoryRouter initialEntries={["/projects"]}>
        <AppCoreProviders initialUser={ADMIN_USER}>
          <Routes>
            <Route
              element={
                <AppShell
                  xpChipSlot={<span data-testid="m4-xp-slot">Lvl 4 · 420 XP</span>}
                  notificationSlot={<span data-testid="m4-bell-slot">3 Alerts</span>}
                />
              }
            >
              <Route path="/projects" element={<h1>Projects Content</h1>} />
            </Route>
            <Route path="/auth/login" element={<h1>Signed Out Login</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByTestId("m4-xp-slot")).toBeInTheDocument();
    expect(screen.getByTestId("m4-bell-slot")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /^admin$/i })).toBeInTheDocument();

    // Open mobile drawer and close with Escape
    const toggleBtn = screen.getByRole("button", { name: /toggle navigation menu/i });
    fireEvent.click(toggleBtn);
    expect(screen.getByRole("navigation", { name: /mobile navigation/i })).toBeInTheDocument();

    fireEvent.keyDown(window, { key: "Escape" });
    await waitFor(() => {
      expect(screen.queryByRole("navigation", { name: /mobile navigation/i })).not.toBeInTheDocument();
    });

    // Sign out triggers POST /auth/logout and navigates to /auth/login
    fireEvent.click(screen.getByRole("button", { name: /sign out/i }));
    await waitFor(() => {
      expect(logoutSpy).toHaveBeenCalledTimes(1);
    });
    expect(await screen.findByText("Signed Out Login")).toBeInTheDocument();
  });

  it("mounts default XpLevelChip and NotificationBell in AppShell with live score, unread badge, and mark-all-read", async () => {
    vi.spyOn(apiClient, "get").mockImplementation(async (url: string) => {
      if (url.includes("/score")) {
        return {
          data: {
            success: true,
            message: "OK",
            statusCode: 200,
            data: {
              studentId: 10,
              totalPoints: 450,
              level: 3,
              currentLevelStart: 400,
              nextLevelThreshold: 900,
              pointsToNext: 450,
              progress: 0.1,
            },
            timestamp: new Date().toISOString(),
          },
          status: 200,
        } as never;
      }
      if (url.includes("/notifications")) {
        return {
          data: {
            success: true,
            message: "OK",
            statusCode: 200,
            data: [
              {
                notificationId: 101,
                studentId: 10,
                title: "Task Done +50 XP",
                message: "Your Kanban task was completed.",
                type: 1,
                isRead: false,
                createdAt: "2026-11-22T00:00:00Z",
              },
            ],
            timestamp: new Date().toISOString(),
          },
          status: 200,
        } as never;
      }
      return { data: { status: "Healthy" }, status: 200 } as never;
    });

    const patchSpy = vi.spyOn(apiClient, "patch").mockResolvedValue({
      data: "",
      status: 204,
    } as never);

    render(
      <MemoryRouter initialEntries={["/projects"]}>
        <AppCoreProviders initialUser={STUDENT_USER}>
          <Routes>
            <Route element={<AppShell />}>
              <Route path="/projects" element={<h1>Projects Content</h1>} />
            </Route>
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText("Lvl 3")).toBeInTheDocument();
    expect(screen.getByText("450 XP")).toBeInTheDocument();

    const bellBtn = await screen.findByRole("button", { name: /open notifications \(1 unread\)/i });
    fireEvent.click(bellBtn);

    expect(await screen.findByText("Task Done +50 XP")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /mark all read/i }));
    await waitFor(() => {
      expect(patchSpy).toHaveBeenCalledWith("/api/v1/notifications/mark-all-read");
    });
  });
});
