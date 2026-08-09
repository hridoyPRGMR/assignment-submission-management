import { isAxiosError } from "axios";

export function getErrorMessage(error: unknown): string | undefined {
  if (!isAxiosError(error)) {
    return undefined;
  }

  const data = error.response?.data;
  if (typeof data === "string") {
    return data;
  }
  if (data && typeof data === "object") {
    const body = data as { title?: unknown; message?: unknown; detail?: unknown };
    const value = body.title ?? body.message ?? body.detail;
    if (typeof value === "string") {
      return value;
    }
  }
  return undefined;
}
