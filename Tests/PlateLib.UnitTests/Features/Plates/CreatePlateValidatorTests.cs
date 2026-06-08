using API.Features.Plates;
using FluentAssertions;

namespace PlateLib.UnitTests.Features.Plates;

public class CreatePlateValidatorTests
{
    private readonly CreatePlate.CreatePlateValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidRequest_Passes()
    {
        var request = new CreatePlate.CreatePlateRequest(
            Name: "MICROPLATE 96 WELL",
            CatalogNumber: "3596",
            WellCount: 96,
            ManufacturerId: Guid.NewGuid(),
            MaterialId: Guid.NewGuid(),
            Properties: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "3596", 96)]
    [InlineData("Valid Name", "", 96)]
    [InlineData("Valid Name", "3596", 0)]
    [InlineData("Valid Name", "3596", -1)]
    public async Task Validate_WithInvalidFields_Fails(string name, string catalogNumber, int wellCount)
    {
        var request = new CreatePlate.CreatePlateRequest(
            Name: name,
            CatalogNumber: catalogNumber,
            WellCount: wellCount,
            ManufacturerId: Guid.NewGuid(),
            MaterialId: Guid.NewGuid(),
            Properties: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WithEmptyManufacturerId_Fails()
    {
        var request = new CreatePlate.CreatePlateRequest(
            Name: "Valid Name",
            CatalogNumber: "3596",
            WellCount: 96,
            ManufacturerId: Guid.Empty,
            MaterialId: Guid.NewGuid(),
            Properties: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }
}
