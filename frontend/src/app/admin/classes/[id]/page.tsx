"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import { ArrowLeft, BookOpen, GraduationCap, Loader2, Plus, Trash2, Users } from "lucide-react";
import { classesApi } from "@/features/classes/classesApi";
import { classSubjectsApi } from "@/features/classSubjects/classSubjectsApi";
import { studentEnrollmentsApi } from "@/features/studentEnrollments/studentEnrollmentsApi";
import { subjectsApi } from "@/features/subjects/subjectsApi";
import { teacherAssignmentsApi } from "@/features/teacherAssignments/teacherAssignmentsApi";
import { usersApi } from "@/features/users/usersApi";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { toast } from "@/components/ui/toast";

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
  const allSubjectsQuery = useQuery({
    queryKey: ["subjects"],
    queryFn: () => subjectsApi.list(1, 100),
  });
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
      toast.add({ type: "success", title: "Subject assigned" });
    },
    onError: () => toast.add({ type: "error", title: "Could not assign subject" }),
  });

  const removeSubjectMutation = useMutation({
    mutationFn: (id: string) => classSubjectsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["class-subjects", classId] });
      toast.add({ type: "success", title: "Subject removed" });
    },
    onError: () => toast.add({ type: "error", title: "Could not remove subject" }),
  });

  const assignTeacherMutation = useMutation({
    mutationFn: () => teacherAssignmentsApi.assign(selectedTeacherId, classId, teacherSubjectId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher-assignments", classId] });
      setSelectedTeacherId("");
      setTeacherSubjectId("");
      toast.add({ type: "success", title: "Teacher assigned" });
    },
    onError: () => toast.add({ type: "error", title: "Could not assign teacher" }),
  });

  const removeTeacherMutation = useMutation({
    mutationFn: (id: string) => teacherAssignmentsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teacher-assignments", classId] });
      toast.add({ type: "success", title: "Teacher removed" });
    },
    onError: () => toast.add({ type: "error", title: "Could not remove teacher" }),
  });

  const enrollStudentMutation = useMutation({
    mutationFn: () => studentEnrollmentsApi.enroll(selectedStudentId, classId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["student-enrollments", classId] });
      setSelectedStudentId("");
      toast.add({ type: "success", title: "Student enrolled" });
    },
    onError: () => toast.add({ type: "error", title: "Could not enroll student" }),
  });

  const removeEnrollmentMutation = useMutation({
    mutationFn: (id: string) => studentEnrollmentsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["student-enrollments", classId] });
      toast.add({ type: "success", title: "Student removed" });
    },
    onError: () => toast.add({ type: "error", title: "Could not remove student" }),
  });

  const linkedSubjects = classSubjectsQuery.data ?? [];
  const availableSubjects = (allSubjectsQuery.data?.items ?? []).filter(
    (subject) => !linkedSubjects.some((cs) => cs.subjectId === subject.id),
  );
  const teacherAssignments = teacherAssignmentsQuery.data ?? [];
  const enrollments = enrollmentsQuery.data ?? [];

  const overview = [
    { label: "Subjects", value: linkedSubjects.length, icon: BookOpen },
    { label: "Teachers", value: teacherAssignments.length, icon: GraduationCap },
    { label: "Students", value: enrollments.length, icon: Users },
  ];

  if (classQuery.isLoading) {
    return (
      <div className="flex flex-1 flex-col items-center justify-center gap-2 py-24">
        <Loader2 className="size-5 animate-spin text-muted-foreground" />
        <p className="text-sm text-muted-foreground">Loading class...</p>
      </div>
    );
  }

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-4">
        <Button
          variant="ghost"
          size="sm"
          className="-ml-2 w-fit"
          render={<Link href="/admin/classes" />}
        >
          <ArrowLeft />
          Back to Classes
        </Button>

        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold tracking-tight">
            {classQuery.data?.name ?? "Class"}
          </h1>
          <p className="text-sm text-muted-foreground">
            {classQuery.data?.description ||
              "Manage the class curriculum, teachers, and student enrollments."}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        {overview.map(({ label, value, icon: Icon }) => (
          <Card key={label}>
            <CardContent className="flex items-center justify-between">
              <div className="flex flex-col gap-1">
                <p className="text-sm text-muted-foreground">{label}</p>
                <p className="text-2xl font-semibold tracking-tight">{value}</p>
              </div>
              <Icon className="size-5 text-muted-foreground" />
            </CardContent>
          </Card>
        ))}
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Subjects</CardTitle>
          <CardDescription>Subjects taught in this class.</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
            <Field className="flex-1">
              <Label>Subject</Label>
              <Select
                value={selectedSubjectId}
                onValueChange={(value) => setSelectedSubjectId(value ?? "")}
                items={availableSubjects.map((subject) => ({
                  label: subject.name,
                  value: subject.id,
                }))}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Select a subject..." />
                </SelectTrigger>
                <SelectContent>
                  {availableSubjects.length === 0 ? (
                    <SelectItem value="__none" disabled>
                      No subjects available
                    </SelectItem>
                  ) : (
                    availableSubjects.map((subject) => (
                      <SelectItem key={subject.id} value={subject.id}>
                        {subject.name}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </Field>
            <Button
              className="w-full sm:w-auto"
              disabled={!selectedSubjectId || assignSubjectMutation.isPending}
              onClick={() => assignSubjectMutation.mutate()}
            >
              <Plus />
              {assignSubjectMutation.isPending ? "Adding..." : "Add Subject"}
            </Button>
          </div>

          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead className="text-right">Action</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {classSubjectsQuery.isLoading ? (
                <TableRow>
                  <TableCell colSpan={2} className="h-16 text-center text-muted-foreground">
                    Loading subjects...
                  </TableCell>
                </TableRow>
              ) : linkedSubjects.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={2} className="h-16 text-center text-muted-foreground">
                    No subjects assigned yet.
                  </TableCell>
                </TableRow>
              ) : (
                linkedSubjects.map((cs) => (
                  <TableRow key={cs.id}>
                    <TableCell className="font-medium">{cs.subjectName}</TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        className="hover:bg-destructive/10 hover:text-destructive"
                        disabled={removeSubjectMutation.isPending}
                        onClick={() => removeSubjectMutation.mutate(cs.id)}
                        aria-label={`Remove ${cs.subjectName}`}
                      >
                        <Trash2 />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Teachers</CardTitle>
          <CardDescription>Assign teachers to subjects in this class.</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-[1fr_1fr_auto] sm:items-end">
            <Field>
              <Label>Subject</Label>
              <Select
                value={teacherSubjectId}
                onValueChange={(value) => setTeacherSubjectId(value ?? "")}
                items={linkedSubjects.map((cs) => ({
                  label: cs.subjectName,
                  value: cs.subjectId,
                }))}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Select a subject..." />
                </SelectTrigger>
                <SelectContent>
                  {linkedSubjects.length === 0 ? (
                    <SelectItem value="__none" disabled>
                      No linked subjects
                    </SelectItem>
                  ) : (
                    linkedSubjects.map((cs) => (
                      <SelectItem key={cs.subjectId} value={cs.subjectId}>
                        {cs.subjectName}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </Field>
            <Field>
              <Label>Teacher</Label>
              <Select
                value={selectedTeacherId}
                onValueChange={(value) => setSelectedTeacherId(value ?? "")}
                items={(teachersQuery.data?.items ?? []).map((teacher) => ({
                  label: teacher.fullName,
                  value: teacher.id,
                }))}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Select a teacher..." />
                </SelectTrigger>
                <SelectContent>
                  {(teachersQuery.data?.items ?? []).length === 0 ? (
                    <SelectItem value="__none" disabled>
                      No teachers available
                    </SelectItem>
                  ) : (
                    (teachersQuery.data?.items ?? []).map((teacher) => (
                      <SelectItem key={teacher.id} value={teacher.id}>
                        {teacher.fullName}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </Field>
            <Button
              className="w-full sm:w-auto"
              disabled={!teacherSubjectId || !selectedTeacherId || assignTeacherMutation.isPending}
              onClick={() => assignTeacherMutation.mutate()}
            >
              <Plus />
              {assignTeacherMutation.isPending ? "Assigning..." : "Assign Teacher"}
            </Button>
          </div>

          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Teacher</TableHead>
                <TableHead>Subject</TableHead>
                <TableHead className="text-right">Action</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {teacherAssignmentsQuery.isLoading ? (
                <TableRow>
                  <TableCell colSpan={3} className="h-16 text-center text-muted-foreground">
                    Loading teachers...
                  </TableCell>
                </TableRow>
              ) : teacherAssignments.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={3} className="h-16 text-center text-muted-foreground">
                    No teachers assigned yet.
                  </TableCell>
                </TableRow>
              ) : (
                teacherAssignments.map((ta) => (
                  <TableRow key={ta.id}>
                    <TableCell className="font-medium">{ta.teacherFullName}</TableCell>
                    <TableCell className="text-muted-foreground">{ta.subjectName}</TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        className="hover:bg-destructive/10 hover:text-destructive"
                        disabled={removeTeacherMutation.isPending}
                        onClick={() => removeTeacherMutation.mutate(ta.id)}
                        aria-label={`Remove ${ta.teacherFullName}`}
                      >
                        <Trash2 />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Students</CardTitle>
          <CardDescription>Students enrolled in this class.</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
            <Field className="flex-1">
              <Label>Student</Label>
              <Select
                value={selectedStudentId}
                onValueChange={(value) => setSelectedStudentId(value ?? "")}
                items={(studentsQuery.data?.items ?? []).map((student) => ({
                  label: student.fullName,
                  value: student.id,
                }))}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Select a student..." />
                </SelectTrigger>
                <SelectContent>
                  {(studentsQuery.data?.items ?? []).length === 0 ? (
                    <SelectItem value="__none" disabled>
                      No students available
                    </SelectItem>
                  ) : (
                    (studentsQuery.data?.items ?? []).map((student) => (
                      <SelectItem key={student.id} value={student.id}>
                        {student.fullName}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
            </Field>
            <Button
              className="w-full sm:w-auto"
              disabled={!selectedStudentId || enrollStudentMutation.isPending}
              onClick={() => enrollStudentMutation.mutate()}
            >
              <Plus />
              {enrollStudentMutation.isPending ? "Enrolling..." : "Enroll Student"}
            </Button>
          </div>

          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Student</TableHead>
                <TableHead className="text-right">Action</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {enrollmentsQuery.isLoading ? (
                <TableRow>
                  <TableCell colSpan={2} className="h-16 text-center text-muted-foreground">
                    Loading students...
                  </TableCell>
                </TableRow>
              ) : enrollments.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={2} className="h-16 text-center text-muted-foreground">
                    No students enrolled yet.
                  </TableCell>
                </TableRow>
              ) : (
                enrollments.map((enrollment) => (
                  <TableRow key={enrollment.id}>
                    <TableCell className="font-medium">{enrollment.studentFullName}</TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        className="hover:bg-destructive/10 hover:text-destructive"
                        disabled={removeEnrollmentMutation.isPending}
                        onClick={() => removeEnrollmentMutation.mutate(enrollment.id)}
                        aria-label={`Remove ${enrollment.studentFullName}`}
                      >
                        <Trash2 />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
