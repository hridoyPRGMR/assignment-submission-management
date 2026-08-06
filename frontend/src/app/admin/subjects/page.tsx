"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { subjectsApi } from "@/features/subjects/subjectsApi";
import { Button } from "@/components/ui/button";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Drawer, DrawerClose, DrawerContent, DrawerDescription, DrawerFooter, DrawerHeader, DrawerTitle, DrawerTrigger } from "@/components/ui/drawer";
import { toast } from "@/components/ui/toast";
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
  const [sort, setSort] = useState("");
  const [open, setOpen] = useState(false);
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const { data, isLoading } = useQuery({
    queryKey: ["subjects", page, pageSize, search, sort],
    queryFn: () => subjectsApi.list(page, pageSize, search, sort),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => subjectsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["subjects"] }),
    onError: () => setErrorMessage("Cannot delete a subject that has existing assignments."),
  });

  const columns = useMemo(
    () =>
      getColumns({
        sort,
        setSort,
        onDelete: (id) => deleteMutation.mutate(id),
      }),
    [sort, deleteMutation]
  );

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<SubjectFormValues>({ resolver: zodResolver(subjectSchema) });

  const createMutation = useMutation({
    mutationFn: (values: SubjectFormValues) => subjectsApi.create(values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["subjects"] });
      reset();
      setOpen(false);
      toast.add({
        type: "success",
        title: "Subject created",
      })
    },
  });


  return (
    <div className="flex flex-1 flex-col gap-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Subjects</h1>

        <Drawer swipeDirection="right" open={open} onOpenChange={setOpen}>
          <DrawerTrigger render={<Button />}>
            Create Subject
          </DrawerTrigger>

          <DrawerContent className="w-full sm:max-w-120">
            <DrawerHeader>
              <DrawerTitle>Create Subject</DrawerTitle>
              <DrawerDescription>
                Create a new subject for assignments.
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
                      ? "Creating..."
                      : "Create Subject"}
                  </Button>
                </div>
              </DrawerFooter>
            </form>
          </DrawerContent>
        </Drawer>
      </div>


      {errorMessage && <p className="text-sm text-red-600">{errorMessage}</p>}

      {isLoading ? (
        <p className="text-neutral-500">Loading subjects...</p>
      ) : data?.items.length === 0 ? (
        <p className="text-neutral-500">No subjects yet.</p>
      ) : (
        <div className="w-full overflow-x-auto">
          {/* <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Description</TableHead>
                <TableHead className="w-30">
                  Action
                </TableHead>
              </TableRow>
            </TableHeader>

            <TableBody>
              {data?.items.map((subject) => (
                <TableRow key={subject.id}>
                  <TableCell>{subject.name}</TableCell>

                  <TableCell>
                    {subject.description || "-"}
                  </TableCell>

                  <TableCell>
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() =>
                        deleteMutation.mutate(subject.id)
                      }
                    >
                      Delete
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table> */}
          <Input
            placeholder="Search subjects..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
          />
          <DataTable columns={columns} data={data?.items ?? []} />
          <Pagination>
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
      )}
    </div>
  );
}
