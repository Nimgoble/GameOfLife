# Conway's Game of Life — REST API

A production-ready ASP.NET Core 8 Web API for [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life).

---

## Table of Contents

- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Running Tests](#running-tests)
- [Docker](#docker)
- [Design Decisions](#design-decisions)

---

## Architecture

The solution follows a clean, layered architecture with three projects:

```
GameOfLife/
├── src/
│   ├── GameOfLife.Core/            # Domain entities, interfaces, exceptions
│   ├── GameOfLife.Infrastructure/  # EF Core (SQLite), repository implementation
│   └── GameOfLife.Api/             # ASP.NET Core controllers, services, middleware
└── tests/
    └── GameOfLife.Tests/
        ├── Unit/                   # BoardEvolver, GameOfLifeService, Validator tests
        └── Integration/            # Full HTTP pipeline + repository tests
```

### Key components

| Component | Responsibility |
|---|---|
| `BoardEvolver` | Pure Conway rule computation; injectable + unit-testable |
| `GameOfLifeService` | Orchestrates persistence + evolution; detects stable/cyclic states |
| `BoardValidator` | Validates grid shape and size before any persistence |
| `BoardRepository` | EF Core / SQLite persistence; maps between `Board` domain entity and `BoardRecord` |
| `ExceptionHandlingMiddleware` | Translates domain exceptions → RFC 7807 Problem Details responses |
| `GameOfLifeOptions` | Strongly-typed, validated configuration (`appsettings.json`) |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run locally

```bash
cd src/GameOfLife.Api
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).
Swagger UI is served at the root `/` in Development mode.

The SQLite database file (`gameoflife_dev.db`) is created automatically on first run.

---

## API Reference

All endpoints accept and return `application/json`.

### POST `/api/boards` — Upload a board

**Request body**

```json
{
  "cells": [
    [false, true, false],
    [false, true, false],
    [false, true, false]
  ]
}
```

`cells` is a 2-D boolean array where `true` = alive, `false` = dead.  
All rows must have the same length.

**Response `201 Created`**

```json
{ "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

---

### GET `/api/boards/{id}/next` — Next generation

Returns the board state after **one** generation.

**Response `200 OK`**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "cells": [[false,false,false],[true,true,true],[false,false,false]],
  "rows": 3,
  "columns": 3,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

---

### GET `/api/boards/{id}/states?generations={n}` — N generations ahead

Returns the board state after **N** generations. `generations` must be ≥ 1.

**Response `200 OK`** — same shape as `/next`.

---

### GET `/api/boards/{id}/final` — Final stable state

Evolves the board until:
- The same state is seen again (still life or oscillator detected), **or**
- The configured `MaxStabilisationIterations` limit is exceeded.

**Response `200 OK`** — same shape as `/next`.

**Response `422 Unprocessable Entity`** — if stability is not reached:

```json
{
  "status": 422,
  "title": "BOARD_DID_NOT_STABILISE",
  "detail": "The board did not reach a stable or cyclic state within 10000 iterations."
}
```

---

### Error responses

All errors follow [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807):

| Status | Code | Cause |
|---|---|---|
| `400` | `INVALID_BOARD` | Malformed board (empty, jagged, too large) |
| `404` | `BOARD_NOT_FOUND` | Unknown board ID |
| `422` | `BOARD_DID_NOT_STABILISE` | Stability limit exceeded |
| `500` | `INTERNAL_ERROR` | Unexpected server error |

---

## Configuration

All settings live under the `GameOfLife` key in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=gameoflife.db"
  },
  "GameOfLife": {
    "MaxStabilisationIterations": 10000,
    "CycleDetectionDepth": 100,
    "MaxBoardDimension": 1000
  }
}
```

| Setting | Default | Description |
|---|---|---|
| `MaxStabilisationIterations` | `10000` | Max generations before `/final` gives up |
| `CycleDetectionDepth` | `100` | How many past state hashes to keep for cycle detection |
| `MaxBoardDimension` | `1000` | Maximum rows or columns allowed |

All settings can be overridden via environment variables:

```bash
GameOfLife__MaxStabilisationIterations=5000
```

---

## Running Tests

```bash
# All tests
dotnet test

# With coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests only
dotnet test --filter "FullyQualifiedName~Integration"
```

---

## Docker

### Build and run

```bash
docker compose up --build
```

The API is exposed on port `8080`. The SQLite database is persisted in a named Docker volume (`db_data`).

### Environment variable overrides

```bash
docker run -e GameOfLife__MaxStabilisationIterations=5000 \
           -e ConnectionStrings__DefaultConnection="Data Source=/data/gol.db" \
           -v /host/path:/app/data \
           gameoflife-api
```

---

## Design Decisions

### Persistence: SQLite with EF Core
SQLite is a solid default for single-instance deployments and zero-friction local development. The `IBoardRepository` abstraction makes it trivial to swap in SQL Server, PostgreSQL, or another provider — just change `UseSqlite` to `UseSqlServer` etc. in `InfrastructureServiceExtensions`.

### Boards are read-only after upload
The original board is never mutated. `/next`, `/states`, and `/final` all compute and return new states without changing what's stored. This gives predictable semantics — a board ID always refers to the same original state.

### Cycle detection via SHA-256 hashing
The `/final` endpoint maintains a sliding window of recent generation hashes. When a hash reappears, the board is in a stable or periodic state. SHA-256 is collision-resistant and fast enough for boards up to the configured maximum size. The window size is configurable (`CycleDetectionDepth`).

### Separation of `IBoardEvolver` from `IGameOfLifeService`
`IBoardEvolver` is a pure computation contract (no I/O), making it trivially unit-testable without mocking any infrastructure. `IGameOfLifeService` handles the orchestration between persistence and evolution.

### Global exception middleware vs. `IExceptionHandler`
`ExceptionHandlingMiddleware` is used for explicit, readable domain exception mapping. All errors are surfaced as RFC 7807 Problem Details for a consistent client contract.
