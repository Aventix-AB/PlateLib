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
        ("Color",     PropertyDataType.Select,  "Well color, e.g. Clear, White, Black."),
        ("Skirt",     PropertyDataType.Select,  "Skirt type, e.g. NoSkirt, HalfSkirt, FullSkirt."),
        ("Sterile", PropertyDataType.Boolean, "Whether the plate is sterile."),
        ("Lid",       PropertyDataType.Boolean, "Whether the plate ships with a lid."),
    ];

    public static async Task SeedAsync(PlateLibContext context, CancellationToken cancellationToken = default)
    {
        await SeedMaterialsAsync(context, cancellationToken);
        await SeedPropertyDefinitionsAsync(context, cancellationToken);
        await SeedManufacturerAndPlateAsync(context, cancellationToken);
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

    private static async Task SeedManufacturerAndPlateAsync(PlateLibContext context, CancellationToken ct)
    {
        var manufacturer = await context.Manufacturers.FirstOrDefaultAsync(m => m.Name == "Greiner", ct);
        if (manufacturer is null)
        {
            manufacturer = new Manufacturer { Id = Guid.NewGuid(), Name = "Greiner" };
            context.Manufacturers.Add(manufacturer);
            await context.SaveChangesAsync(ct);
        }

        if (await context.Plates.AnyAsync(p => p.CatalogNumber == "655101", ct))
            return;

        var psMaterial = await context.Materials.FirstAsync(m => m.Code == "PS", ct);

        var colorDef = await context.PropertyDefinitions.FirstAsync(p => p.Name == "Color", ct);
        var skirtDef = await context.PropertyDefinitions.FirstAsync(p => p.Name == "Skirt", ct);
        var sterileDef = await context.PropertyDefinitions.FirstAsync(p => p.Name == "Sterile", ct);
        var lidDef = await context.PropertyDefinitions.FirstAsync(p => p.Name == "Lid", ct);

        var plate = new Plate
        {
            Id = Guid.NewGuid(),
            Name = "MICROPLATE, 96 WELL, PS, F-BOTTOM, CLEAR",
            CatalogNumber = "651101",
            WellCount = 96,
            MaterialId = psMaterial.Id,
            ManufacturerId = manufacturer.Id,
        };
        context.Plates.Add(plate);

        context.PlateProperties.AddRange(
            new PlateProperty { PlateId = plate.Id, PropertyDefinitionId = colorDef.Id, Value = "Clear" },
            new PlateProperty { PlateId = plate.Id, PropertyDefinitionId = skirtDef.Id, Value = "FullSkirt" },
            new PlateProperty { PlateId = plate.Id, PropertyDefinitionId = sterileDef.Id, Value = "false" },
            new PlateProperty { PlateId = plate.Id, PropertyDefinitionId = lidDef.Id, Value = "false" }
        );

        await context.SaveChangesAsync(ct);
    }
}

