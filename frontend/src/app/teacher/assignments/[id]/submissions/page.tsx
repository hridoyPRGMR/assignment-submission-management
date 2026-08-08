"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ArrowLeft, CircleAlert, Download, Loader2, RotateCcw, UserRound } from "lucide-react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { submissionsApi } from "@/features/submissions/submissionsApi";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { SubmissionStatusBadge } from "@/components/status-badge";
import { toast } from "@/components/ui/toast";
import { formatDateTime, formatFileSize } from "@/lib/format";
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
    onSuccess: () => {
      invalidate();
      toast.add({ type: "success", title: "Review started" });
    },
    onError: () => toast.add({ type: "error", title: "Could not start review" }),
  });

  const gradeMutation = useMutation({
    mutationFn: ({ id, values }: { id: string; values: GradeFormOutput }) =>
      submissionsApi.grade(id, values.marks, values.feedback),
    onSuccess: () => {
      invalidate();
      setExpandedId(null);
      toast.add({ type: "success", title: "Submission graded" });
    },
    onError: () => toast.add({ type: "error", title: "Could not grade submission" }),
  });

  const returnMutation = useMutation({
    mutationFn: ({ id, feedback }: { id: string; feedback: string }) =>
      submissionsApi.returnForRevision(id, feedback),
    onSuccess: () => {
      invalidate();
      setExpandedId(null);
      toast.add({ type: "success", title: "Submission returned for revision" });
    },
    onError: () => toast.add({ type: "error", title: "Could not return submission" }),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<GradeFormValues, unknown, GradeFormOutput>({
    resolver: zodResolver(gradeSchema),
  });

  const startGrading = (submission: Submission) => {
    setExpandedId(submission.id);
    reset({ marks: submission.marks ?? 0, feedback: submission.feedback ?? "" });
  };

  if (assignmentQuery.isLoading || submissionsQuery.isLoading) {
    return (
      <div className="flex flex-1 flex-col items-center justify-center gap-2 py-24">
        <Loader2 className="size-5 animate-spin text-muted-foreground" />
        <p className="text-sm text-muted-foreground">Loading submissions...</p>
      </div>
    );
  }

  const submissions = submissionsQuery.data ?? [];
  const pendingCount = submissions.filter(
    (s) => s.status === "Submitted" || s.status === "LateSubmission",
  ).length;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <Button
        variant="ghost"
        size="sm"
        nativeButton={false}
        className="-ml-2 w-fit"
        render={<Link href={`/teacher/assignments/${assignmentId}`} />}
      >
        <ArrowLeft />
        Back to Assignment
      </Button>

      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">{assignmentQuery.data?.title}</h1>
        <p className="text-sm text-muted-foreground">
          {submissions.length} submission{submissions.length === 1 ? "" : "s"} ·{" "}
          {pendingCount > 0 ? `${pendingCount} pending review` : "all submissions reviewed"}
        </p>
      </div>

      {submissions.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-1 rounded-lg border border-dashed bg-card px-4 py-24 text-center shadow-sm">
          <p className="text-sm font-medium text-foreground">No submissions yet</p>
          <p className="text-sm text-muted-foreground">
            Student submissions will appear here once they are submitted.
          </p>
        </div>
      ) : (
        <div className="flex flex-col gap-4">
          {submissions.map((submission) => {
            const canReview =
              submission.status === "Submitted" || submission.status === "LateSubmission";
            const isExpanded = expandedId === submission.id;
            const busy = gradeMutation.isPending || returnMutation.isPending;

            return (
              <Card key={submission.id}>
                <CardContent className="flex flex-col gap-4">
                  <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                    <div className="flex items-center gap-3">
                      <div className="flex size-9 items-center justify-center rounded-lg bg-muted text-muted-foreground">
                        <UserRound className="size-4" />
                      </div>
                      <div className="flex flex-col gap-0.5">
                        <p className="text-base font-medium">{submission.studentFullName}</p>
                        <p className="text-xs text-muted-foreground">
                          Submitted {formatDateTime(submission.submittedAtUtc)}
                          {submission.isLate ? " · marked as late" : ""}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <SubmissionStatusBadge status={submission.status} />
                      {submission.status === "Graded" && (
                        <span className="text-sm font-medium text-muted-foreground">
                          {submission.marks}/{submission.assignmentMaxMarks}
                        </span>
                      )}
                    </div>
                  </div>

                  <p className="whitespace-pre-wrap text-sm">{submission.content}</p>

                  {submission.fileName && submission.fileDownloadUrl && (
                    <div className="flex items-center gap-3">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          submissionsApi.downloadFile(
                            submission.fileDownloadUrl!,
                            submission.fileName!,
                          )
                        }
                      >
                        <Download />
                        {submission.fileName}
                        {submission.fileSizeBytes != null &&
                          ` (${formatFileSize(submission.fileSizeBytes)})`}
                      </Button>
                    </div>
                  )}

                  {submission.feedback && (
                    <p className="rounded-lg bg-muted/40 p-3 text-sm text-muted-foreground">
                      {submission.status === "Returned" ? "Revision requested: " : "Feedback: "}
                      {submission.feedback}
                    </p>
                  )}

                  {!isExpanded ? (
                    <div className="flex items-center gap-2">
                      {canReview && (
                        <Button
                          variant="outline"
                          size="sm"
                          disabled={startReviewMutation.isPending}
                          onClick={() => startReviewMutation.mutate(submission.id)}
                        >
                          Start review
                        </Button>
                      )}
                      <Button size="sm" onClick={() => startGrading(submission)}>
                        {submission.status === "Graded" ? "Edit grade" : "Grade"}
                      </Button>
                    </div>
                  ) : (
                    <form
                      onSubmit={handleSubmit((values) =>
                        gradeMutation.mutate({ id: submission.id, values }),
                      )}
                      className="flex flex-col gap-4 rounded-lg border bg-muted/30 p-4"
                    >
                      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                        <Field>
                          <Label>Marks (out of {submission.assignmentMaxMarks})</Label>
                          <Input type="number" min={0} {...register("marks")} />
                          {errors.marks && (
                            <p className="text-sm text-destructive">{errors.marks.message}</p>
                          )}
                        </Field>
                        <Field className="sm:col-span-2">
                          <Label>Feedback</Label>
                          <Textarea
                            rows={3}
                            placeholder="Give feedback to the student..."
                            {...register("feedback")}
                          />
                          {errors.feedback && (
                            <p className="text-sm text-destructive">{errors.feedback.message}</p>
                          )}
                        </Field>
                      </div>

                      {gradeMutation.isError || returnMutation.isError ? (
                        <div className="flex items-center gap-2 rounded-lg border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm text-destructive">
                          <CircleAlert className="size-4" />
                          Could not save. Please try again.
                        </div>
                      ) : null}

                      <div className="flex flex-wrap items-center gap-2">
                        <Button type="submit" size="sm" disabled={busy}>
                          {gradeMutation.isPending ? "Grading..." : "Submit grade"}
                        </Button>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          disabled={busy}
                          onClick={handleSubmit((values) =>
                            returnMutation.mutate({
                              id: submission.id,
                              feedback: values.feedback,
                            }),
                          )}
                        >
                          <RotateCcw />
                          Return for revision
                        </Button>
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => setExpandedId(null)}
                        >
                          Cancel
                        </Button>
                      </div>
                    </form>
                  )}
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
