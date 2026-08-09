import { ColumnDef } from "@tanstack/react-table";
import Link from "next/link";
import { Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { AssignmentStatusBadge } from "@/components/status-badge";
import { formatDateTime, formatRelativeDeadline } from "@/lib/format";
import { cn } from "@/lib/utils";
import type { Assignment } from "@/types/assignment";

interface ColumnOptions {
  onPublish: (id: string) => void;
  onUnpublish: (id: string) => void;
  onMarkGraded: (id: string) => void;
  onArchive: (id: string) => void;
  onDelete: (id: string) => void;
  pendingId: string | null;
}

export function getColumns({
  onPublish,
  onUnpublish,
  onMarkGraded,
  onArchive,
  onDelete,
  pendingId,
}: ColumnOptions): ColumnDef<Assignment>[] {
  return [
    {
      accessorKey: "title",
      header: "Title",
      cell: ({ row }) => (
        <Link
          href={`/teacher/assignments/${row.original.id}`}
          className="font-medium underline underline-offset-4"
        >
          {row.original.title}
        </Link>
      ),
    },
    {
      accessorKey: "className",
      header: "Class",
      cell: ({ row }) => <span className="text-muted-foreground">{row.original.className}</span>,
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
      accessorKey: "lifecycleStatus",
      header: "Status",
      cell: ({ row }) => <AssignmentStatusBadge status={row.original.lifecycleStatus} />,
    },
    {
      id: "actions",
      header: () => <div className="text-right">Actions</div>,
      cell: ({ row }) => {
        const assignment = row.original;
        const busy = pendingId === assignment.id;
        return (
          <div className="flex justify-end gap-1">
            <Button
              variant="ghost"
              size="sm"
              nativeButton={false}
              className="hover:bg-muted"
              render={<Link href={`/teacher/assignments/${assignment.id}/submissions`} />}
            >
              Submissions
            </Button>

            {assignment.status === "Draft" && (
              <Button
                variant="ghost"
                size="sm"
                className="hover:bg-muted"
                disabled={busy}
                onClick={() => onPublish(assignment.id)}
              >
                Publish
              </Button>
            )}

            {assignment.lifecycleStatus === "SubmissionOpen" && (
              <Button
                variant="ghost"
                size="sm"
                className="hover:bg-muted"
                disabled={busy}
                onClick={() => onUnpublish(assignment.id)}
              >
                Unpublish
              </Button>
            )}

            {assignment.lifecycleStatus === "DeadlinePassed" && (
              <Button
                variant="ghost"
                size="sm"
                className="hover:bg-muted"
                disabled={busy}
                onClick={() => onMarkGraded(assignment.id)}
              >
                Mark graded
              </Button>
            )}

            {assignment.status === "Graded" && (
              <Button
                variant="ghost"
                size="sm"
                className="hover:bg-muted"
                disabled={busy}
                onClick={() => onArchive(assignment.id)}
              >
                Archive
              </Button>
            )}

            <Tooltip>
              <TooltipTrigger
                render={
                  <Button
                    variant="ghost"
                    size="icon-sm"
                    className="hover:bg-destructive/10 hover:text-destructive"
                    disabled={busy}
                    onClick={() => onDelete(assignment.id)}
                    aria-label={`Delete ${assignment.title}`}
                  />
                }
              >
                <Trash2 />
              </TooltipTrigger>
              <TooltipContent>Delete assignment</TooltipContent>
            </Tooltip>
          </div>
        );
      },
    },
  ];
}
