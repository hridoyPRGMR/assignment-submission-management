import { ColumnDef } from "@tanstack/react-table";
import Link from "next/link";
import { SubmissionStatusBadge } from "@/components/status-badge";
import { formatDateTime, formatRelativeDeadline } from "@/lib/format";
import { cn } from "@/lib/utils";
import type { Assignment } from "@/types/assignment";
import type { Submission } from "@/types/submission";

export function getColumns(
  submissionsByAssignment: Map<string, Submission>,
): ColumnDef<Assignment>[] {
  return [
    {
      accessorKey: "title",
      header: "Title",
      cell: ({ row }) => (
        <Link
          href={`/student/assignments/${row.original.id}`}
          className="font-medium underline underline-offset-4"
        >
          {row.original.title}
        </Link>
      ),
    },
    {
      accessorKey: "subjectName",
      header: "Subject",
      cell: ({ row }) => <span className="text-muted-foreground">{row.original.subjectName}</span>,
    },
    {
      accessorKey: "deadlineUtc",
      header: "Deadline",
      cell: ({ row }) => {
        const deadline = formatRelativeDeadline(row.original.deadlineUtc);
        return (
          <div className="flex flex-col gap-0.5">
            <span className="text-muted-foreground">
              {formatDateTime(row.original.deadlineUtc)}
            </span>
            <span
              className={cn(
                "text-xs",
                deadline.isOverdue ? "text-rose-600" : "text-muted-foreground",
              )}
            >
              {deadline.label}
            </span>
          </div>
        );
      },
    },
    {
      id: "status",
      header: "Status",
      cell: ({ row }) => {
        const submission = submissionsByAssignment.get(row.original.id);
        if (!submission) {
          return (
            <span className="inline-flex items-center rounded-md bg-muted px-2 py-0.5 text-xs font-medium text-muted-foreground">
              Not submitted
            </span>
          );
        }
        return <SubmissionStatusBadge status={submission.status} />;
      },
    },
  ];
}
