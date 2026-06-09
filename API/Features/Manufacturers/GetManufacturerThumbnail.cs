using API.Storage;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Manufacturers;

public static class GetManufacturerThumbnail
{
    public static IEndpointRouteBuilder MapGetManufacturerThumbnail(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/manufacturers/{id:guid}/thumbnail", Handle)
            .WithName("GetManufacturerThumbnail")
            .WithSummary("Download the thumbnail for a manufacturer")
            .WithDescription("Redirects to a short-lived pre-signed URL for direct download of the manufacturer thumbnail from blob storage.")
            .WithTags("Manufacturers");

        return app;
    }

    private static async Task<IResult> Handle(Guid id, PlateLibContext db, IStorageService storage, CancellationToken ct)
    {
        var manufacturer = await db.Manufacturers
            .Select(m => new { m.Id, m.ThumbnailStorageKey })
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (manufacturer is null)
            return Results.NotFound();

        if (manufacturer.ThumbnailStorageKey is null)
            return Results.NotFound();

        var url = await storage.GeneratePresignedUrlAsync(manufacturer.ThumbnailStorageKey, TimeSpan.FromMinutes(15), ct);
        return Results.Redirect(url);
    }
}
