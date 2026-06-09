using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using API.Domain;
using API.Features.Search;

namespace PlateLib.IntegrationTests.Features.Search;

[Collection(AspireAppCollection.CollectionName)]
public class SearchApiTests(AspireAppFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public async Task Search_WithNoQuery_ReturnsOkWithEmptyResults()
    {
        var client = fixture.CreateApiClient();

        var response = await client.GetAsync("/api/search");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SearchAll.SearchResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Search_WithKnownManufacturerName_ReturnsManufacturerResult()
    {
        var client = fixture.CreateApiClient();

        var response = await client.GetAsync("/api/search?q=Greiner");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SearchAll.SearchResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.Contains(result.Items, r =>
            r.EntityType == SearchEntityType.Manufacturer &&
            r.Name.Contains("Greiner", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Search_WithPartialCatalogNumber_ReturnsPlateResult()
    {
        var client = fixture.CreateApiClient();

        // Seed catalog number is "651101" — search with a prefix to verify partial matching
        var response = await client.GetAsync("/api/search?q=6511");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SearchAll.SearchResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.Contains(result.Items, r =>
            r.EntityType == SearchEntityType.Plate &&
            r.CatalogNumber == "651101");
    }

    [Fact]
    public async Task Search_WithUnmatchedQuery_ReturnsEmptyResults()
    {
        var client = fixture.CreateApiClient();

        var response = await client.GetAsync("/api/search?q=zzznomatch999");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SearchAll.SearchResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }
}

