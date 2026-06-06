using Microsoft.EntityFrameworkCore;
using Data.Entities;

public class OpenPlateContext : DbContext
{
    public OpenPlateContext(DbContextOptions<OpenPlateContext> options) : base(options)
    {
    }

    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<Plate> Plates { get; set; }
    public DbSet<PlateFile> PlateFiles { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<PropertyDefinition> PropertyDefinitions { get; set; }
    public DbSet<PlateProperty> PlateProperties { get; set; }
}