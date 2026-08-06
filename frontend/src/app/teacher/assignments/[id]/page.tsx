"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams, useRouter } from "next/navigation";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";

const assignmentSchema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().min(1, "Description is required"),
  deadline: z.string().min(1, "Deadline is required"),
  maxMarks: z.coerce.number().int().positive("Must be greater than zero"),
});

type AssignmentFormValues = z.input<typeof assignmentSchema>;
type AssignmentFormOutput = z.output<typeof assignmentSchema>;

function toLocalInputValue(isoDate: string): string {
  const date = new Date(isoDate);
  const offsetMs = date.getTimezoneOffset() * 60 * 1000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

export default function TeacherAssignmentDetailPage() {
  const params = useParams<{ id: string }>();
  const assignmentId = params.id;
  const queryClient = useQueryClient();
  const router = useRouter();

  const assignmentQuery = useQuery({
    queryKey: ["assignments", assignmentId],
    queryFn: () => assignmentsApi.getById(assignmentId),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<AssignmentFormValues, unknown, AssignmentFormOutput>({
    resolver: zodResolver(assignmentSchema),
  });

  useEffect(() => {
    if (assignmentQuery.data) {
      reset({
        title: assignmentQuery.data.title,
        description: assignmentQuery.data.description,
        deadline: toLocalInputValue(assignmentQuery.data.deadlineUtc),
        maxMarks: assignmentQuery.data.maxMarks,
      });
    }
  }, [assignmentQuery.data, reset]);

  const updateMutation = useMutation({
    mutationFn: (values: AssignmentFormOutput) =>
      assignmentsApi.update(assignmentId, {
        title: values.title,
        description: values.description,
        deadlineUtc: new Date(values.deadline).toISOString(),
        maxMarks: values.maxMarks,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments", assignmentId] });
      queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => assignmentsApi.remove(assignmentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] });
      router.push("/teacher/assignments");
    },
  });

  if (assignmentQuery.isLoading) {
    return <p className="text-neutral-500">Loading...</p>;
  }

  const assignment = assignmentQuery.data;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <h1 className="text-2xl font-semibold">{assignment?.title}</h1>
      <p className="text-sm text-neutral-500">
        {assignment?.className} — {assignment?.subjectName} — {assignment?.status}
      </p>

      <form
        onSubmit={handleSubmit((values) => updateMutation.mutate(values))}
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

        <div>
          <label className="block text-sm font-medium">Deadline</label>
          <input
            type="datetime-local"
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
            {...register("deadline")}
          />
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

        {updateMutation.isError && (
          <p className="sm:col-span-2 text-sm text-red-600">
            Could not save changes. The deadline and max marks cannot be changed once submissions
            exist.
          </p>
        )}

        <div className="sm:col-span-2 flex gap-3">
          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-neutral-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {isSubmitting ? "Saving..." : "Save changes"}
          </button>
          <button
            type="button"
            onClick={() => deleteMutation.mutate()}
            className="rounded-md border border-red-300 px-4 py-2 text-sm font-medium text-red-600"
          >
            Delete assignment
          </button>
        </div>
      </form>
    </div>
  );
}
