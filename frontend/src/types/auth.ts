export type UserRole = "Admin" | "Teacher" | "Student";

export interface AuthUser {
  userId: string;
  email: string;
  fullName: string;
  role: UserRole;
}

export interface AuthResponse extends AuthUser {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
}
