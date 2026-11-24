/**
 * @date 2026-11-22
 * @purpose Global Navbar NotificationBell dropdown with 60s background polling, unread badge, quick mark-read, and mark-all-read support.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Bell, Check, CheckCheck, ExternalLink } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { Link, useLocation } from "react-router-dom";
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

export interface NotificationBellProps {
  isScrolled?: boolean;
  onOpenChange?: (isOpen: boolean) => void;
}

export interface NavbarNotificationItem {
  notificationId: number;
  studentId: number;
  title: string;
  message: string;
  type: NotificationType | number;
  isRead: boolean;
  createdAt: string;
}

export function NotificationBell({ isScrolled = false, onOpenChange }: NotificationBellProps) {
  const { isAuthenticated } = useAuth();
  const queryClient = useQueryClient();
  const location = useLocation();
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
    setIsOpen(false);
    onOpenChange?.(false);
  }, [location.pathname, location.search]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        setIsOpen(false);
        onOpenChange?.(false);
      }
    };

    const handlePointerDown = (event: PointerEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
        onOpenChange?.(false);
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    window.addEventListener("pointerdown", handlePointerDown);
    return () => {
      window.removeEventListener("keydown", handleKeyDown);
      window.removeEventListener("pointerdown", handlePointerDown);
    };
  }, [isOpen, onOpenChange]);

  if (!isAuthenticated) {
    return null;
  }

  const items = notificationsQuery.data ?? [];
  const unreadCount = items.filter((item) => !item.isRead).length;
  const recentItems = items.slice(0, 5);

  return (
    <div ref={containerRef} className="relative" onClick={(event) => event.stopPropagation()}>
      <button
        type="button"
        onClick={(event) => {
          event.stopPropagation();
          setIsOpen((prev) => {
            const next = !prev;
            onOpenChange?.(next);
            return next;
          });
        }}
        aria-label={
          unreadCount > 0 ? `Open notifications (${unreadCount} unread)` : "Open notifications"
        }
        aria-expanded={isOpen}
        aria-haspopup="dialog"
        className={cn(
          "relative inline-flex size-9 cursor-pointer items-center justify-center rounded-xl border transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]",
          isScrolled
            ? "border-black/15 bg-black/5 text-[#14151c] hover:border-black/30 hover:bg-black/10"
            : "border-white/15 bg-[#1b1d26]/90 text-white/80 hover:border-[#d3fb52]/40 hover:text-white",
        )}
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
          className="fixed inset-x-3 top-[76px] z-50 mx-auto max-w-[min(24rem,calc(100vw-1.5rem))] overflow-hidden rounded-2xl border border-white/12 bg-[#1b1d26] shadow-2xl shadow-black/70 sm:absolute sm:inset-x-auto sm:right-0 sm:top-auto sm:mt-2 sm:w-[min(24rem,calc(100vw-2rem))]"
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

          <div className="max-h-[min(20rem,calc(100dvh-11.5rem))] divide-y divide-white/[0.07] overflow-y-auto overscroll-contain">
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
