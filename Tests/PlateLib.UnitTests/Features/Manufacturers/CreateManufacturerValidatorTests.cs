using API.Features.Manufacturers;
using FluentAssertions;

namespace PlateLib.UnitTests.Features.Manufacturers;

public class CreateManufacturerValidatorTests
{
    private readonly CreateManufacturer.CreateManufacturerValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidName_Passes()
    {
        var request = new CreateManufacturer.CreateManufacturerRequest("Corning");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyName_Fails(string? name)
    {
        var request = new CreateManufacturer.CreateManufacturerRequest(name!);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_Fails()
    {
        var request = new CreateManufacturer.CreateManufacturerRequest(new string('A', 201));

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }
}
