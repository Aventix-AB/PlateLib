using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<PlateColorEnum>))]
public enum PlateColorEnum
{
    Clear,
    White,
    Black,
}