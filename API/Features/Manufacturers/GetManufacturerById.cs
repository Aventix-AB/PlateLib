using Microsoft.EntityFrameworkCore;

namespace API.Features.Manufacturers;

public static class GetManufacturerById
{
    public record ManufacturerResponse(Guid Id, string Name);

    public static IEndpointRouteBuilder MapGetManufacturerById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/manufacturers/{id:guid}", Handle)
            .WithName("GetManufacturerById")
            .WithSummary("Get a manufacturer by ID")
            .WithTags("Manufacturers");

        return app;
    }

    private static async Task<IResult> Handle(Guid id, PlateLibContext db, CancellationToken ct)
    {
        var manufacturer = await db.Manufacturers
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (manufacturer is null)
            return Results.NotFound();

        return Results.Ok(new ManufacturerResponse(manufacturer.Id, manufacturer.Name));
    }
}
