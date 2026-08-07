import { ColumnDef } from "@tanstack/react-table";
import Link from "next/link";
import { ArrowDown, ArrowUp, ArrowUpDown, Pencil, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { SchoolClass } from "@/types/admin";
import { SortType } from "@/types/sortType";

interface ColumnOptions {
  sort: string;
  setSort: (sort: string) => void;
  onDelete: (id: string) => void;
  onEdit: (classItem: SchoolClass) => void;
}

export function getColumns({
  sort,
  setSort,
  onDelete,
  onEdit,
}: ColumnOptions): ColumnDef<SchoolClass>[] {
  const toggleSort = (field: string) => {
    if (sort === `${field}${SortType.Asc}`) {
      setSort(`${field}${SortType.Desc}`);
    } else if (sort === `${field}${SortType.Desc}`) {
      setSort("");
    } else {
      setSort(`${field}${SortType.Asc}`);
    }
  };

  const getSortIcon = (field: string) => {
    if (sort === `${field}${SortType.Asc}`) {
      return <ArrowUp className="ml-2 h-4 w-4" />;
    }
    if (sort === `${field}${SortType.Desc}`) {
      return <ArrowDown className="ml-2 h-4 w-4" />;
    }
    return <ArrowUpDown className="ml-2 h-4 w-4 text-muted-foreground" />;
  };

  return [
    {
      accessorKey: "id",
      header: "ID",
      cell: ({ row }) => (
        <span className="font-mono text-xs text-muted-foreground">
          {row.original.id}
        </span>
      ),
    },
    {
      accessorKey: "name",
      header: () => (
        <Button
          variant="ghost"
          onClick={() => toggleSort("name")}
          className="-ml-4 hover:bg-transparent"
        >
          Name
          {getSortIcon("name")}
        </Button>
      ),
      cell: ({ row }) => (
        <Link
          href={`/admin/classes/${row.original.id}`}
          className="font-medium underline underline-offset-4"
        >
          {row.original.name}
        </Link>
      ),
    },
    {
      accessorKey: "description",
      header: () => (
        <Button
          variant="ghost"
          onClick={() => toggleSort("description")}
          className="-ml-4 hover:bg-transparent"
        >
          Description
          {getSortIcon("description")}
        </Button>
      ),
      cell: ({ row }) => (
        <span className="text-muted-foreground">
          {row.original.description || "-"}
        </span>
      ),
    },
    {
      id: "actions",
      header: () => <div className="text-right">Action</div>,
      cell: ({ row }) => (
        <div className="flex justify-end">
          <Button
            variant="ghost"
            size="icon-sm"
            className="hover:bg-muted"
            onClick={() => onEdit(row.original)}
            aria-label={`Edit ${row.original.name}`}
          >
            <Pencil />
          </Button>
          <Button
            variant="ghost"
            size="icon-sm"
            className="hover:bg-destructive/10 hover:text-destructive"
            onClick={() => onDelete(row.original.id)}
            aria-label={`Delete ${row.original.name}`}
          >
            <Trash2 />
          </Button>
        </div>
      ),
    },
  ];
}
