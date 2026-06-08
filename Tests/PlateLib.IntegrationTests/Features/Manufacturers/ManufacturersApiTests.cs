using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Aspire.Hosting.Testing;

namespace PlateLib.IntegrationTests.Features.Manufacturers;

public class ManufacturersApiTests
{
    private const string DevApiKey = "dev-secret-key-change-me";

    [Fact]
    public async Task GetManufacturers_ReturnsOk()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");

        var response = await client.GetAsync("/api/manufacturers");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateManufacturer_WithoutAuth_ReturnsUnauthorized()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");

        var response = await client.PostAsJsonAsync("/api/manufacturers", new { name = "TestCorp" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateManufacturer_WithAuth_CreatesAndReturnsCreated()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevApiKey);

        var uniqueName = $"TestCorp-{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/api/manufacturers", new { name = uniqueName });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateManufacturer_WithAuth_DuplicateName_ReturnsConflict()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevApiKey);

        var uniqueName = $"DupeCorp-{Guid.NewGuid():N}";

        await client.PostAsJsonAsync("/api/manufacturers", new { name = uniqueName });
        var dupeResponse = await client.PostAsJsonAsync("/api/manufacturers", new { name = uniqueName });

        Assert.Equal(HttpStatusCode.Conflict, dupeResponse.StatusCode);
    }
}
