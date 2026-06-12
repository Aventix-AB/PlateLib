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

        entity.Property(e => e.ProductUrl)
            .IsRequired()
            .HasMaxLength(1024);

        entity.Property(e => e.ThumbnailStorageKey)
            .HasMaxLength(1024);

        entity.HasIndex(e => e.ManufacturerId);
        entity.HasIndex(e => e.MaterialId);

        // Full-text search vector: generated from Name and CatalogNumber, indexed with GIN
        entity.HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "english",
                p => new { p.Name, p.CatalogNumber })
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN");

        // Relationships configured in ManufacturerConfiguration and MaterialConfiguration
    }
}