"use client";

import { AppShell } from "@/components/layout/AppShell";

const navItems = [
  { href: "/teacher", label: "Overview" },
  { href: "/teacher/assignments", label: "Assignments" },
];

export default function TeacherLayout({ children }: { children: React.ReactNode }) {
  return (
    <AppShell allowedRoles={["Teacher"]} navItems={navItems} roleLabel="Teacher">
      {children}
    </AppShell>
  );
}
