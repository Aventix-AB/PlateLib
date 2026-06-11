using System.Diagnostics;
using Amazon.S3;
using Data.Seeding;
using Microsoft.EntityFrameworkCore;

namespace MigrationService;

public record StorageSeedConfig(string BucketName);

public class Worker(
    IServiceProvider serviceProvider,
    IHostEnvironment hostEnvironment,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity(
            "Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PlateLibContext>();

            await RunMigrationAsync(dbContext, cancellationToken);

            if (hostEnvironment.IsDevelopment())
            {
                var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
                var storageConfig = scope.ServiceProvider.GetRequiredService<StorageSeedConfig>();

                await SeedDataAsync(dbContext, cancellationToken);
                await SeedFiles.SeedAsync(dbContext, s3, storageConfig.BucketName, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(
        PlateLibContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(
        PlateLibContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            await SeedPlate.SeedAsync(dbContext, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
