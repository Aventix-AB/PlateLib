using Data.Entities;

namespace PlateLib.UnitTests.Features.Plates;

public class GetPlatesTests
{
    [Fact]
    public void Plate_CoreProperties_MappedCorrectly()
    {
        var manufacturer = new Manufacturer { Id = Guid.NewGuid(), Name = "Corning" };
        var material = new Material { Id = Guid.NewGuid(), Code = "PS", Name = "Polystyrene" };
        var plate = new Plate
        {
            Id = Guid.NewGuid(),
            Name = "Corning 96-Well",
            CatalogNumber = "3596",
            WellCount = 96,
            MaterialId = material.Id,
            Material = material,
            ManufacturerId = manufacturer.Id,
            Manufacturer = manufacturer,
        };

        Assert.Equal(96, plate.WellCount);
        Assert.Equal("3596", plate.CatalogNumber);
        Assert.Equal("PS", plate.Material.Code);
        Assert.Equal("Polystyrene", plate.Material.Name);
    }

    [Fact]
    public void PlateProperty_StoresValueAsString()
    {
        var plate = new Plate
        {
            Id = Guid.NewGuid(),
            CatalogNumber = "3596",
            WellCount = 96,
            ManufacturerId = Guid.NewGuid(),
            MaterialId = Guid.NewGuid(),
        };

        var sterilityDef = new PropertyDefinition { Id = Guid.NewGuid(), Name = "Sterility", DataType = Data.Enums.PropertyDataType.Boolean };
        var property = new PlateProperty
        {
            PlateId = plate.Id,
            PropertyDefinitionId = sterilityDef.Id,
            PropertyDefinition = sterilityDef,
            Value = "true",
        };

        Assert.Equal("Sterility", property.PropertyDefinition.Name);
        Assert.Equal("true", property.Value);
    }
}
