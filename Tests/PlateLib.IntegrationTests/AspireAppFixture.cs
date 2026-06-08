using System.Net.Http.Headers;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace PlateLib.IntegrationTests;

public sealed class AspireAppFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(3);
    private const string DevApiKey = "dev-secret-key-change-me";

    public DistributedApplication App { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AppHost>();

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        App = await appHost.BuildAsync();
        await App.StartAsync();

        await App.ResourceNotifications
            .WaitForResourceHealthyAsync("api")
            .WaitAsync(DefaultTimeout);
    }

    /// <summary>Returns a fresh unauthenticated HttpClient for the API.</summary>
    public HttpClient CreateApiClient() => App.CreateHttpClient("api");

    /// <summary>Returns a fresh HttpClient pre-authorized with the dev maintainer bearer token.</summary>
    public HttpClient CreateMaintainerClient()
    {
        var client = App.CreateHttpClient("api");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DevApiKey);
        return client;
    }

    public async Task DisposeAsync() => await App.DisposeAsync();
}

[CollectionDefinition(CollectionName)]
public sealed class AspireAppCollection : ICollectionFixture<AspireAppFixture>
{
    public const string CollectionName = "Aspire App";
}
