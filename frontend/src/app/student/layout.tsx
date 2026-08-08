"use client";

import { AppShell } from "@/components/layout/AppShell";

const navItems = [
  { href: "/student", label: "Overview" },
  { href: "/student/assignments", label: "Assignments" },
];

export default function StudentLayout({ children }: { children: React.ReactNode }) {
  return (
    <AppShell allowedRoles={["Student"]} navItems={navItems} roleLabel="Student">
      {children}
    </AppShell>
  );
}
