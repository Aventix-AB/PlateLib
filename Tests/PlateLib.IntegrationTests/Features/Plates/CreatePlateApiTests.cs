using System.Net;
using System.Net.Http.Json;

namespace PlateLib.IntegrationTests.Features.Plates;

[Collection(AspireAppCollection.CollectionName)]
public class CreatePlateApiTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task CreatePlate_WithoutAuth_ReturnsUnauthorized()
    {
        var client = fixture.CreateApiClient();

        var response = await client.PostAsJsonAsync("/api/plates", new
        {
            name = "Test Plate",
            catalogNumber = "TEST-001",
            productUrl = "https://example.com/products/test-001",
            wellCount = 96,
            manufacturerId = Guid.NewGuid(),
            materialId = Guid.NewGuid(),
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlate_WithAuth_InvalidBody_ReturnsBadRequest()
    {
        var client = fixture.CreateMaintainerClient();

        // wellCount 0 is invalid
        var response = await client.PostAsJsonAsync("/api/plates", new
        {
            name = "Test Plate",
            catalogNumber = "TEST-001",
            productUrl = "https://example.com/products/test-001",
            wellCount = 0,
            manufacturerId = Guid.NewGuid(),
            materialId = Guid.NewGuid(),
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlate_WithAuth_UnknownManufacturer_ReturnsUnprocessableEntity()
    {
        var client = fixture.CreateMaintainerClient();

        var response = await client.PostAsJsonAsync("/api/plates", new
        {
            name = "Test Plate",
            catalogNumber = $"TEST-{Guid.NewGuid():N}",
            productUrl = "https://example.com/products/test-001",
            wellCount = 96,
            manufacturerId = Guid.NewGuid(),
            materialId = Guid.NewGuid(),
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
