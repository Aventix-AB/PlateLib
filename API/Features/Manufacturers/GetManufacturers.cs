using Microsoft.EntityFrameworkCore;

namespace API.Features.Manufacturers;

public static class GetManufacturers
{
    public record ManufacturerResponse(Guid Id, string Name, string? WebsiteUrl, bool HasThumbnail);

    public static IEndpointRouteBuilder MapGetManufacturers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/manufacturers", Handle)
            .WithName("GetManufacturers")
            .WithSummary("Get all manufacturers")
            .WithTags("Manufacturers");

        return app;
    }

    private static async Task<IResult> Handle(PlateLibContext db, CancellationToken ct)
    {
        var manufacturers = await db.Manufacturers
            .OrderBy(m => m.Name)
            .Select(m => new ManufacturerResponse(m.Id, m.Name, m.WebsiteUrl, m.ThumbnailStorageKey != null))
            .ToListAsync(ct);

        return Results.Ok(manufacturers);
    }
}
