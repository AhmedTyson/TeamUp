/**
 * @date 2026-11-21
 * @purpose Top-level TeamUp React application component.
 */
import { AppProviders } from "@/app/providers";
import { AppRouter } from "@/app/router";

export function App() {
  return (
    <AppProviders>
      <AppRouter />
    </AppProviders>
  );
}

export default App;
