"use client";

import { BookOpen, LayoutDashboard, School, Users } from "lucide-react";
import { AppShell } from "@/components/layout/AppShell";

const navItems = [
  { href: "/admin", label: "Overview", icon: LayoutDashboard },
  { href: "/admin/users", label: "Users", icon: Users },
  { href: "/admin/classes", label: "Classes", icon: School },
  { href: "/admin/subjects", label: "Subjects", icon: BookOpen },
];

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <AppShell allowedRoles={["Admin"]} navItems={navItems} roleLabel="Admin">
      {children}
    </AppShell>
  );
}
