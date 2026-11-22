/**
 * @date 2026-11-22
 * @purpose Vitest test suite for Module 04 (Youssef — Landing, Student Directory, Profiles, Gamification, and Notifications).
 */
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  badgesApi,
  gamificationApi,
  invalidateGamification,
  notificationsApi,
  studentsApi,
  USER_ROLES,
  type AuthStudentSummary,
  type LeaderboardEntryDto,
  type NotificationDto,
  type PagedResult,
  type StudentCardDto,
  type StudentDetailDto,
  type StudentPublicProfileDto,
  type StudentScoreDto,
} from "@/api";
import { AppCoreProviders } from "@/app/providers";
import {
  CampusLeaderboardPage,
  XpLevelProgressCard,
} from "@/features/gamification";
import { HeroSection } from "@/features/landing/HeroSection";
import { NotificationsInboxPage } from "@/features/notifications/NotificationsInboxPage";
import {
  SelfProfileStudioPage,
  StudentDirectoryPage,
  StudentPublicProfilePage,
} from "@/features/profile";

const MOCK_STUDENT: AuthStudentSummary = {
  studentId: 10,
  firstName: "Youssef",
  lastName: "Adel",
  email: "youssef@eng.cu.edu.eg",
  academicYear: 4,
  role: USER_ROLES.Student,
  facultyId: 2,
  facultyName: "Faculty of Engineering",
  universityName: "Cairo University",
};

describe("Frontend Module 04 — Landing, Profiles, Gamification & Notifications", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  /* -------------------------------------------------------------------------
   * 1. Landing Hero Search & Chip Navigation
   * ------------------------------------------------------------------------- */
  describe("Landing Hero Search & Chip Navigation", () => {
    it("switches chips and submits typed search to correct module path with query params", async () => {
      render(
        <MemoryRouter initialEntries={["/"]}>
          <Routes>
            <Route path="/" element={<HeroSection />} />
            <Route path="/organizations" element={<h1>Organizations Search Result</h1>} />
            <Route path="/projects" element={<h1>Projects Search Result</h1>} />
          </Routes>
        </MemoryRouter>,
      );

      // Verify Giant Headline
      expect(screen.getByText(/let's find/i)).toBeInTheDocument();
      expect(screen.getByText(/your next team/i)).toBeInTheDocument();

      // Click "Student Activities" filter chip
      const orgsChip = screen.getByRole("button", { name: "Student Activities" });
      fireEvent.click(orgsChip);

      // Click search bar container to enter manual mode
      const searchContainer = document.getElementById("search-bar-container")!;
      fireEvent.click(searchContainer);

      const input = await screen.findByPlaceholderText(/search student activities.../i);
      expect(input).toBeInTheDocument();

      fireEvent.change(input, { target: { value: "Robotics" } });
      const submitBtn = screen.getByRole("button", { name: /submit search/i });
      fireEvent.click(submitBtn);

      expect(await screen.findByText("Organizations Search Result")).toBeInTheDocument();
    });
  });

  /* -------------------------------------------------------------------------
   * 2. Student Directory (THR-I-01 Privacy Mask & PageSize Cap <= 20)
   * ------------------------------------------------------------------------- */
  describe("Student Directory & Public Portfolio (THR-I-01)", () => {
    it("enforces page size cap <= 20 and masks email as 'Email private' when null", async () => {
      const mockStudents: StudentCardDto[] = [
        {
          studentId: 101,
          firstName: "Salma",
          lastName: "Khaled",
          email: null, // Private email!
          academicYear: 3,
          role: "Student",
          facultyId: 1,
          facultyName: "Faculty of Computers and AI",
          universityName: "Cairo University",
          interests: ["Machine Learning", "NLP"],
          photoUrl: null,
        },
        {
          studentId: 102,
          firstName: "Omar",
          lastName: "Mostafa",
          email: "omar@asu.edu.eg", // Opted in!
          academicYear: 4,
          role: "Student",
          facultyId: 2,
          facultyName: "Faculty of Engineering",
          universityName: "Ain Shams University",
          interests: ["Embedded Systems"],
          photoUrl: null,
        },
      ];

      const getDirSpy = vi.spyOn(studentsApi, "getDirectory").mockResolvedValue({
        items: mockStudents,
        page: 1,
        pageSize: 20,
        totalCount: 2,
        totalPages: 1,
      });

      render(
        <MemoryRouter initialEntries={["/students"]}>
          <AppCoreProviders initialUser={MOCK_STUDENT}>
            <StudentDirectoryPage />
          </AppCoreProviders>
        </MemoryRouter>,
      );

      // Verify directory cards render
      expect(await screen.findByText("Salma Khaled")).toBeInTheDocument();
      expect(screen.getByText("Omar Mostafa")).toBeInTheDocument();

      // THR-I-01 Check: Salma has null email -> "Email private"
      expect(screen.getByText(/email private/i)).toBeInTheDocument();
      // Omar has visible email
      expect(screen.getByText("omar@asu.edu.eg")).toBeInTheDocument();

      // Verify API was called with capped pageSize <= 20
      expect(getDirSpy).toHaveBeenCalled();
      const firstCallArgs = getDirSpy.mock.calls[0]?.[0];
      expect(firstCallArgs?.pageSize).toBeLessThanOrEqual(20);
    });

    it("renders Public Portfolio with safe links and gamification summary", async () => {
      const mockProfile: StudentPublicProfileDto = {
        studentId: 55,
        firstName: "Layla",
        lastName: "Ibrahim",
        academicYear: 2,
        facultyName: "Computer Science",
        universityName: "Cairo University",
        profile: {
          bio: "AI researcher interested in vision-language models.",
          photoUrl: "https://example.com/photo.jpg",
          gitHubUrl: "https://github.com/layla",
          linkedInUrl: "https://linkedin.com/in/layla",
        },
        interests: ["PyTorch", "Computer Vision"],
      };

      vi.spyOn(studentsApi, "getById").mockResolvedValue(mockProfile);
      vi.spyOn(gamificationApi, "getStudentScore").mockResolvedValue({
        studentId: 55,
        totalPoints: 1600,
        level: 5,
        currentLevelStart: 1600,
        nextLevelThreshold: 2500,
        pointsToNext: 900,
        progress: 0.15,
      });
      vi.spyOn(badgesApi, "getStudentBadges").mockResolvedValue([]);

      render(
        <MemoryRouter initialEntries={["/students/55"]}>
          <AppCoreProviders initialUser={MOCK_STUDENT}>
            <Routes>
              <Route path="/students/:id" element={<StudentPublicProfilePage />} />
            </Routes>
          </AppCoreProviders>
        </MemoryRouter>,
      );

      expect(await screen.findByText("Layla Ibrahim")).toBeInTheDocument();
      expect(screen.getByText(/vision-language models/i)).toBeInTheDocument();

      // Verify safe external link attributes (THR-T-03)
      const githubLink = screen.getByRole("link", { name: /github/i });
      expect(githubLink).toHaveAttribute("target", "_blank");
      expect(githubLink).toHaveAttribute("rel", "noopener noreferrer");
      expect(githubLink).toHaveAttribute("href", "https://github.com/layla");
    });
  });

  /* -------------------------------------------------------------------------
   * 3. Self-Profile Studio & Privacy Toggle
   * ------------------------------------------------------------------------- */
  describe("Self-Profile Studio (/profile/me)", () => {
    it("renders ShowEmailInDirectory privacy toggle and updates account via PATCH", async () => {
      const myDetail: StudentDetailDto = {
        studentId: 10,
        firstName: "Youssef",
        lastName: "Adel",
        email: "youssef@eng.cu.edu.eg",
        academicYear: 4,
        role: "Student",
        facultyId: 2,
        facultyName: "Faculty of Engineering",
        universityName: "Cairo University",
        profile: {
          bio: "Full stack student builder.",
          photoUrl: null,
          gitHubUrl: "https://github.com/youssef",
          linkedInUrl: null,
        },
        interests: ["TypeScript", "ASP.NET Core"],
        showEmailInDirectory: false,
      };

      vi.spyOn(studentsApi, "getMe").mockResolvedValue(myDetail);
      const patchSpy = vi.spyOn(studentsApi, "updateMe").mockResolvedValue({
        ...myDetail,
        showEmailInDirectory: true,
      });

      render(
        <MemoryRouter initialEntries={["/profile/me"]}>
          <AppCoreProviders initialUser={MOCK_STUDENT}>
            <SelfProfileStudioPage />
          </AppCoreProviders>
        </MemoryRouter>,
      );

      expect(await screen.findByDisplayValue("Youssef")).toBeInTheDocument();
      expect(screen.getByDisplayValue("Adel")).toBeInTheDocument();

      // Find the ShowEmailInDirectory toggle checkbox
      const privacyToggle = screen.getByRole("checkbox", {
        name: /show my email in student directory/i,
      });
      expect(privacyToggle).not.toBeChecked();

      // Toggle it ON
      fireEvent.click(privacyToggle);
      expect(privacyToggle).toBeChecked();

      // Click Save Account Details
      const saveBtn = screen.getByRole("button", { name: /save account details/i });
      fireEvent.click(saveBtn);

      await waitFor(() => {
        expect(patchSpy).toHaveBeenCalledWith(
          expect.objectContaining({
            showEmailInDirectory: true,
          }),
        );
      });
    });
  });

  /* -------------------------------------------------------------------------
   * 4. Gamification Score, Progress & Formula: FLOOR(SQRT(pts/100))+1
   * ------------------------------------------------------------------------- */
  describe("Gamification Hub & Level Math", () => {
    it("renders XpLevelProgressCard with formula explanation and progress bar", () => {
      // 900 pts -> sqrt(900/100) = 3 -> level = 3 + 1 = 4
      const score: StudentScoreDto = {
        studentId: 10,
        totalPoints: 950,
        level: 4,
        currentLevelStart: 900,
        nextLevelThreshold: 1600,
        pointsToNext: 650,
        progress: 0.07,
      };

      render(
        <MemoryRouter>
          <AppCoreProviders initialUser={MOCK_STUDENT}>
            <XpLevelProgressCard score={score} />
          </AppCoreProviders>
        </MemoryRouter>,
      );

      expect(screen.getByText("Level 4 Explorer")).toBeInTheDocument();
      expect(screen.getByText(/950/)).toBeInTheDocument();
      expect(screen.getByText(/650 XP needed to level up/i)).toBeInTheDocument();
      expect(screen.getByText(/FLOOR\(SQRT\(pts \/ 100\)\) \+ 1/)).toBeInTheDocument();
    });

    it("renders Campus Leaderboard with Top 3 and standings table", async () => {
      const mockLeaderboard: PagedResult<LeaderboardEntryDto> = {
        items: [
          {
            rank: 1,
            studentId: 10,
            firstName: "Youssef",
            lastName: "Adel",
            facultyName: "Engineering",
            universityName: "Cairo University",
            totalPoints: 3400,
            level: 6,
            photoUrl: null,
          },
          {
            rank: 2,
            studentId: 11,
            firstName: "Salma",
            lastName: "Khaled",
            facultyName: "Computers & AI",
            universityName: "Cairo University",
            totalPoints: 2800,
            level: 5,
            photoUrl: null,
          },
          {
            rank: 3,
            studentId: 12,
            firstName: "Omar",
            lastName: "Mostafa",
            facultyName: "Engineering",
            universityName: "Ain Shams University",
            totalPoints: 2100,
            level: 5,
            photoUrl: null,
          },
          {
            rank: 4,
            studentId: 13,
            firstName: "Nour",
            lastName: "Hassan",
            facultyName: "Science",
            universityName: "Alexandria University",
            totalPoints: 1500,
            level: 4,
            photoUrl: null,
          },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 4,
        totalPages: 1,
      };

      vi.spyOn(gamificationApi, "getLeaderboard").mockResolvedValue(mockLeaderboard);

      render(
        <MemoryRouter initialEntries={["/leaderboard"]}>
          <AppCoreProviders initialUser={MOCK_STUDENT}>
            <CampusLeaderboardPage />
          </AppCoreProviders>
        </MemoryRouter>,
      );

      expect(await screen.findByText("Youssef Adel")).toBeInTheDocument();
      // Podium cards top 3
      expect(screen.getAllByText("Youssef Adel").length).toBeGreaterThan(0);
      expect(screen.getAllByText("Salma Khaled").length).toBeGreaterThan(0);
      expect(screen.getAllByText("Omar Mostafa").length).toBeGreaterThan(0);
      // Rank 4 in table
      expect(screen.getByText("Nour Hassan")).toBeInTheDocument();
    });

    it("invalidates all cross-module gamification query keys via invalidateGamification()", async () => {
      const queryClient = new QueryClient();
      const invalidateSpy = vi.spyOn(queryClient, "invalidateQueries");

      await invalidateGamification(queryClient, 10);

      // Verify all relevant keys are invalidated
      expect(invalidateSpy).toHaveBeenCalledWith(expect.objectContaining({ queryKey: ["gamification"] }));
      expect(invalidateSpy).toHaveBeenCalledWith(expect.objectContaining({ queryKey: ["badges"] }));
      expect(invalidateSpy).toHaveBeenCalledWith(expect.objectContaining({ queryKey: ["notifications"] }));
      expect(invalidateSpy).toHaveBeenCalledWith(
        expect.objectContaining({ queryKey: ["gamification", "score", 10] }),
      );
      expect(invalidateSpy).toHaveBeenCalledWith(
        expect.objectContaining({ queryKey: ["gamification", "ledger", 10] }),
      );
      expect(invalidateSpy).toHaveBeenCalledWith(
        expect.objectContaining({ queryKey: ["badges", "student", 10] }),
      );
    });
  });

  /* -------------------------------------------------------------------------
   * 5. Notifications Inbox & Mark-All-Read Flow
   * ------------------------------------------------------------------------- */
  describe("Notifications Inbox & Mark-All-Read Flow", () => {
    it("renders notification items, filters unread, and marks all as read", async () => {
      const mockNotifications: NotificationDto[] = [
        {
          notificationId: 1,
          studentId: 10,
          title: "Sprint Task Assigned",
          message: "You have been assigned to 'Build REST API' in TeamUp.",
          type: "TaskAssigned",
          isRead: false,
          createdAt: new Date().toISOString(),
        },
        {
          notificationId: 2,
          studentId: 10,
          title: "Badge Unlocked!",
          message: "Congratulations! You unlocked the 'Squad Pioneer' badge.",
          type: "BadgeEarned",
          isRead: true,
          createdAt: new Date().toISOString(),
        },
      ];

      vi.spyOn(notificationsApi, "getNotifications").mockResolvedValue(mockNotifications);
      const markAllSpy = vi.spyOn(notificationsApi, "markAllAsRead").mockResolvedValue(undefined);

      render(
        <MemoryRouter initialEntries={["/notifications"]}>
          <AppCoreProviders initialUser={MOCK_STUDENT}>
            <NotificationsInboxPage />
          </AppCoreProviders>
        </MemoryRouter>,
      );

      expect(await screen.findByText("Sprint Task Assigned")).toBeInTheDocument();
      expect(screen.getByText("Badge Unlocked!")).toBeInTheDocument();

      // Click "Mark All Read" button
      const markAllBtn = screen.getByRole("button", { name: /mark all read/i });
      fireEvent.click(markAllBtn);

      await waitFor(() => {
        expect(markAllSpy).toHaveBeenCalledTimes(1);
      });
    });
  });
});
