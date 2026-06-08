using System.Diagnostics;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Data.Seeding;
using Microsoft.EntityFrameworkCore;
using LibFile = Data.Entities.File;

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
                await SeedFilesAsync(dbContext, s3, storageConfig.BucketName, cancellationToken);
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

    private static async Task SeedFilesAsync(
        PlateLibContext dbContext,
        IAmazonS3 s3,
        string bucketName,
        CancellationToken cancellationToken)
    {
        // Ensure the MinIO bucket exists
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3, bucketName))
        {
            await s3.PutBucketAsync(new PutBucketRequest { BucketName = bucketName, UseClientRegion = true }, cancellationToken);
        }

        var plate = await dbContext.Plates
            .Include(p => p.Files)
            .FirstOrDefaultAsync(p => p.CatalogNumber == "655101", cancellationToken);

        if (plate is null || plate.Files.Any())
            return;

        // Seed a minimal placeholder PDF so the download flow can be tested locally
        var fileId = Guid.NewGuid();
        var storageKey = $"plates/{plate.Id}/{fileId}/Greiner-655101-Datasheet.pdf";

        var pdfContent = GeneratePlaceholderPdf("Greiner 655101 – 96-well PS F-Bottom Clear");

        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucketName,
            Key = storageKey,
            InputStream = new MemoryStream(pdfContent),
            ContentType = "application/pdf",
        }, cancellationToken);

        var seededFile = new LibFile
        {
            Id = fileId,
            FileName = "Greiner-655101-Datasheet.pdf",
            ContentType = "application/pdf",
            StorageKey = storageKey,
            FileSizeBytes = pdfContent.Length,
        };

        seededFile.Plates.Add(plate);
        dbContext.Files.Add(seededFile);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Generates a minimal valid PDF with placeholder text — avoids a binary asset dependency.
    /// </summary>
    private static byte[] GeneratePlaceholderPdf(string title)
    {
        var body = $"""
            %PDF-1.4
            1 0 obj<</Type /Catalog /Pages 2 0 R>>endobj
            2 0 obj<</Type /Pages /Kids[3 0 R] /Count 1>>endobj
            3 0 obj<</Type /Page /Parent 2 0 R /MediaBox[0 0 612 792] /Contents 4 0 R /Resources<</Font<</F1 5 0 R>>>>>>endobj
            4 0 obj<</Length 44>>
            stream
            BT /F1 18 Tf 50 720 Td ({title}) Tj ET
            endstream
            endobj
            5 0 obj<</Type /Font /Subtype /Type1 /BaseFont /Helvetica>>endobj
            xref
            0 6
            0000000000 65535 f 
            trailer<</Size 6 /Root 1 0 R>>
            startxref
            0
            %%EOF
            """;

        return System.Text.Encoding.ASCII.GetBytes(body);
    }
}
