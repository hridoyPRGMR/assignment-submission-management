"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { StatCard } from "@/components/ui/StatCard";
import { dashboardApi } from "@/features/dashboard/dashboardApi";
import { useAuth } from "@/features/auth/AuthContext";

export default function TeacherDashboardPage() {
  const { user } = useAuth();
  const { data, isLoading, isError } = useQuery({
    queryKey: ["dashboard", "teacher"],
    queryFn: () => dashboardApi.getTeacher(),
  });

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Teacher Dashboard</h1>
        <p className="text-neutral-500">Welcome, {user?.fullName}.</p>
      </div>

      {isLoading && <p className="text-neutral-500">Loading dashboard...</p>}
      {isError && <p className="text-red-600">Could not load dashboard data.</p>}

      {data && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
          <StatCard label="Assignments" value={data.totalAssignments} />
          <StatCard label="Draft" value={data.draftAssignments} />
          <StatCard label="Published" value={data.publishedAssignments} />
          <StatCard label="Submissions" value={data.totalSubmissions} />
          <StatCard label="Pending to grade" value={data.pendingToGradeCount} />
        </div>
      )}

      <Link
        href="/teacher/assignments"
        className="text-sm font-medium underline underline-offset-4"
      >
        View all assignments
      </Link>
    </div>
  );
}
