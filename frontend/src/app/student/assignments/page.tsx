"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { assignmentsApi } from "@/features/assignments/assignmentsApi";
import { submissionsApi } from "@/features/submissions/submissionsApi";

export default function StudentAssignmentsPage() {
  const assignmentsQuery = useQuery({
    queryKey: ["assignments", "student"],
    queryFn: () => assignmentsApi.listPublishedForStudent(),
  });

  const submissionsQuery = useQuery({
    queryKey: ["submissions", "mine"],
    queryFn: () => submissionsApi.listMine(),
  });

  const submissionByAssignmentId = new Map(
    (submissionsQuery.data ?? []).map((s) => [s.assignmentId, s]),
  );

  if (assignmentsQuery.isLoading) {
    return <p className="text-neutral-500">Loading assignments...</p>;
  }

  return (
    <div className="flex flex-1 flex-col gap-6">
      <h1 className="text-2xl font-semibold">Assignments</h1>

      {assignmentsQuery.data?.items.length === 0 ? (
        <p className="text-neutral-500">No assignments yet.</p>
      ) : (
        <div className="max-w-4xl overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="border-b border-neutral-200 text-left">
                <th className="py-2">Title</th>
                <th className="py-2">Subject</th>
                <th className="py-2">Deadline</th>
                <th className="py-2">Status</th>
              </tr>
            </thead>
            <tbody>
              {assignmentsQuery.data?.items.map((assignment) => {
                const submission = submissionByAssignmentId.get(assignment.id);
                const statusLabel = submission ? submission.status : "Not submitted";

                return (
                  <tr key={assignment.id} className="border-b border-neutral-100">
                    <td className="py-2">
                      <Link
                        href={`/student/assignments/${assignment.id}`}
                        className="underline underline-offset-4"
                      >
                        {assignment.title}
                      </Link>
                    </td>
                    <td className="py-2">{assignment.subjectName}</td>
                    <td className="py-2">{new Date(assignment.deadlineUtc).toLocaleString()}</td>
                    <td className="py-2">{statusLabel}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
