/**
 * @date 2026-11-22
 * @purpose Global Navbar NotificationBell dropdown with 60s background polling, unread badge, quick mark-read, and mark-all-read support.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Bell, Check, CheckCheck, ExternalLink } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";
import {
  API_ENDPOINTS,
  apiClient,
  unwrapApiResponse,
  unwrapVoidResponse,
  type ApiResponse,
  type NotificationType,
} from "@/api";
import { Badge } from "@/components/ui";
import { useAuth } from "@/features/auth";
import { cn } from "@/lib/cn";
import { queryKeys } from "@/lib/queryKeys";

export interface NavbarNotificationItem {
  notificationId: number;
  studentId: number;
  title: string;
  message: string;
  type: NotificationType | number;
  isRead: boolean;
  createdAt: string;
}

export function NotificationBell() {
  const { isAuthenticated } = useAuth();
  const queryClient = useQueryClient();
  const containerRef = useRef<HTMLDivElement>(null);
  const [isOpen, setIsOpen] = useState(false);

  const notificationsQuery = useQuery({
    queryKey: queryKeys.notifications.inbox(false),
    queryFn: async ({ signal }) => {
      const res = await apiClient.get<ApiResponse<NavbarNotificationItem[]>>(
        API_ENDPOINTS.gamification.notifications,
        {
          params: { unreadOnly: false },
          signal,
        },
      );
      return unwrapApiResponse(res);
    },
    enabled: isAuthenticated,
    refetchInterval: 60_000,
    retry: false,
  });

  const markReadMutation = useMutation({
    mutationFn: async (notificationId: number) => {
      const res = await apiClient.patch(
        API_ENDPOINTS.gamification.notificationById(notificationId),
      );
      unwrapVoidResponse(res);
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.notifications.all });
    },
  });

  const markAllReadMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.patch(API_ENDPOINTS.gamification.markAllNotificationsRead);
      unwrapVoidResponse(res);
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.notifications.all });
      toast.success("All notifications marked as read.");
    },
  });

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsOpen(false);
      }
    };

    const handlePointerDown = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    window.addEventListener("mousedown", handlePointerDown);
    return () => {
      window.removeEventListener("keydown", handleKeyDown);
      window.removeEventListener("mousedown", handlePointerDown);
    };
  }, [isOpen]);

  if (!isAuthenticated) {
    return null;
  }

  const items = notificationsQuery.data ?? [];
  const unreadCount = items.filter((item) => !item.isRead).length;
  const recentItems = items.slice(0, 5);

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        onClick={() => setIsOpen((prev) => !prev)}
        aria-label={
          unreadCount > 0 ? `Open notifications (${unreadCount} unread)` : "Open notifications"
        }
        aria-expanded={isOpen}
        aria-haspopup="dialog"
        className="relative inline-flex size-8 cursor-pointer items-center justify-center rounded-xl border border-white/10 bg-[#1b1d26] text-white/75 transition-colors hover:border-[#d3fb52]/40 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
      >
        <Bell className="size-4" aria-hidden="true" />
        {unreadCount > 0 && (
          <span className="absolute -right-1 -top-1 inline-flex min-w-4 items-center justify-center rounded-full bg-[#d3fb52] px-1 text-[10px] font-bold leading-4 text-[#14151c] tabular-nums">
            {unreadCount > 9 ? "9+" : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div
          role="dialog"
          aria-label="Recent notifications"
          className="absolute right-0 z-50 mt-2 w-80 overflow-hidden rounded-2xl border border-white/12 bg-[#1b1d26] shadow-2xl shadow-black/70 sm:w-96"
        >
          <div className="flex items-center justify-between border-b border-white/[0.085] px-4 py-3">
            <div className="flex items-center gap-2">
              <span className="text-sm font-semibold text-white">Notifications</span>
              {unreadCount > 0 && <Badge variant="accent" size="sm">{unreadCount} new</Badge>}
            </div>

            {unreadCount > 0 && (
              <button
                type="button"
                disabled={markAllReadMutation.isPending}
                onClick={() => {
                  void markAllReadMutation.mutateAsync();
                }}
                className="inline-flex cursor-pointer items-center gap-1 text-xs font-semibold text-[#d3fb52] hover:underline disabled:opacity-50"
              >
                <CheckCheck className="size-3.5" aria-hidden="true" />
                Mark all read
              </button>
            )}
          </div>

          <div className="max-h-80 divide-y divide-white/[0.07] overflow-y-auto">
            {recentItems.length === 0 ? (
              <p className="px-4 py-8 text-center text-xs text-[#9ca3af]">
                You are all caught up. No notifications yet.
              </p>
            ) : (
              recentItems.map((item) => (
                <div
                  key={item.notificationId}
                  className={cn(
                    "flex items-start justify-between gap-3 px-4 py-3 text-xs transition-colors",
                    item.isRead ? "bg-transparent" : "bg-[#d3fb52]/[0.05]",
                  )}
                >
                  <div className="space-y-1">
                    <p className="font-semibold text-white">{item.title}</p>
                    <p className="text-[#9ca3af]">{item.message}</p>
                  </div>

                  {!item.isRead && (
                    <button
                      type="button"
                      aria-label={`Mark notification ${item.title} as read`}
                      onClick={() => {
                        void markReadMutation.mutateAsync(item.notificationId);
                      }}
                      className="mt-0.5 inline-flex size-6 shrink-0 cursor-pointer items-center justify-center rounded-lg border border-white/10 bg-[#14151c] text-[#d3fb52] hover:border-[#d3fb52]/50"
                    >
                      <Check className="size-3.5" aria-hidden="true" />
                    </button>
                  )}
                </div>
              ))
            )}
          </div>

          <div className="border-t border-white/[0.085] bg-[#14151c]/70 px-4 py-2.5 text-right">
            <Link
              to="/notifications"
              onClick={() => setIsOpen(false)}
              className="inline-flex items-center gap-1 text-xs font-semibold text-[#d3fb52] hover:underline"
            >
              View full inbox
              <ExternalLink className="size-3" aria-hidden="true" />
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}
