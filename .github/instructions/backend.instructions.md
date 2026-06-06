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
5. Create a new migration: `dotnet ef migrations add <MigrationName> --project Data --startup-project MigrationService`

## Enums

Plate characteristic enums (`PlateMaterialEnum`, `PlateColorEnum`, `PlateSkirtEnum`) live in `Common/Enums`. Store enums as integers in PostgreSQL; use descriptive names in API responses by serializing as strings where appropriate.

## Validation

Use FluentValidation for all request models. Validators are auto-registered via `AddValidatorsFromAssemblyContaining<WebApplication>()`.

## OpenAPI / Scalar

API docs are served at `/api` via Scalar. Annotate endpoints with `.WithSummary()` and `.WithDescription()` for useful documentation. Always assign `.WithTags(...)` to group endpoints logically.

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
- Use `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` to spin up the full Aspire application.
- Obtain a typed `HttpClient` via `app.CreateHttpClient("api")`.
- Mirror the feature slice structure: test files live in `Tests/PlateLib.IntegrationTests/Features/<Feature>/`.
- Integration tests require Docker (PostgreSQL container started by Aspire).
- Run with: `dotnet test Tests/PlateLib.IntegrationTests`

Example integration test pattern:
```csharp
public class PlatesApiTests
{
    [Fact]
    public async Task GetPlates_ReturnsOk()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");
        var response = await client.GetAsync("/api/plates");

        response.EnsureSuccessStatusCode();
    }
}
```
