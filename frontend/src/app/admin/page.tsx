"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import {
  BookOpen,
  CheckCircle2,
  ClipboardList,
  FileText,
  GraduationCap,
  Loader2,
  School,
  Send,
  Users,
} from "lucide-react";
import { dashboardApi } from "@/features/dashboard/dashboardApi";
import { useAuth } from "@/features/auth/AuthContext";
import { StatCard } from "@/components/ui/StatCard";
import { Button } from "@/components/ui/button";

export default function AdminDashboardPage() {
  const { user } = useAuth();
  const { data, isLoading, isError } = useQuery({
    queryKey: ["dashboard", "admin"],
    queryFn: () => dashboardApi.getAdmin(),
  });

  return (
    <div className="flex flex-1 flex-col gap-8">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Welcome back, {user?.fullName}</h1>
        <p className="text-sm text-muted-foreground">
          A birds-eye view of users, classes, and assignment activity.
        </p>
      </div>

      {isLoading ? (
        <div className="flex items-center gap-2 py-8 text-sm text-muted-foreground">
          <Loader2 className="size-4 animate-spin" />
          Loading dashboard...
        </div>
      ) : isError ? (
        <div className="rounded-lg border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          Could not load dashboard data. Please try again.
        </div>
      ) : (
        data && (
          <>
            <div className="flex flex-col gap-3">
              <h2 className="text-sm font-medium text-muted-foreground">People &amp; classes</h2>
              <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
                <StatCard label="Users" value={data.totalUsers} icon={Users} tone="blue" />
                <StatCard
                  label="Teachers"
                  value={data.totalTeachers}
                  icon={GraduationCap}
                  tone="violet"
                />
                <StatCard
                  label="Students"
                  value={data.totalStudents}
                  icon={Users}
                  tone="emerald"
                />
                <StatCard label="Classes" value={data.totalClasses} icon={School} />
              </div>
            </div>

            <div className="flex flex-col gap-3">
              <h2 className="text-sm font-medium text-muted-foreground">Assignments</h2>
              <div className="grid grid-cols-2 gap-4 lg:grid-cols-5">
                <StatCard label="Subjects" value={data.totalSubjects} icon={BookOpen} />
                <StatCard
                  label="Assignments"
                  value={data.totalAssignments}
                  icon={ClipboardList}
                  tone="blue"
                />
                <StatCard label="Draft" value={data.draftAssignments} icon={FileText} />
                <StatCard
                  label="Published"
                  value={data.publishedAssignments}
                  icon={Send}
                  tone="emerald"
                />
                <StatCard
                  label="Submissions"
                  value={data.totalSubmissions}
                  icon={CheckCircle2}
                  tone="violet"
                />
              </div>
              <div className="grid grid-cols-2 gap-4 lg:grid-cols-5">
                <StatCard
                  label="Graded submissions"
                  value={data.gradedSubmissions}
                  icon={CheckCircle2}
                  tone="amber"
                />
              </div>
            </div>
          </>
        )
      )}

      <div className="flex flex-col gap-3">
        <div className="flex flex-col gap-0.5">
          <h2 className="text-lg font-semibold tracking-tight">Quick actions</h2>
          <p className="text-sm text-muted-foreground">Jump straight into common admin tasks.</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <Button nativeButton={false} render={<Link href="/admin/users" />}>
            <Users />
            Manage users
          </Button>
          <Button variant="outline" nativeButton={false} render={<Link href="/admin/classes" />}>
            <School />
            Manage classes
          </Button>
          <Button variant="outline" nativeButton={false} render={<Link href="/admin/subjects" />}>
            <BookOpen />
            Manage subjects
          </Button>
        </div>
      </div>
    </div>
  );
}
