"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import {
  ALLOWED_SUBMISSION_FILE_EXTENSIONS,
  MAX_SUBMISSION_FILE_SIZE_BYTES,
  submissionsApi,
} from "@/features/submissions/submissionsApi";

const submissionSchema = z.object({
  content: z.string().min(1, "Submission content is required"),
});

type SubmissionFormValues = z.infer<typeof submissionSchema>;

function formatFileSize(bytes: number): string {
  return bytes < 1024 * 1024
    ? `${Math.round(bytes / 1024)} KB`
    : `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export default function StudentAssignmentDetailPage() {
  const params = useParams<{ id: string }>();
  const assignmentId = params.id;
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

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
      submissionsApi.create({ assignmentId, content: values.content, file: selectedFile }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["submissions", "for-assignment", assignmentId] });
      queryClient.invalidateQueries({ queryKey: ["submissions", "mine"] });
      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    },
  });

  const updateMutation = useMutation({
    mutationFn: (values: SubmissionFormValues) =>
      submissionsApi.update(submissionQuery.data!.id, { content: values.content, file: selectedFile }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["submissions", "for-assignment", assignmentId] });
      queryClient.invalidateQueries({ queryKey: ["submissions", "mine"] });
      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    },
  });

  if (assignmentQuery.isLoading || submissionQuery.isLoading) {
    return <p className="text-neutral-500">Loading...</p>;
  }

  const assignment = assignmentQuery.data;
  const submission = submissionQuery.data;
  const canEdit =
    !submission ||
    submission.status === "Submitted" ||
    submission.status === "LateSubmission" ||
    submission.status === "Returned";
  const mutation = submission ? updateMutation : createMutation;

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0] ?? null;
    if (!file) {
      setSelectedFile(null);
      setFileError(null);
      return;
    }

    const extension = `.${file.name.split(".").pop()?.toLowerCase()}`;
    if (!ALLOWED_SUBMISSION_FILE_EXTENSIONS.includes(extension)) {
      setFileError(`File type not allowed. Allowed types: ${ALLOWED_SUBMISSION_FILE_EXTENSIONS.join(", ")}.`);
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
          {submission.fileName && submission.fileDownloadUrl && (
            <p className="mt-1">
              Attachment:{" "}
              <button
                type="button"
                onClick={() => submissionsApi.downloadFile(submission.fileDownloadUrl!, submission.fileName!)}
                className="underline underline-offset-4"
              >
                {submission.fileName}
              </button>
              {submission.fileSizeBytes != null && ` (${formatFileSize(submission.fileSizeBytes)})`}
            </p>
          )}
          {submission.status === "Graded" && (
            <>
              <p>
                Marks: {submission.marks} / {submission.assignmentMaxMarks}
              </p>
              {submission.feedback && <p>Feedback: {submission.feedback}</p>}
            </>
          )}
          {submission.status === "Returned" && submission.feedback && (
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

          <div>
            <label className="block text-sm font-medium">
              Attach a file (optional — PDF, DOCX, or ZIP, max 20 MB)
            </label>
            <input
              ref={fileInputRef}
              type="file"
              accept={ALLOWED_SUBMISSION_FILE_EXTENSIONS.join(",")}
              onChange={handleFileChange}
              className="mt-1 w-full text-sm"
            />
            {fileError && <p className="mt-1 text-sm text-red-600">{fileError}</p>}
            {selectedFile && !fileError && (
              <p className="mt-1 text-sm text-neutral-500">
                Selected: {selectedFile.name} ({formatFileSize(selectedFile.size)})
              </p>
            )}
          </div>

          {mutation.isError && (
            <p className="text-sm text-red-600">
              Could not save your submission. The deadline for this assignment may have passed.
            </p>
          )}

          <button
            type="submit"
            disabled={isSubmitting || !!fileError}
            className="rounded-md bg-neutral-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {isSubmitting ? "Saving..." : submission ? "Update submission" : "Submit"}
          </button>
        </form>
      )}
    </div>
  );
}
