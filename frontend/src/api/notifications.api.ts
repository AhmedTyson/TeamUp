/**
 * @date 2026-11-22
 * @purpose Notifications API client for student notification inbox, mark as read, and bulk dismiss.
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { NotificationType } from "./enums";
import type { ApiResponse } from "./types";

export interface NotificationDto {
  notificationId: number;
  studentId: number;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
}

export const notificationsApi = {
  async getNotifications(
    unreadOnly = false,
    take = 15,
    signal?: AbortSignal,
  ): Promise<NotificationDto[]> {
    const res = await apiClient.get<ApiResponse<NotificationDto[]>>(
      API_ENDPOINTS.gamification.notifications,
      {
        params: { unreadOnly, take },
        signal,
      },
    );
    return unwrapApiResponse(res);
  },

  async getUnreadCount(signal?: AbortSignal): Promise<number> {
    const res = await apiClient.get<ApiResponse<number>>(
      API_ENDPOINTS.gamification.notificationsUnreadCount,
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async markAsRead(id: number): Promise<void> {
    const res = await apiClient.patch(API_ENDPOINTS.gamification.notificationById(id));
    unwrapVoidResponse(res);
  },

  async markAllAsRead(): Promise<void> {
    const res = await apiClient.patch(API_ENDPOINTS.gamification.markAllNotificationsRead);
    unwrapVoidResponse(res);
  },

  async deleteNotification(id: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.gamification.notificationById(id));
    unwrapVoidResponse(res);
  },
};

