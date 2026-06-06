using Microsoft.EntityFrameworkCore;
using Data.Configurations;

namespace Data.Entities;

[EntityTypeConfiguration(typeof(MaterialConfiguration))]
public class Material
{
    public Guid Id { get; set; }

    /// <summary>
    /// Short industry code, e.g. "PS", "PP", "COC"
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full material name, e.g. "Polystyrene", "Polypropylene"
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Plate> Plates { get; set; } = new List<Plate>();
}
