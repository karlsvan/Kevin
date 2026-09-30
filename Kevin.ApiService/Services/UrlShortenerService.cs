using Kevin.ApiService.Data;

namespace Kevin.ApiService.Services;

public class UrlShortenerService(
    IShortUrlRepository repository,
    ICodeGenerator codeGenerator,
    ICodeEncoder codeEncoder,
    TimeProvider timeProvider,
    ILogger<UrlShortenerService> logger) : IUrlShortenerService
{
    public async Task<string> ShortenAsync(string longUrl, CodeFormat format = CodeFormat.Numeric, CancellationToken cancellationToken = default)
    {
        var id = await repository.NextIdAsync(cancellationToken);
        var code = format switch
        {
            CodeFormat.Numeric => await GenerateUniqueNumericCodeAsync(cancellationToken),
            CodeFormat.Alphanum => codeEncoder.Encode(id),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        await repository.AddAsync(new ShortUrl
        {
            Id = id,
            Code = code,
            LongUrl = longUrl,
            CreatedAt = timeProvider.GetUtcNow()
        }, cancellationToken);

        logger.LogInformation("Shortened {LongUrl} to {Code}", longUrl, code);
        return code;
    }

    public async Task<string?> ResolveAsync(string code, CancellationToken cancellationToken = default)
    {
        var shortUrl = await repository.GetAsync(code, cancellationToken);
        if (shortUrl is null)
        {
            logger.LogWarning("Code {Code} not found", code);
            return null;
        }

        await repository.IncrementVisitsAsync(code, cancellationToken);
        logger.LogInformation("Resolved {Code} to {LongUrl}", code, shortUrl.LongUrl);
        return shortUrl.LongUrl;
    }

    public async Task<long?> GetVisitsAsync(string code, CancellationToken cancellationToken = default)
    {
        var shortUrl = await repository.GetAsync(code, cancellationToken);
        return shortUrl?.Visits;
    }

    public async Task<bool> DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        var deleted = await repository.DeleteAsync(code, cancellationToken);
        if (deleted)
        {
            logger.LogInformation("Deleted {Code}", code);
        }
        return deleted;
    }

    private async Task<string> GenerateUniqueNumericCodeAsync(CancellationToken cancellationToken)
    {
        string code;
        do
        {
            code = codeGenerator.Generate();
        } while (await repository.ExistsAsync(code, cancellationToken));

        return code;
    }
}
