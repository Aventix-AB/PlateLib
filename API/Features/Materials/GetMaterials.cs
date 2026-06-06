using Microsoft.EntityFrameworkCore;

namespace API.Features.Materials;

public static class GetMaterials
{
    public record MaterialResponse(Guid Id, string Code, string Name, string? Description);

    public static IEndpointRouteBuilder MapGetMaterials(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/materials", Handle)
            .WithName("GetMaterials")
            .WithSummary("Get all plate materials")
            .WithTags("Materials");

        return app;
    }

    private static async Task<IResult> Handle(PlateLibContext db, CancellationToken ct)
    {
        var materials = await db.Materials
            .OrderBy(m => m.Code)
            .Select(m => new MaterialResponse(m.Id, m.Code, m.Name, m.Description))
            .ToListAsync(ct);

        return Results.Ok(materials);
    }
}
