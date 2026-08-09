"use client";

import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { classesApi } from "@/features/classes/classesApi";
import { EmptyState } from "@/components/ui/empty-state";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { DataTable } from "./data-table";
import { getColumns } from "./columns";
import type { AssignmentStatus } from "@/types/assignment";

const statusFilters: { key: string; label: string }[] = [
  { key: "all", label: "All" },
  { key: "Draft", label: "Draft" },
  { key: "Published", label: "Published" },
  { key: "Graded", label: "Graded" },
  { key: "Archived", label: "Archived" },
];

export default function AdminAssignmentsPage() {
  const [statusFilter, setStatusFilter] = useState("all");
  const [classFilter, setClassFilter] = useState("all");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const classesQuery = useQuery({
    queryKey: ["classes", "all"],
    queryFn: () => classesApi.list(1, 100),
  });

  const assignmentsQuery = useQuery({
    queryKey: ["assignments", "admin", page, pageSize, statusFilter, classFilter],
    queryFn: () =>
      assignmentsApi.listForAdmin({
        page,
        pageSize,
        status: statusFilter === "all" ? undefined : (statusFilter as AssignmentStatus),
        classId: classFilter === "all" ? undefined : classFilter,
      }),
  });

  const classOptions = [
    { label: "All classes", value: "all" },
    ...(classesQuery.data?.items ?? []).map((c) => ({ label: c.name, value: c.id })),
  ];

  const columns = getColumns();
  const items = assignmentsQuery.data?.items ?? [];
  const isEmpty = assignmentsQuery.data != null && items.length === 0;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Assignments</h1>
        <p className="text-sm text-muted-foreground">
          Browse every assignment across all classes and teachers.
        </p>
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <Tabs
          value={statusFilter}
          onValueChange={(value) => {
            setStatusFilter(value as string);
            setPage(1);
          }}
        >
          <TabsList>
            {statusFilters.map((filter) => (
              <TabsTrigger key={filter.key} value={filter.key}>
                {filter.label}
              </TabsTrigger>
            ))}
          </TabsList>
        </Tabs>

        <Select
          value={classFilter}
          onValueChange={(value) => {
            setClassFilter(value ?? "all");
            setPage(1);
          }}
          items={classOptions}
        >
          <SelectTrigger className="w-full sm:w-56">
            <SelectValue placeholder="Filter by class..." />
          </SelectTrigger>
          <SelectContent>
            {classOptions.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {assignmentsQuery.isLoading ? (
        <TableSkeleton columns={6} />
      ) : isEmpty ? (
        <EmptyState
          title="No assignments found"
          description="Try a different status or class filter."
        />
      ) : (
        <>
          <div className="overflow-hidden rounded-lg border bg-card shadow-sm">
            <DataTable columns={columns} data={items} />
          </div>

          <div className="flex flex-col items-center justify-between gap-3 sm:flex-row">
            <p className="shrink-0 text-sm text-muted-foreground">
              Page {assignmentsQuery.data?.page ?? 1} of {assignmentsQuery.data?.totalPages ?? 1}
            </p>

            <Pagination className="mx-0 w-auto shrink-0 justify-center sm:justify-end">
              <PaginationContent>
                <PaginationItem>
                  <PaginationPrevious onClick={() => page > 1 && setPage(page - 1)} />
                </PaginationItem>

                {Array.from({ length: assignmentsQuery.data?.totalPages ?? 0 }, (_, i) => (
                  <PaginationItem key={i}>
                    <PaginationLink isActive={page === i + 1} onClick={() => setPage(i + 1)}>
                      {i + 1}
                    </PaginationLink>
                  </PaginationItem>
                ))}

                <PaginationItem>
                  <PaginationNext
                    onClick={() =>
                      page < (assignmentsQuery.data?.totalPages ?? 1) && setPage(page + 1)
                    }
                  />
                </PaginationItem>
              </PaginationContent>
            </Pagination>
          </div>
        </>
      )}
    </div>
  );
}
