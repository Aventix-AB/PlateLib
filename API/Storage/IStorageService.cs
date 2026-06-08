namespace API.Storage;

public interface IStorageService
{
    /// <summary>
    /// Uploads a file stream to blob storage and returns the storage key.
    /// </summary>
    Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default);

    /// <summary>
    /// Generates a pre-signed URL for downloading a file directly from storage.
    /// </summary>
    Task<string> GeneratePresignedUrlAsync(string key, TimeSpan expiry, CancellationToken ct = default);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    Task DeleteAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Ensures the configured bucket exists, creating it if necessary.
    /// </summary>
    Task EnsureBucketExistsAsync(CancellationToken ct = default);
}
