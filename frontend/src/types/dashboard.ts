export interface AdminDashboard {
  totalUsers: number;
  totalTeachers: number;
  totalStudents: number;
  totalClasses: number;
  totalSubjects: number;
  totalAssignments: number;
  publishedAssignments: number;
  draftAssignments: number;
  totalSubmissions: number;
  gradedSubmissions: number;
}

export interface TeacherDashboard {
  totalAssignments: number;
  draftAssignments: number;
  publishedAssignments: number;
  totalSubmissions: number;
  pendingToGradeCount: number;
}

export interface StudentDashboard {
  totalPublishedAssignments: number;
  notSubmittedCount: number;
  submittedCount: number;
  gradedCount: number;
}
