using Microsoft.EntityFrameworkCore;

namespace API.Features.PlateFiles;

public static class GetPlateFiles
{
    public record PlateFileResponse(Guid Id, string FileName, string ContentType);

    public static IEndpointRouteBuilder MapGetPlateFiles(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/plates/{plateId:guid}/files", Handle)
            .WithName("GetPlateFiles")
            .WithSummary("Get all files for a plate")
            .WithTags("PlateFiles");

        return app;
    }

    private static async Task<IResult> Handle(Guid plateId, OpenPlateContext db, CancellationToken ct)
    {
        var plateExists = await db.Plates.AnyAsync(p => p.Id == plateId, ct);
        if (!plateExists)
            return Results.NotFound();

        var files = await db.PlateFiles
            .Where(f => f.PlateId == plateId)
            .OrderBy(f => f.FileName)
            .Select(f => new PlateFileResponse(f.Id, f.FileName, f.ContentType))
            .ToListAsync(ct);

        return Results.Ok(files);
    }
}
