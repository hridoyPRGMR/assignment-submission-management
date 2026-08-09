"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { authApi } from "@/features/auth/authApi";
import { tokenStorage } from "@/lib/auth/tokenStorage";
import type { AuthUser } from "@/types/auth";

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<AuthUser>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    // The refresh token lives only in an httpOnly cookie, invisible to JS, so we can't check
    // for its presence up front — just attempt a silent refresh and see if the cookie is valid.
    authApi
      .refresh()
      .then((response) => {
        tokenStorage.setTokens(response.accessToken, response.role);
        setUser({
          userId: response.userId,
          email: response.email,
          fullName: response.fullName,
          role: response.role,
        });
      })
      .catch(() => {
        tokenStorage.clear();
      })
      .finally(() => setIsLoading(false));
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const response = await authApi.login(email, password);
    tokenStorage.setTokens(response.accessToken, response.role);

    const authUser: AuthUser = {
      userId: response.userId,
      email: response.email,
      fullName: response.fullName,
      role: response.role,
    };

    setUser(authUser);
    return authUser;
  }, []);

  const logout = useCallback(async () => {
    await authApi.logout().catch(() => undefined);
    tokenStorage.clear();
    setUser(null);
    router.push("/login");
  }, [router]);

  const value = useMemo(
    () => ({ user, isLoading, login, logout }),
    [user, isLoading, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
