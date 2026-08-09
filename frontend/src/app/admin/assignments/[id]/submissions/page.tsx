"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { ArrowLeft, Download, Loader2, UserRound } from "lucide-react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { submissionsApi } from "@/features/submissions/submissionsApi";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { AssignmentStatusBadge, SubmissionStatusBadge } from "@/components/status-badge";
import { EmptyState } from "@/components/ui/empty-state";
import { formatDateTime, formatFileSize } from "@/lib/format";

export default function AdminSubmissionsPage() {
  const params = useParams<{ id: string }>();
  const assignmentId = params.id;

  const assignmentQuery = useQuery({
    queryKey: ["assignments", assignmentId],
    queryFn: () => assignmentsApi.getById(assignmentId),
  });

  const submissionsQuery = useQuery({
    queryKey: ["submissions", "for-assignment-admin", assignmentId],
    queryFn: () => submissionsApi.listForAssignment(assignmentId),
  });

  if (assignmentQuery.isLoading || submissionsQuery.isLoading) {
    return (
      <div className="flex flex-1 flex-col items-center justify-center gap-2 py-24">
        <Loader2 className="size-5 animate-spin text-muted-foreground" />
        <p className="text-sm text-muted-foreground">Loading submissions...</p>
      </div>
    );
  }

  const assignment = assignmentQuery.data;
  const submissions = submissionsQuery.data ?? [];
  const gradedCount = submissions.filter((s) => s.status === "Graded").length;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <Button
        variant="ghost"
        size="sm"
        nativeButton={false}
        className="-ml-2 w-fit"
        render={<Link href="/admin/assignments" />}
      >
        <ArrowLeft />
        Back to Assignments
      </Button>

      <div className="flex flex-col gap-1">
        <div className="flex flex-wrap items-center gap-2">
          <h1 className="text-2xl font-semibold tracking-tight">{assignment?.title}</h1>
          {assignment && <AssignmentStatusBadge status={assignment.lifecycleStatus} />}
        </div>
        <p className="text-sm text-muted-foreground">
          {assignment?.className} · {assignment?.subjectName} · Teacher{" "}
          {assignment?.teacherFullName} · {submissions.length} submission
          {submissions.length === 1 ? "" : "s"} · {gradedCount} graded
        </p>
      </div>

      {submissions.length === 0 ? (
        <EmptyState
          title="No submissions yet"
          description="Student submissions will appear here once they are submitted."
        />
      ) : (
        <div className="flex flex-col gap-4">
          {submissions.map((submission) => (
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
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
