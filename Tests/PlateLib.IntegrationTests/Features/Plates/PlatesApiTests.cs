using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace PlateLib.IntegrationTests.Features.Plates;

public class PlatesApiTests
{
    [Fact]
    public async Task GetPlates_ReturnsOk()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("api");

        // Act
        var response = await httpClient.GetAsync("/api/plates");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetPlateById_WithUnknownId_ReturnsNotFound()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("api");

        // Act
        var response = await httpClient.GetAsync($"/api/plates/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
