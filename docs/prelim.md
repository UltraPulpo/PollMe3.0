# PollMe 3.0

## Overview

A portfolio-quality polling web app where creators make single- or multiple-choice polls, share vote links, and track live results. Designed for **readability** and **explainability** — every implementation choice should be something you can walk an interviewer through.

### Tech Stack

| Layer | Technology | Why |
|---|---|---|
| Backend framework | ASP.NET Core Web API | Industry standard .NET backend |
| Data access | **Dapper** (micro-ORM) | Explicit SQL, portfolio differentiation vs EF Core |
| Migrations | **FluentMigrator** | Fluent C# API for schema changes, pairs with Dapper |
| Database | **SQLite** | Zero-config local dev, file-based |
| Real-time | **SignalR** | First-party ASP.NET Core WebSocket support, learning goal |
| Observability | **OpenTelemetry** | Industry-standard tracing/metrics, interview talking point |
| Frontend framework | **React** + **TypeScript** | Modern SPA stack |
| Frontend tooling | **Vite** | Fast dev server, industry standard for React+TS |
| Frontend tests | **Jest** + React Testing Library | Standard React testing stack |
| Backend tests | **xUnit** + **NSubstitute** + **FluentAssertions** | Modern .NET test stack |
| CSS | **Pico CSS** | Classless — semantic HTML looks good automatically |
| CI | **GitHub Actions** | Feature rich and natively supported |
