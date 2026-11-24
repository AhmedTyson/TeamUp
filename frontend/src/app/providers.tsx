/**
 * @date 2026-11-21
 * @purpose Root application provider tree composing TanStack QueryClientProvider, BrowserRouter, AuthProvider, and Sonner Toaster.
 */
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState, type ReactNode } from "react";
import { BrowserRouter } from "react-router-dom";
import { Toaster } from "sonner";
import type { AuthStudentSummary } from "@/api";
import { AuthProvider } from "@/features/auth";
import { createAppQueryClient } from "@/lib/queryClient";

export interface AppCoreProvidersProps {
  children: ReactNode;
  queryClient?: QueryClient;
  initialUser?: AuthStudentSummary | null;
}

/**
 * Router-agnostic provider tree (useful inside tests wrapped with <MemoryRouter>).
 */
export function AppCoreProviders({
  children,
  queryClient: providedClient,
  initialUser,
}: AppCoreProvidersProps) {
  const [defaultClient] = useState<QueryClient>(() => providedClient ?? createAppQueryClient());
  const client = providedClient ?? defaultClient;

  return (
    <QueryClientProvider client={client}>
      <AuthProvider initialUser={initialUser}>
        {children}
        <Toaster
          theme="dark"
          position="bottom-right"
          richColors
          closeButton
          toastOptions={{
            style: {
              background: "#1b1d26",
              border: "1px solid rgba(255, 255, 255, 0.12)",
              color: "#ffffff",
            },
          }}
        />
      </AuthProvider>
    </QueryClientProvider>
  );
}

export interface AppProvidersProps {
  children: ReactNode;
  queryClient?: QueryClient;
}

export function AppProviders({ children, queryClient }: AppProvidersProps) {
  return (
    <BrowserRouter>
      <AppCoreProviders queryClient={queryClient}>{children}</AppCoreProviders>
    </BrowserRouter>
  );
}
