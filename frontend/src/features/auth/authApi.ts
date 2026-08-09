import { apiClient } from "@/lib/apiClient";
import type { AuthResponse } from "@/types/auth";

export const authApi = {
  async login(email: string, password: string): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>("/auth/login", { email, password });
    return response.data;
  },

  async refresh(): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>("/auth/refresh");
    return response.data;
  },

  async logout(): Promise<void> {
    await apiClient.post("/auth/logout");
  },

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    await apiClient.post("/auth/change-password", { currentPassword, newPassword });
  },
};
