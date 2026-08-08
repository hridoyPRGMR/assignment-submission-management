"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2, Plus, Search } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import { usersApi } from "@/features/users/usersApi";
import type { UserRole } from "@/types/auth";
import { Button } from "@/components/ui/button";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { InputGroup, InputGroupAddon, InputGroupInput } from "@/components/ui/input-group";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { toast } from "@/components/ui/toast";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { DataTable } from "./data-table";
import { getColumns } from "./columns";

const createUserSchema = z.object({
  email: z.string().min(1, "Email is required").email("Enter a valid email address"),
  password: z.string().min(8, "Password must be at least 8 characters"),
  fullName: z.string().min(1, "Full name is required"),
  role: z.enum(["Admin", "Teacher", "Student"]),
});

type CreateUserFormValues = z.infer<typeof createUserSchema>;

const roles: { label: string; value: UserRole }[] = [
  { label: "Admin", value: "Admin" },
  { label: "Teacher", value: "Teacher" },
  { label: "Student", value: "Student" },
];

const roleFilters: { label: string; value: string }[] = [
  { label: "All roles", value: "all" },
  ...roles,
];

export default function AdminUsersPage() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [page, setPage] = useState(1);
  const [roleFilter, setRoleFilter] = useState("all");
  const [pendingId, setPendingId] = useState<string | null>(null);
  const pageSize = 10;

  useEffect(() => {
    const timeout = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 300);
    return () => clearTimeout(timeout);
  }, [search]);

  const { data, isLoading } = useQuery({
    queryKey: ["users", page, pageSize, roleFilter, debouncedSearch],
    queryFn: () =>
      usersApi.listPaged(
        page,
        pageSize,
        roleFilter === "all" ? undefined : roleFilter,
        debouncedSearch,
      ),
  });

  const {
    control,
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<CreateUserFormValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: { role: "Student" },
  });

  const createMutation = useMutation({
    mutationFn: (values: CreateUserFormValues) => usersApi.create(values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      reset();
      setOpen(false);
      toast.add({ type: "success", title: "User created" });
    },
    onError: () => {
      setError("email", { message: "The email may already be in use." });
      toast.add({ type: "error", title: "Could not create user" });
    },
  });

  const statusMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      usersApi.setActiveStatus(id, isActive),
    onMutate: ({ id }) => setPendingId(id),
    onSettled: () => setPendingId(null),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["users"] }),
  });

  const columns = useMemo(
    () =>
      getColumns({
        onToggleActive: (user) => statusMutation.mutate({ id: user.id, isActive: !user.isActive }),
        pendingId,
      }),
    [pendingId, statusMutation],
  );

  const isEmpty = data != null && data.items.length === 0;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Users</h1>
        <p className="text-sm text-muted-foreground">Manage user accounts and access.</p>
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <div className="w-full sm:w-72">
            <InputGroup>
              <InputGroupAddon align="inline-start">
                <Search />
              </InputGroupAddon>
              <InputGroupInput
                placeholder="Search by name or email..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </InputGroup>
          </div>

          <Select
            value={roleFilter}
            onValueChange={(value) => {
              setRoleFilter(value ?? "all");
              setPage(1);
            }}
            items={roleFilters}
          >
            <SelectTrigger className="w-full sm:w-52">
              <SelectValue placeholder="Filter by role..." />
            </SelectTrigger>
            <SelectContent>
              {roleFilters.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <Drawer
          swipeDirection="right"
          open={open}
          onOpenChange={(value) => {
            setOpen(value);
            if (!value) reset();
          }}
        >
          <DrawerTrigger render={<Button />} onClick={() => reset()}>
            <Plus />
            Create User
          </DrawerTrigger>

          <DrawerContent className="w-full sm:max-w-120">
            <DrawerHeader>
              <DrawerTitle>Create User</DrawerTitle>
              <DrawerDescription>Create a new user account.</DrawerDescription>
            </DrawerHeader>

            <form
              onSubmit={handleSubmit((values) => createMutation.mutate(values))}
              className="flex flex-1 flex-col gap-6 p-6"
            >
              <Field>
                <Label>Full name</Label>

                <Input placeholder="e.g. Jane Doe" {...register("fullName")} />

                {errors.fullName && (
                  <p className="text-sm text-destructive">{errors.fullName.message}</p>
                )}
              </Field>

              <Field>
                <Label>Email</Label>

                <Input type="email" placeholder="e.g. jane@example.com" {...register("email")} />

                {errors.email && <p className="text-sm text-destructive">{errors.email.message}</p>}
              </Field>

              <Field>
                <Label>Password</Label>

                <Input
                  type="password"
                  placeholder="At least 8 characters"
                  {...register("password")}
                />

                {errors.password && (
                  <p className="text-sm text-destructive">{errors.password.message}</p>
                )}
              </Field>

              <Field>
                <Label>Role</Label>

                <Controller
                  control={control}
                  name="role"
                  render={({ field }) => (
                    <Select
                      value={field.value}
                      onValueChange={(value) => field.onChange(value ?? "Student")}
                      items={roles}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select a role..." />
                      </SelectTrigger>
                      <SelectContent>
                        {roles.map((role) => (
                          <SelectItem key={role.value} value={role.value}>
                            {role.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />

                {errors.role && <p className="text-sm text-destructive">{errors.role.message}</p>}
              </Field>

              <DrawerFooter className="mt-auto px-0 pb-0">
                <div className="flex justify-end gap-2">
                  <DrawerClose render={<Button variant="outline" />}>Cancel</DrawerClose>

                  <Button type="submit" disabled={isSubmitting || createMutation.isPending}>
                    {createMutation.isPending ? "Creating..." : "Create User"}
                  </Button>
                </div>
              </DrawerFooter>
            </form>
          </DrawerContent>
        </Drawer>
      </div>

      {isLoading ? (
        <div className="flex flex-1 flex-col items-center justify-center gap-2 py-24">
          <Loader2 className="size-5 animate-spin text-muted-foreground" />
          <p className="text-sm text-muted-foreground">Loading users...</p>
        </div>
      ) : isEmpty ? (
        <div className="flex flex-col items-center justify-center gap-1 rounded-lg border border-dashed bg-card px-4 py-24 text-center shadow-sm">
          <p className="text-sm font-medium text-foreground">
            {debouncedSearch
              ? "No users found"
              : roleFilter !== "all"
                ? "No users found"
                : "No users yet"}
          </p>
          <p className="text-sm text-muted-foreground">
            {debouncedSearch
              ? "Try adjusting your search to find what you're looking for."
              : roleFilter !== "all"
                ? "Try a different role filter."
                : "Create your first user to get started."}
          </p>
        </div>
      ) : (
        <>
          <div className="overflow-hidden rounded-lg border bg-card shadow-sm">
            <DataTable columns={columns} data={data?.items ?? []} />
          </div>

          <div className="flex flex-col items-center justify-between gap-3 sm:flex-row">
            <p className="shrink-0 text-sm text-muted-foreground">
              Page {data?.page ?? 1} of {data?.totalPages ?? 1}
            </p>

            <Pagination className="mx-0 w-auto shrink-0 justify-center sm:justify-end">
              <PaginationContent>
                <PaginationItem>
                  <PaginationPrevious onClick={() => page > 1 && setPage(page - 1)} />
                </PaginationItem>

                {Array.from({ length: data?.totalPages ?? 0 }, (_, i) => (
                  <PaginationItem key={i}>
                    <PaginationLink isActive={page === i + 1} onClick={() => setPage(i + 1)}>
                      {i + 1}
                    </PaginationLink>
                  </PaginationItem>
                ))}

                <PaginationItem>
                  <PaginationNext
                    onClick={() => page < (data?.totalPages ?? 1) && setPage(page + 1)}
                  />
                </PaginationItem>
              </PaginationContent>
            </Pagination>
          </div>
        </>
      )}
    </div>
  );
}
