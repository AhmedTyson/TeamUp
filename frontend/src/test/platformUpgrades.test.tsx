/**
 * @date 2026-11-24
 * @purpose Vitest + React Testing Library suite covering the 20-Item Master Upgrade Plan (flexible onboarding, Arabic RTL i18n, Ctrl+K CommandPalette, 4-tier Hierarchy org-chart, Recruitment Kanban, Luma Digital Pass + .ics, Stage Projector, and ApiHealthPill).
 */
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import {
  AFFILIATION_TYPES,
  authApi,
  eventsApi,
  EVENT_TYPES,
  healthApi,
  organizationsApi,
  ORGANIZATION_SCOPES,
  ORGANIZATION_UNIT_TYPES,
  RECRUITMENT_STAGES,
  universitiesApi,
  USER_ROLES,
  type AuthStudentSummary,
  type EventSummary,
  type MembershipApplication,
  type OrganizationHierarchy,
} from "@/api";
import { AppCoreProviders } from "@/app/providers";
import { ApiHealthPill } from "@/components/layout/ApiHealthPill";
import { CommandPalette } from "@/components/layout/CommandPalette";
import { RegisterPage } from "@/features/auth/pages/RegisterPage";
import { EventDigitalPassCard } from "@/features/events/components/EventDigitalPassCard";
import { StageProjectorOverlay } from "@/features/events/components/StageProjectorOverlay";
import { OrganizationHierarchyTab } from "@/features/organizations/components/OrganizationHierarchyTab";
import { RecruitmentPipelineTab } from "@/features/organizations/components/RecruitmentPipelineTab";
import { buildEventIcsContent } from "@/lib/calendarExport";
import { I18nProvider, useI18n } from "@/lib/i18n";

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

function I18nProbe() {
  const { locale, dir, toggleLocale, t } = useI18n();
  return (
    <div>
      <span data-testid="locale-info">{`${locale}:${dir}`}</span>
      <span>{t("orgs.title")}</span>
      <button type="button" onClick={toggleLocale}>
        Switch Language
      </button>
    </div>
  );
}

describe("20-Item Plan Upgrades Suite", () => {
  afterEach(() => {
    document.documentElement.lang = "en";
    document.documentElement.dir = "ltr";
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

  it("toggles bilingual AR/EN locale and synchronizes <html dir='rtl'> and lang='ar'", () => {
    render(
      <I18nProvider>
        <I18nProbe />
      </I18nProvider>,
    );

    expect(screen.getByTestId("locale-info")).toHaveTextContent("en:ltr");
    expect(document.documentElement.dir).toBe("ltr");

    fireEvent.click(screen.getByRole("button", { name: "Switch Language" }));

    expect(screen.getByTestId("locale-info")).toHaveTextContent("ar:rtl");
    expect(document.documentElement.dir).toBe("rtl");
    expect(document.documentElement.lang).toBe("ar");
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
      />,
    );

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
      <I18nProvider>
        <EventDigitalPassCard
          event={sampleEvent}
          studentName="Salma Khaled"
          studentId={10}
          status="Confirmed"
          sessionsAttendedCount={2}
        />
      </I18nProvider>,
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
});
