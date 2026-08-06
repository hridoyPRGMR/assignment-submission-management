# Assignment & Submission Management System

A role-based Assignment & Submission Management System for a school or college. Teachers create and publish
assignments for a specific class and subject, students submit and (before the deadline) update their answers,
and teachers review submissions and record marks and feedback. Admins manage users, classes, subjects, and
teacher assignments, and have system-wide visibility.

## Main Features

**Admin**
- Manage users (create, update, activate/deactivate, list/filter by role).
- Manage classes and subjects.
- Assign teachers to a subject within a class, and manage student enrollments.
- View all assignments and submissions across the system.
- System-wide dashboard (user/class/assignment/submission counts).

**Teacher**
- Create, update, and delete assignments for a class/subject they are assigned to teach.
- Publish an assignment or keep it as a draft; unpublish a published assignment.
- View submissions for their own assignments, grade them, and return a submission for revision.
- Teacher dashboard (own assignments, pending submissions to grade, etc.).

**Student**
- View published assignments for the classes they are enrolled in.
- Submit an answer before the deadline; update a submission before the deadline.
- View submission status, marks, and teacher feedback.
- Student dashboard (upcoming deadlines, submission history).

**Cross-cutting**
- JWT access + refresh token authentication, with role-based authorization enforced on every endpoint.
- Resource-based ownership checks (a teacher can only manage their own assignments/submissions; a student can
  only see their own submissions and only assignments for classes they're enrolled in).
- Server-side validation (FluentValidation) and a consistent error response shape (RFC 7807 Problem Details).
- Structured logging (Serilog) and a global exception handler for unhandled errors.
- Swagger/OpenAPI documentation with JWT bearer auth wired in, available in Development.

## Technology Stack

| Layer | Technology |
|---|---|
| Frontend | Next.js (App Router), React, TypeScript, TanStack Query, React Hook Form + Zod, Tailwind CSS |
| Backend | ASP.NET Core Web API (.NET 10), C# |
| Database | PostgreSQL, EF Core (code-first migrations) |
| Auth | JWT bearer access tokens + refresh tokens, ASP.NET Core policy-based authorization |
| Validation | FluentValidation |
| Logging | Serilog |
| Testing | xUnit, FluentAssertions, Moq (backend); EF Core InMemory / SQLite for test doubles |

## Project Structure

```
.
├── backend/
│   ├── src/
│   │   ├── AssignmentSystem.Domain/          # Entities, enums, Result type — no external dependencies
│   │   ├── AssignmentSystem.Application/     # Use-case services, DTOs, input validation (per-feature folders)
│   │   ├── AssignmentSystem.Infrastructure/   # EF Core DbContext, repositories, JWT/password hashing, migrations
│   │   └── AssignmentSystem.Api/             # Controllers, auth/authorization policies, Program.cs startup
│   └── tests/
│       ├── AssignmentSystem.Domain.Tests/         # Entity business-rule unit tests
│       ├── AssignmentSystem.Application.Tests/    # Service-level unit tests (business rules, authorization)
│       └── AssignmentSystem.Api.Tests/            # HTTP-level integration tests (full auth/assignment workflows)
└── frontend/
    └── src/
        ├── app/          # Next.js routes, grouped by role: admin/, teacher/, student/, (auth)/
        ├── components/    # Shared UI (forms, layout, ui primitives)
        ├── features/      # Per-feature API hooks/queries, mirroring the backend features
        ├── lib/          # API client, query client, auth/token storage
        └── types/        # Shared TypeScript types
```

The backend follows a layered/DDD-style architecture (Domain → Application → Infrastructure → Api). Each
Application feature folder (`Features/<Feature>/`) contains `Service/` (interface + implementation, one method
per use case), `Dto/` (response DTOs + entity-to-DTO mapping), and `Inputs/` (request records + FluentValidation
validators). Controllers call the feature service directly — there is no CQRS/mediator layer. Data access is
abstracted through a generic `IRepository<TEntity>` (implemented once, inherited per entity) plus a single
`IUnitOfWork.SaveChangesAsync`, rather than services depending on the DbContext directly.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- [PostgreSQL 14+](https://www.postgresql.org/download/) running locally (or reachable via a connection string)
- (Optional) [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) CLI tool: `dotnet tool install --global dotnet-ef`

### 1. Database setup

Create an empty PostgreSQL database (default name used below is `assignment_system`):

```bash
psql -U postgres -c "CREATE DATABASE assignment_system;"
```

You do **not** need to run migrations by hand — the API applies pending EF Core migrations and seeds demo
data automatically on startup **when running in the `Development` environment** (see `Program.cs`). If you'd
rather apply them explicitly first:

```bash
cd backend
dotnet ef database update --project src/AssignmentSystem.Infrastructure --startup-project src/AssignmentSystem.Api
```

### 2. Backend setup

The API reads configuration from `appsettings.json` / `appsettings.Development.json`, not from a root `.env`
file. Copy the example file and fill in your local Postgres credentials and a JWT signing key:

```bash
cd backend/src/AssignmentSystem.Api
cp appsettings.Example.json appsettings.Development.json
# edit appsettings.Development.json: ConnectionStrings:DefaultConnection, Jwt:SigningKey
```

`Jwt:SigningKey` must be at least 32 characters. Never commit real secrets — `appsettings.Development.json` is
git-ignored; only `appsettings.Example.json` (with placeholder values) is tracked.

Run the API:

```bash
cd backend
dotnet run --project src/AssignmentSystem.Api
```

By default this listens on `http://localhost:5298` (see `launchSettings.json`). On first run it will apply
migrations, seed demo data (see [Demo Credentials](#demo-credentials) below), and expose Swagger UI at
`http://localhost:5298/swagger`.

### 3. Frontend setup

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```

`NEXT_PUBLIC_API_BASE_URL` in `.env.local` must point at the backend's base URL including the `/api/v1`
prefix (defaults to `http://localhost:5298/api/v1`, matching the backend's default port above — update it if
you run the API on a different port). The frontend runs on `http://localhost:3000` by default.

## Demo Credentials

Seeded automatically on first run (see `DataSeeder.cs`):

| Role | Email | Password |
|---|---|---|
| Admin | `admin@school.edu` | `Admin@12345` |
| Teacher | `teacher@school.edu` | `Teacher@12345` |
| Student | `student@school.edu` | `Student@12345` |

The seed also creates one class ("Grade 10 - Section A"), one subject ("Mathematics") with the teacher
assigned to it, the student enrolled in that class, a draft assignment, a published assignment, and one
existing student submission — enough to exercise every role's main screens immediately after first login.

## Running Tests

```bash
cd backend
dotnet test AssignmentSystem.slnx
```

This runs all three test projects:
- **`AssignmentSystem.Domain.Tests`** — entity-level business rules (assignment publish/unpublish, submission
  state machine, deadline enforcement, marks validation, etc.), plus the shared `Result` type.
- **`AssignmentSystem.Application.Tests`** — service-level tests covering authorization/ownership rules (e.g.
  a teacher can't grade a submission on an assignment they don't own) and submission workflow rules (enrollment
  checks, duplicate-submission prevention, draft-assignment rejection).
- **`AssignmentSystem.Api.Tests`** — full HTTP-level integration tests (via `WebApplicationFactory` + SQLite
  in-memory) covering the complete auth flow (login, refresh token rotation, unauthorized access) and the full
  assignment lifecycle (create → publish → submit → grade) with role-based access enforcement.

No frontend automated tests are included (out of scope for this submission; manual testing was used for the
UI). See **Known Limitations** below.

## Assumptions

- A user has exactly one role (Admin, Teacher, or Student); there is no multi-role support.
- A teacher must be explicitly assigned (class + subject) before they can create assignments for that
  class/subject — this assignment is managed by an Admin.
- A student can only see and submit to assignments for classes they are enrolled in, and only once published.
- "Update a submission before the deadline" is interpreted as: allowed any time before the assignment's
  deadline, regardless of current submission status, except while a teacher has it `UnderReview` or after it's
  been `Graded` (a graded submission must first be returned for revision by the teacher).
- Deleting an assignment is a soft delete (`MarkDeleted`); it is excluded from listings but not physically
  removed, to preserve submission history/referential integrity.
- Pagination defaults to `page=1, pageSize=20` on list endpoints where pagination is supported.
- JWT access tokens are short-lived (30 minutes by default); refresh tokens are long-lived (7 days by default)
  and rotate on use (the old refresh token is revoked when a new one is issued).

## Known Limitations

- No automated frontend test suite (Jest/Playwright) — only backend unit/integration tests are included.
- No file upload for submissions; a submission's "attachment" is a URL/text field rather than binary upload
  storage.
- No email notifications (e.g. on grading or new assignment publication).
- No Docker Compose setup is included; local setup requires a manually installed PostgreSQL instance (see
  Getting Started above).
- The root-level `.env.example` documents variables intended for a containerized setup (Postgres/JWT/frontend
  URL) for reference, but the backend itself is configured via `appsettings.*.json`, not process environment
  variables — see the Backend setup section for the actual configuration path.
