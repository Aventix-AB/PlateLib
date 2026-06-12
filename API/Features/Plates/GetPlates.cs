using Microsoft.EntityFrameworkCore;

namespace API.Features.Plates;

public static class GetPlates
{
    public record PagedResult<T>(List<T> Items, int TotalCount, int PageIndex, int PageSize);

    public static IEndpointRouteBuilder MapGetPlates(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/plates", Handle)
            .WithName("GetPlates")
            .WithSummary("Get plates with optional filtering and pagination")
            .WithDescription("Returns a paged list of plates. Supports filtering by search text (name/catalog number), manufacturer, and well count.")
            .WithTags("Plates");

        return app;
    }

    private static async Task<IResult> Handle(
        PlateLibContext db,
        string? search,
        Guid? manufacturerId,
        int? wellCount,
        int pageIndex = 0,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Plates
            .Include(p => p.Manufacturer)
            .Include(p => p.Material)
            .Include(p => p.PlateProperties)
                .ThenInclude(pp => pp.PropertyDefinition)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(lower) ||
                p.CatalogNumber.ToLower().Contains(lower));
        }

        if (manufacturerId.HasValue)
            query = query.Where(p => p.ManufacturerId == manufacturerId.Value);

        if (wellCount.HasValue)
            query = query.Where(p => p.WellCount == wellCount.Value);

        var totalCount = await query.CountAsync(ct);

        var plates = await query
            .OrderBy(p => p.Name)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(p => new PlateResponse(
                p.Id,
                p.Name,
                p.CatalogNumber,
                p.ProductUrl,
                p.WellCount,
                new MaterialResponse(p.Material.Code, p.Material.Name),
                p.ManufacturerId,
                p.Manufacturer.Name,
                p.PlateProperties
                    .OrderBy(pp => pp.PropertyDefinition.Name)
                    .Select(pp => new PlatePropertyResponse(pp.PropertyDefinition.Name, pp.Value))
                    .ToList()))
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<PlateResponse>(plates, totalCount, pageIndex, pageSize));
    }
}
