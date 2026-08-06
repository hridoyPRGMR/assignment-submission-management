import { apiClient } from "@/lib/apiClient";
import type { Submission } from "@/types/submission";

export const submissionsApi = {
  async listMine(): Promise<Submission[]> {
    const response = await apiClient.get<Submission[]>("/submissions");
    return response.data;
  },

  async getForAssignment(assignmentId: string): Promise<Submission | null> {
    const response = await apiClient.get<Submission | null>(
      `/submissions/for-assignment/${assignmentId}`,
      {
        validateStatus: (status) => status === 200 || status === 204,
      },
    );
    return response.status === 204 ? null : response.data;
  },

  async create(input: {
    assignmentId: string;
    content: string;
    fileUrl?: string;
  }): Promise<Submission> {
    const response = await apiClient.post<Submission>("/submissions", input);
    return response.data;
  },

  async update(id: string, input: { content: string; fileUrl?: string }): Promise<Submission> {
    const response = await apiClient.put<Submission>(`/submissions/${id}`, input);
    return response.data;
  },

  async listForAssignment(assignmentId: string): Promise<Submission[]> {
    const response = await apiClient.get<Submission[]>(`/assignments/${assignmentId}/submissions`);
    return response.data;
  },

  async startReview(id: string): Promise<Submission> {
    const response = await apiClient.post<Submission>(`/submissions/${id}/start-review`);
    return response.data;
  },

  async grade(id: string, marks: number, feedback: string): Promise<Submission> {
    const response = await apiClient.post<Submission>(`/submissions/${id}/grade`, {
      marks,
      feedback,
    });
    return response.data;
  },

  async returnForRevision(id: string, feedback: string): Promise<Submission> {
    const response = await apiClient.post<Submission>(`/submissions/${id}/return-for-revision`, {
      feedback,
    });
    return response.data;
  },
};
