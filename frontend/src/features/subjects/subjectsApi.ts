import { apiClient } from "@/lib/apiClient";
import type { PagedResult, Subject } from "@/types/admin";

export const subjectsApi = {
  async list(): Promise<PagedResult<Subject>> {
    const response = await apiClient.get<PagedResult<Subject>>("/subjects", {
      params: { page: 1, pageSize: 100 },
    });
    return response.data;
  },

  async create(input: { name: string; description?: string }): Promise<Subject> {
    const response = await apiClient.post<Subject>("/subjects", input);
    return response.data;
  },

  async update(id: string, input: { name: string; description?: string }): Promise<Subject> {
    const response = await apiClient.put<Subject>(`/subjects/${id}`, input);
    return response.data;
  },

  async remove(id: string): Promise<void> {
    await apiClient.delete(`/subjects/${id}`);
  },
};
