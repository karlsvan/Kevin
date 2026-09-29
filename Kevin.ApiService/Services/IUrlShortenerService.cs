namespace Kevin.ApiService.Services;

public interface IUrlShortenerService
{
    Task<string> ShortenAsync(string longUrl, CancellationToken cancellationToken = default);
    Task<string?> ResolveAsync(string code, CancellationToken cancellationToken = default);
    Task<long?> GetVisitsAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string code, CancellationToken cancellationToken = default);
}
