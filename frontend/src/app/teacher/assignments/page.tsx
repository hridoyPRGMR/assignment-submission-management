"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient, type QueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import { Loader2, Plus } from "lucide-react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { teacherAssignmentsApi } from "@/features/teacherAssignments/teacherAssignmentsApi";
import { Button } from "@/components/ui/button";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
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
import { DataTable } from "./data-table";
import { getColumns } from "./columns";
import { cn } from "@/lib/utils";
import type { AssignmentLifecycleStatus } from "@/types/assignment";

const assignmentSchema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().min(1, "Description is required"),
  classSubject: z.string().min(1, "Select a class and subject"),
  deadline: z.string().min(1, "Deadline is required"),
  maxMarks: z.coerce.number().int().positive("Must be greater than zero"),
});

type AssignmentFormValues = z.input<typeof assignmentSchema>;
type AssignmentFormOutput = z.output<typeof assignmentSchema>;

const filters: { key: string; label: string }[] = [
  { key: "all", label: "All" },
  { key: "Draft", label: "Draft" },
  { key: "SubmissionOpen", label: "Open" },
  { key: "DeadlinePassed", label: "Deadline passed" },
  { key: "Graded", label: "Graded" },
  { key: "Archived", label: "Archived" },
];

function useAssignmentLifecycleMutation(
  queryClient: QueryClient,
  action: (id: string) => Promise<unknown>,
  onPendingChange: (id: string | null) => void,
  successTitle: string,
) {
  return useMutation({
    mutationFn: action,
    onMutate: (id: string) => onPendingChange(id),
    onSettled: () => onPendingChange(null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] });
      toast.add({ type: "success", title: successTitle });
    },
    onError: () =>
      toast.add({
        type: "error",
        title: "Action failed",
        description: "The assignment may not allow this action right now.",
      }),
  });
}

export default function TeacherAssignmentsPage() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [filter, setFilter] = useState("all");
  const [pendingId, setPendingId] = useState<string | null>(null);

  const assignmentsQuery = useQuery({
    queryKey: ["assignments", "mine"],
    queryFn: () => assignmentsApi.listMine(),
  });
  const teachingQuery = useQuery({
    queryKey: ["teacher-assignments", "mine"],
    queryFn: () => teacherAssignmentsApi.listMine(),
  });

  const classSubjectOptions = useMemo(() => {
    const seen = new Set<string>();
    return (teachingQuery.data ?? []).flatMap((ta) => {
      const value = `${ta.classId}|${ta.subjectId}`;
      if (seen.has(value)) return [];
      seen.add(value);
      return [{ label: `${ta.className} — ${ta.subjectName}`, value }];
    });
  }, [teachingQuery.data]);

  const {
    control,
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<AssignmentFormValues, unknown, AssignmentFormOutput>({
    resolver: zodResolver(assignmentSchema),
    defaultValues: { maxMarks: 100 },
  });

  const createMutation = useMutation({
    mutationFn: (values: AssignmentFormOutput) => {
      const [classId, subjectId] = values.classSubject.split("|");
      return assignmentsApi.create({
        title: values.title,
        description: values.description,
        classId,
        subjectId,
        deadlineUtc: new Date(values.deadline).toISOString(),
        maxMarks: values.maxMarks,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] });
      reset();
      setOpen(false);
      toast.add({ type: "success", title: "Assignment created" });
    },
    onError: () => toast.add({ type: "error", title: "Could not create assignment" }),
  });

  const publishMutation = useAssignmentLifecycleMutation(
    queryClient,
    (id) => assignmentsApi.publish(id),
    setPendingId,
    "Assignment published",
  );
  const unpublishMutation = useAssignmentLifecycleMutation(
    queryClient,
    (id) => assignmentsApi.unpublish(id),
    setPendingId,
    "Assignment unpublished",
  );
  const markGradedMutation = useAssignmentLifecycleMutation(
    queryClient,
    (id) => assignmentsApi.markGraded(id),
    setPendingId,
    "Assignment marked as graded",
  );
  const archiveMutation = useAssignmentLifecycleMutation(
    queryClient,
    (id) => assignmentsApi.archive(id),
    setPendingId,
    "Assignment archived",
  );

  const deleteMutation = useMutation({
    mutationFn: (id: string) => assignmentsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] });
      toast.add({ type: "success", title: "Assignment deleted" });
    },
    onError: () => toast.add({ type: "error", title: "Could not delete assignment" }),
  });

  const columns = useMemo(
    () =>
      getColumns({
        onPublish: (id) => publishMutation.mutate(id),
        onUnpublish: (id) => unpublishMutation.mutate(id),
        onMarkGraded: (id) => markGradedMutation.mutate(id),
        onArchive: (id) => archiveMutation.mutate(id),
        onDelete: (id) => {
          if (window.confirm("Delete this assignment? This cannot be undone.")) {
            deleteMutation.mutate(id);
          }
        },
        pendingId,
      }),
    [
      pendingId,
      publishMutation,
      unpublishMutation,
      markGradedMutation,
      archiveMutation,
      deleteMutation,
    ],
  );

  const allItems = assignmentsQuery.data?.items ?? [];
  const items =
    filter === "all"
      ? allItems
      : allItems.filter((a) => a.lifecycleStatus === (filter as AssignmentLifecycleStatus));
  const isEmpty = assignmentsQuery.data != null && allItems.length === 0;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Assignments</h1>
        <p className="text-sm text-muted-foreground">
          Create, publish, and manage assignments for your classes.
        </p>
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex flex-wrap items-center gap-2">
          {filters.map((filterOption) => (
            <button
              key={filterOption.key}
              onClick={() => setFilter(filterOption.key)}
              className={cn(
                "rounded-full border px-3 py-1 text-sm font-medium transition-colors",
                filter === filterOption.key
                  ? "border-primary bg-primary text-primary-foreground"
                  : "border-border text-muted-foreground hover:bg-muted hover:text-foreground",
              )}
            >
              {filterOption.label}
            </button>
          ))}
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
            Create Assignment
          </DrawerTrigger>

          <DrawerContent className="w-full sm:max-w-120">
            <DrawerHeader>
              <DrawerTitle>Create Assignment</DrawerTitle>
              <DrawerDescription>
                Create a new assignment for one of your classes.
              </DrawerDescription>
            </DrawerHeader>

            <form
              onSubmit={handleSubmit((values) => createMutation.mutate(values))}
              className="flex flex-1 flex-col gap-6 p-6"
            >
              <Field>
                <Label>Title</Label>
                <Input placeholder="e.g. Chapter 5 Exercises" {...register("title")} />
                {errors.title && <p className="text-sm text-destructive">{errors.title.message}</p>}
              </Field>

              <Field>
                <Label>Description</Label>
                <Textarea
                  rows={4}
                  placeholder="Describe the assignment..."
                  {...register("description")}
                />
                {errors.description && (
                  <p className="text-sm text-destructive">{errors.description.message}</p>
                )}
              </Field>

              <Field>
                <Label>Class & subject</Label>
                <Controller
                  control={control}
                  name="classSubject"
                  render={({ field }) => (
                    <Select
                      value={field.value}
                      onValueChange={(value) => field.onChange(value ?? "")}
                      items={classSubjectOptions}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select a class and subject..." />
                      </SelectTrigger>
                      <SelectContent>
                        {classSubjectOptions.length === 0 ? (
                          <SelectItem value="__none" disabled>
                            No classes assigned to you
                          </SelectItem>
                        ) : (
                          classSubjectOptions.map((option) => (
                            <SelectItem key={option.value} value={option.value}>
                              {option.label}
                            </SelectItem>
                          ))
                        )}
                      </SelectContent>
                    </Select>
                  )}
                />
                {errors.classSubject && (
                  <p className="text-sm text-destructive">{errors.classSubject.message}</p>
                )}
              </Field>

              <Field>
                <Label>Deadline</Label>
                <Input type="datetime-local" {...register("deadline")} />
                {errors.deadline && (
                  <p className="text-sm text-destructive">{errors.deadline.message}</p>
                )}
              </Field>

              <Field>
                <Label>Max marks</Label>
                <Input type="number" min={1} placeholder="e.g. 100" {...register("maxMarks")} />
                {errors.maxMarks && (
                  <p className="text-sm text-destructive">{errors.maxMarks.message}</p>
                )}
              </Field>

              <DrawerFooter className="mt-auto px-0 pb-0">
                <div className="flex justify-end gap-2">
                  <DrawerClose render={<Button variant="outline" />}>Cancel</DrawerClose>
                  <Button type="submit" disabled={isSubmitting || createMutation.isPending}>
                    {createMutation.isPending ? "Creating..." : "Create Assignment"}
                  </Button>
                </div>
              </DrawerFooter>
            </form>
          </DrawerContent>
        </Drawer>
      </div>

      {assignmentsQuery.isLoading ? (
        <div className="flex flex-col items-center justify-center gap-2 py-24">
          <Loader2 className="size-5 animate-spin text-muted-foreground" />
          <p className="text-sm text-muted-foreground">Loading assignments...</p>
        </div>
      ) : items.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-1 rounded-lg border border-dashed bg-card px-4 py-24 text-center shadow-sm">
          <p className="text-sm font-medium text-foreground">
            {isEmpty ? "No assignments yet" : "No assignments match this filter"}
          </p>
          <p className="text-sm text-muted-foreground">
            {isEmpty ? "Create your first assignment to get started." : "Try a different filter."}
          </p>
        </div>
      ) : (
        <div className="overflow-hidden rounded-lg border bg-card shadow-sm">
          <DataTable columns={columns} data={items} />
        </div>
      )}
    </div>
  );
}
