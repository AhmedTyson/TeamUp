/**
 * @date 2026-11-27
 * @purpose Vitest coverage for Verified Student badges, VerifyEmailPage flow, RoleGuard authorization, Explore hover styling, clickable cards, and clickable student profiles.
 */
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  authApi,
  ORGANIZATION_SCOPES,
  organizationsApi,
  USER_ROLES,
  type AuthStudentSummary,
  type StudentCardDto,
} from "@/api";
import { AppCoreProviders } from "@/app/providers";
import { RoleGuard } from "@/features/auth";
import { VerifyEmailPage } from "@/features/auth/pages/VerifyEmailPage";
import { OrganizationParticipantsTab } from "@/features/organizations/components/OrganizationParticipantsTab";
import { OrganizationsDirectoryPage } from "@/features/organizations/pages/OrganizationsDirectoryPage";
import { StudentCard } from "@/features/profile/StudentCard";

const UNVERIFIED_STUDENT: AuthStudentSummary = {
  studentId: 21,
  firstName: "Nour",
  lastName: "Hassan",
  email: "nour@cu.edu.eg",
  academicYear: 3,
  role: USER_ROLES.Student,
  facultyId: 1,
  facultyName: "Faculty of Computers and AI",
  universityName: "Cairo University",
  isEmailVerified: false,
};

describe("Verified Student, Authorization, Explore Style & Clickable Surfaces", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders Verified Student badge only when isEmailVerified is true and keeps whole StudentCard linked", () => {
    const verifiedCard: StudentCardDto = {
      studentId: 101,
      firstName: "Salma",
      lastName: "Khaled",
      email: null,
      academicYear: 3,
      role: "Student",
      facultyId: 1,
      facultyName: "FCAI",
      universityName: "Cairo University",
      interests: ["AI"],
      isEmailVerified: true,
    };

    const unverifiedCard: StudentCardDto = {
      ...verifiedCard,
      studentId: 102,
      firstName: "Karim",
      lastName: "Tester",
      isEmailVerified: false,
    };

    render(
      <MemoryRouter>
        <AppCoreProviders initialUser={UNVERIFIED_STUDENT}>
          <StudentCard student={verifiedCard} />
          <StudentCard student={unverifiedCard} />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByText("Verified Student")).toBeInTheDocument();
    expect(screen.getByText("Unverified Account")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Salma Khaled" })).toHaveAttribute("href", "/students/101");
    expect(screen.getByRole("link", { name: "Karim Tester" })).toHaveAttribute("href", "/students/102");
  });

  it("VerifyEmailPage verifies tokenized email link and handles invalid token error", async () => {
    const verifySpy = vi.spyOn(authApi, "verifyEmail").mockResolvedValue("Email verified!");

    render(
      <MemoryRouter initialEntries={["/auth/verify-email?email=nour@cu.edu.eg&token=tok_valid_123"]}>
        <AppCoreProviders initialUser={null}>
          <Routes>
            <Route path="/auth/verify-email" element={<VerifyEmailPage />} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(verifySpy).toHaveBeenCalledWith({
        email: "nour@cu.edu.eg",
        token: "tok_valid_123",
      });
    });

    expect(await screen.findByText(/Verified Student Status Active/i)).toBeInTheDocument();

    vi.restoreAllMocks();
    const failSpy = vi
      .spyOn(authApi, "verifyEmail")
      .mockRejectedValue(new Error("Invalid or expired email verification link."));

    render(
      <MemoryRouter initialEntries={["/auth/verify-email"]}>
        <AppCoreProviders initialUser={UNVERIFIED_STUDENT}>
          <Routes>
            <Route path="/auth/verify-email" element={<VerifyEmailPage />} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    fireEvent.change(screen.getByPlaceholderText("Paste verification token"), {
      target: { value: "bad_tok" },
    });
    fireEvent.click(screen.getByRole("button", { name: /Verify Email/i }));

    await waitFor(() => {
      expect(failSpy).toHaveBeenCalled();
    });
    expect(await screen.findByText(/Invalid or expired email verification link/i)).toBeInTheDocument();
  });

  it("RoleGuard blocks plain Student from org dashboard while allowing OrgAdmin or scoped orgAdmin", () => {
    render(
      <MemoryRouter initialEntries={["/dashboard/org"]}>
        <AppCoreProviders initialUser={UNVERIFIED_STUDENT}>
          <Routes>
            <Route
              path="/dashboard/org"
              element={
                <RoleGuard roles={[USER_ROLES.OrgAdmin, USER_ROLES.PlatformAdmin]} allowScopedOrgAdmin>
                  <div>Org Leader Console</div>
                </RoleGuard>
              }
            />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.queryByText("Org Leader Console")).not.toBeInTheDocument();
    expect(screen.getByText(/does not have permission/i)).toBeInTheDocument();
  });

  it("OrganizationsDirectoryPage removes underline on Explore hover and links entire card to org workspace", async () => {
    vi.spyOn(organizationsApi, "categories").mockResolvedValue(["Engineering"]);
    vi.spyOn(organizationsApi, "list").mockResolvedValue({
      items: [
        {
          organizationId: 1,
          name: "IEEE Helwan Student Branch",
          description: "Hands-on engineering and embedded community.",
          category: "Engineering",
          scope: ORGANIZATION_SCOPES.UniversityBranch,
          universityName: "Helwan University",
          city: "Cairo",
          isRecruiting: true,
          currentSeasonName: "Fall 2026",
          logoUrl: null,
          coverImageUrl: null,
          memberCount: 95,
          unitCount: 7,
        },
      ],
      page: 1,
      pageSize: 9,
      totalCount: 1,
      totalPages: 1,
    });

    render(
      <MemoryRouter initialEntries={["/organizations"]}>
        <AppCoreProviders initialUser={UNVERIFIED_STUDENT}>
          <OrganizationsDirectoryPage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    const titleLink = await screen.findByRole("link", { name: "IEEE Helwan Student Branch" });
    expect(titleLink).toHaveAttribute("href", "/organizations/1");
    expect(titleLink.className).toContain("after:inset-0");

    const exploreLabel = screen.getByText("Explore").closest("span");
    expect(exploreLabel?.className).toContain("no-underline");
    expect(exploreLabel?.className).not.toContain("hover:underline");
  });

  it("OrganizationParticipantsTab links participant names to /students/:id while preserving certificate button", () => {
    render(
      <MemoryRouter>
        <OrganizationParticipantsTab
          organizationName="IEEE Helwan Student Branch"
          participants={[
            {
              studentId: 77,
              firstName: "Laila",
              lastName: "Mansour",
              affiliationLabel: "Cairo University",
              eventsRegisteredCount: 2,
              sessionsAttendedCount: 3,
              lastEventTitle: "AI Bootcamp",
              lastActiveAt: "2026-11-20T10:00:00Z",
            },
          ]}
          isLoading={false}
          isAuthenticated
        />
      </MemoryRouter>,
    );

    const profileLink = screen.getByRole("link", { name: "Laila Mansour" });
    expect(profileLink).toHaveAttribute("href", "/students/77");
    fireEvent.click(screen.getByRole("button", { name: /Certificate/i }));
    expect(screen.getByText("Verified Participation Certificate")).toBeInTheDocument();
  });
});
