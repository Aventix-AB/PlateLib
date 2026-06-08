using API.Storage;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Files;

public static class DownloadFile
{
    public static IEndpointRouteBuilder MapDownloadFile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/files/{id:guid}/download", Handle)
            .WithName("DownloadFile")
            .WithSummary("Download a file by ID")
            .WithDescription("Redirects to a short-lived pre-signed URL for direct download from blob storage.")
            .WithTags("Files");

        return app;
    }

    private static async Task<IResult> Handle(Guid id, PlateLibContext db, IStorageService storage, CancellationToken ct)
    {
        var file = await db.Files
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (file is null)
            return Results.NotFound();

        var url = await storage.GeneratePresignedUrlAsync(file.StorageKey, TimeSpan.FromMinutes(15), ct);
        return Results.Redirect(url);
    }
}
