using Microsoft.EntityFrameworkCore;
using Data.Configurations;

namespace Data.Entities;

/// <summary>
/// A file stored in the database. Can be linked to multiple entities
/// (plates, manufacturers, etc.) via typed join tables.
/// </summary>
[EntityTypeConfiguration(typeof(FileConfiguration))]
public class File
{
    public Guid Id { get; set; }

    /// <summary>Original file name as uploaded.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>MIME type (e.g., application/pdf).</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Object key in blob storage (e.g. "plates/{plateId}/{fileId}/drawing.pdf").</summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; set; }

    // Navigation properties
    public ICollection<Plate> Plates { get; set; } = new List<Plate>();
}
