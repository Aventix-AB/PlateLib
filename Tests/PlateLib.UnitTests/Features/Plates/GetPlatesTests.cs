using Data.Entities;
using Data.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace PlateLib.UnitTests.Features.Plates;

public class GetPlatesTests
{
    [Fact]
    public void PlateResponse_MapsPropertiesCorrectly()
    {
        var manufacturer = new Manufacturer { Id = Guid.NewGuid(), Name = "Corning" };
        var plate = new Plate
        {
            Id = Guid.NewGuid(),
            Name = "Corning 96-Well",
            CatalogNumber = "3596",
            Wellnumber = 96,
            Material = PlateMaterialEnum.PS,
            Lid = true,
            Color = PlateColorEnum.Clear,
            Skirt = PlateSkirtEnum.HalfSkirt,
            Sterile = true,
            ManufacturerId = manufacturer.Id,
            Manufacturer = manufacturer
        };

        Assert.Equal(96, plate.Wellnumber);
        Assert.Equal("3596", plate.CatalogNumber);
        Assert.Equal(PlateMaterialEnum.PS, plate.Material);
        Assert.True(plate.Sterile);
    }
}
