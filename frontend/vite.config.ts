/**
 * @date 2026-11-21
 * @purpose Vite + React + Tailwind CSS v4 (create-vrtw) build, dev proxy, and Vitest configuration.
 */
import path from "node:path";
import tailwindcss from "@tailwindcss/vite";
import react from "@vitejs/plugin-react";
import { loadEnv } from "vite";
import { defineConfig } from "vitest/config";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  const proxyTarget = env.VITE_API_PROXY_TARGET || "http://localhost:5186";

  return {
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: {
        "@": path.resolve(import.meta.dirname, "./src"),
      },
    },
    server: {
      port: 5173,
      proxy: {
        "/api": {
          target: proxyTarget,
          changeOrigin: true,
        },
        "/health": {
          target: proxyTarget,
          changeOrigin: true,
        },
        "/openapi": {
          target: proxyTarget,
          changeOrigin: true,
        },
      },
    },
    test: {
      environment: "happy-dom",
      globals: true,
      setupFiles: "./src/test/setup.ts",
    },
  };
});
