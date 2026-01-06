using Microsoft.EntityFrameworkCore;
using Common.Enums;
using Common.Models;
using Data.Configurations;

namespace Data.Entities;

[EntityTypeConfiguration(typeof(PlateConfiguration))]
public class Plate
{
    public required Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public required string CatalogNumber { get; set; }
    public int Wellnumber { get; set; }
    public PlateMaterialEnum Material { get; set; }
    public bool Lid { get; set; }
    public PlateColorEnum Color { get; set; }
    public PlateSkirtEnum Skirt { get; set; }
    public bool Sterile { get; set; }
    public PlateVolume Volume { get; set; } = new PlateVolume();

    // Foreign key
    public Guid ManufacturerId { get; set; }

    // Navigation properties
    public Manufacturer Manufacturer { get; set; } = null!;
    public ICollection<PlateFile> PlateFiles { get; set; } = new List<PlateFile>();
}