export type AssignmentStatus = "Draft" | "Published";

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
  createdAtUtc: string;
  updatedAtUtc: string;
}
