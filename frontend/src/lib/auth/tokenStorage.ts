const ACCESS_TOKEN_KEY = "assignment_system_access_token";
const ROLE_COOKIE_KEY = "assignment_system_role";

function setCookie(name: string, value: string, days: number) {
  const expires = new Date(Date.now() + days * 24 * 60 * 60 * 1000).toUTCString();
  document.cookie = `${name}=${value}; expires=${expires}; path=/; SameSite=Lax`;
}

function clearCookie(name: string) {
  document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
}

export const tokenStorage = {
  getAccessToken(): string | null {
    if (typeof window === "undefined") return null;
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  },
  setTokens(accessToken: string, role: string) {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    setCookie(ROLE_COOKIE_KEY, role, 7);
  },
  setAccessToken(accessToken: string) {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  },
  clear() {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    clearCookie(ROLE_COOKIE_KEY);
  },
};
