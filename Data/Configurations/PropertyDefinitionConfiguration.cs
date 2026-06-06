using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Entities;

namespace Data.Configurations;

public class PropertyDefinitionConfiguration : IEntityTypeConfiguration<PropertyDefinition>
{
    public void Configure(EntityTypeBuilder<PropertyDefinition> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        entity.HasIndex(e => e.Name).IsUnique();

        entity.Property(e => e.Description)
            .HasMaxLength(500);
    }
}
