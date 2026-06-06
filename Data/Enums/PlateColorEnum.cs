using System.Text.Json.Serialization;

namespace Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<PlateColorEnum>))]
public enum PlateColorEnum
{
    Clear,
    White,
    Black,
}
