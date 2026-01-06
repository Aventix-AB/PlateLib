using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Entities;

namespace Data.Configurations;

public class PlateFileConfiguration : IEntityTypeConfiguration<PlateFile>
{
    public void Configure(EntityTypeBuilder<PlateFile> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        // Store file content as bytea in PostgreSQL (TOAST will automatically compress/chunk large files)
        entity.Property(e => e.FileContent)
            .IsRequired()
            .HasColumnType("bytea");

        // Indexes for common queries
        entity.HasIndex(e => e.PlateId);

        // Relationship with Plate
        entity.HasOne(e => e.Plate)
            .WithMany(e => e.PlateFiles)
            .HasForeignKey(e => e.PlateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
