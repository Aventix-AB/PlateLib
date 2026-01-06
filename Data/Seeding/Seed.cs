using Data.Entities;
using Common.Enums;
using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Seeding;

public static class SeedPlate
{
    private static Manufacturer CreateManufacturer()
    {
        return new Manufacturer
        {
            Id = Guid.NewGuid(),
            Name = "Greiner"
        };
    }

    private static Plate CreatePlate(Guid manufacturerId)
    {
        return new Plate
        {
            Id = Guid.NewGuid(),
            Name = "MICROPLATE, 96 WELL, PS, F-BOTTOM, CLEAR",
            CatalogNumber = "655101",
            Wellnumber = 96,
            Material = PlateMaterialEnum.PS,
            Lid = false,
            Color = PlateColorEnum.Clear,
            Skirt = PlateSkirtEnum.FullSkirt,
            Sterile = false,
            Volume = new PlateVolume
            {
                Volume = 382,
                MinWorkingVolume = 25,
                MaxWorkingVolume = 340
            },
            ManufacturerId = manufacturerId
        };
    }

    public static void Seed(OpenPlateContext context)
    {
        // First, seed a manufacturer if it doesn't exist
        var manufacturer = context.Set<Manufacturer>()
            .FirstOrDefault(m => m.Name == "Greiner");

        if (manufacturer is null)
        {
            manufacturer = CreateManufacturer();
            context.Set<Manufacturer>().Add(manufacturer);
            context.SaveChanges();
        }

        // Then, seed a test plate if it doesn't exist
        var testPlate = context.Set<Plate>()
            .FirstOrDefault(p => p.CatalogNumber == "655101");

        if (testPlate is null)
        {
            context.Set<Plate>().Add(CreatePlate(manufacturer.Id));
            context.SaveChanges();
        }
    }

    public static async Task SeedAsync(OpenPlateContext context, CancellationToken cancellationToken = default)
    {
        // First, seed a manufacturer if it doesn't exist
        var manufacturer = await context.Set<Manufacturer>()
            .FirstOrDefaultAsync(m => m.Name == "Greiner", cancellationToken);

        if (manufacturer is null)
        {
            manufacturer = CreateManufacturer();
            context.Set<Manufacturer>().Add(manufacturer);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Then, seed a test plate if it doesn't exist
        var testPlate = await context.Set<Plate>()
            .FirstOrDefaultAsync(p => p.CatalogNumber == "655101", cancellationToken);

        if (testPlate is null)
        {
            context.Set<Plate>().Add(CreatePlate(manufacturer.Id));
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
