"use client";

import { ClipboardList, LayoutDashboard } from "lucide-react";
import { AppShell } from "@/components/layout/AppShell";

const navItems = [
  { href: "/teacher", label: "Overview", icon: LayoutDashboard },
  { href: "/teacher/assignments", label: "Assignments", icon: ClipboardList },
];

export default function TeacherLayout({ children }: { children: React.ReactNode }) {
  return (
    <AppShell allowedRoles={["Teacher"]} navItems={navItems} roleLabel="Teacher">
      {children}
    </AppShell>
  );
}
