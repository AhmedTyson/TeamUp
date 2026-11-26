/**
 * @date 2026-11-26
 * @purpose Shared unread notification count query hook keeping cache keys and polling behavior consistent across Navbar, Dashboard, and Inbox.
 */
import { useQuery } from "@tanstack/react-query";
import { notificationsApi } from "@/api";
import { queryKeys } from "@/lib/queryKeys";

export interface UseUnreadNotificationCountOptions {
  enabled?: boolean;
  refetchInterval?: number | false;
}

export function useUnreadNotificationCount(
  options: UseUnreadNotificationCountOptions = {},
) {
  return useQuery({
    queryKey: queryKeys.notifications.unreadCount,
    queryFn: ({ signal }) => notificationsApi.getUnreadCount(signal),
    enabled: options.enabled ?? true,
    refetchInterval: options.refetchInterval ?? false,
    retry: false,
  });
}
