using API.Storage;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Files;

public static class DeleteFile
{
    public static IEndpointRouteBuilder MapDeleteFile(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/plates/{plateId:guid}/files/{fileId:guid}", Handle)
            .WithName("DeleteFile")
            .WithSummary("Remove a file from a plate and delete it from storage")
            .WithDescription("Detaches the file from the plate. If the file has no remaining plate associations it is permanently deleted from blob storage. Requires maintainer authorization.")
            .WithTags("Files")
            .RequireAuthorization("Maintainer");

        return app;
    }

    private static async Task<IResult> Handle(
        Guid plateId,
        Guid fileId,
        PlateLibContext db,
        IStorageService storage,
        CancellationToken ct)
    {
        var plate = await db.Plates
            .Include(p => p.Files)
            .FirstOrDefaultAsync(p => p.Id == plateId, ct);

        if (plate is null)
            return Results.NotFound();

        var file = plate.Files.FirstOrDefault(f => f.Id == fileId);
        if (file is null)
            return Results.NotFound();

        plate.Files.Remove(file);
        await db.SaveChangesAsync(ct);

        // Only delete the blob if no other plates reference this file
        var remainingRefs = await db.Files
            .Where(f => f.Id == fileId)
            .SelectMany(f => f.Plates)
            .AnyAsync(ct);

        if (!remainingRefs)
        {
            await storage.DeleteAsync(file.StorageKey, ct);
            db.Files.Remove(file);
            await db.SaveChangesAsync(ct);
        }

        return Results.NoContent();
    }
}
