using API.Features.Files;

namespace API.Features.Plates;

public record MaterialResponse(string Code, string Name);
public record PlatePropertyResponse(string Name, string Value);

public record PlateResponse(
    Guid Id,
    string Name,
    string CatalogNumber,
    string ProductUrl,
    int WellCount,
    MaterialResponse Material,
    Guid ManufacturerId,
    string ManufacturerName,
    List<PlatePropertyResponse> Properties);

public record PlateDetailResponse(
    Guid Id,
    string Name,
    string CatalogNumber,
    string ProductUrl,
    int WellCount,
    MaterialResponse Material,
    Guid ManufacturerId,
    string ManufacturerName,
    List<PlatePropertyResponse> Properties,
    List<FileResponse> Files);
