using Microsoft.EntityFrameworkCore;
using Data.Configurations;

namespace Data.Entities;

[EntityTypeConfiguration(typeof(PlateFileConfiguration))]
public class PlateFile
{
    public Guid Id { get; set; }

    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type (e.g., application/pdf)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Binary file content stored in PostgreSQL bytea column
    /// </summary>
    public byte[] FileContent { get; set; } = [];

    // Foreign key
    public Guid PlateId { get; set; }

    // Navigation property
    public Plate Plate { get; set; } = null!;
}
