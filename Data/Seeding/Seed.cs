using Data.Entities;
using Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Data.Seeding;

public static class SeedPlate
{
    private static readonly List<(string Code, string Name, string Description)> MaterialSeedData =
    [
        ("PS", "Polystyrene", "The most common microplate material; optically clear, good for absorbance and fluorescence assays."),
        ("PP", "Polypropylene", "Chemical-resistant and low-binding; ideal for sample storage and compound libraries."),
    ];

    private static readonly List<(string Name, PropertyDataType DataType, string Description)> PropertyDefinitionSeedData =
    [
        ("Color",   PropertyDataType.Select,  "Well color, e.g. Clear, White, Black."),
        ("Skirt",   PropertyDataType.Select,  "Skirt type, e.g. NoSkirt, HalfSkirt, FullSkirt."),
        ("Sterile", PropertyDataType.Boolean, "Whether the plate is sterile."),
        ("Lid",     PropertyDataType.Boolean, "Whether the plate ships with a lid."),
    ];

    public static async Task SeedAsync(PlateLibContext context, CancellationToken cancellationToken = default)
    {
        await SeedMaterialsAsync(context, cancellationToken);
        await SeedPropertyDefinitionsAsync(context, cancellationToken);
        await SeedManufacturersAndPlatesAsync(context, cancellationToken);
    }

    private static async Task SeedMaterialsAsync(PlateLibContext context, CancellationToken ct)
    {
        foreach (var (code, name, description) in MaterialSeedData)
        {
            if (!await context.Materials.AnyAsync(m => m.Code == code, ct))
            {
                context.Materials.Add(new Material
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Name = name,
                    Description = description,
                });
            }
        }
        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedPropertyDefinitionsAsync(PlateLibContext context, CancellationToken ct)
    {
        foreach (var (name, dataType, description) in PropertyDefinitionSeedData)
        {
            if (!await context.PropertyDefinitions.AnyAsync(p => p.Name == name, ct))
            {
                context.PropertyDefinitions.Add(new PropertyDefinition
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    DataType = dataType,
                    Description = description,
                });
            }
        }
        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedManufacturersAndPlatesAsync(PlateLibContext context, CancellationToken ct)
    {
        var greiner = await GetOrCreateManufacturerAsync(context, "Greiner", ct);
        var corning = await GetOrCreateManufacturerAsync(context, "Corning", ct);

        var psMaterial = await context.Materials.FirstAsync(m => m.Code == "PS", ct);

        var colorDef = await context.PropertyDefinitions.FirstAsync(p => p.Name == "Color", ct);
        var skirtDef = await context.PropertyDefinitions.FirstAsync(p => p.Name == "Skirt", ct);
        var sterileDef = await context.PropertyDefinitions.FirstAsync(p => p.Name == "Sterile", ct);
        var lidDef = await context.PropertyDefinitions.FirstAsync(p => p.Name == "Lid", ct);

        await SeedPlateIfNotExistsAsync(
            context,
            manufacturerId: greiner.Id,
            materialId: psMaterial.Id,
            catalogNumber: "655101",
            name: "MICROPLATE, 96 WELL, PS, F-BOTTOM, CLEAR",
            wellCount: 96,
            properties:
            [
                (colorDef.Id,   "Clear"),
                (skirtDef.Id,   "FullSkirt"),
                (sterileDef.Id, "false"),
                (lidDef.Id,     "false"),
            ],
            ct);

        await SeedPlateIfNotExistsAsync(
            context,
            manufacturerId: greiner.Id,
            materialId: psMaterial.Id,
            catalogNumber: "781101",
            name: "MICROPLATE, 384 WELL, PS, F-BOTTOM",
            wellCount: 384,
            properties:
            [
                (colorDef.Id,   "Clear"),
                (skirtDef.Id,   "FullSkirt"),
                (sterileDef.Id, "false"),
                (lidDef.Id,     "false"),
            ],
            ct);

        await SeedPlateIfNotExistsAsync(
            context,
            manufacturerId: corning.Id,
            materialId: psMaterial.Id,
            catalogNumber: "3695",
            name: "Corning® 96-well Half Area Clear Flat Bottom Polystyrene Not Treated Microplate, 25 per Bag, without Lid, Nonsterile",
            wellCount: 96,
            properties:
            [
                (colorDef.Id,   "Clear"),
                (skirtDef.Id,   "FullSkirt"),
                (sterileDef.Id, "false"),
                (lidDef.Id,     "false"),
            ],
            ct);
    }

    private static async Task<Manufacturer> GetOrCreateManufacturerAsync(
        PlateLibContext context, string name, CancellationToken ct)
    {
        var manufacturer = await context.Manufacturers.FirstOrDefaultAsync(m => m.Name == name, ct);
        if (manufacturer is null)
        {
            manufacturer = new Manufacturer { Id = Guid.NewGuid(), Name = name };
            context.Manufacturers.Add(manufacturer);
            await context.SaveChangesAsync(ct);
        }
        return manufacturer;
    }

    private static async Task SeedPlateIfNotExistsAsync(
        PlateLibContext context,
        Guid manufacturerId,
        Guid materialId,
        string catalogNumber,
        string name,
        int wellCount,
        (Guid PropertyDefinitionId, string Value)[] properties,
        CancellationToken ct)
    {
        if (await context.Plates.AnyAsync(p => p.CatalogNumber == catalogNumber, ct))
            return;

        var plate = new Plate
        {
            Id = Guid.NewGuid(),
            Name = name,
            CatalogNumber = catalogNumber,
            WellCount = wellCount,
            MaterialId = materialId,
            ManufacturerId = manufacturerId,
        };
        context.Plates.Add(plate);

        foreach (var (propertyDefinitionId, value) in properties)
        {
            context.PlateProperties.Add(new PlateProperty
            {
                PlateId = plate.Id,
                PropertyDefinitionId = propertyDefinitionId,
                Value = value,
            });
        }

        await context.SaveChangesAsync(ct);
    }
}

