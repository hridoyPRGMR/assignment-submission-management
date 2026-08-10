# Assignment & Submission Management System

## Instructions for running the frontend and backend

- Docker (recommended):
  - Copy env: `cp .env.example .env`
  - Start: `docker compose up --build`
  - API: http://localhost:5080 (Swagger at `/swagger`)
  - Frontend: http://localhost:3000

- Local (without Docker):
  - Prereqs: .NET 10 SDK, Node 20+, Postgres 14+
  - Backend:
    - Copy example config: `cp backend/src/AssignmentSystem.Api/appsettings.Example.json backend/src/AssignmentSystem.Api/appsettings.Development.json`
    - Edit `ConnectionStrings:DefaultConnection` and `Jwt:SigningKey` (>=32 chars)
    - Run: `dotnet run --project backend/src/AssignmentSystem.Api`
    - API listens on http://localhost:5298 (Swagger at `/swagger`) — see `backend/src/AssignmentSystem.Api/Properties/launchSettings.json` if you need to change it. `frontend/.env.example` already points `NEXT_PUBLIC_API_BASE_URL` at this port for local (non-Docker) runs.
  - Frontend:
    - `cd frontend`
    - `cp .env.example .env.local`
    - `npm install`
    - `npm run dev`

- Deploying to the cloud (Neon + Render + Vercel): see [DEPLOYMENT.md](DEPLOYMENT.md).

## Short overview

Role-based app: teachers create and grade assignments; students submit answers and optional files; admins manage users, classes, and subjects.

## Main features

- Admin: manage users, classes, subjects, assign teachers, browse all assignments/submissions system-wide, view system-wide dashboard data, change their own password.
- Teacher: create/edit/publish assignments; review, download, grade, or return submissions.
- Student: view published assignments, submit/update answers and files, view marks and feedback.
- Auth: JWT access tokens + rotating, replay-resistant refresh tokens (stored in an httpOnly cookie, not accessible to JavaScript); role-based and ownership authorization enforced at both the API-policy and service layer; login is rate-limited.
- File upload: PDF/DOCX/ZIP, server-side size and extension checks; stored on disk under a generated filename (no path traversal) and served to authorized users only.

## Technology stack

- Frontend: Next.js (App Router), TypeScript, React, TanStack Query
- Backend: ASP.NET Core (.NET 10), EF Core, FluentValidation
- Database: PostgreSQL
- Logging: Serilog
- Testing: xUnit

## Project structure (key folders)

- `backend/src/AssignmentSystem.Api` — controllers, Program.cs, config, `wwwroot/uploads`
- `backend/src/AssignmentSystem.Application` — feature services, DTOs, validators
- `backend/src/AssignmentSystem.Domain` — entities, state machines, business rules
- `backend/src/AssignmentSystem.Infrastructure` — EF Core, repositories, storage, security
- `frontend` — Next.js app (routes, components, API client)
- `backend/tests` — unit and integration tests for the backend (`AssignmentSystem.Domain.Tests`, `AssignmentSystem.Application.Tests`, `AssignmentSystem.Api.Tests`)

## Database setup

- Create DB (example):
  ```bash
  psql -U postgres -c "CREATE DATABASE assignment_system;"
  ```
- When running the API in `Development`, it applies pending EF Core migrations and seeds demo data automatically on first startup.

## Running tests

- Run all backend tests:
  ```bash
  cd backend
  dotnet test AssignmentSystem.slnx
  ```

## Assumptions

- Each user has one role: Admin, Teacher, or Student.
- Teachers must be assigned to a class+subject before creating assignments.
- **A late submission is accepted, not rejected.** We deliberately chose not to hard-block
  submissions/updates after the deadline; instead the submission is created/updated normally
  and flagged `IsLate`/`LateSubmission` so the teacher can see it was late and decide how to
  grade it. This matches "submit an answer... update a submission... if allowed" from the
  brief as "the platform allows it, but marks it," rather than closing the door entirely.
- Login is rate-limited (5 attempts/minute per the app instance, configurable via
  `RateLimiting:Login` in `appsettings.json`) — this is an in-memory limiter scoped to a single
  process, not a distributed one.

## Known limitations

- No automated frontend tests included.
- No virus/malware scanning for uploaded files; only extension and size validated.
- The login rate limiter is per-process/in-memory; it resets on restart and isn't shared across
  multiple backend instances behind a load balancer.

## Business-event logging and deadline processing

- Beyond Serilog's HTTP access logging, the application layer emits structured business-event
  log lines for meaningful admin/teacher actions (assignment created/published/graded/archived,
  submission created/graded/returned, teacher assigned to a class/subject, student
  enrolled/unenrolled, user created/role changed/deleted/activated), each including the acting
  user's id and the affected entity id.
- A `DeadlineProcessingService` background service (registered as an `IHostedService`, polling
  every `DeadlineProcessing:PollingIntervalSeconds` — default 60s) periodically scans for
  published assignments whose deadline has just passed and logs a summary business event
  (late/total/missing submission counts) exactly once per assignment, using the same
  `Assignment.GetLifecycleStatus` computation the read paths already use. This does not change
  `Assignment.Status` automatically — marking an assignment "Graded" remains a deliberate teacher
  action — it only makes the deadline-passed transition observable without a manual refresh.

## Demo credentials (seeded in Development)

- Admin: admin@school.edu / Admin@12345
- Teacher: teacher@school.edu / Teacher@12345
- Student: student@school.edu / Student@12345

---

For implementation details, see the backend API code under `backend/src/AssignmentSystem.Api` and the frontend under `frontend`.

---

For more details or to change configuration, see files under `backend/src/AssignmentSystem.Api` and `frontend`.
