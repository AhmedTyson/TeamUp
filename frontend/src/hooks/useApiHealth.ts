/**
 * @date 2026-11-21
 * @purpose React hook checking TeamUp.Api availability via GET /health.
 */
import { useEffect, useState } from "react";
import { healthApi } from "@/api";

export type ApiConnectionState = "checking" | "online" | "offline";

export function useApiHealth(): ApiConnectionState {
  const [state, setState] = useState<ApiConnectionState>("checking");

  useEffect(() => {
    const controller = new AbortController();

    healthApi
      .check(controller.signal)
      .then((res) => {
        setState(res.status.toLowerCase() === "healthy" ? "online" : "offline");
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setState("offline");
        }
      });

    return () => controller.abort();
  }, []);

  return state;
}
