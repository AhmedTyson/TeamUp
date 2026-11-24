/**
 * @date 2026-11-24
 * @purpose Playwright E2E verification for M4 Profiles Studio, Student Directory, Campus Leaderboard, Gamification Hub, and Notifications Center.
 */
import { expect, test } from "@playwright/test";

const SEED_PASSWORD = process.env.VITE_DEV_SEED_PASSWORD ?? "E2e-Local-Only-Seed-2026!";

test.describe("M4 Profiles, Directory & Gamification E2E", () => {
  test("authenticates Salma Khaled and verifies Profile Studio, Directory, Leaderboard, Gamification, and Notifications", async ({
    page,
  }) => {
    const pageErrors: string[] = [];
    page.on("pageerror", (err) => pageErrors.push(err.message));

    await page.goto("/auth/login");
    await page.locator('input[name="email"]').fill("salma@cu.edu.eg");
    await page.locator('input[name="password"]').fill(SEED_PASSWORD);
    await page.getByRole("button", { name: /Sign In/i }).click();
    await expect(page).toHaveURL(/\/projects$/);

    await page.goto("/profile/me");
    await expect(page.locator("h1").first()).toBeVisible();
    await expect(page.locator('select[name="affiliationType"]')).toBeVisible();

    await page.goto("/students");
    await expect(page.locator("h1").first()).toBeVisible();
    await page.getByPlaceholder("Search by student name...").fill("Salma");
    await expect(page.getByText("Salma Khaled").first()).toBeVisible();

    await page.goto("/leaderboard");
    await expect(page.locator("h1").first()).toBeVisible();

    await page.goto("/gamification");
    await expect(page.locator("h1").first()).toBeVisible();

    await page.goto("/notifications");
    await expect(page.locator("h1").first()).toBeVisible();

    await page.goto("/dashboard");
    await expect(
      page.getByRole("heading", { name: "Org Leadership Command Center" }),
    ).toBeVisible();
    await page.getByRole("link", { name: "Student Dashboard" }).click();
    await expect(page).toHaveURL(/\/dashboard\/student$/);
    await expect(page.getByRole("heading", { name: /Welcome back, Salma/i })).toBeVisible();

    expect(pageErrors).toEqual([]);
  });
});
