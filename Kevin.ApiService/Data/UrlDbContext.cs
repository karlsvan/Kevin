using Microsoft.EntityFrameworkCore;

namespace Kevin.ApiService.Data;

public class UrlDbContext(DbContextOptions<UrlDbContext> options) : DbContext(options)
{
    public const string ShortUrlIdSequence = "short_url_ids";

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>(ShortUrlIdSequence);
    }
}
