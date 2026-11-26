/**
 * @date 2026-11-25
 * @purpose Vitest coverage for the Explore discovery feed (round 4): rails render from the live APIs,
 * cards link to detail pages, and empty data shows friendly empty states instead of broken sections.
 */
import { QueryClient } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { eventsApi, organizationsApi, projectsApi, USER_ROLES, type AuthStudentSummary } from "@/api";
import { AppCoreProviders } from "@/app/providers";
import { ExplorePage } from "@/features/explore/ExplorePage";

const STUDENT: AuthStudentSummary = {
  studentId: 42,
  firstName: "Salma",
  lastName: "Nour",
  email: "salma@teamup.edu",
  academicYear: 2,
  role: USER_ROLES.Student,
  facultyId: 1,
  facultyName: "Computer Science",
  universityName: "Cairo University",
};

function paged<T>(items: T[]) {
  return { items, page: 1, pageSize: 6, totalCount: items.length, totalPages: 1 };
}

function renderExplore() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <MemoryRouter initialEntries={["/explore"]}>
      <AppCoreProviders initialUser={STUDENT} queryClient={queryClient}>
        <ExplorePage />
      </AppCoreProviders>
    </MemoryRouter>,
  );
}

describe("ExplorePage", () => {
  it("renders recruiting organizations, events and projects rails with detail links", async () => {
    vi.spyOn(organizationsApi, "list").mockResolvedValue(
      paged([
        {
          organizationId: 7,
          name: "Robotics Crew",
          description: "Building robots",
          category: "Robotics",
          logoUrl: null,
          memberCount: 12,
          coverImageUrl: null,
        },
      ] as never),
    );
    vi.spyOn(eventsApi, "list").mockResolvedValue(
      paged([
        {
          eventId: 9,
          title: "Intro to ROS",
          description: "Workshop",
          type: "Workshop",
          location: "Hall B",
          startDate: "2026-12-01T10:00:00Z",
          endDate: "2026-12-01T12:00:00Z",
          capacity: 40,
          organizationId: 7,
          organizationName: "Robotics Crew",
          registeredCount: 5,
          sessions: [],
        },
      ] as never),
    );
    vi.spyOn(projectsApi, "list").mockResolvedValue(
      paged([
        {
          projectId: 3,
          title: "Line Follower Bot",
          description: "PID-controlled robot",
          status: "Open",
          maxTeamSize: 4,
          teamSize: 1,
          creatorId: 42,
          creatorName: "Salma Nour",
          createdAt: "2026-11-20T00:00:00Z",
        },
      ] as never),
    );

    renderExplore();

    // The org name appears on its own rail card and on the event card's organization line.
    expect((await screen.findAllByText("Robotics Crew")).length).toBeGreaterThan(0);
    expect(await screen.findByText("Intro to ROS")).toBeInTheDocument();
    expect(await screen.findByText("Line Follower Bot")).toBeInTheDocument();

    expect(
      screen.getAllByRole("link", { name: /Robotics Crew/i }).some((link) => link.getAttribute("href") === "/organizations/7"),
    ).toBe(true);
    expect(screen.getByRole("link", { name: /Intro to ROS/i })).toHaveAttribute("href", "/events/9");
    expect(screen.getByRole("link", { name: /Line Follower Bot/i })).toHaveAttribute(
      "href",
      "/projects/3",
    );
  });

  it("shows friendly empty states when every rail has no data", async () => {
    vi.spyOn(organizationsApi, "list").mockResolvedValue(paged([] as never[]));
    vi.spyOn(eventsApi, "list").mockResolvedValue(paged([] as never[]));
    vi.spyOn(projectsApi, "list").mockResolvedValue(paged([] as never[]));

    renderExplore();

    expect(await screen.findByText(/No open recruitment right now/i)).toBeInTheDocument();
    expect(await screen.findByText(/No events on the calendar yet/i)).toBeInTheDocument();
    expect(await screen.findByText(/No open projects yet/i)).toBeInTheDocument();
  });

  it("renders an error alert with retry when a discovery rail fails instead of a fake empty state", async () => {
    vi.spyOn(organizationsApi, "list").mockRejectedValue(new Error("Directory temporarily unavailable"));
    vi.spyOn(eventsApi, "list").mockResolvedValue(paged([] as never[]));
    vi.spyOn(projectsApi, "list").mockResolvedValue(paged([] as never[]));

    renderExplore();

    expect(await screen.findByRole("alert")).toHaveTextContent(/Directory temporarily unavailable/i);
    expect(screen.queryByText(/No open recruitment right now/i)).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: /retry/i })).toBeInTheDocument();
  });
});
