/**
 * @date 2026-11-22
 * @purpose Automated Playwright E2E smoke tests for M1 RegisterPage and LoginPage against live TeamUp.Api and seeded database.
 */
import { expect, test } from "@playwright/test";

test.describe("M1 Authentication E2E Smoke Suite", () => {
  test("RegisterPage: loads live academic catalog, updates Campus Pass Preview & password meter, and registers a new student", async ({
    page,
  }) => {
    await page.goto("/auth/register");

    await expect(page.getByRole("heading", { name: "Create student account" })).toBeVisible();

    const universitySelect = page.locator('select[name="universityId"]');
    const facultySelect = page.locator('select[name="facultyId"]');
    const academicYearSelect = page.locator('select[name="academicYear"]');

    await expect(facultySelect).toBeDisabled();

    await expect(universitySelect).toContainText("Cairo University (Giza)");
    await expect(universitySelect).toContainText("Ain Shams University (Cairo)");
    await expect(universitySelect).toContainText("Alexandria University (Alexandria)");

    await universitySelect.selectOption("1");
    await expect(facultySelect).toBeEnabled();
    await facultySelect.selectOption("1");
    await academicYearSelect.selectOption("3");

    // Verify university change cascades and resets faculty selection
    await universitySelect.selectOption("2");
    await expect(facultySelect).toHaveValue("");
    await universitySelect.selectOption("1");
    await facultySelect.selectOption("1");

    const uniqueEmail = `salma.e2e.${Date.now()}@cu.edu.eg`;
    await page.locator('input[name="firstName"]').fill("Salma");
    await page.locator('input[name="lastName"]').fill("Khaled");
    await page.locator('input[name="email"]').fill(uniqueEmail);
    await page.locator('input[name="password"]').fill("TeamUp!2026Strong");

    const showcasePanel = page.locator("aside");
    await expect(showcasePanel).toContainText("Salma Khaled");
    await expect(showcasePanel).toContainText("Student · Yr 3");
    await expect(showcasePanel).toContainText("Cairo University");
    await expect(showcasePanel).toContainText("FCAI · Faculty of Computers and Artificial Intelligence");
    await expect(showcasePanel.getByAltText("Cairo University official logo").first()).toBeVisible();

    await expect(page.getByText("Strong · 4/4")).toBeVisible();

    await page.getByRole("button", { name: /Create Account/i }).click();

    await expect(page).toHaveURL(/\/projects$/);
    await expect(page.locator("#main-header")).toContainText("Salma");
    await expect(page.getByRole("button", { name: "Sign out" })).toBeVisible();
  });

  test("LoginPage: enforces mobile form-first layout, renders query alerts & error banner, and authenticates PlatformAdmin end-to-end", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto("/auth/login?reason=session_expired");

    await expect(page.getByText("Session expired")).toBeVisible();

    const formHeading = page.getByRole("heading", { name: "Sign in to TeamUp" });
    const showcaseHeading = page.getByRole("heading", {
      name: /Step back into your campus squad/i,
    });
    const formBox = await formHeading.boundingBox();
    const showcaseBox = await showcaseHeading.boundingBox();
    expect(formBox).not.toBeNull();
    expect(showcaseBox).not.toBeNull();
    expect(formBox!.y).toBeLessThan(showcaseBox!.y);

    await page.setViewportSize({ width: 1440, height: 900 });
    await page.goto("/auth/login?reset=success");

    await expect(page.getByText("Password updated")).toBeVisible();

    await page.locator('input[name="email"]').fill("admin@teamup.local");
    await page.locator('input[name="password"]').fill("WrongPassword123!");
    await page.getByRole("button", { name: /Sign In/i }).click();

    await expect(page.getByRole("alert").first()).toBeVisible();
    await expect(page).toHaveURL(/\/auth\/login/);

    await page.getByRole("button", { name: /PlatformAdmin/i }).click();
    await page.getByRole("button", { name: /Sign In/i }).click();

    await expect(page).toHaveURL(/\/projects$/);
    const adminLink = page.getByRole("link", { name: "Admin", exact: true });
    await expect(adminLink).toBeVisible();

    await adminLink.click();
    await expect(page).toHaveURL(/\/admin$/);
    await expect(page.getByRole("heading", { name: "Platform Overview" })).toBeVisible();
    await expect(page.getByRole("heading", { name: "Partner Universities" })).toBeVisible();
    await expect(page.getByText("Accredited Faculties")).toBeVisible();

    // Navigate to Academic Catalog and verify DB university logos + multi-page pagination
    await page.getByRole("link", { name: /Academic Catalog/i }).click();
    await expect(page).toHaveURL(/\/admin\/catalog$/);
    await expect(page.getByAltText("Cairo University official logo")).toBeVisible();
    await expect(page.getByRole("button", { name: "Next page" })).toBeEnabled();
    await page.getByRole("button", { name: "Next page" }).click();
    await expect(page).toHaveURL(/page=2/);

    await page.getByRole("button", { name: "Sign out" }).click();
    await expect(page).toHaveURL(/\/auth\/login/);
  });

  test("LandingPage & Morphing Navbar: opens liquid gliding-pill dropdown menus, morphs to white floating pill with T icon on scroll, and navigates via sub-option", async ({
    page,
  }) => {
    // Public API showcase and static people images must be publicly served from TeamUp.Api
    const showcaseRes = await page.request.get("http://localhost:5186/api/v1/students/showcase?limit=6");
    expect(showcaseRes.ok()).toBe(true);
    const showcaseBody = await showcaseRes.json();
    expect(showcaseBody.data.length).toBeGreaterThanOrEqual(3);
    expect(showcaseBody.data[0].photoUrl).toMatch(/^\/images\/people\//);
    expect(JSON.stringify(showcaseBody)).not.toContain("@");

    const imgRes = await page.request.get("http://localhost:5186/images/people/salma-khaled.jpg");
    expect(imgRes.ok()).toBe(true);
    expect(imgRes.headers()["content-type"]).toContain("image/jpeg");

    await page.goto("/");

    const heroPortraits = page
      .locator('section[aria-label="TeamUp Campus Student Hero"] img[src*="/images/people/"]');
    await expect(heroPortraits).toHaveCount(3);
    for (const idx of [0, 1, 2]) {
      await expect(heroPortraits.nth(idx)).toBeVisible();
      const loadedWidth = await heroPortraits
        .nth(idx)
        .evaluate((el) => (el as HTMLImageElement).naturalWidth);
      expect(loadedWidth).toBeGreaterThan(0);
    }

    await expect(page.locator("#brand-wordmark")).toBeVisible();
    await expect(page.locator("#brand-icon-scrolled")).toHaveCount(0);
    await expect(page.locator("#nav-gliding-pill")).toHaveCSS("opacity", "0");
    await expect(page.getByRole("link", { name: "Sign In" })).toBeVisible();

    const primaryNav = page.locator("#primary-nav");
    const glidingPill = page.locator("#nav-gliding-pill");

    await primaryNav.getByRole("button", { name: "Team Formation" }).click();
    await expect(glidingPill).toHaveCSS("opacity", "1");

    const teamFormationTransform = await glidingPill.evaluate(
      (el) => (el as HTMLElement).style.transform,
    );

    const teamMenu = primaryNav.getByRole("menu");
    await expect(teamMenu).toBeVisible();
    await expect(teamMenu).toContainText("Create a Project");
    await expect(teamMenu).toContainText("Publish Required Skills");
    await expect(teamMenu).toContainText("Project Tasks Board");

    await primaryNav.getByRole("button", { name: "Events & Workshops" }).click();
    const eventsMenu = primaryNav.getByRole("menu");
    await expect(eventsMenu).toContainText("Discover University Events");
    await expect(eventsMenu).toContainText("Verify Attendance Check-In");

    await expect(async () => {
      const nextTransform = await glidingPill.evaluate((el) => (el as HTMLElement).style.transform);
      expect(nextTransform).not.toBe(teamFormationTransform);
    }).toPass();

    // Verify Partner Universities directory renders official DB logos and paginates to page 2
    const partnerSection = page.locator("#partner-universities");
    await partnerSection.scrollIntoViewIfNeeded();
    const cairoLogo = partnerSection.getByAltText("Cairo University official logo");
    await expect(cairoLogo).toBeVisible();
    await expect(cairoLogo).toHaveAttribute("src", /^https:\/\//);
    const capitalLogo = partnerSection.getByAltText("Capital University official logo");
    await expect(capitalLogo).toBeVisible();
    await expect(capitalLogo).toHaveAttribute("src", /Capital_University_Logo\.png/);
    await partnerSection.getByRole("button", { name: "Next page" }).click();
    await expect(partnerSection.getByAltText("Zagazig University official logo")).toBeVisible();

    await page.evaluate(() => window.scrollTo({ top: 960, behavior: "instant" }));

    await expect(page.locator("#brand-icon-scrolled")).toBeVisible();
    await expect(page.locator("#brand-wordmark")).toHaveCount(0);
    await expect(page.locator("#header-inner")).toHaveClass(/bg-white/);

    await primaryNav.getByRole("button", { name: "Gamification" }).click();
    await expect(glidingPill).toHaveClass(/bg-\[#d3fb52\]/);
    await expect(glidingPill).toHaveCSS("opacity", "1");

    await page.getByRole("menuitem", { name: "Campus Leaderboards" }).click();
    await expect(page).toHaveURL(/\/leaderboard$/);
  });
});
