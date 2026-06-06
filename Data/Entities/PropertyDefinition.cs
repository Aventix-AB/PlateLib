using Microsoft.EntityFrameworkCore;
using Data.Configurations;
using Data.Enums;

namespace Data.Entities;

[EntityTypeConfiguration(typeof(PropertyDefinitionConfiguration))]
public class PropertyDefinition
{
    public Guid Id { get; set; }

    /// <summary>
    /// Human-readable property name, e.g. "Sterility", "Lid", "Color"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Describes how the stored Value string should be interpreted
    /// </summary>
    public PropertyDataType DataType { get; set; }

    public string? Description { get; set; }

    // Navigation properties
    public ICollection<PlateProperty> PlateProperties { get; set; } = new List<PlateProperty>();
}
