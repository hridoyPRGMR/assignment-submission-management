"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { teacherAssignmentsApi } from "@/features/teacherAssignments/teacherAssignmentsApi";

const assignmentSchema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().min(1, "Description is required"),
  classSubject: z.string().min(1, "Select a class and subject"),
  deadline: z.string().min(1, "Deadline is required"),
  maxMarks: z.coerce.number().int().positive("Must be greater than zero"),
});

type AssignmentFormValues = z.input<typeof assignmentSchema>;
type AssignmentFormOutput = z.output<typeof assignmentSchema>;

export default function TeacherAssignmentsPage() {
  const queryClient = useQueryClient();

  const assignmentsQuery = useQuery({
    queryKey: ["assignments", "mine"],
    queryFn: () => assignmentsApi.listMine(),
  });
  const teachingQuery = useQuery({
    queryKey: ["teacher-assignments", "mine"],
    queryFn: () => teacherAssignmentsApi.listMine(),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<AssignmentFormValues, unknown, AssignmentFormOutput>({
    resolver: zodResolver(assignmentSchema),
  });

  const createMutation = useMutation({
    mutationFn: (values: AssignmentFormOutput) => {
      const [classId, subjectId] = values.classSubject.split("|");
      return assignmentsApi.create({
        title: values.title,
        description: values.description,
        classId,
        subjectId,
        deadlineUtc: new Date(values.deadline).toISOString(),
        maxMarks: values.maxMarks,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] });
      reset({ title: "", description: "", classSubject: "", deadline: "", maxMarks: 100 });
    },
  });

  const publishMutation = useMutation({
    mutationFn: (id: string) => assignmentsApi.publish(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] }),
  });

  const unpublishMutation = useMutation({
    mutationFn: (id: string) => assignmentsApi.unpublish(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => assignmentsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] }),
  });

  return (
    <div className="flex flex-1 flex-col gap-8">
      <h1 className="text-2xl font-semibold">Assignments</h1>

      <form
        onSubmit={handleSubmit((values) => createMutation.mutate(values))}
        className="grid max-w-2xl grid-cols-1 gap-4 rounded-md border border-neutral-200 p-4 sm:grid-cols-2"
      >
        <div className="sm:col-span-2">
          <label className="block text-sm font-medium">Title</label>
          <input
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
            {...register("title")}
          />
          {errors.title && <p className="mt-1 text-sm text-red-600">{errors.title.message}</p>}
        </div>

        <div className="sm:col-span-2">
          <label className="block text-sm font-medium">Description</label>
          <textarea
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
            rows={3}
            {...register("description")}
          />
          {errors.description && (
            <p className="mt-1 text-sm text-red-600">{errors.description.message}</p>
          )}
        </div>

        <div className="sm:col-span-2">
          <label className="block text-sm font-medium">Class & subject</label>
          <select
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
            {...register("classSubject")}
          >
            <option value="">Select...</option>
            {(teachingQuery.data ?? []).map((ta) => (
              <option key={ta.id} value={`${ta.classId}|${ta.subjectId}`}>
                {ta.className} — {ta.subjectName}
              </option>
            ))}
          </select>
          {errors.classSubject && (
            <p className="mt-1 text-sm text-red-600">{errors.classSubject.message}</p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium">Deadline</label>
          <input
            type="datetime-local"
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
            {...register("deadline")}
          />
          {errors.deadline && (
            <p className="mt-1 text-sm text-red-600">{errors.deadline.message}</p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium">Max marks</label>
          <input
            type="number"
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
            {...register("maxMarks")}
          />
          {errors.maxMarks && (
            <p className="mt-1 text-sm text-red-600">{errors.maxMarks.message}</p>
          )}
        </div>

        <div className="sm:col-span-2">
          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-neutral-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {isSubmitting ? "Creating..." : "Create assignment"}
          </button>
        </div>
      </form>

      {assignmentsQuery.isLoading ? (
        <p className="text-neutral-500">Loading assignments...</p>
      ) : assignmentsQuery.data?.items.length === 0 ? (
        <p className="text-neutral-500">No assignments yet.</p>
      ) : (
        <div className="max-w-4xl overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-neutral-200 text-left">
                <th className="py-2">Title</th>
                <th className="py-2">Class</th>
                <th className="py-2">Subject</th>
                <th className="py-2">Deadline</th>
                <th className="py-2">Status</th>
                <th className="py-2">Actions</th>
              </tr>
            </thead>
            <tbody>
              {assignmentsQuery.data?.items.map((assignment) => (
                <tr key={assignment.id} className="border-b border-neutral-100">
                  <td className="py-2">
                    <Link
                      href={`/teacher/assignments/${assignment.id}`}
                      className="underline underline-offset-4"
                    >
                      {assignment.title}
                    </Link>
                  </td>
                  <td className="py-2">{assignment.className}</td>
                  <td className="py-2">{assignment.subjectName}</td>
                  <td className="py-2">{new Date(assignment.deadlineUtc).toLocaleString()}</td>
                  <td className="py-2">{assignment.status}</td>
                  <td className="py-2 space-x-2">
                    <Link
                      href={`/teacher/assignments/${assignment.id}/submissions`}
                      className="text-sm font-medium text-neutral-700 underline"
                    >
                      Submissions
                    </Link>
                    {assignment.status === "Draft" ? (
                      <button
                        onClick={() => publishMutation.mutate(assignment.id)}
                        className="text-sm font-medium text-neutral-700 underline"
                      >
                        Publish
                      </button>
                    ) : (
                      <button
                        onClick={() => unpublishMutation.mutate(assignment.id)}
                        className="text-sm font-medium text-neutral-700 underline"
                      >
                        Unpublish
                      </button>
                    )}
                    <button
                      onClick={() => deleteMutation.mutate(assignment.id)}
                      className="text-sm font-medium text-red-600 underline"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
