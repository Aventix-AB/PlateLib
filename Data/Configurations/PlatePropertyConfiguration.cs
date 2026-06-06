using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Entities;

namespace Data.Configurations;

public class PlatePropertyConfiguration : IEntityTypeConfiguration<PlateProperty>
{
    public void Configure(EntityTypeBuilder<PlateProperty> entity)
    {
        entity.HasKey(e => new { e.PlateId, e.PropertyDefinitionId });

        entity.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(500);

        entity.HasOne(e => e.Plate)
            .WithMany(e => e.PlateProperties)
            .HasForeignKey(e => e.PlateId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.PropertyDefinition)
            .WithMany(e => e.PlateProperties)
            .HasForeignKey(e => e.PropertyDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
