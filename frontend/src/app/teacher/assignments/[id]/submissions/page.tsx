"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { submissionsApi } from "@/features/submissions/submissionsApi";
import type { Submission } from "@/types/submission";

const gradeSchema = z.object({
  marks: z.coerce.number().int().min(0, "Marks cannot be negative"),
  feedback: z.string().min(1, "Feedback is required"),
});

type GradeFormValues = z.input<typeof gradeSchema>;
type GradeFormOutput = z.output<typeof gradeSchema>;

export default function TeacherSubmissionsPage() {
  const params = useParams<{ id: string }>();
  const assignmentId = params.id;
  const queryClient = useQueryClient();
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const assignmentQuery = useQuery({
    queryKey: ["assignments", assignmentId],
    queryFn: () => assignmentsApi.getById(assignmentId),
  });

  const submissionsQuery = useQuery({
    queryKey: ["submissions", "for-assignment-teacher", assignmentId],
    queryFn: () => submissionsApi.listForAssignment(assignmentId),
  });

  const invalidate = () =>
    queryClient.invalidateQueries({
      queryKey: ["submissions", "for-assignment-teacher", assignmentId],
    });

  const startReviewMutation = useMutation({
    mutationFn: (id: string) => submissionsApi.startReview(id),
    onSuccess: invalidate,
  });

  const gradeMutation = useMutation({
    mutationFn: ({ id, values }: { id: string; values: GradeFormOutput }) =>
      submissionsApi.grade(id, values.marks, values.feedback),
    onSuccess: () => {
      invalidate();
      setExpandedId(null);
    },
  });

  const returnMutation = useMutation({
    mutationFn: ({ id, feedback }: { id: string; feedback: string }) =>
      submissionsApi.returnForRevision(id, feedback),
    onSuccess: () => {
      invalidate();
      setExpandedId(null);
    },
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<GradeFormValues, unknown, GradeFormOutput>({ resolver: zodResolver(gradeSchema) });

  const startGrading = (submission: Submission) => {
    setExpandedId(submission.id);
    reset({ marks: submission.marks ?? 0, feedback: submission.feedback ?? "" });
  };

  if (assignmentQuery.isLoading || submissionsQuery.isLoading) {
    return <p className="text-neutral-500">Loading...</p>;
  }

  return (
    <div className="flex flex-1 flex-col gap-6">
      <h1 className="text-2xl font-semibold">{assignmentQuery.data?.title} — Submissions</h1>

      <div className="max-w-3xl space-y-3">
        {submissionsQuery.data?.length === 0 && (
          <p className="text-neutral-500">No submissions yet.</p>
        )}

        {submissionsQuery.data?.map((submission) => (
          <div key={submission.id} className="rounded-md border border-neutral-200 p-4 text-sm">
            <div className="flex items-center justify-between">
              <div>
                <p className="font-medium">{submission.studentFullName}</p>
                <p className="text-neutral-500">
                  Status: {submission.status}
                  {submission.status === "Graded" &&
                    ` — ${submission.marks}/${submission.assignmentMaxMarks}`}
                </p>
              </div>
              <div className="flex gap-2">
                {(submission.status === "Submitted" || submission.status === "LateSubmission") && (
                  <button
                    onClick={() => startReviewMutation.mutate(submission.id)}
                    className="text-sm font-medium text-neutral-700 underline"
                  >
                    Start review
                  </button>
                )}
                <button
                  onClick={() => startGrading(submission)}
                  className="text-sm font-medium text-neutral-700 underline"
                >
                  Grade
                </button>
              </div>
            </div>

            <p className="mt-2 whitespace-pre-wrap text-neutral-700">{submission.content}</p>

            {expandedId === submission.id && (
              <form
                onSubmit={handleSubmit((values) =>
                  gradeMutation.mutate({ id: submission.id, values }),
                )}
                className="mt-4 space-y-3 border-t border-neutral-200 pt-4"
              >
                <div>
                  <label className="block text-sm font-medium">
                    Marks (out of {submission.assignmentMaxMarks})
                  </label>
                  <input
                    type="number"
                    className="mt-1 w-full max-w-xs rounded-md border border-neutral-300 px-3 py-2 text-sm"
                    {...register("marks")}
                  />
                  {errors.marks && (
                    <p className="mt-1 text-sm text-red-600">{errors.marks.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium">Feedback</label>
                  <textarea
                    rows={3}
                    className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
                    {...register("feedback")}
                  />
                  {errors.feedback && (
                    <p className="mt-1 text-sm text-red-600">{errors.feedback.message}</p>
                  )}
                </div>

                <div className="flex gap-3">
                  <button
                    type="submit"
                    className="rounded-md bg-neutral-900 px-4 py-2 text-sm font-medium text-white"
                  >
                    Submit grade
                  </button>
                  <button
                    type="button"
                    onClick={handleSubmit((values) =>
                      returnMutation.mutate({ id: submission.id, feedback: values.feedback }),
                    )}
                    className="rounded-md border border-neutral-300 px-4 py-2 text-sm font-medium"
                  >
                    Return for revision
                  </button>
                  <button
                    type="button"
                    onClick={() => setExpandedId(null)}
                    className="rounded-md px-4 py-2 text-sm font-medium text-neutral-500"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
