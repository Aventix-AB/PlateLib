using System.Text.Json.Serialization;

namespace Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<PlateMaterialEnum>))]
public enum PlateMaterialEnum
{
    PS,
    PP,
}