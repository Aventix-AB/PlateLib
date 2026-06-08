using Microsoft.EntityFrameworkCore;

namespace API.Features.Manufacturers;

public static class GetManufacturerById
{
    public record PlateResponse(Guid Id, string Name, string CatalogNumber, int WellCount);
    public record ManufacturerResponse(Guid Id, string Name, string? WebsiteUrl, bool HasThumbnail, List<PlateResponse> Plates);

    public static IEndpointRouteBuilder MapGetManufacturerById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/manufacturers/{id:guid}", Handle)
            .WithName("GetManufacturerById")
            .WithSummary("Get a manufacturer by ID")
            .WithTags("Manufacturers");

        return app;
    }

    private static async Task<IResult> Handle(Guid id, PlateLibContext db, CancellationToken ct)
    {
        var manufacturer = await db.Manufacturers
            .Include(m => m.Plates)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (manufacturer is null)
            return Results.NotFound();

        var response = new ManufacturerResponse(
            manufacturer.Id,
            manufacturer.Name,
            manufacturer.WebsiteUrl,
            manufacturer.ThumbnailStorageKey != null,
            manufacturer.Plates
                .OrderBy(p => p.Name)
                .Select(p => new PlateResponse(p.Id, p.Name, p.CatalogNumber, p.WellCount))
                .ToList());

        return Results.Ok(response);
    }
}
