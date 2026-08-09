"use client";

import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { Loader2 } from "lucide-react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { submissionsApi } from "@/features/submissions/submissionsApi";
import { DataTable } from "./data-table";
import { getColumns } from "./columns";
import { cn } from "@/lib/utils";
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

      <div className="flex flex-wrap items-center gap-2">
        {filters.map((filterOption) => (
          <button
            key={filterOption.key}
            onClick={() => setFilter(filterOption.key)}
            className={cn(
              "rounded-full border px-3 py-1 text-sm font-medium transition-colors",
              filter === filterOption.key
                ? "border-primary bg-primary text-primary-foreground"
                : "border-border text-muted-foreground hover:bg-muted hover:text-foreground",
            )}
          >
            {filterOption.label} ({counts[filterOption.key]})
          </button>
        ))}
      </div>

      {assignmentsQuery.isLoading ? (
        <div className="flex flex-col items-center justify-center gap-2 py-24">
          <Loader2 className="size-5 animate-spin text-muted-foreground" />
          <p className="text-sm text-muted-foreground">Loading assignments...</p>
        </div>
      ) : items.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-1 rounded-lg border border-dashed bg-card px-4 py-24 text-center shadow-sm">
          <p className="text-sm font-medium text-foreground">
            {isEmpty ? "No assignments yet" : "No assignments match this filter"}
          </p>
          <p className="text-sm text-muted-foreground">
            {isEmpty
              ? "Assignments published by your teachers will appear here."
              : "Try a different filter."}
          </p>
        </div>
      ) : (
        <div className="overflow-hidden rounded-lg border bg-card shadow-sm">
          <DataTable columns={columns} data={items} />
        </div>
      )}
    </div>
  );
}
