namespace Common.DTOs;

public class PlateDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Manufacturer information
    public Guid ManufacturerId { get; set; }
    public string ManufacturerName { get; set; } = string.Empty;

    // Plate files
    public List<PlateFileDTO> PlateFiles { get; set; } = new();
}

public class PlateFileDTO
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}