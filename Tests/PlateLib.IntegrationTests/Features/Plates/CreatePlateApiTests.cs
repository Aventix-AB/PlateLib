using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Aspire.Hosting.Testing;

namespace PlateLib.IntegrationTests.Features.Plates;

public class CreatePlateApiTests
{
    private const string DevApiKey = "dev-secret-key-change-me";

    [Fact]
    public async Task CreatePlate_WithoutAuth_ReturnsUnauthorized()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");

        var response = await client.PostAsJsonAsync("/api/plates", new
        {
            name = "Test Plate",
            catalogNumber = "TEST-001",
            wellCount = 96,
            manufacturerId = Guid.NewGuid(),
            materialId = Guid.NewGuid(),
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlate_WithAuth_InvalidBody_ReturnsBadRequest()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevApiKey);

        // wellCount 0 is invalid
        var response = await client.PostAsJsonAsync("/api/plates", new
        {
            name = "Test Plate",
            catalogNumber = "TEST-001",
            wellCount = 0,
            manufacturerId = Guid.NewGuid(),
            materialId = Guid.NewGuid(),
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlate_WithAuth_UnknownManufacturer_ReturnsUnprocessableEntity()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevApiKey);

        var response = await client.PostAsJsonAsync("/api/plates", new
        {
            name = "Test Plate",
            catalogNumber = $"TEST-{Guid.NewGuid():N}",
            wellCount = 96,
            manufacturerId = Guid.NewGuid(),
            materialId = Guid.NewGuid(),
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
