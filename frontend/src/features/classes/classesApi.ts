import { apiClient } from "@/lib/apiClient";
import type { PagedResult, SchoolClass } from "@/types/admin";

export const classesApi = {
  async list(): Promise<PagedResult<SchoolClass>> {
    const response = await apiClient.get<PagedResult<SchoolClass>>("/classes", {
      params: { page: 1, pageSize: 100 },
    });
    return response.data;
  },

  async getById(id: string): Promise<SchoolClass> {
    const response = await apiClient.get<SchoolClass>(`/classes/${id}`);
    return response.data;
  },

  async create(input: { name: string; description?: string }): Promise<SchoolClass> {
    const response = await apiClient.post<SchoolClass>("/classes", input);
    return response.data;
  },

  async update(id: string, input: { name: string; description?: string }): Promise<SchoolClass> {
    const response = await apiClient.put<SchoolClass>(`/classes/${id}`, input);
    return response.data;
  },

  async remove(id: string): Promise<void> {
    await apiClient.delete(`/classes/${id}`);
  },
};
