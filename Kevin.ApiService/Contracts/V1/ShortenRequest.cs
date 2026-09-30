using System.ComponentModel.DataAnnotations;
using Kevin.ApiService.Services;

namespace Kevin.ApiService.Contracts.V1;

public record ShortenRequest([Required, AllowedTargetUrl] string Url, [EnumDataType(typeof(CodeFormat))] CodeFormat Format = CodeFormat.Numeric);
