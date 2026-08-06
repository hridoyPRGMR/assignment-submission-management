"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { subjectsApi } from "@/features/subjects/subjectsApi";

const subjectSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional(),
});

type SubjectFormValues = z.infer<typeof subjectSchema>;

export default function AdminSubjectsPage() {
  const queryClient = useQueryClient();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["subjects"],
    queryFn: () => subjectsApi.list(),
  });

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
      reset({ name: "", description: "" });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => subjectsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["subjects"] }),
    onError: () => setErrorMessage("Cannot delete a subject that has existing assignments."),
  });

  return (
    <div className="flex flex-1 flex-col gap-8">
      <h1 className="text-2xl font-semibold">Subjects</h1>

      <form
        onSubmit={handleSubmit((values) => createMutation.mutate(values))}
        className="grid max-w-2xl grid-cols-1 gap-4 rounded-md border border-neutral-200 p-4 sm:grid-cols-2"
      >
        <div>
          <label className="block text-sm font-medium">Name</label>
          <input
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
            {...register("name")}
          />
          {errors.name && <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium">Description</label>
          <input
            className="mt-1 w-full rounded-md border border-neutral-300 px-3 py-2 text-sm"
            {...register("description")}
          />
        </div>

        <div className="sm:col-span-2">
          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-neutral-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {isSubmitting ? "Creating..." : "Create subject"}
          </button>
        </div>
      </form>

      {errorMessage && <p className="text-sm text-red-600">{errorMessage}</p>}

      {isLoading ? (
        <p className="text-neutral-500">Loading subjects...</p>
      ) : data?.items.length === 0 ? (
        <p className="text-neutral-500">No subjects yet.</p>
      ) : (
        <div className="max-w-3xl overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-neutral-200 text-left">
                <th className="py-2">Name</th>
                <th className="py-2">Description</th>
                <th className="py-2">Action</th>
              </tr>
            </thead>
            <tbody>
              {data?.items.map((subject) => (
                <tr key={subject.id} className="border-b border-neutral-100">
                  <td className="py-2">{subject.name}</td>
                  <td className="py-2">{subject.description}</td>
                  <td className="py-2">
                    <button
                      onClick={() => deleteMutation.mutate(subject.id)}
                      className="text-sm font-medium text-red-600 underline"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
