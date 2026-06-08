using Data.Entities;
using FluentAssertions;
using LibFile = Data.Entities.File;

namespace PlateLib.UnitTests.Features.Files;

public class FileEntityTests
{
    [Fact]
    public void File_StorageKey_DefaultsToEmpty()
    {
        var file = new LibFile();

        file.StorageKey.Should().Be(string.Empty);
        file.FileSizeBytes.Should().Be(0);
    }

    [Fact]
    public void File_StorageKey_FollowsKeyConvention()
    {
        var plateId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var fileName = "drawing.pdf";

        var expectedKey = $"plates/{plateId}/{fileId}/{fileName}";

        var file = new LibFile
        {
            Id = fileId,
            FileName = fileName,
            ContentType = "application/pdf",
            StorageKey = expectedKey,
            FileSizeBytes = 1024,
        };

        file.StorageKey.Should().StartWith("plates/");
        file.StorageKey.Should().Contain(plateId.ToString());
        file.StorageKey.Should().EndWith(fileName);
        file.FileSizeBytes.Should().Be(1024);
    }
}
