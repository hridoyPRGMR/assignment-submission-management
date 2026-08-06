"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams } from "next/navigation";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { submissionsApi } from "@/features/submissions/submissionsApi";

const submissionSchema = z.object({
  content: z.string().min(1, "Submission content is required"),
});

type SubmissionFormValues = z.infer<typeof submissionSchema>;

export default function StudentAssignmentDetailPage() {
  const params = useParams<{ id: string }>();
  const assignmentId = params.id;
  const queryClient = useQueryClient();

  const assignmentQuery = useQuery({
    queryKey: ["assignments", assignmentId],
    queryFn: () => assignmentsApi.getById(assignmentId),
  });

  const submissionQuery = useQuery({
    queryKey: ["submissions", "for-assignment", assignmentId],
    queryFn: () => submissionsApi.getForAssignment(assignmentId),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<SubmissionFormValues>({ resolver: zodResolver(submissionSchema) });

  useEffect(() => {
    if (submissionQuery.data) {
      reset({ content: submissionQuery.data.content });
    }
  }, [submissionQuery.data, reset]);

  const createMutation = useMutation({
    mutationFn: (values: SubmissionFormValues) =>
      submissionsApi.create({ assignmentId, content: values.content }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["submissions", "for-assignment", assignmentId] });
      queryClient.invalidateQueries({ queryKey: ["submissions", "mine"] });
    },
  });

  const updateMutation = useMutation({
    mutationFn: (values: SubmissionFormValues) =>
      submissionsApi.update(submissionQuery.data!.id, { content: values.content }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["submissions", "for-assignment", assignmentId] });
      queryClient.invalidateQueries({ queryKey: ["submissions", "mine"] });
    },
  });

  if (assignmentQuery.isLoading || submissionQuery.isLoading) {
    return <p className="text-neutral-500">Loading...</p>;
  }

  const assignment = assignmentQuery.data;
  const submission = submissionQuery.data;
  const canEdit =
    !submission || submission.status === "Submitted" || submission.status === "ReturnedForRevision";
  const mutation = submission ? updateMutation : createMutation;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <h1 className="text-2xl font-semibold">{assignment?.title}</h1>
      <p className="text-sm text-neutral-500">
        {assignment?.subjectName} — Due{" "}
        {assignment && new Date(assignment.deadlineUtc).toLocaleString()} — Max marks{" "}
        {assignment?.maxMarks}
      </p>
      <p className="max-w-2xl whitespace-pre-wrap text-sm">{assignment?.description}</p>

      {submission && (
        <div className="max-w-2xl rounded-md border border-neutral-200 p-4 text-sm">
          <p>
            Status: <strong>{submission.status}</strong>
          </p>
          {submission.status === "Graded" && (
            <>
              <p>
                Marks: {submission.marks} / {submission.assignmentMaxMarks}
              </p>
              {submission.feedback && <p>Feedback: {submission.feedback}</p>}
            </>
          )}
          {submission.status === "ReturnedForRevision" && submission.feedback && (
            <p>Feedback: {submission.feedback}</p>
          )}
        </div>
      )}

      {canEdit && (
        <form
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
          className="max-w-2xl space-y-4 rounded-md border border-neutral-200 p-4"
        >
          <div>
            <label className="block text-sm font-medium">Your answer</label>
            <textarea
              rows={6}
              className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
              {...register("content")}
            />
            {errors.content && (
              <p className="mt-1 text-sm text-red-600">{errors.content.message}</p>
            )}
          </div>

          {mutation.isError && (
            <p className="text-sm text-red-600">
              Could not save your submission. The deadline for this assignment may have passed.
            </p>
          )}

          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-neutral-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {isSubmitting ? "Saving..." : submission ? "Update submission" : "Submit"}
          </button>
        </form>
      )}
    </div>
  );
}
