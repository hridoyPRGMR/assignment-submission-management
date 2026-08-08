"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import {
  CheckCircle2,
  ClipboardList,
  Clock,
  FileText,
  Loader2,
  Pencil,
  Plus,
  Send,
} from "lucide-react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { dashboardApi } from "@/features/dashboard/dashboardApi";
import { useAuth } from "@/features/auth/AuthContext";
import { StatCard } from "@/components/ui/StatCard";
import { Button } from "@/components/ui/button";
import { AssignmentStatusBadge } from "@/components/status-badge";
import { formatDateTime } from "@/lib/format";

export default function TeacherDashboardPage() {
  const { user } = useAuth();
  const dashboardQuery = useQuery({
    queryKey: ["dashboard", "teacher"],
    queryFn: () => dashboardApi.getTeacher(),
  });
  const assignmentsQuery = useQuery({
    queryKey: ["assignments", "mine"],
    queryFn: () => assignmentsApi.listMine(),
  });

  const recent = [...(assignmentsQuery.data?.items ?? [])]
    .sort((a, b) => new Date(b.createdAtUtc).getTime() - new Date(a.createdAtUtc).getTime())
    .slice(0, 5);

  const firstName = user?.fullName?.split(" ")[0] ?? "there";

  return (
    <div className="flex flex-1 flex-col gap-8">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Welcome back, {firstName}</h1>
        <p className="text-sm text-muted-foreground">
          Manage your assignments and review student submissions.
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
          <div className="grid grid-cols-2 gap-4 lg:grid-cols-5">
            <StatCard
              label="Assignments"
              value={dashboardQuery.data.totalAssignments}
              icon={ClipboardList}
              tone="blue"
            />
            <StatCard label="Draft" value={dashboardQuery.data.draftAssignments} icon={FileText} />
            <StatCard
              label="Published"
              value={dashboardQuery.data.publishedAssignments}
              icon={Send}
              tone="emerald"
            />
            <StatCard
              label="Submissions"
              value={dashboardQuery.data.totalSubmissions}
              icon={CheckCircle2}
              tone="violet"
            />
            <StatCard
              label="Pending to grade"
              value={dashboardQuery.data.pendingToGradeCount}
              icon={Clock}
              tone="amber"
            />
          </div>
        )
      )}

      <div className="flex flex-wrap items-center gap-3">
        <Button nativeButton={false} render={<Link href="/teacher/assignments" />}>
          <Plus />
          Create assignment
        </Button>
        <Button
          variant="outline"
          nativeButton={false}
          render={<Link href="/teacher/assignments" />}
        >
          View assignments
        </Button>
      </div>

      <div className="flex flex-col gap-3">
        <div className="flex flex-col gap-0.5">
          <h2 className="text-lg font-semibold tracking-tight">Recent assignments</h2>
          <p className="text-sm text-muted-foreground">
            Your latest assignments and their current status.
          </p>
        </div>

        {assignmentsQuery.isLoading ? (
          <div className="flex items-center gap-2 py-8 text-sm text-muted-foreground">
            <Loader2 className="size-4 animate-spin" />
            Loading assignments...
          </div>
        ) : recent.length === 0 ? (
          <div className="flex flex-col items-center justify-center gap-1 rounded-lg border border-dashed bg-card px-4 py-12 text-center shadow-sm">
            <p className="text-sm font-medium text-foreground">No assignments yet</p>
            <p className="text-sm text-muted-foreground">
              Create your first assignment to get started.
            </p>
          </div>
        ) : (
          <div className="flex flex-col overflow-hidden rounded-lg border bg-card shadow-sm">
            {recent.map((assignment, index) => (
              <div
                key={assignment.id}
                className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between"
              >
                <div className="flex min-w-0 flex-col gap-1">
                  <Link
                    href={`/teacher/assignments/${assignment.id}`}
                    className="truncate font-medium underline underline-offset-4"
                  >
                    {assignment.title}
                  </Link>
                  <p className="text-sm text-muted-foreground">
                    {assignment.className} · {assignment.subjectName} · Due{" "}
                    {formatDateTime(assignment.deadlineUtc)}
                  </p>
                </div>
                <div className="flex shrink-0 items-center gap-2">
                  <AssignmentStatusBadge status={assignment.lifecycleStatus} />
                  <Button
                    variant="ghost"
                    size="icon-sm"
                    nativeButton={false}
                    className="hover:bg-muted"
                    render={<Link href={`/teacher/assignments/${assignment.id}`} />}
                    aria-label={`Edit ${assignment.title}`}
                  >
                    <Pencil />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
