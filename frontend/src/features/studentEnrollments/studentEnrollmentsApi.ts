import { apiClient } from "@/lib/apiClient";
import type { StudentEnrollment } from "@/types/admin";

export const studentEnrollmentsApi = {
  async listByClass(classId: string): Promise<StudentEnrollment[]> {
    const response = await apiClient.get<StudentEnrollment[]>("/student-enrollments", {
      params: { classId },
    });
    return response.data;
  },

  async enroll(studentUserId: string, classId: string): Promise<StudentEnrollment> {
    const response = await apiClient.post<StudentEnrollment>("/student-enrollments", {
      studentUserId,
      classId,
    });
    return response.data;
  },

  async remove(id: string): Promise<void> {
    await apiClient.delete(`/student-enrollments/${id}`);
  },
};
