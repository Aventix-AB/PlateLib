using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace API.Storage;

public class StorageOptions
{
    public const string Section = "Storage";

    public string ServiceUrl { get; set; } = string.Empty;
    public string BucketName { get; set; } = "platelib";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Must be true for MinIO and other non-AWS S3-compatible providers.</summary>
    public bool ForcePathStyle { get; set; } = false;
}

public class S3StorageService(IAmazonS3 s3, IOptions<StorageOptions> options) : IStorageService
{
    private readonly string _bucket = options.Value.BucketName;

    public async Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            DisablePayloadSigning = true,
        };

        await s3.PutObjectAsync(request, ct);
        return key;
    }

    public Task<string> GeneratePresignedUrlAsync(string key, TimeSpan expiry, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry),
            Verb = HttpVerb.GET,
        };

        var url = s3.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        await s3.DeleteObjectAsync(_bucket, key, ct);
    }

    public async Task EnsureBucketExistsAsync(CancellationToken ct = default)
    {
        var exists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3, _bucket);
        if (!exists)
        {
            await s3.PutBucketAsync(new PutBucketRequest { BucketName = _bucket, UseClientRegion = true }, ct);
        }
    }
}
