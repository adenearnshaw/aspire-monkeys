# Implementation Plan: Monkeys Distributed Application

Date: 10 Jan 2026

## Repository layout (target)
- `src/MyMonkeys.AppHost/` (Aspire AppHost)
- `src/MyMonkeys.ServiceDefaults/` (Aspire defaults)
- `src/MyMonkeys.WebBff/` (ASP.NET 10 BFF)
- `src/MyMonkeys.MobileBff/` (ASP.NET 10 BFF)
- `src/monkeys-service/` (Go HTTP microservice)
- `src/monkeys-service/data/monkeys.json` (MCP-seeded dataset)
- `src/web/monkeys-web/` (Angular app)
- `src/mobile/MyMonkeys.Mobile/` (MAUI app)

## Phase 0 — Tooling & prerequisites
- Install .NET SDK 10
- Install MAUI workloads (`dotnet workload install maui` if needed)
- Install Node.js + npm
- Install Go
- Install Docker (recommended for running Go/Angular as containers under Aspire)

## Phase 1 — Seed dataset from Monkeys MCP
- Use Monkeys MCP during development to pull:
  - List of monkeys
  - Per-monkey detail (if available)
  - Optional “journey/activities” data
- Generate `src/monkeys-service/data/monkeys.json` and commit it.
- Keep dataset generation as a repeatable dev script (optional): `tools/generate-monkeys-data/`.

## Phase 2 — Build Go Monkeys API
- Create Go module `src/monkeys-service`
- Implement HTTP server:
  - `GET /health`
  - `GET /monkeys`
  - `GET /monkeys/{id}`
- Load `data/monkeys.json` at startup.
- Add Dockerfile for Aspire orchestration.

## Phase 3 — Build two ASP.NET 10 BFFs
- Create projects:
  - `MyMonkeys.WebBff` (Minimal API)
  - `MyMonkeys.MobileBff` (Minimal API)
- Add endpoints:
  - `GET /api/monkeys`
  - `GET /api/monkeys/{id}`
  - `GET /health`
- Implement upstream calls to Go service using `HttpClient` + service discovery.
- Add basic resilience:
  - timeouts
  - simple in-memory caching (optional)

## Phase 4 — Aspire orchestration
- Create Aspire AppHost and ServiceDefaults.
- Add resources:
  - Go monkeys-service container
  - WebBff project
  - MobileBff project
- Configure service discovery / env vars:
  - `MONKEYS_SERVICE_URL` for BFFs
- Validate local run:
  - `dotnet run --project src/MyMonkeys.AppHost`

## Phase 5 — Angular web app
- Scaffold Angular app `src/web/monkeys-web`.
- Add routes:
  - `/monkeys` list
  - `/monkeys/:id` detail
- Add service:
  - `MonkeysApiClient` calling WebBff `/api/monkeys`.
- Add basic UI:
  - list cards
  - detail view
- Dev proxy to BFF (Angular `proxy.conf.json`).

## Phase 6 — MAUI app
- Scaffold MAUI app `src/mobile/MyMonkeys.Mobile` targeting:
  - Android
  - iOS
  - MacCatalyst
- Add pages:
  - `MonkeyListPage`
  - `MonkeyDetailPage`
- Add typed client:
  - `MonkeysClient` calling MobileBff `/api/monkeys`.
- Ensure platform-safe base URL configuration for local dev.

## Phase 7 — Tests & quality gates
- Go:
  - unit tests for data loading and ID lookup
- BFFs:
  - minimal integration tests (optional)
- CI:
  - `dotnet build`
  - `go test ./...`
  - `npm test` (optional)

## Deliverables checklist
- [ ] Dataset file committed from MCP
- [ ] Go Monkeys API with list/detail
- [ ] Two BFFs calling Go service
- [ ] Angular list/detail consuming Web BFF
- [ ] MAUI list/detail consuming Mobile BFF
- [ ] Aspire AppHost orchestrating services
