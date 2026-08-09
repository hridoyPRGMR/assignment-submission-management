"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Search } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { subjectsApi } from "@/features/subjects/subjectsApi";
import type { Subject } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { InputGroup, InputGroupAddon, InputGroupInput } from "@/components/ui/input-group";
import { Drawer, DrawerClose, DrawerContent, DrawerDescription, DrawerFooter, DrawerHeader, DrawerTitle, DrawerTrigger } from "@/components/ui/drawer";
import { toast } from "@/components/ui/toast";
import { EmptyState } from "@/components/ui/empty-state";
import { TableSkeleton } from "@/components/ui/table-skeleton";
import { Pagination, PaginationContent, PaginationItem, PaginationLink, PaginationNext, PaginationPrevious } from "@/components/ui/pagination";
import { DataTable } from "./data-table";
import { getColumns } from "./columns";

const subjectSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
});

type SubjectFormValues = z.infer<typeof subjectSchema>;

export default function AdminSubjectsPage() {
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [sort, setSort] = useState("");
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Subject | null>(null);
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
    queryKey: ["subjects", page, pageSize, debouncedSearch, sort],
    queryFn: () => subjectsApi.list(page, pageSize, debouncedSearch, sort),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => subjectsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["subjects"] }),
    onError: () => setErrorMessage("Cannot delete a subject that has existing assignments."),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<SubjectFormValues>({ resolver: zodResolver(subjectSchema) });

  const columns = useMemo(
    () =>
      getColumns({
        sort,
        setSort,
        onDelete: (id) => deleteMutation.mutate(id),
        onEdit: (subject) => {
          setEditing(subject);
          reset({ name: subject.name, description: subject.description ?? "" });
          setOpen(true);
        },
      }),
    [sort, deleteMutation, reset]
  );

  const createMutation = useMutation({
    mutationFn: (values: SubjectFormValues) =>
      editing
        ? subjectsApi.update(editing.id, values)
        : subjectsApi.create(values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["subjects"] });
      reset();
      setOpen(false);
      setEditing(null);
      toast.add({
        type: "success",
        title: editing ? "Subject updated" : "Subject created",
      })
    },
  });

  const isEmpty = data != null && data.items.length === 0;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Subjects</h1>
        <p className="text-sm text-muted-foreground">
          Manage system subjects and curriculum specifications.
        </p>
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="w-full sm:w-72">
          <InputGroup>
            <InputGroupAddon align="inline-start">
              <Search />
            </InputGroupAddon>
            <InputGroupInput
              placeholder="Search subjects..."
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
            Create Subject
          </DrawerTrigger>

          <DrawerContent className="w-full sm:max-w-120">
            <DrawerHeader>
              <DrawerTitle>{editing ? "Edit Subject" : "Create Subject"}</DrawerTitle>
              <DrawerDescription>
                {editing
                  ? "Update the subject details for assignments."
                  : "Create a new subject for assignments."}
              </DrawerDescription>
            </DrawerHeader>

            <form
              onSubmit={handleSubmit((values) => createMutation.mutate(values))}
              className="flex flex-1 flex-col gap-6 p-6"
            >
              <Field>
                <Label>Name</Label>

                <Input
                  placeholder="e.g. Mathematics"
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
                  placeholder="Enter subject description..."
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
                        : "Create Subject"}
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
        <TableSkeleton columns={3} />
      ) : isEmpty ? (
        <EmptyState
          title={search ? "No subjects found" : "No subjects yet"}
          description={
            search
              ? "Try adjusting your search to find what you're looking for."
              : "Create your first subject to get started."
          }
        />
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
