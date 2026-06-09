using API.Storage;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Manufacturers;

public static class UploadManufacturerThumbnail
{
    public record ThumbnailUploadResponse(Guid ManufacturerId, string ThumbnailUrl);

    public static IEndpointRouteBuilder MapUploadManufacturerThumbnail(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/manufacturers/{id:guid}/thumbnail", Handle)
            .WithName("UploadManufacturerThumbnail")
            .WithSummary("Upload or replace the thumbnail for a manufacturer")
            .WithDescription("Uploads a logo/thumbnail image to blob storage and links it to the manufacturer. Replaces any existing thumbnail. Requires maintainer authorization.")
            .WithTags("Manufacturers")
            .RequireAuthorization("Maintainer")
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> Handle(
        Guid id,
        IFormFile file,
        PlateLibContext db,
        IStorageService storage,
        CancellationToken ct)
    {
        var manufacturer = await db.Manufacturers.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (manufacturer is null)
            return Results.NotFound();

        if (file.Length == 0)
            return Results.Problem("File must not be empty.", statusCode: StatusCodes.Status400BadRequest);

        // Delete old thumbnail from storage if present
        if (manufacturer.ThumbnailStorageKey is not null)
            await storage.DeleteAsync(manufacturer.ThumbnailStorageKey, ct);

        var storageKey = $"manufacturers/{id}/thumbnail/{file.FileName}";

        await using var stream = file.OpenReadStream();
        await storage.UploadAsync(storageKey, stream, file.ContentType, ct);

        manufacturer.ThumbnailStorageKey = storageKey;
        await db.SaveChangesAsync(ct);

        return Results.Ok(new ThumbnailUploadResponse(manufacturer.Id, $"/api/manufacturers/{manufacturer.Id}/thumbnail"));
    }
}
