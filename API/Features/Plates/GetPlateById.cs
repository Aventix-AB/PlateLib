using Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Plates;

public static class GetPlateById
{
    public record PlateFileResponse(Guid Id, string FileName, string ContentType);

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
        string ManufacturerName,
        List<PlateFileResponse> PlateFiles);

    public static IEndpointRouteBuilder MapGetPlateById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/plates/{id:guid}", Handle)
            .WithName("GetPlateById")
            .WithSummary("Get a plate by ID")
            .WithTags("Plates");

        return app;
    }

    private static async Task<IResult> Handle(Guid id, OpenPlateContext db, CancellationToken ct)
    {
        var plate = await db.Plates
            .Include(p => p.Manufacturer)
            .Include(p => p.PlateFiles)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (plate is null)
            return Results.NotFound();

        var response = new PlateResponse(
            plate.Id,
            plate.Name,
            plate.CatalogNumber,
            plate.Wellnumber,
            plate.Material,
            plate.Lid,
            plate.Color,
            plate.Skirt,
            plate.Sterile,
            plate.ManufacturerId,
            plate.Manufacturer.Name,
            plate.PlateFiles.Select(f => new PlateFileResponse(f.Id, f.FileName, f.ContentType)).ToList());

        return Results.Ok(response);
    }
}
