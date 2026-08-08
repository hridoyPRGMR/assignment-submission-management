"use client";

import { AppShell } from "@/components/layout/AppShell";

const navItems = [
  { href: "/admin", label: "Overview" },
  { href: "/admin/users", label: "Users" },
  { href: "/admin/classes", label: "Classes" },
  { href: "/admin/subjects", label: "Subjects" },
];

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <AppShell allowedRoles={["Admin"]} navItems={navItems} roleLabel="Admin">
      {children}
    </AppShell>
  );
}
