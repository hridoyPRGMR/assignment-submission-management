"use client";

import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { submissionsApi } from "@/features/submissions/submissionsApi";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { EmptyState } from "@/components/ui/empty-state";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { DataTable } from "./data-table";
import { getColumns } from "./columns";
import type { Assignment } from "@/types/assignment";
import type { Submission } from "@/types/submission";

const filters = [
  { key: "all", label: "All" },
  { key: "not-submitted", label: "Not submitted" },
  { key: "submitted", label: "Submitted" },
  { key: "graded", label: "Graded" },
  { key: "returned", label: "Returned" },
] as const;

type FilterKey = (typeof filters)[number]["key"];

function matchesFilter(
  assignment: Assignment,
  submission: Submission | undefined,
  key: FilterKey,
): boolean {
  switch (key) {
    case "all":
      return true;
    case "not-submitted":
      return !submission;
    case "submitted":
      return !!submission && submission.status !== "Graded" && submission.status !== "Returned";
    case "graded":
      return submission?.status === "Graded";
    case "returned":
      return submission?.status === "Returned";
  }
}

export default function StudentAssignmentsPage() {
  const [filter, setFilter] = useState<FilterKey>("all");

  const assignmentsQuery = useQuery({
    queryKey: ["assignments", "student"],
    queryFn: () => assignmentsApi.listPublishedForStudent(),
  });
  const submissionsQuery = useQuery({
    queryKey: ["submissions", "mine"],
    queryFn: () => submissionsApi.listMine(),
  });

  const submissionByAssignmentId = useMemo(
    () => new Map((submissionsQuery.data ?? []).map((s) => [s.assignmentId, s])),
    [submissionsQuery.data],
  );

  const allItems = assignmentsQuery.data?.items ?? [];
  const items = allItems.filter((assignment) =>
    matchesFilter(assignment, submissionByAssignmentId.get(assignment.id), filter),
  );

  const columns = useMemo(() => getColumns(submissionByAssignmentId), [submissionByAssignmentId]);

  const counts = useMemo(() => {
    const count = (key: FilterKey) =>
      allItems.filter((assignment) =>
        matchesFilter(assignment, submissionByAssignmentId.get(assignment.id), key),
      ).length;
    return {
      all: count("all"),
      "not-submitted": count("not-submitted"),
      submitted: count("submitted"),
      graded: count("graded"),
      returned: count("returned"),
    };
  }, [allItems, submissionByAssignmentId]);

  const isEmpty = assignmentsQuery.data != null && allItems.length === 0;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Assignments</h1>
        <p className="text-sm text-muted-foreground">
          All assignments published to you, with your submission status.
        </p>
      </div>

      <Tabs value={filter} onValueChange={(value) => setFilter(value as FilterKey)}>
        <TabsList className="h-auto flex-wrap">
          {filters.map((filterOption) => (
            <TabsTrigger key={filterOption.key} value={filterOption.key}>
              {filterOption.label} ({counts[filterOption.key]})
            </TabsTrigger>
          ))}
        </TabsList>
      </Tabs>

      {assignmentsQuery.isLoading ? (
        <TableSkeleton columns={4} />
      ) : items.length === 0 ? (
        <EmptyState
          title={isEmpty ? "No assignments yet" : "No assignments match this filter"}
          description={
            isEmpty
              ? "Assignments published by your teachers will appear here."
              : "Try a different filter."
          }
        />
      ) : (
        <div className="overflow-hidden rounded-lg border bg-card shadow-sm">
          <DataTable columns={columns} data={items} />
        </div>
      )}
    </div>
  );
}
