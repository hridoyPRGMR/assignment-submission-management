"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ArrowLeft, Loader2, Trash2 } from "lucide-react";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { AssignmentStatusBadge } from "@/components/status-badge";
import { toast } from "@/components/ui/toast";
import { formatDateTime } from "@/lib/format";

const assignmentSchema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().min(1, "Description is required"),
  deadline: z.string().min(1, "Deadline is required"),
  maxMarks: z.coerce.number().int().positive("Must be greater than zero"),
});

type AssignmentFormValues = z.input<typeof assignmentSchema>;
type AssignmentFormOutput = z.output<typeof assignmentSchema>;

function toLocalInputValue(isoDate: string): string {
  const date = new Date(isoDate);
  const offsetMs = date.getTimezoneOffset() * 60 * 1000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

export default function TeacherAssignmentDetailPage() {
  const params = useParams<{ id: string }>();
  const assignmentId = params.id;
  const queryClient = useQueryClient();
  const router = useRouter();

  const assignmentQuery = useQuery({
    queryKey: ["assignments", assignmentId],
    queryFn: () => assignmentsApi.getById(assignmentId),
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<AssignmentFormValues, unknown, AssignmentFormOutput>({
    resolver: zodResolver(assignmentSchema),
  });

  useEffect(() => {
    if (assignmentQuery.data) {
      reset({
        title: assignmentQuery.data.title,
        description: assignmentQuery.data.description,
        deadline: toLocalInputValue(assignmentQuery.data.deadlineUtc),
        maxMarks: assignmentQuery.data.maxMarks,
      });
    }
  }, [assignmentQuery.data, reset]);

  const updateMutation = useMutation({
    mutationFn: (values: AssignmentFormOutput) =>
      assignmentsApi.update(assignmentId, {
        title: values.title,
        description: values.description,
        deadlineUtc: new Date(values.deadline).toISOString(),
        maxMarks: values.maxMarks,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments", assignmentId] });
      queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] });
      toast.add({ type: "success", title: "Assignment updated" });
    },
    onError: () =>
      toast.add({
        type: "error",
        title: "Could not save changes",
        description: "The deadline and max marks cannot be changed once submissions exist.",
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: () => assignmentsApi.remove(assignmentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments", "mine"] });
      toast.add({ type: "success", title: "Assignment deleted" });
      router.push("/teacher/assignments");
    },
    onError: () => toast.add({ type: "error", title: "Could not delete assignment" }),
  });

  if (assignmentQuery.isLoading) {
    return (
      <div className="flex flex-1 flex-col items-center justify-center gap-2 py-24">
        <Loader2 className="size-5 animate-spin text-muted-foreground" />
        <p className="text-sm text-muted-foreground">Loading assignment...</p>
      </div>
    );
  }

  const assignment = assignmentQuery.data;

  return (
    <div className="flex flex-1 flex-col gap-6">
      <Button
        variant="ghost"
        size="sm"
        nativeButton={false}
        className="-ml-2 w-fit"
        render={<Link href="/teacher/assignments" />}
      >
        <ArrowLeft />
        Back to Assignments
      </Button>

      <div className="flex flex-col gap-1">
        <div className="flex flex-wrap items-center gap-2">
          <h1 className="text-2xl font-semibold tracking-tight">{assignment?.title}</h1>
          {assignment && <AssignmentStatusBadge status={assignment.lifecycleStatus} />}
        </div>
        <p className="text-sm text-muted-foreground">
          {assignment?.className} · {assignment?.subjectName} · Due{" "}
          {assignment && formatDateTime(assignment.deadlineUtc)}
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Edit assignment</CardTitle>
          <CardDescription>
            Update the assignment details. Once submissions exist, the deadline and max marks become
            locked.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form
            onSubmit={handleSubmit((values) => updateMutation.mutate(values))}
            className="grid grid-cols-1 gap-6 sm:grid-cols-2"
          >
            <Field className="sm:col-span-2">
              <Label>Title</Label>
              <Input {...register("title")} />
              {errors.title && <p className="text-sm text-destructive">{errors.title.message}</p>}
            </Field>

            <Field className="sm:col-span-2">
              <Label>Description</Label>
              <Textarea rows={5} {...register("description")} />
              {errors.description && (
                <p className="text-sm text-destructive">{errors.description.message}</p>
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
              <Input type="number" min={1} {...register("maxMarks")} />
              {errors.maxMarks && (
                <p className="text-sm text-destructive">{errors.maxMarks.message}</p>
              )}
            </Field>

            <div className="flex items-center gap-3 sm:col-span-2">
              <Button type="submit" disabled={isSubmitting || updateMutation.isPending}>
                {updateMutation.isPending ? "Saving..." : "Save changes"}
              </Button>

              <Button
                type="button"
                variant="destructive"
                onClick={() => {
                  if (window.confirm("Delete this assignment? This cannot be undone.")) {
                    deleteMutation.mutate();
                  }
                }}
                disabled={deleteMutation.isPending}
              >
                <Trash2 />
                {deleteMutation.isPending ? "Deleting..." : "Delete assignment"}
              </Button>

              <Button
                variant="outline"
                nativeButton={false}
                className="ml-auto"
                render={<Link href={`/teacher/assignments/${assignmentId}/submissions`} />}
              >
                View submissions
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
