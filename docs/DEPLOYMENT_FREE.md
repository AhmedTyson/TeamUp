# Free Demo Deployment Guide (Option 1 — Zero Credit Card)

Deploy **TeamUp** 100% free with persistent **SQL Server**, persistent profile photo uploads (`POST /api/v1/students/me/profile/photo`), and global HTTPS CDN hosting:

- **Backend + Database:** [MonsterASP.NET](https://www.monsterasp.net/) Free Plan (native `.NET 10`, Windows IIS, 5 GB disk, Free SQL Server DB) — *or* **Azure for Students** (`$100` free credit + Azure SQL Free Offer with a `.edu` email).
- **Frontend SPA:** **Cloudflare Pages**, **Vercel**, or **Netlify** (all free for public static SPAs, zero credit card required).

---

## Part 1 — Backend + SQL Server on MonsterASP.NET

### 1. Create the Free Website & Database
1. Sign up on **MonsterASP.NET** (no credit card required) and create a **Free Website** (`https://<your-site>.runasp.net`).
2. Open **Websites → `<your-site>` → Scripting** and set **ASP.NET Core Runtime** to **`.NET 10`** (In-Process hosting).
3. Go to **Databases → Create MSSQL Database** to provision your free SQL Server database.
4. Copy the SQL Server **Connection String** (use `dbXXXX.public.databaseasp.net` if migrating remotely, or `dbXXXX.databaseasp.net` when running directly on MonsterASP) and keep strict TLS:
   ```text
   Server=dbXXXX.public.databaseasp.net;Database=dbXXXX;User Id=dbXXXX;Password=YOUR_DB_PASSWORD;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;
   ```

### 2. Generate Production Secrets & Build the Publish ZIP
1. Generate a random 64-character `Jwt:Key` in PowerShell:
   ```powershell
   -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object { [char]$_ })
   ```
2. Copy [`backend/src/TeamUp.Api/appsettings.Production.example.json`](../backend/src/TeamUp.Api/appsettings.Production.example.json) to a **git-ignored** file `backend/src/TeamUp.Api/appsettings.Production.json` and fill in:
   - `ConnectionStrings:TeamUpDb`
   - `Jwt:Key`
   - `Seed:AdminPassword` (min 12 chars)
   - `Cors:AllowedOrigins` (temporarily `["http://localhost:5173"]`, then update to your live frontend URL in Part 3)
   - `Email` SMTP settings (Gmail App Password, Brevo SMTP, or Resend SMTP)
3. Build the MonsterASP.NET publish archive cleanly:
   ```powershell
   .\scripts\publish-monsterasp.ps1
   ```
   This emits `artifacts/teamup-api-monsterasp.zip` (with `ServerGarbageCollection=false` for the 256 MB RAM tier, `web.config`, and seeded `wwwroot/images/people/*.jpg`, stripped of development configs).

### 3. Deploy via Git Auto-Deploy (Recommended) or Manual ZIP

#### Option A: Automated GitHub Actions + Web Deploy (Recommended)
[`deploy-monsterasp.yml`](../.github/workflows/deploy-monsterasp.yml) builds `backend/src/TeamUp.Api` cleanly on every push to `main` touching `backend/**` and syncs using `-enableRule:DoNotDeleteRule` (so uploaded student profile portraits in `wwwroot/images/people/student-*` are preserved across deploys):

1. In MonsterASP.NET Control Panel, go to **Websites → `<your-site>` → Deploy → Visual Studio access (WebDeploy)** and toggle it **Enabled**.
2. Copy the 4 WebDeploy values into GitHub **Settings → Secrets and variables → Actions → New repository secret**:
   - `MONSTERASP_SITE_NAME` (your Site name)
   - `MONSTERASP_SERVER` (your WebDeploy Server host/URL)
   - `MONSTERASP_USERNAME` (your WebDeploy Login)
   - `MONSTERASP_PASSWORD` (your WebDeploy Password)
   - Optional: `MONSTERASP_APPSETTINGS_JSON` (full JSON content of your `appsettings.Production.json` if you want CI to write/update it too)
3. Push to `main` (or trigger **Actions → Deploy Backend to MonsterASP.NET → Run workflow**).

#### Option B: Manual ZIP Upload
1. Upload and extract `artifacts/teamup-api-monsterasp.zip` into your site's `wwwroot` directory via MonsterASP.NET File Manager or FTP.
2. Upload your filled `appsettings.Production.json` into the same `wwwroot` directory.

### 4. Apply Schema & Verify Endpoints
1. With `Database:AutoMigrate = true` and `Seed:Enabled = true`, EF Core automatically applies all migrations and seeds Partner Universities, faculties, showcase student portraits, and your PlatformAdmin account on first boot.
   - **Alternative manual SQL script:** Run [`docs/database/teamup-sqlserver-idempotent.sql`](database/teamup-sqlserver-idempotent.sql) in SSMS or WebMSSQL if you prefer applying schema changes before booting the API (`Database:AutoMigrate = false`).
2. Verify your live backend:
   - `GET https://<your-site>.runasp.net/health` → `{ "status": "Healthy", "database": "Connected" }`
   - `GET https://<your-site>.runasp.net/api/v1/students/showcase?limit=6` → returns seeded campus builders & portrait paths.

---

## Part 2 — Frontend SPA (Pick Any Free Provider)

Set **Environment Variable**:
- `VITE_API_BASE_URL` = `https://<your-site>.runasp.net` *(no trailing slash)*

See [`frontend/.env.example`](../frontend/.env.example).

| Option | Dashboard / Repo Setup | SPA Routing File Already Included |
| :--- | :--- | :--- |
| **Cloudflare Pages** *(unlimited bandwidth)* | Connect GitHub → Root directory: `frontend` → Build command: `npm run build` → Output directory: `dist` | [`frontend/public/_redirects`](../frontend/public/_redirects) |
| **Netlify** | Connect GitHub *(root [`netlify.toml`](../netlify.toml) sets `base = "frontend"`, `command = "npm run build"`, `publish = "dist"` automatically)* | [`netlify.toml`](../netlify.toml) + [`frontend/public/_redirects`](../frontend/public/_redirects) |
| **Vercel** | Import repo → Root directory: `frontend` → Framework Preset: `Vite` → Output directory: `dist` | [`frontend/vercel.json`](../frontend/vercel.json) |

---

## Part 3 — Final Wiring & Smoke Check

1. Update `Cors:AllowedOrigins` (and `Email:ResetPasswordUrlBase`) inside MonsterASP.NET `appsettings.Production.json` to your live frontend origin (e.g. `https://teamup-demo.pages.dev`).
2. Restart the website in MonsterASP.NET Control Panel so the new CORS origin takes effect.
3. Open your live frontend URL and verify:
   - Homepage student portraits + Partner Universities logos load cleanly.
   - Registration loads live universities/faculties (`GET /api/v1/universities/catalog`).
   - Uploading a profile photo on `/profile/me` (`POST /api/v1/students/me/profile/photo`) persists in `wwwroot/images/people/student-*` and renders cross-origin via `resolveMediaUrl`.

---

## Optional Alternative: Azure for Students (`$100` Free Credit + `.edu`)

If you prefer Microsoft Azure with a verified university email:
1. Activate **Azure for Students** (no credit card) and create an **Azure SQL Database** (Free offer) + **App Service (Windows or Linux, .NET 10)**.
2. Set `ConnectionStrings:TeamUpDb`, `Jwt:Key`, `Cors:AllowedOrigins`, `Database:AutoMigrate=true`, and `Seed:Enabled=true` in **App Service → Environment variables** (`__` double underscore syntax, e.g., `Jwt__Key`, `Cors__AllowedOrigins__0`, `Seed__AdminPassword`).
3. Deploy `backend/src/TeamUp.Api` via `az webapp deploy` or GitHub Actions, then connect Cloudflare Pages / Vercel / Netlify as in Part 2.
