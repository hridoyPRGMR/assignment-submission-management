"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Loader2, Plus, Search } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { classesApi } from "@/features/classes/classesApi";
import type { SchoolClass } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { InputGroup, InputGroupAddon, InputGroupInput } from "@/components/ui/input-group";
import { Drawer, DrawerClose, DrawerContent, DrawerDescription, DrawerFooter, DrawerHeader, DrawerTitle, DrawerTrigger } from "@/components/ui/drawer";
import { toast } from "@/components/ui/toast";
import { Pagination, PaginationContent, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from "@/components/ui/pagination";
import { DataTable } from "./data-table";
import { getColumns } from "./columns";

const classSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
});

type ClassFormValues = z.infer<typeof classSchema>;

export default function AdminClassesPage() {
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [sort, setSort] = useState("");
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<SchoolClass | null>(null);
  const [page, setPage] = useState(1);
  const pageSize = 10;

  useEffect(() => {
    const timeout = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 300);
    return () => clearTimeout(timeout);
  }, [search]);

  const { data, isLoading } = useQuery({
    queryKey: ["classes", page, pageSize, debouncedSearch, sort],
    queryFn: () => classesApi.list(page, pageSize, debouncedSearch, sort),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => classesApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["classes"] }),
    onError: () =>
      setErrorMessage("Cannot delete a class that has assignments or enrolled students."),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ClassFormValues>({ resolver: zodResolver(classSchema) });

  const columns = useMemo(
    () =>
      getColumns({
        sort,
        setSort,
        onDelete: (id) => deleteMutation.mutate(id),
        onEdit: (classItem) => {
          setEditing(classItem);
          reset({ name: classItem.name, description: classItem.description ?? "" });
          setOpen(true);
        },
      }),
    [sort, deleteMutation, reset]
  );

  const createMutation = useMutation({
    mutationFn: (values: ClassFormValues) =>
      editing
        ? classesApi.update(editing.id, values)
        : classesApi.create(values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
      reset();
      setOpen(false);
      setEditing(null);
      toast.add({
        type: "success",
        title: editing ? "Class updated" : "Class created",
      })
    },
  });

  const isEmpty = data != null && data.items.length === 0;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Classes</h1>
        <p className="text-sm text-muted-foreground">
          Manage school classes and enrollments.
        </p>
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="w-full sm:w-72">
          <InputGroup>
            <InputGroupAddon align="inline-start">
              <Search />
            </InputGroupAddon>
            <InputGroupInput
              placeholder="Search classes..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </InputGroup>
        </div>

        <Drawer swipeDirection="right" open={open} onOpenChange={(value) => {
          setOpen(value);
          if (!value) {
            reset();
            setEditing(null);
          }
        }}>
          <DrawerTrigger
            render={<Button />}
            onClick={() => {
              reset();
              setEditing(null);
            }}
          >
            <Plus />
            Create Class
          </DrawerTrigger>

          <DrawerContent className="w-full sm:max-w-120">
            <DrawerHeader>
              <DrawerTitle>{editing ? "Edit Class" : "Create Class"}</DrawerTitle>
              <DrawerDescription>
                {editing
                  ? "Update the class details."
                  : "Create a new class."}
              </DrawerDescription>
            </DrawerHeader>

            <form
              onSubmit={handleSubmit((values) => createMutation.mutate(values))}
              className="flex flex-1 flex-col gap-6 p-6"
            >
              <Field>
                <Label>Name</Label>

                <Input
                  placeholder="e.g. Grade 10 A"
                  {...register("name")}
                />

                {errors.name && (
                  <p className="text-sm text-destructive">
                    {errors.name.message}
                  </p>
                )}
              </Field>

              <Field>
                <Label>Description</Label>

                <Textarea
                  rows={4}
                  placeholder="Enter class description..."
                  {...register("description")}
                />

                {errors.description && (
                  <p className="text-sm text-destructive">
                    {errors.description.message}
                  </p>
                )}
              </Field>

              <DrawerFooter className="mt-auto px-0 pb-0">
                <div className="flex justify-end gap-2">
                  <DrawerClose render={<Button variant="outline" />}>
                    Cancel
                  </DrawerClose>

                  <Button
                    type="submit"
                    disabled={isSubmitting || createMutation.isPending}
                  >
                    {createMutation.isPending
                      ? editing
                        ? "Saving..."
                        : "Creating..."
                      : editing
                        ? "Save Changes"
                        : "Create Class"}
                  </Button>
                </div>
              </DrawerFooter>
            </form>
          </DrawerContent>
        </Drawer>
      </div>

      {errorMessage && (
        <div className="rounded-lg border border-destructive/20 bg-destructive/10 px-4 py-3 text-sm text-destructive">
          {errorMessage}
        </div>
      )}

      {isLoading ? (
        <div className="flex flex-1 flex-col items-center justify-center gap-2 py-24">
          <Loader2 className="size-5 animate-spin text-muted-foreground" />
          <p className="text-sm text-muted-foreground">Loading classes...</p>
        </div>
      ) : isEmpty ? (
        <div className="flex flex-col items-center justify-center gap-1 rounded-lg border border-dashed bg-card px-4 py-24 text-center shadow-sm">
          <p className="text-sm font-medium text-foreground">
            {search ? "No classes found" : "No classes yet"}
          </p>
          <p className="text-sm text-muted-foreground">
            {search
              ? "Try adjusting your search to find what you're looking for."
              : "Create your first class to get started."}
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
                  <PaginationPrevious
                    onClick={() => page > 1 && setPage(page - 1)}
                  />
                </PaginationItem>

                {Array.from(
                  { length: data?.totalPages ?? 0 },
                  (_, i) => (
                    <PaginationItem key={i}>
                      <PaginationLink
                        isActive={page === i + 1}
                        onClick={() => setPage(i + 1)}
                      >
                        {i + 1}
                      </PaginationLink>
                    </PaginationItem>
                  )
                )}

                <PaginationItem>
                  <PaginationNext
                    onClick={() =>
                      page < (data?.totalPages ?? 1) &&
                      setPage(page + 1)
                    }
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
