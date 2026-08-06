import { ColumnDef } from "@tanstack/react-table";
import { ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Subject } from "@/types/admin";
import { SortType } from "@/types/sortType";

interface ColumnOptions {
  sort: string;
  setSort: (sort: string) => void;
  onDelete: (id: string) => void;
}

export function getColumns({
  sort,
  setSort,
  onDelete,
}: ColumnOptions): ColumnDef<Subject>[] {
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
      header: "Id",
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
      cell: ({ row }) => <span className="font-medium">{row.original.name}</span>,
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
      cell: ({ row }) => row.original.description || "-",
    },
    {
      id: "actions",
      header: "Action",
      cell: ({ row }) => (
        <Button
          variant="destructive"
          size="sm"
          onClick={() => onDelete(row.original.id)}
        >
          Delete
        </Button>
      ),
    },
  ];
}