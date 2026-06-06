using Microsoft.EntityFrameworkCore;
using Data.Configurations;

namespace Data.Entities;

[EntityTypeConfiguration(typeof(PlatePropertyConfiguration))]
public class PlateProperty
{
    public Guid PlateId { get; set; }
    public Guid PropertyDefinitionId { get; set; }

    /// <summary>
    /// Stored as a string; interpret using PropertyDefinition.DataType
    /// </summary>
    public string Value { get; set; } = string.Empty;

    // Navigation properties
    public Plate Plate { get; set; } = null!;
    public PropertyDefinition PropertyDefinition { get; set; } = null!;
}
