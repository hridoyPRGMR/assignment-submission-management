import { apiClient } from "@/lib/apiClient";
import type { ClassSubject } from "@/types/admin";

export const classSubjectsApi = {
  async listByClass(classId: string): Promise<ClassSubject[]> {
    const response = await apiClient.get<ClassSubject[]>("/class-subjects", {
      params: { classId },
    });
    return response.data;
  },

  async assign(classId: string, subjectId: string): Promise<ClassSubject> {
    const response = await apiClient.post<ClassSubject>("/class-subjects", { classId, subjectId });
    return response.data;
  },

  async remove(id: string): Promise<void> {
    await apiClient.delete(`/class-subjects/${id}`);
  },
};
