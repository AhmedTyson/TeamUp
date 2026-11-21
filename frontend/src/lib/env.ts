/**
 * @date 2026-11-21
 * @purpose Centralized, typed access to Vite runtime environment variables.
 */
export const env = {
  /**
   * Base URL for backend API calls.
   * Empty string defaults to same-origin requests routed through Vite's dev proxy.
   */
  apiBaseUrl: (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, ""),
} as const;
