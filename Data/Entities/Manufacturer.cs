
using Microsoft.EntityFrameworkCore;
using Data.Configurations;

namespace Data.Entities;

[EntityTypeConfiguration(typeof(ManufacturerConfiguration))]
public class Manufacturer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional link to the manufacturer's website.</summary>
    public string? WebsiteUrl { get; set; }

    /// <summary>Object key in blob storage for the manufacturer's logo/thumbnail, e.g. "manufacturers/{id}/thumbnail/logo.png". Null when no thumbnail has been uploaded.</summary>
    public string? ThumbnailStorageKey { get; set; }

    // Navigation properties
    public ICollection<Plate> Plates { get; set; } = new List<Plate>();
    public ICollection<File> Files { get; set; } = new List<File>();
}