import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import type { AssignmentLifecycleStatus } from "@/types/assignment";
import type { SubmissionStatus } from "@/types/submission";

const toneStyles: Record<string, string> = {
  neutral: "bg-muted text-muted-foreground",
  blue: "bg-blue-500/10 text-blue-600",
  emerald: "bg-emerald-500/10 text-emerald-600",
  amber: "bg-amber-500/10 text-amber-600",
  violet: "bg-violet-500/10 text-violet-600",
  rose: "bg-rose-500/10 text-rose-600",
};

const assignmentStatusConfig: Record<AssignmentLifecycleStatus, { label: string; tone: string }> = {
  Draft: { label: "Draft", tone: "neutral" },
  Published: { label: "Published", tone: "blue" },
  SubmissionOpen: { label: "Open for submission", tone: "emerald" },
  DeadlinePassed: { label: "Deadline passed", tone: "amber" },
  Graded: { label: "Graded", tone: "violet" },
  Archived: { label: "Archived", tone: "neutral" },
};

const submissionStatusConfig: Record<SubmissionStatus, { label: string; tone: string }> = {
  Submitted: { label: "Submitted", tone: "blue" },
  LateSubmission: { label: "Late submission", tone: "amber" },
  UnderReview: { label: "Under review", tone: "violet" },
  Graded: { label: "Graded", tone: "emerald" },
  Returned: { label: "Returned", tone: "rose" },
};

export function AssignmentStatusBadge({ status }: { status: AssignmentLifecycleStatus }) {
  const config = assignmentStatusConfig[status];
  return (
    <Badge variant="outline" className={cn("border-transparent", toneStyles[config.tone])}>
      {config.label}
    </Badge>
  );
}

export function SubmissionStatusBadge({ status }: { status: SubmissionStatus }) {
  const config = submissionStatusConfig[status];
  return (
    <Badge variant="outline" className={cn("border-transparent", toneStyles[config.tone])}>
      {config.label}
    </Badge>
  );
}
