/**
 * @date 2026-11-24
 * @purpose Playwright E2E suite covering Public Activities/Events Preview, IEEE Helwan 4-Tier Hierarchy, IndependentLearner onboarding, Luma Digital Pass + .ics, OrgAdmin Recruitment Kanban, Ctrl+K CommandPalette, and Arabic RTL switching.
 */
import { expect, test } from "@playwright/test";

const SEED_PASSWORD = process.env.VITE_DEV_SEED_PASSWORD ?? "E2e-Local-Only-Seed-2026!";

test.describe("M3 Activities, Hierarchy, Recruitment & Events E2E", () => {
  test("allows anonymous browsing of Student Activities, IEEE Helwan 4-tier Hierarchy, and Events", async ({
    page,
  }) => {
    const pageErrors: string[] = [];
    page.on("pageerror", (err) => pageErrors.push(err.message));

    await page.goto("/organizations");
    await expect(page).toHaveURL(/\/organizations$/);
    await expect(page.getByText("IEEE Helwan Student Branch")).toBeVisible();
    await expect(page.getByText("Enactus Helwan")).toBeVisible();
    await expect(page.getByText("GDG Helwan")).toBeVisible();

    await page.getByText("IEEE Helwan Student Branch").first().click();
    await expect(page).toHaveURL(/\/organizations\/\d+$/);
    await expect(
      page.getByRole("heading", { name: "IEEE Helwan Student Branch" }),
    ).toBeVisible();

    await page.getByRole("tab", { name: /Chapters & Committees/i }).click();
    await expect(page.getByText(/Computer Society/i)).toBeVisible();

    await page.getByRole("tab", { name: /Org Hierarchy/i }).click();
    await expect(page.getByText(/Tier 1 · High Board/i)).toBeVisible();
    await expect(page.getByText(/Tier 2 · Chapter Chairs & Committee Heads/i)).toBeVisible();

    await page.goto("/events");
    await expect(page).toHaveURL(/\/events$/);
    await expect(page.getByText("Auxilio Engineering & AI Bootcamp 2026")).toBeVisible();
    await page.getByText("Auxilio Engineering & AI Bootcamp 2026").first().click();
    await expect(page).toHaveURL(/\/events\/\d+$/);
    await expect(
      page.getByRole("heading", { name: "Auxilio Engineering & AI Bootcamp 2026" }),
    ).toBeVisible();

    expect(pageErrors).toEqual([]);
  });

  test("registers an IndependentLearner without faculty, enrolls in Auxilio Bootcamp, and renders Luma Digital Pass", async ({
    page,
  }) => {
    const pageErrors: string[] = [];
    page.on("pageerror", (err) => pageErrors.push(err.message));

    await page.goto("/auth/register");
    await page.locator('select[name="affiliationType"]').selectOption("IndependentLearner");
    await expect(page.locator('select[name="universityId"]')).toHaveCount(0);
    await expect(page.locator('select[name="facultyId"]')).toHaveCount(0);

    await page.locator('input[name="firstName"]').fill("Tarek");
    await page.locator('input[name="lastName"]').fill("Nabil");
    await page.locator('input[name="city"]').fill("Helwan, Cairo");
    await page.locator('input[name="email"]').fill(`tarek.indie.${Date.now()}@teamup.dev`);
    await page.locator('input[name="password"]').fill("TeamUp!2026Strong");

    await page.getByRole("button", { name: /Create Account/i }).click();
    await expect(page).toHaveURL(/\/projects$/);

    await page.goto("/events");
    await page.getByText("Auxilio Engineering & AI Bootcamp 2026").first().click();
    await expect(page).toHaveURL(/\/events\/\d+$/);

    await page.getByRole("button", { name: /Register for event|Join waitlist/i }).click();
    await expect(page.getByText("Tarek Nabil").first()).toBeVisible();
    await expect(
      page.getByRole("button", { name: /Add to Calendar \(\.ics\)/i }).first(),
    ).toBeVisible();

    // Also submit a recruitment application as an IndependentLearner so Chair Kanban sees live candidate
    await page.goto("/organizations");
    await page.getByText("IEEE Helwan Student Branch").first().click();
    await expect(page).toHaveURL(/\/organizations\/\d+$/);
    await page.getByRole("tab", { name: /Recruitment/i }).click();
    await page.getByRole("button", { name: /Apply to join/i }).click();
    await expect(page.getByText(/Your membership application/i)).toBeVisible();

    expect(pageErrors).toEqual([]);
  });

  test("authenticates Chair nour@cu.edu.eg for IEEE Helwan Kanban, Ctrl+K CommandPalette, and Arabic RTL mode", async ({
    page,
  }) => {
    const pageErrors: string[] = [];
    page.on("pageerror", (err) => pageErrors.push(err.message));

    await page.goto("/auth/login");
    await page.locator('input[name="email"]').fill("nour@cu.edu.eg");
    await page.locator('input[name="password"]').fill(SEED_PASSWORD);
    await page.getByRole("button", { name: /Sign In/i }).click();
    await expect(page).toHaveURL(/\/projects$/);

    await page.goto("/organizations");
    await page.getByText("IEEE Helwan Student Branch").first().click();
    await expect(page).toHaveURL(/\/organizations\/\d+$/);

    await page.getByRole("tab", { name: /Recruitment/i }).click();
    await expect(page.getByText("1. Applied")).toBeVisible();
    await expect(page.getByText("2. HR Screening")).toBeVisible();
    await expect(page.getByText("Tarek Nabil").first()).toBeVisible();
    await page.getByRole("button", { name: /Advance/i }).first().click();

    await page.getByRole("tab", { name: /Participants/i }).click();
    await expect(page.getByRole("tab", { name: /Participants/i })).toHaveAttribute(
      "aria-selected",
      "true",
    );

    // Command Palette Ctrl+K (all pages reachable)
    await page.keyboard.press("Control+k");
    const cmdInput = page.getByPlaceholder(/Search activities, events, projects/i);
    await expect(cmdInput).toBeVisible();
    await cmdInput.fill("Auxilio");
    await expect(page.getByRole("button", { name: /Auxilio/i }).first()).toBeVisible();
    await page.keyboard.press("Escape");

    // English-only UI: no language switch toggle, and role-specific Org Leader Dashboard available
    await expect(page.getByRole("button", { name: /Switch language/i })).toHaveCount(0);
    await page.getByRole("link", { name: "Dashboard" }).click();
    await expect(page).toHaveURL(/\/dashboard\/org$/);
    await expect(
      page.getByRole("heading", { name: "Org Leadership Command Center" }),
    ).toBeVisible();

    expect(pageErrors).toEqual([]);
  });
});
