export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  role: "Admin" | "Teacher" | "Student";
  isActive: boolean;
  createdAtUtc: string;
}

export interface SchoolClass {
  id: string;
  name: string;
  description: string | null;
  createdAtUtc: string;
}

export interface Subject {
  id: string;
  name: string;
  description: string | null;
  createdAtUtc: string;
}

export interface ClassSubject {
  id: string;
  classId: string;
  className: string;
  subjectId: string;
  subjectName: string;
}

export interface TeacherAssignment {
  id: string;
  teacherUserId: string;
  teacherFullName: string;
  classId: string;
  className: string;
  subjectId: string;
  subjectName: string;
}

export interface StudentEnrollment {
  id: string;
  studentUserId: string;
  studentFullName: string;
  classId: string;
  className: string;
}
