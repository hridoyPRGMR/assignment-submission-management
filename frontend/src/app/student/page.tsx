"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { StatCard } from "@/components/ui/StatCard";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { dashboardApi } from "@/features/dashboard/dashboardApi";
import { useAuth } from "@/features/auth/AuthContext";

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
    .slice(0, 3);

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Student Dashboard</h1>
        <p className="text-neutral-500">Welcome, {user?.fullName}.</p>
      </div>

      {dashboardQuery.isLoading && <p className="text-neutral-500">Loading dashboard...</p>}
      {dashboardQuery.isError && <p className="text-red-600">Could not load dashboard data.</p>}

      {dashboardQuery.data && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          <StatCard label="Assignments" value={dashboardQuery.data.totalPublishedAssignments} />
          <StatCard label="Not submitted" value={dashboardQuery.data.notSubmittedCount} />
          <StatCard label="Submitted" value={dashboardQuery.data.submittedCount} />
          <StatCard label="Graded" value={dashboardQuery.data.gradedCount} />
        </div>
      )}

      <div>
        <h2 className="text-lg font-medium">Upcoming deadlines</h2>
        {upcoming.length === 0 ? (
          <p className="text-neutral-500">No upcoming assignments.</p>
        ) : (
          <ul className="mt-2 max-w-xl space-y-2 text-sm">
            {upcoming.map((assignment) => (
              <li
                key={assignment.id}
                className="flex items-center justify-between border-b border-neutral-100 py-1"
              >
                <Link
                  href={`/student/assignments/${assignment.id}`}
                  className="underline underline-offset-4"
                >
                  {assignment.title}
                </Link>
                <span className="text-neutral-500">
                  {new Date(assignment.deadlineUtc).toLocaleString()}
                </span>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
