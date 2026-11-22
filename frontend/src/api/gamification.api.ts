/**
 * @date 2026-11-22
 * @purpose Gamification client for score summary, score transactions ledger, manual adjustments, and campus leaderboards.
 */
import type { QueryClient } from "@tanstack/react-query";
import { apiClient, unwrapApiResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { ScoreSourceType } from "./enums";
import type { ApiResponse, PagedResult } from "./types";
import { queryKeys } from "@/lib/queryKeys";

export interface StudentScoreDto {
  studentId: number;
  totalPoints: number;
  level: number;
  currentLevelStart: number;
  nextLevelThreshold: number;
  pointsToNext: number;
  progress: number;
}

export interface ScoreTransactionDto {
  transactionId: number;
  studentId: number;
  points: number;
  reason: string;
  sourceType: ScoreSourceType;
  createdAt: string;
}

export interface CreateScoreTransactionRequest {
  studentId: number;
  points: number;
  reason: string;
  sourceType?: ScoreSourceType;
}

export interface LeaderboardEntryDto {
  rank: number;
  studentId: number;
  firstName: string;
  lastName: string;
  facultyName: string;
  universityName: string;
  totalPoints: number;
  level: number;
  photoUrl?: string | null;
}

export interface LeaderboardQueryParams {
  page?: number;
  pageSize?: number;
}

export const gamificationApi = {
  async getLeaderboard(
    params?: LeaderboardQueryParams,
    signal?: AbortSignal,
  ): Promise<PagedResult<LeaderboardEntryDto>> {
    const res = await apiClient.get<ApiResponse<PagedResult<LeaderboardEntryDto>>>(
      API_ENDPOINTS.gamification.leaderboard,
      {
        params: {
          page: params?.page ?? 1,
          pageSize: params?.pageSize ?? 20,
        },
        signal,
      },
    );
    return unwrapApiResponse(res);
  },

  async getStudentScore(studentId: number, signal?: AbortSignal): Promise<StudentScoreDto> {
    const res = await apiClient.get<ApiResponse<StudentScoreDto>>(
      API_ENDPOINTS.students.score(studentId),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async getScoreTransactions(
    studentId: number,
    params?: { page?: number; pageSize?: number },
    signal?: AbortSignal,
  ): Promise<PagedResult<ScoreTransactionDto>> {
    const res = await apiClient.get<ApiResponse<PagedResult<ScoreTransactionDto>>>(
      API_ENDPOINTS.students.scoreTransactions(studentId),
      {
        params: {
          page: params?.page ?? 1,
          pageSize: params?.pageSize ?? 20,
        },
        signal,
      },
    );
    return unwrapApiResponse(res);
  },

  async createScoreTransaction(
    data: CreateScoreTransactionRequest,
  ): Promise<ScoreTransactionDto> {
    const res = await apiClient.post<ApiResponse<ScoreTransactionDto>>(
      API_ENDPOINTS.gamification.scoreTransactions,
      data,
    );
    return unwrapApiResponse(res);
  },
};

/**
 * Cross-module invalidation helper called when XP is earned (sprint Done, QR check-in, etc.)
 * Supports string or number studentId.
 */
export async function invalidateGamification(
  queryClient: QueryClient,
  studentId?: number | string,
): Promise<void> {
  const numericId = typeof studentId === "string" ? Number(studentId) : studentId;
  const tasks: Promise<void>[] = [
    queryClient.invalidateQueries({ queryKey: queryKeys.gamification.all }),
    queryClient.invalidateQueries({ queryKey: queryKeys.badges.all }),
    queryClient.invalidateQueries({ queryKey: queryKeys.notifications.all }),
  ];

  if (numericId !== undefined && !Number.isNaN(numericId) && numericId > 0) {
    tasks.push(
      queryClient.invalidateQueries({ queryKey: queryKeys.gamification.score(numericId) }),
      queryClient.invalidateQueries({ queryKey: queryKeys.gamification.ledger(numericId) }),
      queryClient.invalidateQueries({ queryKey: queryKeys.badges.student(numericId) }),
    );
  }

  await Promise.all(tasks);
}

