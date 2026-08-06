export type SubmissionStatus = "Submitted" | "UnderReview" | "Graded" | "ReturnedForRevision";

export interface Submission {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  assignmentMaxMarks: number;
  studentUserId: string;
  studentFullName: string;
  content: string;
  fileUrl: string | null;
  submittedAtUtc: string;
  isLate: boolean;
  status: SubmissionStatus;
  marks: number | null;
  feedback: string | null;
  gradedAtUtc: string | null;
  gradedByUserId: string | null;
}
