/**
 * @date 2026-11-22
 * @purpose Smoke tests verifying App renders prototype morphing Navbar, TeamUp hero, service cards, and Handshake Rosetta-locked ProposedHeroDemo (/demo/hero).
 */
import { act, fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import { skillsApi, studentsApi, USER_ROLES, type AuthStudentSummary } from "@/api";
import { App } from "@/App";
import { AppCoreProviders } from "@/app/providers";
import { ProposedHeroDemo } from "@/components/home/ProposedHeroDemo";
import { Navbar } from "@/components/layout/Navbar";
import { SelfProfileStudioPage } from "@/features/profile/SelfProfileStudioPage";

describe("App shell and home route", () => {
  afterEach(() => {
    delete document.documentElement.dataset.motion;
    window.localStorage.clear();
    window.sessionStorage.clear();
    vi.restoreAllMocks();
  });

  it("renders TeamUp prototype hero, morphing navbar dropdowns, and service cards", () => {
    render(<App />);

    expect(screen.getByText(/let's find/i)).toBeInTheDocument();
    expect(screen.getByText(/your next team/i)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /join your campus/i })).toHaveAttribute(
      "href",
      "/auth/register",
    );
    expect(screen.getByText("Omar · ENG_CAPU")).toBeInTheDocument();
    expect(screen.getByText("Salma · FCAI")).toBeInTheDocument();
    expect(screen.getByText("Youssef · 2310 XP")).toBeInTheDocument();

    const salmaPortrait = screen.getByAltText(/Salma Khaled/i);
    expect(salmaPortrait).toHaveAttribute("src", "/images/people/salma-khaled.jpg");
    const omarPortrait = screen.getByAltText(/Omar Mostafa/i);
    expect(omarPortrait).toHaveAttribute("src", "/images/people/omar-mostafa.jpg");
    const youssefPortrait = screen.getByAltText(/Youssef Adel/i);
    expect(youssefPortrait).toHaveAttribute("src", "/images/people/youssef-adel.jpg");

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

    const footerSection = document.getElementById("footer-section");
    expect(footerSection).toBeInTheDocument();
    expect(footerSection?.querySelector(".footer-ambient-glow")).toBeInTheDocument();
    expect(footerSection?.querySelector(".footer-scroll-stage")).toBeInTheDocument();
    expect(footerSection?.querySelector(".footer-horizon-line")).toBeInTheDocument();
    expect(document.getElementById("footer-brand-burst")).toBeInTheDocument();

    // Service cards should stagger on their wrapper, keeping 0ms hover delay on the card itself
    const serviceCards = Array.from(document.querySelectorAll<HTMLElement>(".service-card"));
    expect(serviceCards).toHaveLength(6);
    serviceCards.forEach((card) => {
      expect(card.style.transitionDelay).toBe("");
      expect(card.parentElement).toHaveClass("reveal-stagger-item");
    });

    // Partner universities & editorial campus gallery use scroll reveal classes
    expect(document.getElementById("partner-universities")).toHaveClass("section-reveal-up");
    expect(document.querySelectorAll(".showcase-text-reveal-rtl")).toHaveLength(2);
    expect(document.querySelectorAll(".showcase-text-reveal-ltr")).toHaveLength(2);
  });

  it("renders animations naturally without a motion toggle button or reduced-motion banner even when OS reduced motion matches", () => {
    const originalMatchMedia = window.matchMedia;
    Object.defineProperty(window, "matchMedia", {
      configurable: true,
      writable: true,
      value: vi.fn().mockImplementation((query: string) => ({
        matches: query.includes("prefers-reduced-motion"),
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      })),
    });

    try {
      render(<App />);

      expect(screen.queryByRole("button", { name: "Enable animations" })).not.toBeInTheDocument();
      expect(screen.queryByRole("button", { name: /Animations:/i })).not.toBeInTheDocument();
      expect(document.getElementById("footer-brand-burst")).toBeInTheDocument();
      expect(document.querySelector(".hero-aurora-layer")).toBeInTheDocument();
      expect(document.querySelectorAll(".hero-portrait-float")).toHaveLength(3);
    } finally {
      Object.defineProperty(window, "matchMedia", {
        configurable: true,
        writable: true,
        value: originalMatchMedia,
      });
    }
  });

  it("toggles homepage footer burst & continuous loop when IntersectionObserver enters and leaves viewport", () => {
    const callbacks: IntersectionObserverCallback[] = [];
    const OriginalIO = window.IntersectionObserver;

    class MockIntersectionObserver implements IntersectionObserver {
      readonly root = null;
      readonly rootMargin = "0px";
      readonly scrollMargin = "0px";
      readonly thresholds = [0.08, 0.25];
      constructor(cb: IntersectionObserverCallback) {
        callbacks.push(cb);
      }
      observe() {}
      unobserve() {}
      disconnect() {}
      takeRecords(): IntersectionObserverEntry[] {
        return [];
      }
    }

    vi.stubGlobal("IntersectionObserver", MockIntersectionObserver);
    try {
      render(<App />);
      const footerSection = document.getElementById("footer-section")!;
      const footerBurst = document.getElementById("footer-brand-burst")!;

      expect(footerBurst).not.toHaveClass("is-footer-looping");

      const footerObserverCb = callbacks[callbacks.length - 1];
      act(() => {
        footerObserverCb(
          [{ target: footerSection, isIntersecting: true } as unknown as IntersectionObserverEntry],
          {} as IntersectionObserver,
        );
      });
      expect(footerSection).toHaveClass("is-footer-looping");
      expect(footerBurst).toHaveClass("is-burst-visible", "is-footer-looping");

      act(() => {
        footerObserverCb(
          [{ target: footerSection, isIntersecting: false } as unknown as IntersectionObserverEntry],
          {} as IntersectionObserver,
        );
      });
      expect(footerSection).not.toHaveClass("is-footer-looping");
      expect(footerBurst).not.toHaveClass("is-footer-looping");
    } finally {
      vi.stubGlobal("IntersectionObserver", OriginalIO);
    }
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
    expect(screen.getByText("+7")).toBeInTheDocument();
    expect(screen.getAllByAltText(/Salma Khaled/i).length).toBeGreaterThan(0);
    expect(screen.getAllByAltText(/Omar Mostafa/i).length).toBeGreaterThan(0);
    expect(screen.getAllByAltText(/hands-on engineering workshop/i).length).toBeGreaterThan(0);

    fireEvent.click(screen.getByRole("tab", { name: "Student Portrait Stage" }));
    const stageHeading = screen.getByRole("heading", { level: 1 });
    expect(stageHeading).toHaveTextContent(/let's find/i);
    expect(stageHeading).toHaveTextContent(/your next team/i);
    expect(screen.getByRole("link", { name: /join your campus/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /explore squads/i })).toBeInTheDocument();
    expect(screen.getByText("Omar · ENG_CAPU")).toBeInTheDocument();
    expect(screen.getByText("Salma · FCAI")).toBeInTheDocument();
    expect(screen.getByText("Youssef · 2310 XP")).toBeInTheDocument();

    const salmaPortrait = screen.getByAltText(/Salma Khaled/i);
    expect(salmaPortrait).toHaveAttribute("src", "/images/people/salma-khaled.jpg");
    const omarPortrait = screen.getByAltText(/Omar Mostafa/i);
    expect(omarPortrait).toHaveAttribute("src", "/images/people/omar-mostafa.jpg");
    const youssefPortrait = screen.getByAltText(/Youssef Adel/i);
    expect(youssefPortrait).toHaveAttribute("src", "/images/people/youssef-adel.jpg");
  });

  it("uploads a validated profile portrait from /profile/me via studentsApi.uploadProfilePhoto", async () => {
    const user: AuthStudentSummary = {
      studentId: 10,
      firstName: "Nour",
      lastName: "Hassan",
      email: "nour@cu.edu.eg",
      academicYear: 3,
      role: USER_ROLES.Student,
      facultyId: 1,
      facultyName: "Computer Science",
      universityName: "Cairo University",
      photoUrl: "/images/people/salma-khaled.jpg",
    };

    vi.spyOn(studentsApi, "getMe").mockResolvedValue({
      ...user,
      showEmailInDirectory: false,
      interests: [],
      profile: {
        bio: null,
        photoUrl: "/images/people/salma-khaled.jpg",
        gitHubUrl: null,
        linkedInUrl: null,
      },
    });
    vi.spyOn(skillsApi, "list").mockResolvedValue([]);
    vi.spyOn(skillsApi, "getStudentSkills").mockResolvedValue([]);
    const uploadSpy = vi.spyOn(studentsApi, "uploadProfilePhoto").mockResolvedValueOnce({
      bio: null,
      photoUrl: "/images/people/student-10-uploaded.png",
      gitHubUrl: null,
      linkedInUrl: null,
    });

    render(
      <MemoryRouter initialEntries={["/profile/me"]}>
        <AppCoreProviders initialUser={user}>
          <SelfProfileStudioPage />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    const fileInput = (await screen.findByLabelText(/upload profile photo/i)) as HTMLInputElement;
    const validFile = new File(["png-bytes"], "portrait.png", { type: "image/png" });
    fireEvent.change(fileInput, { target: { files: [validFile] } });

    await waitFor(() => {
      expect(uploadSpy).toHaveBeenCalledWith(validFile);
    });
  });

  it("keeps mega-panel open on pointer hover followed by click and toggles closed only on explicit second click", () => {
    render(
      <MemoryRouter initialEntries={["/"]}>
        <AppCoreProviders initialUser={null}>
          <Navbar />
        </AppCoreProviders>
      </MemoryRouter>,
    );

    const primaryNav = screen.getByRole("navigation", { name: /primary navigation/i });
    const teamTrigger = within(primaryNav).getByRole("button", { name: "Team Formation" });
    const eventsTrigger = within(primaryNav).getByRole("button", { name: "Events & Workshops" });

    fireEvent.click(teamTrigger);
    expect(within(primaryNav).getByRole("menu")).toHaveTextContent("Create a Project");

    // Moving pointer onto sibling item auto-opens it while pinned, then clicking it must NOT immediately collapse it
    fireEvent.mouseEnter(eventsTrigger);
    fireEvent.click(eventsTrigger);

    const eventsMenu = within(primaryNav).getByRole("menu");
    expect(eventsMenu).toHaveTextContent("Discover University Events");
    expect(eventsTrigger).toHaveAttribute("aria-expanded", "true");

    // Second explicit click on the already-pinned item toggles it closed
    fireEvent.click(eventsTrigger);
    expect(eventsTrigger).toHaveAttribute("aria-expanded", "false");
  });
});
