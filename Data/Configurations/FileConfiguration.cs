using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibFile = Data.Entities.File;

namespace Data.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<LibFile>
{
    public void Configure(EntityTypeBuilder<LibFile> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        // TOAST will automatically compress/chunk large binary content
        entity.Property(e => e.FileContent)
            .IsRequired()
            .HasColumnType("bytea");

        // Many-to-many: a file can be attached to multiple plates;
        // a plate can reference multiple files.
        entity.HasMany(f => f.Plates)
            .WithMany(p => p.Files)
            .UsingEntity(j => j.ToTable("PlateFiles"));
    }
}
