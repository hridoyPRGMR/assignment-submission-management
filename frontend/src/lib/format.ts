export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  });
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, {
    dateStyle: "medium",
  });
}

export function formatRelativeDeadline(iso: string): {
  label: string;
  isOverdue: boolean;
} {
  const diffMs = new Date(iso).getTime() - Date.now();
  const isOverdue = diffMs < 0;
  const absMs = Math.abs(diffMs);
  const days = Math.ceil(absMs / (1000 * 60 * 60 * 24));
  const hours = Math.ceil(absMs / (1000 * 60 * 60));

  if (isOverdue) {
    const label =
      days >= 1
        ? `Overdue by ${days} day${days === 1 ? "" : "s"}`
        : `Overdue by ${hours} hour${hours === 1 ? "" : "s"}`;
    return { label, isOverdue };
  }

  if (days >= 1) {
    return { label: `${days} day${days === 1 ? "" : "s"} left`, isOverdue };
  }
  if (hours >= 1) {
    return { label: `${hours} hour${hours === 1 ? "" : "s"} left`, isOverdue };
  }
  return { label: "Due soon", isOverdue };
}

export function formatFileSize(bytes: number): string {
  return bytes < 1024 * 1024
    ? `${Math.round(bytes / 1024)} KB`
    : `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
