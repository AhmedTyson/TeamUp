/**
 * @date 2026-11-22
 * @purpose Smoke tests verifying App renders prototype morphing Navbar, TeamUp hero, service cards, and Handshake Rosetta-locked ProposedHeroDemo (/demo/hero).
 */
import { fireEvent, render, screen, within } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { App } from "@/App";
import { AppCoreProviders } from "@/app/providers";
import { ProposedHeroDemo } from "@/components/home/ProposedHeroDemo";

describe("App shell and home route", () => {
  it("renders TeamUp prototype hero, morphing navbar dropdowns, and service cards", () => {
    render(<App />);

    expect(screen.getByText(/let's find/i)).toBeInTheDocument();
    expect(screen.getByText(/your next team/i)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /join your campus/i })).toHaveAttribute(
      "href",
      "/auth/register",
    );
    expect(screen.getByText("Cairo · AI Squad")).toBeInTheDocument();
    expect(screen.getByText("Capital · Clubs Lead")).toBeInTheDocument();
    expect(screen.getByText("Ain Shams · +100 XP")).toBeInTheDocument();

    const primaryNav = within(screen.getByRole("navigation", { name: /primary navigation/i }));
    expect(primaryNav.getByRole("button", { name: "Team Formation" })).toBeInTheDocument();
    expect(primaryNav.getByRole("button", { name: "Profiles & Scoring" })).toBeInTheDocument();
    expect(primaryNav.getByRole("button", { name: "Student Activities" })).toBeInTheDocument();
    expect(primaryNav.getByRole("button", { name: "Events & Workshops" })).toBeInTheDocument();
    expect(primaryNav.getByRole("button", { name: "Gamification" })).toBeInTheDocument();

    expect(screen.getByRole("link", { name: "Sign In" })).toBeInTheDocument();

    expect(
      screen.getByRole("heading", { level: 3, name: "Team Formation & Projects" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { level: 3, name: "Student Profiles & Scoring" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { level: 3, name: "Student Activities Hub" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { level: 3, name: "Events & Workshops" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { level: 3, name: "Attendance Verification" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { level: 3, name: "Gamification & Leaderboards" }),
    ).toBeInTheDocument();
  });

  it("renders Handshake Rosetta-locked ProposedHeroDemo (/demo/hero) and switches between Editorial Photo Orbit and Student Portrait Stage", () => {
    render(
      <MemoryRouter initialEntries={["/demo/hero"]}>
        <AppCoreProviders initialUser={null}>
          <ProposedHeroDemo />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    const orbitHeading = screen.getByRole("heading", { level: 1 });
    expect(orbitHeading).toHaveTextContent(/let's find/i);
    expect(orbitHeading).toHaveTextContent(/your next team/i);
    expect(
      screen.getByRole("searchbox", { name: /search teammates, clubs, or workshops/i }),
    ).toBeInTheDocument();
    expect(screen.getAllByAltText(/Cairo University official logo/i).length).toBeGreaterThan(0);
    expect(screen.getAllByAltText(/Capital University official logo/i).length).toBeGreaterThan(0);
    expect(screen.getByText("+6")).toBeInTheDocument();
    expect(
      screen.getAllByAltText(/university student smiling with notebooks and laptop/i).length,
    ).toBeGreaterThan(0);
    expect(screen.getAllByAltText(/hands-on engineering workshop/i).length).toBeGreaterThan(0);

    fireEvent.click(screen.getByRole("tab", { name: "Student Portrait Stage" }));
    const stageHeading = screen.getByRole("heading", { level: 1 });
    expect(stageHeading).toHaveTextContent(/let's find/i);
    expect(stageHeading).toHaveTextContent(/your next team/i);
    expect(screen.getByRole("link", { name: /join your campus/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /explore squads/i })).toBeInTheDocument();
    expect(screen.getByText("Cairo · AI Squad")).toBeInTheDocument();
    expect(screen.getByText("Capital · Clubs Lead")).toBeInTheDocument();
    expect(screen.getByText("Ain Shams · +100 XP")).toBeInTheDocument();
  });
});
