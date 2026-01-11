# PRD: Monkeys Distributed Application (Angular + MAUI + BFFs + Go)

Date: 10 Jan 2026

## Summary
Build a distributed application that displays a list of monkeys and monkey details in:
- An Angular web app
- A .NET MAUI mobile/desktop app (Android, iOS, macOS via MacCatalyst)

Both front ends must communicate through their own dedicated ASP.NET 10 Backend-for-Frontend (BFF). Both BFFs call a Go microservice (“Monkeys API”) that serves a static, sample dataset of monkeys.

The Monkeys API must be seeded using the “Monkeys MCP” service as the source of sample data (fetched during development/scaffolding and committed into the repo as a static JSON dataset).

## Goals
- Provide a consistent Monkey List and Monkey Detail experience across web and mobile/desktop.
- Enforce separation of concerns using two BFFs (web + mobile).
- Implement a Go microservice that serves monkey data via a simple HTTP API.
- Make the system runnable locally as a distributed app (Aspire/AppHost) with service discovery, health checks, and basic observability.

## Non-goals
- No persistent database or CRUD for monkeys (static dataset only).
- No user accounts, payments, or complex roles.
- No offline sync; MAUI can cache responses in-memory only.

## Users & Scenarios
### Primary user
A user who wants to browse monkeys and open a monkey’s details.

### User stories
- As a user, I can view a list of monkeys with key info (name + image + location).
- As a user, I can tap/click a monkey to view details.
- As a user, I can navigate back to the list.

## Functional requirements

### Angular web app
- Pages
  - Monkey List page: displays list; selecting navigates to detail.
  - Monkey Detail page: displays details for selected monkey.
- Routing
  - `/monkeys` list
  - `/monkeys/:id` detail
- Data access
  - Calls Web BFF only.

### MAUI app (Android/iOS/MacCatalyst)
- Pages
  - Monkey List page
  - Monkey Detail page
- Navigation
  - Shell navigation with route to detail.
- Data access
  - Calls Mobile BFF only.

### Web BFF (ASP.NET 10)
- Responsibilities
  - Serve web-friendly API shape.
  - Optionally add web-specific caching and response shaping.
- Endpoints
  - `GET /api/monkeys`
  - `GET /api/monkeys/{id}`
- Calls
  - Calls Go Monkeys service.

### Mobile BFF (ASP.NET 10)
- Responsibilities
  - Serve mobile-friendly API shape.
  - Optionally handle image URL shaping and payload minimization.
- Endpoints
  - `GET /api/monkeys`
  - `GET /api/monkeys/{id}`
- Calls
  - Calls Go Monkeys service.

### Go Monkeys API (microservice)
- Responsibilities
  - Provide the source-of-truth monkey dataset.
  - Serve a static list and allow lookup by ID.
  - Load dataset from a JSON file committed to the repo.
- Endpoints
  - `GET /monkeys` returns all monkeys.
  - `GET /monkeys/{id}` returns one monkey or 404.
- Dataset seeding
  - Dataset must come from Monkeys MCP during development (checked into repo).

## Data model
Minimum fields (can expand as MCP data allows):
- `id` (string)
- `name` (string)
- `location` (string)
- `details` (string)
- `imageUrl` (string)

## Non-functional requirements
- Local dev runnable via Aspire AppHost.
- Health endpoints for each service.
- Basic logging and distributed tracing headers propagation (BFFs).
- Reasonable response times for list/detail (p95 < 250ms in local dev).

## Security & privacy
- No PII stored.
- For now, anonymous access is acceptable.
- Design BFFs so auth can be added later without reworking the front ends.

## Observability
- Each service exposes health checks.
- BFFs include structured logs for upstream request timing.

## Acceptance criteria
- Web app shows list and detail using Web BFF.
- MAUI app shows list and detail using Mobile BFF.
- Both BFFs call the Go Monkeys API successfully.
- Go Monkeys API serves a static dataset sourced from Monkeys MCP (committed JSON).
- Aspire AppHost runs the backend services locally.
