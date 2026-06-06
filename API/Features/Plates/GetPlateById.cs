using Microsoft.EntityFrameworkCore;

namespace API.Features.Plates;

public static class GetPlateById
{
    public record FileResponse(Guid Id, string FileName, string ContentType);
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
        List<PlatePropertyResponse> Properties,
        List<FileResponse> Files);

    public static IEndpointRouteBuilder MapGetPlateById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/plates/{id:guid}", Handle)
            .WithName("GetPlateById")
            .WithSummary("Get a plate by ID")
            .WithTags("Plates");

        return app;
    }

    private static async Task<IResult> Handle(Guid id, PlateLibContext db, CancellationToken ct)
    {
        var plate = await db.Plates
            .Include(p => p.Manufacturer)
            .Include(p => p.Material)
            .Include(p => p.PlateProperties)
                .ThenInclude(pp => pp.PropertyDefinition)
            .Include(p => p.Files)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (plate is null)
            return Results.NotFound();

        var response = new PlateResponse(
            plate.Id,
            plate.Name,
            plate.CatalogNumber,
            plate.WellCount,
            new MaterialResponse(plate.Material.Code, plate.Material.Name),
            plate.ManufacturerId,
            plate.Manufacturer.Name,
            plate.PlateProperties
                .OrderBy(pp => pp.PropertyDefinition.Name)
                .Select(pp => new PlatePropertyResponse(pp.PropertyDefinition.Name, pp.Value))
                .ToList(),
            plate.Files
                .Select(f => new FileResponse(f.Id, f.FileName, f.ContentType))
                .ToList());

        return Results.Ok(response);
    }
}
