using Microsoft.EntityFrameworkCore;

namespace Kevin.ApiService.Data;

public class UrlDbContext(DbContextOptions<UrlDbContext> options) : DbContext(options)
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();
}
