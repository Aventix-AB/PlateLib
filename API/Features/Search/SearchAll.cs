using API.Domain;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Search;

public static class SearchAll
{
    /// <summary>
    /// A flat discriminated search result. <see cref="EntityType"/> identifies whether the result is a
    /// <see cref="SearchEntityType.Plate"/> or <see cref="SearchEntityType.Manufacturer"/>.
    /// Plate-specific fields (<see cref="CatalogNumber"/>, <see cref="WellCount"/>, <see cref="ManufacturerId"/>,
    /// <see cref="ManufacturerName"/>) are null for manufacturer results.
    /// Manufacturer-specific fields (<see cref="WebsiteUrl"/>) are null for plate results.
    /// </summary>
    public record SearchResultItem(
        SearchEntityType EntityType,
        Guid Id,
        string Name,
        bool HasThumbnail,
        // Plate fields
        string? CatalogNumber,
        int? WellCount,
        Guid? ManufacturerId,
        string? ManufacturerName,
        // Manufacturer fields
        string? WebsiteUrl);

    public record SearchResponse(List<SearchResultItem> Items, int TotalCount);

    public static IEndpointRouteBuilder MapSearch(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/search", Handle)
            .WithName("Search")
            .WithSummary("Search plates and manufacturers")
            .WithDescription("Full-text search across plates (name, catalog number) and manufacturers (name). Returns a combined list ordered by name.")
            .WithTags("Search")
            .Produces<SearchResponse>();

        return app;
    }

    private static async Task<IResult> Handle(
        PlateLibContext db,
        string? q,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        if (string.IsNullOrWhiteSpace(q))
            return TypedResults.Ok(new SearchResponse([], 0));

        // Build a prefix-matching tsquery: each word gets :* so partial terms match.
        // e.g. "CLS35 cor" → "CLS35:* & cor:*"
        var tsQuery = string.Join(
            " & ",
            q.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)
             .Select(term => $"{term}:*"));

        var plateResults = await db.Plates
            .Include(p => p.Manufacturer)
            .Where(p => p.SearchVector.Matches(EF.Functions.ToTsQuery("english", tsQuery)))
            .OrderBy(p => p.Name)
            .Select(p => new SearchResultItem(
                SearchEntityType.Plate,
                p.Id,
                p.Name,
                p.ThumbnailStorageKey != null,
                p.CatalogNumber,
                p.WellCount,
                p.ManufacturerId,
                p.Manufacturer.Name,
                null))
            .ToListAsync(ct);

        var manufacturerResults = await db.Manufacturers
            .Where(m => m.SearchVector.Matches(EF.Functions.ToTsQuery("english", tsQuery)))
            .OrderBy(m => m.Name)
            .Select(m => new SearchResultItem(
                SearchEntityType.Manufacturer,
                m.Id,
                m.Name,
                m.ThumbnailStorageKey != null,
                null,
                null,
                null,
                null,
                m.WebsiteUrl))
            .ToListAsync(ct);

        var items = plateResults
            .Concat(manufacturerResults)
            .OrderBy(r => r.Name)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        var totalCount = plateResults.Count + manufacturerResults.Count;

        return TypedResults.Ok(new SearchResponse(items, totalCount));
    }
}
