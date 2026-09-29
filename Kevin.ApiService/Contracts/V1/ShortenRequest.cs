using System.ComponentModel.DataAnnotations;

namespace Kevin.ApiService.Contracts.V1;

public record ShortenRequest([Required, Url] string Url);
