using Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Features.PropertyDefinitions;

public static class GetPropertyDefinitions
{
    public record PropertyDefinitionResponse(Guid Id, string Name, PropertyDataType DataType, string? Description);

    public static IEndpointRouteBuilder MapGetPropertyDefinitions(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/property-definitions", Handle)
            .WithName("GetPropertyDefinitions")
            .WithSummary("Get all plate property definitions")
            .WithTags("PropertyDefinitions");

        return app;
    }

    private static async Task<IResult> Handle(PlateLibContext db, CancellationToken ct)
    {
        var definitions = await db.PropertyDefinitions
            .OrderBy(d => d.Name)
            .Select(d => new PropertyDefinitionResponse(d.Id, d.Name, d.DataType, d.Description))
            .ToListAsync(ct);

        return Results.Ok(definitions);
    }
}
