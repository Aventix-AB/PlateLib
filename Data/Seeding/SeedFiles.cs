using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.EntityFrameworkCore;
using LibFile = Data.Entities.File;

namespace Data.Seeding;

public static class SeedFiles
{
    private static readonly byte[] PlaceholderPdfContent =
        System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\n%%EOF\n");

    public static async Task SeedAsync(
        PlateLibContext context,
        IAmazonS3 s3,
        string bucketName,
        CancellationToken cancellationToken = default)
    {
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3, bucketName))
        {
            await s3.PutBucketAsync(
                new PutBucketRequest { BucketName = bucketName, UseClientRegion = true },
                cancellationToken);
        }

        await SeedPlateFilesAsync(context, s3, bucketName, cancellationToken);
        await SeedManufacturerFilesAsync(context, s3, bucketName, cancellationToken);
    }

    private static async Task SeedPlateFilesAsync(
        PlateLibContext context,
        IAmazonS3 s3,
        string bucketName,
        CancellationToken ct)
    {
        var plates = await context.Plates
            .Include(p => p.Files)
            .Where(p => !p.Files.Any())
            .ToListAsync(ct);

        foreach (var plate in plates)
        {
            var fileId = Guid.NewGuid();
            var fileName = $"{plate.CatalogNumber}-Datasheet.pdf";
            var storageKey = $"plates/{plate.Id}/{fileId}/{fileName}";

            await UploadPlaceholderAsync(s3, bucketName, storageKey, ct);

            var seededFile = new LibFile
            {
                Id = fileId,
                FileName = fileName,
                ContentType = "application/pdf",
                StorageKey = storageKey,
                FileSizeBytes = PlaceholderPdfContent.Length,
            };

            seededFile.Plates.Add(plate);
            context.Files.Add(seededFile);
        }

        if (plates.Count > 0)
            await context.SaveChangesAsync(ct);
    }

    private static async Task SeedManufacturerFilesAsync(
        PlateLibContext context,
        IAmazonS3 s3,
        string bucketName,
        CancellationToken ct)
    {
        var manufacturers = await context.Manufacturers
            .Include(m => m.Files)
            .Where(m => !m.Files.Any())
            .ToListAsync(ct);

        foreach (var manufacturer in manufacturers)
        {
            var fileId = Guid.NewGuid();
            var fileName = $"{manufacturer.Name.Replace(" ", "-")}-Overview.pdf";
            var storageKey = $"manufacturers/{manufacturer.Id}/{fileId}/{fileName}";

            await UploadPlaceholderAsync(s3, bucketName, storageKey, ct);

            var seededFile = new LibFile
            {
                Id = fileId,
                FileName = fileName,
                ContentType = "application/pdf",
                StorageKey = storageKey,
                FileSizeBytes = PlaceholderPdfContent.Length,
            };

            seededFile.Manufacturers.Add(manufacturer);
            context.Files.Add(seededFile);
        }

        if (manufacturers.Count > 0)
            await context.SaveChangesAsync(ct);
    }

    private static Task UploadPlaceholderAsync(
        IAmazonS3 s3,
        string bucketName,
        string storageKey,
        CancellationToken ct)
    {
        return s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucketName,
            Key = storageKey,
            InputStream = new MemoryStream(PlaceholderPdfContent),
            ContentType = "application/pdf",
        }, ct);
    }
}
