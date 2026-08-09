"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { ChevronsUpDown, GraduationCap, LogOut, type LucideIcon } from "lucide-react";
import { useAuth } from "@/features/auth/AuthContext";
import { RoleGuard } from "@/components/layout/RoleGuard";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
} from "@/components/ui/sidebar";
import { Separator } from "@/components/ui/separator";
import type { UserRole } from "@/types/auth";

interface NavItem {
  href: string;
  label: string;
  icon: LucideIcon;
}

interface AppShellProps {
  allowedRoles: UserRole[];
  navItems: NavItem[];
  roleLabel: string;
  children: React.ReactNode;
}

function getInitials(name?: string) {
  if (!name) return "?";
  const parts = name.trim().split(/\s+/);
  const initials = parts
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
  return initials || "?";
}

export function AppShell({ allowedRoles, navItems, roleLabel, children }: AppShellProps) {
  const pathname = usePathname();
  const { user, logout } = useAuth();

  const isActive = (href: string) => (href === navItems[0]?.href ? pathname === href : pathname.startsWith(href));

  const activeItem = [...navItems].reverse().find((item) => isActive(item.href)) ?? navItems[0];
  const isRoot = activeItem?.href === navItems[0]?.href;

  return (
    <RoleGuard allowedRoles={allowedRoles}>
      <SidebarProvider>
        <Sidebar collapsible="icon">
          <SidebarHeader>
            <SidebarMenu>
              <SidebarMenuItem>
                <SidebarMenuButton size="lg" render={<Link href={navItems[0]?.href ?? "/"} />}>
                  <span className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                    <GraduationCap className="size-4" />
                  </span>
                  <div className="grid flex-1 text-left leading-tight">
                    <span className="truncate text-sm font-semibold">Assignment Hub</span>
                    <span className="truncate text-xs text-muted-foreground">{roleLabel}</span>
                  </div>
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarHeader>

          <SidebarContent>
            <SidebarGroup>
              <SidebarMenu>
                {navItems.map((item) => (
                  <SidebarMenuItem key={item.href}>
                    <SidebarMenuButton
                      isActive={isActive(item.href)}
                      tooltip={item.label}
                      render={<Link href={item.href} />}
                    >
                      <item.icon />
                      <span>{item.label}</span>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroup>
          </SidebarContent>

          <SidebarFooter>
            <SidebarMenu>
              <SidebarMenuItem>
                <DropdownMenu>
                  <DropdownMenuTrigger
                    render={
                      <SidebarMenuButton
                        size="lg"
                        className="data-open:bg-sidebar-accent data-open:text-sidebar-accent-foreground"
                      />
                    }
                  >
                    <Avatar size="sm" className="rounded-lg">
                      <AvatarFallback className="rounded-lg">
                        {getInitials(user?.fullName)}
                      </AvatarFallback>
                    </Avatar>
                    <div className="grid flex-1 text-left leading-tight group-data-[collapsible=icon]:hidden">
                      <span className="truncate text-sm font-medium">{user?.fullName}</span>
                      <span className="truncate text-xs text-muted-foreground">{roleLabel}</span>
                    </div>
                    <ChevronsUpDown className="ml-auto size-4 text-muted-foreground group-data-[collapsible=icon]:hidden" />
                  </DropdownMenuTrigger>
                  <DropdownMenuContent side="top" align="start" className="w-56">
                    <DropdownMenuGroup>
                      <DropdownMenuLabel>
                        <div className="flex flex-col">
                          <span className="text-sm font-medium text-foreground">
                            {user?.fullName}
                          </span>
                          <span className="text-xs text-muted-foreground">
                            {roleLabel} account
                          </span>
                        </div>
                      </DropdownMenuLabel>
                    </DropdownMenuGroup>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem variant="destructive" onClick={() => logout()}>
                      <LogOut />
                      Log out
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarFooter>
        </Sidebar>

        <SidebarInset>
          <header className="sticky top-0 z-40 flex h-14 shrink-0 items-center gap-2 border-b bg-background/90 px-4 backdrop-blur">
            <SidebarTrigger className="-ml-1" />
            <Separator orientation="vertical" className="h-4" />
            <Breadcrumb>
              <BreadcrumbList>
                <BreadcrumbItem>
                  {isRoot ? (
                    <BreadcrumbPage>{roleLabel}</BreadcrumbPage>
                  ) : (
                    <BreadcrumbLink render={<Link href={navItems[0]?.href ?? "/"} />}>
                      {roleLabel}
                    </BreadcrumbLink>
                  )}
                </BreadcrumbItem>
                {!isRoot && activeItem && (
                  <>
                    <BreadcrumbSeparator />
                    <BreadcrumbItem>
                      <BreadcrumbPage>{activeItem.label}</BreadcrumbPage>
                    </BreadcrumbItem>
                  </>
                )}
              </BreadcrumbList>
            </Breadcrumb>
          </header>

          <main className="flex flex-1 flex-col bg-muted/30 p-4 sm:p-6">
            <div className="mx-auto w-full max-w-[1600px] flex-1">{children}</div>
          </main>
        </SidebarInset>
      </SidebarProvider>
    </RoleGuard>
  );
}
