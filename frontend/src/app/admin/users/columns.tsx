import { ColumnDef } from "@tanstack/react-table";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { AdminUser } from "@/types/admin";

interface ColumnOptions {
  onToggleActive: (user: AdminUser) => void;
  pendingId: string | null;
}

const roleStyles: Record<AdminUser["role"], string> = {
  Admin: "bg-blue-500/10 text-blue-600",
  Teacher: "bg-amber-500/10 text-amber-600",
  Student: "bg-emerald-500/10 text-emerald-600",
};

export function getColumns({ onToggleActive, pendingId }: ColumnOptions): ColumnDef<AdminUser>[] {
  return [
    {
      accessorKey: "fullName",
      header: "Full Name",
      cell: ({ row }) => <span className="font-medium">{row.original.fullName}</span>,
    },
    {
      accessorKey: "email",
      header: "Email",
      cell: ({ row }) => <span className="text-muted-foreground">{row.original.email}</span>,
    },
    {
      accessorKey: "role",
      header: "Role",
      cell: ({ row }) => (
        <span
          className={cn(
            "inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium",
            roleStyles[row.original.role],
          )}
        >
          {row.original.role}
        </span>
      ),
    },
    {
      accessorKey: "isActive",
      header: "Status",
      cell: ({ row }) => (
        <span
          className={cn(
            "inline-flex items-center gap-1.5 rounded-md px-2 py-0.5 text-xs font-medium",
            row.original.isActive
              ? "bg-emerald-500/10 text-emerald-600"
              : "bg-muted text-muted-foreground",
          )}
        >
          <span
            className={cn(
              "size-1.5 rounded-full",
              row.original.isActive ? "bg-emerald-500" : "bg-muted-foreground",
            )}
          />
          {row.original.isActive ? "Active" : "Inactive"}
        </span>
      ),
    },
    {
      id: "actions",
      header: () => <div className="text-right">Action</div>,
      cell: ({ row }) => {
        const user = row.original;
        return (
          <div className="flex justify-end">
            <Button
              variant="outline"
              size="sm"
              disabled={pendingId === user.id}
              onClick={() => onToggleActive(user)}
              aria-label={
                user.isActive ? `Deactivate ${user.fullName}` : `Activate ${user.fullName}`
              }
            >
              {user.isActive ? "Deactivate" : "Activate"}
            </Button>
          </div>
        );
      },
    },
  ];
}
