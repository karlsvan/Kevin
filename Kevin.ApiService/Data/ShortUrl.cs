using System.ComponentModel.DataAnnotations;

namespace Kevin.ApiService.Data;

public class ShortUrl
{
    [Key]
    public required string Code { get; set; }
    public required string LongUrl { get; set; }
    public long Visits { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
