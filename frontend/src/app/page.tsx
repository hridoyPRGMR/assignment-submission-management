import Link from "next/link";

export default function HomePage() {
  return (
    <main className="flex flex-1 flex-col items-center justify-center gap-4 p-8 text-center">
      <h1 className="text-3xl font-semibold">Assignment & Submission Management System</h1>
      <p className="text-neutral-500">
        Role-based platform for teachers, students, and admins to manage assignments and
        submissions.
      </p>
      <Link
        href="/login"
        className="rounded-md bg-neutral-900 px-4 py-2 text-sm font-medium text-white"
      >
        Sign in
      </Link>
    </main>
  );
}
