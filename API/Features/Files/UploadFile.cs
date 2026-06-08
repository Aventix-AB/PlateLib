using API.Storage;
using Microsoft.EntityFrameworkCore;
using LibFile = Data.Entities.File;

namespace API.Features.Files;

public static class UploadFile
{
    public record UploadedFileResponse(Guid Id, string FileName, string ContentType, long FileSizeBytes);

    public static IEndpointRouteBuilder MapUploadFile(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/plates/{plateId:guid}/files", Handle)
            .WithName("UploadFile")
            .WithSummary("Upload a file and attach it to a plate")
            .WithDescription("Uploads a file to blob storage and links it to the specified plate. Requires maintainer authorization.")
            .WithTags("Files")
            .RequireAuthorization("Maintainer")
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> Handle(
        Guid plateId,
        IFormFile file,
        PlateLibContext db,
        IStorageService storage,
        CancellationToken ct)
    {
        var plate = await db.Plates.FirstOrDefaultAsync(p => p.Id == plateId, ct);
        if (plate is null)
            return Results.NotFound();

        if (file.Length == 0)
            return Results.Problem("File must not be empty.", statusCode: StatusCodes.Status400BadRequest);

        var fileId = Guid.NewGuid();
        var storageKey = $"plates/{plateId}/{fileId}/{file.FileName}";

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

        storedFile.Plates.Add(plate);
        db.Files.Add(storedFile);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/files/{fileId}/download",
            new UploadedFileResponse(storedFile.Id, storedFile.FileName, storedFile.ContentType, storedFile.FileSizeBytes));
    }
}
