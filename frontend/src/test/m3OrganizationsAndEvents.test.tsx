/**
 * @date 2026-11-24
 * @purpose Unit and React Testing Library integration tests for M3 Organizations, Events, Waitlist, Live QR Projector, and Self Check-In.
 */
import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import {
  ApiClientError,
  attendanceApi,
  eventsApi,
  organizationsApi,
  sessionsApi,
  USER_ROLES,
  type AuthStudentSummary,
  type EventSummary,
  type OrganizationDetail,
} from "@/api";
import { AppCoreProviders } from "@/app/providers";
import { QrProjectorCard } from "@/features/events/components/QrProjectorCard";
import { EventDetailPage } from "@/features/events/pages/EventDetailPage";
import { SessionAttendancePage } from "@/features/events/pages/SessionAttendancePage";
import { StudentCheckInPage } from "@/features/events/pages/StudentCheckInPage";
import {
  formatAttendanceRecorder,
  fromLocalDateTimeInput,
  nextCountdownTick,
  secondsUntilExpiry,
  toLocalDateTimeInput,
} from "@/features/events/utils/attendance";
import { OrganizationDetailPage } from "@/features/organizations/pages/OrganizationDetailPage";
import { canManageOrganization } from "@/features/organizations/utils/authorization";

function makeStudent(overrides: Partial<AuthStudentSummary> = {}): AuthStudentSummary {
  return {
    studentId: 10,
    firstName: "Salma",
    lastName: "Khaled",
    email: "salma@cu.edu.eg",
    academicYear: 4,
    role: USER_ROLES.Student,
    facultyId: 1,
    facultyName: "FCAI",
    universityName: "Cairo University",
    photoUrl: null,
    ...overrides,
  };
}

const SAMPLE_ORG: OrganizationDetail = {
  organizationId: 1,
  name: "IEEE Cairo University SB",
  description: "Engineering workshops and robotics builds across campus.",
  category: "Engineering",
  logoUrl: null,
  members: [
    {
      studentId: 2,
      firstName: "Nour",
      lastName: "Hassan",
      email: "nour@cu.edu.eg",
      role: "Admin",
      joinedAt: "2026-10-01T10:00:00Z",
    },
    {
      studentId: 10,
      firstName: "Salma",
      lastName: "Khaled",
      email: "salma@cu.edu.eg",
      role: "Member",
      joinedAt: "2026-10-05T10:00:00Z",
    },
  ],
  events: [],
};

const SAMPLE_EVENT: EventSummary = {
  eventId: 5,
  title: "Distributed Systems Workshop",
  description: "Hands-on reliability engineering workshop for undergraduates.",
  type: "Workshop",
  location: "Cairo University Hall B",
  startDate: "2026-12-01T14:00:00Z",
  endDate: "2026-12-01T18:00:00Z",
  capacity: 1,
  organizationId: 1,
  organizationName: "IEEE Cairo University SB",
  registeredCount: 1,
  sessions: [
    {
      sessionId: 12,
      eventId: 5,
      sessionNo: 1,
      title: "Session 1: Consensus",
      startTime: "2026-12-01T14:00:00Z",
      endTime: "2026-12-01T16:00:00Z",
      room: "Hall B-204",
    },
  ],
};

describe("M3 — Organizations, Events & Live Attendance", () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.restoreAllMocks();
  });

  it("evaluates scoped OrgAdmin permissions and pure attendance utilities", () => {
    const members = SAMPLE_ORG.members;
    expect(canManageOrganization(1, 99, true, () => false, [])).toBe(true);
    expect(canManageOrganization(1, 2, false, (id) => id === 1, [])).toBe(true);
    expect(canManageOrganization(1, 2, false, () => false, members)).toBe(true);
    // Scoped BOLA check: OrgAdmin of org #99 cannot manage org #1
    expect(canManageOrganization(1, 88, false, (id) => id === 99, members)).toBe(false);
    // Regular member cannot manage org #1
    expect(canManageOrganization(1, 10, false, () => false, members)).toBe(false);

    const nowMs = Date.parse("2026-11-24T12:00:00.000Z");
    expect(secondsUntilExpiry("2026-11-24T12:00:58.000Z", nowMs)).toBe(58);
    expect(secondsUntilExpiry("2026-11-24T11:59:50.000Z", nowMs)).toBe(0);
    expect(nextCountdownTick(2)).toEqual({ seconds: 1, shouldRefresh: false });
    expect(nextCountdownTick(1)).toEqual({ seconds: 0, shouldRefresh: true });

    const iso = "2026-12-01T14:30:00.000Z";
    const localVal = toLocalDateTimeInput(iso);
    expect(Date.parse(fromLocalDateTimeInput(localVal))).toBe(Date.parse(iso));

    expect(formatAttendanceRecorder({ studentId: 10, recordedByStudentId: 10 })).toBe("Self check-in");
    expect(formatAttendanceRecorder({ studentId: 10, recordedByStudentId: null })).toBe("Self check-in");
    expect(formatAttendanceRecorder({ studentId: 10, recordedByStudentId: 2 })).toBe("Organizer #2");
  });

  it("hides Apply form & admin applications query for existing org member and provides Leave action", async () => {
    vi.spyOn(organizationsApi, "detail").mockResolvedValue(SAMPLE_ORG);
    const appsSpy = vi.spyOn(organizationsApi, "applications").mockResolvedValue([]);
    const removeMemberSpy = vi.spyOn(organizationsApi, "removeMember").mockResolvedValue();

    render(
      <MemoryRouter initialEntries={["/organizations/1"]}>
        <AppCoreProviders initialUser={makeStudent({ studentId: 10 })}>
          <Routes>
            <Route path="/organizations/:id" element={<OrganizationDetailPage />} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText("IEEE Cairo University SB")).toBeInTheDocument();
    expect(screen.getByText("Active organization member")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /Apply to join/i })).not.toBeInTheDocument();
    expect(appsSpy).not.toHaveBeenCalled();

    fireEvent.click(screen.getByRole("tab", { name: /Members/i }));
    const leaveBtn = await screen.findByRole("button", { name: /Leave organization/i });
    fireEvent.click(leaveBtn);

    const dialog = await screen.findByRole("dialog");
    fireEvent.click(within(dialog).getByRole("button", { name: /Leave organization/i }));

    await waitFor(() => {
      expect(removeMemberSpy).toHaveBeenCalledWith(1, 10);
    });
  });

  it("handles full-capacity Waitlist registration and 409 sync + Cancel without calling BOLA-restricted GET /registrations for students", async () => {
    vi.spyOn(eventsApi, "detail").mockResolvedValue(SAMPLE_EVENT);
    vi.spyOn(organizationsApi, "detail").mockResolvedValue(SAMPLE_ORG);
    vi.spyOn(organizationsApi, "list").mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      totalPages: 1,
    });
    vi.spyOn(sessionsApi, "listForEvent").mockResolvedValue(SAMPLE_EVENT.sessions);
    const regListSpy = vi.spyOn(eventsApi, "registrations").mockResolvedValue([]);
    const registerSpy = vi.spyOn(eventsApi, "register").mockRejectedValueOnce(
      new ApiClientError("You already have an active registration for this event.", 409),
    );
    const cancelSpy = vi.spyOn(eventsApi, "cancelRegistration").mockResolvedValue();

    render(
      <MemoryRouter initialEntries={["/events/5"]}>
        <AppCoreProviders initialUser={makeStudent({ studentId: 10 })}>
          <Routes>
            <Route path="/events/:id" element={<EventDetailPage />} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText("Distributed Systems Workshop")).toBeInTheDocument();
    expect(regListSpy).not.toHaveBeenCalled();

    fireEvent.click(screen.getByRole("button", { name: /Join waitlist/i }));
    await waitFor(() => expect(registerSpy).toHaveBeenCalledWith(5));

    const cancelBtn = await screen.findByRole("button", { name: /Cancel registration/i });
    fireEvent.click(cancelBtn);

    const dialog = await screen.findByRole("dialog");
    fireEvent.click(within(dialog).getByRole("button", { name: /Cancel registration/i }));

    await waitFor(() => {
      expect(cancelSpy).toHaveBeenCalledWith(5, undefined);
    });
  });

  it("displays full 25-char {step}.{hex} token on QrProjectorCard when generated manually while paused", async () => {
    const token = "29927200.a1b2c3d4e5f60718";
    const onGen = vi.fn().mockResolvedValue({
      sessionId: 12,
      code: token,
      expiresAtUtc: new Date(Date.now() + 55_000).toISOString(),
    });

    render(
      <QrProjectorCard
        sessionId={12}
        isGeneratingCode={false}
        isLoadingReport={false}
        onGenerateCode={onGen}
        onOpenReport={async () => {}}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: /Generate code now/i }));
    expect(await screen.findByTestId("projector-token-value")).toHaveTextContent(token);
    expect(onGen).toHaveBeenCalledTimes(1);

    fireEvent.click(screen.getByRole("button", { name: /Start projector/i }));
    await waitFor(() => expect(onGen).toHaveBeenCalledTimes(2));
  });

  it("renders Self check-in label for matching RecordedByStudentId and sets allow-modals on HTML report iframe", async () => {
    vi.spyOn(eventsApi, "detail").mockResolvedValue(SAMPLE_EVENT);
    vi.spyOn(organizationsApi, "detail").mockResolvedValue(SAMPLE_ORG);
    vi.spyOn(sessionsApi, "listForEvent").mockResolvedValue(SAMPLE_EVENT.sessions);
    vi.spyOn(eventsApi, "registrations").mockResolvedValue([]);
    vi.spyOn(attendanceApi, "list").mockResolvedValue([
      {
        sessionId: 12,
        studentId: 10,
        firstName: "Salma",
        lastName: "Khaled",
        checkInTime: "2026-12-01T14:05:00Z",
        verificationMethod: "QRCode",
        recordedByStudentId: 10,
      },
    ]);
    vi.spyOn(attendanceApi, "getReportHtml").mockResolvedValue("<html><body><h1>Attendance Report</h1></body></html>");

    render(
      <MemoryRouter initialEntries={["/events/5/sessions/12/attendance"]}>
        <AppCoreProviders initialUser={makeStudent({ studentId: 2, role: USER_ROLES.OrgAdmin })}>
          <Routes>
            <Route
              path="/events/:id/sessions/:sessionId/attendance"
              element={<SessionAttendancePage />}
            />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    expect(await screen.findByText("Salma Khaled")).toBeInTheDocument();
    expect(screen.getByText("Self check-in")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Attendance report/i }));
    const iframe = await screen.findByTitle("Attendance report");
    expect(iframe).toHaveAttribute("sandbox", "allow-same-origin allow-modals");
  });

  it("submits full {step}.{hex} QR deep link token cleanly on StudentCheckInPage and handles 400/409 feedback", async () => {
    const token = "29927200.a1b2c3d4e5f60718";
    const checkInSpy = vi
      .spyOn(attendanceApi, "selfCheckIn")
      .mockResolvedValueOnce({
        sessionId: 12,
        studentId: 10,
        firstName: "Salma",
        lastName: "Khaled",
        checkInTime: "2026-12-01T14:05:00Z",
        verificationMethod: "QRCode",
        recordedByStudentId: 10,
      })
      .mockRejectedValueOnce(new ApiClientError("Invalid or expired session check-in code.", 400));

    render(
      <MemoryRouter initialEntries={[`/check-in?sessionId=12&code=${encodeURIComponent(token)}`]}>
        <AppCoreProviders initialUser={makeStudent({ studentId: 10 })}>
          <Routes>
            <Route path="/check-in" element={<StudentCheckInPage />} />
          </Routes>
        </AppCoreProviders>
      </MemoryRouter>,
    );

    const codeInput = screen.getByLabelText(/Verification token/i) as HTMLInputElement;
    expect(codeInput.value).toBe(token);

    const submitBtn = screen.getByRole("button", { name: /Verify attendance/i });
    expect(submitBtn).toBeEnabled();
    fireEvent.click(submitBtn);

    expect(await screen.findByText(/Attendance verified!/i)).toBeInTheDocument();
    expect(checkInSpy).toHaveBeenCalledWith(12, token);

    fireEvent.click(submitBtn);
    expect(await screen.findByText(/Invalid or expired verification code/i)).toBeInTheDocument();
  });
});
