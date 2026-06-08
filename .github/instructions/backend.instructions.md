---
description: "Use when working on backend code: API endpoints, repositories, EF Core entities, DTOs, database migrations, or FluentValidation. Covers vertical slice conventions, data layer patterns, and PlateLib API structure."
applyTo: "API/**,Data/**,Common/**,MigrationService/**,Tests/PlateLib.UnitTests/**,Tests/PlateLib.IntegrationTests/**"
---
# Backend Guidelines

## Project Roles

| Project | Responsibility |
|---------|---------------|
| `API` | Minimal API endpoints — maps HTTP → repository calls |
| `Common` | Shared DTOs, interfaces, enums, models — no dependencies on Data or API |
| `Data` | EF Core entities, DbContext, repositories, migrations, seeding |
| `MigrationService` | Standalone migration runner for production deployments |
| `Tests/PlateLib.UnitTests` | xUnit unit tests for business logic, common helpers, and feature slice logic |
| `Tests/PlateLib.IntegrationTests` | xUnit integration tests using Aspire's `DistributedApplicationTestingBuilder` |

## Endpoint Conventions (Vertical Slices)

- Register endpoints in `API/Endpoints/<Feature>Endpoints.cs` using extension methods on `IEndpointRouteBuilder`.
- Keep endpoint handlers lean: validate input → call repository → map to response.
- Use `FluentValidation` for request validation; inject `IValidator<T>` directly into the endpoint handler.
- Return `TypedResults` (e.g., `TypedResults.Ok(...)`, `TypedResults.NotFound()`) for compile-time response type safety.
- Return `Results.Problem(...)` for unexpected errors; do not throw unhandled exceptions from endpoints.

Example endpoint registration pattern:
```csharp
public static class PlateEndpoints
{
    public static IEndpointRouteBuilder MapPlateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/plates").WithTags("Plates");
        group.MapGet("/", GetAllPlates);
        return app;
    }

    private static async Task<Results<Ok<IEnumerable<PlateDTO>>, ProblemHttpResult>> GetAllPlates(
        IPlateRepository repo, CancellationToken ct)
    {
        var plates = await repo.GetAllAsync(ct);
        return TypedResults.Ok(plates);
    }
}
```

## Data Layer Conventions

- Repositories implement interfaces from `Common/Interfaces` — the API only depends on interfaces, never concrete repositories directly.
- Inject repositories via constructor injection using the interface type.
- EF entity configuration lives in `Data/Configurations/<Entity>Configuration.cs` and is referenced via `[EntityTypeConfiguration]` on the entity class.
- Map entities → DTOs inside the repository (`MapToDTO`), never in the endpoint.
- Never expose `byte[]` file content in list or summary DTOs; only expose it in a dedicated download endpoint.
- Always include `CancellationToken` in all async repository methods.

## Adding a New Plate Property

1. Add property to `Data/Entities/Plate.cs`
2. Add corresponding property to `Common/DTOs/PlateDTO.cs`
3. Update `PlateRepository.MapToDTO()`
4. Add/update EF configuration in `Data/Configurations/PlateConfiguration.cs`
5. Create a new migration (see below)

## Creating Migrations

Migrations are managed via the `Data` project. There is **no design-time factory** — migrations must be created with the Aspire stack running so EF Core can reach the database.

### Steps to add a migration

1. Start the Aspire AppHost so PostgreSQL is available:
   ```bash
   dotnet run --project AppHost
   ```

2. In a second terminal, run `dotnet ef` targeting the `Data` project with `MigrationService` as startup project:
   ```bash
   dotnet ef migrations add <MigrationName> \
     --project Data \
     --startup-project MigrationService \
     --context PlateLibContext
   ```
   Example:
   ```bash
   dotnet ef migrations add AddPlateColor \
     --project Data \
     --startup-project MigrationService \
     --context PlateLibContext
   ```

3. Review the generated migration in `Data/Migrations/` before committing.

### Migration naming conventions
- Use PascalCase descriptive names: `AddPlateColor`, `AddManufacturerLogo`, `RefactorFilesToStoredFile`
- Migration files are prefixed with a timestamp: `20260606175400_<Name>.cs`

### Seeding
- Seed data lives in `Data/Seeding/Seed.cs` and is **only run in the Development environment** (controlled in `MigrationService/Worker.cs`).
- Migrations always run in all environments; seeding never runs in production.

## Blob Storage

All file binaries are stored in S3-compatible blob storage (MinIO locally, Hetzner/S3 in production). The `File` entity holds only **metadata** in Postgres — no binary content.

| Field | Purpose |
|---|---|
| `StorageKey` | Object key in the bucket, e.g. `plates/{plateId}/{fileId}/drawing.pdf` |
| `FileSizeBytes` | Stored at upload time; never recalculated from the blob |

The `IStorageService` abstraction lives in `API/Storage/`. Inject it in endpoints — never call `IAmazonS3` directly from endpoints.

```csharp
// Upload pattern (write endpoint)
var storageKey = $"plates/{plateId}/{fileId}/{file.FileName}";
await storage.UploadAsync(storageKey, stream, file.ContentType, ct);

// Download pattern — redirect, do not stream through the API
var url = await storage.GeneratePresignedUrlAsync(file.StorageKey, TimeSpan.FromMinutes(15), ct);
return Results.Redirect(url);
```

**Key naming convention:** `plates/{plateId}/{fileId}/{originalFileName}` — keeps files organized by plate and collision-free.

### Local development

MinIO is added to Aspire AppHost and starts automatically with `dotnet run --project AppHost`. The MigrationService seeds a sample file to MinIO on first run (Development environment only).

- MinIO console: `http://localhost:9001` (user: `minioadmin`, password: `minioadmin`)
- Storage config is injected by Aspire via `Storage__ServiceUrl`, `Storage__AccessKey`, `Storage__SecretKey` env vars — no manual config needed locally.

### Production configuration

Set via environment variables or secrets (never commit these):

```
Storage__ServiceUrl=https://<bucket>.your-hetzner-endpoint.com
Storage__BucketName=platelib
Storage__AccessKey=<key>
Storage__SecretKey=<secret>
Storage__ForcePathStyle=false
```

## Authentication

Write endpoints (`POST`/`DELETE`) are protected by the `Maintainer` authorization policy. Read endpoints are public.

```csharp
app.MapPost("/api/plates", Handle)
    .RequireAuthorization("Maintainer");
```

The policy uses **JWT Bearer** (WorkOS in production). In development a static API key scheme is enabled (`Auth:AllowDevKey=true` in `appsettings.Development.json`) so maintainer endpoints can be tested without a real OIDC flow:

```
Authorization: Bearer dev-secret-key-change-me
```

**Never set `Auth:AllowDevKey=true` in production.** Production requires `Auth:Authority` and `Auth:ClientId` to be set to the WorkOS OIDC values.



Plate characteristic enums (`PlateMaterialEnum`, `PlateColorEnum`, `PlateSkirtEnum`) live in `Common/Enums`. Store enums as integers in PostgreSQL; use descriptive names in API responses by serializing as strings where appropriate.

## Validation

Use FluentValidation for all request models. Validators are auto-registered via `AddValidatorsFromAssemblyContaining<WebApplication>()`.

## OpenAPI / Scalar

API docs are served at `/api` via Scalar. Annotate endpoints with `.WithSummary()` and `.WithDescription()` for useful documentation. Always assign `.WithTags(...)` to group endpoints logically.

**OpenAPI annotations directly drive TypeScript type generation on the frontend** — when `pnpm generate:api` is run, the TypeScript schema is generated from these annotations. This means:
- Endpoint summaries and descriptions improve auto-complete in the frontend client.
- All response types must be explicitly declared (use `TypedResults` or `Produces<T>`) so that `openapi-typescript` can generate accurate TypeScript interfaces.
- Prefer `TypedResults.Ok<T>(...)` over untyped `Results.Ok(...)` to ensure response types appear correctly in the generated schema.

## Testing

### Unit Tests (`Tests/PlateLib.UnitTests`)

- **All business logic and common helper functions must have unit tests.**
- Mirror the feature slice structure: test files live in `Tests/PlateLib.UnitTests/Features/<Feature>/` and `Tests/PlateLib.UnitTests/Common/`.
- Use **NSubstitute** for mocking dependencies (e.g., `IPlateRepository`).
- Use **FluentAssertions** for readable assertions.
- Test validators, mapping logic (`MapToDTO`), and any helper/utility functions in `Common`.
- Run with: `dotnet test Tests/PlateLib.UnitTests`

### Integration Tests (`Tests/PlateLib.IntegrationTests`)

- **Every new feature must have minimal integration tests covering the most critical HTTP behaviour** (happy path + key error cases like 404 or 400).
- Integration tests require Docker
- Mirror the feature slice structure: test files live in `Tests/PlateLib.IntegrationTests/Features/<Feature>/`.
- Run with: `dotnet test Tests/PlateLib.IntegrationTests`

#### Shared AppHost fixture (required pattern)

The Aspire AppHost is expensive to start. **All integration tests share a single AppHost instance** via xUnit's `ICollectionFixture`. Never create a `DistributedApplication` inside an individual test.

The fixture is defined in `Tests/PlateLib.IntegrationTests/AspireAppFixture.cs`. It configures an HTTP resilience handler, starts the app, and waits for the `"api"` resource to be healthy before any test runs:

#### Test class pattern

Decorate every integration test class with `[Collection(AspireAppCollection.CollectionName)]` and inject `AspireAppFixture` via the primary constructor. Use `fixture.CreateApiClient()` to get a fresh client per test — pass the dev API key for authenticated tests.

Each test must be **self-contained and isolated** — use `Guid.NewGuid()` to generate unique names/IDs so tests do not interfere with shared database state.

```csharp
[Collection(AspireAppCollection.CollectionName)]
public class PlatesApiTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task GetPlates_ReturnsOk()
    {
        var client = fixture.CreateApiClient();            // unauthenticated
        var response = await client.GetAsync("/api/plates");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreatePlate_WithAuth_ReturnsCreated()
    {
        var client = fixture.CreateMaintainerClient();    // dev bearer token
        // ...
    }
}
```
