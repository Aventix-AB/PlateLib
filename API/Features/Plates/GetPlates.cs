using Microsoft.EntityFrameworkCore;

namespace API.Features.Plates;

public static class GetPlates
{
    public record MaterialResponse(string Code, string Name);
    public record PlatePropertyResponse(string Name, string Value);

    public record PlateResponse(
        Guid Id,
        string Name,
        string CatalogNumber,
        int WellCount,
        MaterialResponse Material,
        Guid ManufacturerId,
        string ManufacturerName,
        List<PlatePropertyResponse> Properties);

    public static IEndpointRouteBuilder MapGetPlates(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/plates", Handle)
            .WithName("GetPlates")
            .WithSummary("Get all plates")
            .WithTags("Plates");

        return app;
    }

    private static async Task<IResult> Handle(OpenPlateContext db, CancellationToken ct)
    {
        var plates = await db.Plates
            .Include(p => p.Manufacturer)
            .Include(p => p.Material)
            .Include(p => p.PlateProperties)
                .ThenInclude(pp => pp.PropertyDefinition)
            .OrderBy(p => p.Name)
            .Select(p => new PlateResponse(
                p.Id,
                p.Name,
                p.CatalogNumber,
                p.WellCount,
                new MaterialResponse(p.Material.Code, p.Material.Name),
                p.ManufacturerId,
                p.Manufacturer.Name,
                p.PlateProperties
                    .OrderBy(pp => pp.PropertyDefinition.Name)
                    .Select(pp => new PlatePropertyResponse(pp.PropertyDefinition.Name, pp.Value))
                    .ToList()))
            .ToListAsync(ct);

        return Results.Ok(plates);
    }
}
