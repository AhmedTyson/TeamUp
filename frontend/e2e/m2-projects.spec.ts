/**
 * @date 2026-11-24
 * @purpose Playwright E2E tests for M2 Projects Discovery, Create Project, and Project Workspace details.
 */
import { expect, test } from "@playwright/test";

test.describe("M2 Projects Discovery & Workspace E2E", () => {
  test("authenticates PlatformAdmin, browses /projects, creates a new project, and inspects detail workspace", async ({
    page,
  }) => {
    const pageErrors: string[] = [];
    page.on("pageerror", (err) => pageErrors.push(err.message));

    await page.goto("/auth/login");
    await page.getByRole("button", { name: /PlatformAdmin/i }).click();
    await page.getByRole("button", { name: /Sign In/i }).click();
    await expect(page).toHaveURL(/\/projects$/);

    await page.goto("/projects/new");
    const uniqueTitle = `Helwan Smart Campus Bot ${Date.now()}`;
    await page.getByLabel(/Project title/i).fill(uniqueTitle);
    await page
      .getByLabel(/Description/i)
      .fill("Cross-faculty engineering and AI assistant connecting student chapters and workshops.");
    await page.getByLabel(/Maximum team size/i).fill("5");
    await page.getByRole("button", { name: /^Create project$/i }).click();

    await expect(page).toHaveURL(/\/projects\/\d+$/);
    await expect(page.getByRole("heading", { name: uniqueTitle })).toBeVisible();
    await expect(page.getByRole("button", { name: /Overview & Skills/i })).toBeVisible();

    await page.goto("/projects");
    await expect(page.getByText(uniqueTitle)).toBeVisible();
    expect(pageErrors).toEqual([]);
  });
});
