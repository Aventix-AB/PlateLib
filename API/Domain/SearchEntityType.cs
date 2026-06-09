using System.Text.Json.Serialization;

namespace API.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SearchEntityType
{
    Plate,
    Manufacturer,
}
