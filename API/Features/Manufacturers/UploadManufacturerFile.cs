using API.Storage;
using Microsoft.EntityFrameworkCore;
using LibFile = Data.Entities.File;

namespace API.Features.Manufacturers;

public static class UploadManufacturerFile
{
    public record UploadedFileResponse(Guid Id, string FileName, string ContentType, long FileSizeBytes);

    public static IEndpointRouteBuilder MapUploadManufacturerFile(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/manufacturers/{manufacturerId:guid}/files", Handle)
            .WithName("UploadManufacturerFile")
            .WithSummary("Upload a file and attach it to a manufacturer")
            .WithDescription("Uploads a file to blob storage and links it to the specified manufacturer. Requires maintainer authorization.")
            .WithTags("Manufacturers")
            .RequireAuthorization("Maintainer")
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> Handle(
        Guid manufacturerId,
        IFormFile file,
        PlateLibContext db,
        IStorageService storage,
        CancellationToken ct)
    {
        var manufacturer = await db.Manufacturers.FirstOrDefaultAsync(m => m.Id == manufacturerId, ct);
        if (manufacturer is null)
            return Results.NotFound();

        if (file.Length == 0)
            return Results.Problem("File must not be empty.", statusCode: StatusCodes.Status400BadRequest);

        var fileId = Guid.NewGuid();
        var storageKey = $"manufacturers/{manufacturerId}/{fileId}/{file.FileName}";

        await using var stream = file.OpenReadStream();
        await storage.UploadAsync(storageKey, stream, file.ContentType, ct);

        var storedFile = new LibFile
        {
            Id = fileId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            StorageKey = storageKey,
            FileSizeBytes = file.Length,
        };

        storedFile.Manufacturers.Add(manufacturer);
        db.Files.Add(storedFile);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/files/{fileId}/download",
            new UploadedFileResponse(storedFile.Id, storedFile.FileName, storedFile.ContentType, storedFile.FileSizeBytes));
    }
}
