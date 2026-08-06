"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { LogoutButton } from "@/components/layout/LogoutButton";
import { RoleGuard } from "@/components/layout/RoleGuard";

const navItems = [
  { href: "/student", label: "Overview" },
  { href: "/student/assignments", label: "Assignments" },
];

export default function StudentLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();

  return (
    <RoleGuard allowedRoles={["Student"]}>
      <div className="flex flex-1 flex-col">
        <header className="flex flex-wrap items-center justify-between gap-3 border-b border-neutral-200 px-4 py-4 sm:px-8">
          <nav className="flex flex-wrap gap-3 text-sm font-medium sm:gap-4">
            {navItems.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className={
                  pathname === item.href
                    ? "text-neutral-900 underline underline-offset-4"
                    : "text-neutral-500 hover:text-neutral-900"
                }
              >
                {item.label}
              </Link>
            ))}
          </nav>
          <LogoutButton />
        </header>
        <div className="flex flex-1 flex-col p-4 sm:p-8">{children}</div>
      </div>
    </RoleGuard>
  );
}
