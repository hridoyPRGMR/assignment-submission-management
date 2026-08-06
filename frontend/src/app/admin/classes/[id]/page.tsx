"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams } from "next/navigation";
import { useState } from "react";
import { classesApi } from "@/features/classes/classesApi";
import { classSubjectsApi } from "@/features/classSubjects/classSubjectsApi";
import { studentEnrollmentsApi } from "@/features/studentEnrollments/studentEnrollmentsApi";
import { subjectsApi } from "@/features/subjects/subjectsApi";
import { teacherAssignmentsApi } from "@/features/teacherAssignments/teacherAssignmentsApi";
import { usersApi } from "@/features/users/usersApi";

export default function AdminClassDetailPage() {
  const params = useParams<{ id: string }>();
  const classId = params.id;
  const queryClient = useQueryClient();

  const [selectedSubjectId, setSelectedSubjectId] = useState("");
  const [teacherSubjectId, setTeacherSubjectId] = useState("");
  const [selectedTeacherId, setSelectedTeacherId] = useState("");
  const [selectedStudentId, setSelectedStudentId] = useState("");

  const classQuery = useQuery({
    queryKey: ["classes", classId],
    queryFn: () => classesApi.getById(classId),
  });
  const allSubjectsQuery = useQuery({ queryKey: ["subjects"], queryFn: () => subjectsApi.list() });
  const classSubjectsQuery = useQuery({
    queryKey: ["class-subjects", classId],
    queryFn: () => classSubjectsApi.listByClass(classId),
  });
  const teachersQuery = useQuery({
    queryKey: ["users", "Teacher"],
    queryFn: () => usersApi.list("Teacher"),
  });
  const teacherAssignmentsQuery = useQuery({
    queryKey: ["teacher-assignments", classId],
    queryFn: () => teacherAssignmentsApi.listByClass(classId),
  });
  const studentsQuery = useQuery({
    queryKey: ["users", "Student"],
    queryFn: () => usersApi.list("Student"),
  });
  const enrollmentsQuery = useQuery({
    queryKey: ["student-enrollments", classId],
    queryFn: () => studentEnrollmentsApi.listByClass(classId),
  });

  const assignSubjectMutation = useMutation({
    mutationFn: () => classSubjectsApi.assign(classId, selectedSubjectId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["class-subjects", classId] });
      setSelectedSubjectId("");
    },
  });

  const removeSubjectMutation = useMutation({
    mutationFn: (id: string) => classSubjectsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["class-subjects", classId] }),
  });

  const assignTeacherMutation = useMutation({
    mutationFn: () => teacherAssignmentsApi.assign(selectedTeacherId, classId, teacherSubjectId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher-assignments", classId] });
      setSelectedTeacherId("");
      setTeacherSubjectId("");
    },
  });

  const removeTeacherMutation = useMutation({
    mutationFn: (id: string) => teacherAssignmentsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["teacher-assignments", classId] }),
  });

  const enrollStudentMutation = useMutation({
    mutationFn: () => studentEnrollmentsApi.enroll(selectedStudentId, classId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["student-enrollments", classId] });
      setSelectedStudentId("");
    },
  });

  const removeEnrollmentMutation = useMutation({
    mutationFn: (id: string) => studentEnrollmentsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["student-enrollments", classId] }),
  });

  const linkedSubjects = classSubjectsQuery.data ?? [];
  const availableSubjects = (allSubjectsQuery.data?.items ?? []).filter(
    (subject) => !linkedSubjects.some((cs) => cs.subjectId === subject.id),
  );

  return (
    <div className="flex flex-1 flex-col gap-8">
      <h1 className="text-2xl font-semibold">{classQuery.data?.name ?? "Class"}</h1>

      <section className="max-w-3xl space-y-3">
        <h2 className="text-lg font-medium">Subjects</h2>
        <div className="flex gap-2">
          <select
            value={selectedSubjectId}
            onChange={(e) => setSelectedSubjectId(e.target.value)}
            className="rounded-md border border-neutral-300 px-3 py-2 text-sm"
          >
            <option value="">Select a subject...</option>
            {availableSubjects.map((subject) => (
              <option key={subject.id} value={subject.id}>
                {subject.name}
              </option>
            ))}
          </select>
          <button
            disabled={!selectedSubjectId}
            onClick={() => assignSubjectMutation.mutate()}
            className="rounded-md bg-neutral-900 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            Add subject
          </button>
        </div>
        <ul className="space-y-1 text-sm">
          {linkedSubjects.map((cs) => (
            <li
              key={cs.id}
              className="flex items-center justify-between border-b border-neutral-100 py-1"
            >
              {cs.subjectName}
              <button
                onClick={() => removeSubjectMutation.mutate(cs.id)}
                className="text-red-600 underline"
              >
                Remove
              </button>
            </li>
          ))}
        </ul>
      </section>

      <section className="max-w-3xl space-y-3">
        <h2 className="text-lg font-medium">Teachers</h2>
        <div className="flex flex-wrap gap-2">
          <select
            value={teacherSubjectId}
            onChange={(e) => setTeacherSubjectId(e.target.value)}
            className="rounded-md border border-neutral-300 px-3 py-2 text-sm"
          >
            <option value="">Subject...</option>
            {linkedSubjects.map((cs) => (
              <option key={cs.subjectId} value={cs.subjectId}>
                {cs.subjectName}
              </option>
            ))}
          </select>
          <select
            value={selectedTeacherId}
            onChange={(e) => setSelectedTeacherId(e.target.value)}
            className="rounded-md border border-neutral-300 px-3 py-2 text-sm"
          >
            <option value="">Teacher...</option>
            {(teachersQuery.data?.items ?? []).map((teacher) => (
              <option key={teacher.id} value={teacher.id}>
                {teacher.fullName}
              </option>
            ))}
          </select>
          <button
            disabled={!teacherSubjectId || !selectedTeacherId}
            onClick={() => assignTeacherMutation.mutate()}
            className="rounded-md bg-neutral-900 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            Assign teacher
          </button>
        </div>
        <ul className="space-y-1 text-sm">
          {(teacherAssignmentsQuery.data ?? []).map((ta) => (
            <li
              key={ta.id}
              className="flex items-center justify-between border-b border-neutral-100 py-1"
            >
              {ta.teacherFullName} — {ta.subjectName}
              <button
                onClick={() => removeTeacherMutation.mutate(ta.id)}
                className="text-red-600 underline"
              >
                Remove
              </button>
            </li>
          ))}
        </ul>
      </section>

      <section className="max-w-3xl space-y-3">
        <h2 className="text-lg font-medium">Students</h2>
        <div className="flex gap-2">
          <select
            value={selectedStudentId}
            onChange={(e) => setSelectedStudentId(e.target.value)}
            className="rounded-md border border-neutral-300 px-3 py-2 text-sm"
          >
            <option value="">Select a student...</option>
            {(studentsQuery.data?.items ?? []).map((student) => (
              <option key={student.id} value={student.id}>
                {student.fullName}
              </option>
            ))}
          </select>
          <button
            disabled={!selectedStudentId}
            onClick={() => enrollStudentMutation.mutate()}
            className="rounded-md bg-neutral-900 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            Enroll student
          </button>
        </div>
        <ul className="space-y-1 text-sm">
          {(enrollmentsQuery.data ?? []).map((enrollment) => (
            <li
              key={enrollment.id}
              className="flex items-center justify-between border-b border-neutral-100 py-1"
            >
              {enrollment.studentFullName}
              <button
                onClick={() => removeEnrollmentMutation.mutate(enrollment.id)}
                className="text-red-600 underline"
              >
                Remove
              </button>
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}
