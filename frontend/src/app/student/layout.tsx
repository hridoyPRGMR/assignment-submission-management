"use client";

import { ClipboardList, LayoutDashboard } from "lucide-react";
import { AppShell } from "@/components/layout/AppShell";

const navItems = [
  { href: "/student", label: "Overview", icon: LayoutDashboard },
  { href: "/student/assignments", label: "Assignments", icon: ClipboardList },
];

export default function StudentLayout({ children }: { children: React.ReactNode }) {
  return (
    <AppShell allowedRoles={["Student"]} navItems={navItems} roleLabel="Student">
      {children}
    </AppShell>
  );
}
