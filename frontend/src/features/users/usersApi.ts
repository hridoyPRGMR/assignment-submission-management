import { apiClient } from "@/lib/apiClient";
import type { AdminUser, PagedResult } from "@/types/admin";

export const usersApi = {
  async list(role?: string): Promise<PagedResult<AdminUser>> {
    const response = await apiClient.get<PagedResult<AdminUser>>("/users", {
      params: { page: 1, pageSize: 100, role: role || undefined },
    });
    return response.data;
  },

  async listPaged(page: number, pageSize: number, role?: string): Promise<PagedResult<AdminUser>> {
    const response = await apiClient.get<PagedResult<AdminUser>>("/users", {
      params: { page, pageSize, role: role || undefined },
    });
    return response.data;
  },

  async create(input: {
    email: string;
    password: string;
    fullName: string;
    role: string;
  }): Promise<AdminUser> {
    const response = await apiClient.post<AdminUser>("/users", input);
    return response.data;
  },

  async setActiveStatus(id: string, isActive: boolean): Promise<void> {
    await apiClient.post(`/users/${id}/${isActive ? "activate" : "deactivate"}`);
  },
};
