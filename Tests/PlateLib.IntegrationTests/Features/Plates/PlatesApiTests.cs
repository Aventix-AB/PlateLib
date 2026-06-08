namespace PlateLib.IntegrationTests.Features.Plates;

[Collection(AspireAppCollection.CollectionName)]
public class PlatesApiTests(AspireAppFixture fixture)
{
    [Fact]
    public async Task GetPlates_ReturnsOk()
    {
        var client = fixture.CreateApiClient();
        var response = await client.GetAsync("/api/plates");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetPlateById_WithUnknownId_ReturnsNotFound()
    {
        var client = fixture.CreateApiClient();
        var response = await client.GetAsync($"/api/plates/{Guid.NewGuid()}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
