using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Scalar.AspNetCore;
using Data.Seeding;

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

builder.AddNpgsqlDbContext<OpenPlateContext>(
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Ensure database is created and seeded
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<OpenPlateContext>();
        await context.Database.EnsureCreatedAsync();
    }
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

app.Run();
