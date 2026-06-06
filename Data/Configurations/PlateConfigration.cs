using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Entities;

namespace Data.Configurations;

public class PlateConfiguration : IEntityTypeConfiguration<Plate>
{
    public void Configure(EntityTypeBuilder<Plate> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.CatalogNumber)
            .IsRequired()
            .HasMaxLength(100);

        entity.HasIndex(e => e.ManufacturerId);
        entity.HasIndex(e => e.MaterialId);

        // Relationships configured in ManufacturerConfiguration and MaterialConfiguration
    }
}