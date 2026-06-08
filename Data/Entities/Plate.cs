using Microsoft.EntityFrameworkCore;
using Data.Configurations;

namespace Data.Entities;

[EntityTypeConfiguration(typeof(PlateConfiguration))]
public class Plate
{
    public required Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public required string CatalogNumber { get; set; }
    public int WellCount { get; set; }

    /// <summary>Object key in blob storage for the plate's thumbnail image. Null when no thumbnail has been uploaded.</summary>
    public string? ThumbnailStorageKey { get; set; }

    // Foreign keys
    public Guid ManufacturerId { get; set; }
    public Guid MaterialId { get; set; }

    // Navigation properties
    public Manufacturer Manufacturer { get; set; } = null!;
    public Material Material { get; set; } = null!;
    public ICollection<File> Files { get; set; } = new List<File>();
    public ICollection<PlateProperty> PlateProperties { get; set; } = new List<PlateProperty>();
}