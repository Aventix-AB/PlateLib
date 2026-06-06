using Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Plates;

public static class GetPlates
{
    public record PlateResponse(
        Guid Id,
        string Name,
        string CatalogNumber,
        int WellNumber,
        PlateMaterialEnum Material,
        bool Lid,
        PlateColorEnum Color,
        PlateSkirtEnum Skirt,
        bool Sterile,
        Guid ManufacturerId,
        string ManufacturerName);

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
            .OrderBy(p => p.Name)
            .Select(p => new PlateResponse(
                p.Id,
                p.Name,
                p.CatalogNumber,
                p.Wellnumber,
                p.Material,
                p.Lid,
                p.Color,
                p.Skirt,
                p.Sterile,
                p.ManufacturerId,
                p.Manufacturer.Name))
            .ToListAsync(ct);

        return Results.Ok(plates);
    }
}
