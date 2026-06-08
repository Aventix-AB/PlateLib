using System.Text.Json.Serialization;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using API.Auth;
using API.Features.Files;
using API.Features.Manufacturers;
using API.Features.Materials;
using API.Features.Plates;
using API.Features.PropertyDefinitions;
using API.Storage;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.AddServiceDefaults();

// Add problem details for API responses
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
  {
      context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method}{context.HttpContext.Request.Path}";

      context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

      var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
      if (activity != null)
      {
          context.ProblemDetails.Extensions.TryAdd("traceId", activity.Id);
      }
  };
});

builder.AddNpgsqlDbContext<PlateLibContext>(
    connectionName: "postgresdb");

builder.Services.AddValidatorsFromAssemblyContaining<WebApplication>();

// Add health checks
builder.Services.AddHealthChecks();

// Add minimal API support
builder.Services.AddEndpointsApiExplorer();


builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Tell OpenApi generator to report number fields as integers/floats only, not strings
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

// Add OpenAPI support for Scalar
builder.Services.AddOpenApi();

// ── Blob Storage ────────────────────────────────────────────────────────────
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.Section));

var storageOptions = builder.Configuration.GetSection(StorageOptions.Section).Get<StorageOptions>() ?? new();

var s3Config = new AmazonS3Config
{
    ForcePathStyle = storageOptions.ForcePathStyle,
};

if (!string.IsNullOrEmpty(storageOptions.ServiceUrl))
    s3Config.ServiceURL = storageOptions.ServiceUrl;
else
    s3Config.RegionEndpoint = RegionEndpoint.EUCentral1;

var credentials = new BasicAWSCredentials(storageOptions.AccessKey, storageOptions.SecretKey);
builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(credentials, s3Config));
builder.Services.AddSingleton<IStorageService, S3StorageService>();

// ── Authentication ───────────────────────────────────────────────────────────
var authBuilder = builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:ClientId"];
        options.TokenValidationParameters.ValidateAudience = !string.IsNullOrEmpty(builder.Configuration["Auth:ClientId"]);
    });

if (builder.Configuration.GetValue<bool>("Auth:AllowDevKey"))
{
    authBuilder.AddScheme<DevApiKeyAuthOptions, DevApiKeyAuthHandler>(
        DevApiKeyAuthHandler.SchemeName,
        opts => opts.ApiKey = builder.Configuration["Auth:DevApiKey"] ?? string.Empty);
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Maintainer", policy =>
    {
        policy.RequireAuthenticatedUser();
        if (builder.Configuration.GetValue<bool>("Auth:AllowDevKey"))
            policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, DevApiKeyAuthHandler.SchemeName);
        else
            policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Ensure database is created and seeded
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<PlateLibContext>();
        await context.Database.EnsureCreatedAsync();
    }

    // Ensure storage bucket exists
    var storage = app.Services.GetRequiredService<IStorageService>();
    await storage.EnsureBucketExistsAsync();
}

app.MapOpenApi();
{
    app.MapScalarApiReference("/api", options =>
    {
        options.WithTitle("PlateLib API");
    });
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Register feature endpoints
app.MapGetPlates();
app.MapGetPlateById();
app.MapGetManufacturers();
app.MapCreateManufacturer();
app.MapGetManufacturerById();
app.MapGetFilesForPlate();
app.MapDownloadFile();
app.MapUploadFile();
app.MapDeleteFile();
app.MapCreatePlate();
app.MapGetMaterials();
app.MapGetPropertyDefinitions();

app.Run();

