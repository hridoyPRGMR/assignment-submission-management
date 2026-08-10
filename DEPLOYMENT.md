# Deploying to Neon + Render + Vercel

This guide walks through hosting the three pieces of this app on free tiers:

- **Database** — Neon (serverless PostgreSQL)
- **Backend** — Render (Docker web service, using `backend/src/AssignmentSystem.Api/Dockerfile`)
- **Frontend** — Vercel (Next.js)

Deploy in this order — each step needs a value produced by the previous one. There's a
"chicken-and-egg" step at the end where the backend's CORS setting needs the frontend's final
URL, so you'll redeploy the backend once after the frontend is live.

---

## 0. Before you start

Two things in this repo were fixed specifically so this cross-origin setup works — you don't
need to do anything, just know they're there:

- The refresh-token cookie uses `SameSite=None; Secure` outside local development, because
  Vercel and Render are different origins and browsers block `SameSite=Lax` cookies on
  cross-site requests.
- Database migrations and demo-data seeding, which previously only ran in `Development`, are now
  controlled by `Database__ApplyMigrationsOnStartup` and `Database__SeedDemoData` config keys
  (both default to "on" in Development, "off" elsewhere) so Render can apply them against a
  brand-new Neon database on first boot without you running anything manually.

---

## 1. Database — Neon

1. Sign up at [neon.tech](https://neon.tech) and create a new project (any region close to
   Render's is fine — Render's free tier runs in Oregon (US West) by default).
2. Neon creates a default database and gives you a connection string that looks like:
   ```
   postgresql://<user>:<password>@<endpoint>.neon.tech/<dbname>?sslmode=require
   ```
   Copy it — you'll need to convert it to the ADO.NET/Npgsql format below.
3. Convert it to an Npgsql connection string (note the different key names and the added
   `Trust Server Certificate=true`, which Npgsql needs for Neon's pooled connection endpoints):
   ```
   Host=<endpoint>.neon.tech;Database=<dbname>;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true
   ```
4. Keep this string handy — it's the `ConnectionStrings__DefaultConnection` value for Render.

You do not need to run any migration commands yourself; the backend applies them on first boot
(see step 0).

---

## 2. Backend — Render

1. Push this repo to GitHub if it isn't already there.
2. In Render, choose **New > Web Service**, connect the GitHub repo, and pick **Docker** as the
   runtime. Either:
   - Point Render at the included [render.yaml](render.yaml) blueprint (**New > Blueprint**),
     which pre-fills everything below except the four `sync: false` secrets, or
   - Configure a Web Service manually:
     - **Dockerfile path**: `backend/src/AssignmentSystem.Api/Dockerfile`
     - **Docker build context**: `backend`
     - **Health check path**: `/api/v1/health`
3. Set these environment variables (Render dashboard → Environment):

   | Key | Value |
   |---|---|
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `DOTNET_hostBuilder__reloadConfigOnChange` | `false` — Render's hosts cap inotify instances low enough that .NET's appsettings.json file-watcher can crash the app on startup with `IOException: configured user limit (128) on the number of inotify instances`. Disabling it is safe since nothing edits appsettings.json at runtime in a container. |
   | `ConnectionStrings__DefaultConnection` | the Npgsql string from step 1 |
   | `Jwt__Issuer` | `AssignmentSystem` |
   | `Jwt__Audience` | `AssignmentSystemClient` |
   | `Jwt__SigningKey` | a random string, 32+ characters (e.g. generate with `openssl rand -base64 48`) |
   | `Jwt__AccessTokenMinutes` | `30` |
   | `Jwt__RefreshTokenDays` | `7` |
   | `Cors__AllowedOrigins__0` | your Vercel URL — placeholder for now, e.g. `https://placeholder.vercel.app` |
   | `RateLimiting__Login__PermitLimit` | `5` |
   | `RateLimiting__Login__WindowSeconds` | `60` |
   | `DeadlineProcessing__PollingIntervalSeconds` | `60` |
   | `Database__ApplyMigrationsOnStartup` | `true` |
   | `Database__SeedDemoData` | `true` |

4. Deploy. Watch the logs — you should see `Application started` after migrations run and demo
   users are seeded. Note the Render-assigned URL, e.g. `https://assignment-system-api.onrender.com`.
5. Sanity-check it: `curl https://<your-render-url>/api/v1/health` should return
   `{"status":"healthy",...}`.

> Render's free tier spins down after inactivity and takes ~30-60s to wake back up on the next
> request — worth mentioning if a recruiter tries the link cold.

---

## 3. Frontend — Vercel

1. Import the repo into Vercel, set the **root directory** to `frontend`.
2. Vercel auto-detects Next.js; no build command changes needed.
3. Add one environment variable (Project Settings → Environment Variables):

   | Key | Value |
   |---|---|
   | `NEXT_PUBLIC_API_BASE_URL` | `https://<your-render-url>/api/v1` |

4. Deploy. Note the Vercel URL, e.g. `https://assignment-system.vercel.app`.

`NEXT_PUBLIC_*` variables are baked in at build time, so if you ever change the backend URL
you'll need to redeploy the frontend, not just update the env var.

---

## 4. Close the loop — update backend CORS

Now that you have the real Vercel URL:

1. Back in Render, update `Cors__AllowedOrigins__0` to the actual frontend URL (no trailing
   slash), e.g. `https://assignment-system.vercel.app`.
2. Save — Render redeploys automatically on env var change.

---

## 5. Verify end-to-end

1. Open the Vercel URL, log in with a seeded demo account (see `README.md` → *Demo credentials*).
2. Confirm login works and stays logged in after a refresh (this exercises the cross-site
   refresh cookie — the part most likely to silently fail if a step above was missed).
3. Create/publish an assignment as the teacher account, submit as the student account, grade it
   — this exercises the full stack against the live Neon database.
4. Check Render's logs for the structured business-event log lines (e.g. "Teacher ... published
   assignment ...") to confirm the audit logging feature is live too.

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| Login succeeds but you're logged out on refresh | `Cors__AllowedOrigins__0` doesn't exactly match the Vercel URL (scheme + host, no path/trailing slash), or the frontend is calling the API over `http://` instead of `https://`. |
| 500 errors on startup / migration failures in Render logs | Connection string malformed — double check `Host`/`Username`/`Password` match Neon exactly, and that `SSL Mode=Require` is present. |
| CORS errors in the browser console | `Cors__AllowedOrigins__0` not set, or set to the wrong URL, or the backend hasn't redeployed since you changed it. |
| First request after idle is very slow | Expected — Render's free tier cold-starts. Not a bug. |
| Deploy log shows "Application exited early" with `IOException: The configured user limit (128) on the number of inotify instances has been reached` | Set `DOTNET_hostBuilder__reloadConfigOnChange=false` (see the env var table above) — Render's inotify limit is too low for .NET's default appsettings.json file-watcher. |
| Docker build fails with `"/src": not found` or similar checksum errors | Build context mismatch. If deploying manually (not via the `render.yaml` Blueprint), make sure **Root Directory** is blank and **Dockerfile Path** / **Docker Build Context Directory** are both set relative to the actual repo root (`backend/src/AssignmentSystem.Api/Dockerfile` and `backend` respectively) — using **New → Blueprint** avoids this class of error entirely. |
