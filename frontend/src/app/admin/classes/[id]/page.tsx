"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import { ArrowLeft, Check, Loader2, Plus, Trash2 } from "lucide-react";
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
import { Separator } from "@/components/ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { toast } from "@/components/ui/toast";
import { cn } from "@/lib/utils";

type TabKey = "subjects" | "teachers" | "students";

function EmptyState({ message, hint }: { message: string; hint: string }) {
  return (
    <div className="flex flex-col items-center justify-center gap-1 rounded-lg border border-dashed px-4 py-12 text-center">
      <p className="text-sm font-medium text-foreground">{message}</p>
      <p className="text-sm text-muted-foreground">{hint}</p>
    </div>
  );
}

interface SetupStep {
  key: TabKey;
  label: string;
  hint: string;
  count: number;
}

function SetupGuide({
  steps,
  active,
  onStepClick,
}: {
  steps: SetupStep[];
  active: TabKey;
  onStepClick: (tab: TabKey) => void;
}) {
  const doneCount = steps.filter((step) => step.count > 0).length;
  const progress = Math.round((doneCount / steps.length) * 100);

  return (
    <div className="flex flex-col gap-4">
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
        {steps.map((step, index) => {
          const isDone = step.count > 0;
          const isActive = active === step.key;
          return (
            <button
              key={step.key}
              type="button"
              onClick={() => onStepClick(step.key)}
              aria-current={isActive ? "step" : undefined}
              className={cn(
                "flex items-center gap-3 rounded-xl border bg-card p-3 text-left transition-colors hover:bg-accent",
                isActive ? "border-primary ring-2 ring-primary" : "border-border",
              )}
            >
              <span
                className={cn(
                  "flex size-8 shrink-0 items-center justify-center rounded-full text-sm font-semibold",
                  isDone
                    ? "bg-primary text-primary-foreground"
                    : "bg-muted text-muted-foreground",
                )}
              >
                {isDone ? <Check className="size-4" /> : index + 1}
              </span>
              <span className="flex flex-col gap-0.5">
                <span className="text-sm font-medium">{step.label}</span>
                <span className="text-xs text-muted-foreground">
                  {step.count > 0
                    ? `${step.count} ${step.key === "subjects" ? "added" : step.key === "teachers" ? "assigned" : "enrolled"}`
                    : step.hint}
                </span>
              </span>
            </button>
          );
        })}
      </div>

      <div className="flex flex-col gap-1.5">
        <div
          role="progressbar"
          aria-valuemin={0}
          aria-valuemax={steps.length}
          aria-valuenow={doneCount}
          className="h-1.5 w-full overflow-hidden rounded-full bg-muted"
        >
          <div
            className="h-full rounded-full bg-primary transition-all"
            style={{ width: `${progress}%` }}
          />
        </div>
        <p className="text-xs text-muted-foreground">
          {doneCount === steps.length
            ? "Setup complete. You can add or remove subjects, teachers, and students anytime."
            : `Next: ${steps[doneCount].label} — ${steps[doneCount].hint}`}
        </p>
      </div>
    </div>
  );
}

export default function AdminClassDetailPage() {
  const params = useParams<{ id: string }>();
  const classId = params.id;
  const queryClient = useQueryClient();

  const [activeTab, setActiveTab] = useState<TabKey>("subjects");
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
  const allTeachers = teachersQuery.data?.items ?? [];
  const allStudents = studentsQuery.data?.items ?? [];
  const teacherAssignments = teacherAssignmentsQuery.data ?? [];
  const enrollments = enrollmentsQuery.data ?? [];

  const setupSteps: SetupStep[] = [
    {
      key: "subjects",
      label: "Add subjects",
      hint: "Add the subjects taught in this class.",
      count: linkedSubjects.length,
    },
    {
      key: "teachers",
      label: "Assign teachers",
      hint: "Assign a teacher to each subject.",
      count: teacherAssignments.length,
    },
    {
      key: "students",
      label: "Enroll students",
      hint: "Enroll students in this class.",
      count: enrollments.length,
    },
  ];

  const tabDescriptions: Record<TabKey, string> = {
    subjects: "Add the subjects this class covers. Teachers are assigned to these subjects in the next step.",
    teachers: "Assign a teacher to each subject added in step 1. Teachers manage assignments for their subjects.",
    students: "Enroll students in this class so they can view assignments and submit work.",
  };

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
          nativeButton={false}
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
              "Set up this class by adding subjects, assigning teachers, and enrolling students."}
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Class setup</CardTitle>
          <CardDescription>
            Complete the steps below to get this class ready. Each step unlocks the next.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <SetupGuide
            steps={setupSteps}
            active={activeTab}
            onStepClick={setActiveTab}
          />

          <Separator className="my-6" />

          <Tabs
            value={activeTab}
            onValueChange={(value) => setActiveTab((value as TabKey) ?? "subjects")}
          >
            <div className="flex flex-col gap-1">
              <TabsList>
                <TabsTrigger value="subjects">1. Subjects ({linkedSubjects.length})</TabsTrigger>
                <TabsTrigger value="teachers">2. Teachers ({teacherAssignments.length})</TabsTrigger>
                <TabsTrigger value="students">3. Students ({enrollments.length})</TabsTrigger>
              </TabsList>
              <p className="text-sm text-muted-foreground">{tabDescriptions[activeTab]}</p>
            </div>

            <TabsContent value="subjects" className="mt-4 flex flex-col gap-4">
              <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
                <Field className="flex-1">
                  <Label>Add subject</Label>
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

              {classSubjectsQuery.isLoading ? (
                <div className="flex items-center gap-2 py-8 text-sm text-muted-foreground">
                  <Loader2 className="size-4 animate-spin" />
                  Loading subjects...
                </div>
              ) : linkedSubjects.length === 0 ? (
                <EmptyState
                  message="No subjects assigned yet"
                  hint="Add subjects to define what this class teaches. Teachers are assigned to subjects in the next step."
                />
              ) : (
                <div className="overflow-hidden rounded-lg border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Name</TableHead>
                        <TableHead className="text-right">Action</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {linkedSubjects.map((cs) => (
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
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
            </TabsContent>

            <TabsContent value="teachers" className="mt-4 flex flex-col gap-4">
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
                    items={allTeachers.map((teacher) => ({
                      label: teacher.fullName,
                      value: teacher.id,
                    }))}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Select a teacher..." />
                    </SelectTrigger>
                    <SelectContent>
                      {allTeachers.length === 0 ? (
                        <SelectItem value="__none" disabled>
                          No teachers available
                        </SelectItem>
                      ) : (
                        allTeachers.map((teacher) => (
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
                  disabled={
                    !teacherSubjectId || !selectedTeacherId || assignTeacherMutation.isPending
                  }
                  onClick={() => assignTeacherMutation.mutate()}
                >
                  <Plus />
                  {assignTeacherMutation.isPending ? "Assigning..." : "Assign Teacher"}
                </Button>
              </div>

              {teacherAssignmentsQuery.isLoading ? (
                <div className="flex items-center gap-2 py-8 text-sm text-muted-foreground">
                  <Loader2 className="size-4 animate-spin" />
                  Loading teachers...
                </div>
              ) : teacherAssignments.length === 0 ? (
                <EmptyState
                  message="No teachers assigned yet"
                  hint="Assign a teacher to each subject you added in step 1. Teachers manage assignments for their subjects."
                />
              ) : (
                <div className="overflow-hidden rounded-lg border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Teacher</TableHead>
                        <TableHead>Subject</TableHead>
                        <TableHead className="text-right">Action</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {teacherAssignments.map((ta) => (
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
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
            </TabsContent>

            <TabsContent value="students" className="mt-4 flex flex-col gap-4">
              <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
                <Field className="flex-1">
                  <Label>Enroll student</Label>
                  <Select
                    value={selectedStudentId}
                    onValueChange={(value) => setSelectedStudentId(value ?? "")}
                    items={allStudents.map((student) => ({
                      label: student.fullName,
                      value: student.id,
                    }))}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Select a student..." />
                    </SelectTrigger>
                    <SelectContent>
                      {allStudents.length === 0 ? (
                        <SelectItem value="__none" disabled>
                          No students available
                        </SelectItem>
                      ) : (
                        allStudents.map((student) => (
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

              {enrollmentsQuery.isLoading ? (
                <div className="flex items-center gap-2 py-8 text-sm text-muted-foreground">
                  <Loader2 className="size-4 animate-spin" />
                  Loading students...
                </div>
              ) : enrollments.length === 0 ? (
                <EmptyState
                  message="No students enrolled yet"
                  hint="Enroll students in this class so they can view assignments and submit work."
                />
              ) : (
                <div className="overflow-hidden rounded-lg border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Student</TableHead>
                        <TableHead className="text-right">Action</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {enrollments.map((enrollment) => (
                        <TableRow key={enrollment.id}>
                          <TableCell className="font-medium">
                            {enrollment.studentFullName}
                          </TableCell>
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
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>
    </div>
  );
}
