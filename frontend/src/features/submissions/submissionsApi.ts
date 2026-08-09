import { apiClient } from "@/lib/apiClient";
import type { Submission } from "@/types/submission";

export const ALLOWED_SUBMISSION_FILE_EXTENSIONS = [".pdf", ".docx", ".zip"];
export const MAX_SUBMISSION_FILE_SIZE_BYTES = 20 * 1024 * 1024;

function buildSubmissionForm(content: string, file?: File | null): FormData {
  const form = new FormData();
  form.append("content", content);
  if (file) {
    form.append("file", file);
  }
  return form;
}

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

  async create(input: { assignmentId: string; content: string; file?: File | null }): Promise<Submission> {
    const form = buildSubmissionForm(input.content, input.file);
    form.append("assignmentId", input.assignmentId);
    const response = await apiClient.post<Submission>("/submissions", form);
    return response.data;
  },

  async update(id: string, input: { content: string; file?: File | null }): Promise<Submission> {
    const form = buildSubmissionForm(input.content, input.file);
    const response = await apiClient.put<Submission>(`/submissions/${id}`, form);
    return response.data;
  },

  async downloadFile(fileDownloadUrl: string, fileName: string): Promise<void> {
    const url = fileDownloadUrl.replace(/^\/api\/v1/, "");
    const response = await apiClient.get(url, { responseType: "blob" });
    const objectUrl = URL.createObjectURL(response.data as Blob);
    const link = document.createElement("a");
    link.href = objectUrl;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.setTimeout(() => URL.revokeObjectURL(objectUrl), 1000);
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
