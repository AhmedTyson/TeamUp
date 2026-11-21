/**
 * @date 2026-11-21
 * @purpose Root context provider tree (routing and future global state/auth providers).
 */
import type { ReactNode } from "react";
import { BrowserRouter } from "react-router-dom";

interface AppProvidersProps {
  children: ReactNode;
}

export function AppProviders({ children }: AppProvidersProps) {
  return <BrowserRouter>{children}</BrowserRouter>;
}
