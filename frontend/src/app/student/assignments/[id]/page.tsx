"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import {
  ArrowLeft,
  CalendarClock,
  CheckCircle2,
  CircleAlert,
  Download,
  FileUp,
  Loader2,
} from "lucide-react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import {
  ALLOWED_SUBMISSION_FILE_EXTENSIONS,
  MAX_SUBMISSION_FILE_SIZE_BYTES,
  submissionsApi,
} from "@/features/submissions/submissionsApi";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { SubmissionStatusBadge } from "@/components/status-badge";
import { toast } from "@/components/ui/toast";
import { getErrorMessage } from "@/lib/errors";
import { formatDateTime, formatFileSize, formatRelativeDeadline } from "@/lib/format";
import { cn } from "@/lib/utils";

const submissionSchema = z.object({
  content: z.string().min(1, "Submission content is required"),
});

type SubmissionFormValues = z.infer<typeof submissionSchema>;

export default function StudentAssignmentDetailPage() {
  const params = useParams<{ id: string }>();
  const assignmentId = params.id;
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

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

  const invalidateSubmissions = () => {
    queryClient.invalidateQueries({ queryKey: ["submissions", "for-assignment", assignmentId] });
    queryClient.invalidateQueries({ queryKey: ["submissions", "mine"] });
  };

  const clearFile = () => {
    setSelectedFile(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const createMutation = useMutation({
    mutationFn: (values: SubmissionFormValues) =>
      submissionsApi.create({ assignmentId, content: values.content, file: selectedFile }),
    onSuccess: () => {
      invalidateSubmissions();
      clearFile();
      setSubmitError(null);
      toast.add({ type: "success", title: "Submission saved" });
    },
    onError: (error) => {
      const message =
        getErrorMessage(error) ?? "Could not save your submission. Please try again.";
      setSubmitError(message);
      toast.add({ type: "error", title: "Could not submit", description: message });
    },
  });

  const updateMutation = useMutation({
    mutationFn: (values: SubmissionFormValues) =>
      submissionsApi.update(submissionQuery.data!.id, {
        content: values.content,
        file: selectedFile,
      }),
    onSuccess: () => {
      invalidateSubmissions();
      clearFile();
      setSubmitError(null);
      toast.add({ type: "success", title: "Submission updated" });
    },
    onError: (error) => {
      const message =
        getErrorMessage(error) ?? "Could not update your submission. Please try again.";
      setSubmitError(message);
      toast.add({ type: "error", title: "Could not update submission", description: message });
    },
  });

  if (assignmentQuery.isLoading || submissionQuery.isLoading) {
    return (
      <div className="flex flex-1 flex-col items-center justify-center gap-2 py-24">
        <Loader2 className="size-5 animate-spin text-muted-foreground" />
        <p className="text-sm text-muted-foreground">Loading assignment...</p>
      </div>
    );
  }

  const assignment = assignmentQuery.data;
  const submission = submissionQuery.data;
  const canEdit =
    !submission ||
    submission.status === "Submitted" ||
    submission.status === "LateSubmission" ||
    submission.status === "Returned";
  const mutation = submission ? updateMutation : createMutation;
  const deadline = assignment ? formatRelativeDeadline(assignment.deadlineUtc) : null;

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null;
    if (!file) {
      setSelectedFile(null);
      setFileError(null);
      return;
    }

    const extension = `.${file.name.split(".").pop()?.toLowerCase()}`;
    if (!ALLOWED_SUBMISSION_FILE_EXTENSIONS.includes(extension)) {
      setFileError(
        `File type not allowed. Allowed types: ${ALLOWED_SUBMISSION_FILE_EXTENSIONS.join(", ")}.`,
      );
      setSelectedFile(null);
      return;
    }

    if (file.size > MAX_SUBMISSION_FILE_SIZE_BYTES) {
      setFileError("File must be smaller than 20 MB.");
      setSelectedFile(null);
      return;
    }

    setFileError(null);
    setSelectedFile(file);
  };

  return (
    <div className="flex flex-1 flex-col gap-6">
      <Button
        variant="ghost"
        size="sm"
        nativeButton={false}
        className="-ml-2 w-fit"
        render={<Link href="/student/assignments" />}
      >
        <ArrowLeft />
        Back to Assignments
      </Button>

      <div className="flex flex-col gap-1">
        <div className="flex flex-wrap items-center gap-2">
          <h1 className="text-2xl font-semibold tracking-tight">{assignment?.title}</h1>
          {submission ? (
            <SubmissionStatusBadge status={submission.status} />
          ) : (
            <span className="inline-flex items-center rounded-md bg-muted px-2 py-0.5 text-xs font-medium text-muted-foreground">
              Not submitted
            </span>
          )}
        </div>
        <p className="text-sm text-muted-foreground">
          {assignment?.subjectName} · Taught by {assignment?.teacherFullName}
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card>
          <CardContent className="flex items-center gap-3">
            <div className="flex size-9 items-center justify-center rounded-lg bg-blue-500/10 text-blue-600">
              <CalendarClock className="size-4" />
            </div>
            <div className="flex flex-col gap-0.5">
              <p className="text-xs text-muted-foreground">Due</p>
              <p className="text-sm font-medium">
                {assignment && formatDateTime(assignment.deadlineUtc)}
              </p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-3">
            <div className="flex size-9 items-center justify-center rounded-lg bg-violet-500/10 text-violet-600">
              <CheckCircle2 className="size-4" />
            </div>
            <div className="flex flex-col gap-0.5">
              <p className="text-xs text-muted-foreground">Max marks</p>
              <p className="text-sm font-medium">{assignment?.maxMarks}</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-3">
            <div className="flex size-9 items-center justify-center rounded-lg bg-muted text-muted-foreground">
              <CircleAlert className="size-4" />
            </div>
            <div className="flex flex-col gap-0.5">
              <p className="text-xs text-muted-foreground">Deadline</p>
              <p className={cn("text-sm font-medium", deadline?.isOverdue ? "text-rose-600" : "")}>
                {deadline?.label}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Assignment details</CardTitle>
          <CardDescription>
            {assignment?.className} · {assignment?.subjectName}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <p className="whitespace-pre-wrap text-sm text-muted-foreground">
            {assignment?.description}
          </p>
        </CardContent>
      </Card>

      {!submission && deadline?.isOverdue && (
        <div className="flex items-center gap-2 rounded-lg border border-rose-500/20 bg-rose-500/10 px-4 py-3 text-sm text-rose-600">
          <CircleAlert className="size-4" />
          The deadline for this assignment has passed, so you can no longer submit.
        </div>
      )}

      {submission && (
        <Card>
          <CardHeader>
            <CardTitle>Your submission</CardTitle>
            <CardDescription>
              Submitted {formatDateTime(submission.submittedAtUtc)}
              {submission.isLate ? " · marked as late" : ""}
            </CardDescription>
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            <p className="whitespace-pre-wrap text-sm">{submission.content}</p>

            {submission.fileName && submission.fileDownloadUrl && (
              <div className="flex items-center gap-3">
                <div className="flex size-9 items-center justify-center rounded-lg bg-muted text-muted-foreground">
                  <FileUp className="size-4" />
                </div>
                <div className="flex flex-1 flex-col gap-0.5">
                  <p className="text-sm font-medium">{submission.fileName}</p>
                  {submission.fileSizeBytes != null && (
                    <p className="text-xs text-muted-foreground">
                      {formatFileSize(submission.fileSizeBytes)}
                    </p>
                  )}
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() =>
                    submissionsApi.downloadFile(submission.fileDownloadUrl!, submission.fileName!)
                  }
                >
                  <Download />
                  Download
                </Button>
              </div>
            )}

            {(submission.status === "Graded" ||
              (submission.status === "Returned" && submission.feedback)) && (
              <div className="rounded-lg border bg-muted/40 p-4">
                {submission.status === "Graded" && (
                  <div className="flex items-baseline gap-2">
                    <p className="text-2xl font-semibold tracking-tight">{submission.marks}</p>
                    <p className="text-sm text-muted-foreground">
                      / {submission.assignmentMaxMarks} marks
                    </p>
                  </div>
                )}
                {submission.feedback && (
                  <p className="mt-2 whitespace-pre-wrap text-sm text-muted-foreground">
                    {submission.status === "Returned" ? "Revision requested: " : "Feedback: "}
                    {submission.feedback}
                  </p>
                )}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {canEdit && (
        <Card>
          <CardHeader>
            <CardTitle>{submission ? "Update your submission" : "Submit your work"}</CardTitle>
            <CardDescription>
              You can update your answer or attachment at any time until the deadline.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form
              onSubmit={handleSubmit((values) => mutation.mutate(values))}
              className="flex flex-col gap-6"
            >
              <Field>
                <Label>Your answer</Label>
                <Textarea
                  rows={6}
                  placeholder="Write your answer here..."
                  {...register("content")}
                />
                {errors.content && (
                  <p className="text-sm text-destructive">{errors.content.message}</p>
                )}
              </Field>

              <Field>
                <Label>Attach a file (optional — PDF, DOCX, or ZIP, max 20 MB)</Label>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept={ALLOWED_SUBMISSION_FILE_EXTENSIONS.join(",")}
                  onChange={handleFileChange}
                  className="block w-full cursor-pointer rounded-lg border border-dashed border-input px-3 py-2 text-sm text-muted-foreground file:mr-3 file:cursor-pointer file:rounded-md file:border-0 file:bg-muted file:px-3 file:py-1.5 file:text-sm file:font-medium file:text-foreground hover:bg-muted/40"
                />
                {fileError && <p className="text-sm text-destructive">{fileError}</p>}
                {selectedFile && !fileError && (
                  <p className="text-sm text-muted-foreground">
                    Selected: {selectedFile.name} ({formatFileSize(selectedFile.size)})
                  </p>
                )}
              </Field>

              {submitError && (
                <div className="flex items-center gap-2 rounded-lg border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm text-destructive">
                  <CircleAlert className="size-4" />
                  {submitError}
                </div>
              )}

              <div className="flex justify-end gap-2">
                <Button type="submit" disabled={isSubmitting || !!fileError || mutation.isPending}>
                  {mutation.isPending ? "Saving..." : submission ? "Update submission" : "Submit"}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
