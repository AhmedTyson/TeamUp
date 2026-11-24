/**
 * @date 2026-11-22
 * @purpose Playwright E2E smoke suite configuration starting TeamUp.Api (:5186) and Vite (:5173) against the seeded database.
 */
import { defineConfig, devices } from "@playwright/test";

const E2E_SEED_PASSWORD = process.env.VITE_DEV_SEED_PASSWORD ?? "E2e-Local-Only-Seed-2026!";

export default defineConfig({
  testDir: "./e2e",
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: 1,
  timeout: 30_000,
  expect: {
    timeout: 10_000,
  },
  reporter: process.env.CI
    ? [["github"], ["list"], ["html", { open: "never" }]]
    : [["list"], ["html", { open: "never" }]],
  use: {
    baseURL: "http://localhost:5173",
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },
  projects: [
    {
      name: "chromium",
      use: {
        ...devices["Desktop Chrome"],
        viewport: { width: 1440, height: 900 },
      },
    },
  ],
  webServer: [
    {
      command: "dotnet run --project ../backend/src/TeamUp.Api --launch-profile http",
      url: "http://localhost:5186/health",
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
      env: {
        ASPNETCORE_ENVIRONMENT: "Development",
        UseSqliteFallbackIfOffline: "true",
        Seed__Enabled: "true",
        Seed__AdminPassword: E2E_SEED_PASSWORD,
        RateLimiting__AuthPermitLimit: "100",
        RateLimiting__AnonymousPermitLimit: "200",
      },
    },
    {
      command: "npm run dev -- --port 5173 --strictPort",
      url: "http://localhost:5173",
      reuseExistingServer: !process.env.CI,
      timeout: 60_000,
      env: {
        VITE_DEV_SEED_PASSWORD: E2E_SEED_PASSWORD,
      },
    },
  ],
});
