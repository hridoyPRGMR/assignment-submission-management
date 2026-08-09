"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { CalendarClock, CheckCircle2, CircleAlert, ClipboardList, Loader2 } from "lucide-react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { dashboardApi } from "@/features/dashboard/dashboardApi";
import { useAuth } from "@/features/auth/AuthContext";
import { StatCard } from "@/components/ui/StatCard";
import { Button } from "@/components/ui/button";
import { EmptyState } from "@/components/ui/empty-state";
import { AssignmentStatusBadge } from "@/components/status-badge";
import { formatDateTime, formatRelativeDeadline } from "@/lib/format";
import { cn } from "@/lib/utils";

export default function StudentDashboardPage() {
  const { user } = useAuth();
  const dashboardQuery = useQuery({
    queryKey: ["dashboard", "student"],
    queryFn: () => dashboardApi.getStudent(),
  });
  const assignmentsQuery = useQuery({
    queryKey: ["assignments", "student"],
    queryFn: () => assignmentsApi.listPublishedForStudent(),
  });

  const upcoming = [...(assignmentsQuery.data?.items ?? [])]
    .sort((a, b) => new Date(a.deadlineUtc).getTime() - new Date(b.deadlineUtc).getTime())
    .slice(0, 4);

  const firstName = user?.fullName?.split(" ")[0] ?? "there";

  return (
    <div className="flex flex-1 flex-col gap-8">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Welcome back, {firstName}</h1>
        <p className="text-sm text-muted-foreground">
          Track your assignments, submissions, and grades.
        </p>
      </div>

      {dashboardQuery.isLoading ? (
        <div className="flex items-center gap-2 py-8 text-sm text-muted-foreground">
          <Loader2 className="size-4 animate-spin" />
          Loading your dashboard...
        </div>
      ) : dashboardQuery.isError ? (
        <div className="rounded-lg border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Could not load your dashboard data. Please try again.
        </div>
      ) : (
        dashboardQuery.data && (
          <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
            <StatCard
              label="Total assignments"
              value={dashboardQuery.data.totalPublishedAssignments}
              icon={ClipboardList}
              tone="blue"
            />
            <StatCard
              label="Not submitted"
              value={dashboardQuery.data.notSubmittedCount}
              icon={CircleAlert}
              tone="amber"
            />
            <StatCard
              label="Submitted"
              value={dashboardQuery.data.submittedCount}
              icon={CheckCircle2}
              tone="emerald"
            />
            <StatCard
              label="Graded"
              value={dashboardQuery.data.gradedCount}
              icon={CalendarClock}
              tone="violet"
            />
          </div>
        )
      )}

      <div className="flex flex-col gap-3">
        <div className="flex items-center justify-between">
          <div className="flex flex-col gap-0.5">
            <h2 className="text-lg font-semibold tracking-tight">Upcoming deadlines</h2>
            <p className="text-sm text-muted-foreground">Assignments that need your attention.</p>
          </div>
          <Button
            variant="outline"
            size="sm"
            nativeButton={false}
            render={<Link href="/student/assignments" />}
          >
            View all
          </Button>
        </div>

        {upcoming.length === 0 ? (
          <EmptyState
            title="No assignments yet"
            description="New assignments will appear here when your teachers publish them."
            className="py-12"
          />
        ) : (
          <div className="flex flex-col overflow-hidden rounded-lg border bg-card shadow-sm">
            {upcoming.map((assignment, index) => {
              const deadline = formatRelativeDeadline(assignment.deadlineUtc);
              return (
                <div
                  key={assignment.id}
                  className={cn(
                    "flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between",
                    index !== upcoming.length - 1 && "border-b",
                  )}
                >
                  <div className="flex min-w-0 flex-col gap-1">
                    <Link
                      href={`/student/assignments/${assignment.id}`}
                      className="truncate font-medium underline underline-offset-4"
                    >
                      {assignment.title}
                    </Link>
                    <p className="text-sm text-muted-foreground">
                      {assignment.subjectName} · Due {formatDateTime(assignment.deadlineUtc)}
                    </p>
                  </div>
                  <div className="flex shrink-0 items-center gap-2">
                    <span
                      className={cn(
                        "inline-flex items-center gap-1.5 rounded-md px-2 py-0.5 text-xs font-medium",
                        deadline.isOverdue
                          ? "bg-rose-500/10 text-rose-600"
                          : "bg-muted text-muted-foreground",
                      )}
                    >
                      <CalendarClock className="size-3" />
                      {deadline.label}
                    </span>
                    <AssignmentStatusBadge status={assignment.lifecycleStatus} />
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
