export type AssignmentStatus = "Draft" | "Published" | "Graded" | "Archived";

export type AssignmentLifecycleStatus =
  | "Draft"
  | "Published"
  | "SubmissionOpen"
  | "DeadlinePassed"
  | "Graded"
  | "Archived";

export interface Assignment {
  id: string;
  title: string;
  description: string;
  classId: string;
  className: string;
  subjectId: string;
  subjectName: string;
  teacherUserId: string;
  teacherFullName: string;
  deadlineUtc: string;
  maxMarks: number;
  status: AssignmentStatus;
  lifecycleStatus: AssignmentLifecycleStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
}
