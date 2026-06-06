using System.Text.Json.Serialization;

namespace Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<PlateSkirtEnum>))]
public enum PlateSkirtEnum
{
    NoSkirt,
    HalfSkirt,
    FullSkirt,
}
