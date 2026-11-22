/**
 * Typed API wrappers for project sprint tasks and assignees.
 */
import { apiClient, unwrapApiResponse, unwrapVoidResponse } from "./client";
import { API_ENDPOINTS } from "./endpoints";
import type { ProjectTaskStatus, TaskPriority } from "./enums";
import type { ApiResponse } from "./types";

export interface ProjectTaskDto {
  taskId: number;
  title: string;
  description: string | null;
  status: ProjectTaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
  projectId: number;
  assignedStudentId: number | null;
  assignedStudentName: string | null;
}

export interface CreateProjectTaskPayload {
  title: string;
  description: string | null;
  dueDate: string | null;
  priority: TaskPriority;
}

export interface UpdateProjectTaskPayload extends CreateProjectTaskPayload {
  status: ProjectTaskStatus;
}

export const tasksApi = {
  async listForProject(projectId: number, signal?: AbortSignal): Promise<ProjectTaskDto[]> {
    const res = await apiClient.get<ApiResponse<ProjectTaskDto[]>>(
      API_ENDPOINTS.projects.tasks(projectId),
      { signal },
    );
    return unwrapApiResponse(res);
  },

  async createForProject(
    projectId: number,
    payload: CreateProjectTaskPayload,
  ): Promise<ProjectTaskDto> {
    const res = await apiClient.post<ApiResponse<ProjectTaskDto>>(
      API_ENDPOINTS.projects.tasks(projectId),
      payload,
    );
    return unwrapApiResponse(res);
  },

  async getById(taskId: number, signal?: AbortSignal): Promise<ProjectTaskDto> {
    const res = await apiClient.get<ApiResponse<ProjectTaskDto>>(API_ENDPOINTS.tasks.byId(taskId), {
      signal,
    });
    return unwrapApiResponse(res);
  },

  async update(taskId: number, payload: UpdateProjectTaskPayload): Promise<ProjectTaskDto> {
    const res = await apiClient.patch<ApiResponse<ProjectTaskDto>>(
      API_ENDPOINTS.tasks.byId(taskId),
      payload,
    );
    return unwrapApiResponse(res);
  },

  async remove(taskId: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.tasks.byId(taskId));
    unwrapVoidResponse(res);
  },

  async assign(taskId: number, studentId: number): Promise<void> {
    const res = await apiClient.put(API_ENDPOINTS.tasks.assignee(taskId), { studentId });
    unwrapVoidResponse(res);
  },

  async unassign(taskId: number): Promise<void> {
    const res = await apiClient.delete(API_ENDPOINTS.tasks.assignee(taskId));
    unwrapVoidResponse(res);
  },
};
