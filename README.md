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
  - Frontend:
    - `cd frontend`
    - `cp .env.example .env.local`
    - `npm install`
    - `npm run dev`

## Short overview

Role-based app: teachers create and grade assignments; students submit answers and optional files; admins manage users, classes, and subjects.

## Main features

- Admin: manage users, classes, subjects, assign teachers, view system-wide data.
- Teacher: create/edit/publish assignments; review, download, grade, or return submissions.
- Student: view published assignments, submit/update answers and files, view marks and feedback.
- Auth: JWT access tokens + rotating refresh tokens; role-based and ownership authorization enforced.
- File upload: PDF/DOCX/ZIP, server-side size and extension checks; stored on disk and served to authorized users only.

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
- `tests` — unit and integration tests for backend

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
- A late submission is accepted and flagged `LateSubmission` rather than rejected.

## Known limitations

- No automated frontend tests included.
- No virus/malware scanning for uploaded files; only extension and size validated.
- No automatic background job to mark deadlines; deadline state is computed.

## Demo credentials (seeded in Development)

- Admin: admin@school.edu / Admin@12345
- Teacher: teacher@school.edu / Teacher@12345
- Student: student@school.edu / Student@12345

---

For implementation details, see the backend API code under `backend/src/AssignmentSystem.Api` and the frontend under `frontend`.

---

For more details or to change configuration, see files under `backend/src/AssignmentSystem.Api` and `frontend`.
