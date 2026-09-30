using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kevin.ApiService.Data;

[Index(nameof(Code), IsUnique = true)]
public class ShortUrl
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
    public required string Code { get; set; }
    public required string LongUrl { get; set; }
    public long Visits { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
