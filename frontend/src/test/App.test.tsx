/**
 * @date 2026-11-21
 * @purpose Smoke tests verifying App renders TeamUp branding, modules, and API health status.
 */
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { healthApi } from "@/api";
import { App } from "@/App";

describe("App shell and home route", () => {
  it("renders TeamUp hero and platform module cards", async () => {
    vi.spyOn(healthApi, "check").mockResolvedValue({ status: "Healthy" });

    render(<App />);

    expect(screen.getByText(/let's find your next team/i)).toBeInTheDocument();
    expect(screen.getByText("Projects & Teams")).toBeInTheDocument();
    expect(screen.getByText("Student Organizations")).toBeInTheDocument();
    expect(await screen.findByText("API Online")).toBeInTheDocument();
  });
});
