/**
 * @date 2026-11-22
 * @purpose Notifications Inbox page (/notifications) with All/Unread filter, typed icon badges, mark-read, mark-all-read, and deletion.
 */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Award,
  Bell,
  Calendar,
  Check,
  CheckCheck,
  CheckCircle2,
  ClipboardList,
  Loader2,
  Trash2,
  UserCheck,
} from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import {
  notificationsApi,
  type NotificationType,
} from "@/api";
import { Badge, Button, EmptyState, ErrorBanner } from "@/components/ui";
import { formatDateTime } from "@/lib/formatters";
import { queryKeys } from "@/lib/queryKeys";

function renderNotificationIcon(type: NotificationType) {
  switch (type) {
    case "ApplicationStatus":
      return <UserCheck className="size-4 text-[#38bdf8]" aria-hidden="true" />;
    case "TaskAssigned":
      return <ClipboardList className="size-4 text-[#d3fb52]" aria-hidden="true" />;
    case "BadgeEarned":
      return <Award className="size-4 text-[#eab308]" aria-hidden="true" />;
    case "EventReminder":
      return <Calendar className="size-4 text-[#10b981]" aria-hidden="true" />;
    default:
      return <Bell className="size-4 text-white/70" aria-hidden="true" />;
  }
}

function renderTypeBadge(type: NotificationType) {
  switch (type) {
    case "ApplicationStatus":
      return <Badge variant="default" size="sm">Application</Badge>;
    case "TaskAssigned":
      return <Badge variant="accent" size="sm">Task</Badge>;
    case "BadgeEarned":
      return <Badge variant="warning" size="sm">Badge</Badge>;
    case "EventReminder":
      return <Badge variant="success" size="sm">Event</Badge>;
    default:
      return <Badge variant="default" size="sm">{type}</Badge>;
  }
}

export function NotificationsInboxPage() {
  const queryClient = useQueryClient();
  const [unreadOnly, setUnreadOnly] = useState(false);

  const notificationsQuery = useQuery({
    queryKey: queryKeys.notifications.inbox(unreadOnly),
    queryFn: ({ signal }) => notificationsApi.getNotifications(unreadOnly, signal),
  });

  const markReadMutation = useMutation({
    mutationFn: (id: number) => notificationsApi.markAsRead(id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.notifications.all });
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : "Failed to mark notification as read.";
      toast.error(msg);
    },
  });

  const markAllReadMutation = useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: async () => {
      toast.success("All notifications marked as read.");
      await queryClient.invalidateQueries({ queryKey: queryKeys.notifications.all });
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : "Failed to mark all as read.";
      toast.error(msg);
    },
  });

  const deleteNotificationMutation = useMutation({
    mutationFn: (id: number) => notificationsApi.deleteNotification(id),
    onSuccess: async () => {
      toast.success("Notification removed.");
      await queryClient.invalidateQueries({ queryKey: queryKeys.notifications.all });
    },
    onError: (err: unknown) => {
      const msg = err instanceof Error ? err.message : "Failed to delete notification.";
      toast.error(msg);
    },
  });

  const notifications = notificationsQuery.data ?? [];
  const unreadCount = notifications.filter((n) => !n.isRead).length;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-white/[0.08] pb-5">
        <div>
          <div className="inline-flex items-center gap-1.5 rounded-full border border-[#d3fb52]/30 bg-[#d3fb52]/10 px-2.5 py-0.5 text-xs font-bold text-[#d3fb52]">
            <Bell className="size-3.5" aria-hidden="true" />
            Global Activity Stream
          </div>
          <h1
            className="mt-2 text-2xl font-bold tracking-tight text-white sm:text-3xl"
            style={{ fontFamily: "var(--font-heading)" }}
          >
            Notifications Inbox
          </h1>
          <p className="mt-1 text-sm text-[#9ca3af]">
            Real-time campus updates for sprint assignments, squad acceptances, badges, and workshop check-ins.
          </p>
        </div>

        <div className="flex items-center gap-2 self-start sm:self-auto">
          {unreadCount > 0 && (
            <Button
              variant="outline"
              size="sm"
              disabled={markAllReadMutation.isPending}
              onClick={() => markAllReadMutation.mutate()}
              className="gap-1.5"
            >
              <CheckCheck className="size-4 text-[#d3fb52]" aria-hidden="true" />
              Mark All Read
            </Button>
          )}
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex items-center gap-2 border-b border-white/[0.06] pb-3">
        <button
          type="button"
          onClick={() => setUnreadOnly(false)}
          className={`cursor-pointer rounded-xl px-3.5 py-1.5 text-xs font-semibold transition-colors ${
            !unreadOnly ? "bg-[#d3fb52] text-[#14151c]" : "text-white/70 hover:text-white"
          }`}
        >
          All Notifications ({notifications.length})
        </button>
        <button
          type="button"
          onClick={() => setUnreadOnly(true)}
          className={`cursor-pointer rounded-xl px-3.5 py-1.5 text-xs font-semibold transition-colors ${
            unreadOnly ? "bg-[#d3fb52] text-[#14151c]" : "text-white/70 hover:text-white"
          }`}
        >
          Unread Only ({unreadCount})
        </button>
      </div>

      {/* Content */}
      {notificationsQuery.isLoading ? (
        <div className="flex min-h-[300px] flex-col items-center justify-center gap-3 text-sm text-[#9ca3af]">
          <Loader2 className="size-6 animate-spin text-[#d3fb52]" aria-hidden="true" />
          <span>Loading notifications feed…</span>
        </div>
      ) : notificationsQuery.isError ? (
        <ErrorBanner
          message={
            notificationsQuery.error instanceof Error
              ? notificationsQuery.error.message
              : "Unable to load notifications."
          }
          onRetry={() => void notificationsQuery.refetch()}
        />
      ) : notifications.length === 0 ? (
        <EmptyState
          icon={<CheckCircle2 className="size-6" />}
          title="All caught up!"
          description={
            unreadOnly
              ? "You have no unread notifications right now."
              : "No notifications in your inbox yet. As you collaborate on projects and events, updates will appear here."
          }
          action={
            unreadOnly ? (
              <Button variant="outline" size="sm" onClick={() => setUnreadOnly(false)}>
                View All Notifications
              </Button>
            ) : undefined
          }
        />
      ) : (
        <div className="space-y-3">
          {notifications.map((item) => (
            <div
              key={item.notificationId}
              className={`flex items-start justify-between gap-4 rounded-2xl border p-4 sm:p-5 transition-colors ${
                item.isRead
                  ? "border-white/[0.07] bg-[#1b1d26]/80"
                  : "border-[#d3fb52]/30 bg-[#d3fb52]/[0.05]"
              }`}
            >
              <div className="flex items-start gap-3.5 min-w-0">
                <div className="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-xl border border-white/10 bg-[#14151c]">
                  {renderNotificationIcon(item.type)}
                </div>
                <div className="min-w-0 space-y-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <p className="font-semibold text-white text-sm">{item.title}</p>
                    {renderTypeBadge(item.type)}
                    {!item.isRead && (
                      <span className="size-2 rounded-full bg-[#d3fb52]" aria-hidden="true" />
                    )}
                  </div>
                  <p className="text-xs text-[#9ca3af] leading-relaxed break-words">{item.message}</p>
                  <p className="font-mono text-[11px] text-white/50 pt-0.5">{formatDateTime(item.createdAt)}</p>
                </div>
              </div>

              {/* Actions */}
              <div className="flex items-center gap-1.5 shrink-0">
                {!item.isRead && (
                  <button
                    type="button"
                    aria-label={`Mark notification ${item.title} as read`}
                    onClick={() => markReadMutation.mutate(item.notificationId)}
                    disabled={markReadMutation.isPending}
                    className="inline-flex size-8 cursor-pointer items-center justify-center rounded-xl border border-white/10 bg-[#14151c] text-[#d3fb52] hover:border-[#d3fb52]/50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#d3fb52]"
                  >
                    <Check className="size-4" aria-hidden="true" />
                  </button>
                )}
                <button
                  type="button"
                  aria-label={`Delete notification ${item.title}`}
                  onClick={() => deleteNotificationMutation.mutate(item.notificationId)}
                  disabled={deleteNotificationMutation.isPending}
                  className="inline-flex size-8 cursor-pointer items-center justify-center rounded-xl border border-white/10 bg-[#14151c] text-white/50 hover:text-red-400 hover:border-red-400/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-400"
                >
                  <Trash2 className="size-4" aria-hidden="true" />
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
