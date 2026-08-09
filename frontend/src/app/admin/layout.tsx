"use client";

import { BookOpen, ClipboardList, LayoutDashboard, School, Settings, Users } from "lucide-react";
import { AppShell } from "@/components/layout/AppShell";

const navItems = [
  { href: "/admin", label: "Overview", icon: LayoutDashboard },
  { href: "/admin/users", label: "Users", icon: Users },
  { href: "/admin/classes", label: "Classes", icon: School },
  { href: "/admin/subjects", label: "Subjects", icon: BookOpen },
  { href: "/admin/assignments", label: "Assignments", icon: ClipboardList },
  { href: "/admin/settings", label: "Settings", icon: Settings },
];

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <AppShell allowedRoles={["Admin"]} navItems={navItems} roleLabel="Admin">
      {children}
    </AppShell>
  );
}
