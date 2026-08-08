"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { GraduationCap, LogOut } from "lucide-react";
import { useAuth } from "@/features/auth/AuthContext";
import { RoleGuard } from "@/components/layout/RoleGuard";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import type { UserRole } from "@/types/auth";

interface AppShellProps {
  allowedRoles: UserRole[];
  navItems: { href: string; label: string }[];
  roleLabel: string;
  children: React.ReactNode;
}

export function AppShell({ allowedRoles, navItems, roleLabel, children }: AppShellProps) {
  const pathname = usePathname();
  const { user, logout } = useAuth();

  const isActive = (href: string) =>
    href === "/admin" || href === "/teacher" || href === "/student"
      ? pathname === href
      : pathname.startsWith(href);

  return (
    <RoleGuard allowedRoles={allowedRoles}>
      <div className="flex min-h-dvh flex-col bg-muted/30">
        <header className="sticky top-0 z-40 border-b bg-background/90 backdrop-blur">
          <div className="mx-auto flex w-full max-w-6xl flex-wrap items-center justify-between gap-3 px-4 py-3 sm:px-6">
            <div className="flex flex-wrap items-center gap-4 sm:gap-6">
              <Link
                href={`/${roleLabel.toLowerCase()}`}
                className="flex items-center gap-2 text-sm font-semibold tracking-tight"
              >
                <span className="flex size-7 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                  <GraduationCap className="size-4" />
                </span>
                Assignment Hub
              </Link>

              <nav className="flex flex-wrap items-center gap-1">
                {navItems.map((item) => (
                  <Link
                    key={item.href}
                    href={item.href}
                    className={cn(
                      "rounded-md px-3 py-1.5 text-sm font-medium transition-colors",
                      isActive(item.href)
                        ? "bg-muted text-foreground"
                        : "text-muted-foreground hover:text-foreground",
                    )}
                  >
                    {item.label}
                  </Link>
                ))}
              </nav>
            </div>

            <div className="flex items-center gap-3">
              <div className="hidden flex-col items-end leading-tight sm:flex">
                <span className="text-sm font-medium">{user?.fullName}</span>
                <span className="text-xs text-muted-foreground">{roleLabel}</span>
              </div>
              <Button variant="outline" size="sm" onClick={() => logout()}>
                <LogOut />
                Log out
              </Button>
            </div>
          </div>
        </header>

        <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-6 sm:px-6 sm:py-8">
          {children}
        </main>
      </div>
    </RoleGuard>
  );
}
