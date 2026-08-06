"use client";

import { useQuery } from "@tanstack/react-query";
import { StatCard } from "@/components/ui/StatCard";
import { dashboardApi } from "@/features/dashboard/dashboardApi";
import { useAuth } from "@/features/auth/AuthContext";

export default function AdminDashboardPage() {
  const { user } = useAuth();
  const { data, isLoading, isError } = useQuery({
    queryKey: ["dashboard", "admin"],
    queryFn: () => dashboardApi.getAdmin(),
  });

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Admin Dashboard</h1>
        <p className="text-neutral-500">Welcome, {user?.fullName}.</p>
      </div>

      {isLoading && <p className="text-neutral-500">Loading dashboard...</p>}
      {isError && <p className="text-red-600">Could not load dashboard data.</p>}

      {data && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
          <StatCard label="Users" value={data.totalUsers} />
          <StatCard label="Teachers" value={data.totalTeachers} />
          <StatCard label="Students" value={data.totalStudents} />
          <StatCard label="Classes" value={data.totalClasses} />
          <StatCard label="Subjects" value={data.totalSubjects} />
          <StatCard label="Assignments" value={data.totalAssignments} />
          <StatCard label="Published" value={data.publishedAssignments} />
          <StatCard label="Draft" value={data.draftAssignments} />
          <StatCard label="Submissions" value={data.totalSubmissions} />
          <StatCard label="Graded" value={data.gradedSubmissions} />
        </div>
      )}
    </div>
  );
}
