using System.Net;
using System.Net.Http.Headers;
using Aspire.Hosting.Testing;

namespace PlateLib.IntegrationTests.Features.Files;

public class FilesApiTests
{
    private const string DevApiKey = "dev-secret-key-change-me";

    [Fact]
    public async Task GetFilesForPlate_WithUnknownPlateId_ReturnsNotFound()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");

        var response = await client.GetAsync($"/api/plates/{Guid.NewGuid()}/files");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DownloadFile_WithUnknownId_ReturnsNotFound()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");

        var response = await client.GetAsync($"/api/files/{Guid.NewGuid()}/download");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UploadFile_WithoutAuth_ReturnsUnauthorized()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x25, 0x50, 0x44, 0x46]), "file", "test.pdf");

        var response = await client.PostAsync($"/api/plates/{Guid.NewGuid()}/files", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadFile_WithAuth_ToUnknownPlate_ReturnsNotFound()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevApiKey);

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x25, 0x50, 0x44, 0x46]), "file", "test.pdf");

        var response = await client.PostAsync($"/api/plates/{Guid.NewGuid()}/files", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFile_WithoutAuth_ReturnsUnauthorized()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var client = app.CreateHttpClient("api");

        var response = await client.DeleteAsync($"/api/plates/{Guid.NewGuid()}/files/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
