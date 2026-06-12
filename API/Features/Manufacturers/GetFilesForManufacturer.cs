using Microsoft.EntityFrameworkCore;
using API.Features.Files;

namespace API.Features.Manufacturers;

public static class GetFilesForManufacturer
{
    public static IEndpointRouteBuilder MapGetFilesForManufacturer(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/manufacturers/{manufacturerId:guid}/files", Handle)
            .WithName("GetFilesForManufacturer")
            .WithSummary("Get all files attached to a manufacturer")
            .WithTags("Manufacturers");

        return app;
    }

    private static async Task<IResult> Handle(Guid manufacturerId, PlateLibContext db, CancellationToken ct)
    {
        var manufacturerExists = await db.Manufacturers.AnyAsync(m => m.Id == manufacturerId, ct);
        if (!manufacturerExists)
            return Results.NotFound();

        var files = await db.Files
            .Where(f => f.Manufacturers.Any(m => m.Id == manufacturerId))
            .OrderBy(f => f.FileName)
            .Select(f => new FileResponse(f.Id, f.FileName, f.ContentType))
            .ToListAsync(ct);

        return Results.Ok(files);
    }
}
