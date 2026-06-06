# PlateLib Project Guidelines

## Project Overview

PlateLib is an open-source microwell plate library. It serves as a reference catalog for labware — specifically well plates used in life science and lab automation. Users can discover plates by properties (well count, material, color, skirt type, sterility), find plates from the same manufacturer, and access associated files such as engineering drawings and data sheets.

## Architecture

```
PlateLib/
├── API/            # ASP.NET Core Minimal API — vertical slice endpoints
├── Data/           # EF Core data layer — entities, migrations
├── MigrationService/ # Database migration runner
├── AppHost/        # .NET Aspire orchestration
├── ServiceDefaults/ # Shared Aspire service configuration
└── Web/            # React + Vite frontend (TypeScript)
```

**Key principle**: The API is organized as **vertical slices** — each feature (e.g., plates, manufacturers) owns its endpoint registration and maps directly to repository calls via the `Common` interfaces. No service layer sits between endpoints and repositories.

## Domain Concepts

- **Plate**: A microwell plate identified by catalog number, well count, material, color, skirt type, lid, and sterility. Belongs to a Manufacturer.
- **Manufacturer**: The company producing the plate (e.g., Corning, Thermo Fisher).
- **PlateFile**: A binary attachment to a plate (PDF engineering drawing, datasheet, etc.), stored as `bytea` in PostgreSQL.

## Tech Stack

- **Backend**: .NET 10, ASP.NET Core Minimal API, EF Core, PostgreSQL (via Npgsql), FluentValidation, Scalar (OpenAPI UI)
- **Frontend**: React 19, TypeScript, Vite, TanStack Router, TanStack Query, TanStack Table, Tailwind CSS v4, Radix UI, Zod, shadcn/ui component conventions
- **Infrastructure**: .NET Aspire (AppHost), Docker (PostgreSQL)

## Build & Run

```bash
# Run via Aspire AppHost (recommended)
dotnet run --project AppHost

# API docs (Scalar UI)
https://localhost:7034/api

# Frontend dev server
cd Web && pnpm dev   # runs on port 3000
```

## Conventions

- Always pass `CancellationToken` through async repository and endpoint calls.
- Return `Results.Problem(...)` for errors; use `ProblemDetails` consistently.
- DTOs live in `Common/DTOs` — never expose EF entities directly from the API.
- Enums for plate properties live in `Common/Enums`.
- New plate properties added to `Plate` entity must also be reflected in `PlateDTO` and the relevant endpoint request/response models.
- File content is stored as `byte[]` in `PlateFile.FileContent`; only metadata (`FileName`, `ContentType`) is returned in list/detail DTOs — never the binary content in listing responses.
