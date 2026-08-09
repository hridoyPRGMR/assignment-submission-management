"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { KeyRound, ShieldCheck } from "lucide-react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { authApi } from "@/features/auth/authApi";
import {
  ALLOWED_SUBMISSION_FILE_EXTENSIONS,
  MAX_SUBMISSION_FILE_SIZE_BYTES,
} from "@/features/submissions/submissionsApi";
import { useAuth } from "@/features/auth/AuthContext";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { toast } from "@/components/ui/toast";
import { formatFileSize } from "@/lib/format";
import { getErrorMessage } from "@/lib/errors";

const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Current password is required"),
    newPassword: z.string().min(8, "New password must be at least 8 characters"),
    confirmPassword: z.string().min(1, "Please confirm your new password"),
  })
  .refine((values) => values.newPassword === values.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>;

export default function AdminSettingsPage() {
  const { user } = useAuth();

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
  });

  const changePasswordMutation = useMutation({
    mutationFn: (values: ChangePasswordFormValues) =>
      authApi.changePassword(values.currentPassword, values.newPassword),
    onSuccess: () => {
      reset();
      toast.add({ type: "success", title: "Password updated" });
    },
    onError: (error) => {
      const message = getErrorMessage(error) ?? "Could not update your password.";
      setError("currentPassword", { message });
      toast.add({ type: "error", title: "Could not update password", description: message });
    },
  });

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-1">
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        <p className="text-sm text-muted-foreground">
          Manage your account security and view application-level policies.
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <KeyRound className="size-4 text-muted-foreground" />
            <CardTitle>Change password</CardTitle>
          </div>
          <CardDescription>
            Update the password for {user?.email}. You&apos;ll keep using this password to sign
            in.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form
            onSubmit={handleSubmit((values) => changePasswordMutation.mutate(values))}
            className="grid grid-cols-1 gap-6 sm:max-w-md"
          >
            <Field>
              <Label>Current password</Label>
              <Input type="password" {...register("currentPassword")} />
              {errors.currentPassword && (
                <p className="text-sm text-destructive">{errors.currentPassword.message}</p>
              )}
            </Field>

            <Field>
              <Label>New password</Label>
              <Input type="password" placeholder="At least 8 characters" {...register("newPassword")} />
              {errors.newPassword && (
                <p className="text-sm text-destructive">{errors.newPassword.message}</p>
              )}
            </Field>

            <Field>
              <Label>Confirm new password</Label>
              <Input type="password" {...register("confirmPassword")} />
              {errors.confirmPassword && (
                <p className="text-sm text-destructive">{errors.confirmPassword.message}</p>
              )}
            </Field>

            <Button type="submit" disabled={isSubmitting || changePasswordMutation.isPending}>
              {changePasswordMutation.isPending ? "Updating..." : "Update password"}
            </Button>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <ShieldCheck className="size-4 text-muted-foreground" />
            <CardTitle>Submission policy</CardTitle>
          </div>
          <CardDescription>
            Application-wide limits enforced whenever a student uploads a file with their
            submission.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="flex flex-col gap-1">
              <dt className="text-sm text-muted-foreground">Allowed file types</dt>
              <dd className="text-sm font-medium">
                {ALLOWED_SUBMISSION_FILE_EXTENSIONS.join(", ")}
              </dd>
            </div>
            <div className="flex flex-col gap-1">
              <dt className="text-sm text-muted-foreground">Maximum file size</dt>
              <dd className="text-sm font-medium">
                {formatFileSize(MAX_SUBMISSION_FILE_SIZE_BYTES)}
              </dd>
            </div>
          </dl>
        </CardContent>
      </Card>
    </div>
  );
}
