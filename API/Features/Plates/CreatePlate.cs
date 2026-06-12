using Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Plates;

public static class CreatePlate
{
    public record CreatePlateRequest(
        string Name,
        string CatalogNumber,
        string ProductUrl,
        int WellCount,
        Guid ManufacturerId,
        Guid MaterialId,
        Dictionary<string, string>? Properties);

    public record CreatePlateResponse(Guid Id, string Name, string CatalogNumber);

    public class CreatePlateValidator : AbstractValidator<CreatePlateRequest>
    {
        public CreatePlateValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CatalogNumber).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ProductUrl)
                .NotEmpty()
                .MaximumLength(1024)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("ProductUrl must be a valid absolute URL.");
            RuleFor(x => x.WellCount).GreaterThan(0);
            RuleFor(x => x.ManufacturerId).NotEmpty();
            RuleFor(x => x.MaterialId).NotEmpty();
        }
    }

    public static IEndpointRouteBuilder MapCreatePlate(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/plates", Handle)
            .WithName("CreatePlate")
            .WithSummary("Create a new plate")
            .WithDescription("Creates a new plate entry in the catalog. Requires maintainer authorization.")
            .WithTags("Plates")
            .RequireAuthorization("Maintainer");

        return app;
    }

    private static async Task<IResult> Handle(
        CreatePlateRequest request,
        IValidator<CreatePlateRequest> validator,
        PlateLibContext db,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var manufacturerExists = await db.Manufacturers.AnyAsync(m => m.Id == request.ManufacturerId, ct);
        if (!manufacturerExists)
            return Results.Problem($"Manufacturer {request.ManufacturerId} not found.", statusCode: StatusCodes.Status422UnprocessableEntity);

        var materialExists = await db.Materials.AnyAsync(m => m.Id == request.MaterialId, ct);
        if (!materialExists)
            return Results.Problem($"Material {request.MaterialId} not found.", statusCode: StatusCodes.Status422UnprocessableEntity);

        var duplicate = await db.Plates.AnyAsync(p => p.CatalogNumber == request.CatalogNumber, ct);
        if (duplicate)
            return Results.Problem($"A plate with catalog number '{request.CatalogNumber}' already exists.", statusCode: StatusCodes.Status409Conflict);

        var plate = new Plate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CatalogNumber = request.CatalogNumber,
            ProductUrl = request.ProductUrl,
            WellCount = request.WellCount,
            ManufacturerId = request.ManufacturerId,
            MaterialId = request.MaterialId,
        };

        if (request.Properties is { Count: > 0 })
        {
            var definitionNames = request.Properties.Keys.ToList();
            var definitions = await db.PropertyDefinitions
                .Where(d => definitionNames.Contains(d.Name))
                .ToDictionaryAsync(d => d.Name, ct);

            foreach (var (name, value) in request.Properties)
            {
                if (!definitions.TryGetValue(name, out var def))
                    return Results.Problem($"Property definition '{name}' not found.", statusCode: StatusCodes.Status422UnprocessableEntity);

                plate.PlateProperties.Add(new PlateProperty
                {
                    PlateId = plate.Id,
                    PropertyDefinitionId = def.Id,
                    Value = value,
                });
            }
        }

        db.Plates.Add(plate);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/plates/{plate.Id}", new CreatePlateResponse(plate.Id, plate.Name, plate.CatalogNumber));
    }
}
