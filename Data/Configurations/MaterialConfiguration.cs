using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Entities;

namespace Data.Configurations;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        entity.HasIndex(e => e.Code).IsUnique();

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Description)
            .HasMaxLength(500);

        entity.HasMany(e => e.Plates)
            .WithOne(e => e.Material)
            .HasForeignKey(e => e.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
