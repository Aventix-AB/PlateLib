using System.Net;

namespace PlateLib.IntegrationTests.Features.Files;

[Collection(AspireAppCollection.CollectionName)]
public class FilesApiTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task GetFilesForPlate_WithUnknownPlateId_ReturnsNotFound()
    {
        var client = fixture.CreateApiClient();
        var response = await client.GetAsync($"/api/plates/{Guid.NewGuid()}/files");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DownloadFile_WithUnknownId_ReturnsNotFound()
    {
        var client = fixture.CreateApiClient();
        var response = await client.GetAsync($"/api/files/{Guid.NewGuid()}/download");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UploadFile_WithoutAuth_ReturnsUnauthorized()
    {
        var client = fixture.CreateApiClient();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x25, 0x50, 0x44, 0x46]), "file", "test.pdf");

        var response = await client.PostAsync($"/api/plates/{Guid.NewGuid()}/files", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadFile_WithAuth_ToUnknownPlate_ReturnsNotFound()
    {
        var client = fixture.CreateMaintainerClient();

        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([0x25, 0x50, 0x44, 0x46]), "file", "test.pdf");

        var response = await client.PostAsync($"/api/plates/{Guid.NewGuid()}/files", content);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFile_WithoutAuth_ReturnsUnauthorized()
    {
        var client = fixture.CreateApiClient();
        var response = await client.DeleteAsync($"/api/plates/{Guid.NewGuid()}/files/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
