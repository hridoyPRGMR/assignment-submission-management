import { apiClient } from "@/lib/apiClient";
import type { AdminDashboard, StudentDashboard, TeacherDashboard } from "@/types/dashboard";

export const dashboardApi = {
  async getAdmin(): Promise<AdminDashboard> {
    const response = await apiClient.get<AdminDashboard>("/dashboard/admin");
    return response.data;
  },

  async getTeacher(): Promise<TeacherDashboard> {
    const response = await apiClient.get<TeacherDashboard>("/dashboard/teacher");
    return response.data;
  },

  async getStudent(): Promise<StudentDashboard> {
    const response = await apiClient.get<StudentDashboard>("/dashboard/student");
    return response.data;
  },
};
