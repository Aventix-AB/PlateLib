using System.Net;
using System.Net.Http.Json;

namespace PlateLib.IntegrationTests.Features.Manufacturers;

[Collection(AspireAppCollection.CollectionName)]
public class ManufacturersApiTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task GetManufacturers_ReturnsOk()
    {
        var client = fixture.CreateApiClient();
        var response = await client.GetAsync("/api/manufacturers");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateManufacturer_WithoutAuth_ReturnsUnauthorized()
    {
        var client = fixture.CreateApiClient();
        var response = await client.PostAsJsonAsync("/api/manufacturers", new { name = "TestCorp" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateManufacturer_WithAuth_CreatesAndReturnsCreated()
    {
        var client = fixture.CreateMaintainerClient();
        var uniqueName = $"TestCorp-{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/api/manufacturers", new { name = uniqueName });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateManufacturer_WithAuth_DuplicateName_ReturnsConflict()
    {
        var client = fixture.CreateMaintainerClient();
        var uniqueName = $"DupeCorp-{Guid.NewGuid():N}";

        await client.PostAsJsonAsync("/api/manufacturers", new { name = uniqueName });
        var dupeResponse = await client.PostAsJsonAsync("/api/manufacturers", new { name = uniqueName });

        Assert.Equal(HttpStatusCode.Conflict, dupeResponse.StatusCode);
    }
}
