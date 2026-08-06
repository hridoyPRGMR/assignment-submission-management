import { apiClient } from "@/lib/apiClient";
import type { TeacherAssignment } from "@/types/admin";

export const teacherAssignmentsApi = {
  async listByClass(classId: string): Promise<TeacherAssignment[]> {
    const response = await apiClient.get<TeacherAssignment[]>("/teacher-assignments", {
      params: { classId },
    });
    return response.data;
  },

  async listMine(): Promise<TeacherAssignment[]> {
    const response = await apiClient.get<TeacherAssignment[]>("/teacher-assignments");
    return response.data;
  },

  async assign(
    teacherUserId: string,
    classId: string,
    subjectId: string,
  ): Promise<TeacherAssignment> {
    const response = await apiClient.post<TeacherAssignment>("/teacher-assignments", {
      teacherUserId,
      classId,
      subjectId,
    });
    return response.data;
  },

  async remove(id: string): Promise<void> {
    await apiClient.delete(`/teacher-assignments/${id}`);
  },
};
