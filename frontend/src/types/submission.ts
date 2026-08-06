export type SubmissionStatus = "Submitted" | "LateSubmission" | "UnderReview" | "Graded" | "Returned";

export interface Submission {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  assignmentMaxMarks: number;
  studentUserId: string;
  studentFullName: string;
  content: string;
  fileName: string | null;
  fileContentType: string | null;
  fileSizeBytes: number | null;
  fileDownloadUrl: string | null;
  submittedAtUtc: string;
  isLate: boolean;
  status: SubmissionStatus;
  marks: number | null;
  feedback: string | null;
  gradedAtUtc: string | null;
  gradedByUserId: string | null;
}
