using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Entities;

namespace Data.Configurations;

public class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.WebsiteUrl)
            .HasMaxLength(500);

        entity.Property(e => e.ThumbnailStorageKey)
            .HasMaxLength(1024);

        // One-to-many relationship with Plates
        entity.HasMany(e => e.Plates)
            .WithOne(e => e.Manufacturer)
            .HasForeignKey(e => e.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-many: a manufacturer can have multiple files; a file can be attached to multiple manufacturers.
        entity.HasMany(m => m.Files)
            .WithMany(f => f.Manufacturers);
    }
}