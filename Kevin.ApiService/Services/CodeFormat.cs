using System.Text.Json.Serialization;

namespace Kevin.ApiService.Services;

[JsonConverter(typeof(JsonStringEnumConverter<CodeFormat>))]
public enum CodeFormat
{
    Numeric,
    Alphanum
}
