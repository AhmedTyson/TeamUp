/**
 * @date 2026-11-25
 * @purpose Multi-device & accessibility responsiveness suite modeled on mobile-agent-testers personas across 360px Nexus/Anna, 412px 200% font scale Grace, 768px tablet portrait, 1024px tablet landscape, and 1440px desktop.
 */
import { expect, test, type Page } from "@playwright/test";

async function measureLayoutAudit(page: Page) {
  return page.evaluate(() => {
    const vw = window.innerWidth;
    const docOverflow = document.documentElement.scrollWidth - vw;
    const header = document.getElementById("header-inner");

    let headerOverflow = 0;
    const clipped: string[] = [];
    const tinyControls: Array<{ label: string; width: number; height: number }> = [];

    if (header) {
      const hr = header.getBoundingClientRect();
      headerOverflow = header.scrollWidth - header.clientWidth;

      const controls = Array.from(header.querySelectorAll("a, button")).filter((el) => {
        const rect = el.getBoundingClientRect();
        const style = getComputedStyle(el);
        return (
          style.display !== "none" &&
          style.visibility !== "hidden" &&
          rect.width > 0 &&
          rect.height > 0
        );
      });

      for (const el of controls) {
        const r = el.getBoundingClientRect();
        const label = (el.getAttribute("aria-label") || el.textContent || "").trim().slice(0, 32);

        if (r.right > hr.right + 2 || r.left < hr.left - 2 || r.bottom > hr.bottom + 4) {
          clipped.push(label);
        }
        if (r.width < 36 || r.height < 36) {
          tinyControls.push({
            label,
            width: Math.round(r.width),
            height: Math.round(r.height),
          });
        }
      }
    }

    return {
      docOverflow,
      headerOverflow,
      clipped,
      tinyControls,
    };
  });
}

test.describe("M5 Cross-Device Responsiveness & Navbar Audit", () => {
  test("Small Phone (360x640, Anna): zero overflow, icon-only command search opens palette cleanly", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 360, height: 640 });

    for (const route of ["/", "/organizations", "/events", "/auth/login", "/auth/register"]) {
      await page.goto(route);
      await expect(page.locator("#header-inner")).toBeVisible();

      const audit = await measureLayoutAudit(page);
      expect(audit.docOverflow, `Route ${route} had horizontal overflow`).toBeLessThanOrEqual(1);
      expect(audit.headerOverflow, `Header overflow on ${route}`).toBeLessThanOrEqual(1);
      expect(audit.clipped, `Header controls clipped on ${route}`).toEqual([]);
      expect(audit.tinyControls, `Tiny header controls on ${route}`).toEqual([]);
    }

    // Icon-only Quick Search button is visible on 360px mobile and has zero text label clutter
    await page.goto("/");
    const quickSearchBtn = page.getByRole("button", { name: "Quick search command palette" });
    await expect(quickSearchBtn).toBeVisible();
    await expect(quickSearchBtn).toHaveText("");

    await quickSearchBtn.click();
    const paletteDialog = page.getByRole("dialog", { name: "Global command palette" });
    await expect(paletteDialog).toBeVisible();

    const paletteBox = await paletteDialog.locator("> div").boundingBox();
    expect(paletteBox).not.toBeNull();
    expect(paletteBox!.width).toBeLessThanOrEqual(360);
  });

  test("200% Accessibility Font Scale (412x915, Grace): header expands cleanly with zero vertical or horizontal clipping", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 412, height: 915 });

    for (const route of ["/", "/organizations", "/events"]) {
      await page.goto(route);
      await page.evaluate(() => {
        document.documentElement.style.fontSize = "200%";
      });

      const audit = await measureLayoutAudit(page);
      expect(audit.docOverflow, `200% zoom overflow on ${route}`).toBeLessThanOrEqual(1);
      expect(audit.headerOverflow, `200% zoom header overflow on ${route}`).toBeLessThanOrEqual(1);
      expect(audit.clipped, `200% zoom clipped controls on ${route}`).toEqual([]);
    }
  });

  test("Authenticated PlatformAdmin Matrix (360px, 768px, 1024px, 1440px): compact desktop nav, mobile drawer Org Requests & inner pages", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 1280, height: 800 });
    await page.goto("/auth/login");
    await page.getByRole("button", { name: /PlatformAdmin/i }).click();
    await page.getByRole("button", { name: /Sign In/i }).click();
    await expect(page).toHaveURL(/\/projects$/);

    // Check compact desktop/tablet-landscape header at 1024x768 & 1440x900 (unscrolled & scrolled pill)
    for (const size of [
      { width: 1024, height: 768 },
      { width: 1440, height: 900 },
    ]) {
      await page.setViewportSize(size);
      await page.goto("/");
      const topAudit = await measureLayoutAudit(page);
      expect(topAudit.docOverflow).toBeLessThanOrEqual(1);
      expect(topAudit.headerOverflow).toBeLessThanOrEqual(1);
      expect(topAudit.clipped).toEqual([]);
      expect(topAudit.tinyControls).toEqual([]);

      await page.evaluate(() => window.scrollTo({ top: 640, behavior: "instant" }));
      await expect(page.locator("#header-inner")).toHaveClass(/bg-white/);
      await page.waitForTimeout(420);
      const scrolledAudit = await measureLayoutAudit(page);
      expect(scrolledAudit.headerOverflow).toBeLessThanOrEqual(1);
      expect(scrolledAudit.clipped).toEqual([]);
      expect(scrolledAudit.tinyControls).toEqual([]);
    }

    // Tablet portrait (768x1024)
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto("/admin/organization-requests");
    await expect(page.getByRole("heading", { name: /^Organization Requests$/i })).toBeVisible();
    const tabletAudit = await measureLayoutAudit(page);
    expect(tabletAudit.docOverflow).toBeLessThanOrEqual(1);
    expect(tabletAudit.headerOverflow).toBeLessThanOrEqual(1);
    expect(tabletAudit.tinyControls).toEqual([]);

    // Small Phone authenticated matrix (360x640) across inner pages
    await page.setViewportSize({ width: 360, height: 640 });
    for (const route of [
      "/dashboard/student",
      "/admin",
      "/admin/organization-requests",
      "/admin/audit-logs",
      "/projects",
      "/leaderboard",
    ]) {
      await page.goto(route);
      const mobileAudit = await measureLayoutAudit(page);
      expect(mobileAudit.docOverflow, `Authenticated mobile overflow on ${route}`).toBeLessThanOrEqual(1);
      expect(mobileAudit.headerOverflow, `Authenticated header overflow on ${route}`).toBeLessThanOrEqual(1);
      expect(mobileAudit.clipped, `Authenticated clipped controls on ${route}`).toEqual([]);
      expect(mobileAudit.tinyControls, `Authenticated tiny controls on ${route}`).toEqual([]);
    }

    // Open mobile drawer on 360x640 and verify Quick Search & Org Requests link
    const toggleBtn = page.getByRole("button", { name: "Toggle navigation menu" });
    await toggleBtn.click();
    const drawer = page.locator("#mobile-navigation-drawer");
    await expect(drawer).toBeVisible();
    await expect(drawer.getByRole("button", { name: /Quick Search/i })).toBeVisible();

    const orgRequestsLink = drawer.getByRole("link", { name: "Org Requests" });
    await expect(orgRequestsLink).toBeVisible();
    await orgRequestsLink.click();
    await expect(page).toHaveURL(/\/admin\/organization-requests$/);
  });
});
