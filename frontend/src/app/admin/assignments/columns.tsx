import { ColumnDef } from "@tanstack/react-table";
import Link from "next/link";
import { Eye } from "lucide-react";
import { Button } from "@/components/ui/button";
import { AssignmentStatusBadge } from "@/components/status-badge";
import { formatDateTime } from "@/lib/format";
import type { Assignment } from "@/types/assignment";

export function getColumns(): ColumnDef<Assignment>[] {
  return [
    {
      accessorKey: "title",
      header: "Title",
      cell: ({ row }) => <span className="font-medium">{row.original.title}</span>,
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
      accessorKey: "teacherFullName",
      header: "Teacher",
      cell: ({ row }) => (
        <span className="text-muted-foreground">{row.original.teacherFullName}</span>
      ),
    },
    {
      accessorKey: "deadlineUtc",
      header: "Deadline",
      cell: ({ row }) => (
        <span className="text-muted-foreground">{formatDateTime(row.original.deadlineUtc)}</span>
      ),
    },
    {
      accessorKey: "lifecycleStatus",
      header: "Status",
      cell: ({ row }) => <AssignmentStatusBadge status={row.original.lifecycleStatus} />,
    },
    {
      id: "actions",
      header: () => <div className="text-right">Action</div>,
      cell: ({ row }) => (
        <div className="flex justify-end">
          <Button
            variant="ghost"
            size="sm"
            nativeButton={false}
            className="hover:bg-muted"
            render={<Link href={`/admin/assignments/${row.original.id}/submissions`} />}
          >
            <Eye />
            Submissions
          </Button>
        </div>
      ),
    },
  ];
}
