import { apiClient } from "@/lib/apiClient";
import type { PagedResult } from "@/types/admin";
import type { Assignment } from "@/types/assignment";

export const assignmentsApi = {
  async listMine(): Promise<PagedResult<Assignment>> {
    const response = await apiClient.get<PagedResult<Assignment>>("/assignments", {
      params: { page: 1, pageSize: 100 },
    });
    return response.data;
  },

  async listPublishedForStudent(): Promise<PagedResult<Assignment>> {
    const response = await apiClient.get<PagedResult<Assignment>>("/assignments/student", {
      params: { page: 1, pageSize: 100 },
    });
    return response.data;
  },

  async getById(id: string): Promise<Assignment> {
    const response = await apiClient.get<Assignment>(`/assignments/${id}`);
    return response.data;
  },

  async create(input: {
    title: string;
    description: string;
    classId: string;
    subjectId: string;
    deadlineUtc: string;
    maxMarks: number;
  }): Promise<Assignment> {
    const response = await apiClient.post<Assignment>("/assignments", input);
    return response.data;
  },

  async update(
    id: string,
    input: { title: string; description: string; deadlineUtc: string; maxMarks: number },
  ): Promise<Assignment> {
    const response = await apiClient.put<Assignment>(`/assignments/${id}`, input);
    return response.data;
  },

  async publish(id: string): Promise<Assignment> {
    const response = await apiClient.post<Assignment>(`/assignments/${id}/publish`);
    return response.data;
  },

  async unpublish(id: string): Promise<Assignment> {
    const response = await apiClient.post<Assignment>(`/assignments/${id}/unpublish`);
    return response.data;
  },

  async markGraded(id: string): Promise<Assignment> {
    const response = await apiClient.post<Assignment>(`/assignments/${id}/mark-graded`);
    return response.data;
  },

  async archive(id: string): Promise<Assignment> {
    const response = await apiClient.post<Assignment>(`/assignments/${id}/archive`);
    return response.data;
  },

  async remove(id: string): Promise<void> {
    await apiClient.delete(`/assignments/${id}`);
  },
};
