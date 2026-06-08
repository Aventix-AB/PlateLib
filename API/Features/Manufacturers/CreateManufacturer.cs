using Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Manufacturers;

public static class CreateManufacturer
{
    public record CreateManufacturerRequest(string Name);

    public record CreateManufacturerResponse(Guid Id, string Name);

    public class CreateManufacturerValidator : AbstractValidator<CreateManufacturerRequest>
    {
        public CreateManufacturerValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }
    }

    public static IEndpointRouteBuilder MapCreateManufacturer(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/manufacturers", Handle)
            .WithName("CreateManufacturer")
            .WithSummary("Create a new manufacturer")
            .WithDescription("Adds a new manufacturer to the catalog. Requires maintainer authorization.")
            .WithTags("Manufacturers")
            .RequireAuthorization("Maintainer");

        return app;
    }

    private static async Task<IResult> Handle(
        CreateManufacturerRequest request,
        IValidator<CreateManufacturerRequest> validator,
        PlateLibContext db,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var duplicate = await db.Manufacturers.AnyAsync(m => m.Name == request.Name, ct);
        if (duplicate)
            return Results.Problem($"A manufacturer named '{request.Name}' already exists.", statusCode: StatusCodes.Status409Conflict);

        var manufacturer = new Manufacturer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        };

        db.Manufacturers.Add(manufacturer);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/manufacturers/{manufacturer.Id}", new CreateManufacturerResponse(manufacturer.Id, manufacturer.Name));
    }
}
