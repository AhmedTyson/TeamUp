/**
 * @date 2026-11-22
 * @purpose React hooks checking TeamUp.Api and database connectivity via GET /health with periodic polling and manual retry.
 */
import { useQuery } from "@tanstack/react-query";
import { healthApi } from "@/api";
import { queryKeys } from "@/lib/queryKeys";

export type ApiConnectionState = "checking" | "online" | "degraded" | "offline";

export interface ApiHealthDetails {
  state: ApiConnectionState;
  database: string | null;
  refetch: () => void;
}

export function useApiHealthDetails(): ApiHealthDetails {
  const query = useQuery({
    queryKey: queryKeys.health,
    queryFn: ({ signal }) => healthApi.check(signal),
    refetchInterval: 30_000,
    retry: false,
    staleTime: 10_000,
  });

  if (query.isPending) {
    return {
      state: "checking",
      database: null,
      refetch: () => {
        void query.refetch();
      },
    };
  }

  if (query.isError || !query.data) {
    return {
      state: "offline",
      database: null,
      refetch: () => {
        void query.refetch();
      },
    };
  }

  const isHealthy = query.data.status.toLowerCase() === "healthy";
  const dbStatus = query.data.database ?? null;
  const isDbConnected = !dbStatus || dbStatus.toLowerCase() === "connected";

  return {
    state: isHealthy && isDbConnected ? "online" : "degraded",
    database: dbStatus,
    refetch: () => {
      void query.refetch();
    },
  };
}

export function useApiHealth(): "checking" | "online" | "offline" {
  const { state } = useApiHealthDetails();
  if (state === "checking") {
    return "checking";
  }
  return state === "online" ? "online" : "offline";
}
