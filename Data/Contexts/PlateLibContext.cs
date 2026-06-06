using Microsoft.EntityFrameworkCore;
using Data.Entities;
using LibFile = Data.Entities.File;

public class PlateLibContext : DbContext
{
    public PlateLibContext(DbContextOptions<PlateLibContext> options) : base(options)
    {
    }

    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<Plate> Plates { get; set; }
    public DbSet<LibFile> Files { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<PropertyDefinition> PropertyDefinitions { get; set; }
    public DbSet<PlateProperty> PlateProperties { get; set; }
}