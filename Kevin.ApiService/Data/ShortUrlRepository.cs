using Microsoft.EntityFrameworkCore;

namespace Kevin.ApiService.Data;

public class ShortUrlRepository(UrlDbContext db) : IShortUrlRepository
{
    public Task<long> NextIdAsync(CancellationToken cancellationToken = default) =>
        db.Database
            .SqlQueryRaw<long>($"SELECT nextval('{UrlDbContext.ShortUrlIdSequence}') AS \"Value\"")
            .SingleAsync(cancellationToken);

    public Task<ShortUrl?> GetAsync(string code, CancellationToken cancellationToken = default) =>
        db.ShortUrls.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default) =>
        db.ShortUrls.AnyAsync(x => x.Code == code, cancellationToken);

    public async Task AddAsync(ShortUrl shortUrl, CancellationToken cancellationToken = default)
    {
        db.ShortUrls.Add(shortUrl);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task IncrementVisitsAsync(string code, CancellationToken cancellationToken = default) =>
        db.ShortUrls
            .Where(x => x.Code == code)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Visits, x => x.Visits + 1), cancellationToken);

    public async Task<bool> DeleteAsync(string code, CancellationToken cancellationToken = default) =>
        await db.ShortUrls.Where(x => x.Code == code).ExecuteDeleteAsync(cancellationToken) > 0;
}
