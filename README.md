# PollMe 3.0

A portfolio-quality polling web app where creators make single- or multiple-choice polls, share vote links, and track live results. Built via agentic coding with Copilot CLI.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 9, Dapper, SQLite, FluentMigrator, SignalR |
| Auth | JWT (cookie-based, HS256) + BCrypt |
| Frontend | React 19, TypeScript, Vite, React Router v7 |
| Real-time | SignalR (`TallyHub`) |
| Testing | xUnit + NSubstitute + WebApplicationFactory (backend), Jest + RTL (frontend) |
| Containers | Docker Compose (optional) |

---

## Running Locally on Windows

### Prerequisites

Install these tools if you don't already have them:

| Tool | Version | Download |
|---|---|---|
| .NET SDK | 9.0 or later | https://dot.net/download |
| Node.js | 20 LTS or later | https://nodejs.org |
| Git | any | https://git-scm.com |

Verify your installs:

```powershell
dotnet --version   # should print 9.x.x
node --version     # should print v20.x.x or higher
```

---

### Step 1 — Clone the repository

```powershell
git clone https://github.com/UltraPulpo/PollMe3.0.git
cd PollMe3.0
```

---

### Step 2 — Set the JWT secret

The API requires a `Jwt:Secret` value at startup. Use .NET User Secrets so the key is stored outside the project folder and never committed:

```powershell
cd PollMe.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "replace-this-with-any-long-random-string-32-chars-min"
cd ..
```

> **Tip:** Any string of 32+ characters works for local dev (e.g. `local-dev-secret-do-not-use-in-prod`). The secret is stored in `%APPDATA%\Microsoft\UserSecrets\` and is automatically picked up by `dotnet run`.

---

### Step 3 — Start the backend API

Open a PowerShell window and run:

```powershell
cd PollMe.Api
dotnet run
```

You should see output like:

```
info: FluentMigrator running migrations...
info: Microsoft.Hosting.Lifetime Now listening on: http://localhost:5207
```

The SQLite database file (`pollme.db`) is created automatically in the `PollMe.Api/` folder on first run. Leave this terminal open.

---

### Step 4 — Start the frontend

Open a **second** PowerShell window and run:

```powershell
cd pollme-frontend
npm install        # first time only
npm run dev
```

You should see:

```
  VITE v6.x.x  ready in xxx ms

  ➜  Local:   http://localhost:5173/
```

The Vite dev server automatically proxies `/api/*` and `/hubs/*` requests to the backend at `http://localhost:5207`, so CORS is not an issue.

---

### Step 5 — Open the app

Navigate to **http://localhost:5173** in your browser.

1. Click **Register** to create an account
2. Click **Create Poll** to make your first poll
3. Share the vote link with others (or open it in a second browser tab)
4. Watch the results update live via SignalR

---

### Running the tests

**Backend (60 tests — unit + integration):**

```powershell
# from the repo root
dotnet test PollMe.Api.Tests/PollMe.Api.Tests.csproj --verbosity normal
```

**Frontend:**

```powershell
cd pollme-frontend
npm test
```

---

## Optional: Run with Docker Compose

If you have **Docker Desktop** installed you can spin up both services with a single command instead of the steps above.

```powershell
# copy the example env file and set your secret
Copy-Item .env.example .env
# edit .env and replace the placeholder value for JWT_SECRET

docker compose up --build
```

The app will be available at **http://localhost:3000** (frontend) proxying to the API at **http://localhost:8080**.

---

## Project Structure

```
PollMe3.0/
├── PollMe.Api/               # ASP.NET Core 9 web API
│   ├── Controllers/          # HTTP endpoints
│   ├── Services/             # Business logic
│   ├── Repositories/         # Dapper data access
│   ├── Migrations/           # FluentMigrator schema migrations
│   ├── Hubs/                 # SignalR TallyHub
│   ├── Models/               # Domain models
│   ├── Dtos/                 # Request/response DTOs
│   └── Infrastructure/       # Config, DB factory, type handlers
├── PollMe.Api.Tests/         # xUnit test project
│   ├── Integration/          # WebApplicationFactory tests
│   ├── Services/             # Unit tests
│   └── Helpers/              # Seed helpers, builders, assertions
├── pollme-frontend/          # React + TypeScript SPA (Vite)
│   └── src/
│       ├── api/              # Typed fetch wrappers
│       ├── components/       # Reusable UI components
│       ├── hooks/            # useAuth, useSignalR, useVoteStatus
│       ├── pages/            # Route-level page components
│       └── types/            # TypeScript interfaces
├── docker-compose.yml
├── .env.example
└── .github/workflows/ci.yml  # GitHub Actions CI
```

