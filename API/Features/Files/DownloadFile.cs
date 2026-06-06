using Microsoft.EntityFrameworkCore;

namespace API.Features.Files;

public static class DownloadFile
{
    public static IEndpointRouteBuilder MapDownloadFile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/files/{id:guid}/download", Handle)
            .WithName("DownloadFile")
            .WithSummary("Download a file by ID")
            .WithTags("Files");

        return app;
    }

    private static async Task<IResult> Handle(Guid id, PlateLibContext db, CancellationToken ct)
    {
        var file = await db.Files
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (file is null)
            return Results.NotFound();

        return Results.File(file.FileContent, file.ContentType, file.FileName);
    }
}
