namespace Kevin.ApiService.Data;

public interface IShortUrlRepository
{
    Task<long> NextIdAsync(CancellationToken cancellationToken = default);
    Task<ShortUrl?> GetAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(ShortUrl shortUrl, CancellationToken cancellationToken = default);
    Task IncrementVisitsAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string code, CancellationToken cancellationToken = default);
}
