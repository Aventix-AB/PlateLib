using System.Text.Json.Serialization;

namespace Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<PlateMaterialEnum>))]
public enum PlateMaterialEnum
{
    PS,
    PP,
}
