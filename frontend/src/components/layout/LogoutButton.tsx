"use client";

import { useAuth } from "@/features/auth/AuthContext";

export function LogoutButton() {
  const { logout } = useAuth();

  return (
    <button
      onClick={() => logout()}
      className="whitespace-nowrap rounded-md border border-neutral-300 px-3 py-1.5 text-sm font-medium hover:bg-neutral-100"
    >
      Log out
    </button>
  );
}
