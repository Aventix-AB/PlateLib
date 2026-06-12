using API.Features.Files;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Plates;

public static class GetPlateById
{

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

        var response = new PlateDetailResponse(
            plate.Id,
            plate.Name,
            plate.CatalogNumber,
            plate.ProductUrl,
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
