using Microsoft.EntityFrameworkCore;

namespace API.Features.Files;

public static class GetFilesForPlate
{
    public static IEndpointRouteBuilder MapGetFilesForPlate(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/plates/{plateId:guid}/files", Handle)
            .WithName("GetFilesForPlate")
            .WithSummary("Get all files attached to a plate")
            .WithTags("Files");

        return app;
    }

    private static async Task<IResult> Handle(Guid plateId, PlateLibContext db, CancellationToken ct)
    {
        var plateExists = await db.Plates.AnyAsync(p => p.Id == plateId, ct);
        if (!plateExists)
            return Results.NotFound();

        var files = await db.Files
            .Where(f => f.Plates.Any(p => p.Id == plateId))
            .OrderBy(f => f.FileName)
            .Select(f => new FileResponse(f.Id, f.FileName, f.ContentType))
            .ToListAsync(ct);

        return Results.Ok(files);
    }
}
