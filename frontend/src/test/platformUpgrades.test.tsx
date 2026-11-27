/**
 * @date 2026-11-24
 * @purpose Vitest + React Testing Library suite covering English-only Navbar & complete CommandPalette, Role Dashboards (Student, OrgAdmin, PlatformAdmin), flexible onboarding, 4-tier Hierarchy, Recruitment Kanban, Digital Pass + .ics, Stage Projector, and ApiHealthPill.
 */
import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import {
  AFFILIATION_TYPES,
  authApi,
  eventsApi,
  EVENT_TYPES,
  gamificationApi,
  healthApi,
  notificationsApi,
  organizationsApi,
  ORGANIZATION_SCOPES,
  ORGANIZATION_UNIT_TYPES,
  projectsApi,
  RECRUITMENT_STAGES,
  universitiesApi,
  USER_ROLES,
  workspacesApi,
  type AuthStudentSummary,
  type EventSummary,
  type MembershipApplication,
  type OrganizationHierarchy,
} from "@/api";
import { AppCoreProviders } from "@/app/providers";
import { ApiHealthPill } from "@/components/layout/ApiHealthPill";
import { AppShell } from "@/components/layout/AppShell";
import { CommandPalette } from "@/components/layout/CommandPalette";
import { Navbar } from "@/components/layout/Navbar";
import { LandingFooterSection } from "@/features/landing/LandingFooterSection";
import { RegisterPage } from "@/features/auth/pages/RegisterPage";
import {
  OrgAdminDashboardPage,
  RoleDashboardRouterPage,
  StudentDashboardPage,
} from "@/features/dashboard";
import { EventDigitalPassCard } from "@/features/events/components/EventDigitalPassCard";
import { StageProjectorOverlay } from "@/features/events/components/StageProjectorOverlay";
import { OrganizationHierarchyTab } from "@/features/organizations/components/OrganizationHierarchyTab";
import { RecruitmentPipelineTab } from "@/features/organizations/components/RecruitmentPipelineTab";
import { buildEventIcsContent } from "@/lib/calendarExport";

function makeStudent(overrides: Partial<AuthStudentSummary> = {}): AuthStudentSummary {
  return {
    studentId: 99,
    firstName: "Ziad",
    lastName: "Tarek",
    email: "ziad@indie.dev",
    academicYear: 1,
    affiliationType: AFFILIATION_TYPES.IndependentLearner,
    city: "Cairo",
    role: USER_ROLES.Student,
    facultyId: null,
    facultyName: "",
    universityName: "",
    photoUrl: null,
    ...overrides,
  };
}

describe("20-Item Plan & Role Dashboards Suite", () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.restoreAllMocks();
  });

  it("supports flexible IndependentLearner registration without university/faculty selection", async () => {
    vi.spyOn(universitiesApi, "getCatalog").mockResolvedValueOnce([]);
    const registerSpy = vi.spyOn(authApi, "register").mockResolvedValueOnce({
      accessToken: "indie-access",
      refreshToken: "indie-refresh",
      expiresAt: new Date(Date.now() + 900_000).toISOString(),
      student: makeStudent(),
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

    const affiliationSelect = screen.getByLabelText(/pathway \/ affiliation/i);
    fireEvent.change(affiliationSelect, {
      target: { value: AFFILIATION_TYPES.IndependentLearner },
    });

    expect(screen.queryByLabelText(/^university\*?$/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/^faculty\*?$/i)).not.toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/first name/i), { target: { value: "Ziad" } });
    fireEvent.change(screen.getByLabelText(/last name/i), { target: { value: "Tarek" } });
    fireEvent.change(screen.getByLabelText(/city/i), { target: { value: "Cairo" } });
    fireEvent.change(screen.getByLabelText(/^(university email|email address)\*?$/i), {
      target: { value: "ziad@indie.dev" },
    });
    fireEvent.change(screen.getByLabelText(/^password\*?$/i), {
      target: { value: "TeamUp!2026Strong" },
    });

    fireEvent.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(registerSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          firstName: "Ziad",
          lastName: "Tarek",
          email: "ziad@indie.dev",
          affiliationType: AFFILIATION_TYPES.IndependentLearner,
          city: "Cairo",
          facultyId: null,
        }),
      );
    });
    expect(await screen.findByText("Projects Hub")).toBeInTheDocument();
  });

  it("renders English-only Navbar with role-specific Dashboard/Admin links and no language switcher", () => {
    const studentView = render(
      <MemoryRouter initialEntries={["/"]}>
        <AppCoreProviders
          initialUser={makeStudent({
            firstName: "Salma",
            role: USER_ROLES.Student,
          })}
        >
          <Navbar xpChipSlot={<span />} notificationSlot={<span />} />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.queryByRole("button", { name: /Switch language/i })).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Dashboard" })).toHaveAttribute("href", "/dashboard");
    studentView.unmount();

    const orgAdminView = render(
      <MemoryRouter initialEntries={["/"]}>
        <AppCoreProviders
          initialUser={makeStudent({
            firstName: "Nour",
            role: USER_ROLES.OrgAdmin,
          })}
        >
          <Navbar xpChipSlot={<span />} notificationSlot={<span />} />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByRole("link", { name: "Dashboard" })).toHaveAttribute(
      "href",
      "/dashboard/org",
    );
    orgAdminView.unmount();

    render(
      <MemoryRouter initialEntries={["/"]}>
        <AppCoreProviders
          initialUser={makeStudent({
            firstName: "Admin",
            role: USER_ROLES.PlatformAdmin,
          })}
        >
          <Navbar xpChipSlot={<span />} notificationSlot={<span />} />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.getByRole("link", { name: "Admin" })).toHaveAttribute("href", "/admin");
  });

  it("renders StudentDashboardPage with live XP, event passes, and community applications in one request", async () => {
    const scoreSpy = vi.spyOn(gamificationApi, "getStudentScore");
    const myRegSpy = vi.spyOn(eventsApi, "myRegistrations");
    const myAppsSpy = vi.spyOn(organizationsApi, "myApplications");
    const notifSpy = vi.spyOn(notificationsApi, "getNotifications");
    const projSpy = vi.spyOn(projectsApi, "list");

    vi.spyOn(workspacesApi, "studentDashboard").mockResolvedValueOnce({
      score: {
        studentId: 10,
        totalPoints: 1250,
        level: 4,
        currentLevelStart: 900,
        nextLevelThreshold: 1500,
        pointsToNext: 250,
        progress: 0.58,
      },
      myRegistrations: {
        items: [
          {
            eventId: 1,
            eventTitle: "Auxilio Engineering & AI Bootcamp 2026",
            organizationId: 1,
            organizationName: "IEEE Helwan Student Branch",
            eventType: EVENT_TYPES.Workshop,
            deliveryMode: "Hybrid",
            location: "Helwan Main Hall",
            startDate: "2026-12-01T09:00:00Z",
            endDate: "2026-12-03T16:00:00Z",
            status: "Confirmed",
            registeredAt: "2026-11-20T10:00:00Z",
            sessionsAttendedCount: 1,
            totalSessionsCount: 3,
          },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
      },
      myApplications: [],
      notifications: [],
      unreadNotificationsCount: 0,
      recruitingProjects: {
        items: [],
        page: 1,
        pageSize: 4,
        totalCount: 0,
        totalPages: 1,
      },
    });

    render(
      <MemoryRouter initialEntries={["/dashboard/student"]}>
        <AppCoreProviders
          initialUser={makeStudent({
            studentId: 10,
            firstName: "Salma",
            lastName: "Khaled",
            universityName: "Cairo University",
            facultyName: "FCAI",
          })}
        >
          <StudentDashboardPage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText(/Welcome back, Salma/i)).toBeInTheDocument();
    expect(await screen.findByText(/Lvl 4 · 1250 XP/i)).toBeInTheDocument();
    expect(
      await screen.findByText(/Auxilio Engineering & AI Bootcamp 2026/i),
    ).toBeInTheDocument();
    expect(scoreSpy).not.toHaveBeenCalled();
    expect(myRegSpy).not.toHaveBeenCalled();
    expect(myAppsSpy).not.toHaveBeenCalled();
    expect(notifSpy).not.toHaveBeenCalled();
    expect(projSpy).not.toHaveBeenCalled();
  });

  it("renders OrgAdminDashboardPage with 4-stage recruitment funnel and hosted event QR links in one request", async () => {
    const orgsSpy = vi.spyOn(organizationsApi, "list");
    const appsSpy = vi.spyOn(organizationsApi, "applications");
    const eventsSpy = vi.spyOn(eventsApi, "list");

    vi.spyOn(workspacesApi, "orgDashboard").mockResolvedValue({
      managedOrganizations: [
        {
          organizationId: 1,
          name: "IEEE Helwan Student Branch",
          description: "Engineering & AI community",
          category: "Engineering",
          scope: ORGANIZATION_SCOPES.UniversityBranch,
          universityName: "Helwan University",
          city: "Helwan, Cairo",
          isRecruiting: true,
          currentSeasonName: "Fall 2026",
          logoUrl: null,
          memberCount: 120,
          unitCount: 9,
        },
      ],
      activeOrganizationId: 1,
      applications: [
        {
          applicationId: 1,
          organizationId: 1,
          studentId: 50,
          studentFirstName: "Tarek",
          studentLastName: "Nabil",
          studentEmail: "tarek@indie.dev",
          status: "Pending",
          stage: RECRUITMENT_STAGES.HrScreening,
          message: "Joining CS",
          appliedAt: "2026-11-24T10:00:00Z",
          reviewedAt: null,
        },
      ],
      hostedEvents: {
        items: [
          {
            eventId: 1,
            title: "Auxilio Bootcamp",
            description: "Hands-on engineering bootcamp",
            type: EVENT_TYPES.Workshop,
            location: "Main Hall",
            startDate: "2026-12-01T09:00:00Z",
            endDate: "2026-12-03T16:00:00Z",
            capacity: 120,
            registeredCount: 8,
            organizationId: 1,
            organizationName: "IEEE Helwan Student Branch",
            deliveryMode: "Hybrid",
            sessions: [
              {
                sessionId: 11,
                eventId: 1,
                sessionNo: 1,
                title: "Opening Keynote",
                startTime: "2026-12-01T09:00:00Z",
                endTime: "2026-12-01T12:00:00Z",
                room: "A1",
              },
            ],
          },
        ],
        totalCount: 1,
        totalPages: 1,
        page: 1,
        pageSize: 6,
      },
    });

    render(
      <MemoryRouter initialEntries={["/dashboard"]}>
        <AppCoreProviders
          initialUser={makeStudent({
            studentId: 2,
            firstName: "Nour",
            role: USER_ROLES.OrgAdmin,
          })}
        >
          <RoleDashboardRouterPage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(
      await screen.findByRole("heading", { name: "Org Leadership Command Center" }),
    ).toBeInTheDocument();
    expect(await screen.findByText(/2\. HR Screening/i)).toBeInTheDocument();
    expect(await screen.findByText("Auxilio Bootcamp")).toBeInTheDocument();
    expect(await screen.findByRole("link", { name: /Session #1 QR/i })).toHaveAttribute(
      "href",
      "/events/1/sessions/11/attendance",
    );
    expect(orgsSpy).not.toHaveBeenCalled();
    expect(appsSpy).not.toHaveBeenCalled();
    expect(eventsSpy).not.toHaveBeenCalled();
  });

  it("guards OrgAdminDashboardPage when viewed by a Student account", () => {
    render(
      <MemoryRouter initialEntries={["/dashboard/org"]}>
        <AppCoreProviders initialUser={makeStudent({ role: USER_ROLES.Student })}>
          <OrgAdminDashboardPage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(
      screen.getByText(/No organization leadership role assigned yet/i),
    ).toBeInTheDocument();
  });

  it("opens CommandPalette with Ctrl+K, searches, and navigates to target route", async () => {
    vi.spyOn(organizationsApi, "publicList").mockResolvedValue({
      items: [
        {
          organizationId: 7,
          name: "IEEE Helwan Student Branch",
          description: "Engineering & AI community",
          category: "Engineering",
          scope: ORGANIZATION_SCOPES.UniversityBranch,
          universityName: "Helwan University",
          city: "Helwan, Cairo",
          isRecruiting: true,
          currentSeasonName: "Fall 2026",
          logoUrl: null,
          memberCount: 120,
          unitCount: 9,
        },
      ],
      totalCount: 1,
      totalPages: 1,
      page: 1,
      pageSize: 5,
    });
    vi.spyOn(eventsApi, "publicList").mockResolvedValue({
      items: [],
      totalCount: 0,
      totalPages: 0,
      page: 1,
      pageSize: 5,
    });

    render(
      <MemoryRouter initialEntries={["/"]}>
        <AppCoreProviders initialUser={null}>
          <CommandPalette />
          <Routes>
            <Route path="/" element={<div>Landing</div>} />
            <Route path="/organizations/:id" element={<h1>IEEE Helwan Workspace</h1>} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    fireEvent.keyDown(window, { key: "k", ctrlKey: true });

    const input = await screen.findByPlaceholderText(/search activities, events, projects/i);
    fireEvent.change(input, { target: { value: "Helwan" } });

    const item = await screen.findByRole("button", { name: /IEEE Helwan Student Branch/i });
    fireEvent.click(item);

    expect(await screen.findByText("IEEE Helwan Workspace")).toBeInTheDocument();
  });

  it("renders all 4 Egyptian student activity tiers and inspector details in OrganizationHierarchyTab", () => {
    const hierarchy: OrganizationHierarchy = {
      organizationId: 7,
      organizationName: "IEEE Helwan Student Branch",
      scope: ORGANIZATION_SCOPES.UniversityBranch,
      units: [
        {
          organizationUnitId: 1,
          organizationId: 7,
          name: "Computer Society (CS)",
          code: "CS",
          description: "Software & Systems",
          unitType: ORGANIZATION_UNIT_TYPES.TechnicalChapter,
          isRecruiting: true,
          openSeats: 15,
          memberCount: 22,
        },
      ],
      highBoard: [
        {
          studentId: 1,
          firstName: "Nour",
          lastName: "Hassan",
          email: "nour@cu.edu.eg",
          role: "Chair",
          hierarchyTier: "HighBoard",
          organizationUnitId: null,
          unitName: null,
          unitCode: null,
          joinedAt: "2026-09-01T10:00:00Z",
        },
      ],
      committeeLeadership: [
        {
          studentId: 2,
          firstName: "Karim",
          lastName: "Adel",
          email: "karim@helwan.edu.eg",
          role: "Head",
          hierarchyTier: "CommitteeLeadership",
          organizationUnitId: 1,
          unitName: "Computer Society (CS)",
          unitCode: "CS",
          joinedAt: "2026-09-05T10:00:00Z",
        },
      ],
      organizingCrew: [
        {
          studentId: 3,
          firstName: "Mariam",
          lastName: "Samir",
          email: "mariam@helwan.edu.eg",
          role: "Member",
          hierarchyTier: "OrganizingCrew",
          organizationUnitId: 1,
          unitName: "Computer Society (CS)",
          unitCode: "CS",
          joinedAt: "2026-09-10T10:00:00Z",
        },
      ],
      participantsCount: 84,
    };

    render(
      <MemoryRouter>
        <OrganizationHierarchyTab hierarchy={hierarchy} isLoading={false} />
      </MemoryRouter>,
    );

    expect(screen.getByText(/Tier 1 · High Board/i)).toBeInTheDocument();
    expect(screen.getByText(/Tier 2 · Chapter Chairs & Committee Heads/i)).toBeInTheDocument();
    expect(screen.getByText(/Tier 3 · Instructors & Organizing Crew/i)).toBeInTheDocument();
    expect(screen.getByText(/Tier 4 · Event Participants & Bootcamp Trainees/i)).toBeInTheDocument();
    expect(screen.getByText("Nour Hassan")).toBeInTheDocument();
    expect(screen.getByText("Karim Adel")).toBeInTheDocument();
    expect(screen.getByText("Mariam Samir")).toBeInTheDocument();
    expect(screen.getByText("84")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Karim Adel/i }));
    expect(
      screen.getByText(/Review & score applicants targeting Computer Society/i),
    ).toBeInTheDocument();
  });

  it("renders RecruitmentPipelineTab Kanban stages and triggers stage advancement", async () => {
    const updateStageMock = vi.fn().mockResolvedValue({
      applicationId: 42,
      stage: RECRUITMENT_STAGES.HrScreening,
    });

    const apps: MembershipApplication[] = [
      {
        applicationId: 42,
        organizationId: 7,
        studentId: 50,
        studentFirstName: "Hana",
        studentLastName: "Mahmoud",
        studentEmail: "hana@helwan.edu.eg",
        organizationUnitId: 1,
        unitName: "Computer Society (CS)",
        unitCode: "CS",
        status: "Pending",
        stage: RECRUITMENT_STAGES.Applied,
        evaluationScore: null,
        interviewNotes: null,
        message: "Excited to join AI & Backend tracks.",
        appliedAt: "2026-10-15T12:00:00Z",
        reviewedAt: null,
      },
    ];

    render(
      <MemoryRouter>
        <RecruitmentPipelineTab
          organizationId={7}
          currentStudentId={1}
          canManage
          canReviewRecruitment
          isMember
          units={[
            {
              organizationUnitId: 1,
              organizationId: 7,
              name: "Computer Society (CS)",
              code: "CS",
              description: "Backend & AI",
              unitType: ORGANIZATION_UNIT_TYPES.TechnicalChapter,
              isRecruiting: true,
              openSeats: 10,
              memberCount: 5,
            },
          ]}
          applications={apps}
          isLoadingApps={false}
          appsError={null}
          isBusy={false}
          onApply={vi.fn()}
          onWithdraw={vi.fn()}
          onReview={vi.fn()}
          onUpdateStage={updateStageMock}
          onExportCsv={vi.fn()}
        />
      </MemoryRouter>,
    );

    const applicantLink = screen.getByRole("link", { name: "Hana Mahmoud" });
    expect(applicantLink).toHaveAttribute("href", "/students/50");

    expect(screen.getByText("1. Applied")).toBeInTheDocument();
    expect(screen.getByText("2. HR Screening")).toBeInTheDocument();
    expect(screen.getByText("3. Committee Interview")).toBeInTheDocument();
    expect(screen.getByText("4. Accepted")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Advance/i }));

    await waitFor(() => {
      expect(updateStageMock).toHaveBeenCalledWith(42, {
        stage: RECRUITMENT_STAGES.HrScreening,
      });
    });
  });

  it("renders Luma EventDigitalPassCard and exports valid RFC-5545 .ics calendar text", () => {
    const sampleEvent: EventSummary = {
      eventId: 12,
      title: "Auxilio Engineering & AI Bootcamp 2026",
      description: "Three-day hands-on engineering & AI bootcamp.",
      type: EVENT_TYPES.Workshop,
      location: "Helwan Faculty of Engineering Main Hall",
      startDate: "2026-12-01T09:00:00Z",
      endDate: "2026-12-03T16:00:00Z",
      capacity: 120,
      registeredCount: 84,
      organizationId: 7,
      organizationName: "IEEE Helwan Student Branch",
      sessions: [],
      deliveryMode: "Hybrid",
      meetingUrl: "https://meet.google.com/teamup-auxilio",
      isPublic: true,
    };

    const ics = buildEventIcsContent(sampleEvent);
    expect(ics).toContain("BEGIN:VCALENDAR");
    expect(ics).toContain("BEGIN:VEVENT");
    expect(ics).toContain("SUMMARY:Auxilio Engineering & AI Bootcamp 2026");

    render(
      <EventDigitalPassCard
        event={sampleEvent}
        studentName="Salma Khaled"
        studentId={10}
        status="Confirmed"
        sessionsAttendedCount={2}
      />,
    );

    expect(screen.getByText("#TU-12")).toBeInTheDocument();
    expect(screen.getByText("Salma Khaled")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Join Live Stream/i })).toHaveAttribute(
      "href",
      "https://meet.google.com/teamup-auxilio",
    );
    expect(screen.getByRole("button", { name: /Add to Calendar \(\.ics\)/i })).toBeInTheDocument();
  });

  it("renders StageProjectorOverlay and closes on Escape key", () => {
    const onClose = vi.fn();

    render(
      <StageProjectorOverlay
        open
        onClose={onClose}
        sessionTitle="Day 1 — Clean Backend Architecture"
        eventTitle="Auxilio Bootcamp"
        roomLabel="Main Hall A"
        codeData={{
          sessionId: 5,
          code: "AUXILIO-DAY1-QR",
          expiresAtUtc: new Date(Date.now() + 90_000).toISOString(),
        }}
        checkInDeepLink="http://localhost:5173/check-in?code=AUXILIO-DAY1-QR"
        secondsLeft={88}
        checkedInCount={42}
        confirmedCount={100}
        onRefreshCode={vi.fn()}
      />,
    );

    expect(screen.getByText("Day 1 — Clean Backend Architecture")).toBeInTheDocument();
    expect(screen.getByText("42 / 100")).toBeInTheDocument();

    fireEvent.keyDown(window, { key: "Escape" });
    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it("renders ApiHealthPill with live healthy response from GET /health", async () => {
    vi.spyOn(healthApi, "check").mockResolvedValueOnce({
      status: "Healthy",
      database: "Connected",
    });

    render(
      <MemoryRouter>
        <AppCoreProviders initialUser={null}>
          <ApiHealthPill />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText("API Live")).toBeInTheDocument();
  });

  it("does not mount API Live telemetry indicator in AppShell or LandingFooterSection", () => {
    render(
      <MemoryRouter initialEntries={["/projects"]}>
        <AppCoreProviders initialUser={null}>
          <AppShell>
            <div>Main workspace</div>
          </AppShell>
          <LandingFooterSection footerVisible footerLinksRevealed />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(screen.queryByText(/API Live/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/API status:/i)).not.toBeInTheDocument();
  });

  it("renders icon-only Quick Search button opening CommandPalette and exposes Org Requests in PlatformAdmin mobile drawer", async () => {
    render(
      <MemoryRouter initialEntries={["/"]}>
        <AppCoreProviders
          initialUser={makeStudent({
            firstName: "Amina",
            role: USER_ROLES.PlatformAdmin,
          })}
        >
          <Navbar xpChipSlot={<span />} notificationSlot={<span />} />
          <CommandPalette />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    const searchTrigger = screen.getByRole("button", { name: "Quick search command palette" });
    expect(searchTrigger).toBeInTheDocument();
    expect(searchTrigger.textContent?.trim()).toBe("");
    expect(screen.queryByText("Ctrl K")).not.toBeInTheDocument();

    fireEvent.click(searchTrigger);
    expect(
      await screen.findByRole("dialog", { name: "Global command palette" }),
    ).toBeInTheDocument();
    fireEvent.keyDown(screen.getByRole("button", { name: "Close command palette" }), {
      key: "Escape",
    });

    fireEvent.click(screen.getByRole("button", { name: "Toggle navigation menu" }));
    const drawer = document.getElementById("mobile-navigation-drawer");
    expect(drawer).not.toBeNull();
    expect(within(drawer!).getByRole("button", { name: /Quick Search/i })).toBeInTheDocument();
    expect(within(drawer!).getByRole("link", { name: "Org Requests" })).toHaveAttribute(
      "href",
      "/admin/organization-requests",
    );
  });
});
