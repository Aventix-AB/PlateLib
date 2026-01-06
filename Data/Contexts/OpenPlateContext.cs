using Microsoft.EntityFrameworkCore;
using Data.Entities;

public class OpenPlateContext : DbContext
{
    public DbSet<Manufacturer> Manufacturers { get; set; }
    public DbSet<Plate> Plates { get; set; }
    public DbSet<PlateFile> PlateFiles { get; set; }
}