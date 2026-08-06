import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const ROLE_COOKIE_KEY = "assignment_system_role";

const roleHomePage: Record<string, string> = {
  Admin: "/admin",
  Teacher: "/teacher",
  Student: "/student",
};

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const role = request.cookies.get(ROLE_COOKIE_KEY)?.value;

  if (pathname === "/login") {
    if (role && roleHomePage[role]) {
      return NextResponse.redirect(new URL(roleHomePage[role], request.url));
    }
    return NextResponse.next();
  }

  const requiredRole = Object.keys(roleHomePage).find((r) => pathname.startsWith(roleHomePage[r]));

  if (requiredRole) {
    if (!role) {
      return NextResponse.redirect(new URL("/login", request.url));
    }
    if (role !== requiredRole) {
      return NextResponse.redirect(new URL(roleHomePage[role] ?? "/login", request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*", "/teacher/:path*", "/student/:path*", "/login"],
};
