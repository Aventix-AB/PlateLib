using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<PlateSkirtEnum>))]
public enum PlateSkirtEnum
{
    NoSkirt,
    HalfSkirt,
    FullSkirt,
}